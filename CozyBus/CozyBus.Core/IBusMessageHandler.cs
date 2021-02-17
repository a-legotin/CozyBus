using System.Threading.Tasks;

namespace CozyBus.Core
{
    public interface IBusMessageHandler<in T> where T : IBusMessage
    {
        Task Handle(T request);
    }
}