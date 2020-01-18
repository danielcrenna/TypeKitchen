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

		public List<Entity> ActiveEntities { get; }
		public List<Entity> InactiveEntities { get; }

		public int Ticks { get; private set; }

		public UpdateContext() : this(Container.Create())
		{

		}

		public UpdateContext(Container container)
		{
			_container = container;
			ActiveEntities = new List<Entity>();
			InactiveEntities = new List<Entity>();
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

		public IEnumerable<(Entity, IEnumerable<ValueType>)> StreamEntities(IEnumerable<Entity> source)
		{
			foreach (var entity in source)
				yield return (entity, _container.GetComponents(entity));
		}

		public T GetComponent<T>(Entity entity) where T : struct
		{
			return _container.GetComponent<T>(entity);
		}
		
		public void Reset()
		{
			ActiveEntities.Clear();
			InactiveEntities.Clear();
			States.Clear();
		}
	}
}