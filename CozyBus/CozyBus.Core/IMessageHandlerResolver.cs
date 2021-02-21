using System;

namespace CozyBus.Core
{
    public interface IMessageHandlerResolver
    {
        Type Resolve(Type handlerType);
    }
}