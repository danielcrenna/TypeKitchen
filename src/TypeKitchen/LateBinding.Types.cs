// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeKitchen
{
    public static partial class LateBinding
    {
        #region CallSite 

        public static Dictionary<string, Func<object, object>> CallSiteBindGet(AccessorMembers members)
        {
            return members.Members.Where(member => member.CanRead)
                .ToDictionary(member => member.Name, CallSiteBindGet);
        }

        public static Dictionary<string, Action<object, object>> CallSiteBindSet(AccessorMembers members)
        {
            return members.Members.Where(member => member.CanWrite)
                .ToDictionary(member => member.Name, CallSiteBindSet);
        }

        #endregion

        #region DynamicMethod

        public static Dictionary<string, Func<object, object>> DynamicMethodBindGet(AccessorMembers members)
        {
            return members.Members.Where(member => member.CanRead)
                .ToDictionary(member => member.Name, DynamicMethodBindGet);
        }

        public static Dictionary<string, Action<object, object>> DynamicMethodBindSet(AccessorMembers members)
        {
            return members.Members.Where(member => member.CanWrite)
                .ToDictionary(member => member.Name, DynamicMethodBindSet);
        }

        #endregion

        #region Open Delegates

        public static Dictionary<string, Func<object, object>> OpenDelegateBindGet<TTarget>(AccessorMembers members)
            where TTarget : class
        {
            return members.Members.Where(member => member.CanRead)
                .ToDictionary(member => member.Name, OpenDelegateBindGet<TTarget>);
        }

        public static Dictionary<string, Action<object, object>> OpenDelegateBindSet<TTarget>(AccessorMembers members)
            where TTarget : class
        {
            return members.Members.Where(member => member.CanWrite)
                .ToDictionary(member => member.Name, OpenDelegateBindSet<TTarget>);
        }

        #endregion

        #region Expression

        public static Dictionary<string, Func<object, object>> ExpressionBindGet(AccessorMembers members)
        {
            return members.Members.Where(member => member.CanRead)
                .ToDictionary(member => member.Name, ExpressionBindGet);
        }

        public static Dictionary<string, Action<object, object>> ExpressionBindSet(AccessorMembers members)
        {
            return members.Members.Where(member => member.CanWrite)
                .ToDictionary(member => member.Name, ExpressionBindSet);
        }

        #endregion

        #region Method Invoke

        public static Dictionary<string, Func<object, object>> MethodInvokeBindGet(AccessorMembers members)
        {
            return members.Members.Where(member => member.CanRead)
                .ToDictionary(member => member.Name, MethodInvokeBindGet);
        }

        public static Dictionary<string, Action<object, object>> MethodInvokeBindSet(AccessorMembers members)
        {
            return members.Members.Where(member => member.CanRead)
                .ToDictionary(member => member.Name, MethodInvokeBindSet);
        }

        #endregion
    }
}