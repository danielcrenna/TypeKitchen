// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using TypeKitchen.StateMachine;

namespace TypeKitchen.Tests.StateMachine.Fakes
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
