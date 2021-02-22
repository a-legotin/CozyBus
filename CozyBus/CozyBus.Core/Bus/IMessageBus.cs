using System;
using CozyBus.Core.Handlers;
using CozyBus.Core.Messages;

namespace CozyBus.Core.Bus
{
    public interface IMessageBus : IDisposable
    {
        void Publish<T>(IBusMessage message)  where T : IBusMessage;

        void Subscribe<T, TH>()
            where T : IBusMessage
            where TH : IBusMessageHandler<T>;
    }
}