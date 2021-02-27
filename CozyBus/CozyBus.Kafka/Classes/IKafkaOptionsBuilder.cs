using CozyBus.Core.Handlers;

namespace CozyBus.Kafka.Classes
{
    public interface IKafkaOptionsBuilder
    {
        void UseResolver<T>() where T : IMessageHandlerResolver;

        IKafkaOptionsBuilder WithTopicName(string topicName);
        IKafkaOptionsBuilder WithBootstrapServers(string bootstrapServers);
    }
}