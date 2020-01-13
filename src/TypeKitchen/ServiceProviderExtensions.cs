// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace TypeKitchen
{
	public static class ServiceProviderExtensions
	{
		private static readonly IDictionary<Type, CreateInstance> Activators =
			new ConcurrentDictionary<Type, CreateInstance>();

		private static readonly IDictionary<Type, ConstructorInfo> Constructors =
			new ConcurrentDictionary<Type, ConstructorInfo>();

		private static readonly IDictionary<ConstructorInfo, ParameterInfo[]> ConstructorParameters =
			new ConcurrentDictionary<ConstructorInfo, ParameterInfo[]>();

		public static object AutoResolve<T>(this IServiceProvider serviceProvider, bool throwIfCantResolve = true,
			params Assembly[] assemblies)
		{
			return serviceProvider.AutoResolve(typeof(T), throwIfCantResolve, assemblies);
		}

		public static object AutoResolve(this IServiceProvider serviceProvider, Type serviceType,
			bool throwIfCantResolve = true, params Assembly[] assemblies)
		{
			while (true)
			{
				if (!serviceType.IsAbstract)
					return CreateInstance(serviceProvider, serviceType, throwIfCantResolve, assemblies);

				Type type = null;
				foreach (var assembly in assemblies)
				foreach (var getType in assembly.GetTypes())
				{
					if (!serviceType.IsAssignableFrom(getType) || getType.GetTypeInfo().IsInterface)
						continue;
					type = getType;
					break;
				}

				if (type != null) continue;

				var service = serviceProvider?.GetService(serviceType);
				if (service != null)
					return service;

				if (throwIfCantResolve) throw new InvalidOperationException($"No registration for {serviceType}");
				return null;
			}
		}

		private static object CreateInstance(this IServiceProvider serviceProvider, Type implementationType,
			bool throwIfCantResolve, params Assembly[] assemblies)
		{
			if (!Constructors.TryGetValue(implementationType, out var ctor))
				Constructors[implementationType] = ctor = implementationType.GetWidestConstructor();

			if (!ConstructorParameters.TryGetValue(ctor, out var parameters))
				ConstructorParameters[ctor] = parameters = ctor.GetParameters();

			if (!Activators.TryGetValue(implementationType, out var activator))
				Activators[implementationType] = activator = Activation.ExpressionWeakTyped(ctor);

			var args = new object[parameters.Length];
			for (var i = 0; i < parameters.Length; i++)
			{
				var type = parameters[i].ParameterType;

				args[i] = AutoResolve(serviceProvider, type, throwIfCantResolve, assemblies);
			}

			return activator(args);
		}
	}
}