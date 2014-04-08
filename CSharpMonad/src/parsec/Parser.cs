using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec
{
	public class Parser<A>
	{
		public readonly Func<IEnumerable<char>,IEnumerable<Tuple<A,IEnumerable<char>>>> Value;

		public Parser(Func<IEnumerable<char>,IEnumerable<Tuple<A,IEnumerable<char>>>> func)
		{
			this.Value = func;
		}

		public IEnumerable<Tuple<A,IEnumerable<char>>> Parse(IEnumerable<char> input)
		{
			return Value(input);
		}

		public static Parser<A> Create(Func<IEnumerable<char>,IEnumerable<Tuple<A,IEnumerable<char>>>> func)
		{
			return new Parser<A>(func);
		}
	}
}