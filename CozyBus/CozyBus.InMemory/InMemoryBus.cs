using System.Threading.Tasks;
using CozyBus.Core.Bus;
using CozyBus.Core.Handlers;
using CozyBus.Core.Managers;
using CozyBus.Core.Messages;
using Microsoft.Extensions.Logging;

namespace CozyBus.InMemory
{
    internal class InMemoryBus : IMessageBus
    {
        private readonly IMessageHandlerResolver _handlerResolver;
        private readonly ILogger<IMessageBus> _logger;
        private readonly IMessageBusSubscriptionsManager _subscriptionsManager;

        public InMemoryBus(IMessageHandlerResolver handlerResolver, ILogger<IMessageBus> logger,
            IMessageBusSubscriptionsManager subscriptionsManager)
        {
            _handlerResolver = handlerResolver;
            _logger = logger;
            _subscriptionsManager = subscriptionsManager;
        }

        public void Publish<T>(IBusMessage message) where T : IBusMessage
        {
            PublishAsyncCore<T>(message);
        }

        public void Subscribe<T, TH>() where T : IBusMessage where TH : IBusMessageHandler<T>
        {
            var messageKey = _subscriptionsManager.GetMessageKey<T>();
            _logger.LogInformation($"Subscribing to message {messageKey} with {typeof(TH)}",
                messageKey,
                nameof(TH));
            _subscriptionsManager.AddSubscription<T, TH>();
        }

        public void Dispose()
        {
            _subscriptionsManager.Clear();
        }

        private void PublishAsyncCore<T>(IBusMessage message) where T : IBusMessage
        {
            var publishTask = Task.Run(async () =>
                await ProcessMessage(_subscriptionsManager.GetMessageKey<T>(), message));
            var tcs = new TaskCompletionSource();
            publishTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    tcs.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled)
                    tcs.TrySetCanceled();
            }, TaskScheduler.Default);
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