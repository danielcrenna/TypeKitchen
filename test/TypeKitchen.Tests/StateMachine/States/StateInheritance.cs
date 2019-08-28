// Copyright © Conatus Creative, Inc. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE.md in the project root for license terms.

using System;
using TypeKitchen.StateMachine;

namespace TypeKitchen.Tests.StateMachine.States
{
    public class StateInheritance : StateMachine<object>
    {
        private void State_StateA_BeginState(object stateData, State previousState)
        {
            var current = (DerivedState) CurrentState;
            if (current.Widgets != 10 || current.Sprockets != 5)
            {
                throw new Exception("widgets had unexpected value");
            }
        }

        public class StateA : DerivedState
        {
            public override int Sprockets => 5;
            public override int Widgets => 10;
        }
    }
}
