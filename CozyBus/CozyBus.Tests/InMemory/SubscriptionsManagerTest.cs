using System.Linq;
using CozyBus.Core.Bus;
using CozyBus.Core.Handlers;
using CozyBus.Core.Managers;
using CozyBus.InMemory;
using CozyBus.Tests.Classes;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CozyBus.Tests.InMemory
{
    public class SubscriptionsManagerTest
    {
        [Test]
        public void Should_Add_Handler()
        {
            var handlerResolver = new Mock<IMessageHandlerResolver>();
            var logger = new Mock<ILogger<IMessageBus>>();
            var subscriptionsManager = new InMemoryMessageBusSubscriptionsManager();

            var inMemoryMessageBus = new InMemoryBus(handlerResolver.Object, logger.Object, subscriptionsManager);
            inMemoryMessageBus.Subscribe<TestMessage, TestMessageHandler>();

            var handlers = subscriptionsManager.GetHandlersForMessage<TestMessage>();

            Assert.IsNotNull(handlers);
            Assert.AreEqual(1, handlers.Count());
        }

        [Test]
        public void Should_Add_Then_Remove_Handler()
        {
            var handlerResolver = new Mock<IMessageHandlerResolver>();
            var logger = new Mock<ILogger<IMessageBus>>();
            var subscriptionsManager = new InMemoryMessageBusSubscriptionsManager();

            var inMemoryMessageBus = new InMemoryBus(handlerResolver.Object, logger.Object, subscriptionsManager);
            inMemoryMessageBus.Subscribe<TestMessage, TestMessageHandler>();

            var handlers = subscriptionsManager.GetHandlersForMessage<TestMessage>().ToArray();

            Assert.IsNotNull(handlers);
            Assert.AreEqual(1, handlers.Length);

            inMemoryMessageBus.Unsubscribe<TestMessage, TestMessageHandler>();
            handlers = subscriptionsManager.GetHandlersForMessage<TestMessage>().ToArray();

            Assert.IsNotNull(handlers);
            Assert.AreEqual(0, handlers.Length);
        }
    }
}