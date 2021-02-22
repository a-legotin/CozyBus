﻿using CozyBus.Core.Handlers;

namespace CozyBus.InMemory.Extensions
{
    public interface IInMemoryBusOptionsBuilder
    {
        void UseResolver<T>() where T : IMessageHandlerResolver;
    }
}