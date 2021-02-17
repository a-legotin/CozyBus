using System;

namespace CozyBus.Core
{
    public interface IHandlerResolver
    {
        Type Resolve(Type eventName);
    }
}