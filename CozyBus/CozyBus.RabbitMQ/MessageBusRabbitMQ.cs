using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CozyBus.Core.Bus;
using CozyBus.Core.Handlers;
using CozyBus.Core.Managers;
using CozyBus.Core.Messages;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace CozyBus.RabbitMQ
{
    internal class MessageBusRabbitMQ : IMessageBus
    {
        private const string BROKER_NAME = nameof(MessageBusRabbitMQ);
        private readonly string _brokerName;
        private readonly IMessageHandlerResolver _handlerResolver;
        private readonly ILogger<IMessageBus> _logger;
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly int _retryCount;
        private readonly IMessageBusSubscriptionsManager _subscriptionsManager;
        private IModel _consumerChannel;
        private string _queueName;

        public MessageBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection,
            ILogger<IMessageBus> logger,
            IMessageBusSubscriptionsManager subsManager,
            IMessageHandlerResolver handlerResolver,
            string queueName = null,
            string brokerName = BROKER_NAME,
            int retryCount = 5)
        {
            _persistentConnection =
                persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _handlerResolver = handlerResolver;
            _subscriptionsManager = subsManager ?? new InMemoryMessageBusSubscriptionsManager();
            _queueName = queueName;
            _brokerName = brokerName;
            _consumerChannel = CreateConsumerChannel();
            _retryCount = retryCount;
            _subscriptionsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        public void Dispose()
        {
            _subscriptionsManager.OnEventRemoved -= SubsManager_OnEventRemoved;
            _consumerChannel?.Dispose();
            _subscriptionsManager.Clear();
        }


        public void Subscribe<T, TH>() where T : IBusMessage where TH : IBusMessageHandler<T>
        {
            var messageTypeName = _subscriptionsManager.GetMessageKey<T>();
            DoInternalSubscription(messageTypeName);

            _logger.LogInformation($"Subscribing to event {messageTypeName} with {typeof(TH)}", messageTypeName,
                typeof(TH).FullName);

            _subscriptionsManager.AddSubscription<T, TH>();
            StartBasicConsume();
        }

        public void Publish<T>(IBusMessage message) where T : IBusMessage
        {
            if (!_persistentConnection.IsConnected) _persistentConnection.TryConnect();

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) =>
                    {
                        _logger.LogWarning(ex,
                            "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", message.Id,
                            $"{time.TotalSeconds:n1}", ex.Message);
                    });

            var messageTypeName = message.GetType().Name;

            _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({messageTypeName})", message.Id,
                messageTypeName);

            using (var channel = _persistentConnection.CreateModel())
            {
                _logger.LogTrace($"Declaring RabbitMQ exchange to publish event: {messageTypeName}", message.Id);

                channel.ExchangeDeclare(_brokerName, "direct");

                var messageSerialized = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageSerialized);

                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent

                    _logger.LogTrace($"Publishing event to RabbitMQ: {messageTypeName}", message.Id);

                    channel.BasicPublish(
                        _brokerName,
                        messageTypeName,
                        true,
                        properties,
                        body);
                });
            }
        }

        private void SubsManager_OnEventRemoved(object sender, string messageTypeName)
        {
            if (!_persistentConnection.IsConnected) _persistentConnection.TryConnect();

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(_queueName,
                    _brokerName,
                    messageTypeName);

                if (_subscriptionsManager.IsEmpty)
                {
                    _queueName = string.Empty;
                    _consumerChannel.Close();
                }
            }
        }


        private void DoInternalSubscription(string messageTypeName)
        {
            var containsKey = _subscriptionsManager.HasSubscriptionsForMessage(messageTypeName);
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected) _persistentConnection.TryConnect();

                using (var channel = _persistentConnection.CreateModel())
                {
                    channel.QueueBind(_queueName,
                        _brokerName,
                        messageTypeName);
                }
            }
        }

        public void Unsubscribe<T, TH>()
            where T : IBusMessage
            where TH : IBusMessageHandler<T>
        {
            var messageTypeName = _subscriptionsManager.GetMessageKey<T>();

            _logger.LogInformation($"Unsubscribing from event {messageTypeName}", messageTypeName);

            _subscriptionsManager.RemoveSubscription<T, TH>();
        }


        private void StartBasicConsume()
        {
            _logger.LogTrace("Starting RabbitMQ basic consume");

            if (_consumerChannel != null)
            {
                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.Received += Consumer_Received;

                _consumerChannel.BasicConsume(
                    _queueName,
                    false,
                    consumer);
            }
            else
            {
                _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var messageTypeName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            try
            {
                if (message.ToLowerInvariant().Contains("throw-fake-exception"))
                    throw new InvalidOperationException($"Fake exception requested: \"{message}\"");

                await ProcessEvent(messageTypeName, message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", message);
            }

            // Even on exception we take the message off the queue.
            // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
            // For more information see: https://www.rabbitmq.com/dlx.html
            _consumerChannel.BasicAck(eventArgs.DeliveryTag, false);
        }

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected) _persistentConnection.TryConnect();

            _logger.LogTrace("Creating RabbitMQ consumer channel");

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(_brokerName,
                "direct");

            channel.QueueDeclare(_queueName,
                true,
                false,
                false,
                null);

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");

                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel();
                StartBasicConsume();
            };

            return channel;
        }

        private async Task ProcessEvent(string messageTypeName, string message)
        {
            _logger.LogTrace("Processing RabbitMQ event: {messageTypeName}", messageTypeName);

            if (_subscriptionsManager.HasSubscriptionsForMessage(messageTypeName))
            {
                var subscriptions = _subscriptionsManager.GetHandlersForMessage(messageTypeName);
                foreach (var subscription in subscriptions)
                {
                    var handler = _handlerResolver.Resolve(subscription.HandlerType);
                    if (handler == null)
                        continue;
                    var messageType = _subscriptionsManager.GetMessageTypeByName(messageTypeName);
                    var concreteType = typeof(IBusMessageHandler<>).MakeGenericType(messageType);

                    await Task.Yield();
                    await (Task) concreteType.GetMethod("Handle").Invoke(handler, new[] {message});
                }
            }
            else
                _logger.LogWarning("No subscription for RabbitMQ event: {messageTypeName}", messageTypeName);
        }
    }
}