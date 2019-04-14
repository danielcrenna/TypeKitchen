// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using TypeKitchen.Internal;

namespace TypeKitchen
{
    public delegate object CreateInstance(params object[] args);

    public static class Activation
    {
        public static CreateInstance ActivatorWeakTyped(ConstructorInfo ctor)
        {
            return args => Activator.CreateInstance(ctor.DeclaringType ?? throw new ArgumentNullException(), args);
        }

        public static CreateInstance InvokeWeakTyped(ConstructorInfo ctor)
        {
            return ctor.Invoke;
        }

        public static CreateInstance DynamicMethodWeakTyped(ConstructorInfo ctor)
        {
            var parameters = ctor.GetParameters();

            var dm = new DynamicMethod($"Construct_{ctor.MetadataToken}", ctor.DeclaringType, new[] { typeof(object[]) });
            var il = dm.GetILGeneratorInternal();

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                il.Ldarg_0();
                NextParameterIndex(i);
                il.Ldelem_Ref();
                if (parameter.ParameterType.IsValueType)
                    il.Unbox_Any(parameter.ParameterType);
                else
                    il.Castclass(parameter.ParameterType);
            }

            il.Newobj(ctor);
            il.Ret();
            return (CreateInstance)dm.CreateDelegate(typeof(CreateInstance));

            void NextParameterIndex(int i)
            {
                switch (i)
                {
                    case 0:
                        il.Ldc_I4_0();
                        break;
                    case 1:
                        il.Ldc_I4_1();
                        break;
                    case 2:
                        il.Ldc_I4_2();
                        break;
                    case 3:
                        il.Ldc_I4_3();
                        break;
                    case 4:
                        il.Ldc_I4_4();
                        break;
                    case 5:
                        il.Ldc_I4_5();
                        break;
                    case 6:
                        il.Ldc_I4_6();
                        break;
                    case 7:
                        il.Ldc_I4_7();
                        break;
                    case 8:
                        il.Ldc_I4_8();
                        break;
                    default:
                        il.Ldc_I4_S((byte) i);
                        break;
                }
            }
        }

        public static CreateInstance ExpressionWeakTyped(ConstructorInfo ctor)
        {
            var parameters = ctor.GetParameters();
            var argsParam = Expression.Parameter(typeof(object[]), "args");
            var arguments = new Expression[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
                arguments[i] = Expression.Convert(Expression.ArrayIndex(argsParam, Expression.Constant(i)), parameters[i].ParameterType);
            var lambda = Expression.Lambda(typeof(CreateInstance), Expression.New(ctor, arguments), argsParam);
            var compiled = (CreateInstance)lambda.Compile();
            return compiled;
        }
    }
}