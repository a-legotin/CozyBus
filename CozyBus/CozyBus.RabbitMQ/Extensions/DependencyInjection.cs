using System;
using CozyBus.Core.Bus;
using CozyBus.Core.Handlers;
using CozyBus.Core.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace CozyBus.RabbitMQ.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection UseInMemoryMessageBus(this IServiceCollection services)
        {
            services.AddSingleton<IMessageBusSubscriptionsManager, InMemoryMessageBusSubscriptionsManager>();
            services.AddSingleton<IMessageHandlerResolver, DefaultResolver>();
            services.AddSingleton<IMessageBus, MessageBusRabbitMQ>();
            return services;
        }

        public static IServiceCollection UseInMemoryMessageBus(this IServiceCollection services,
            Action<IInMemoryBusOptionsBuilder> optionsAction)
        {
            var options = new InMemoryBusOptionsBuilder();
            optionsAction?.Invoke(options);
            services.AddSingleton(options.GetResolver());
            services.AddSingleton<IMessageBusSubscriptionsManager, InMemoryMessageBusSubscriptionsManager>();
            services.AddSingleton<IMessageBus, MessageBusRabbitMQ>();
            return services;
        }
    }
}