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

		internal static readonly Dictionary<int, Type> ProxyCache = new Dictionary<int, Type>();

		public static Type Create(object @object, out AccessorMembers members, ProxyType proxyType = ProxyType.Pure)
		{
			if (@object is Type t)
				return Create(t, out members, proxyType);
			t = @object.GetType();
			return Create(t, out members, proxyType);
		}

		public static Type Create(object @object, ProxyType proxyType = ProxyType.Pure)
		{
			if (@object is Type type)
				return Create(type, proxyType);
			type = @object.GetType();
			return Create(type, out _, proxyType);
		}

		public static Type Create(Type type, ProxyType proxyType = ProxyType.Pure)
		{
			return Create(type, out _, proxyType);
		}

		public static Type Create(Type type, out AccessorMembers members, ProxyType proxyType = ProxyType.Pure)
		{
			lock (Sync)
			{
				var key = KeyForType(type, AccessorMemberTypes.All, AccessorMemberScope.Public, proxyType);

				if (ProxyCache.TryGetValue(key, out var proxy))
				{
					members = type.IsAnonymous()
						? CreateAnonymousReadAccessorMembers(type)
						: CreateReadAccessorMembers(type, AccessorMemberTypes.All);
					return proxy;
				}

				proxy = CreateProxy(type, out members, proxyType);
				ProxyCache[key] = proxy;
				return proxy;
			}
		}

		private static AccessorMembers CreateReadAccessorMembers(Type type,
			AccessorMemberTypes types = AccessorMemberTypes.Fields | AccessorMemberTypes.Properties,
			AccessorMemberScope scope = AccessorMemberScope.Public)
		{
			return AccessorMembers.Create(type, types, scope);
		}

		private static AccessorMembers CreateAnonymousReadAccessorMembers(Type type)
		{
			return AccessorMembers.Create(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public);
		}

		private static int KeyForType(Type type, AccessorMemberTypes types, AccessorMemberScope scope, ProxyType proxyType)
		{
			var key = type.IsAnonymous()
				? new AccessorMembersKey(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public)
				: new AccessorMembersKey(type, types, scope);

			return key.GetHashCode() ^ proxyType.GetHashCode();
		}
		
		private static Type CreateProxy(Type type, out AccessorMembers members, ProxyType proxyType)
		{
			Type parent;
			switch (proxyType)
			{
				case ProxyType.Pure:
				case ProxyType.Hybrid:
					if (type.IsSealed)
						throw new InvalidOperationException("Cannot create a proxy for a sealed type, you should create a mimic instead.");
					parent = type;
					break;
				case ProxyType.Mimic:
					parent = type.IsValueType ? typeof(ValueType) : typeof(object);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			var proxyName = CreateNameForType(type, proxyType);
			var tb = DynamicAssembly.Module.DefineType(proxyName, TypeAttributes.Public, parent);
			if(type.IsInterface)
				tb.AddInterfaceImplementation(type);

			members = AccessorMembers.Create(type, AccessorMemberTypes.All, AccessorMemberScope.Public);

			const MethodAttributes ma = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

			foreach (var member in members)
			{
				switch (member.MemberInfo)
				{
					case MethodInfo method:
					{
						if (method.ShouldSkip(proxyType))
							continue;
						if (member.CanCall)
							tb.ForwardFrom(method);
						break;
					}
						
					case FieldInfo field:
					{
						switch (proxyType)
						{
							case ProxyType.Pure:
							{
								break;
							}
							case ProxyType.Hybrid:
							{
								var pb = tb.DefineProperty(field.Name, PropertyAttributes.None, field.FieldType, null);
								var get = tb.DefineMethod($"get_{field.Name}", ma, field.FieldType, Type.EmptyTypes);
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
								pb.SetGetMethod(get);

								var set = tb.DefineMethod($"set_{field.Name}", ma, typeof(void), new[] {field.FieldType});
								il = set.GetILGeneratorInternal();
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
								pb.SetSetMethod(set);
								break;
							}

							case ProxyType.Mimic:
							{
								tb.DefineField(field.Name, field.FieldType, field.Attributes);
								break;
							}

							default:
								throw new ArgumentOutOfRangeException();
						}
						
						break;
					}

					case PropertyInfo property:
					{
						switch (proxyType)
						{
							case ProxyType.Pure:
							case ProxyType.Hybrid:
							{
								if (property.GetMethod.ShouldSkip(proxyType))
									continue;
								var pb = tb.DefineProperty(property.Name, property.Attributes, property.PropertyType, null);
								if (member.CanRead)
								{
									var getMethod = property.GetGetMethod();
									pb.SetGetMethod(tb.ForwardFrom(getMethod));
								}
								if (member.CanWrite)
								{
									var setMethod = property.GetSetMethod();
									pb.SetSetMethod(tb.ForwardFrom(setMethod));
								}
								break;
							}

							case ProxyType.Mimic:
							{
								var propertyName = property.Name;
								var propertyType = property.PropertyType;

								var fb = tb.DefineField($"<{propertyName}>k__BackingField", propertyType, FieldAttributes.Private);
								var pb = tb.DefineProperty(propertyName, property.Attributes, propertyType, null);

								if (member.CanRead)
								{
									var mb = tb.DefineMethod($"get_{propertyName}", type.IsInterface ? ma | MethodAttributes.Virtual : ma, propertyType, Type.EmptyTypes);
									var il = mb.GetILGeneratorInternal();
									il.Ldarg_0();
									il.Ldfld(fb);
									il.Ret();
									pb.SetGetMethod(mb);
								}

								if (member.CanWrite)
								{
									var mb = tb.DefineMethod($"set_{propertyName}", type.IsInterface ? ma | MethodAttributes.Virtual : ma, typeof(void), new [] { propertyType });
									var il = mb.GetILGeneratorInternal();
									il.Ldarg_0();
									il.Ldarg_1();
									il.Stfld(fb);
									il.Ret();
									pb.SetSetMethod(mb);
								}
								break;
							}

							default:
								throw new ArgumentOutOfRangeException(nameof(proxyType), proxyType, null);
						}
						
						break;
					}
				}
			}

			var typeInfo = tb.CreateTypeInfo();
			return typeInfo.AsType();
		}

		private static bool ShouldSkip(this MethodBase method, ProxyType proxyType)
		{
			switch (proxyType)
			{
				case ProxyType.Pure:
				case ProxyType.Hybrid:
					if (!method.IsVirtual)
						return true; // don't hide non-virtual methods
					break;
				case ProxyType.Mimic:
					if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
						return true; // don't duplicate property accessors
					if (method.DeclaringType == typeof(object) || method.DeclaringType == typeof(ValueType))
						return true; // don't duplicate base methods
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(proxyType), proxyType, null);
			}

			return false;
		}

		/// <summary> Create a method that calls another method. </summary>
		internal static MethodBuilder ForwardFrom(this TypeBuilder tb, MethodInfo method)
		{
			var parameterTypes = method.GetParameters().Select(x => x.ParameterType).ToArray(); // FIXME self-enumerate

			var mb = tb.DefineMethod(method.Name, method.Attributes | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot, method.ReturnType, parameterTypes);
			mb.SetImplementationFlags(MethodImplAttributes.AggressiveInlining);

			if (method.IsVirtual)
				tb.DefineMethodOverride(mb, method);

			var forward = mb.GetILGeneratorInternal();

			forward.Ldarg_0();
			forward.Ldarg_1();
			forward.CallOrCallvirt(method, tb);
			forward.Ret();

			return mb;
		}

		private static string CreateNameForType(Type type, ProxyType proxyType)
		{
			var assemblyName = type.Assembly.IsDynamic ? "Dynamic" : type.Assembly.GetName().Name;
			var name = type.IsAnonymous()
				? $"Proxy_Anonymous_{assemblyName}_{type.AssemblyQualifiedName}_{proxyType}"
				: $"Proxy_{assemblyName}_{type.AssemblyQualifiedName}_{proxyType}";
			return name;
		}
	}
}