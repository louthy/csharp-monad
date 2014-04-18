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
    /// The option delegate
    /// </summary>
    public delegate OptionResult<T> Option<T>();

    public static class Option
    {
        /// <summary>
        /// Represents an Option without a value
        /// </summary>
        public static Option<T> Nothing<T>()
        {
            return () => new Nothing<T>();
        }

        public static Option<T> Mempty<T>()
        {
            return Option.Nothing<T>();
        }
    }

    /// <summary>
    /// Option monad
    /// </summary>
    public abstract class OptionResult<T>
    {
        /// <summary>
        /// Conversion from any value to Option<T>
        /// Null is always considered as Nothing
        /// </summary>
        public static implicit operator OptionResult<T>(T value)
        {
            return value == null
                ? new Nothing<T>() as OptionResult<T>
                : new Just<T>(value) as OptionResult<T>;
        }

        /// <summary>
        /// Monad value
        /// </summary>
        public abstract T Value
        {
            get;
        }

        /// <summary>
        /// Does the monad have a value
        /// </summary>
        public abstract bool HasValue
        {
            get;
        }

        /// <summary>
        /// Get the monad's value or the default value for the type
        /// </summary>
        /// <returns></returns>
        public T GetValueOrDefault()
        {
            return HasValue ? Value : default(T);
        }

        public abstract OptionResult<T> Mappend(OptionResult<T> rhs);
    }

    /// <summary>
    /// Option<T> monad extension methods
    /// </summary>
    public static class OptionExtensions
    {
        /// <summary>
        /// Converts this object to a Option monad.
        /// Tretas null as Nothing
        /// </summary>
        /// <returns>Option<T></returns>
        public static Option<T> ToOption<T>(this T self)
        {
            return self == null
                ? Option.Nothing<T>()
                : () => new Just<T>(self);
        }

        public static Option<R> Select<T, R>(this Option<T> self, Func<T, R> map)
        {
            var resT = self();
            return resT.HasValue
                ? map(resT.Value).ToOption()
                : Option.Nothing<R>();
        }

        public static Option<U> SelectMany<T, U>(this Option<T> self, Func<T, Option<U>> k)
        {
            return () =>
            {
                var resT = self();
                return resT.HasValue
                    ? k(resT.Value)()
                    : new Nothing<U>();
            };
        }

        public static Option<V> SelectMany<T, U, V>(
            this Option<T> self, 
            Func<T, Option<U>> select, 
            Func<T, U, V> project
            )
        {
            return () =>
            {
                var resT = self();

                if (!resT.HasValue)
                    return new Nothing<V>();

                var resU = select(resT.Value)();
                if (!resU.HasValue)
                    return new Nothing<V>();

                return new Just<V>(project(resT.Value, resU.Value));
            };
        }

        /// <summary>
        /// Underlying value
        /// </summary>
        public static T Value<T>(this Option<T> self)
        {
            return self().Value;
        }

        /// <summary>
        /// Does the monad have a value
        /// </summary>
        public static bool HasValue<T>(this Option<T> self)
        {
            return self().HasValue;
        }

        /// <summary>
        /// Get the monad's value or the default value for the type
        /// </summary>
        /// <returns></returns>
        public static T GetValueOrDefault<T>(this Option<T> self)
        {
            var res = self();
            return res.HasValue ? res.Value : default(T);
        }

        /// <summary>
        /// Executes the delegate related to the derived Option type.
        /// </summary>
        public static R Match<T,R>(this Option<T> self, Func<R> Just, Func<R> Nothing)
        {
            var res = self();
            if (res.HasValue)
                return Just();
            else
                return Nothing();
        }

        /// <summary>
        /// Executes the delegate related to the derived Option type.
        /// </summary>
        public static R Match<T, R>(this Option<T> self, Func<T, R> Just, Func<R> Nothing)
        {
            var res = self();
            if (res.HasValue)
                return Just(res.Value);
            else
                return Nothing();
        }

        /// <summary>
        /// Executes the delegate related to the derived Option type.
        /// </summary>
        public static R Match<T, R>(this Option<T> self, Func<R> Just, R Nothing)
        {
            var res = self();
            if (res.HasValue)
                return Just();
            else
                return Nothing;
        }

        /// <summary>
        /// Executes the delegate related to the derived Option type.
        /// </summary>
        public static R Match<T, R>(this Option<T> self, Func<T, R> Just, R Nothing)
        {
            var res = self();
            if (res.HasValue)
                return Just(res.Value);
            else
                return Nothing;
        }

        /// <summary>
        /// Monadic append
        /// If the lhs or rhs are in a Nothing state then Nothing propagates
        /// </summary>
        public static Option<T> Mappend<T>(this Option<T> self, Option<T> rhs)
        {
            return () => self().Mappend(rhs());
        }

        /// <summary>
        /// Converts the Option to an enumerable
        /// </summary>
        /// <returns>
        /// Just: A list with one T in
        /// Nothing: An empty list
        /// </returns>
        public static IEnumerable<T> AsEnumerable<T>(this Option<T> self)
        {
            var res = self();
            if (res.HasValue)
                yield return res.Value;
            else
                yield break;
        }

        /// <summary>
        /// Converts the Option to an infinite enumerable
        /// </summary>
        /// <returns>
        /// Just: An infinite list of T
        /// Nothing: An empty list
        /// </returns>
        public static IEnumerable<T> AsEnumerableInfinte<T>(this Option<T> self)
        {
            var res = self();
            if (res.HasValue)
                while (true) yield return res.Value;
            else
                yield break;
        }

        /// <summary>
        /// Memoize the result 
        /// </summary>
        public static Func<OptionResult<T>> Memo<T>(this Option<T> self)
        {
            var res = self();
            return () => res;
        }
    }
}