using System;
using System.Collections;
using System.Collections.Generic;

namespace TypeKitchen.Composition
{
	public class UpdateContext
	{
		private readonly Container _container;

		internal readonly Queue States;

		public int StateCount => States.Count;

		private readonly List<Entity> _active;
		private readonly List<Entity> _inactive;

		public IReadOnlyList<Entity> ActiveEntities => _active;
		public IReadOnlyList<Entity> InactiveEntities => _inactive;

		public int Ticks { get; private set; }

		public UpdateContext() : this(Container.Create())
		{

		}

		public UpdateContext(Container container)
		{
			_container = container;
			_active = new List<Entity>();
			_inactive = new List<Entity>();
			States = new Queue();
		}

		public void Tick()
		{
			Ticks++;
		}

		public void AddState(object state)
		{
			States.Enqueue(state);
		}

		public IEnumerable<(Entity, IEnumerable<ValueType>)> StreamEntities(IReadOnlyList<Entity> source)
		{
			foreach (var entity in source)
				yield return (entity, _container.GetComponents(entity));
		}

		public IEnumerable<(Entity, IEnumerable<ValueType>)> StreamEntities()
		{
			foreach (var entity in _container.GetEntities())
				yield return (entity, _container.GetComponents(entity));
		}

		public T GetComponent<T>(Entity entity) where T : struct
		{
			return _container.GetComponent<T>(entity);
		}

		public void SetComponent<T>(Entity entity, T value) where T : struct
		{
			_container.SetComponent(entity, value);
		}
		
		public void Reset()
		{
			_active.Clear();
			_inactive.Clear();
			States.Clear();
		}

		internal void AddActiveEntity(uint entity)
		{
			_active.Add(entity);
		}

		internal bool RemoveActiveEntity(uint entity)
		{
			return _active.Remove(entity);
		}

		internal void AddInactiveEntity(uint entity)
		{
			_inactive.Add(entity);
		}

		internal bool RemoveInactiveEntity(uint entity)
		{
			return _inactive.Remove(entity);
		}

		#region CreateEntity

		public uint CreateEntity(params Type[] componentTypes)
        {
	        return _container.CreateEntity(componentTypes);
        }

		public uint CreateEntity(params object[] components)
		{
			return _container.CreateEntity(components);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		public uint CreateEntity<T1>() where T1 : struct
		{
			return _container.CreateEntity((object) typeof(T1));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2>() where T1 : struct where T2 : struct
		{
			return _container.CreateEntity((object) typeof(T1), typeof(T2));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3>() where T1 : struct where T2 : struct where T3 : struct
		{
			return _container.CreateEntity((object) typeof(T1), typeof(T2), typeof(T3));
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
			return _container.CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4));
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
			return _container.CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
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
			return _container.CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
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
			return _container.CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
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
			return _container.CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
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
			return _container.CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
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
			return _container.CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
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
			return _container.CreateEntity((object) typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11));
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		public uint CreateEntity<T1>(T1 component1) where T1 : struct
		{
			return _container.CreateEntity((object) component1);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2>(T1 component1, T2 component2) where T1 : struct where T2 : struct
		{
			return _container.CreateEntity((object) component1, component2);
		}

		/// <summary>
		/// Create a new entity possessing the specified component data.
		/// </summary>
		/// <typeparam name="T1">The first type of declared component data.</typeparam>
		/// <typeparam name="T2">The second type of declared component data.</typeparam>
		/// <typeparam name="T3">The third type of declared component data.</typeparam>
		public uint CreateEntity<T1, T2, T3>(T1 component1, T2 component2, T3 component3) where T1 : struct where T2 : struct where T3 : struct
		{
			return _container.CreateEntity((object) component1, component2, component3);
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
			return _container.CreateEntity((object) component1, component2, component3, component4);
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
			return _container.CreateEntity((object) component1, component2, component3, component4, component5);
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
			return _container.CreateEntity((object) component1, component2, component3, component4, component5, component6);
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
			return _container.CreateEntity((object) component1, component2, component3, component4, component5, component6, component7);
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
			return _container.CreateEntity((object) component1, component2, component3, component4, component5, component6, component7, component8);
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
			return _container.CreateEntity((object) component1, component2, component3, component4, component5, component6, component7, component8, component9);
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
			return _container.CreateEntity((object) component1, component2, component3, component4, component5, component6, component7, component8, component9, component10);
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
			return _container.CreateEntity((object) component1, component2, component3, component4, component5, component6, component7, component8, component9, component10, component11);
		}

		#endregion
	}
}