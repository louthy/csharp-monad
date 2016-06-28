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
    /// Option monad
    /// </summary>
    public abstract class OptionStrict<T> : IEquatable<OptionStrict<T>>, IEquatable<T>
    {
        /// <summary>
        /// Represents a Option monad without a value
        /// </summary>
        public readonly static OptionStrict<T> Nothing = new NothingStrict<T>();

        /// <summary>
        /// Conversion from any value to Option<T>
        /// Null is always considered as Nothing
        /// </summary>
        public static implicit operator OptionStrict<T>(T value)
        {
            return value.ToOptionStrict();
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

        /// <summary>
        /// Executes the delegate related to the derived Option type.
        /// </summary>
        public abstract R Match<R>(Func<R> Just, Func<R> Nothing);

        /// <summary>
        /// Executes the delegate related to the derived Option type.
        /// </summary>
        public abstract R Match<R>(Func<T, R> Just, Func<R> Nothing);

        /// <summary>
        /// Executes the delegate related to the derived Option type.
        /// </summary>
        public abstract R Match<R>(Func<R> Just, R Nothing);

        /// <summary>
        /// Executes the delegate related to the derived Option type.
        /// </summary>
        public abstract R Match<R>(Func<T, R> Just, R Nothing);

        /// <summary>
        /// Mappend
        /// If the left-hand side or right-hand side are in a Left state, then Left propogates
        /// </summary>
        public static OptionStrict<T> operator +(OptionStrict<T> lhs, OptionStrict<T> rhs)
        {
            if (lhs == null) throw new ArgumentNullException("lhs");
            if (rhs == null) throw new ArgumentNullException("rhs");
            return lhs.Mappend(rhs);
        }

        /// <summary>
        /// Nothing coalescing operator
        /// Returns the left-hand operand if the operand is not Nothing; otherwise it returns the right hand operand.
        /// In other words it returns the first valid option in the operand sequence.
        /// </summary>
        public static OptionStrict<T> operator |(OptionStrict<T> lhs, OptionStrict<T> rhs)
        {
            if (lhs == null) throw new ArgumentNullException("lhs");
            if (rhs == null) throw new ArgumentNullException("rhs");

            return lhs.HasValue
                ? lhs
                : rhs;
        }

        /// <summary>
        /// Returns the right-hand side if the left-hand and right-hand side are not Nothing.
        /// In order words every operand must hold a value for the result to be Just.
        /// </summary>
        public static OptionStrict<T> operator &(OptionStrict<T> lhs, OptionStrict<T> rhs)
        {
            if (lhs == null) throw new ArgumentNullException("lhs");
            if (rhs == null) throw new ArgumentNullException("rhs");

            return lhs.HasValue && rhs.HasValue
                ? rhs
                : lhs.HasValue
                    ? rhs
                    : lhs;
        }

        /// <summary>
        /// Equals override
        /// Compares the underlying values
        /// </summary>
        public static bool operator ==(OptionStrict<T> lhs, OptionStrict<T> rhs)
        {
            if (ReferenceEquals(null, lhs)) throw new ArgumentNullException("lhs");
            if (ReferenceEquals(null, rhs)) throw new ArgumentNullException("rhs");

            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Not equals override
        /// Compares the underlying values
        /// </summary>
        public static bool operator !=(OptionStrict<T> lhs, OptionStrict<T> rhs)
        {
            if (lhs == null) throw new ArgumentNullException("lhs");
            if (rhs == null) throw new ArgumentNullException("rhs");

            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// Monadic append
        /// If the lhs or rhs are in a Nothing state then Nothing propagates
        /// </summary>
        public abstract OptionStrict<T> Mappend(OptionStrict<T> rhs);

        /// <summary>
        /// Converts the Option to an enumerable
        /// </summary>
        /// <returns>
        /// Just: A list with one T in
        /// Nothing: An empty list
        /// </returns>
        public IEnumerable<T> AsEnumerable()
        {
            if (HasValue)
                yield return Value;
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
        public IEnumerable<T> AsEnumerableInfinite()
        {
            if (HasValue)
                while (true) yield return Value;
            else
                yield break;
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns>Just.GetHashCode() or Nothing.GetHashCode()</returns>
        public override int GetHashCode()
        {
            return HasValue
                ? Value == null ? 0 : Value.GetHashCode()
                : Nothing.GetHashCode();
        }

        /// <summary>
        /// Equals override
        /// Compares the underlying values
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                if (obj is OptionStrict<T>)
                {
                    var rhs = (OptionStrict<T>)obj;
                    return HasValue && rhs.HasValue
                        ? Value.Equals(rhs.Value)
                        : !HasValue && !rhs.HasValue
                            ? true
                            : false;
                }
                else if (obj is T)
                {
                    var rhs = (T)obj;
                    return HasValue
                        ? Value.Equals(rhs)
                        : false;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Equals override
        /// Compares the underlying values
        /// </summary>
        public bool Equals(OptionStrict<T> rhs)
        {
            return Equals((object)rhs);
        }

        /// <summary>
        /// Equals override
        /// Compares the underlying values
        /// </summary>
        public bool Equals(T rhs)
        {
            return Equals((object)rhs);
        }
    }

    /// <summary>
    /// Option<T> extensions
    /// </summary>
    public static class OptionStrict
    {
        /// <summary>
        /// Monadic zero
        /// </summary>
        public static OptionStrict<T> Mempty<T>()
        {
            return new JustStrict<T>(default(T));
        }
    }

    /// <summary>
    /// Option<T> monad extension methods
    /// </summary>
    public static class OptionStrictExtensions
    {
        /// <summary>
        /// Converts this object to a Option monad.
        /// Tretas null as Nothing
        /// </summary>
        /// <returns>Option<T></returns>
        public static OptionStrict<T> ToOptionStrict<T>(this T self)
        {
            return self == null
                ? OptionStrict<T>.Nothing
                : new JustStrict<T>(self);
        }

        public static OptionStrict<R> Select<T, R>(this OptionStrict<T> self, Func<T, R> map)
        {
            if (map == null) throw new ArgumentNullException("map");
            return self.HasValue
                ? map(self.Value).ToOptionStrict()
                : OptionStrict<R>.Nothing;
        }

        public static OptionStrict<U> SelectMany<T, U>(this OptionStrict<T> self, Func<T, OptionStrict<U>> k)
        {
            if (k == null) throw new ArgumentNullException("k");

            return self.HasValue
                ? k(self.Value)
                : OptionStrict<U>.Nothing;
        }

        public static OptionStrict<V> SelectMany<T, U, V>(
            this OptionStrict<T> self,
            Func<T, OptionStrict<U>> select,
            Func<T, U, V> project
            )
        {
            if (select == null) throw new ArgumentNullException("select");
            if (project == null) throw new ArgumentNullException("project");

            if (!self.HasValue)
                return OptionStrict<V>.Nothing;

            var res = select(self.Value);
            if (!res.HasValue)
                return OptionStrict<V>.Nothing;

            return new JustStrict<V>(project(self.Value, res.Value));
        }

        /// <summary>
        /// Mconcat
        /// </summary>
        public static OptionStrict<T> Mconcat<T>(this IEnumerable<OptionStrict<T>> ms)
        {
            var value = ms.Head();

            foreach (var m in ms.Tail())
            {
                if (!value.HasValue)
                    return value;

                value = value.Mappend(m);
            }
            return value;
        }
    }
}