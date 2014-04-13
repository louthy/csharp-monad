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
                    if (res.IsFaulted) 
                        return res;

                    var fst = res.Value.First();
                    if (!predicate(fst.Item1))
                        return ParserResult.Fail<T>(new ParserError[0]);

                    return res;
                });
        }

        public static Parser<U> Select<T, U>(this Parser<T> self, Func<T, U> select)
        {
            return new Parser<U>(
                input =>
                {
                    var res = self.Parse(input);
                    if (res.IsFaulted)
                        return ParserResult.Fail<U>(res.Errors);

                    var fst = res.Value.First();
                    return ParserResult.Success(
                        Tuple.Create<U, IEnumerable<ParserChar>>(
                            select(fst.Item1), fst.Item2).Cons()
                        );
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
                    if (res.IsFaulted) 
                        return ParserResult.Fail<V>(res.Errors);

                    var fst = res.Value.First();
                    var res2 = bind(fst.Item1).Parse(fst.Item2);
                    if (res2.IsFaulted)
                        return ParserResult.Fail<V>(res2.Errors);

                    var snd = res2.Value.First();
                    return ParserResult.Success<V>(
                        Tuple.Create<V, IEnumerable<ParserChar>>(select(fst.Item1, snd.Item1), snd.Item2).Cons()
                        );
                }
            );
        }

        public static Parser<T> Or<T>(this Parser<T> self, Parser<T> alternative)
        {
            return new Parser<T>(
                input =>
                {
                    var res = self.Parse(input);
                    if (res.IsFaulted)
                        return alternative.Parse(input);
                    else
                        return res;
                });
        }

        public static Parser<U> And<T,U>(this Parser<T> self, Parser<U> also)
        {
            return new Parser<U>(
                input =>
                {
                    var res = self.Parse(input);
                    if (res.IsFaulted)
                        return ParserResult.Fail<U>(res.Errors);
                    else
                        return also.Parse(res.Value.First().Item2);
                });
        }

        public static Parser<T> Fail<T>(this Parser<T> self, string expected, string message = "")
        {
            return new Parser<T>(
                    input =>
                    {
                        var res = self.Parse(input);
                        return res.IsFaulted
                            ? ParserResult.Fail<T>(ParserError.Create(expected, input, message).Cons(res.Errors))
                            : res;
                    }
                );
        }

        public static bool IsEqualTo(this IEnumerable<ParserChar> self, string rhs)
        {
            return self.Count() != rhs.Length
                ? false
                : self.Zip(rhs, (left, right) => left.Value == right)
                      .Where(match => match == false)
                      .Count() == 0;
        }

        public static bool IsNotEqualTo(this IEnumerable<ParserChar> self, string rhs)
        {
            return !self.IsEqualTo(rhs);
        }
    }
}
