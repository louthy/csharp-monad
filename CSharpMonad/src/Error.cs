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
                return self();
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
                    }
                    catch (Exception e)
                    {
                        return new ErrorResult<V>(e);
                    }

                    ErrorResult<U> resU;
                    try
                    {
                        resU = select(resT.Value)();
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
    }
}