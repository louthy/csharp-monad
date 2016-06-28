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
    /// The Reader Writer State monad
    /// </summary>
    public delegate RWSResult<W, S, A> RWS<R, W, S, A>(R r, S s);

    /// <summary>
    /// RWS result.
    /// </summary>
    public struct RWSResult<W, S, A>
    {
        public readonly A Value;
        public readonly IEnumerable<W> Output;
        public readonly S State;

        internal RWSResult(A value, IEnumerable<W> output, S state)
        {
            Value = value;
            Output = output;
            State = state;
        }
    }

    /// <summary>
    /// RWSResult factory
    /// </summary>
    public static class RWSResult
    {
        public static RWSResult<W, S, A> Create<W, S, A>(A value, IEnumerable<W> output, S state)
        {
            if (output == null) throw new ArgumentNullException("output");
            return new RWSResult<W, S, A>(value, output, state);
        }
    }

    /// <summary>
    /// Reader Writer State
    /// </summary>
    public static class RWS
    {
        public static RWS<R, W, S, A> Return<R, W, S, A>(A a)
        {
            return (R r, S s) => RWSResult.Create<W, S, A>(a, new W[0], s);
        }

        public static RWSResult<W, S, A> Tell<W, S, A>(A a, W w)
        {
            return RWSResult.Create<W, S, A>(a, new W[1] { w }, default(S));
        }

        public static RWSResult<W, S, A> Tell<W, S, A>(A a, IEnumerable<W> ws)
        {
            if (ws == null) throw new ArgumentNullException("ws");
            return RWSResult.Create<W, S, A>(a, ws, default(S));
        }

        public static RWS<R, W, S, Unit> Tell<R, W, S>(W value)
        {
            return (R r, S s) => RWSResult.Create<W, S, Unit>(Unit.Default, new W[1] { value }, s);
        }

        public static RWS<R, W, S, R> Ask<R, W, S>(Func<R, R> f)
        {
            if (f == null) throw new ArgumentNullException("f");
            return (R r, S s) => RWSResult.Create(f(r), new W[0], s);
        }

        public static RWS<R, W, S, R> Ask<R, W, S>()
        {
            return (R r, S s) => RWSResult.Create(r, new W[0], s);
        }

        public static RWS<R, W, S, S> Get<R, W, S>(Func<S, S> f)
        {
            if (f == null) throw new ArgumentNullException("f");
            return (R r, S s) => RWSResult.Create<W, S, S>(s, new W[0], f(s));
        }

        public static RWS<R, W, S, S> Get<R, W, S>()
        {
            return (R r, S s) => RWSResult.Create<W, S, S>(s, new W[0], s);
        }

        public static RWS<R, W, S, Unit> Put<R, W, S>(S state)
        {
            return (R r, S s) => RWSResult.Create<W, S, Unit>(Unit.Default, new W[0], state);
        }

    }

    /// <summary>
    /// Reader Writer State extension methods
    /// </summary>
    public static class RWSExt
    {
        public static RWS<R, W, S, R> Ask<R, W, S, T>(this RWS<R, W, S, T> self, Func<R, R> f)
        {
            if (f == null) throw new ArgumentNullException("f");
            return (R r, S s) => RWSResult.Create(f(r), new W[0], s);
        }

        public static RWS<R, W, S, R> Ask<R, W, S, T>(this RWS<R, W, S, T> self)
        {
            return (R r, S s) => RWSResult.Create(r, new W[0], s);
        }

        /// <summary>
        /// Select
        /// </summary>
        public static RWS<R, W, S, U> Select<R, W, S, T, U>(this RWS<R, W, S, T> self, Func<T, U> select)
            where S : class
        {
            if (select == null) throw new ArgumentNullException("select");
            return (R r, S s) =>
            {
                var resT = self(r, s);
                var resU = select(resT.Value);
                return RWSResult.Create<W, S, U>(resU, resT.Output, resT.State ?? s);
            };
        }

        /// <summary>
        /// Select Many
        /// </summary>
        public static RWS<R, W, S, V> SelectMany<R, W, S, T, U, V>(
            this RWS<R, W, S, T> self,
            Func<T, RWS<R, W, S, U>> bind,
            Func<T, U, V> project
        )
            where S : class
        {
            if (bind == null) throw new ArgumentNullException("bind");
            if (project == null) throw new ArgumentNullException("project");

            return (R r, S s) =>
            {
                var resT = self(r, s);
                var resU = bind(resT.Value).Invoke(r, resT.State ?? s);
                var resV = project(resT.Value, resU.Value);

                return RWSResult.Create<W, S, V>(resV, resT.Output.Concat(resU.Output), resU.State ?? resT.State ?? s);
            };
        }

        /// <summary>
        /// Memoize the result 
        /// </summary>
        public static Func<RWSResult<W, S, T>> Memo<R, W, S, T>(this RWS<R, W, S, T> self, R r, S s)
        {
            var res = self(r, s);
            return () => res;
        }
    }
}

