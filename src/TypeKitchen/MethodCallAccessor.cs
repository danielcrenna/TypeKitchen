// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace TypeKitchen
{
    public abstract class MethodCallAccessor : IMethodCallAccessor
    {
        public string MethodName { get; set; }
        public ParameterInfo[] Parameters { get; set; }

        public object Call(object target)
        {
            var args = Arguments.Get(0);
            try
            {
                return Call(target, args);
            }
            finally
            {
                Arguments.Return(args);
            }
        }

		public object Call(object target, object arg1)
		{
			var args = Arguments.Get(1);
			args[0] = arg1;
			try
			{
				return Call(target, args);
			}
			finally
			{
				Arguments.Return(args);
			}
		}

		public object Call(object target, object arg1, object arg2)
		{
			var args = Arguments.Get(2);
			args[0] = arg1;
			args[1] = arg2;
			try
			{
				return Call(target, args);
			}
			finally
			{
				Arguments.Return(args);
			}
		}

		public object Call(object target, object arg1, object arg2, object arg3)
		{
			var args = Arguments.Get(3);
			args[0] = arg1;
			args[1] = arg2;
			args[2] = arg3;
			try
			{
				return Call(target, args);
			}
			finally
			{
				Arguments.Return(args);
			}
		}

		public abstract object Call(object target, object[] args);


        #region Pooling 

        public ArgumentsPool Arguments = new ArgumentsPool();

        public class ArgumentsPool
        {
            private readonly ObjectWrapper[] _items;
            private object[] _firstItem;

            public ArgumentsPool() : this(Environment.ProcessorCount * 2)
            {
            }

            public ArgumentsPool(int maximumRetained)
            {
                _items = new ObjectWrapper[maximumRetained - 1];
            }

            public object[] Get(int length)
            {
                var comparator = _firstItem;
                if (comparator != null &&
                    Interlocked.CompareExchange(ref _firstItem, default, comparator) == comparator)
                    return comparator;
                var items = _items;
                for (var index = 0; index < items.Length; ++index)
                {
                    var item = items[index].Element;
                    if (item?.Length != length)
                        continue;
                    if (item != null && Interlocked.CompareExchange(ref items[index].Element, default, item) == item)
                        return item;
                }

                return new object[length];
            }

            public void Return(object[] obj)
            {
                if (_firstItem == null && Interlocked.CompareExchange(ref _firstItem, obj, default) == null)
                    return;
                var items = _items;
                var index = 0;
                while (index < items.Length &&
                       Interlocked.CompareExchange(ref items[index].Element, obj, default) != null)
                    ++index;
            }

            [DebuggerDisplay("{" + nameof(Element) + "}")]
            private struct ObjectWrapper
            {
                public object[] Element;
            }
        }

        #endregion
    }
}