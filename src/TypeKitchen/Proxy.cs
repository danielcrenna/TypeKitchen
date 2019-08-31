// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TypeKitchen.Internal;

namespace TypeKitchen
{
	public static class Proxy
	{
		private static readonly object Sync = new object();

		private static readonly Dictionary<AccessorMembersKey, Type> ProxyCache =
			new Dictionary<AccessorMembersKey, Type>();

		public static Type Create(object @object, out AccessorMembers members)
		{
			return Create(@object, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, AccessorMemberScope.All,
				out members);
		}

		public static Type Create(object @object, AccessorMemberTypes types, out AccessorMembers members)
		{
			return Create(@object, types, AccessorMemberScope.All,
				out members);
		}

		public static Type Create(object @object, AccessorMemberScope scope, out AccessorMembers members)
		{
			return Create(@object, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, scope,
				out members);
		}

		public static Type Create(object @object, AccessorMemberTypes types, AccessorMemberScope scope,
			out AccessorMembers members)
		{
			if (@object is Type type)
				return Create(type, out members);
			type = @object.GetType();
			return Create(type, types, scope, out members);
		}

		public static Type Create(object @object,
			AccessorMemberTypes types = AccessorMemberTypes.Fields | AccessorMemberTypes.Properties,
			AccessorMemberScope scope = AccessorMemberScope.All)
		{
			if (@object is Type type)
				return Create(type, types, scope);
			type = @object.GetType();
			return Create(type, types, scope, out _);
		}

		public static Type Create(Type type, AccessorMemberTypes types, out AccessorMembers members)
		{
			return Create(type, types, AccessorMemberScope.All, out members);
		}

		public static Type Create(Type type, AccessorMemberScope scope, out AccessorMembers members)
		{
			return Create(type, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, scope,
				out members);
		}

		public static Type Create(Type type, out AccessorMembers members)
		{
			return Create(type, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, AccessorMemberScope.All,
				out members);
		}
		
		public static Type Create(Type type,
			AccessorMemberTypes types = AccessorMemberTypes.Fields | AccessorMemberTypes.Properties,
			AccessorMemberScope scope = AccessorMemberScope.All)
		{
			return Create(type, types, scope, out _);
		}

		private static Type Create(Type type, AccessorMemberTypes types, AccessorMemberScope scope, out AccessorMembers members)
		{
			lock (Sync)
			{
				var key = KeyForType(type, types, scope);

				if (ProxyCache.TryGetValue(key, out var proxy))
				{
					members = type.IsAnonymous()
						? CreateAnonymousReadAccessorMembers(type)
						: CreateReadAccessorMembers(type, types, scope);
					return proxy;
				}

				proxy = CreateProxy(type, out members, types, scope);
				ProxyCache[key] = proxy;
				return proxy;
			}
		}

		private static AccessorMembers CreateReadAccessorMembers(Type type,
			AccessorMemberTypes types = AccessorMemberTypes.Fields | AccessorMemberTypes.Properties,
			AccessorMemberScope scope = AccessorMemberScope.All)
		{
			return AccessorMembers.Create(type, types, scope);
		}

		private static AccessorMembers CreateAnonymousReadAccessorMembers(Type type)
		{
			return AccessorMembers.Create(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public);
		}

		private static AccessorMembersKey KeyForType(Type type, AccessorMemberTypes types, AccessorMemberScope scope)
		{
			var key = type.IsAnonymous()
				? new AccessorMembersKey(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public)
				: new AccessorMembersKey(type, types, scope);
			return key;
		}
		
		private static Type CreateProxy(Type type, out AccessorMembers members, AccessorMemberTypes types = AccessorMemberTypes.All, AccessorMemberScope scope = AccessorMemberScope.All)
		{
			var tb = DynamicAssembly.Module.DefineType(CreateNameForType(type), TypeAttributes.Public, type);
			members = AccessorMembers.Create(type, types, scope);

			foreach (var member in members)
			{
				switch (member.MemberInfo)
				{
					case MethodInfo method:
						if(member.CanCall)
							tb.ForwardFrom(method);
						break;
					case FieldInfo field:
					{
						if (member.CanRead)
						{
							var get = new DynamicMethod($"_Get{field.Name}", field.FieldType, new[] {type}, true);
							var il = get.GetILGeneratorInternal();
							if (field.IsStatic)
							{
								il.Ldsfld(field);
							}
							else
							{
								il.Ldarg_0();
								il.Ldfld(field);
							}
							il.Ret();
						}

						if (member.CanWrite)
						{
							var set = new DynamicMethod($"_Set{field.Name}", null, new[] {type, field.FieldType}, true);
							var il = set.GetILGeneratorInternal();
							if (field.IsStatic)
							{
								il.Ldarg_1();
								il.Stsfld(field);
							}
							else
							{
								il.Ldarg_0();
								il.Ldarg_1();
								il.Stfld(field);
							}
							il.Ret();
						}

						break;
					}
					case PropertyInfo property:
						if(member.CanRead)
							tb.ForwardFrom(property.GetGetMethod());
						if(member.CanWrite)
							tb.ForwardFrom(property.GetSetMethod());
						break;
				}
			}

			return tb.CreateTypeInfo().AsType();
		}

		internal static void ForwardFrom(this TypeBuilder tb, MethodInfo method)
		{
			var parameterTypes = method.GetParameters().Select(x => x.ParameterType).ToArray(); // FIXME self-enumerate

			var mb = tb.DefineMethod(method.Name, method.Attributes | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot, method.ReturnType, parameterTypes);
			mb.SetImplementationFlags(MethodImplAttributes.AggressiveInlining);

			if (method.IsVirtual)
				tb.DefineMethodOverride(mb, method);

			var forward = mb.GetILGeneratorInternal();

			forward.Ldarg_0();
			forward.Ldarg_1();
			forward.Call(method);
			forward.Ret();
		}

		private static string CreateNameForType(Type type)
		{
			var assemblyName = type.Assembly.IsDynamic ? "Dynamic" : type.Assembly.GetName().Name;
			var name = type.IsAnonymous()
				? $"Proxy_Anonymous_{assemblyName}_{type.AssemblyQualifiedName}"
				: $"Proxy_{assemblyName}_{type.AssemblyQualifiedName}";
			return name;
		}
	}
}