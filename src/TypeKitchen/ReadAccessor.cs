// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TypeKitchen.Internal;

namespace TypeKitchen
{
	public static class ReadAccessor
	{
		private static readonly object Sync = new object();

		private static readonly Dictionary<AccessorMembersKey, ITypeReadAccessor> AccessorCache =
			new Dictionary<AccessorMembersKey, ITypeReadAccessor>();

		public static ITypeReadAccessor Create(object @object, out AccessorMembers members)
		{
			return Create(@object, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, AccessorMemberScope.All,
				out members);
		}

		public static ITypeReadAccessor Create(object @object, AccessorMemberTypes types, out AccessorMembers members)
		{
			return Create(@object, types, AccessorMemberScope.All,
				out members);
		}

		public static ITypeReadAccessor Create(object @object, AccessorMemberScope scope, out AccessorMembers members)
		{
			return Create(@object, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, scope,
				out members);
		}

		public static ITypeReadAccessor Create(object @object, AccessorMemberTypes types, AccessorMemberScope scope,
			out AccessorMembers members)
		{
			if (@object is Type type)
				return Create(type, types, scope, out members);
			type = @object.GetType();
			return Create(type, @object, types, scope, out members);
		}

		public static ITypeReadAccessor Create(object @object,
			AccessorMemberTypes types = AccessorMemberTypes.Fields | AccessorMemberTypes.Properties,
			AccessorMemberScope scope = AccessorMemberScope.All)
		{
			if (@object is Type type)
				return Create(type, types, scope);
			type = @object.GetType();
			return Create(type, @object, types, scope, out _);
		}

		public static ITypeReadAccessor Create(Type type, AccessorMemberTypes types, out AccessorMembers members)
		{
			return Create(type, types, AccessorMemberScope.All, out members);
		}

		public static ITypeReadAccessor Create(Type type, AccessorMemberScope scope, out AccessorMembers members)
		{
			return Create(type, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, scope,
				out members);
		}

		public static ITypeReadAccessor Create(Type type, out AccessorMembers members)
		{
			return Create(type, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, AccessorMemberScope.All,
				out members);
		}

		public static ITypeReadAccessor Create(Type type, AccessorMemberTypes types, AccessorMemberScope scope,
			out AccessorMembers members)
		{
			return Create(type, null, types, scope, out members);
		}

		public static ITypeReadAccessor Create(Type type,
			AccessorMemberTypes types = AccessorMemberTypes.Fields | AccessorMemberTypes.Properties,
			AccessorMemberScope scope = AccessorMemberScope.All)
		{
			return Create(type, null, types, scope, out _);
		}

		private static ITypeReadAccessor Create(Type type, object @object, AccessorMemberTypes types,
			AccessorMemberScope scope, out AccessorMembers members)
		{
			lock (Sync)
			{
				var key = KeyForType(type, types, scope);

				if (AccessorCache.TryGetValue(key, out var accessor))
				{
					members = type.IsAnonymous()
						? CreateAnonymousReadAccessorMembers(type)
						: CreateReadAccessorMembers(type, types, scope);
					return accessor;
				}

				var anonymous = type.IsAnonymous();

				accessor = anonymous
					? CreateAnonymousReadAccessor(type, out members, @object)
					: CreateReadAccessor(type, out members, types, scope);

				AccessorCache[key] = accessor;
				return accessor;
			}
		}

		private static AccessorMembersKey KeyForType(Type type, AccessorMemberTypes types, AccessorMemberScope scope)
		{
			var key = type.IsAnonymous()
				? new AccessorMembersKey(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public)
				: new AccessorMembersKey(type, types, scope);
			return key;
		}

