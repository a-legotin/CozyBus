using System;
using System.Collections.Generic;

namespace CozyBus.Core
{
    public interface IMessageBusSubscriptionsManager
    {
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