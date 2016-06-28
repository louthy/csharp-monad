using System;

namespace Monad.Utility
{
    /// <summary>
    /// Type inference helper
    /// </summary>
    public static class Lam
    {
        public static Func<R> da<R>(Func<R> fn)
        {
            if (fn == null) throw new ArgumentNullException("fn");
            return fn;
        }

        public static Func<T, R> da<T,R>(Func<T, R> fn)
        {
            if (fn == null) throw new ArgumentNullException("fn");
            return fn;
        }

        public static Func<T1, T2, R> da<T1, T2, R>(Func<T1, T2, R> fn)
        {
            if (fn == null) throw new ArgumentNullException("fn");
            return fn;
        }

        public static Func<T1, T2, T3, R> da<T1, T2, T3, R>(Func<T1, T2, T3, R> fn)
        {
            if (fn == null) throw new ArgumentNullException("fn");
            return fn;
        }

        public static Func<T1, T2, T3, T4, R> da<T1, T2, T3, T4, R>(Func<T1, T2, T3, T4, R> fn)
        {
            if (fn == null) throw new ArgumentNullException("fn");
            return fn;
        }
    }
}
