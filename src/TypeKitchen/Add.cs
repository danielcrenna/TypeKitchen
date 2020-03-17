// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace TypeKitchen
{
	public static class Add
	{
		public static IServiceCollection AddTypeResolver(this IServiceCollection services, ILogger logger = null)
		{
			services.TryAddSingleton<ITypeResolver>(r =>
				new ReflectionTypeResolver(AppDomain.CurrentDomain.GetAssemblies(), logger));
			return services;
		}
	}
}