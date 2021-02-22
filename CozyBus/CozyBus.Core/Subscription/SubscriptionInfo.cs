using System;

namespace CozyBus.Core.Subscription
{
    public class SubscriptionInfo
    {
        public SubscriptionInfo(Type handlerType) => HandlerType = handlerType;

        public Type HandlerType { get; }
    }
}