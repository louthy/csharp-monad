using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec
{
    public static partial class New
    {
        public static Item Item()
        {
            return new Item();
        }
        public static Failure<T> Failure<T>(ParserError error)
        {
            return new Failure<T>(error);
        }
        public static Failure<T> Failure<T>(ParserError error, IEnumerable<ParserError> errors)
        {
            return new Failure<T>(error, errors);
        }
        public static Return<T> Return<T>(T v)
        {
            return new Return<T>(v);
        }
        public static Choice<A> Choice<A>(Parser<A> p, params Parser<A>[] ps)
        {
            return new Choice<A>(p, ps);
        }
        public static Satisfy Satisfy(Func<char, bool> predicate, string expecting)
        {
            return new Satisfy(predicate, expecting);
        }
        public static Digit Digit()
        {
            return new Digit();
        }
        public static Letter Letter()
        {
            return new Letter();
        }
        public static LetterOrDigit LetterOrDigit()
        {
            return new LetterOrDigit();
        }
        public static Integer Integer()
        {
            return new Integer();
        }
        public static Character Character(char c)
        {
            return new Character(c);
        }
        public static Many<T> Many<T>(Parser<T> parser)
        {
            return new Many<T>(parser);
        }
        public static Many1<T> Many1<T>(Parser<T> parser)
        {
            return new Many1<T>(parser);
        }
        public static ZeroOrOne<T> ZeroOrOne<T>(Parser<T> parser)
        {
            return new ZeroOrOne<T>(parser);
        }
        public static StringParse String(string str)
        {
            return new StringParse(str);
        }
        public static StringParse String(IEnumerable<char> str)
        {
            return new StringParse(str);
        }
        public static ParserChar ParserChar(char c, int column=0, int line=0)
        {
            return new ParserChar(c, column, line);
        }
        public static Whitespace Whitespace()
        {
            return new Whitespace();
        }

        public static Func<A, Parser<ParserChar>> FromDelegate<A>(Func<A,Func<IEnumerable<ParserChar>, ParserResult<ParserChar>>> func)
        {
            return FromDelegate<A, ParserChar>(func);
        }

        public static Func<A, Parser<P>> FromDelegate<A, P>(Func<A, Func<IEnumerable<ParserChar>, ParserResult<P>>> func)
        {
            return (A a) => new Parser<P>(func(a));
        }
    }
}
