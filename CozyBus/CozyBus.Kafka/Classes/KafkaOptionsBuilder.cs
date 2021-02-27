using System;
using CozyBus.Core.Handlers;

namespace CozyBus.Kafka.Classes
{
    internal class KafkaOptionsBuilder : IKafkaOptionsBuilder, IKafkaTopicOptions
    {
        private Type _resolver = typeof(DefaultResolver);
        public string BootstrapServers { get; private set; }

        public IKafkaOptionsBuilder WithTopicName(string topicName)
        {
            TopicName = topicName;
            return this;
        }

        public IKafkaOptionsBuilder WithBootstrapServers(string bootstrapServers)
        {
            BootstrapServers = bootstrapServers;
            return this;
        }

        public void UseResolver<T>() where T : IMessageHandlerResolver
        {
            _resolver = typeof(T);
        }

        public string TopicName { get; private set; }

        internal Type GetResolver() => _resolver;
    }
}