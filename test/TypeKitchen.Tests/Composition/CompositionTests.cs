using System.Diagnostics;
using System.Linq;
using TypeKitchen.Composition;
using Xunit;

namespace TypeKitchen.Tests.Composition
{
	public class CompositionTests
	{
		[Fact]
		public void BasicTests_compose_simple_system()
		{
            var container = Container.Create();
            container.CreateEntity(typeof(A), typeof(B));
            container.AddSystem<System>();
            container.Update();

            var c = container.GetComponents(1).ToArray();
			Assert.Equal(+1, ((A) c[0]).Value);
			Assert.Equal(-1, ((B) c[1]).Value);
		}

		[DebuggerDisplay("{GetType().Name}: {Value}")]
		public class A
		{
			public float Value { get; set; }
		}

		[DebuggerDisplay("{GetType().Name}: {Value}")]
		public class B
		{
			public float Value { get; set; }
		}

		public class System : ISystem<A, B>
		{
			public void Update(ref A a, ref B b)
			{
				a.Value += 1;
				b.Value -= 1;
			}
		}
	}
}
