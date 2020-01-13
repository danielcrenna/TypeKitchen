using System.Collections;
using System.Collections.Generic;

namespace TypeKitchen.Composition
{
	public class UpdateContext
	{
		public List<Entity> ActiveEntities { get; }
		public List<Entity> InactiveEntities { get; }

		public Queue States { get; }
		public int Ticks { get; private set; }

		public UpdateContext()
		{
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
		
		public void Reset()
		{
			ActiveEntities.Clear();
			InactiveEntities.Clear();
			States.Clear();
		}
	}
}