// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using TypeKitchen.StateMachine;

namespace TypeKitchen.Tests.StateMachine.States
{
    public class DuplicateStateMethods : StateMachine<object>
    {
        #region StateA

        public class StateA : State
        {
        }

        private void StateA_BeginState(object stateData, State previousState)
        {
        }

        private void State_StateA_BeginState(object stateData, State previousState)
        {
        }

        #endregion
    }
}
