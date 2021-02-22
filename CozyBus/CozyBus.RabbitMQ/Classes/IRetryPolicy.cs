namespace CozyBus.RabbitMQ.Classes
{
    internal interface IRetryPolicy
    {
        short RetryCount { get;  }
    }
}