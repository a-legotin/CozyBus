using System;
using System.Net;
using Confluent.Kafka;
using CozyBus.Core.Bus;
using CozyBus.Core.Managers;
using CozyBus.Kafka.Classes;
using Microsoft.Extensions.DependencyInjection;

namespace CozyBus.Kafka.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection UseKafkaMessageBus(this IServiceCollection services,
            Action<IKafkaOptionsBuilder> optionsAction)
        {
            var options = new KafkaOptionsBuilder();
            optionsAction?.Invoke(options);

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = options.BootstrapServers,
                ClientId = Dns.GetHostName()
            };
            var producer = new ProducerBuilder<string, string>(producerConfig).Build();
            services.AddSingleton(producer);


            var config = new ConsumerConfig
            {
                BootstrapServers = options.BootstrapServers,
                GroupId = "cozy-consumer"
            };

            var consumer = new ConsumerBuilder<string, string>(config).Build();
            services.AddSingleton(consumer);

            services.AddSingleton<IKafkaTopicOptions>(options);
            services.AddSingleton(options.GetResolver());
            services.AddSingleton<IMessageBusSubscriptionsManager, InMemoryMessageBusSubscriptionsManager>();
            services.AddSingleton<IMessageBus, KafkaMessageBus>();
            return services;
        }
    }
}