using System;
using System.Net.Sockets;
using System.Text.Json;
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
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IMessageHandlerResolver _handlerResolver;
        private readonly ILogger<IMessageBus> _logger;

        private readonly IProducer<Ignore, string> _producer;
        private readonly int _retryCount;
        private readonly IMessageBusSubscriptionsManager _subscriptionsManager;

        private readonly string _topic;

        public KafkaMessageBus(IMessageHandlerResolver handlerResolver,
            ILogger<IMessageBus> logger,
            IMessageBusSubscriptionsManager subscriptionsManager,
            IProducer<Ignore, string> producer,
            IConsumer<Ignore, string> consumer,
            IKafkaTopicOptions kafkaTopicOptions)
        {
            _handlerResolver = handlerResolver;
            _logger = logger;
            _subscriptionsManager = subscriptionsManager;
            _producer = producer;
            _consumer = consumer;
            _topic = kafkaTopicOptions.TopicName;
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
                _producer.Produce(_topic, new Message<Ignore, string>
                {
                    Value = messageSerialized
                });
            });
        }

        public void Dispose()
        {
            _producer?.Dispose();
            _consumer?.Dispose();
        }

        private async Task ProcessMessage(string messageName, IBusMessage message)
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