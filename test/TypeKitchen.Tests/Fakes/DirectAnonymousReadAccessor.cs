// Copyright (c) Blowdart, Inc. All rights reserved.

using System;
using System.Reflection.Emit;

namespace TypeKitchen.Tests.Fakes
{
    public sealed class DirectAnonymousReadAccessor : ITypeReadAccessor
    {
        public static DirectAnonymousReadAccessor Instance = new DirectAnonymousReadAccessor();

        // func is still necessary because real anonymous types don't allow ldfld or callvirt getgetmethod (private, in another assembly)
        public static Func<object, object> Foo;
        public static Func<object, object> Bar;

        private DirectAnonymousReadAccessor()
        {
            Foo = o => ((TwoProperties) o).Foo;
            Bar = o => ((TwoProperties) o).Bar;
        }

        public bool TryGetValue(object target, string key, out object value)
        {
            switch (key)
            {
                case "Foo":
                    value = Foo(target);
                    return true;
                case "Bar":
                    value = Bar(target);
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public object this[object target, string key]
        {
            get
            {
                switch (key)
                {
                    case "Foo":
                        return Foo(target);
                    case "Bar":
                        return Bar(target);
                    default:
                        throw new ArgumentNullException();
                }
            }
        }

        public Func<object, string, object> SimulateIndirectAccess()
        {
            return (target, key) => this[target, key];
        }

        public Func<object, object> SimulateNonBranchingIndirectAccess(string fieldName)
        {
            var dm = new DynamicMethod("_", typeof(object), new[] {typeof(object)});
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldsfld, GetType().GetField(fieldName));
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, GetType().GetField(fieldName).FieldType.GetMethod("Invoke"));
            il.Emit(OpCodes.Ret);
            var @delegate = (Func<object, object>) dm.CreateDelegate(typeof(Func<object, object>));
            return @delegate;
        }
    }
}