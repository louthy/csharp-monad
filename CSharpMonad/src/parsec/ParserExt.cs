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

        public static Parser<R> Switch<T,R>(this Parser<T> self, Func<T,R> result, string expected, string message = "")
        {
            return new Parser<R>(
                    input =>
                    {
                        var res = self.Parse(input);
                        return res.IsFaulted
                            ? ParserResult.Fail<R>(ParserError.Create(expected, input, message).Cons(res.Errors))
                            : new ParserResult<R>(Tuple.Create(result(res.Value.First().Item1),res.Value.First().Item2).Cons() );
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
