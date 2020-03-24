// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading;
using TypeKitchen.Reflection;

namespace TypeKitchen.StateMachine
{
	public class StateProvider
	{
		private const string StateDisambiguatorPrefix = "State" + Separator;
		private const string Separator = "_";

		private static List<State> _allStateInstances;
		private static Dictionary<Type, List<State>> _allStatesByType;

		public static ReadOnlyCollection<State> AllStateInstances
		{
			get
			{
				Debug.Assert(_allStateInstances != null);
				return _allStateInstances.AsReadOnly();
			}
		}

		public static void Clear()
		{
			if (_allStateInstances != null)
				foreach (var state in _allStateInstances)
				{
					var stateInstanceLookupType = typeof(StateInstanceLookup<>).MakeGenericType(state.GetType());
					const BindingFlags bindingFlags =
						BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
					var clearMethod =
						stateInstanceLookupType.GetMethod(nameof(StateInstanceLookup<State>.Clear), bindingFlags);
					clearMethod?.Invoke(null, null);
				}

			_allStatesByType?.Clear();
			_allStatesByType = null;

			_allStateInstances?.Clear();
			_allStateInstances = null;
		}

		public interface INamedState
		{
			string Name { get; }
		}

		public class MethodTable
		{
		}

		public class State
		{
			public MethodTable methodTable;
		}

		#region Lookup Helpers

		private static class StateInstanceLookup<TState> where TState : State, new()
		{
			internal static readonly Dictionary<Type, TState> ForType = new Dictionary<Type, TState>();

			// ReSharper disable once UnusedMember.Local
			internal static void Add(Type stateMachineType, TState state)
			{
				ForType.Add(stateMachineType, state);
			}

			// ReSharper disable once UnusedMember.Local
			// ReSharper disable once MemberHidesStaticFromOuterClass
			internal static void Clear()
			{
				ForType.Clear();
			}
		}

		public TState GetState<TState>() where TState : State, new()
		{
			var type = GetType();

			return StateInstanceLookup<TState>.ForType[type];
		}

		public static TState GetState<TType, TState>()
			where TType : StateProvider
			where TState : State, new()
		{
			return StateInstanceLookup<TState>.ForType[typeof(TType)];
		}

		public static ReadOnlyCollection<State> GetAllStatesByType(Type type)
		{
			return _allStatesByType[type].AsReadOnly();
		}

		public static ReadOnlyCollection<State> GetAllStatesByType<T>()
		{
			return GetAllStatesByType(typeof(T));
		}

		public State GetStateByName(string symbol)
		{
			var type = GetType();

			foreach (var state in _allStatesByType[type])
			{
				// ReSharper disable once SuspiciousTypeConversion.Global
				if (!(state is INamedState queryable)) continue;

				if (queryable.Name == symbol) return state;
			}

			return null;
		}

		#endregion

		#region Setup

		/// <summary>Initialize all state machines. </summary>
		public static void Setup()
		{
			Setup((IEnumerable<Assembly>) AppDomain.CurrentDomain.GetAssemblies());
		}

		/// <summary>Initialize all state machines. </summary>
		public static void Setup(params Assembly[] assemblies)
		{
			Setup((IEnumerable<Assembly>) assemblies);
		}

		/// <summary>Initialize all state machines. </summary>
		public static void Setup(IEnumerable<Assembly> assemblies)
		{
			var types = assemblies.SelectMany(a => a.GetTypes());

			Setup(types);
		}

		/// <summary>Initialize all state machines. </summary>
		public static void Setup(params Type[] types)
		{
			Setup((IEnumerable<Type>) types);
		}

		/// <summary>Initialize all state machines. </summary>
		public static void Setup<T>()
		{
			Setup(typeof(T));
		}

