// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using TypeKitchen.StateMachine;
using TypeKitchen.Tests.StateMachine.Fakes;
using Xunit;

namespace TypeKitchen.Tests.StateMachine
{
    public class StateMachineTests
    {
        [Fact]
        public void BeforeBegin_failure_blocks_state_transition()
        {
            using (new StateProviderFixture())
            {
                StateProvider.Setup<ThreeValidStaticStates>();

                var data = new ThreeValidStatesData {AllowBeginStateC = false};
                var actor = new ThreeValidStaticStates();

                Assert.Equal(typeof(StateProvider.State), actor.CurrentState.GetType());
                actor.SetState<ThreeValidStaticStates.StateC>(data);
                Assert.Equal(typeof(StateProvider.State), actor.CurrentState.GetType());
            }
        }

        [Fact]
        public void BeforeEnd_failure_blocks_state_transition()
        {
            using (new StateProviderFixture())
            {
                StateProvider.Setup<ThreeValidStaticStates>();

                var data = new ThreeValidStatesData {AllowEndStateC = false};
                var actor = new ThreeValidStaticStates();

                Assert.Equal(typeof(StateProvider.State), actor.CurrentState.GetType());
                actor.SetState<ThreeValidStaticStates.StateC>(data);
                Assert.Equal(typeof(ThreeValidStaticStates.StateC), actor.CurrentState.GetType());
                actor.SetState<ThreeValidStaticStates.StateB>(data);
                Assert.Equal(typeof(ThreeValidStaticStates.StateC), actor.CurrentState.GetType());
            }
        }

        [Fact]
        public void Can_inherit_state_data()
        {
            using (new StateProviderFixture())
            {
                StateProvider.Setup<StateDataInheritance>();

                var data = new BaseStateData {A = true};
                var actor = new StateDataInheritance();
                actor.SetState<StateDataInheritance.StateA>(data);
                Assert.False(data.A);
            }
        }

        [Fact]
        public void Can_inherit_states_and_pass_null_context()
        {
            using (new StateProviderFixture())
            {
                StateProvider.Setup<StateInheritance>();
                var actor = new StateInheritance();
                actor.SetState<StateInheritance.StateA>();
            }
        }

        [Fact]
        public void Can_transition_between_states()
        {
            using (new StateProviderFixture())
            {
                StateProvider.Setup<ThreeValidStaticStates>();

                var data = new ThreeValidStatesData();
                var actor = new ThreeValidStaticStates();

                actor.SetState<ThreeValidStaticStates.StateA>(data);
                actor.SetState<ThreeValidStaticStates.StateB>(data);
                actor.SetState<ThreeValidStaticStates.StateC>(data);

                Assert.True(data.BeginStateA);
                Assert.True(data.EndStateA);
                Assert.True(data.BeginStateB);
                Assert.True(data.EndStateB);

                // last state was C
                Assert.Equal(typeof(ThreeValidStaticStates.StateC), actor.CurrentState.GetType());
                Assert.Equal("ThreeValidStaticStates (StateC)", actor.DisplayName);

                // all states A and C have names
                Assert.NotNull(actor.GetState<ThreeValidStaticStates.StateA>().Name);
                Assert.NotNull(actor.GetState<ThreeValidStaticStates.StateC>().Name);

                // can lookup state by name
                Assert.Equal(typeof(ThreeValidStaticStates.StateA), actor.GetStateByName("StateA").GetType());

                // can enumerate all states
                var allStates = StateProvider.GetAllStatesByType<ThreeValidStaticStates>();
                Assert.Equal(4, allStates.Count);
            }
        }

        [Fact]
        public void Non_reentrant_state_does_nothing_when_reentering()
        {
            using (new StateProviderFixture())
            {
                StateProvider.Setup<ThreeValidStaticStates>();

                var data = new ThreeValidStatesData();
                var actor = new ThreeValidStaticStates();

                actor.SetState<ThreeValidStaticStates.StateA>(data);
                actor.SetState<ThreeValidStaticStates.StateA>(data);

                Assert.True(data.BeginStateA);
                Assert.False(data.EndStateA);
            }
        }

        [Fact]
        public void Reentrant_state_ends_itself()
        {
            using (new StateProviderFixture())
            {
                StateProvider.Setup<ThreeValidStaticStates>();

                var data = new ThreeValidStatesData();
                var actor = new ThreeValidStaticStates();

                actor.SetState<ThreeValidStaticStates.StateA>(data);
                actor.SetState<ThreeValidStaticStates.StateA>(data, true);

                Assert.True(data.BeginStateA);
                Assert.True(data.EndStateA);
            }
        }

        [Fact]
        public void Update_respects_current_state_and_is_optional()
        {
            using (new StateProviderFixture())
            {
                StateProvider.Setup<ThreeValidStaticStates>();

                var data = new ThreeValidStatesData();
                var actor = new ThreeValidStaticStates();

                Assert.Equal(0, data.TicksA);
                Assert.Equal(0, data.TicksB);

                actor.SetState<ThreeValidStaticStates.StateA>(data);
                actor.Update(data);
                actor.SetState<ThreeValidStaticStates.StateB>(data);
                actor.Update(data);
                actor.SetState<ThreeValidStaticStates.StateC>(data);
                actor.Update(data);

                Assert.Equal(1, data.TicksA);
                Assert.Equal(1, data.TicksB);
            }
        }
    }
}
