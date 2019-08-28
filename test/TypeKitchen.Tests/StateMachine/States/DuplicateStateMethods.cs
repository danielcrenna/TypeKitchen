// Copyright © Conatus Creative, Inc. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE.md in the project root for license terms.

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