		/// <summary>Initialize all state machines. </summary>
		public static void Setup(IEnumerable<Type> types)
		{
			Initialize();

			var stateMachinesToStates = new Dictionary<Type, Dictionary<Type, State>>();
			var stateMachinesToAbstractStates = new Dictionary<Type, Dictionary<Type, MethodTable>>();

			foreach (var type in types.Where(t => typeof(StateProvider).IsAssignableFrom(t)))
				SetupStateMachineTypeRecursive(stateMachinesToStates, stateMachinesToAbstractStates, type);

			_allStatesByType = new Dictionary<Type, List<State>>();
			foreach (var stateMachineAndStates in stateMachinesToStates.StableOrder(kvp => kvp.Key.ToString()))
			foreach (var state in stateMachineAndStates.Value.StableOrder(kvp => kvp.Key.ToString()))
			{
				_allStateInstances.Add(state.Value);

				if (!_allStatesByType.TryGetValue(stateMachineAndStates.Key, out var states))
				{
					states = new List<State>();
					_allStatesByType.Add(stateMachineAndStates.Key, states);
				}

				_allStatesByType[stateMachineAndStates.Key].Add(state.Value);

				var stateInstanceLookupType = typeof(StateInstanceLookup<>).MakeGenericType(state.Key);

				const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
				stateInstanceLookupType.GetMethod(nameof(StateInstanceLookup<State>.Add), bindingFlags)?
					.Invoke(null, new object[] {stateMachineAndStates.Key, state.Value});
			}
		}

		private static void Initialize()
		{
			if (Interlocked.CompareExchange(ref _allStateInstances, new List<State>(), null) != null)
				throw new AlreadyInitializedException();
		}

		private static string GetStateName(MemberInfo stateType)
		{
			var stateName = stateType.Name;
			if (stateName != "State" && stateName.EndsWith("State"))
				stateName = stateName.Substring(0, stateName.Length - "State".Length);

			return stateName;
		}