		private static ITypeReadAccessor CreateReadAccessor(Type type, out AccessorMembers members,
			AccessorMemberTypes types = AccessorMemberTypes.Fields | AccessorMemberTypes.Properties,
			AccessorMemberScope scope = AccessorMemberScope.All)
		{
			members = CreateReadAccessorMembers(type, types, scope);

			var typeName = CreateNameForType(type);

			TypeBuilder tb;
			try
			{
				tb = DynamicAssembly.Module.DefineType(
					typeName,
					TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit |
					TypeAttributes.AutoClass | TypeAttributes.AnsiClass);

				tb.AddInterfaceImplementation(typeof(ITypeReadAccessor));
			}
			catch (ArgumentException e)
			{
				if (e.Message == "Duplicate type name within an assembly.")
					throw new ArgumentException($"Duplicate type name within an assembly: {typeName}", nameof(typeName),
						e);
				throw;
			}

			//
			// Type Type =>:
			//
			tb.MemberProperty(nameof(ITypeReadAccessor.Type), type,
				typeof(ITypeReadAccessor).GetMethod($"get_{nameof(ITypeReadAccessor.Type)}"));

			//
			// bool TryGetValue(object target, string key, out object value):
			//
			{
				var tryGetValue = tb.DefineMethod(nameof(ITypeReadAccessor.TryGetValue),
					MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
					MethodAttributes.Virtual | MethodAttributes.NewSlot, typeof(bool),
					new[] {typeof(object), typeof(string), typeof(object).MakeByRefType()});
				var il = tryGetValue.GetILGeneratorInternal();

				var branches = new Dictionary<AccessorMember, Label>();
				foreach (var member in members)
					branches.Add(member, il.DefineLabel());
				var fail = il.DefineLabel();

				foreach (var member in members)
				{
					il.Ldarg_2(); // key
					il.GotoIfStringEquals(member.Name, branches[member]); // if (key == "Foo") goto found;
				}

				il.Br(fail);

				foreach (var member in members)
				{
					il.MarkLabel(branches[member]);		// found:
					il.Ldarg_3();						// value
					il.Ldarg_1();						// target
					il.CastOrUnbox(type);				// ({Type}) target
					switch (member.MemberInfo)			// result = target.{member.Name}
					{
						case PropertyInfo property:
							il.Callvirt(property.GetGetMethod(true));
							break;
						case FieldInfo field:
							il.Ldfld(field);
							break;
					}

					il.MaybeBox(member.Type);			// (object) result
					il.Stind_Ref();						// value = result
					il.Ldc_I4_1();						// 1
					il.Ret();							// return 1 (true)
				}

				il.MarkLabel(fail);
				il.Ldarg_3();							// value
				il.Ldnull();							// null
				il.Stind_Ref();							// value = null
				il.Ldc_I4_0();							// 0
				il.Ret();								// return 0 (false)

				tb.DefineMethodOverride(tryGetValue, typeof(ITypeReadAccessor).GetMethod("TryGetValue"));
			}

			//
			// object this[object target, string key]:
			//
			{
				var getItem = tb.DefineMethod("get_Item",
					MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
					MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.SpecialName, typeof(object),
					new[] {typeof(object), typeof(string)});
				var il = getItem.GetILGeneratorInternal();

				var branches = new Dictionary<AccessorMember, Label>();
				foreach (var member in members)
					branches.Add(member, il.DefineLabel());

				foreach (var member in members)
				{
					il.Ldarg_2();											// key
					il.GotoIfStringEquals(member.Name, branches[member]);	// if (key == "{member.Name}") goto found;
				}

				foreach (var member in members)
				{
					il.MarkLabel(branches[member]);
					il.Ldarg_1();					// target
					il.CastOrUnbox(type);			// ({Type}) target

					switch (member.MemberInfo)		// result = target.Foo
					{
						case PropertyInfo property:
							il.Callvirt(property.GetGetMethod(true));
							break;
						case FieldInfo field:
							il.Ldfld(field);
							break;
					}

					il.MaybeBox(member.Type);       // (object) result
					il.Ret();						// return result;
				}

				il.Newobj(typeof(ArgumentNullException).GetConstructor(Type.EmptyTypes));
				il.Throw();

				var getItemProperty = tb.DefineProperty("Item", PropertyAttributes.SpecialName, typeof(object),
					new[] {typeof(string)});
				getItemProperty.SetGetMethod(getItem);

				tb.DefineMethodOverride(getItem, typeof(ITypeReadAccessor).GetMethod("get_Item"));
			}

			var typeInfo = tb.CreateTypeInfo();
			return (ITypeReadAccessor) Activator.CreateInstance(typeInfo.AsType(), false);
		}
		
