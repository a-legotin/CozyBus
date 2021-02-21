using CozyBus.Core;

namespace CozyBus.InMemory.Extensions
{
    public interface IInMemoryBusOptionsBuilder
    {
        void UseResolver<T>() where T : IMessageHandlerResolver;
    }
}