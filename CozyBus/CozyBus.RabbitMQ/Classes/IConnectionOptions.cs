namespace CozyBus.RabbitMQ.Classes
{
    internal interface IConnectionOptions
    {
        string MessageBusConnection { get;  }
        int MessageBusPort { get;  }
        string MessageBusUserName { get;  }
        string MessageBusPassword { get;  }
    }
}