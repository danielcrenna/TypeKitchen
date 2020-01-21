// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

			var entity = container.CreateEntity(new Velocity { Value = 10f }, new Position2D());

			var c = container.GetComponents(entity).ToArray();
			var velocity = c[1].QuackLike<Velocity>();
			var position = c[0].QuackLike<Position2D>();
			Assert.Equal(0, position.X);
			Assert.Equal(0, position.Y);
			Assert.Equal(10f, velocity.Value);
			
			container.Update(TimeSpan.FromSeconds(0.1));

			c = container.GetComponents(entity).ToArray();
			position = c[0].QuackLike<Position2D>();
			velocity = c[1].QuackLike<Velocity>();

			Assert.Equal(1, position.X);
			Assert.Equal(1, position.Y);
			Assert.Equal(10f, velocity.Value);
		}

		#region Fakes

		public sealed class VelocitySystem : ISystemWithState<TimeSpan, Velocity, Position2D>
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
			public float Value { get; set; }
		}

		[DebuggerDisplay("({" + nameof(X) + "}, {" + nameof(Y) + "})")]
		public struct Position2D
		{
			public int X { get; set; }
			public int Y { get; set; }
		}

		#endregion

		[Fact]
		public void Inactive_entities_do_not_continue_running()
		{
			var container = Container.Create();
			container.AddSystem<IdentitySystem>();
			container.AddSystem<ComponentSystem>();

			var entity = container.CreateEntity(new Component(), new Identity("A"));
			Assert.Equal(0, container.GetComponent<Component>(entity).Value);

			container.Update(new IdentityEvent("B"), inactive: InactiveHandling.Ignore);
			Assert.Equal(0, container.GetComponent<Component>(entity).Value);
		}

		[Fact]
		public void Active_entities_continue_running()
		{
			var container = Container.Create();
			container.AddSystem<IdentitySystem>();
			container.AddSystem<ComponentSystem>();

			var entity = container.CreateEntity(new Component(), new Identity("A"));
			Assert.Equal(0, container.GetComponent<Component>(entity).Value);

			container.Update(new IdentityEvent("A"));
			Assert.Equal(1, container.GetComponent<Component>(entity).Value);
		}

		[Fact]
		public void Update_context_resets_between_runs()
		{
			var container = Container.Create();
			container.AddSystem<IdentitySystem>();
			container.AddSystem<ComponentSystem>();

			var entity = container.CreateEntity(new Component(), new Identity("A"));
			Assert.Equal(0, container.GetComponent<Component>(entity).Value);

			var context = container.Update(new IdentityEvent("B"), inactive: InactiveHandling.Ignore);
			Assert.Single(context.InactiveEntities);
			Assert.Empty(context.ActiveEntities);
			Assert.Equal(0, container.GetComponent<Component>(entity).Value);

			container.Update(new IdentityEvent("A"), inactive: InactiveHandling.Ignore);
			Assert.Empty(context.InactiveEntities);
			Assert.Single(context.ActiveEntities);
			Assert.Equal(1, container.GetComponent<Component>(entity).Value);
		}

		#region Fakes

		public interface IIdentityEvent
		{
			string Id { get; }
		}

		public class IdentityEvent : IIdentityEvent
		{
			public string Id { get; }

			public IdentityEvent(string id)
			{
				Id = id;
			}
		}

		public struct Component
		{
			public int Value { get; set; }
		}

		public struct Identity
		{
			public string Id { get; }

			public Identity(string id)
			{
				Id = id;
			}
		}

		public class IdentitySystem : ISystemWithState<IIdentityEvent, Identity>
		{
			public bool Update(UpdateContext context, IIdentityEvent evt, ref Identity identity)
			{
				return evt.Id == identity.Id;
			}
		}

		public class ComponentSystem : ISystem<Component>, IDependOn<IdentitySystem>
		{
			public bool Update(UpdateContext context, ref Component component)
			{
				component.Value++;
				return true;
			}
		}

		#endregion

	}
}
