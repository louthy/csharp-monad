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
    /// Pretty awful really - An expirment, don't rely on these methods.
    /// I guess I could try and code-gen a ton of variants
    /// Would get pretty crazy pretty quickly
    /// More thought needed, but head currently exploding.
    /// </summary>
    public static class LiftExt
    {
        /// <summary>
        /// Lowest level - lift the value out of the monad and perform a function on it
        /// </summary>
        public static U LiftM<T, U>(this Option<T> m, Func<T, U> liftFn)
        {
            return (from v in m select liftFn(v)).Value();
        }
        public static U LiftM<T, U>(this OptionStrict<T> m, Func<T, U> liftFn)
        {
            return (from v in m select liftFn(v)).Value;
        }
        public static U LiftM<R, L, U>(this Either<R, L> m, Func<R, U> liftFn)
        {
            return (from v in m select liftFn(v)).Right();
        }
        public static U LiftM<R, L, U>(this EitherStrict<R, L> m, Func<R, U> liftFn)
        {
            return (from v in m select liftFn(v)).Right;
        }
        public static U LiftM<T, U>(this IO<T> m, Func<T, U> liftFn)
        {
            return (from v in m select liftFn(v))();
        }
        public static U LiftM<T, U>(this Try<T> m, Func<T, U> liftFn)
        {
            return (from v in m select liftFn(v)).Value();
        }
        public static U LiftM<E, T, U>(this Reader<E, T> m, E envir, Func<T, U> liftFn)
        {
            return (from v in m select liftFn(v))(envir);
        }
        public static U LiftIO<T, U>(this IO<T> m, Func<T, U> liftFn)
        {
            return (from v in m select liftFn(v))();
        }

        /// <summary>
        /// One level LiftIO
        /// </summary>
        public static U LiftIO<T, U>(this Option<IO<T>> m, Func<T, U> liftFn)
        {
            return liftFn(m.Value()());
        }

        public static U LiftIO<R, L, U>(this Either<IO<R>, L> m, Func<R, U> liftFn)
        {
            return liftFn(m.Right()());
        }

        public static U LiftIO<T, U>(this IO<IO<T>> m, Func<T, U> liftFn)
        {
            return liftFn(m()());
        }

        public static U LiftIO<T, U>(this Try<IO<T>> m, Func<T, U> liftFn)
        {
            return liftFn(m().Value());
        }

        public static U LiftIO<E, T, U>(this Reader<E, IO<T>> m, E env, Func<T, U> liftFn)
        {
            return liftFn(m(env)());
        }

        /// <summary>
        /// LiftM2 - Option
        /// </summary>
        public static U LiftM2<T, U>(this Option<Option<T>> m, Func<T, U> liftFn)
        {
            return liftFn(m.Value().Value());
        }

        public static U LiftM2<R, L, U>(this Either<Option<R>, L> m, Func<R, U> liftFn)
        {
            return liftFn(m.Right().Value());
        }

        public static U LiftM2<T, U>(this IO<Option<T>> m, Func<T, U> liftFn)
        {
            return liftFn(m().Value());
        }

        public static U LiftM2<T, U>(this Try<Option<T>> m, Func<T, U> liftFn)
        {
            return liftFn(m().Value().Value);
        }

        public static U LiftM2<E, T, U>(this Reader<E, Option<T>> m, E env, Func<T, U> liftFn)
        {
            return liftFn(m(env)().Value);
        }

        /// <summary>
        /// LiftM2 - Either
        /// </summary>
        public static U LiftM2<R, L, U>(this Option<Either<R,L>> m, Func<R, U> liftFn)
        {
            return liftFn(m.Value()().Right);
        }

        public static U LiftM2<R, L, U>(this Either<Either<R, L>, L> m, Func<R, U> liftFn)
        {
            return liftFn(m.Right()().Right);
        }

        public static U LiftM2<R, L, U>(this IO<Either<R, L>> m, Func<R, U> liftFn)
        {
            return liftFn(m()().Right);
        }

        public static U LiftM2<R, L, U>(this Try<Either<R, L>> m, Func<R, U> liftFn)
        {
            return liftFn(m().Value.Right());
        }

        public static U LiftM2<E, R, L, U>(this Reader<E, Either<R, L>> m, E env, Func<R, U> liftFn)
        {
            return liftFn(m(env)().Right);
        }

        /// <summary>
        /// LiftM2 - Try
        /// </summary>
        public static U LiftM2<T, U>(this Option<Try<T>> m, Func<T, U> liftFn)
        {
            return liftFn(m.Value().Value());
        }

        public static U LiftM2<R, L, U>(this Either<Try<R>, L> m, Func<R, U> liftFn)
        {
            return liftFn(m.Right().Value());
        }

        public static U LiftM2<T, U>(this IO<Try<T>> m, Func<T, U> liftFn)
        {
            return liftFn(m().Value());
        }

        public static U LiftM2<T, U>(this Try<Try<T>> m, Func<T, U> liftFn)
        {
            return liftFn(m().Value.Value());
        }

        public static U LiftM2<E, T, U>(this Reader<E, Try<T>> m, E env, Func<T, U> liftFn)
        {
            return liftFn(m(env).Value());
        }

        /// <summary>
        /// LiftM2 - Reader
        /// </summary>
        public static U LiftM2<E, T, U>(this Option<Reader<E, T>> m, E env, Func<T, U> liftFn)
        {
            return liftFn(m.Value()(env));
        }

        public static U LiftM2<E, R, L, U>(this Either<Reader<E, R>, L> m, E env, Func<R, U> liftFn)
        {
            return liftFn(m.Right()(env));
        }

        public static U LiftM2<E, T, U>(this IO<Reader<E, T>> m, E env, Func<T, U> liftFn)
        {
            return liftFn(m()(env));
        }

        public static U LiftM2<E, T, U>(this Try<Reader<E, T>> m, E env, Func<T, U> liftFn)
        {
            return liftFn(m().Value(env));
        }

        public static U LiftM2<E, T, U>(this Reader<E, Reader<E, T>> m, E env, Func<T, U> liftFn)
        {
            return liftFn(m(env)(env));
        }

        /// <summary>
        /// LiftM2 - IO
        /// </summary>
        public static U LiftM2<T, U>(this Option<IO<T>> m, Func<T, U> liftFn)
        {
            return m.LiftIO(liftFn);
        }

        public static U LiftM2<R, L, U>(this Either<IO<R>, L> m, Func<R, U> liftFn)
        {
            return m.LiftIO(liftFn);
        }

        public static U LiftM2<T, U>(this IO<IO<T>> m, Func<T, U> liftFn)
        {
            return m.LiftIO(liftFn);
        }

        public static U LiftM2<T, U>(this Try<IO<T>> m, Func<T, U> liftFn)
        {
            return m.LiftIO(liftFn);
        }

        public static U LiftM2<E, T, U>(this Reader<E, IO<T>> m, E env, Func<T, U> liftFn)
        {
            return m.LiftIO(env,liftFn);
        }
    }
}
