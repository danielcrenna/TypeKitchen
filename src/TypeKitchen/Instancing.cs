// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection;

namespace TypeKitchen
{
	public static class Instancing
	{
		private static readonly Dictionary<Type, CreateInstance> Factory = new Dictionary<Type, CreateInstance>();
		private static readonly Dictionary<CreateInstance, ParameterInfo[]> Parameters = new Dictionary<CreateInstance, ParameterInfo[]>();
		private static readonly ArrayPool<object> ArgumentsPool = ArrayPool<object>.Create();
		private static readonly ParameterInfo[] EmptyParameters = new ParameterInfo[0];

		public static T CreateInstance<T>()
		{
			return (T) CreateInstance(typeof(T));
		}

		public static T CreateInstance<T>(params object[] args)
		{
			return (T) CreateInstance(typeof(T), args);
		}

		public static T CreateInstance<T>(IServiceProvider serviceProvider)
		{
			return (T) CreateInstance(typeof(T), serviceProvider);
		}

		public static object CreateInstance(Type type)
		{
			var activator = GetOrBuildActivator(type);

			return activator();
		}

		public static object CreateInstance(object instance)
		{
			if (instance is Type type)
				return CreateInstance(type);

			return CreateInstance(instance.GetType());
		}

		public static object CreateInstance(Type type, params object[] args)
		{
			var activator = GetOrBuildActivator(type);

			return activator(args);
		}

		public static object CreateInstance(Type type, IServiceProvider serviceProvider)
		{
			var activator = GetOrBuildActivator(type);
			var parameters = Parameters[activator];
			var args = ArgumentsPool.Rent(parameters.Length);

			try
			{
				for (var i = 0; i < parameters.Length; i++)
				{
					var parameterType = parameters[i].ParameterType;
					var parameter = serviceProvider.GetService(parameterType);
					if(parameter != null)
						args[i] = parameter;
				}

				var instance = activator(args);
				return instance;
			}
			finally
			{
				ArgumentsPool.Return(args, true);
			}
		}

		private static CreateInstance GetOrBuildActivator<T>()
		{
			return GetOrBuildActivator(typeof(T));
		}

		private static CreateInstance GetOrBuildActivator(Type type)
		{
			lock (Factory)
			{
				if (Factory.TryGetValue(type, out var activator))
					return activator;

				lock (Factory)
				{
					if (Factory.TryGetValue(type, out activator))
						return activator;

					var ctor = type.GetConstructor(Type.EmptyTypes) ?? type.GetWidestConstructor();

					if (ctor == null)
					{
						Factory.Add(type, activator = args => Activator.CreateInstance(type));
						Parameters.Add(activator, EmptyParameters);
					}
					else
					{
						Factory.Add(type, activator = Activation.DynamicMethodWeakTyped(ctor));
						Parameters.Add(activator, ctor.GetParameters());
					}
				}

				return activator;
			}
		}
	}
}