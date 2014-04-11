using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec
{
	public class Parser<A>
	{
        public readonly Func<IEnumerable<ParserChar>,  ParserResult<A>> Value;

        public Parser(Func<IEnumerable<ParserChar>, ParserResult<A>> func)
		{
			this.Value = func;
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
	}
}