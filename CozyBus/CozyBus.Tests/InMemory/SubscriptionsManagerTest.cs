using System.Collections.Generic;
using System.Linq;
using CozyBus.Core;
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
            var subscriptionsManager = new InMemoryEventBusSubscriptionsManager();

            var inMemoryMessageBus = new InMemoryBus(handlerResolver.Object, logger.Object, subscriptionsManager);
            inMemoryMessageBus.Subscribe<TestMessage, TestMessageHandler>();

            var handlers = subscriptionsManager.GetHandlersForEvent<TestMessage>();
            
            Assert.IsNotNull(handlers);
            Assert.AreEqual(1, handlers.Count());
        }
        
        [Test]
        public void Should_Add_Then_Remove_Handler()
        {
            var handlerResolver = new Mock<IMessageHandlerResolver>();
            var logger = new Mock<ILogger<IMessageBus>>();
            var subscriptionsManager = new InMemoryEventBusSubscriptionsManager();

            var inMemoryMessageBus = new InMemoryBus(handlerResolver.Object, logger.Object, subscriptionsManager);
            inMemoryMessageBus.Subscribe<TestMessage, TestMessageHandler>();

            var handlers = subscriptionsManager.GetHandlersForEvent<TestMessage>().ToArray();

            Assert.IsNotNull(handlers);
            Assert.AreEqual(1, handlers.Count());
            
            inMemoryMessageBus.Unsubscribe<TestMessage, TestMessageHandler>();
            Assert.Throws<KeyNotFoundException>(() =>subscriptionsManager.GetHandlersForEvent<TestMessage>());
        }
    }
}