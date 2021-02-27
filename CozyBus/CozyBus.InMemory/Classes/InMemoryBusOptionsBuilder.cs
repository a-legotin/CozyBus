using System;
using CozyBus.Core.Handlers;

namespace CozyBus.InMemory.Classes
{
    internal class InMemoryBusOptionsBuilder : IInMemoryBusOptionsBuilder
    {
        private Type _resolver = typeof(DefaultResolver);

        public void UseResolver<T>() where T : IMessageHandlerResolver
        {
            _resolver = typeof(T);
        }

        internal Type GetResolver() => _resolver;
    }
}