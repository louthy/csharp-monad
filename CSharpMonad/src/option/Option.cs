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
    /// Option monad
    /// </summary>
    public abstract class Option<T> : IEquatable<Option<T>>, IEquatable<T>
    {
        /// <summary>
        /// Represents a Option monad without a value
        /// </summary>
        public readonly static Option<T> Nothing = new Nothing<T>();

        /// <summary>
        /// Conversion from any value to Option<T>
        /// Null is always considered as Nothing
        /// </summary>
        public static implicit operator Option<T>(T value)
        {
            return value.ToOption();
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
        /// Monadic append
        /// If the left-hand side or right-hand side are in a Left state, then Left propogates
        /// </summary>
        public static Option<T> operator +(Option<T> lhs, Option<T> rhs)
        {
            return lhs.Mappend(rhs);
        }

        /// <summary>
        /// Monadic equality
        /// </summary>
        public static bool operator ==(Option<T> lhs, Option<T> rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Monadic equality
        /// </summary>
        public static bool operator !=(Option<T> lhs, Option<T> rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// Monadic append
        /// If the lhs or rhs are in a Nothing state then Nothing propagates
        /// </summary>
        public abstract Option<T> Mappend(Option<T> rhs);

        /// <summary>
        /// Monadic zero
        /// </summary>
        public static Option<T> Mempty()
        {
            return new Just<T>(default(T));
        }

        public override int GetHashCode()
        {
            return HasValue
                ? Value == null ? 0 : Value.GetHashCode()
                : Nothing.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                if (obj is Option<T>)
                {
                    var rhs = (Option<T>)obj;
                    return HasValue && rhs.HasValue
                        ? Value.Equals(rhs.Value)
                        : !HasValue && !rhs.HasValue
                            ? true
                            : false;
                }
                else if( obj is T )
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

        public bool Equals(Option<T> rhs)
        {
            return Equals((object)rhs);
        }

        public bool Equals(T rhs)
        {
            return Equals((object)rhs);
        }
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
                ? Option<T>.Nothing
                : new Just<T>(self);
        }

        public static Option<R> Select<T, R>(this Option<T> self, Func<T, R> map)
        {
            return self.HasValue
                ? map(self.Value).ToOption()
                : Option<R>.Nothing;
        }

        public static Option<U> SelectMany<T, U>(this Option<T> self, Func<T, Option<U>> k)
        {
            return self.HasValue
                ? k(self.Value)
                : Option<U>.Nothing;
        }

        public static Option<V> SelectMany<T, U, V>(this Option<T> self, Func<T, Option<U>> k, Func<T, U, V> s)
        {
            return self.HasValue
                ? s(self.Value, k(self.Value).Value).ToOption()
                : Option<V>.Nothing;
        }
    }
}