		private static void SetupStateMachineTypeRecursive(
			IDictionary<Type, Dictionary<Type, State>> stateMachinesToStates,
			IDictionary<Type, Dictionary<Type, MethodTable>> stateMachinesToAbstractStates,
			Type stateMachineType)
		{
			Debug.Assert(typeof(StateProvider).IsAssignableFrom(stateMachineType));

			if (stateMachinesToStates.ContainsKey(stateMachineType)) return;

			Dictionary<Type, State> baseStates = null;
			Dictionary<Type, MethodTable> baseAbstractStates = null;
			if (stateMachineType != typeof(StateProvider))
			{
				SetupStateMachineTypeRecursive(stateMachinesToStates, stateMachinesToAbstractStates,
					stateMachineType.BaseType);
				baseStates =
					stateMachinesToStates[
						stateMachineType.BaseType ??
						throw new InvalidOperationException($"{nameof(MethodTable)} base type not found")];
				baseAbstractStates = stateMachinesToAbstractStates[stateMachineType.BaseType];
			}

			const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
			                                  BindingFlags.DeclaredOnly;

			var typeMethods = stateMachineType.GetMethods(bindingFlags);
			var stateMethods = typeMethods.ToDictionary(mi => mi.Name);

			var newStateTypes = stateMachineType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
				.Where(nt => typeof(State).IsAssignableFrom(nt)).ToArray();
			foreach (var newStateType in newStateTypes)
			{
				var newStateTypeMethods = newStateType.GetMethods(bindingFlags);
				foreach (var newStateTypeMethod in newStateTypeMethods)
				{
					var methodName = GetFullyQualifiedStateMethodName(newStateTypeMethod);

					if (stateMethods.ContainsKey(methodName))
					{
						var duplicateMethods = new List<string> {methodName};

						if (methodName.StartsWith(StateDisambiguatorPrefix))
						{
							var naturalName = methodName.Replace(StateDisambiguatorPrefix, string.Empty);

							if (stateMethods.ContainsKey(naturalName)) duplicateMethods.Add(naturalName);
						}

						throw new DuplicateStateMethodException(duplicateMethods.ToArray());
					}

					stateMethods.Add(methodName, newStateTypeMethod);
				}
			}

			Type methodTableType;
			var methodTableSearchType = stateMachineType;
			while ((methodTableType =
				       methodTableSearchType?.GetNestedType(nameof(MethodTable),
					       BindingFlags.Public | BindingFlags.NonPublic)) == null)
			{
				if (!typeof(StateProvider).IsAssignableFrom(methodTableSearchType?.BaseType)) break;

				methodTableSearchType = methodTableSearchType?.BaseType;
			}

			if (methodTableType == null)
				throw new InvalidOperationException($"{nameof(MethodTable)} not found for {stateMachineType}");

			if (!typeof(MethodTable).IsAssignableFrom(methodTableType))
				throw new InvalidOperationException(
					$"{nameof(MethodTable)} must be derived from StateMachine.MethodTable");

			var states = new Dictionary<Type, State>();
			var abstractStates = new Dictionary<Type, MethodTable>();

			Debug.Assert(baseStates != null == (baseAbstractStates != null));
			if (baseStates != null)
			{
				foreach (var baseState in baseStates)
				{
					var state = (State) Activator.CreateInstance(baseState.Key);
					state.methodTable =
						ShallowCloneToDerived(baseState.Value.methodTable, methodTableType, stateMachineType);
					FillMethodTableWithOverrides(baseState.Key, state.methodTable, stateMachineType, stateMethods);
					states.Add(baseState.Key, state);
				}

				foreach (var baseAbstractState in baseAbstractStates)
				{
					var methodTable = ShallowCloneToDerived(baseAbstractState.Value, methodTableType, stateMachineType);
					FillMethodTableWithOverrides(baseAbstractState.Key, methodTable, stateMachineType, stateMethods);
					abstractStates.Add(baseAbstractState.Key, methodTable);
				}
			}

			foreach (var stateType in newStateTypes)
				SetupStateTypeRecursive(states, abstractStates, stateType, stateMachineType, methodTableType,
					stateMethods);

			var stateTypesToMethodTables = states
				.Select(kvp => new KeyValuePair<Type, MethodTable>(kvp.Key, kvp.Value.methodTable))
				.Concat(abstractStates).ToList();

			foreach (var typeToMethodTable in stateTypesToMethodTables)
			{
				var methodTable = typeToMethodTable.Value;
				var allMethodTableEntries = methodTable.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
					.Where(fi => fi.FieldType.BaseType == typeof(MulticastDelegate)).ToList();

				if (!allMethodTableEntries.Any()) stateMethods.Clear();

				var toRemove = new List<string>(stateMethods.Keys);
				foreach (var fieldInfo in allMethodTableEntries)
				foreach (var stateMethod in stateMethods)
				{
					var ignore = stateMethod.Value.GetCustomAttributes(typeof(IgnoreStateMethodAttribute), false);
					if (ignore.Length > 0) continue;

					var aliasName = $@"{fieldInfo.Name}";
					var disambiguatedName = $@"{StateDisambiguatorPrefix}_\w*_{fieldInfo.Name}";
					var naturalName = $@"\w*_{fieldInfo.Name}";

					if (Regex.IsMatch(stateMethod.Key, disambiguatedName) ||
					    Regex.IsMatch(stateMethod.Key, naturalName) ||
					    stateMethod.Key == aliasName)
						toRemove.Remove(stateMethod.Key);
				}

				foreach (var stateMethod in toRemove) stateMethods.Remove(stateMethod);
			}

			if (stateMethods.Count > 0) throw new UnusedStateMethodsException(stateMethods.Values);

			foreach (var typeToMethodTable in stateTypesToMethodTables)
			{
				var methodTable = typeToMethodTable.Value;
				var allMethodTableEntries = methodTable.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
					.Where(fi => fi.FieldType.BaseType == typeof(MulticastDelegate));

				foreach (var fieldInfo in allMethodTableEntries)
				{
					if (fieldInfo.GetCustomAttributes(typeof(AlwaysNullCheckedAttribute), true).Length != 0) continue;

					if (fieldInfo.GetValue(methodTable) == null)
					{
						var methodInMethodTable = fieldInfo.FieldType.GetMethod("Invoke");
						Debug.Assert(methodInMethodTable != null, nameof(methodInMethodTable) + " != null");

						var dynamicMethod = new DynamicMethod(
							$"DoNothing{Separator}{GetStateName(typeToMethodTable.Key)}{Separator}{fieldInfo.Name}",
							methodInMethodTable.ReturnType,
							methodInMethodTable.GetParameters().Select(pi => pi.ParameterType).ToArray(),
							stateMachineType);

						var il = dynamicMethod.GetILGenerator();
						EmitDefault(il, methodInMethodTable.ReturnType);
						il.Emit(OpCodes.Ret);

						fieldInfo.SetValue(methodTable, dynamicMethod.CreateDelegate(fieldInfo.FieldType));
					}
				}
			}

			stateMachinesToStates.Add(stateMachineType, states);
			stateMachinesToAbstractStates.Add(stateMachineType, abstractStates);
		}

