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
using Monad.Utility;

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

                    foreach (var fst in res.Value)
                    {
                        if (!predicate(fst.Item1))
                            return ParserResult.Fail<T>(new ParserError[0]);
                    }
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

                    var resT = res.Value;
                    var left = resT.IsEmpty
                        ? input
                        : resT.Last().Item2;

                    return ParserResult.Success(
                        resT.Select(resU =>
                            Tuple.Create<U, ImmutableList<ParserChar>>(select(resU.Item1), left)
                        ));
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
                    if (parser == null)
                    {
                        return ParserResult.Fail<V>("parser is null",input);
                    }

                    var res = parser.Parse(input);
                    if (res.IsFaulted) 
                        return ParserResult.Fail<V>(res.Errors);

                    var left = res.Value.IsEmpty
                        ? input
                        : res.Value.Last().Item2;

                    var resV = new List<Tuple<V, ImmutableList<ParserChar>>>();
                    foreach (var resT in res.Value)
                    {
                        var resUTmp = bind(resT.Item1).Parse(left);

                        if (resUTmp.IsFaulted)
                            return ParserResult.Fail<V>(resUTmp.Errors);

                        left = resUTmp.Value.IsEmpty
                            ? input
                            : resUTmp.Value.Last().Item2;

                        foreach (var resU in resUTmp.Value)
                        {
                            resV.Add(Tuple.Create<V, ImmutableList<ParserChar>>(select(resT.Item1, resU.Item1), left));
                        }
                    }
                    return ParserResult.Success<V>(resV);
                }
            );
        }

        public static Parser<T> Or<T>(this Parser<T> self, Parser<T> alternative)
        {
            return new Parser<T>(
                input =>
                {
                    var res = self.Parse(input);
                    if (res.IsFaulted || res.Value.IsEmpty)
                        return alternative.Parse(input);
                    else
                        return res;
                });
        }

        public static Parser<U> And<T, U>(this Parser<T> self, Parser<U> also)
        {
            return new Parser<U>(
                input =>
                {
                    var res = self.Parse(input);
                    if (res.IsFaulted || res.Value.IsEmpty)
                        return ParserResult.Fail<U>(res.Errors);
                    else
                    {
                        return also.Parse(res.Value.Last().Item2);
                    }
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
                            : new ParserResult<R>(res.Value.Select( fst => Tuple.Create(result(fst.Item1),fst.Item2)));
                    }
                );
        }

        public static bool IsEqualTo(this ImmutableList<ParserChar> self, string rhs)
        {
            return self.Count() != rhs.Length
                ? false
                : self.Zip(rhs.Cast<char>(), (left, right) => left.Value == right)
                      .Where(match => match == false)
                      .Count() == 0;
        }

        public static bool IsNotEqualTo(this ImmutableList<ParserChar> self, string rhs)
        {
            return !self.IsEqualTo(rhs);
        }
    }
}
