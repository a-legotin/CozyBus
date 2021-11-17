using System;
using CozyBus.Core.Bus;
using CozyBus.Core.Handlers;
using CozyBus.Core.Managers;
using CozyBus.RabbitMQ.Classes;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace CozyBus.RabbitMQ.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection UseRabbitMqMessageBus(this IServiceCollection services,
            Action<IRabbitMqOptionsBuilder> optionsAction)
        {
            var options = new RabbitMqOptionsBuilder();
            optionsAction?.Invoke(options);

            var factory = new ConnectionFactory
            {
                HostName = options.MessageBusConnection,
                Port = options.MessageBusPort,
                DispatchConsumersAsync = true,
                UserName = options.MessageBusUserName,
                Password = options.MessageBusPassword
            };

            services.AddSingleton<IConnectionFactory>(factory);
            services.AddSingleton<IRetryPolicy>(options);
            services.AddSingleton<IBrokerOptions>(options);
            services.AddSingleton<IConnectionOptions>(options);
            services.AddSingleton<IQueueOptions>(options);
            services.AddSingleton<IRabbitMQPersistentConnection, DefaultRabbitMQPersistentConnection>();
            services.AddSingleton<IMessageBusSubscriptionsManager, InMemoryMessageBusSubscriptionsManager>();
            services.AddSingleton<IMessageBus, MessageBusRabbitMQ>();
            if (!options.IsCustomResolver)
            {
                services.AddSingleton<IMessageHandlerResolver, DefaultResolver>();
            }
            return services;
        }
    }
}