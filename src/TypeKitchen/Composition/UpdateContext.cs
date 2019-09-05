using System.Collections;
using System.Collections.Generic;

namespace TypeKitchen.Composition
{
	public class UpdateContext
	{
		public List<Entity> ActiveEntities { get; }
		public Queue States { get; }

		public UpdateContext()
		{
			ActiveEntities = new List<Entity>();
			States = new Queue();
		}

		public void AddState(object state)
		{
			States.Enqueue(state);
		}
		
		public void Reset()
		{
			ActiveEntities.Clear();
			States.Clear();
		}
	}
}