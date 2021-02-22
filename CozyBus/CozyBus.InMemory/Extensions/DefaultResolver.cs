using System;
using CozyBus.Core.Handlers;

namespace CozyBus.InMemory.Extensions
{
    internal class DefaultResolver : IMessageHandlerResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultResolver(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public object Resolve(Type handlerType) => _serviceProvider.GetService(handlerType);
    }
}