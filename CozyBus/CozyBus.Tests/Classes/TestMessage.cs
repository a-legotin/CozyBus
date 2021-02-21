using System;
using CozyBus.Core;

namespace CozyBus.Tests.Classes
{
    class TestMessage : IBusMessage
    {
        public TestMessage(Guid id, DateTime creationDate)
        {
            Id = id;
            CreationDate = creationDate;
        }

        public Guid Id { get; }
        public DateTime CreationDate { get; }
    }
}