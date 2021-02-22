using CozyBus.Core.Handlers;

namespace CozyBus.RabbitMQ.Extensions
{
    public interface IInMemoryBusOptionsBuilder
    {
        void UseResolver<T>() where T : IMessageHandlerResolver;
    }
}