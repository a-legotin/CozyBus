using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CozyBus.Core
{
    public class InMemoryEventBusSubscriptionsManager : IEventBusSubscriptionsManager
    {
        private readonly List<Type> _eventTypes;
        private readonly IDictionary<string, List<SubscriptionInfo>> _handlers;

        public InMemoryEventBusSubscriptionsManager()
        {
            _handlers = new ConcurrentDictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
        }

        public bool IsEmpty => !_handlers.Keys.Any();
        public void Clear() => _handlers.Clear();

        public void AddSubscription<T, TH>()
            where T : IBusMessage
            where TH : IBusMessageHandler<T>
        {
            var eventName = GetEventKey<T>();

            DoAddSubscription(typeof(TH), eventName, false);

            if (!_eventTypes.Contains(typeof(T)))
                _eventTypes.Add(typeof(T));
        }


        public void RemoveSubscription<T, TH>()
            where TH : IBusMessageHandler<T>
            where T : IBusMessage
        {
            var handlerToRemove = FindSubscriptionToRemove<T, TH>();
            var eventName = GetEventKey<T>();
            DoRemoveHandler(eventName, handlerToRemove);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IBusMessage
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => _handlers[eventName];

        public bool HasSubscriptionsForEvent<T>() where T : IBusMessage
        {
            var key = GetEventKey<T>();
            return HasSubscriptionsForEvent(key);
        }

        public bool HasSubscriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName);

        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName);

        public string GetEventKey<T>() => typeof(T).Name;

        public event EventHandler<string> OnEventRemoved;

        private void DoAddSubscription(Type handlerType, string eventName, bool isDynamic)
        {
            if (!HasSubscriptionsForEvent(eventName))
                _handlers.Add(eventName, new List<SubscriptionInfo>());

            if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));

            _handlers[eventName]
                .Add(isDynamic ? SubscriptionInfo.Dynamic(handlerType) : SubscriptionInfo.Typed(handlerType));
        }


        private void DoRemoveHandler(string eventName, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove == null)
                return;
            _handlers[eventName].Remove(subsToRemove);
            if (_handlers[eventName].Any())
                return;
            _handlers.Remove(eventName);
            var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
            if (eventType != null) _eventTypes.Remove(eventType);
            RaiseOnEventRemoved(eventName);
        }

        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this, eventName);
        }


        private SubscriptionInfo FindSubscriptionToRemove<T, TH>()
            where T : IBusMessage
            where TH : IBusMessageHandler<T>
        {
            var eventName = GetEventKey<T>();
            return DoFindSubscriptionToRemove(eventName, typeof(TH));
        }

        private SubscriptionInfo DoFindSubscriptionToRemove(string eventName, Type handlerType) =>
            !HasSubscriptionsForEvent(eventName)
                ? null
                : _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);
    }
}