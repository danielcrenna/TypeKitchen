// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using TypeKitchen.StateMachine;

namespace TypeKitchen.Tests.StateMachine.States
{
    public class ThreeValidStaticStates : StateMachine<ThreeValidStatesData>
    {
        #region StateA

        public class StateA : State, INamedState
        {
            public string Name => "StateA";

            private void State_StateA_BeginState(ThreeValidStatesData stateData, State previousState)
            {
                stateData.BeginStateA = true;
            }

            private void State_StateA_EndState(ThreeValidStatesData stateData, State nextState)
            {
                stateData.EndStateA = true;
            }

            private void State_StateA_Update(ThreeValidStatesData stateData)
            {
                stateData.TicksA++;
            }
        }

        #endregion

        #region StateB

        public class StateB : State
        {
            private void StateB_BeginState(ThreeValidStatesData stateData, State previousState)
            {
                stateData.BeginStateB = true;
            }

            private void StateB_EndState(ThreeValidStatesData stateData, State nextState)
            {
                stateData.EndStateB = true;
            }

            private void State_StateB_Update(ThreeValidStatesData stateData)
            {
                stateData.TicksB++;
            }
        }

        #endregion

        #region StateC

        public class StateC : State, INamedState
        {
            public string Name => "StateC";

            private bool State_StateC_BeforeBeginState(ThreeValidStatesData stateData, State nextState)
            {
                return stateData.AllowBeginStateC;
            }

            private void BeginState(ThreeValidStatesData stateData, State previousState)
            {
            }

            private bool State_StateC_BeforeEndState(ThreeValidStatesData stateData, State previousState)
            {
                return stateData.AllowEndStateC;
            }

            private void EndState(ThreeValidStatesData stateData, State nextState)
            {
            }
        }

        #endregion
    }
}
