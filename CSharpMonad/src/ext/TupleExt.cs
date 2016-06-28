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

using System;

namespace Monad
{
    public static class TupleExtensions
    {
        public static R Apply<T1, T2, R>(this Tuple<T1, T2> self, Func<T1, T2, R> func)
        {
            if (func == null) throw new ArgumentNullException("func");
            return func(self.Item1, self.Item2);
        }

        public static R Apply<T1, T2, T3, R>(this Tuple<T1, T2, T3> self, Func<T1, T2, T3, R> func)
        {
            if (func == null) throw new ArgumentNullException("func");
            return func(self.Item1, self.Item2, self.Item3);
        }

        public static R Apply<T1, T2, T3, T4, R>(this Tuple<T1, T2, T3, T4> self, Func<T1, T2, T3, T4, R> func)
        {
            if (func == null) throw new ArgumentNullException("func");
            return func(self.Item1, self.Item2, self.Item3, self.Item4);
        }

        public static R Apply<T1, T2, T3, T4, T5, R>(this Tuple<T1, T2, T3, T4, T5> self, Func<T1, T2, T3, T4, T5, R> func)
        {
            if (func == null) throw new ArgumentNullException("func");
            return func(self.Item1, self.Item2, self.Item3, self.Item4, self.Item5);
        }

        public static void Apply<T1, T2>(this Tuple<T1, T2> self, Action<T1, T2> func)
        {
            if (func == null) throw new ArgumentNullException("func");
            func(self.Item1, self.Item2);
        }

        public static void Apply<T1, T2, T3>(this Tuple<T1, T2, T3> self, Action<T1, T2, T3> func)
        {
            if (func == null) throw new ArgumentNullException("func");
            func(self.Item1, self.Item2, self.Item3);
        }

        public static void Apply<T1, T2, T3, T4>(this Tuple<T1, T2, T3, T4> self, Action<T1, T2, T3, T4> func)
        {
            if (func == null) throw new ArgumentNullException("func");
            func(self.Item1, self.Item2, self.Item3, self.Item4);
        }

        public static void Apply<T1, T2, T3, T4, T5>(this Tuple<T1, T2, T3, T4, T5> self, Action<T1, T2, T3, T4, T5> func)
        {
            if (func == null) throw new ArgumentNullException("func");
            func(self.Item1, self.Item2, self.Item3, self.Item4, self.Item5);
        }
    }
}
