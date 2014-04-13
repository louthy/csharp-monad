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
        public static Choice<A> Choice<A>(Parser<A> p, IEnumerable<Parser<A>> ps)
        {
            return new Choice<A>(p, ps);
        }
        public static Choice<A> Choice<A>(IEnumerable<Parser<A>> ps)
        {
            return new Choice<A>(ps);
        }
        public static Satisfy Satisfy(Func<char, bool> predicate, string expecting = "")
        {
            return new Satisfy(predicate, expecting);
        }
        public static OneOf OneOf(string chars)
        {
            return new OneOf(chars);
        }
        public static OneOf OneOf(IEnumerable<char> chars)
        {
            return new OneOf(chars);
        }
        public static OneOf OneOf(IEnumerable<ParserChar> chars)
        {
            return new OneOf(chars);
        }
        public static Parser<IEnumerable<A>> SepBy<A,B>(Parser<A> parser, Parser<B> sepParser)
        {
            return SepBy1<A, B>(parser, sepParser)
                        .Or(New.Return<IEnumerable<A>>(new A[0]));
        }
        public static Parser<IEnumerable<A>> SepBy1<A, B>(Parser<A> parser, Parser<B> sepParser)
        {
            return (from x in parser
                    from xs in New.Many<A>( sepParser.And(parser) )
                    select x.Cons(xs));
        }

        public static Digit Digit()
        {
            return new Digit();
        }
        public static HexDigit HexDigit()
        {
            return new HexDigit();
        }
        public static OctalDigit OctalDigit()
        {
            return new OctalDigit();
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
        public static Try<T> Try<T>(Parser<T> parser)
        {
            return new Try<T>(parser);
        }
        public static StringParse String(string str)
        {
            return new StringParse(str);
        }
        public static StringParse String(IEnumerable<char> str)
        {
            return new StringParse(str);
        }
        public static ParserChar ParserChar(char c, SrcLoc location = null)
        {
            return new ParserChar(c, location);
        }
        public static WhiteSpace WhiteSpace()
        {
            return new WhiteSpace();
        }
        public static SimpleSpace SimpleSpace()
        {
            return new SimpleSpace();
        }
        public static NotFollowedBy<A> NotFollowedBy<A>(Parser<A> followParser)
        {
            return new NotFollowedBy<A>(followParser);
        }

        public static Between<O, C, B> Between<O, C, B>(Parser<O> openParser, Parser<C> closeParser, Parser<B> betweenParser)
        {
            return new Between<O, C, B>(openParser,closeParser,betweenParser);
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
