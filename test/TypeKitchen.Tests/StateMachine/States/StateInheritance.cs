// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
