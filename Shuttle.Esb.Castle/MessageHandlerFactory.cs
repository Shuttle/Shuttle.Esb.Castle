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
        private static readonly Type MessageHandlerType = typeof (IMessageHandler<>);
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

        public override IEnumerable<Type> MessageTypesHandled
        {
            get { return _messageHandlerTypes.Keys; }
        }

        public override IMessageHandler CreateHandler(object message)
        {
            var all = _container.ResolveAll(MessageHandlerType.MakeGenericType(message.GetType()));

            return all.Length != 0 ? (IMessageHandler) all.GetValue(0) : null;
        }

        public void Initialize(IServiceBus bus)
        {
            Guard.AgainstNull(bus, "bus");

            if (!_container.Kernel.HasComponent(typeof (IServiceBus)))
            {
                _container.Register(Component.For<IServiceBus>().Instance(bus));
            }
        }

        public override void ReleaseHandler(IMessageHandler handler)
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
                    foreach (var @interface in type.GetInterfaces())
                    {
                        var messageType = @interface.GetGenericArguments()[0];

                        if (!_messageHandlerTypes.ContainsKey(messageType))
                        {
                            _messageHandlerTypes.Add(messageType, type);
                        }
                        else
                        {
                            _log.Warning(string.Format(CastleResources.DuplicateMessageHandlerIgnored, _messageHandlerTypes[messageType].FullName, messageType.FullName, type.FullName));
                        }

                        _container.Register(Component.For(MessageHandlerType.MakeGenericType(messageType)).ImplementedBy(type).LifestyleTransient());
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warning(string.Format(EsbResources.RegisterHandlersException, assembly.FullName,
                    ex.AllMessages()));
            }

            return this;
        }
    }
}