﻿using System;
using CozyBus.Core;

namespace CozyBus.InMemory.Extensions
{
    internal class DefaultResolver : IMessageHandlerResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultResolver(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public Type Resolve(Type handlerType) => _serviceProvider.GetService(handlerType) as Type;
    }
}