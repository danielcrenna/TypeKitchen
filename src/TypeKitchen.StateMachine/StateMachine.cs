// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace TypeKitchen.StateMachine
{
	[DebuggerDisplay("{" + nameof(DisplayName) + "}")]
	public class StateMachine<TStateData> : StateProvider where TStateData : class
	{
		public StateMachine() => CurrentState = GetState<State>();

		public string DisplayName =>
			$"{GetType().Name} ({(CurrentState != null ? CurrentState.GetType().Name : "(null)")})";

		public MethodTable StateMethods => (MethodTable) CurrentState.methodTable;
		public State CurrentState { get; private set; }

		public void SetState<TState>(TStateData stateData = null, bool allowStateRestart = false)
			where TState : State, new()
		{
			DirectlySetState(GetState<TState>(), stateData, allowStateRestart);
		}

		[IgnoreStateMethod]
		public virtual void Update(TStateData stateData)
		{
			StateMethods.Update?.Invoke(this, stateData);
		}

		private void DirectlySetState(State nextState, TStateData stateData, bool allowStateRestart)
		{
			if (!allowStateRestart && ReferenceEquals(CurrentState, nextState)) return;

			{
				if (CurrentState?.methodTable is MethodTable methodTable)
				{
					var beforeEnd = methodTable.BeforeEndState?.Invoke(this, stateData, CurrentState);
					if (beforeEnd.HasValue && !beforeEnd.Value) return;
				}
			}

			{
				if (nextState?.methodTable is MethodTable methodTable)
				{
					var beforeBegin = methodTable.BeforeBeginState?.Invoke(this, stateData, nextState);
					if (beforeBegin.HasValue && !beforeBegin.Value) return;
				}
			}

			StateMethods.EndState?.Invoke(this, stateData, nextState);
			var previousState = CurrentState;

			CurrentState = nextState;
			StateMethods.BeginState?.Invoke(this, stateData, previousState);
		}

		public new class MethodTable : StateProvider.MethodTable
		{
			[AlwaysNullChecked] public Func<StateMachine<TStateData>, TStateData, State, bool> BeforeBeginState;

			[AlwaysNullChecked] public Func<StateMachine<TStateData>, TStateData, State, bool> BeforeEndState;

			[AlwaysNullChecked] public Action<StateMachine<TStateData>, TStateData, State> BeginState;

			[AlwaysNullChecked] public Action<StateMachine<TStateData>, TStateData, State> EndState;

			[AlwaysNullChecked] public Action<StateMachine<TStateData>, TStateData> Update;
		}
	}
}