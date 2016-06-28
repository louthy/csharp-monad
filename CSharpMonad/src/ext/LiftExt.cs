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
    /// Pretty awful really - An expirment, don't rely on these methods.
    /// I guess I could try and code-gen a ton of variants
    /// Would get pretty crazy pretty quickly
    /// More thought needed, but head currently exploding.
    /// </summary>
    public static class Lift
    {
        /// <summary>
        /// Lift the value out of the monad,perform a function on it, repackage it
        /// </summary>
        public static Option<U> M<T, U>(Option<T> m, Func<T, U> liftFn)
        {
            return from v in m select liftFn(v);
        }
        public static OptionStrict<U> M<T, U>(OptionStrict<T> m, Func<T, U> liftFn)
        {
            return from v in m select liftFn(v);
        }
        public static Either<L, U> M<L, R, U>(Either<L, R> m, Func<R, U> liftFn)
        {
            return from v in m select liftFn(v);
        }
        public static EitherStrict<L, U> M<L, R, U>(EitherStrict<L, R> m, Func<R, U> liftFn)
        {
            return from v in m select liftFn(v);
        }
        public static IO<U> M<T, U>(IO<T> m, Func<T, U> liftFn)
        {
            return from v in m select liftFn(v);
        }
        public static Try<U> M<T, U>(Try<T> m, Func<T, U> liftFn)
        {
            return from v in m select liftFn(v);
        }
        public static Reader<E, U> M<E, T, U>(Reader<E, T> m, Func<T, U> liftFn)
        {
            return from v in m select liftFn(v);
        }
        public static Writer<W, U> M<W, T, U>(Writer<W, T> m, Func<T, U> liftFn)
        {
            return from v in m select liftFn(v);
        }
        public static State<S, U> M<S, T, U>(State<S, T> m, Func<T, U> liftFn)
        {
            return from v in m select liftFn(v);
        }
        public static RWS<R, W, S, U> M<R, W, S, T, U>(RWS<R, W, S, T> m, Func<T, U> liftFn)
            where S : class
        {
            return from v in m select liftFn(v);
        }
        public static IO<U> IO<T, U>(IO<T> m, Func<T, U> liftFn)
        {
            return from v in m select liftFn(v);
        }

        /// <summary>
        /// Lift the value out of the monad,perform a function on it, repackage it
        /// </summary>
        public static Option<C> M<A, B, C>(Option<A> ma, Option<B> mb, Func<A, B, C> liftFn)
        {
            return from a in ma
                   from b in mb
                   select liftFn(a, b);
        }
        public static OptionStrict<C> M<A, B, C>(OptionStrict<A> ma, OptionStrict<B> mb, Func<A, B, C> liftFn)
        {
            return from a in ma
                   from b in mb
                   select liftFn(a, b);
        }
        public static Either<L, C> M<L, A, B, C>(Either<L, A> ma, Either<L, B> mb, Func<A, B, C> liftFn)
        {
            return from a in ma
                   from b in mb
                   select liftFn(a, b);
        }
        public static EitherStrict<L, C> M<L, A, B, C>(EitherStrict<L, A> ma, EitherStrict<L, B> mb, Func<A, B, C> liftFn)
        {
            return from a in ma
                   from b in mb
                   select liftFn(a, b);
        }
        public static IO<C> M<A, B, C>(IO<A> ma, IO<B> mb, Func<A, B, C> liftFn)
        {
            return from a in ma
                   from b in mb
                   select liftFn(a, b);
        }
        public static Try<C> M<A, B, C>(Try<A> ma, Try<B> mb, Func<A, B, C> liftFn)
        {
            return from a in ma
                   from b in mb
                   select liftFn(a, b);
        }
        public static Reader<E, C> M<E, A, B, C>(Reader<E, A> ma, Reader<E, B> mb, Func<A, B, C> liftFn)
        {
            return from a in ma
                   from b in mb
                   select liftFn(a, b);
        }
        public static Writer<W, C> M<W, A, B, C>(Writer<W, A> ma, Writer<W, B> mb, Func<A, B, C> liftFn)
        {
            return from a in ma
                   from b in mb
                   select liftFn(a, b);
        }
        public static State<S, C> M<S, A, B, C>(State<S, A> ma, State<S, B> mb, Func<A, B, C> liftFn)
        {
            return from a in ma
                   from b in mb
                   select liftFn(a, b);
        }
        public static RWS<R, W, S, C> M<R, W, S, A, B, C>(RWS<R, W, S, A> ma, RWS<R, W, S, B> mb, Func<A, B, C> liftFn)
            where S : class
        {
            return from a in ma
                   from b in mb
                   select liftFn(a, b);
        }
        public static IO<C> IO<A, B, C>(IO<A> ma, IO<B> mb, Func<A, B, C> liftFn)
        {
            return from a in ma
                   from b in mb
                   select liftFn(a, b);
        }

        /// <summary>
        /// Lift the value out of the monad,perform a function on it, repackage it
        /// </summary>
        public static Option<D> M<A, B, C, D>(Option<A> ma, Option<B> mb, Option<C> mc, Func<A, B, C, D> liftFn)
        {
            return from a in ma
                   from b in mb
                   from c in mc
                   select liftFn(a, b, c);
        }
        public static OptionStrict<D> M<A, B, C, D>(OptionStrict<A> ma, OptionStrict<B> mb, OptionStrict<C> mc, Func<A, B, C, D> liftFn)
        {
            return from a in ma
                   from b in mb
                   from c in mc
                   select liftFn(a, b, c);
        }
        public static Either<L, D> M<L, A, B, C, D>(Either<L, A> ma, Either<L, B> mb, Either<L, C> mc, Func<A, B, C, D> liftFn)
        {
            return from a in ma
                   from b in mb
                   from c in mc
                   select liftFn(a, b, c);
        }
        public static EitherStrict<L, D> M<L, A, B, C, D>(EitherStrict<L, A> ma, EitherStrict<L, B> mb, EitherStrict<L, C> mc, Func<A, B, C, D> liftFn)
        {
            return from a in ma
                   from b in mb
                   from c in mc
                   select liftFn(a, b, c);
        }
        public static IO<D> M<A, B, C, D>(IO<A> ma, IO<B> mb, IO<C> mc, Func<A, B, C, D> liftFn)
        {
            return from a in ma
                   from b in mb
                   from c in mc
                   select liftFn(a, b, c);
        }
        public static Try<D> M<A, B, C, D>(Try<A> ma, Try<B> mb, Try<C> mc, Func<A, B, C, D> liftFn)
        {
            return from a in ma
                   from b in mb
                   from c in mc
                   select liftFn(a, b, c);
        }
        public static Reader<E, D> M<E, A, B, C, D>(Reader<E, A> ma, Reader<E, B> mb, Reader<E, C> mc, Func<A, B, C, D> liftFn)
        {
            return from a in ma
                   from b in mb
                   from c in mc
                   select liftFn(a, b, c);
        }
        public static Writer<W, D> M<W, A, B, C, D>(Writer<W, A> ma, Writer<W, B> mb, Writer<W, C> mc, Func<A, B, C, D> liftFn)
        {
            return from a in ma
                   from b in mb
                   from c in mc
                   select liftFn(a, b, c);
        }
        public static State<S, D> M<S, A, B, C, D>(State<S, A> ma, State<S, B> mb, State<S, C> mc, Func<A, B, C, D> liftFn)
        {
            return from a in ma
                   from b in mb
                   from c in mc
                   select liftFn(a, b, c);
        }
        public static RWS<R, W, S, D> M<R, W, S, A, B, C, D>(RWS<R, W, S, A> ma, RWS<R, W, S, B> mb, RWS<R, W, S, C> mc, Func<A, B, C, D> liftFn)
            where S : class
        {
            return from a in ma
                   from b in mb
                   from c in mc
                   select liftFn(a, b, c);
        }
        public static IO<D> IO<A, B, C, D>(IO<A> ma, IO<B> mb, IO<C> mc, Func<A, B, C, D> liftFn)
        {
            return from a in ma
                   from b in mb
                   from c in mc
                   select liftFn(a, b, c);
        }


        /// <summary>
        /// One level LiftIO
        /// </summary>
        public static Option<IO<U>> IO<T, U>(Option<IO<T>> m, Func<T, U> liftFn)
        {
            return from v in m select Lift.M(v, liftFn);
        }

        public static Either<L, IO<U>> IO<R, L, U>(Either<L, IO<R>> m, Func<R, U> liftFn)
        {
            return from v in m select Lift.M(v, liftFn);
        }

        public static IO<IO<U>> IO<T, U>(IO<IO<T>> m, Func<T, U> liftFn)
        {
            return from v in m select Lift.M(v, liftFn);
        }

        public static Try<IO<U>> IO<T, U>(Try<IO<T>> m, Func<T, U> liftFn)
        {
            return from v in m select Lift.M(v, liftFn);
        }

        public static Reader<E, IO<U>> IO<E, T, U>(Reader<E, IO<T>> m, Func<T, U> liftFn)
        {
            return from v in m select Lift.M(v, liftFn);
        }

        public static Writer<W, IO<U>> IO<W, T, U>(Writer<W, IO<T>> m, Func<T, U> liftFn)
        {
            return from v in m select Lift.M(v, liftFn);
        }

        public static State<S, IO<U>> IO<S, T, U>(State<S, IO<T>> m, Func<T, U> liftFn)
        {
            return from v in m select Lift.M(v, liftFn);
        }

        public static RWS<R, W, S, IO<U>> IO<R, W, S, T, U>(RWS<R, W, S, IO<T>> m, Func<T, U> liftFn)
            where S : class
        {
            return from v in m select Lift.M(v, liftFn);
        }
    }
}