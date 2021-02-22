using CozyBus.Core.Handlers;

namespace CozyBus.InMemory.Classes
{
    public interface IInMemoryBusOptionsBuilder
    {
        void UseResolver<T>() where T : IMessageHandlerResolver;
    }
}