using System;

namespace CozyBus.Core.Messages
{
    public interface IBusMessage
    {
        public Guid Id { get; }
        public DateTime CreationDate { get; }
    }
}