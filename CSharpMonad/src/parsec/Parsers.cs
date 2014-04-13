using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monad;
using Monad.Parsec;

namespace Monad.Parsec
{
    public class Item : Parser<ParserChar>
    {
        public Item()
            :
            base(
                inp => inp.Count() == 0
                    ? ParserResult.Fail<ParserChar>("a character", inp)
                    : Tuple.Create(inp.Head(), inp.Tail()).Cons().Success()
            )
        { }
    }

    public class Failure<A> : Parser<A>
    {
        readonly IEnumerable<ParserError> errors;

        public Failure(ParserError error)
            :
            base(
                inp => ParserResult.Fail<A>(error)
            )
        {
            errors = error.Cons();
        }

        private Failure(IEnumerable<ParserError> errors)
            :
            base(
                inp => ParserResult.Fail<A>(errors)
            )
        {
            this.errors = errors;
        }

        public Failure(ParserError error, IEnumerable<ParserError> errors)
            :
            base(
                inp => ParserResult.Fail<A>(error.Cons(errors))
            )
        {
            this.errors = error.Cons(errors);
        }

        public IEnumerable<ParserError> Errors
        {
            get
            {
                return errors;
            }
        }

        public Failure<A> AddError(ParserError error)
        {
            return new Failure<A>(error.Cons(errors));
        }
    }

    public class Return<A> : Parser<A>
    {
        public Return(A v)
            :
            base(
                inp => Tuple.Create(v, inp).Cons().Success()
            )
        { }
    }

    public class Choice<A> : Parser<A>
    {
        public Choice(Parser<A> p, params Parser<A>[] ps)
            :
            this(p,ps.AsEnumerable())
        {
        }

        public Choice(IEnumerable<Parser<A>> ps)
            :
            this(ps.Head(),ps.Tail())
        {
        }

        public Choice(Parser<A> p, IEnumerable<Parser<A>> ps)
            :
            base(
                inp =>
                {
                    var r = p.Parse(inp);
                    return r.IsFaulted
                        ? ps.Count() > 0
                            ? new Choice<A>(ps.Head(), ps.Tail()).Parse(inp)
                            : ParserResult.Fail<A>("choice not satisfied", inp)
                        : r;
                }
            )
        {
        }
    }

    public class Satisfy : Parser<ParserChar>
    {
        public Satisfy(Func<char, bool> pred, string expecting)
            :
            base(
                inp =>
                    inp.Count() == 0
                        ? New.Failure<ParserChar>(ParserError.Create(expecting, inp)).Parse(inp)
                        : (from res in New.Item().Parse(inp).Value
                           select pred(res.Item1.Value)
                              ? New.Return(res.Item1).Parse(inp.Tail())
                              : ParserResult.Fail<ParserChar>(expecting, inp))
                          .First()
            )
        { }
    }

    public class OneOf : Satisfy
    {
        public OneOf(string chars)
            :
            base(ch => chars.Contains(ch), "one of: " + chars)
        { }
        public OneOf(IEnumerable<char> chars)
            :
            base(ch => chars.Contains(ch), "one of: " + chars)
        { }
        public OneOf(IEnumerable<ParserChar> chars)
            :
            base(ch => chars.Select(pc=>pc.Value).Contains(ch), "one of: " + chars)
        { }
    }

    public class Digit : Satisfy
    {
        public Digit()
            :
            base(c => Char.IsDigit(c), "a digit")
        {
        }
    }

    public class OctalDigit : Satisfy
    {
        public OctalDigit()
            :
            base(c => "01234567".Contains(c), "an octal-digit")
        {
        }
    }

    public class HexDigit : Satisfy
    {
        public HexDigit()
            :
            base(c => Char.IsDigit(c) || "abcdefABCDEF".Contains(c) , "a hex-digit")
        {
        }
    }

    public class Letter : Satisfy
    {
        public Letter()
            :
            base(c => Char.IsLetter(c), "a letter")
        {
        }
    }

    public class LetterOrDigit : Satisfy
    {
        public LetterOrDigit()
            :
            base(c => Char.IsLetterOrDigit(c), "a letter or a digit")
        {
        }
    }

    public class Integer : Parser<int>
    {
        public Integer()
            :
            base(inp =>
                (from minus in New.Try(New.Character('-'))
                 from digits in New.Many1(New.Digit())
                 let v = DigitsToInt(digits)
                 select minus.Count() == 0
                    ? v
                    : -v)
                .Parse(inp)
            )
        {
        }

