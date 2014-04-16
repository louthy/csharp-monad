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
using Monad;
using Monad.Parsec;

namespace Monad.Parsec
{
    public class Item : Parser<ParserChar>
    {
        public Item()
            :
            base(
                inp => inp.IsEmpty()
                    ? ParserResult.Fail<ParserChar>("a character", inp)
                    : Tuple.Create(inp.Head(), inp.Tail()).Cons().Success()
            )
        { }
    }

    public class Empty<A> : Parser<A>
    {
        public Empty()
            :
            base(
                inp => new ParserResult<A>(new Tuple<A, IEnumerable<ParserChar>>[0])
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
            this(ps.HeadSafe(), ps.Tail())
        {
        }

        public Choice(Parser<A> p, IEnumerable<Parser<A>> ps)
            :
            base(
                inp =>
                {
                    if (p == null)
                        return ParserResult.Fail<A>("choice not satisfied", inp);

                    var r = p.Parse(inp);
                    return r.IsFaulted
                        ? ps.IsEmpty()
                            ? ParserResult.Fail<A>("choice not satisfied", inp)
                            : new Choice<A>(ps.Head(), ps.Tail()).Parse(inp)
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
                    inp.IsEmpty()
                        ? Gen.Failure<ParserChar>(ParserError.Create(expecting, inp)).Parse(inp)
                        : (from res in Gen.Item().Parse(inp).Value
                           select pred(res.Item1.Value)
                              ? Gen.Return(res.Item1).Parse(inp.Tail())
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

    public class NoneOf : Satisfy
    {
        public NoneOf(string chars)
            :
            base(ch => !chars.Contains(ch), "none of: " + chars)
        { }
        public NoneOf(IEnumerable<char> chars)
            :
            base(ch => !chars.Contains(ch), "none of: " + chars)
        { }
        public NoneOf(IEnumerable<ParserChar> chars)
            :
            base(ch => !chars.Select(pc => pc.Value).Contains(ch), "none of: " + chars)
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
                (from minus in Gen.Try(Gen.Character('-') | Gen.Return( new ParserChar('+') ) )
                 from digits in Gen.Many1(Gen.Digit())
                 let v = DigitsToInt(digits)
                 select minus.Value == '+'
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

    public class Try<A> : Parser<A>
    {
        public Try(Parser<A> parser)
            : base(
                inp =>
                {
                    var res = parser.Parse(inp);
                    return res.IsFaulted
                        ? new ParserResult<A>(new Tuple<A,IEnumerable<ParserChar>>[0])
                        : res;
                }
            )
        {
        }

        public Try<A> OrTry(Parser<A> p)
        {
            return Gen.Try<A>(p);
        }
    }

    public class Many<A> : Parser<IEnumerable<A>>
    {
        public Many(Parser<A> parser)
            :
            base(inp => (Gen.Many1(parser) | Gen.Return(new A[0].AsEnumerable())).Parse(inp))
        { }
    }

    public class Many1<A> : Parser<IEnumerable<A>>
    {
        public Many1(Parser<A> parser)
            :
            base( inp =>
                (from v in parser
                 from vs in Gen.Many(parser)
                 select v.Cons(vs))
                .Parse(inp)
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
                inp => str.IsEmpty()
                          ? Gen.Return(new ParserChar[0] as IEnumerable<ParserChar>).Parse(inp)
                          : (from x in Gen.Character(str.Head())
                             from xs in Gen.String(str.Tail())
                             select x.Cons(xs))
                            .Parse(inp)
            )
        { }
    }

    public class WhiteSpace : Parser<Unit>
    {
        public WhiteSpace()
            :
            base(
                inp => Gen.SkipMany(
                            Gen.Character(' ')
                            | Gen.Character('\t')
                            | Gen.Character('\n')
                            | Gen.Character('\r')
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
                inp => Gen.Many( Gen.Character(' ') )
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