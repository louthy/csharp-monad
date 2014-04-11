using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec
{
    public class Item : Parser<ParserChar>
    {
        public Item()
            :
            base(
                inp => inp.Count() == 0
                    ? ParserResult.Fail<ParserChar>("a character",inp)
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
        IEnumerable<Parser<A>> parsers;

        public Choice(Parser<A> p, params Parser<A>[] ps)
            :
            base(
                inp =>
                {
                    var errors = new List<ParserError>();
                    var pres = p.Parse(inp);

                    foreach(var parser in ps)
                    {
                        if (pres.IsFaulted)
                        {
                            errors.AddRange(pres.Errors);
                            pres = parser.Parse(inp);
                        }
                        else
                        {
                            return pres;
                        }
                    }

                    if( pres.IsFaulted )
                    {
                        return ParserResult.Fail<A>( 
                            String.Join(", ",errors.Select(e=>e.Expected)),
                            inp
                            );
                    }
                    else
                    {
                        return pres;
                    }
                }
            )
        {
            parsers = p.Cons(ps);
        }
    }

    public class Satisfy : Parser<ParserChar>
    {
        public Satisfy(Func<char, bool> pred, string expecting)
            :
            base(
                inp =>
                    inp.Count() == 0
                        ? New.Failure<ParserChar>( ParserError.Create(expecting, inp) ).Parse(inp)
                        : (from res in New.Item().Parse(inp).Value
                           select pred(res.Item1.Value)
                              ? New.Return(res.Item1).Parse(inp.Tail())
                              : ParserResult.Fail<ParserChar>(expecting, inp))
                          .First()
            )
        { }
    }

    public class Digit : Satisfy
    {
        public Digit()
            :
            base(c=> Char.IsDigit(c), "a digit")
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

    public class Character : Satisfy
    {
        public Character(char isChar)
            :
            base(c => c == isChar, "'"+isChar+"'")
        { }
    }

    public class Many<A> : Parser<IEnumerable<A>>
    {
        public Many(Parser<A> parser)
            :
            base(
                inp =>
                    New.Choice(
                        New.Many1(parser),
                        New.Return<IEnumerable<A>>( new A[0] )
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
            this( str as IEnumerable<char> )
        {
        }

        public StringParse(IEnumerable<char> str)
            :
            base(
                inp => str.Count() == 0
                          ? New.Return(new ParserChar[0] as IEnumerable<ParserChar>).Parse(inp)
                          : (from x in New.Character(str.Head())
                             from xs in New.String(str.Tail())
                             select x.Cons(xs) )
                            .Parse(inp)
            )
        { }
    }

    public class Whitespace : Parser<IEnumerable<ParserChar>>
    {
        public Whitespace()
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
}
