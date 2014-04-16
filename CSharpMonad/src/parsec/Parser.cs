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
    public interface IParser<out A>
    {
    }

	public class Parser<A> : IParser<A>
	{
        public readonly Func<IEnumerable<ParserChar>,  ParserResult<A>> Value;

        public Parser(Func<IEnumerable<ParserChar>, ParserResult<A>> func)
		{
            // This memoisation will only work if the exact same reference
            // is passed to Parse.  We could obviously hash the entire enumerable
            // but I suspect that would negate any benefits from the memoisation.
			this.Value = func.Memo();
		}

        public ParserResult<A> Parse(IEnumerable<ParserChar> input)
		{
			return Value(input);
		}

        public ParserResult<A> Parse(IEnumerable<char> input)
        {
            return Parse(input.ToParserChar());
        }

        public static Parser<A> Create(Func<IEnumerable<ParserChar>, ParserResult<A>> func)
		{
			return new Parser<A>(func);
		}

        public static implicit operator Parser<A>(Func<IEnumerable<ParserChar>, ParserResult<A>> func)
        {
            return new Parser<A>(func);
        }

        public static implicit operator A(Parser<A> parser)
        {
            return from p in parser
                   select p;
        }

        public static Parser<A> operator |(Parser<A> lhs, Parser<A> rhs) 
        {
            return lhs.Or(rhs);
        }

        public static Parser<A> operator &(Parser<A> lhs, Parser<A> rhs)
        {
            return lhs.Or(rhs);
        }

        public Parser<IEnumerable<A>> Mconcat( IEnumerable<Parser<A>> parsers )
        {
            return new Parser<IEnumerable<A>>(
                inp =>
                {
                    parsers = this.Cons(parsers);

                    var final = new A[0].AsEnumerable();
                    var last = inp;

                    foreach (var parser in parsers)
                    {
                        var res = parser.Parse(inp);
                        if (res.IsFaulted)
                            return new ParserResult<IEnumerable<A>>(res.Errors);

                        final = final.Concat(res.Value.Select(r => r.Item1));
                        if (!res.Value.IsEmpty())
                            last = res.Value.Last().Item2;
                    }

                    return new ParserResult<IEnumerable<A>>(Tuple.Create(final, last).Cons());
                });
        }

        public Parser<IEnumerable<A>> Mconcat(params Parser<A>[] parsers)
        {
            return Mconcat(parsers.AsEnumerable());
        }
    }
}