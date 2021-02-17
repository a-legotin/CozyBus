using System;
using System.Collections.Generic;

namespace CozyBus.Core
{
    public interface IEventBusSubscriptionsManager
    {
        void AddSubscription<T, TH>()
            where T : IBusMessage
            where TH : IBusMessageHandler<T>;

        void RemoveSubscription<T, TH>()
            where TH : IBusMessageHandler<T>
            where T : IBusMessage;

        void Clear();
        string GetEventKey<T>();
        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IBusMessage;
        bool HasSubscriptionsForEvent<T>() where T : IBusMessage;
        bool HasSubscriptionsForEvent(string eventName);
        Type GetEventTypeByName(string eventName);
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
    }
}