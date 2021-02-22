using System;
using CozyBus.Core.Handlers;

namespace CozyBus.RabbitMQ.Classes
{
    internal class RabbitMqOptionsBuilder : IRabbitMqOptionsBuilder,
        IRetryPolicy,
        IQueueOptions,
        IBrokerOptions,
        IConnectionOptions
    {
        private Type _resolver = typeof(DefaultResolver);
        public string BrokerName { get; set; }
        public string MessageBusConnection { get; private set; }
        public int MessageBusPort { get; private set; }
        public string MessageBusUserName { get; private set; }
        public string MessageBusPassword { get; private set; }

        public string QueueName { get; set; }

        public void UseResolver<T>() where T : IMessageHandlerResolver
        {
            _resolver = typeof(T);
        }

        public IRabbitMqOptionsBuilder WithConnection(string connection)
        {
            MessageBusConnection = connection;
            return this;
        }

        public IRabbitMqOptionsBuilder WithBrokerName(string brokerName)
        {
            BrokerName = brokerName;
            return this;
        }

        public IRabbitMqOptionsBuilder WithQueueName(string queueName)
        {
            QueueName = queueName;
            return this;
        }

        public IRabbitMqOptionsBuilder WithPort(int port)
        {
            MessageBusPort = port;
            return this;
        }

        public IRabbitMqOptionsBuilder WithUsername(string username)
        {
            MessageBusUserName = username;
            return this;
        }

        public IRabbitMqOptionsBuilder WithPassword(string password)
        {
            MessageBusPassword = password;
            return this;
        }

        public IRabbitMqOptionsBuilder WithRetryCount(short retryCount)
        {
            RetryCount = retryCount;
            return this;
        }

        public short RetryCount { get; private set; }

        internal Type GetResolver() => _resolver;
    }
}