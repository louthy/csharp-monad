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
    /// The error monad delegate
    /// </summary>
    public delegate ErrorResult<T> Error<T>();

    /// <summary>
    /// Holds the state of the error monad during the bind function
    /// If IsFaulted == true then the bind function will be cancelled.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ErrorResult<T>
    {
        public readonly T Value;
        public readonly Exception Exception;

        /// <summary>
        /// Ctor
        /// </summary>
        public ErrorResult(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public ErrorResult(Exception e)
        {
            Exception  = e;
        }

        public static implicit operator ErrorResult<T>(T value)
        {
            return new ErrorResult<T>(value);
        }

        /// <summary>
        /// True if faulted
        /// </summary>
        public bool IsFaulted
        {
            get
            {
                return Exception != null;
            }
        }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return IsFaulted
                ? Exception.ToString()
                : Value != null
                    ? Value.ToString()
                    : "[null]";
        }
    }

    /// <summary>
    /// Extension methods for the error monad
    /// </summary>
    public static class ErrorExt
    {
        /// <summary>
        /// Return a valid value regardless of the faulted state
        /// </summary>
        public static T GetValueOrDefault<T>(this Error<T> self)
        {
            var res = self.Return();
            if (res.IsFaulted)
                return default(T);
            else
                return res.Value;
        }

        /// <summary>
        /// Return the Value of the monad.  Note this will throw an InvalidOperationException if
        /// the monad is in a faulted state.
        /// </summary>
        public static T GetValue<T>(this Error<T> self)
        {
            var res = self();
            if (res.IsFaulted)
                throw new InvalidOperationException("The error monad has no value.  It holds an exception of type: "+res.GetType().Name+".");
            else
                return res.Value;
        }

        /// <summary>
        /// Invokes the bind function and returns the monad state
        /// </summary>
        public static ErrorResult<T> Return<T>(this Error<T> self)
        {
            try
            {
                var mdel = (MulticastDelegate)self;
                var invocationList = mdel.GetInvocationList();

                if (invocationList.Count() > 1)
                {
                    return invocationList.Select(del => (Error<T>)del).Mconcat().Return();
                }
                else
                {
                    return self();
                }
            }
            catch (Exception e)
            {
                return new ErrorResult<T>(e);
            }
        }

        /// <summary>
        /// LINQ select override
        /// </summary>
        public static Error<U> Select<T, U>(this Error<T> self, Func<T, U> select)
        {
            return new Error<U>( () =>
                {
                    ErrorResult<T> resT;
                    try
                    {
                        resT = self();
                        if (resT.IsFaulted)
                            return new ErrorResult<U>(resT.Exception);
                    }
                    catch(Exception e)
                    {
                        return new ErrorResult<U>(e);
                    }

                    U resU;
                    try
                    {
                        resU = select(resT.Value);
                    }
                    catch (Exception e)
                    {
                        return new ErrorResult<U>(e);
                    }

                    return new ErrorResult<U>(resU);
                });
        }

        /// <summary>
        /// LINQ select-many override
        /// </summary>
        public static Error<V> SelectMany<T, U, V>(
            this Error<T> self,
            Func<T, Error<U>> select,
            Func<T, U, V> bind
            )
        {
            return new Error<V>(
                () =>
                {
                    ErrorResult<T> resT;
                    try
                    {
                        resT = self();
                        if( resT.IsFaulted )
                            return new ErrorResult<V>(resT.Exception);
                    }
                    catch (Exception e)
                    {
                        return new ErrorResult<V>(e);
                    }

                    ErrorResult<U> resU;
                    try
                    {
                        resU = select(resT.Value)();
                        if (resU.IsFaulted)
                            return new ErrorResult<V>(resU.Exception);
                    }
                    catch (Exception e)
                    {
                        return new ErrorResult<V>(e);
                    }

                    V resV;
                    try
                    {
                        resV = bind(resT.Value, resU.Value);
                    }
                    catch (Exception e)
                    {
                        return new ErrorResult<V>(e);
                    }

                    return new ErrorResult<V>(resV);
                }
            );
        }

        /// <summary>
        /// Allows fluent chaining of Error monads
        /// </summary>
        public static Error<U> Then<T, U>(this Error<T> self, Func<T, U> getValue)
        {
            var resT = self();

            return resT.IsFaulted
                ? new Error<U>( () => new ErrorResult<U>(resT.Exception) )
                : new Error<U>( () => 
                    {
                        try
                        {
                            U resU = getValue(resT.Value);
                            return new ErrorResult<U>(resU);
                        }
                        catch (Exception e)
                        {
                            return new ErrorResult<U>(e);
                        }
                    });
        }

        /// <summary>
        /// Converts the Error to an enumerable of T
        /// </summary>
        /// <returns>
        /// Success: A list with one T in
        /// Error: An empty list
        /// </returns>
        public static IEnumerable<T> AsEnumerable<T>(this Error<T> self)
        {
            var res = self.Return();
            if (res.IsFaulted)
                yield break;
            else
                yield return res.Value;
        }

        /// <summary>
        /// Converts the Error to an infinite enumerable of T
        /// </summary>
        /// <returns>
        /// Success: An infinite list of T
        /// Error: An empty list
        /// </returns>
        public static IEnumerable<T> AsEnumerableInfinite<T>(this Error<T> self)
        {
            var res = self.Return();
            if (res.IsFaulted)
                yield break;
            else
                while (true) yield return res.Value;
        }

        /// <summary>
        /// Mappend
        /// </summary>
        public static Error<T> Mappend<T>(this Error<T> lhs, Error<T> rhs)
        {
            return () =>
            {
                var lhsValue = lhs.Return();
                if (lhsValue.IsFaulted) return lhsValue;

                var rhsValue = rhs.Return();
                if (rhsValue.IsFaulted) return rhsValue;

                bool IsAppendable = typeof(IAppendable<T>).IsAssignableFrom(typeof(T));

                if (IsAppendable)
                {
                    var lhsAppendValue = lhsValue.Value as IAppendable<T>;
                    return lhsAppendValue.Append(rhsValue.Value);
                }
                else
                {
                    string TypeOfT = typeof(T).ToString();

                    // TODO: Consider replacing this with a static Reflection.Emit which does this job efficiently.
                    switch (TypeOfT)
                    {
                        case "System.Int64":
                            return (T)Convert.ChangeType((Convert.ToInt64(lhsValue.Value) + Convert.ToInt64(rhsValue.Value)), typeof(T));
                        case "System.UInt64":
                            return (T)Convert.ChangeType((Convert.ToUInt64(lhsValue.Value) + Convert.ToUInt64(rhsValue.Value)), typeof(T));
                        case "System.Int32":
                            return (T)Convert.ChangeType((Convert.ToInt32(lhsValue.Value) + Convert.ToInt32(rhsValue.Value)), typeof(T));
                        case "System.UInt32":
                            return (T)Convert.ChangeType((Convert.ToUInt32(lhsValue.Value) + Convert.ToUInt32(rhsValue.Value)), typeof(T));
                        case "System.Int16":
                            return (T)Convert.ChangeType((Convert.ToInt16(lhsValue.Value) + Convert.ToInt16(rhsValue.Value)), typeof(T));
                        case "System.UInt16":
                            return (T)Convert.ChangeType((Convert.ToUInt16(lhsValue.Value) + Convert.ToUInt16(rhsValue.Value)), typeof(T));
                        case "System.Decimal":
                            return (T)Convert.ChangeType((Convert.ToDecimal(lhsValue.Value) + Convert.ToDecimal(rhsValue.Value)), typeof(T));
                        case "System.Double":
                            return (T)Convert.ChangeType((Convert.ToDouble(lhsValue.Value) + Convert.ToDouble(rhsValue.Value)), typeof(T));
                        case "System.Single":
                            return (T)Convert.ChangeType((Convert.ToSingle(lhsValue.Value) + Convert.ToSingle(rhsValue.Value)), typeof(T));
                        case "System.Char":
                            return (T)Convert.ChangeType((Convert.ToChar(lhsValue.Value) + Convert.ToChar(rhsValue.Value)), typeof(T));
                        case "System.Byte":
                            return (T)Convert.ChangeType((Convert.ToByte(lhsValue.Value) + Convert.ToByte(rhsValue.Value)), typeof(T));
                        case "System.String":
                            return (T)Convert.ChangeType((Convert.ToString(lhsValue.Value) + Convert.ToString(rhsValue.Value)), typeof(T));
                        default:
                            throw new InvalidOperationException("Type " + typeof(T).Name + " is not appendable.  Consider implementing the IAppendable interface.");
                    }
                }
            };
        }

        /// <summary>
        /// Mconcat
        /// </summary>
        public static Error<T> Mconcat<T>(this IEnumerable<Error<T>> ms)
        {
            var value = ms.Head();

            foreach (var m in ms.Tail())
            {
                value = value.Mappend(m);
            }
            return value;
        }

        /// <summary>
        /// Pattern matching
        /// </summary>
        public static R Match<T,R>(this Error<T> self, Func<T,R> Success, Func<Exception,R> Fail )
        {
            var res = self.Return();
            return res.IsFaulted
                ? Fail(res.Exception)
                : Success(res.Value);
        }

        /// <summary>
        /// Pattern matching
        /// </summary>
        public static R Match<T, R>(this Error<T> self, Func<T, R> Success)
        {
            var res = self.Return();
            return res.IsFaulted
                ? default(R)
                : Success(res.Value);
        }

        /// <summary>
        /// Pattern matching
        /// </summary>
        public static Unit Match<T>(this Error<T> self, Action<T> Success, Action<Exception> Fail)
        {
            var res = self.Return();

            if (res.IsFaulted)
                Fail(res.Exception);
            else
                Success(res.Value);

            return Unit.Return();
        }

        /// <summary>
        /// Pattern matching
        /// </summary>
        public static Unit Match<T>(this Error<T> self, Action<T> Success)
        {
            var res = self.Return();
            if( !res.IsFaulted )
                Success(res.Value);
            return Unit.Return();
        }
    }

    public class Error
    {
        /// <summary>
        /// Mempty
        /// </summary>
        public static Error<T> Mempty<T>()
        {
            return () => default(T);
        }
    }
}