using System.Linq;
using Castle.Windsor;
using NUnit.Framework;

namespace Shuttle.Esb.Castle.Tests
{
    [TestFixture]
    public class CastleMessageHandlerFactoryFaxture
    {
        [Test]
        public void Should_be_able_to_find_message_handlers()
        {
            var container = new WindsorContainer();

            var factory = new CastleMessageHandlerFactory(container);

            factory.RegisterHandlers(GetType().Assembly);

            Assert.IsTrue(factory.MessageTypesHandled.Contains(typeof (SimpleCommand)));
            Assert.IsTrue(factory.MessageTypesHandled.Contains(typeof (SimpleEvent)));
            Assert.IsNotNull(factory.CreateHandler(new SimpleCommand()));
            Assert.IsNotNull(factory.CreateHandler(new SimpleEvent()));
        }
    }
}