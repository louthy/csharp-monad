using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec
{
    public static class ParserExt
    {
        public static Parser<T> Where<T>(this Parser<T> self, Func<T, bool> predicate)
        {
            return new Parser<T>(
                input =>
                {
                    var res = self.Parse(input);
                    if (res.Count() == 0) return null;
                    var fst = res.First();
                    if (!predicate(fst.Item1)) return null;
                    return res;
                });
        }

        public static Parser<U> Select<T, U>(this Parser<T> self, Func<T, U> select)
        {
            return new Parser<U>(
                input =>
                {
                    var res = self.Parse(input);
                    if (res.Count() == 0) return Empty.Return<U>();
                    var fst = res.First();
                    return Tuple.Create<U, IEnumerable<char>>(select(fst.Item1), fst.Item2).Cons();
                });
        }

        public static Parser<V> SelectMany<T, U, V>(
            this Parser<T> parser,
            Func<T, Parser<U>> bind,
            Func<T, U, V> select
            )
        {
            return new Parser<V>(
                input =>
                {
                    var res = parser.Parse(input);
                    if (res.Count() == 0) return Empty.Return<V>();
                    var fst = res.First();
                    var res2 = bind(fst.Item1).Parse(fst.Item2);
                    if (res2.Count() == 0) return Empty.Return<V>();
                    var snd = res2.First();
                    return Tuple.Create<V, IEnumerable<char>>(select(fst.Item1, snd.Item1), snd.Item2).Cons();
                }
            );
        }
    }
}
