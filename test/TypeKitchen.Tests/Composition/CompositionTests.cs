using System;
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
            container.AddSystem<VelocitySystem>();
            container.AddSystem<ClockSystem>();

			var entity = container.CreateEntity(new Velocity { Value = 10f }, new Position2D());
			container.Update(TimeSpan.FromSeconds(0.1));

			var c = container.GetComponents(entity).ToArray();
			var position = c[0].QuackLike<Position2D>();
			var velocity = c[1].QuackLike<Velocity>();

			Assert.Equal(1, position.X);
			Assert.Equal(1, position.Y);
			Assert.Equal(10, velocity.Value);
		}

		public sealed class ClockSystem : ISystem<float>
		{
			public bool Update(UpdateContext updateContext, ref float elapsed)
			{
				return false;
			}
		}

		public sealed class VelocitySystem : ISystemWithState<TimeSpan, Velocity, Position2D>, IDependOn<ClockSystem>
		{
			public bool Update(UpdateContext updateContext, TimeSpan elapsedTime, ref Velocity velocity, ref Position2D position)
			{
				var delta = elapsedTime.Milliseconds * 0.001;
				position.X += (int) (velocity.Value * delta);
				position.Y += (int) (velocity.Value * delta);
				return true;
			}
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
	}
}
