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
        private bool _useCustomResolver;
        public string BrokerName { get; set; }
        public string MessageBusConnection { get; private set; }
        public int MessageBusPort { get; private set; }
        public string MessageBusUserName { get; private set; }
        public string MessageBusPassword { get; private set; }

        public string QueueName { get; set; }

        public IRabbitMqOptionsBuilder UseCustomResolver()
        {
            _useCustomResolver = true;
            return this;
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

        internal bool IsCustomResolver => _useCustomResolver;
    }
}