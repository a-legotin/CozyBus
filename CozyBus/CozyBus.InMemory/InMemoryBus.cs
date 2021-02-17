using System.Threading.Tasks;
using CozyBus.Core;
using Microsoft.Extensions.Logging;

namespace CozyBus.InMemory
{
    public class InMemoryBus : IMessageBus
    {
        private readonly IHandlerResolver _handlerResolver;
        private readonly ILogger<IMessageBus> _logger;
        private readonly IEventBusSubscriptionsManager _subscriptionsManager;

        public void Publish<T>(IBusMessage message) where T : IBusMessage
        {
            PublishAsyncCore<T>(message);
        }

        public void Subscribe<T, TH>() where T : IBusMessage where TH : IBusMessageHandler<T>
        {
            var eventName = _subscriptionsManager.GetEventKey<T>();
            _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}",
                eventName,
                nameof(TH));
            _subscriptionsManager.AddSubscription<T, TH>();
        }

        public void Dispose()
        {
            _subscriptionsManager.Clear();
        }

        private void PublishAsyncCore<T>(IBusMessage message) where T : IBusMessage
        {
            var publishTask = Task.Run(async () => await ProcessEvent(_subscriptionsManager.GetEventKey<T>(), message));

            var tcs = new TaskCompletionSource();
            publishTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    tcs.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled)
                    tcs.TrySetCanceled();
            }, TaskScheduler.Default);
        }

        private async Task ProcessEvent(string eventName, IBusMessage message)
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

            if (_subscriptionsManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptions = _subscriptionsManager.GetHandlersForEvent(eventName);
                foreach (var subscription in subscriptions)
                {
                    var handler = _handlerResolver.Resolve(subscription.HandlerType);
                    if (handler == null)
                        continue;
                    var eventType = _subscriptionsManager.GetEventTypeByName(eventName);
                    var concreteType = typeof(IBusMessageHandler<>).MakeGenericType(eventType);

                    await Task.Yield();
                    await (Task) concreteType.GetMethod("Handle").Invoke(handler, new[] {message});
                }
            }
            else
                _logger.LogWarning("No subscription for event: {EventName}", eventName);
        }

        public void Unsubscribe<T, TH>()
            where T : IBusMessage
            where TH : IBusMessageHandler<T>
        {
            var eventName = _subscriptionsManager.GetEventKey<T>();
            _logger.LogInformation("Unsubscribing from event {EventName}", eventName);
            _subscriptionsManager.RemoveSubscription<T, TH>();
        }
    }
}