using System;
using CozyBus.Core;

namespace CozyBus.InMemory.Extensions
{
    internal class InMemoryBusOptionsBuilder : IInMemoryBusOptionsBuilder
    {
        private Type _resolver;

        public void UseResolver<T>() where T : IMessageHandlerResolver
        {
            _resolver = typeof(T);
        }

        internal Type GetResolver() => _resolver;
    }
}