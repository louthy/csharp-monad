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
using Monad.Utility;

namespace Monad
{
    /// <summary>
    /// The writer monad
    /// </summary>
    public delegate WriterResult<W, A> Writer<W, A>();

    /// <summary>
    /// Writer result.
    /// </summary>
    public struct WriterResult<W, A>
    {
        public readonly A Value;
        public readonly IEnumerable<W> Output;

        internal WriterResult(A value, IEnumerable<W> output)
        {
            if (output == null) throw new ArgumentNullException("output");
            Value = value;
            Output = output;
        }
    }

    /// <summary>
    /// Writer result factory
    /// </summary>
    public static class WriterResult
    {
        public static WriterResult<W, A> Create<W, A>(A value, IEnumerable<W> output)
        {
            if (output == null) throw new ArgumentNullException("output");
            return new WriterResult<W, A>(value, output);
        }
    }

    /// <summary>
    /// Writer
    /// </summary>
    public static class Writer
    {
        public static Writer<W, A> Return<W, A>(A a)
        {
            return () => WriterResult.Create<W, A>(a, new W[0]);
        }

        public static WriterResult<W, A> Tell<W, A>(A a, W w)
        {
            return WriterResult.Create<W, A>(a, new W[1] { w });
        }

        public static WriterResult<W, A> Tell<W, A>(A a, IEnumerable<W> ws)
        {
            if (ws == null) throw new ArgumentNullException("ws");
            return WriterResult.Create<W, A>(a, ws);
        }

        public static Writer<W, Unit> Tell<W>(W value)
        {
            return () => WriterResult.Create<W, Unit>(Unit.Default, new W[1] { value });
        }
    }

    /// <summary>
    /// Writer extension methods
    /// </summary>
    public static class WriterExt
    {
        /// <summary>
        /// Select
        /// </summary>
        public static Writer<W, U> Select<W, T, U>(this Writer<W, T> self, Func<T, U> select)
        {
            if (select == null) throw new ArgumentNullException("select");
            return () =>
            {
                var resT = self();
                var resU = select(resT.Value);
                return WriterResult.Create<W, U>(resU, resT.Output);
            };
        }

        /// <summary>
        /// Select Many
        /// </summary>
        public static Writer<W, V> SelectMany<W, T, U, V>(
            this Writer<W, T> self,
            Func<T, Writer<W, U>> bind,
            Func<T, U, V> project
        )
        {
            if (bind == null) throw new ArgumentNullException("bind");
            if (project == null) throw new ArgumentNullException("project");

            return () =>
            {
                var resT = self();
                var resU = bind(resT.Value).Invoke();
                var resV = project(resT.Value, resU.Value);

                return WriterResult.Create<W, V>(resV, resT.Output.Concat(resU.Output));
            };
        }

        /// <summary>
        /// Memoize the result 
        /// </summary>
        public static Func<WriterResult<W, T>> Memo<W, T>(this Writer<W, T> self)
        {
            var res = self();
            return () => res;
        }
    }
}