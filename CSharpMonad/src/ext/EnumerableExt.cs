using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad
{
    public static class EnumerableExt
    {
        public static T Head<T>(this IEnumerable<T> self)
        {
            return self.First();
        }

        public static IEnumerable<T> Tail<T>(this IEnumerable<T> self)
        {
            return self.Skip(1);
        }

        public static string AsString(this IEnumerable<char> self)
        {
            return String.Concat(self);
        }

        public static T Second<T>(this IEnumerable<T> self)
        {
            return self.Tail().Head();
        }
    }
}
