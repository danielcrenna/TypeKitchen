using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace TypeKitchen
{
    public static class Pooling
    {
        public static class StringBuilderPool
        {
            private static readonly ObjectPool<StringBuilder> Pool =
                new LeakTrackingObjectPool<StringBuilder>(new DefaultObjectPool<StringBuilder>(
                    new StringBuilderPooledObjectPolicy()
                ));

            public static StringBuilder Get() => Pool.Get();
            public static void Return(StringBuilder obj) => Pool.Return(obj);

            public static string Scoped(Action<StringBuilder> closure)
            {
                var sb = Pool.Get();
                try
                {
                    closure(sb);
                    return sb.ToString();
                }
                finally
                {
                    Pool.Return(sb);
                }
            }

            public static string Scoped(Action<StringBuilder> closure, int startIndex, int length)
            {
                var sb = Pool.Get();
                try
                {
                    closure(sb);
                    return sb.ToString(startIndex, length);
                }
                finally
                {
                    Pool.Return(sb);
                }
            }
        }

        public static class ListPool<T>
        {
            private static readonly ObjectPool<List<T>> Pool =
                new LeakTrackingObjectPool<List<T>>(
                    new DefaultObjectPool<List<T>>(new ListObjectPolicy<List<T>>())
                );

            public static List<T> Get() => Pool.Get();

            public static void Return(List<T> obj) => Pool.Return(obj);
        }


        #region Policies

        /// <summary> The default policy provided by Microsoft uses new T() constraint, which silently defers to Activator.CreateInstance. </summary>
        private class DefaultObjectPolicy<T> : IPooledObjectPolicy<T>
        {
            internal static T CreateNew() { return Instancing.CreateInstance<T>(); }

            public T Create()
            {
                return CreateNew();
            }

            public bool Return(T obj)
            {
                return true;
            }
        }

        private class ListObjectPolicy<T> : IPooledObjectPolicy<T> where T : IList
        {
            public T Create()
            {
                return Instancing.CreateInstance<T>();
            }

            public bool Return(T obj)
            {
                obj.Clear();
                return obj.Count == 0;
            }
        }
        
        #endregion
    }
}
