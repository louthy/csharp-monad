////////////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
// 
// Copyright (c) 2014 Paul Louth
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 

using Monad.Parsec;
using Monad.Utility;
using System;

namespace Monad
{
    public static class ImmutableListExt
    {
        public static bool CanTake<T>(this ImmutableList<T> self, int amount)
        {
            return self.Length - amount >= 0;
        }

        public static string AsString(this ImmutableList<ParserChar> self)
        {
            return String.Concat(self.Select(pc=>pc.Value));
        }

        public static T Second<T>(this ImmutableList<T> self)
        {
            return self.Tail().Head();
        }

        /// <summary>
        /// Foldr
        /// </summary>
        public static U Foldr<T, U>(this ImmutableList<T> self, Func<T, U, U> func, U state)
        {
            foreach (var item in self)
            {
                state = func(item, state);
            }
            return state;
        }

        /// <summary>
        /// Foldr
        /// </summary>
        public static U Foldr<T, U>(this ImmutableList<T> self, Func<U, U> func, U state)
        {
            foreach (var item in self)
            {
                state = func(state);
            }
            return state;
        }

        /// <summary>
        /// Foldl
        /// </summary>
        public static U Foldl<T, U>(this ImmutableList<T> self, Func<T, U, U> func, U state)
        {
            var iter = self.GetReverseEnumerator();
            while (iter.MoveNext())
            {
                state = func(iter.Current, state);
            }
            return state;
        }
    }
}
