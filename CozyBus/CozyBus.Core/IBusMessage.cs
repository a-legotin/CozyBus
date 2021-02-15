using System;

namespace CozyBus.Core
{
    public interface IBusMessage
    {
        public Guid Id { get; }
        public DateTime CreationDate { get; }
    }
}