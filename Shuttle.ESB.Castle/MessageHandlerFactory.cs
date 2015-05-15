using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Shuttle.Core.Infrastructure;
using Shuttle.ESB.Core;

namespace Shuttle.ESB.Castle
{
	public class CastleMessageHandlerFactory : MessageHandlerFactory
	{
		private readonly IWindsorContainer _container;
		private static readonly Type _generic = typeof (IMessageHandler<>);
		private readonly Dictionary<Type, Type> _messageHandlerTypes = new Dictionary<Type, Type>();
		private readonly ILog _log;

		public CastleMessageHandlerFactory(IWindsorContainer container)
		{
			Guard.AgainstNull(container, "container");

			_container = container;

			_log = Log.For(this);
		}

		public override IMessageHandler CreateHandler(object message)
		{
			var all = _container.ResolveAll(_generic.MakeGenericType(message.GetType()));

			return all.Length != 0 ? (IMessageHandler) all.GetValue(0) : null;
		}

		public override IEnumerable<Type> MessageTypesHandled
		{
			get { return _messageHandlerTypes.Keys; }
		}

		public override void Initialize(IServiceBus bus)
		{
			Guard.AgainstNull(bus, "bus");

			if (!_container.Kernel.HasComponent(typeof (IServiceBus)))
			{
				_container.Register(Component.For<IServiceBus>().Instance(bus));
			}

			_messageHandlerTypes.Clear();

			RefreshHandledTypes();
		}

		private void RefreshHandledTypes()
		{
			var handlers = _container.Kernel.GetAssignableHandlers(typeof (IMessageHandler<>));

			foreach (var handler in handlers)
			{
				foreach (var type in handler.ComponentModel.Implementation.InterfacesAssignableTo(_generic))
				{
					var messageType = type.GetGenericArguments()[0];

					if (_messageHandlerTypes.ContainsKey(messageType))
					{
						return;
					}

					_messageHandlerTypes.Add(messageType, type);

					_log.Information(string.Format(ESBResources.MessageHandlerFactoryHandlerRegistered, messageType.FullName, type.FullName));
				}
			}
		}

		public override void ReleaseHandler(IMessageHandler handler)
		{
			base.ReleaseHandler(handler);

			_container.Release(handler);
		}

		public CastleMessageHandlerFactory RegisterHandlers()
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				RegisterHandlers(assembly);
			}

			return this;
		}

		public CastleMessageHandlerFactory RegisterHandlers(Assembly assembly)
		{
			_container.Register(
				Classes.FromAssembly(assembly)
					.BasedOn(typeof (IMessageHandler<>))
					.WithServiceFromInterface()
					.LifestyleTransient()
				);

			RefreshHandledTypes();

			return this;
		}
	}
}