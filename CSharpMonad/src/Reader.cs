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
    /// <summary>
    /// The reader monad
    /// Allows for an 'environment' value to be carried through bind functions
    /// </summary>
    /// <typeparam name="E">Environment</typeparam>
    /// <typeparam name="A">The underlying monadic type</typeparam>
    public delegate A Reader<E, A>(E environment);

    /// <summary>
    /// Reader.
    /// </summary>
    public static class Reader
    {
        public static Reader<E, A> Return<E, A>(A value = default(A))
        {
            return (E env) => value;
        }

        public static Reader<E, E> Ask<E>(Func<E, E> f)
        {
            if (f == null) throw new ArgumentNullException("f");
            return (E env) => f(env);
        }

        public static Reader<E, E> Ask<E>()
        {
            return (E env) => env;
        }
    }

    /// <summary>
    /// Reader monad extensions
    /// </summary>
    public static class ReaderExt
    {
        public static Reader<E, E> Ask<E, T>(this Reader<E, T> self, Func<E, E> f)
        {
            if (f == null) throw new ArgumentNullException("f");
            return (E env) => f(env);
        }

        public static Reader<E, E> Ask<E, T>(this Reader<E, T> self)
        {
            return (E env) => env;
        }

        /// <summary>
        /// Select
        /// </summary>
        public static Reader<E, U> Select<E, T, U>(this Reader<E, T> self, Func<T, U> select)
        {
            if (select == null) throw new ArgumentNullException("select");
            return (E env) => select(self(env));
        }

        /// <summary>
        /// Select Many
        /// </summary>
        public static Reader<E, V> SelectMany<E, T, U, V>(
            this Reader<E, T> self,
            Func<T, Reader<E, U>> bind,
            Func<T, U, V> project
            )
        {
            if (bind == null) throw new ArgumentNullException("bind");
            if (project == null) throw new ArgumentNullException("project");
            return (E env) =>
                {
                    var resT = self(env);
                    var resU = bind(resT);
                    var resV = project(resT, resU(env));
                    return resV;
                };
        }

        /// <summary>
        /// Memoize the result 
        /// </summary>
        public static Func<T> Memo<E, T>(this Reader<E, T> self, E environment)
        {
            var res = self(environment);
            return () => res;
        }

    }
}