using System;
using System.Linq;
using System.Reflection;
using Castle.Windsor;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Infrastructure;
using Shuttle.Esb.Castle.Tests.Duplicate;

namespace Shuttle.Esb.Castle.Tests
{
    [TestFixture]
    public class CastleMessageHandlerFactoryFixture
    {
        [Test]
        public void Should_be_able_to_find_message_handlers()
        {
            var container = new WindsorContainer();

            var factory = new CastleMessageHandlerFactory(container);

            factory.RegisterHandlers(GetType().Assembly);

            Assert.IsTrue(factory.MessageTypesHandled.Contains(typeof(SimpleCommand)));
            Assert.IsTrue(factory.MessageTypesHandled.Contains(typeof(SimpleEvent)));
            Assert.IsNotNull(factory.CreateHandler(new SimpleCommand()));
            Assert.IsNotNull(factory.CreateHandler(new SimpleEvent()));
        }

        [Test]
        public void Should_fail_when_attempting_to_register_duplicate_handlers()
        {
            var container = new WindsorContainer();

            var factory = new CastleMessageHandlerFactory(container);

            Assert.Throws<InvalidOperationException>(() => factory.RegisterHandlers(typeof(DuplicateCommand).Assembly));
        }
    }
}