using System.Threading.Tasks;
using CozyBus.Core;

namespace CozyBus.Tests.Classes
{
    class TestMessageHandler : IBusMessageHandler<TestMessage>
    {
        public async Task Handle(TestMessage request) => await Task.CompletedTask;
    }
}