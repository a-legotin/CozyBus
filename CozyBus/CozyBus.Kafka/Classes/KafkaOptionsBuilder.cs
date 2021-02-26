using System;
using CozyBus.Core.Handlers;

namespace CozyBus.Kafka.Classes
{
    internal class KafkaOptionsBuilder : IKafkaOptionsBuilder, IKafkaTopicOptions
    {
        private Type _resolver;

        public string TopicName { get; }
        public string BootstrapServers { get; private set; }

        public void UseResolver<T>() where T : IMessageHandlerResolver
        {
            _resolver = typeof(T);
        }

        internal Type GetResolver() => _resolver;
    }
}