// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using TypeKitchen.StateMachine;

namespace TypeKitchen.Tests.StateMachine
{
    public class StateProviderFixture : IDisposable
    {
        private static readonly object Sync = new object();

        public StateProviderFixture()
        {
            Monitor.Enter(Sync);
            StateProvider.Clear();
        }

        public void Dispose()
        {
            StateProvider.Clear();
            Monitor.Exit(Sync);
        }
    }
}
