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
using System.Collections.Generic;
using System.Linq;

namespace Monad
{
    public static class EnumerableExt
    {
        public static T Head<T>(this IEnumerable<T> self)
        {
            return self.First();
        }
        public static IEnumerable<T> Tail<T>(this IEnumerable<T> self)
        {
            return self.Skip(1);
        }
        public static T Second<T>(this IEnumerable<T> self)
        {
            return self.Tail().Head();
        }

        public static ImmutableList<ParserChar> ToParserChar(this IEnumerable<char> input)
        {
            int line = 1;
            int col = 1;

            var list = new List<ParserChar>();

            foreach (var c in input)
            {
                if (c == '\r')
                {
                    continue;
                }

                list.Add(new ParserChar(c, SrcLoc.Return(line, col)));

                if (c == '\n')
                {
                    line++;
                    col = 1;
                }
                col++;
            }

            return new ImmutableList<ParserChar>(list);
        }

        /// <summary>
        /// Foldr
        /// </summary>
        public static U Foldr<T, U>(this IEnumerable<T> self, Func<T, U, U> func, U state)
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
        public static U Foldr<T, U>(this IEnumerable<T> self, Func<U, U> func, U state)
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
        public static U Foldl<T, U>(this IEnumerable<T> self, Func<T, U, U> func, U state)
        {
            return self.Reverse().Foldr(func, state);
        }
    }
}
