// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TypeKitchen.Composition
{
	public partial class Container
	{
		private readonly Value128 _seed;

		private Container(Value128 seed)
		{
			_seed = seed;
		}

		private readonly List<ISystem> _systems = new List<ISystem>();
		private Value128[] _archetypes = new Value128[0];
		private uint[] _entities = new uint[0];
		
		public static Container Create(Value128 seed = default)
		{
			return new Container(seed);
		}

		private IEnumerable<ExecutionPlanLine> _executionPlan;

		private struct ExecutionPlanLine
		{
			public ISystem System;
			public MethodInfo Update;
			public ParameterInfo[] Parameters;
			public int Start;
			public int Length;
		}

		public void AddSystem<T>() where T : ISystem, new()
		{
			var system = new T();
			_systems.Add(system);
		}

		private IEnumerable<ExecutionPlanLine> BuildExecutionPlan()
		{
			var dependencyMap = new Dictionary<Type, List<ISystem>>();

			foreach (var system in _systems)
			{
				var dependencies = system.GetType().GetTypeInfo().ImplementedInterfaces
					.Where(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IDependOn<>))
					.SelectMany(y => y.GetGenericArguments());

				foreach (var dependency in dependencies)
				{
					if (!dependencyMap.TryGetValue(dependency, out var dependents))
						dependencyMap.Add(dependency, dependents = new List<ISystem>());

					dependents.Add(system);
				}
			}

			var indices = Enumerable.Range(0, _systems.Count).ToList();

			var order = indices.TopologicalSort(i =>
			{
				var s = _systems[i];
				return !dependencyMap.TryGetValue(s.GetType(), out var dependents)
					? Enumerable.Empty<int>()
					: dependents.Select(x => _systems.IndexOf(x));
			}).ToArray();

			foreach (var system in _systems.OrderBy(x =>
			{
				var index = Array.IndexOf(order, _systems.IndexOf(x));
				return index < 0 ? int.MaxValue : index;
			}))
			{
				var archetype = system.Archetype(_seed);
				int? start = null;
				var length = 0;
				for (var i = 0; i < _archetypes.Length; i++)
				{
					if (_archetypes[i] == archetype)
					{
						if(!start.HasValue)
							start = i;
						length++;
					}
					else if (start.HasValue)
						break;
				}

				var update = system.GetType().GetMethod(nameof(ExecutionPlanLine.Update));
				if (update == null)
					continue;

				var line = new ExecutionPlanLine
				{
					System = system,
					Update = update,
					Start = start.GetValueOrDefault(),
					Length = length,
					Parameters = update.GetParameters()
				};

				yield return line;
			}
		}

		public void Update()
		{
			Update<object>();
		}

		public void Update<TState>(TState state = default)
		{
			_executionPlan = _executionPlan ?? BuildExecutionPlan();

			foreach (var line in _executionPlan)
			{
				if (line.Length == 0)
					continue;

				var arguments = Pooling.Arguments.Get(line.Parameters.Length);
				try
				{
					var entities = new ReadOnlySpan<uint>(_entities, line.Start, line.Length);

					foreach (var entity in entities)
					{
						var components = _componentsByEntity[entity];
						for (var i = 0; i < line.Parameters.Length; i++)
						{
							var type = line.Parameters[i].ParameterType;
							if (type == typeof(TState))
							{
								arguments[i] = state;
								continue;
							}
							foreach (var c in components)
							{
								if (c.RefType != type)
									continue;
								var refMethod = c.GetType().GetProperty("Ref");
								var surrogate = refMethod?.GetValue(c);
								arguments[i] = surrogate;
							}
						}

						line.Update.Invoke(line.System, arguments);

						foreach (var argument in arguments)
							SetComponent(entity, argument.GetType(), argument);
					}
				}
				finally
				{
					Pooling.Arguments.Return(arguments);
				}
			}
		}

		public void SetComponent<T>(uint entity, T value) where T : struct
		{
			SetComponent(entity, typeof(T), value);
		}

		private void SetComponent(uint entity, Type type, object value)
		{
			for (var i = 0; i < _componentsByEntity[entity].Count; i++)
			{
				var c = _componentsByEntity[entity][i];
				if (type.MakeByRefType() != c.RefType)
					continue;

				// FIXME: accessors violate memory :-(
				var readProxy = ReadAccessor.Create(c, AccessorMemberTypes.Properties, AccessorMemberScope.Public, out var proxyMembers);
				var readSurrogate = ReadAccessor.Create(value, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, AccessorMemberScope.Public, out var surrogateMembers);
				
				foreach (var member in surrogateMembers)
				{
					switch (member.MemberInfo)
					{
						case FieldInfo field:
						{
							var o = field.GetValue(value);
							var pm = proxyMembers.Single(x => x.Name == member.Name);
							switch (pm.MemberInfo)
							{
								case FieldInfo f:
								{
									f.SetValue(c, o);
									break;
								}
								case PropertyInfo p:
								{
									p.SetValue(c, o);
									break;
								}
							}
							break;
						}
						case PropertyInfo property:
						{
							var o = property.GetValue(value);
							var pm = proxyMembers.Single(x => x.Name == member.Name);
							switch (pm.MemberInfo)
							{
								case FieldInfo f:
								{
									f.SetValue(c, o);
									break;
								}
								case PropertyInfo p:
								{
									p.SetValue(c, o);
									break;
								}
							}
							break;
						}
					}
				}

				break;
			}
		}

		public IEnumerable<object> GetComponents(Entity entity)
		{
			foreach (var component in _componentsByEntity[entity])
				yield return component;
		}
	}
}