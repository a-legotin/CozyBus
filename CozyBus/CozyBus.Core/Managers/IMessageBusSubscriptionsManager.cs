using System;
using System.Collections.Generic;
using CozyBus.Core.Handlers;
using CozyBus.Core.Messages;
using CozyBus.Core.Subscription;

namespace CozyBus.Core.Managers
{
    public interface IMessageBusSubscriptionsManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnEventRemoved;

        void AddSubscription<T, TH>()
            where T : IBusMessage
            where TH : IBusMessageHandler<T>;

        void RemoveSubscription<T, TH>()
            where TH : IBusMessageHandler<T>
            where T : IBusMessage;

        void Clear();
        string GetMessageKey<T>();
        IEnumerable<SubscriptionInfo> GetHandlersForMessage<T>() where T : IBusMessage;
        bool HasSubscriptionsForMessage<T>() where T : IBusMessage;
        bool HasSubscriptionsForMessage(string messageTypeName);
        Type GetMessageTypeByName(string messageTypeName);
        IEnumerable<SubscriptionInfo> GetHandlersForMessage(string messageTypeName);
    }
}