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
    public delegate Tuple<S,A> State<S,A>(S state);

    public static class State
    {
        public static State<S,A> Return<S,A>(A value = default(A))
        {
            return (S state) => new Tuple<S,A>(state, value);
        }

        public static State<S,S> Get<S>( Func<S,S> f )
        {
            return (S state) => Tuple.Create<S,S>( state, f(state) );
        }

        public static State<S,S> Get<S>()
        {
            return (S state) => Tuple.Create<S,S>( state, state );
        }

        public static State<S,Unit> Put<S>( S state )
        {
            return _ => Tuple.Create<S,Unit>( state, Unit.Return() );
        }
    }

    public static class StateExt
    {
        public static State<S,A> With<S,A>( this State<S,A> self, Func<S,S> f )
        {
            return (S state) =>
            {
                var res = self(state);
                return Tuple.Create<S,A>(f(res.Item1), res.Item2);
            };
        }

        public static State<S, U> Select<S,T,U>(this State<S,T> self, Func<T,U> map)
        {
            return (S state) =>
            {
                var resT = self(state);
                return Tuple.Create<S,U>( resT.Item1, map(resT.Item2) );
            };
        }

        public static State<S, V> SelectMany<S,T,U,V>(
            this State<S,T> self,
            Func<T,State<S,U>> bind,
            Func<T,U,V> project
        )
        {
            return (S state) =>
            {
                var resT = self(state);
                var resU = bind(resT.Item2)(resT.Item1);
                var resV = project(resT.Item2,resU.Item2);
                return new Tuple<S,V>(resU.Item1,resV);
            };
        }

        /// <summary>
        /// Memoize the result 
        /// </summary>
        public static Func<Tuple<S,A>> Memo<S, A>(this State<S, A> self, S state)
        {
            var res = self(state);
            return () => res;
        } 
    } 
}

