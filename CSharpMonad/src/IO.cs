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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad
{
    /// <summary>
    /// IO monad
    /// </summary>
    public delegate T IO<T>();

    /// <summary>
    /// IO monad extension methds
    /// </summary>
    public static class IOMonadExtensions
    {
        /// <summary>
        /// Monadic bind method for the IO delegate
        /// </summary>
        public static IO<R> SelectMany<T, R>(this IO<T> self, Func<T, IO<R>> func)
        {
            return func(self());
        }

        /// <summary>
        /// Extension *bind* method for the IO delegate
        /// </summary>
        public static IO<V> SelectMany<T, U, V>(
            this IO<T> self,
            Func<T, IO<U>> k,
            Func<T, U, V> m)
        {
            return self.SelectMany(
                t => k(t).SelectMany(
                    u => new IO<V>(() => m(t, u))
                    )
                );
        }

        /// <summary>
        /// Allows fluent chaining of IO monads
        /// </summary>
        public static IO<U> Then<T, U>(this IO<T> self, Func<T, U> getValue)
        {
            return () => getValue( self.Invoke() );
        }
    }

    /// <summary>
    /// Helper method to make local var inference work, i.e:
    /// 
    ///		var m = I.O( ()=> File.ReadAllText(path) );
    ///		
    /// </summary>
    public static class I
    {
        public static IO<T> O<T>(IO<T> func)
        {
            return func;
        }
    }
}