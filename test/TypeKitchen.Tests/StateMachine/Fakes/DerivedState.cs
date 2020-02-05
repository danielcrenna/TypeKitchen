// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using TypeKitchen.StateMachine;

namespace TypeKitchen.Tests.StateMachine.Fakes
{
    public class DerivedState : StateProvider.State
    {
        public virtual int Sprockets => 10;
        public virtual int Widgets => 5;
    }
}
