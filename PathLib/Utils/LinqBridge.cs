// Derived from:
// Enumerable.cs
//
// Authors:
//  Marek Safar (marek.safar@gmail.com)
//  Antonello Provenzano  <antonello@deveel.com>
//  Alejandro Serrano "Serras" (trupill@yahoo.es)
//  Jb Evain (jbevain@novell.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;

#pragma warning disable 1591

namespace PathLib.Utils
{
    /// <summary>
    /// See https://github.com/mono/mono/blob/master/mcs/class/System.Core/System.Linq/Enumerable.cs
    /// </summary>
    [DebuggerStepThrough]
    internal static class LinqBridge
    {
        private static class EmptyOf<T>
        {
            private static readonly T[] PValue = new T[0];
                        
            public static T[] Arr {get { return PValue; }}
        }

        public delegate TResult Func<in T, out TResult>(T elem);

        public delegate TResult Func<in T1, in T2, out TResult>(T1 elem1, T2 elem2);

        public static IEnumerable<T> AsEnumerable<T>(T[] arr)
        {
            foreach (var s in arr)
            {
                yield return s;
            }
        } 

        public static IEnumerable<TResult> Select<T, TResult>(IEnumerable<T> source, Func<T, TResult> selector)
        {
            foreach (var elem in source)
            {
                yield return selector(elem);
            }
        }

        public static IEnumerable<T> Where<T>(IEnumerable<T> source, Func<T, bool> selector)
        {
            foreach (var elem in source)
            {
                if (selector(elem))
                {
                    yield return elem;
                }
            }
        } 

        public static IEnumerable<T> Concat<T>(IEnumerable<T> fst, IEnumerable<T> snd)
        {
            foreach (var elem in fst)
            {
                yield return elem;
            }
            foreach (var elem in snd)
            {
                yield return elem;
            }
        } 

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(IEnumerable<TFirst> fst,
                                                                         IEnumerable<TSecond> snd,
                                                                         Func<TFirst, TSecond, TResult> selector)
        {
            using (var firstEnumerator = fst.GetEnumerator())
            {
                using (var secondEnumerator = snd.GetEnumerator())
                {

                    while (firstEnumerator.MoveNext() && secondEnumerator.MoveNext())
                    {
                        yield return selector(firstEnumerator.Current, secondEnumerator.Current);
                    }
                }
            }
        }

        public static IEnumerable<T> Take<T>(IEnumerable<T> source, int count)
        {
            if (count <= 0) yield break;

            var e = source.GetEnumerator();
            while (e.MoveNext() && count > 0)
            {
                yield return e.Current;
                count--;
            }
        }

        public static IEnumerable<TSource> Skip<TSource>(IEnumerable<TSource> source, int count)
        {
            var enumerator = source.GetEnumerator();
            try
            {
                while (count-- > 0)
                    if (!enumerator.MoveNext())
                        yield break;

                while (enumerator.MoveNext())
                    yield return enumerator.Current;

            }
            finally
            {
                enumerator.Dispose();
            }
        }
 
        public static int Count<T>(IEnumerable<T> source)
        {
            var collection = source as ICollection<T>;
            if (collection != null)
                return collection.Count;

            var counter = 0;
            using (var enumerator = source.GetEnumerator())
                while (enumerator.MoveNext())
                    checked { counter++; }

            return counter;
        }

        public static bool Any<T>(IEnumerable<T> source)
        {
            var collection = source as ICollection<T>;
            if (collection != null)
                return collection.Count > 0;

            using (var enumerator = source.GetEnumerator())
                return enumerator.MoveNext();

        }

        public static T First<T>(IEnumerable<T> source)
        {
            var list = source as IList<T>;
            if (list != null)
            {
                if (list.Count != 0)
                    return list[0];
            }
            else
            {
                using (var enumerator = source.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                        return enumerator.Current;
                }
            }
            throw new ArgumentOutOfRangeException();
        }

        public static T FirstOrDefault<T>(IEnumerable<T> source)
        {
            var list = source as IList<T>;
            if (list != null)
            {
                if (list.Count != 0)
                    return list[0];
            }
            else
            {
                using (var enumerator = source.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                        return enumerator.Current;
                }
            }
            return default(T);
        }

        public static T LastOrDefault<T>(IEnumerable<T> source)
        {
            var list = source as IList<T>;
            if (list != null)
                return list.Count > 0 ? list [list.Count - 1] : default (T);

            var item = default (T);
            foreach (var element in source) {
                item = element;
            }

            return item;
        }
    }
}
