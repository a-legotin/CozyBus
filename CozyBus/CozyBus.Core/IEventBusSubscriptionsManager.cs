﻿using System;
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
        IEnumerable<SubscriptionInfo> GetHandlersForMessage<T>() where T : IBusMessage;
        bool HasSubscriptionsForMessage<T>() where T : IBusMessage;
        bool HasSubscriptionsForMessage(string eventName);
        Type GetMessageTypeByName(string eventName);
        IEnumerable<SubscriptionInfo> GetHandlersForMessage(string eventName);
    }
}