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
using Monad.Utility;

namespace Monad
{
    public delegate StateResult<S, A> State<S, A>(S state);

    public static class State
    {
        public static State<S, A> Return<S, A>(A value = default(A))
        {
            return (S state) => new StateResult<S, A>(state, value);
        }

        public static State<S, S> Get<S>(Func<S, S> f)
        {
            if (f == null) throw new ArgumentNullException("f");
            return (S state) => StateResult.Create<S, S>(state, f(state));
        }

        public static State<S, S> Get<S>()
        {
            return (S state) => StateResult.Create<S, S>(state, state);
        }

        public static State<S, Unit> Put<S>(S state)
        {
            return _ => StateResult.Create<S, Unit>(state, Unit.Default);
        }
    }

    /// <summary>
    /// State result.
    /// </summary>
    public struct StateResult<S, A>
    {
        public readonly A Value;
        public readonly S State;

        internal StateResult(S state, A value)
        {
            Value = value;
            State = state;
        }
    }

    /// <summary>
    /// State result factory
    /// </summary>
    public static class StateResult
    {
        public static StateResult<S, A> Create<S, A>(S state, A value)
        {
            return new StateResult<S, A>(state, value);
        }
    }

    public static class StateExt
    {
        public static State<S, A> With<S, A>(this State<S, A> self, Func<S, S> f)
        {
            if (f == null) throw new ArgumentNullException("f");
            return (S state) =>
            {
                var res = self(state);
                return StateResult.Create<S, A>(f(res.State), res.Value);
            };
        }

        public static State<S, U> Select<S, T, U>(this State<S, T> self, Func<T, U> map)
        {
            if (map == null) throw new ArgumentNullException("map");
            return (S state) =>
            {
                var resT = self(state);
                return StateResult.Create<S, U>(resT.State, map(resT.Value));
            };
        }

        public static State<S, V> SelectMany<S, T, U, V>(
            this State<S, T> self,
            Func<T, State<S, U>> bind,
            Func<T, U, V> project 
            )
        {
            if (bind == null) throw new ArgumentNullException("bind");
            if (project == null) throw new ArgumentNullException("project");

            return (S state) =>
            {
                var resT = self(state);
                var resU = bind(resT.Value)(resT.State);
                var resV = project(resT.Value, resU.Value);
                return new StateResult<S, V>(resU.State, resV);
            };
        }

        /// <summary>
        /// Memoize the result 
        /// </summary>
        public static Func<StateResult<S, A>> Memo<S, A>(this State<S, A> self, S state)
        {
            var res = self(state);
            return () => res;
        }
    }
}