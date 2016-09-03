using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.Castle
{
    public class CastleMessageHandlerFactory : MessageHandlerFactory, IRequireInitialization
    {
        private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);
        private readonly IWindsorContainer _container;
        private readonly ILog _log;
        private readonly Dictionary<Type, Type> _messageHandlerTypes = new Dictionary<Type, Type>();
        private readonly ReflectionService _reflectionService = new ReflectionService();

        public CastleMessageHandlerFactory(IWindsorContainer container)
        {
            Guard.AgainstNull(container, "container");

            _container = container;

            _log = Log.For(this);
        }

        public override IMessageHandlerFactory RegisterHandler(Type type)
        {
            Guard.AgainstNull(type, "type");

            var serviceTypes = new List<Type>();

            foreach (var @interface in type.GetInterfaces())
            {
                if (!@interface.IsAssignableTo(MessageHandlerType))
                {
                    continue;
                }

                var messageType = @interface.GetGenericArguments()[0];

                if (!_messageHandlerTypes.ContainsKey(messageType))
                {
                    _messageHandlerTypes.Add(messageType, type);
                    serviceTypes.Add(MessageHandlerType.MakeGenericType(messageType));
                }
                else
                {
                    throw new InvalidOperationException(string.Format(EsbResources.DuplicateMessageHandlerException, _messageHandlerTypes[messageType].FullName, messageType.FullName, type.FullName));
                }
            }

            _container.Register(Component.For(serviceTypes).ImplementedBy(type).LifestyleTransient());

            return this;
        }

        public override IEnumerable<Type> MessageTypesHandled
        {
            get { return _messageHandlerTypes.Keys; }
        }

        public override object CreateHandler(object message)
        {
            var all = _container.ResolveAll(MessageHandlerType.MakeGenericType(message.GetType()));

            return all.Length != 0 ? all.GetValue(0) : null;
        }

        public void Initialize(IServiceBus bus)
        {
            Guard.AgainstNull(bus, "bus");

            if (!_container.Kernel.HasComponent(typeof(IServiceBus)))
            {
                _container.Register(Component.For<IServiceBus>().Instance(bus));
            }
        }

        public override void ReleaseHandler(object handler)
        {
            base.ReleaseHandler(handler);

            _container.Release(handler);
        }

        public override IMessageHandlerFactory RegisterHandlers(Assembly assembly)
        {
            try
            {
                foreach (var type in _reflectionService.GetTypes(MessageHandlerType, assembly))
                {
                    RegisterHandler(type);
                }
            }
            catch (Exception ex)
            {
                _log.Fatal(string.Format(EsbResources.RegisterHandlersException, assembly.FullName,
                    ex.AllMessages()));

                throw;
            }

            return this;
        }
    }
}