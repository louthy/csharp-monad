using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad
{
    /// <summary>
    /// Create a memoised value
    /// </summary>
    public static class Memo
    {
        /// <summary>
        /// Create a memoised (cached) value, doesn't calculate the provided expression until the
        /// value is required, and once it has been calculated it will cache the value for future
        /// access.
        /// </summary>
        public static Memo<T> ise<T>(Func<T> func)
        {
            return new Memo<T>(func);
        }
    }

    /// <summary>
    /// Create a memoised (cached) value, doesn't calculate the provided expression until the
    /// value is required, and once it has been calculated it will cache the value for future
    /// access.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Memo<T>
    {
        readonly Func<T> func;
        WeakReference reference;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="func">Function to invoke to get the value when needed</param>
        public Memo(Func<T> func)
        {
            this.func = func;
        }

        /// <summary>
        /// Access the value (this will cause the calculation if the value hasn't yet
        /// been memoised).
        /// </summary>
        public T Value
        {
            get
            {
                if (reference == null)
                {
                    reference = new WeakReference(func());
                }
                else if (!reference.IsAlive)
                {
                    reference.Target = func();
                }
                return (T)reference.Target;
            }
        }

        public bool HasValue
        {
            get
            {
                return reference != null && reference.IsAlive;
            }
        }

        /// <summary>
        /// Forget the memoised value
        /// </summary>
        public void Forget()
        {
            reference = null;
        }

        /// <summary>
        /// Allows implicit conversion from the Memo<T> to T.
        /// </summary>
        public static implicit operator T(Memo<T> memo)
        {
            return memo.Value;
        }
    }

    /// <summary>
    /// Extension method for Func<T,R>
    /// </summary>
    public static class MemoExt
    {
        /// <summary>
        /// Memoise the function
        /// </summary>
        public static Func<T, R> Memo<T, R>(this Func<T, R> f)
        {
            Dictionary<T, R> cache = new Dictionary<T, R>();
            return t => cache.ContainsKey(t)
                ? cache[t]
                : (cache[t] = f(t));
        }

        /// <summary>
        /// Memoise the function
        /// </summary>
        public static Func<T1, T2, R> Memo<T1, T2, R>(this Func<T1, T2, R> f)
        {
            Func<T1, Func<T2, R>> del = (T1 t1) => (T2 t2) => f(t1, t2);
            del = del.Memo();
            return (T1 t1, T2 t2) => del(t1)(t2);
        }

        /// <summary>
        /// Memoise the function
        /// </summary>
        public static Func<T1, T2, T3, R> Memo<T1, T2, T3, R>(this Func<T1, T2, T3, R> f)
        {
            Func<T1, Func<T2, Func<T3, R>>> del = (T1 t1) => (T2 t2) => (T3 t3) => f(t1, t2, t3);
            del = del.Memo();
            return (T1 t1, T2 t2, T3 t3) => del(t1)(t2)(t3);
        }
    }
}
