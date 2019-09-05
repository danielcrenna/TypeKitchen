using System.Collections.Generic;

namespace TypeKitchen.Composition
{
	public class UpdateContext
	{
		public List<Entity> ActiveEntities { get; }

		public UpdateContext()
		{
			ActiveEntities = new List<Entity>();
		}

		public void Reset()
		{
			ActiveEntities.Clear();
		}
	}
}