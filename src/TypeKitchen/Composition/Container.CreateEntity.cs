// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using TypeKitchen.Internal;

namespace TypeKitchen.Composition
{
#if LIGHT
	partial class Container
	{
		private readonly Dictionary<uint, List<ValueType>> _componentsByEntity = new Dictionary<uint, List<ValueType>>();

		public uint CreateEntity(params Type[] componentTypes)
		{
			var entity = InitializeEntity(componentTypes);
			foreach (var componentType in componentTypes.NetworkOrder(x => x.Name))
			{
				if (!_componentsByEntity.TryGetValue(entity, out var list))
					_componentsByEntity.Add(entity, list = new List<ValueType>());
				list.Add((ValueType) Activator.CreateInstance(componentType));
			}
			return entity;
		}

		public uint CreateEntity(params object[] components)
		{
			var entity = InitializeEntity(components.Select(x => x.GetType()));
			foreach (var component in components.NetworkOrder(x => x.GetType().Name))
			{
				if (!_componentsByEntity.TryGetValue(entity, out var list))
					_componentsByEntity.Add(entity, list = new List<ValueType>());
				list.Add((ValueType) component);
			}
			return entity;
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		public uint CreateEntity<T1>() where T1 : struct
		{
			return CreateEntity((object) typeof(T1));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2>() where T1 : struct where T2 : struct
		{
			return CreateEntity((object) typeof(T1), typeof(T2));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3>() where T1 : struct where T2 : struct where T3 : struct
		{
			return CreateEntity((object) typeof(T1), typeof(T2), typeof(T3));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct
		{
			return CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
		{
			return CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
		{
			return CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct
		{
			return CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct
		{
			return CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		/// <typeparam name="T9">The ninth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct
		{
			return CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		/// <typeparam name="T9">The ninth type of declared component data.</typeparam>
		/// <typeparam name="T10">The tenth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct
		{
			return CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		/// <typeparam name="T9">The ninth type of declared component data.</typeparam>
		/// <typeparam name="T10">The tenth type of declared component data.</typeparam>
		/// <typeparam name="T11">The eleventh type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct where T11 : struct
		{
			return CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		public uint CreateEntity<T1>(T1 component1) where T1 : struct
		{
			return CreateEntity((object) component1);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2>(T1 component1, T2 component2) where T1 : struct where T2 : struct
		{
			return CreateEntity((object) component1, component2);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3>(T1 component1, T2 component2, T3 component3) where T1 : struct where T2 : struct where T3 : struct
		{
			return CreateEntity((object) component1, component2, component3);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4>(T1 component1, T2 component2, T3 component3, T4 component4) where T1 : struct where T2 : struct where T3 : struct where T4 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5>(T1 component1, T2 component2, T3 component3, T4 component4, T5 component5) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4, component5);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6>(T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4, component5, component6);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7>(T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4, component5, component6, component7);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8>(T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4, component5, component6, component7, component8);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		/// <typeparam name="T9">The ninth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8, T9 component9) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4, component5, component6, component7, component8, component9);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		/// <typeparam name="T9">The ninth type of declared component data.</typeparam>
		/// <typeparam name="T10">The tenth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8, T9 component9, T10 component10) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4, component5, component6, component7, component8, component9, component10);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		/// <typeparam name="T9">The ninth type of declared component data.</typeparam>
		/// <typeparam name="T10">The tenth type of declared component data.</typeparam>
		/// <typeparam name="T11">The eleventh type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8, T9 component9, T10 component10, T11 component11) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct where T11 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4, component5, component6, component7, component8, component9, component10, component11);
		}
	}
#else
	partial class Container
	{
		private readonly Dictionary<uint, List<IComponentProxy>> _componentsByEntity = new Dictionary<uint, List<IComponentProxy>>();

		private Action<ReferenceBuilder> _configureAction;
		public void ConfigureBuilder(Action<ReferenceBuilder> configureAction)
		{
			_configureAction = configureAction;
		}

		public uint CreateEntity(params Type[] componentTypes)
		{
			var entity = InitializeEntity(componentTypes);
			foreach (var componentType in componentTypes.NetworkOrder(x => x.FullName))
				CreateComponentProxy(entity, componentType, null);
			return entity;
		}

		public uint CreateEntity(params object[] components)
		{
			var componentTypes = components.Select(x => x.GetType());
			var entity = InitializeEntity(componentTypes);
			foreach (var component in components.NetworkOrder(x => x.GetType().FullName))
				CreateComponentProxy(entity, component.GetType(), component);
			return entity;
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		public uint CreateEntity<T1>() where T1 : struct
		{
			return CreateEntity(typeof(T1));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2>() where T1 : struct where T2 : struct
		{
			return CreateEntity(typeof(T1), typeof(T2));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3>() where T1 : struct where T2 : struct where T3 : struct
		{
			return CreateEntity(typeof(T1), typeof(T2), typeof(T3));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct
		{
			return CreateEntity(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
		{
			return CreateEntity(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
		{
			return CreateEntity(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct
		{
			return CreateEntity(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct
		{
			return CreateEntity(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		/// <typeparam name="T9">The ninth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct
		{
			return CreateEntity(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		/// <typeparam name="T9">The ninth type of declared component data.</typeparam>
		/// <typeparam name="T10">The tenth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct
		{
			return CreateEntity(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		/// <typeparam name="T9">The ninth type of declared component data.</typeparam>
		/// <typeparam name="T10">The tenth type of declared component data.</typeparam>
		/// <typeparam name="T11">The eleventh type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct where T11 : struct
		{
			return CreateEntity(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		public uint CreateEntity<T1>(T1 component1) where T1 : struct
		{
			return CreateEntity((object) component1);
		} 

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2>(T1 component1, T2 component2) where T1 : struct where T2 : struct
		{
			return CreateEntity((object) component1, component2);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3>(T1 component1, T2 component2, T3 component3) where T1 : struct where T2 : struct where T3 : struct
		{
			return CreateEntity((object) component1, component2, component3);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4>(T1 component1, T2 component2, T3 component3, T4 component4) where T1 : struct where T2 : struct where T3 : struct where T4 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5>(T1 component1, T2 component2, T3 component3, T4 component4, T5 component5) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4, component5);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6>(T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4, component5, component6);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7>(T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4, component5, component6, component7);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8>(T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4, component5, component6, component7, component8);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		/// <typeparam name="T9">The ninth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8, T9 component9) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4, component5, component6, component7, component8, component9);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		/// <typeparam name="T9">The ninth type of declared component data.</typeparam>
		/// <typeparam name="T10">The tenth type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8, T9 component9, T10 component10) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4, component5, component6, component7, component8, component9, component10);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		/// <typeparam name="T4">The fourth type of declared component data.</typeparam>
		/// <typeparam name="T5">The fifth type of declared component data.</typeparam>
		/// <typeparam name="T6">The sixth type of declared component data.</typeparam>
		/// <typeparam name="T7">The seventh type of declared component data.</typeparam>
		/// <typeparam name="T8">The eighth type of declared component data.</typeparam>
		/// <typeparam name="T9">The ninth type of declared component data.</typeparam>
		/// <typeparam name="T10">The tenth type of declared component data.</typeparam>
		/// <typeparam name="T11">The eleventh type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8, T9 component9, T10 component10, T11 component11) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct where T11 : struct
		{
			return CreateEntity((object) component1, component2, component3, component4, component5, component6, component7, component8, component9, component10, component11);
		}
	}
#endif
}
