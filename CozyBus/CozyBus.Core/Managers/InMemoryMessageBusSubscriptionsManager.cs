using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CozyBus.Core.Handlers;
using CozyBus.Core.Messages;
using CozyBus.Core.Subscription;

namespace CozyBus.Core.Managers
{
    public class InMemoryMessageBusSubscriptionsManager : IMessageBusSubscriptionsManager
    {
        private readonly IDictionary<string, List<SubscriptionInfo>> _handlers;
        private readonly List<Type> _messageTypes;

        public InMemoryMessageBusSubscriptionsManager()
        {
            _handlers = new ConcurrentDictionary<string, List<SubscriptionInfo>>();
            _messageTypes = new List<Type>();
        }

        public bool IsEmpty => !_handlers.Keys.Any();
        public void Clear() => _handlers.Clear();
        public event EventHandler<string> OnEventRemoved;

        public void AddSubscription<T, TH>()
            where T : IBusMessage
            where TH : IBusMessageHandler<T>
        {
            var messageKey = GetMessageKey<T>();

            DoAddSubscription(typeof(TH), messageKey);

            if (!_messageTypes.Contains(typeof(T)))
                _messageTypes.Add(typeof(T));
        }


        public void RemoveSubscription<T, TH>()
            where TH : IBusMessageHandler<T>
            where T : IBusMessage
        {
            var handlerToRemove = FindSubscriptionToRemove<T, TH>();
            var messageKey = GetMessageKey<T>();
            DoRemoveHandler(messageKey, handlerToRemove);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForMessage<T>() where T : IBusMessage
        {
            var key = GetMessageKey<T>();
            return GetHandlersForMessage(key);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForMessage(string messageTypeName)
            => _handlers.TryGetValue(messageTypeName, out var handlers)
                ? handlers.ToArray()
                : ArraySegment<SubscriptionInfo>.Empty;

        public bool HasSubscriptionsForMessage<T>() where T : IBusMessage
        {
            var key = GetMessageKey<T>();
            return HasSubscriptionsForMessage(key);
        }

        public bool HasSubscriptionsForMessage(string messageTypeName) => _handlers.ContainsKey(messageTypeName);

        public Type GetMessageTypeByName(string messageTypeName) =>
            _messageTypes.SingleOrDefault(t => t.Name == messageTypeName);

        public string GetMessageKey<T>() => typeof(T).Name;

        private void DoAddSubscription(Type handlerType, string messageTypeName)
        {
            if (!HasSubscriptionsForMessage(messageTypeName))
                _handlers.Add(messageTypeName, new List<SubscriptionInfo>());

            if (_handlers[messageTypeName].Any(s => s.HandlerType == handlerType))
                throw new ArgumentException(
                    $"Handler type {handlerType.Name} already registered for '{messageTypeName}'", nameof(handlerType));

            _handlers[messageTypeName]
                .Add(new SubscriptionInfo(handlerType));
        }


        private void DoRemoveHandler(string messageTypeName, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove == null)
                return;
            _handlers[messageTypeName].Remove(subsToRemove);
            if (_handlers[messageTypeName].Any())
                return;
            _handlers.Remove(messageTypeName);
            var messageType = _messageTypes.SingleOrDefault(e => e.Name == messageTypeName);
            if (messageType == null)
                return;
            _messageTypes.Remove(messageType);
            RaiseOnEventRemoved(messageTypeName);
        }

        private void RaiseOnEventRemoved(string messageTypeName)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this, messageTypeName);
        }

        private SubscriptionInfo FindSubscriptionToRemove<T, TH>()
            where T : IBusMessage
            where TH : IBusMessageHandler<T>
        {
            var messageKey = GetMessageKey<T>();
            return DoFindSubscriptionToRemove(messageKey, typeof(TH));
        }

        private SubscriptionInfo DoFindSubscriptionToRemove(string messageTypeName, Type handlerType) =>
            !HasSubscriptionsForMessage(messageTypeName)
                ? null
                : _handlers[messageTypeName].SingleOrDefault(s => s.HandlerType == handlerType);
    }
}