		private static string CreateNameForType(Type type)
		{
			var assemblyName = type.Assembly.IsDynamic ? "Dynamic" : type.Assembly.GetName().Name;
			var name = type.IsAnonymous()
				? $"ReadAccessor_Anonymous_{assemblyName}_{type.AssemblyQualifiedName}"
				: $"ReadAccessor_{assemblyName}_{type.AssemblyQualifiedName}";
			return name;
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

		/// <summary>
		///     Anonymous types only have private readonly properties with no logic before their backing fields, so we can do
		///     a lot to optimize access to them, though we must delegate the access itself due to private reflection rules.
		/// </summary>
		private static ITypeReadAccessor CreateAnonymousReadAccessor(Type type, out AccessorMembers members,
			object debugObject = null)
		{
			members = CreateAnonymousReadAccessorMembers(type);

			var typeName = CreateNameForType(type);

			var tb = DynamicAssembly.Module.DefineType(typeName,
				TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit |
				TypeAttributes.AutoClass | TypeAttributes.AnsiClass);
			tb.AddInterfaceImplementation(typeof(ITypeReadAccessor));

			//
			// Perf: Add static delegates on the type, that store access to the backing fields behind the readonly properties.
			//
			var staticFieldsByMethod = new Dictionary<MethodBuilder, Func<object, object>>();
			var staticFieldsByMember = new Dictionary<AccessorMember, FieldBuilder>();
			foreach (var member in members)
			{
				var backingField = type.GetField($"<{member.Name}>i__Field",
					BindingFlags.NonPublic | BindingFlags.Instance);
				if (backingField == null)
					throw new NullReferenceException();

				var dm = new DynamicMethod($"_{member.Name}", typeof(object), new[] {typeof(object)}, tb.Module);
				var dmIl = dm.GetILGeneratorInternal();
				dmIl.Ldarg_0()
					.Ldfld(backingField);
				if (backingField.FieldType.IsValueType)
					dmIl.Box(backingField.FieldType);
				dmIl.Ret();
				var backingFieldDelegate = (Func<object, object>) dm.CreateDelegate(typeof(Func<object, object>));

				var getField = tb.DefineField($"_Get{member.Name}", typeof(Func<object, object>),
					FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly);
				var setField = tb.DefineMethod($"_SetGet{member.Name}",
					MethodAttributes.Static | MethodAttributes.Private, CallingConventions.Standard, typeof(void),
					new[] {typeof(Func<object, object>)});
				var setFieldIl = setField.GetILGeneratorInternal();
				setFieldIl.Ldarg_0();
				setFieldIl.Stsfld(getField);
				setFieldIl.Ret();

				staticFieldsByMethod.Add(setField, backingFieldDelegate);
				staticFieldsByMember.Add(member, getField);
			}

			//
			// Type Type =>:
			//
			{
				tb.MemberProperty(nameof(ITypeReadAccessor.Type), type,
					typeof(ITypeReadAccessor).GetMethod($"get_{nameof(ITypeReadAccessor.Type)}"));
			}

			//
			// bool TryGetValue(object target, string key, out object value):
			//
			{
				var tryGetValue = tb.DefineMethod(nameof(ITypeReadAccessor.TryGetValue),
					MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
					MethodAttributes.Virtual | MethodAttributes.NewSlot, typeof(bool),
					new[] {typeof(object), typeof(string), typeof(object).MakeByRefType()});
				var il = tryGetValue.GetILGeneratorInternal();

				var branches = new Dictionary<AccessorMember, Label>();
				foreach (var member in members)
					branches.Add(member, il.DefineLabel());
				var fail = il.DefineLabel();

				foreach (var member in members)
				{
					il.Ldarg_2(); // key
					il.GotoIfStringEquals(member.Name, branches[member]); // if(key == "Foo") goto found;
				}

				il.Br(fail);

				foreach (var member in members)
				{
					var fb = staticFieldsByMember[member];

					il.MarkLabel(branches[member]); // found:
					il.Ldarg_3(); //     value
					il.Ldsfld(fb); //     _GetFoo
					il.Ldarg_1(); //     target
					il.Call(fb.FieldType.GetMethod("Invoke")); //     result = _GetFoo.Invoke(target)
					il.Stind_Ref(); //     value = result
					il.Ldc_I4_1(); //     1
					il.Ret(); //     return 1 (true)
				}

				il.MarkLabel(fail);
				il.Ldarg_3(); //     value
				il.Ldnull(); //     null
				il.Stind_Ref(); //     value = null
				il.Ldc_I4_0(); //     0
				il.Ret(); //     return 0 (false)

				tb.DefineMethodOverride(tryGetValue,
					typeof(ITypeReadAccessor).GetMethod(nameof(ITypeReadAccessor.TryGetValue)));
			}

			//
			// object this[object target, string key]:
			//
			{
				var getItem = tb.DefineMethod("get_Item",
					MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
					MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.SpecialName, typeof(object),
					new[] {typeof(object), typeof(string)});
				var il = getItem.GetILGeneratorInternal();

				var branches = new Dictionary<AccessorMember, Label>();
				foreach (var member in members)
					branches.Add(member, il.DefineLabel());

				foreach (var member in members)
				{
					il.Ldarg_2(); // key
					il.GotoIfStringEquals(member.Name, branches[member]); // if(key == "Foo") goto found;
				}

				foreach (var member in members)
				{
					var fb = staticFieldsByMember[member];

					il.MarkLabel(branches[member]); // found:
					il.Ldsfld(fb); // _GetFoo
					il.Ldarg_1(); // target
					il.Call(fb.FieldType.GetMethod("Invoke")); //     result = _GetFoo.Invoke(target)
					il.Ret(); // return result;
				}

				il.Newobj(typeof(ArgumentNullException).GetConstructor(Type.EmptyTypes));
				il.Throw();

				var item = tb.DefineProperty("Item", PropertyAttributes.SpecialName, typeof(object),
					new[] {typeof(string)});
				item.SetGetMethod(getItem);

				tb.DefineMethodOverride(getItem, typeof(ITypeReadAccessor).GetMethod("get_Item"));
			}
			
			var typeInfo = tb.CreateTypeInfo();

			//
			// Perf: Set static field values to generated delegate instances.
			//
			foreach (var setter in staticFieldsByMethod)
			{
				var setField = typeInfo.GetMethod(setter.Key.Name, BindingFlags.Static | BindingFlags.NonPublic);
				if (setField == null)
					throw new NullReferenceException();
				setField.Invoke(null, new object[] {setter.Value});

				if (debugObject != null)
				{
					var memberName = setter.Key.Name.Replace("_SetGet", string.Empty);

					var staticFieldFunc = (Func<object, object>) typeInfo.GetField($"_Get{memberName}").GetValue(debugObject);
					if (staticFieldFunc != setter.Value)
						throw new ArgumentException($"replacing _Get{memberName} with function from _SetGet{memberName} was unsuccessful");

					var backingField = type.GetField($"<{memberName}>i__Field",
						BindingFlags.NonPublic | BindingFlags.Instance);
					if (backingField == null)
						throw new NullReferenceException("backing field was not found");

					var backingFieldValue = backingField.GetValue(debugObject);
					var cachedDelegateValue = setter.Value(debugObject);
					if (!backingFieldValue.Equals(cachedDelegateValue))
						throw new ArgumentException($"{memberName} backing field value '{backingFieldValue}' does not agree with cached delegate value {cachedDelegateValue}");
				}
			}

			var accessor = (ITypeReadAccessor) Activator.CreateInstance(typeInfo, false);

			if (debugObject != null)
			{
				foreach (var member in members)
				{
					var byAccessor = accessor[debugObject, member.Name];
					var byReflection = ((Func<object, object>) typeInfo.GetField($"_Get{member.Name}").GetValue(debugObject))(debugObject);
					if (!byAccessor.Equals(byReflection))
						throw new InvalidOperationException("IL produced incorrect accessor");
				}
			}

			return accessor;
		}
	}
}