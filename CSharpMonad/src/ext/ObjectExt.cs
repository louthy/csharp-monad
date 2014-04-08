using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad
{
    public static class ObjectExt
    {
        public static IEnumerable<T> Cons<T>(this T x, IEnumerable<T> xs)
        {
            yield return x;
            foreach (var item in xs) yield return item;
        }

        public static IEnumerable<T> Cons<T>(this T x)
        {
            yield return x;
        }
    }
}
