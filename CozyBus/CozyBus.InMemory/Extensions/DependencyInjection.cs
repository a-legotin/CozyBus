using System;
using CozyBus.Core;
using Microsoft.Extensions.DependencyInjection;

namespace CozyBus.InMemory.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection UseInMemoryMessageBus(this IServiceCollection services)
        {
            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
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
            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
            services.AddSingleton<IMessageBus, InMemoryBus>();
            return services;
        }
    }
}