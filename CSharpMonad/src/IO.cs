using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static IO<R> Bind<T, R>(this IO<T> self, Func<T, IO<R>> func)
        {
            return func(self());
        }

        /// <summary>
        /// Extension *bind* method for the IO delegate
        /// </summary>
        public static IO<V> SelectMany<T, U, V>(
            this IO<T> self,
            Func<T, IO<U>> k,
            Func<T, U, V> m)
        {
            return self.Bind(
                t => k(t).Bind(
                    u => new IO<V>(() => m(t, u))
                    )
                );
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
            return func;
        }
    }
}