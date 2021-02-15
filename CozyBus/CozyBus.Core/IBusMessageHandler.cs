using System.Threading.Tasks;

namespace CozyBus.Core
{
    public interface IBusMessageHandler<in TIntegrationEvent> : IBusMessageHandler
        where TIntegrationEvent : IBusMessage
    {
        Task Handle(TIntegrationEvent request);
    }
}