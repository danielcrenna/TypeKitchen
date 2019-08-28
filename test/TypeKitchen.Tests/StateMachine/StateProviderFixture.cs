// Copyright © Conatus Creative, Inc. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE.md in the project root for license terms.

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
