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
            var entity = container.CreateEntity<Velocity, Position2D>();
			entity.Set(new Velocity { Value = 10f }, container);
            container.AddSystem<VelocitySystem>();
            container.Update();

            var c = container.GetComponents(entity).ToArray();
            Assert.Equal(1, ((Position2D) c[0]).X);
            Assert.Equal(1, ((Position2D) c[0]).Y);
			Assert.Equal(10, ((Velocity) c[1]).Value);
		}

		[DebuggerDisplay("{" + nameof(Value) + "}")]
		public struct Velocity
		{
			public float Value;
		}

		[DebuggerDisplay("({" + nameof(X) + "}, {" + nameof(Y) + "})")]
		public struct Position2D
		{
			public int X;
			public int Y;
		}

		public sealed class VelocitySystem : ISystem<Velocity, Position2D>
		{
			public void Update(ref Velocity velocity, ref Position2D position)
			{
				position.X += (int) (velocity.Value * 0.1f);
				position.Y += (int) (velocity.Value * 0.1f);
			}
		}
	}
}
