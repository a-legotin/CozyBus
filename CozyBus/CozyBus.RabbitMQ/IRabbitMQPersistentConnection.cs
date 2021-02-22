using System;
using RabbitMQ.Client;

namespace CozyBus.RabbitMQ
{
    internal interface IRabbitMQPersistentConnection : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}