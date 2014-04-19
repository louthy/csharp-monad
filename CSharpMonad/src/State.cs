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
    public delegate Tuple<S,A> State<S,A>(S state);

    public class State
    {
        public static State<S,A> Return<S,A>(A value)
        {
            return (S state) => new Tuple<S,A>(state, value);
        }
    }

    public class StateResult<S,A>
    {
        public readonly S State;
        public readonly A Value;

        public StateResult(S state, A value)
        {
            State = state;
            Value = value;
        }

        public StateResult<S,A> Set(S state)
        {
            return new StateResult<S,A>(state, Value);
        }
    }

    public static class StateExt
    {
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
    } 
}