        private static int DigitsToInt(IEnumerable<ParserChar> digits)
        {
            return Int32.Parse(digits.AsString());
        }
    }

    public class Character : Satisfy
    {
        public Character(char isChar)
            :
            base(c => c == isChar, "'" + isChar + "'")
        { }
    }

    public class Try<A> : Parser<IEnumerable<A>>
    {
        public Try(Parser<A> parser)
            : base(
                inp =>
                New.Choice(
                    new ValueToListParser<A>(parser),
                    New.Return<IEnumerable<A>>(new A[0])
                )
                .Parse(inp)
            )
        {
        }
    }

    class ValueToListParser<A> : Parser<IEnumerable<A>>
    {
        public ValueToListParser(Parser<A> parser)
            :
            base(inp =>
            {
                var res = parser.Parse(inp);
                if (res.IsFaulted)
                {
                    return ParserResult.Fail<IEnumerable<A>>(res.Errors);
                }
                else
                {
                    var a = res.Value.First();
                    return new ParserResult<IEnumerable<A>>(
                        Tuple.Create(a.Item1.Cons(), a.Item2).Cons()
                    );
                }
            })
        {
        }
    }

    public class Many<A> : Parser<IEnumerable<A>>
    {
        public Many(Parser<A> parser)
            :
            base(
                inp =>
                    New.Choice(
                        New.Many1(parser),
                        New.Return<IEnumerable<A>>(new A[0])
                    )
                    .Parse(inp)
            )
        { }
    }

    public class Many1<A> : Parser<IEnumerable<A>>
    {
        public Many1(Parser<A> parser)
            :
            base(
                inp =>
                {
                    var v = parser.Parse(inp);
                    if (v.IsFaulted)
                        return ParserResult.Fail<IEnumerable<A>>(v.Errors);

                    var fst = v.Value.First();
                    var vs = New.Many(parser).Parse(fst.Item2);
                    if (vs.IsFaulted)
                        return New.Return(fst.Item1.Cons()).Parse(fst.Item2);

                    var snd = vs.Value.First();
                    return New.Return(fst.Item1.Cons(snd.Item1)).Parse(snd.Item2);
                }
            )
        { }
    }

    public class StringParse : Parser<IEnumerable<ParserChar>>
    {
        public StringParse(string str)
            :
            this(str as IEnumerable<char>)
        {
        }

        public StringParse(IEnumerable<char> str)
            :
            base(
                inp => str.Count() == 0
                          ? New.Return(new ParserChar[0] as IEnumerable<ParserChar>).Parse(inp)
                          : (from x in New.Character(str.Head())
                             from xs in New.String(str.Tail())
                             select x.Cons(xs))
                            .Parse(inp)
            )
        { }
    }

    public class WhiteSpace : Parser<IEnumerable<ParserChar>>
    {
        public WhiteSpace()
            :
            base(
                inp => New.Many(
                            New.Character(' ')
                                .Or(New.Character('\t'))
                                .Or(New.Character('\n'))
                                .Or(New.Character('\r'))
                        )
                        .Parse(inp)
            )
        { }
    }

    public class SimpleSpace : Parser<IEnumerable<ParserChar>>
    {
        public SimpleSpace()
            :
            base(
                inp => New.Many( New.Character(' ') )
                          .Parse(inp)
            )
        { }
    }

    public class Between<O,C,B> : Parser<B>
    {
        public Between(
            Parser<O> openParser,
            Parser<C> closeParser,
            Parser<B> betweenParser
            )
            :
            base(
                inp => (from o in openParser
                        from b in betweenParser
                        from c in closeParser
                        select b)
                       .Parse(inp)
            )
        {
        }
    }

    public class NotFollowedBy<A> : Parser<Unit>
    {
        public NotFollowedBy(Parser<A> parser)
            :
            base(
                inp => 
                {
                    var res = (from c in parser select c).Parse(inp);
                    if( res.IsFaulted )
                    {
                        return Tuple.Create( Unit.Return(), inp ).Cons().Success();
                    }
                    else
                    {
                        return ParserResult.Fail<Unit>(ParserError.Create("unexpected",inp));
                    }
                }
            )
        { }
    }
}