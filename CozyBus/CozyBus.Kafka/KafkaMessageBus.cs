using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using CozyBus.Core.Bus;
using CozyBus.Core.Handlers;
using CozyBus.Core.Managers;
using CozyBus.Core.Messages;
using CozyBus.Kafka.Classes;
using Microsoft.Extensions.Logging;
using Polly;

namespace CozyBus.Kafka
{
    internal class KafkaMessageBus : IMessageBus
    {
        private readonly CancellationToken _cancellationToken;
        private readonly IConsumer<string, string> _consumer;
        private readonly IMessageHandlerResolver _handlerResolver;
        private readonly ILogger<IMessageBus> _logger;

        private readonly IProducer<string, string> _producer;
        private readonly int _retryCount;
        private readonly IMessageBusSubscriptionsManager _subscriptionsManager;

        private readonly string _topic;

        public KafkaMessageBus(IMessageHandlerResolver handlerResolver,
            ILogger<IMessageBus> logger,
            IMessageBusSubscriptionsManager subscriptionsManager,
            IProducer<string, string> producer,
            IConsumer<string, string> consumer,
            IKafkaTopicOptions kafkaTopicOptions)
        {
            _handlerResolver = handlerResolver;
            _logger = logger;
            _subscriptionsManager = subscriptionsManager;
            _producer = producer;
            _consumer = consumer;
            _topic = kafkaTopicOptions.TopicName;
            _retryCount = 5;
            _cancellationToken = new CancellationToken();
            StartConsumeBasic();
        }

        public void Subscribe<T, TH>() where T : IBusMessage where TH : IBusMessageHandler<T>
        {
            var messageKey = _subscriptionsManager.GetMessageKey<T>();
            _logger.LogInformation($"Subscribing to message {messageKey} with {typeof(TH)}",
                messageKey,
                nameof(TH));
            _subscriptionsManager.AddSubscription<T, TH>();
        }

        public void Publish<T>(IBusMessage message) where T : IBusMessage
        {
            var policy = Policy.Handle<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) =>
                    {
                        _logger.LogWarning(ex,
                            $"Could not publish event: {message} after {time.TotalSeconds:n1}s ({ex.Message})");
                    });

            var messageTypeName = message.GetType().Name;

            _logger.LogTrace($"Creating RabbitMQ channel to publish message: {messageTypeName}");


            var messageSerialized = JsonSerializer.Serialize(message);


            policy.Execute(() =>
            {
                _logger.LogTrace($"Publishing message to Kafka: {messageTypeName}");
                Task.Run(async () =>
                {
                    try
                    {
                        var dr = await _producer.ProduceAsync(_topic, new Message<string, string>
                        {
                            Key = messageTypeName,
                            Value = messageSerialized
                        }, _cancellationToken);
                        _logger.LogTrace($"Publishing message to Kafka with result: {dr}");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Failed to publish message to Kafka {e.Message}");
                    }
                }, _cancellationToken);
            });
        }

        public void Dispose()
        {
            _producer?.Dispose();
            _consumer?.Dispose();
        }

        private void StartConsumeBasic()
        {
            Task.Run(async () =>
            {
                try
                {
                    _consumer.Subscribe(_topic);

                    while (true)
                    {
                        try
                        {
                            var cr = _consumer.Consume(_cancellationToken);
                            _logger.LogTrace($"Consumed message '{cr.Message.Value}' at: '{cr.TopicPartitionOffset}'.");
                            await ProcessMessage(cr.Message.Key, cr.Message.Value);
                        }
                        catch (ConsumeException e)
                        {
                            _logger.LogError($"Error occurred in Kafka consumer: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
            });
        }

        private async Task ProcessMessage(string messageName, string messageSerialized)
        {
            _logger.LogTrace($"Processing message: {messageName}", messageName);

            if (_subscriptionsManager.HasSubscriptionsForMessage(messageName))
            {
                var subscriptions = _subscriptionsManager.GetHandlersForMessage(messageName);
                foreach (var subscription in subscriptions)
                {
                    var handler = _handlerResolver.Resolve(subscription.HandlerType);
                    if (handler == null)
                        continue;
                    var messageType = _subscriptionsManager.GetMessageTypeByName(messageName);
                    var concreteType = typeof(IBusMessageHandler<>).MakeGenericType(messageType);
                    var message = JsonSerializer.Deserialize(messageSerialized, messageType);

                    await Task.Yield();
                    await (Task) concreteType.GetMethod("Handle").Invoke(handler, new[] {message});
                }
            }
            else
                _logger.LogWarning($"No subscription for message: {messageName}", messageName);
        }

        public void Unsubscribe<T, TH>()
            where T : IBusMessage
            where TH : IBusMessageHandler<T>
        {
            var messageName = _subscriptionsManager.GetMessageKey<T>();
            _logger.LogInformation($"Unsubscribing from message {messageName}", messageName);
            _subscriptionsManager.RemoveSubscription<T, TH>();
        }
    }
}