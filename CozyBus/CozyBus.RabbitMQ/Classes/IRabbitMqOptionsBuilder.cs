using CozyBus.Core.Handlers;

namespace CozyBus.RabbitMQ.Classes
{
    public interface IRabbitMqOptionsBuilder
    {
        IRabbitMqOptionsBuilder UseCustomResolver();
        IRabbitMqOptionsBuilder WithConnection(string connection);
        IRabbitMqOptionsBuilder WithBrokerName(string brokerName);
        IRabbitMqOptionsBuilder WithQueueName(string queueName);
        IRabbitMqOptionsBuilder WithPort(int port);
        IRabbitMqOptionsBuilder WithUsername(string username);
        IRabbitMqOptionsBuilder WithPassword(string password);
        IRabbitMqOptionsBuilder WithRetryCount(short retryCount);
    }
}