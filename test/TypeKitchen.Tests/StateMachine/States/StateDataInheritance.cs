// Copyright © Conatus Creative, Inc. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE.md in the project root for license terms.

using TypeKitchen.StateMachine;

namespace TypeKitchen.Tests.StateMachine.States
{
    public class StateDataInheritance : StateMachine<BaseStateData>
    {
        private void State_StateA_BeginState(DerivedStateData stateData, State previousState)
        {
            stateData.A = false;
            stateData.B = true;
        }

        public class StateA : State
        {
        }
    }
}
