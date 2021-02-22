using System;
using CozyBus.Core.Bus;
using CozyBus.Core.Handlers;
using CozyBus.Core.Managers;
using CozyBus.InMemory.Classes;
using Microsoft.Extensions.DependencyInjection;

namespace CozyBus.InMemory.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection UseInMemoryMessageBus(this IServiceCollection services)
        {
            services.AddSingleton<IMessageBusSubscriptionsManager, InMemoryMessageBusSubscriptionsManager>();
            services.AddSingleton<IMessageHandlerResolver, DefaultResolver>();
            services.AddSingleton<IMessageBus, InMemoryBus>();
            return services;
        }

        public static IServiceCollection UseInMemoryMessageBus(this IServiceCollection services,
            Action<IInMemoryBusOptionsBuilder> optionsAction)
        {
            var options = new InMemoryBusOptionsBuilder();
            optionsAction?.Invoke(options);
            services.AddSingleton(options.GetResolver());
            services.AddSingleton<IMessageBusSubscriptionsManager, InMemoryMessageBusSubscriptionsManager>();
            services.AddSingleton<IMessageBus, InMemoryBus>();
            return services;
        }
    }
}