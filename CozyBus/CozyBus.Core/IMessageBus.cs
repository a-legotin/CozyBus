using System;

namespace CozyBus.Core
{
    public interface IMessageBus : IDisposable
    {
        void Publish<T>(IBusMessage message)  where T : IBusMessage;

        void Subscribe<T, TH>()
            where T : IBusMessage
            where TH : IBusMessageHandler<T>;
    }
}