		private static string GetFullyQualifiedStateMethodName(MemberInfo newStateTypeMethod)
		{
			var methodName = newStateTypeMethod.Name;

			var stateTypeName = newStateTypeMethod.DeclaringType?.Name;

			if (methodName.StartsWith($"{stateTypeName}{Separator}"))
				methodName = $"{StateDisambiguatorPrefix}{methodName}";
			else if (!methodName.StartsWith($"{StateDisambiguatorPrefix}{stateTypeName}{Separator}"))
				methodName =
					$"{StateDisambiguatorPrefix}{newStateTypeMethod?.DeclaringType?.Name}{Separator}{newStateTypeMethod.Name}";

			return methodName;
		}

		private static void EmitDefault(ILGenerator il, Type type)
		{
			if (type == typeof(void)) return;

			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean:
				case TypeCode.Char:
				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
					il.Emit(OpCodes.Ldc_I4_0);
					break;

				case TypeCode.Int64:
				case TypeCode.UInt64:
					il.Emit(OpCodes.Ldc_I4_0);
					il.Emit(OpCodes.Conv_I8);
					break;

				case TypeCode.Single:
					il.Emit(OpCodes.Ldc_R4, default(float));
					break;

				case TypeCode.Double:
					il.Emit(OpCodes.Ldc_R8, default(double));
					break;

				default:
					if (type.IsValueType)
					{
						var lb = il.DeclareLocal(type);
						il.Emit(OpCodes.Ldloca, lb);
						il.Emit(OpCodes.Initobj, type);
						il.Emit(OpCodes.Ldloc, lb);
					}
					else
						il.Emit(OpCodes.Ldnull);

					break;
			}
		}

		private static void SetupStateTypeRecursive(IDictionary<Type, State> states,
			IDictionary<Type, MethodTable> abstractStates,
			Type stateType, Type stateMachineType, Type methodTableType, Dictionary<string, MethodInfo> stateMethods)
		{
			if (states.ContainsKey(stateType) || abstractStates.ContainsKey(stateType)) return;

			if (stateType == typeof(State) && stateMachineType == typeof(StateProvider))
			{
				Debug.Assert(!stateType.IsAbstract);
				states.Add(stateType, new State {methodTable = new MethodTable()});
				return;
			}

			Debug.Assert(stateType != typeof(State));

			SetupStateTypeRecursive(states, abstractStates, stateType.BaseType, stateMachineType, methodTableType,
				stateMethods);

			Debug.Assert(stateType.BaseType != null, "stateType.BaseType != null");

			var parentMethodTable = stateType.BaseType.IsAbstract
				? abstractStates[stateType.BaseType]
				: states[stateType.BaseType].methodTable;
			var methodTable = ShallowCloneToDerived(parentMethodTable, methodTableType, stateMachineType);
			FillMethodTableWithOverrides(stateType, methodTable, stateMachineType, stateMethods);

			if (stateType.IsAbstract)
				abstractStates.Add(stateType, methodTable);
			else
			{
				var state = (State) Activator.CreateInstance(stateType);
				state.methodTable = methodTable;
				states.Add(stateType, state);
			}
		}

		private static MethodTable ShallowCloneToDerived(MethodTable state, Type derivedType, Type stateMachineType)
		{
			if (derivedType.IsGenericType)
			{
				var typeSource = stateMachineType.IsGenericType ? stateMachineType : state.GetType();
				derivedType = derivedType.MakeGenericType(typeSource.GetGenericArguments()[0]);
			}

			var baseType = state.GetType();
			if (!baseType.IsAssignableFrom(derivedType))
				throw new Exception("Method table inheritance hierarchy error.");

			if (derivedType.ContainsGenericParameters) return state;

			var derivedMethodTable = (MethodTable) Activator.CreateInstance(derivedType);
			foreach (var field in baseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
			                                         BindingFlags.Instance))
				field.SetValue(derivedMethodTable, field.GetValue(state));

