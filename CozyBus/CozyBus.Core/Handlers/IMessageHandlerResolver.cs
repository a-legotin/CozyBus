using System;

namespace CozyBus.Core.Handlers
{
    public interface IMessageHandlerResolver
    {
        object Resolve(Type handlerType);
    }
}