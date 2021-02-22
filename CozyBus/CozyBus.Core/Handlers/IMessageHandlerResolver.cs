using System;

namespace CozyBus.Core.Handlers
{
    public interface IMessageHandlerResolver
    {
        Type Resolve(Type handlerType);
    }
}