using System;
using CozyBus.Core.Handlers;

namespace CozyBus.RabbitMQ.Extensions
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