			return derivedMethodTable;
		}

		private static void FillMethodTableWithOverrides(MemberInfo stateType, MethodTable methodTable,
			Type stateMachineType, Dictionary<string, MethodInfo> stateMethods)
		{
			var allMethodTableEntries = methodTable.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
				.Where(fi => fi.FieldType.BaseType == typeof(MulticastDelegate));

			foreach (var fieldInfo in allMethodTableEntries)
			{
				var naturalName = $"{GetStateName(stateType)}{Separator}{fieldInfo.Name}";
				var disambiguatedName = $"{StateDisambiguatorPrefix}{naturalName}";

				if (stateMethods.TryGetValue(naturalName, out var methodInStateMachine))
				{
					if (stateMethods.ContainsKey(disambiguatedName))
						throw new DuplicateStateMethodException(naturalName, disambiguatedName);

					PotentialMethodNameMatch(methodTable, stateMachineType, stateMethods, fieldInfo,
						methodInStateMachine, naturalName);
				}

				if (stateMethods.TryGetValue(disambiguatedName, out methodInStateMachine))
				{
					if (stateMethods.ContainsKey(naturalName))
						throw new DuplicateStateMethodException(disambiguatedName, naturalName);

					PotentialMethodNameMatch(methodTable, stateMachineType, stateMethods, fieldInfo,
						methodInStateMachine, disambiguatedName);
				}
			}
		}

		private static void PotentialMethodNameMatch(MethodTable methodTable, Type stateMachineType,
			IDictionary<string, MethodInfo> stateMethods,
			FieldInfo fieldInfo, MethodInfo methodInStateMachine, string potentialMethodName)
		{
			var methodInMethodTable = fieldInfo.FieldType.GetMethod("Invoke");
			Debug.Assert(methodInMethodTable != null, nameof(methodInMethodTable) + " != null");

			if (methodInStateMachine.ReturnType != methodInMethodTable.ReturnType)
				ThrowMethodMismatch(methodInStateMachine, methodInMethodTable);

			var methodInMethodTableParameters = methodInMethodTable.GetParameters();
			var methodInStateMachineParameters = methodInStateMachine.GetParameters();

			if (methodInStateMachineParameters.Length != methodInMethodTableParameters.Length - 1
			) // -1 to account for 'this' parameter to open delegate
				ThrowMethodMismatch(methodInStateMachine, methodInMethodTable);

			for (var i = 0; i < methodInStateMachineParameters.Length; i++)
				if (methodInStateMachineParameters[i].ParameterType !=
				    methodInMethodTableParameters[i + 1]
					    .ParameterType && // +1 to account for 'this' parameter to open delegate     
				    !methodInMethodTableParameters[i + 1].ParameterType
					    .IsAssignableFrom(methodInStateMachineParameters[i].ParameterType)
				) // i.e. supports custom implementations of TStateData
					ThrowMethodMismatch(methodInStateMachine, methodInMethodTable);

			if (!stateMachineType.IsAssignableFrom(methodInMethodTableParameters[0].ParameterType))
			{
				Debug.Assert(methodInMethodTableParameters[0].ParameterType.IsAssignableFrom(stateMachineType));
				var dynamicMethod = new DynamicMethod($"CastingShim{Separator}{potentialMethodName}",
					methodInMethodTable.ReturnType,
					methodInMethodTableParameters.Select(pi => pi.ParameterType).ToArray(),
					stateMachineType);
				var il = dynamicMethod.GetILGenerator();
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Castclass, stateMachineType); // <- the casting bit of the shim
				if (methodInMethodTableParameters.Length > 1) il.Emit(OpCodes.Ldarg_1);

				if (methodInMethodTableParameters.Length > 2) il.Emit(OpCodes.Ldarg_2);

				if (methodInMethodTableParameters.Length > 3) il.Emit(OpCodes.Ldarg_3);

				for (var i = 4; i < methodInMethodTableParameters.Length; i++)
					if (i <= byte.MaxValue)
						il.Emit(OpCodes.Ldarg_S, (byte) i);
					else
						il.Emit(OpCodes.Ldarg, (ushort) i);

				il.Emit(OpCodes.Callvirt, methodInStateMachine);
				il.Emit(OpCodes.Ret);

				fieldInfo.SetValue(methodTable, dynamicMethod.CreateDelegate(fieldInfo.FieldType));
			}
			else
				fieldInfo.SetValue(methodTable, Delegate.CreateDelegate(fieldInfo.FieldType, methodInStateMachine));

			stateMethods.Remove(potentialMethodName);
		}

		private static void ThrowMethodMismatch(MethodInfo methodInStateMachine, MethodInfo methodInMethodTable)
		{
			throw new Exception(
				$"Method signature does not match: \"{methodInStateMachine}\" cannot be used for \"{methodInMethodTable}\"");
		}

		#endregion
	}
}