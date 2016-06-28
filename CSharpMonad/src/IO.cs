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
using System.Reflection;

namespace Monad
{
    /// <summary>
    /// IO monad
    /// </summary>
    public delegate T IO<T>();

    /// <summary>
    /// IO monad extension methds
    /// </summary>
    public static class IOMonadExtensions
    {
        /// <summary>
        /// Monadic bind method for the IO delegate
        /// </summary>
        public static IO<R> SelectMany<T, R>(this IO<T> self, Func<T, IO<R>> func)
        {
            if (func == null) throw new ArgumentNullException("func");
            return func(self());
        }

        /// <summary>
        /// Select
        /// </summary>
        public static IO<U> Select<T,U>(this IO<T> self, Func<T,U> select)
        {
            if (select == null) throw new ArgumentNullException("select");
            return () => select(self());
        }

        /// <summary>
        /// SelectMany
        /// </summary>
        public static IO<V> SelectMany<T, U, V>(
            this IO<T> self,
            Func<T, IO<U>> select,
            Func<T, U, V> bind)
        {
            if (bind == null) throw new ArgumentNullException("bind");
            if (select == null) throw new ArgumentNullException("select");
            var resT = self();
            return () => bind(resT, select(resT)());
        }

        /// <summary>
        /// Allows fluent chaining of IO monads
        /// </summary>
        public static IO<U> Then<T, U>(this IO<T> self, Func<T, U> getValue)
        {
            if (getValue == null) throw new ArgumentNullException("getValue");
            return () => getValue(self.Invoke());
        }

        /// <summary>
        /// Mappend
        /// </summary>
        public static IO<T> Mappend<T>(this IO<T> lhs, IO<T> rhs)
        {
            if (rhs == null) throw new ArgumentNullException("rhs");
            return () =>
            {
                var lhsValue = lhs();
                var rhsValue = rhs();

                bool IsAppendable = typeof(IAppendable<T>).GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo());

                if (IsAppendable)
                {
                    var lhsAppendValue = lhsValue as IAppendable<T>;
                    return lhsAppendValue.Append(rhsValue);
                }
                else
                {
                    string TypeOfT = typeof(T).ToString();

                    // TODO: Consider replacing this with a static Reflection.Emit which does this job efficiently.
                    switch (TypeOfT)
                    {
                        case "System.Int64":
                            return (T)Convert.ChangeType((Convert.ToInt64(lhsValue) + Convert.ToInt64(rhsValue)), typeof(T));
                        case "System.UInt64":
                            return (T)Convert.ChangeType((Convert.ToUInt64(lhsValue) + Convert.ToUInt64(rhsValue)), typeof(T));
                        case "System.Int32":
                            return (T)Convert.ChangeType((Convert.ToInt32(lhsValue) + Convert.ToInt32(rhsValue)), typeof(T));
                        case "System.UInt32":
                            return (T)Convert.ChangeType((Convert.ToUInt32(lhsValue) + Convert.ToUInt32(rhsValue)), typeof(T));
                        case "System.Int16":
                            return (T)Convert.ChangeType((Convert.ToInt16(lhsValue) + Convert.ToInt16(rhsValue)), typeof(T));
                        case "System.UInt16":
                            return (T)Convert.ChangeType((Convert.ToUInt16(lhsValue) + Convert.ToUInt16(rhsValue)), typeof(T));
                        case "System.Decimal":
                            return (T)Convert.ChangeType((Convert.ToDecimal(lhsValue) + Convert.ToDecimal(rhsValue)), typeof(T));
                        case "System.Double":
                            return (T)Convert.ChangeType((Convert.ToDouble(lhsValue) + Convert.ToDouble(rhsValue)), typeof(T));
                        case "System.Single":
                            return (T)Convert.ChangeType((Convert.ToSingle(lhsValue) + Convert.ToSingle(rhsValue)), typeof(T));
                        case "System.Char":
                            return (T)Convert.ChangeType((Convert.ToChar(lhsValue) + Convert.ToChar(rhsValue)), typeof(T));
                        case "System.Byte":
                            return (T)Convert.ChangeType((Convert.ToByte(lhsValue) + Convert.ToByte(rhsValue)), typeof(T));
                        case "System.String":
                            return (T)Convert.ChangeType((Convert.ToString(lhsValue) + Convert.ToString(rhsValue)), typeof(T));
                        default:
                            throw new InvalidOperationException("Type " + typeof(T).Name + " is not appendable.  Consider implementing the IAppendable interface.");
                    }
                }
            };
        }

        /// <summary>
        /// Mconcat
        /// </summary>
        public static IO<T> Mconcat<T>(this IEnumerable<IO<T>> ms)
        {
            var value = ms.Head();

            foreach (var m in ms.Tail())
            {
                value = value.Mappend(m);
            }
            return value;
        }

        /// <summary>
        /// RunIO - NOTE: You don't need to use this unless you're using the + operator
        /// to append IO monads.  Return() will check for + and automatically combine the
        /// results.  Regular invocation cannot do that.
        /// </summary>
        public static T RunIO<T>(this IO<T> self)
        {
            var mdel = (MulticastDelegate)self;
            var invocationList = mdel.GetInvocationList();

            if (invocationList.Count() > 1)
            {
                return invocationList.Select(del => (IO<T>)del).Mconcat().RunIO();
            }
            else
            {
                return self();
            }
        }

        /// <summary>
        /// Memoize the result 
        /// </summary>
        public static Func<T> Memo<T>(this IO<T> self)
        {
            var res = self();
            return () => res;
        }

    }

    public static class IO
    {
        /// <summary>
        /// Mempty
        /// </summary>
        public static IO<T> Mempty<T>()
        {
            return () => default(T);
        }

        public static IO<T> Return<T>(IO<T> func)
        {
            if (func == null) throw new ArgumentNullException("func");
            return func;
        }
    }

    /// <summary>
    /// Helper method to make local var inference work, i.e:
    /// 
    ///		var m = I.O( ()=> File.ReadAllText(path) );
    ///		
    /// </summary>
    public static class I
    {
        public static IO<T> O<T>(IO<T> func)
        {
            if (func == null) throw new ArgumentNullException("func");
            return func;
        }
    }
}