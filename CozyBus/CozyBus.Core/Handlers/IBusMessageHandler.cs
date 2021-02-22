using System.Threading.Tasks;
using CozyBus.Core.Messages;

namespace CozyBus.Core.Handlers
{
    public interface IBusMessageHandler<in T> where T : IBusMessage
    {
        Task Handle(T request);
    }
}