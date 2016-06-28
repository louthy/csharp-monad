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

namespace Monad
{
    /// <summary>
    /// The option delegate
    /// </summary>
    public delegate OptionResult<T> Option<T>();

    /// <summary>
    /// Non-generic Option helper class.  Contains generic methods for creating
    /// Option<T> types.
    /// </summary>
    public static class Option
    {
        /// <summary>
        /// Represents an Option without a value
        /// </summary>
        public static Option<T> Nothing<T>()
        {
            return () => NothingResult<T>.Default;
        }

        public static Option<T> Mempty<T>()
        {
            return Option.Nothing<T>();
        }

        /// <summary>
        /// Wraps a Func<T> in an Option<T>
        /// Upon invocation if del() returns null it will automatically be
        /// converted to a Nothing<T>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Option<T> Return<T>(Func<T> del)
        {
            return new Option<T>( () =>
                {
                    var res = del();
                    return res == null
                        ? NothingResult<T>.Default
                        : new JustResult<T>(res);
                });
        }

    }

    /// <summary>
    /// Option monad
    /// </summary>
    public abstract class OptionResult<T>
    {
        /// <summary>
        /// Conversion from any value to OptionResult<T>
        /// Null is always considered as Nothing
        /// </summary>
        public static implicit operator OptionResult<T>(T value)
        {
            return value == null
                ? NothingResult<T>.Default
                : new JustResult<T>(value);
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
        /// Converts _this_ to an OptionResult<T>.
        /// Treats null as Nothing
        /// </summary>
        /// <returns>OptionResult<T></returns>
        public static OptionResult<T> ToOption<T>(this T self)
        {
            return self == null
                    ? NothingResult<T>.Default
                    : new JustResult<T>(self);
        }

        /// <summary>
        /// Select
        /// </summary>
        public static Option<R> Select<T, R>(this Option<T> self, Func<T, R> map)
        {
            if (map == null) throw new ArgumentNullException("map");
            return () =>
            {
                var resT = self();
                if (resT != null && resT.HasValue)
                {
                    var resR = map(resT.Value);
                    if (resR == null)
                    {
                        return NothingResult<R>.Default;
                    }
                    else
                    {
                        return new JustResult<R>(resR);
                    }
                }
                else
                {
                    return NothingResult<R>.Default;
                }
            };
        }

        public static Option<V> SelectMany<T, U, V>(
            this Option<T> self, 
            Func<T, Option<U>> select, 
            Func<T, U, V> project
            )
        {
            if (select == null) throw new ArgumentNullException("select");
            if (project == null) throw new ArgumentNullException("project");

            return () =>
            {
                var resT = self();

                if (resT != null && resT.HasValue)
                {
                    var resUOpt = select(resT.Value);
                    if (resUOpt != null)
                    {
                        var resU = resUOpt();
                        if (resU != null && resU.HasValue)
                        {
                            var resV = project(resT.Value, resU.Value);
                            if (resV != null)
                            {
                                return new JustResult<V>(resV);
                            }
                            else
                            {
                                return NothingResult<V>.Default;
                            }
                        }
                        else
                        {
                            return NothingResult<V>.Default;
                        }
                    }
                    else
                    {
                        return NothingResult<V>.Default;
                    }
                }
                else
                {
                    return NothingResult<V>.Default;
                }
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
        public static Func<R> Match<T,R>(this Option<T> self, Func<R> Just, Func<R> Nothing)
        {
            if (Just == null) throw new ArgumentNullException("Just");
            if (Nothing == null) throw new ArgumentNullException("Nothing");
            return () =>
            {
                var res = self();
                if (res.HasValue)
                    return Just();
                else
                    return Nothing();
            };
        }

        /// <summary>
        /// Executes the delegate related to the derived Option type.
        /// </summary>
        public static Func<R> Match<T, R>(this Option<T> self, Func<T, R> Just, Func<R> Nothing)
        {
            if (Just == null) throw new ArgumentNullException("Just");
            if (Nothing == null) throw new ArgumentNullException("Nothing");
            return () =>
            {
                var res = self();
                if (res.HasValue)
                    return Just(res.Value);
                else
                    return Nothing();
            };
        }

        /// <summary>
        /// Executes the delegate related to the derived Option type.
        /// </summary>
        public static Func<R> Match<T, R>(this Option<T> self, Func<R> Just, R Nothing)
        {
            if (Just == null) throw new ArgumentNullException("Just");
            return () =>
            {
                var res = self();
                if (res.HasValue)
                    return Just();
                else
                    return Nothing;
            };
        }

        /// <summary>
        /// Executes the delegate related to the derived Option type.
        /// </summary>
        public static Func<R> Match<T, R>(this Option<T> self, Func<T, R> Just, R Nothing)
        {
            if (Just == null) throw new ArgumentNullException("Just");
            return () =>
            {
                var res = self();
                if (res.HasValue)
                    return Just(res.Value);
                else
                    return Nothing;
            };
        }

        /// <summary>
        /// Monadic append
        /// If the lhs or rhs are in a Nothing state then Nothing propagates
        /// </summary>
        public static Option<T> Mappend<T>(this Option<T> self, Option<T> rhs)
        {
            if (rhs == null) throw new ArgumentNullException("rhs");
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
        public static IEnumerable<T> AsEnumerableInfinite<T>(this Option<T> self)
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