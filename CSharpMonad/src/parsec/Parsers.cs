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
using Monad;
using Monad.Utility;

namespace Monad.Parsec
{
    public class Lazy<A> : Parser<A>
    {
        public Lazy(Func<Parser<A>> func)
            :
            base(
                inp => func() == null 
                        ? new Failure<A>(new ParserError("Lazy parser not resolved before use",inp)).Parse(inp)
                        : func().Parse(inp)
            )
        {
        }
    }

    public class Item : Parser<ParserChar>
    {
        public Item()
            :
            base(
                inp => inp.IsEmpty
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
                inp => new ParserResult<A>( new Tuple<A, ImmutableList<ParserChar>>[0] )
            )
        { }
    }

    public class Failure<A> : Parser<A>
    {
        public Failure(ParserError error)
            :
            base(
                inp => ParserResult.Fail<A>(error)
            )
        {
        }

        private Failure(IEnumerable<ParserError> errors)
            :
            base(
                inp => ParserResult.Fail<A>(new ImmutableList<ParserError>(errors))
            )
        {
        }
        public Failure(ParserError error, IEnumerable<ParserError> errors)
            :
            base(
                inp => ParserResult.Fail<A>(
                    new ImmutableList<ParserError>(Enumerable.Concat(new ParserError[]{ error}, errors))
                    )
            )
        {
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
            this(ps.FirstOrDefault(), ps.Skip(1))
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
                        ? ps.FirstOrDefault() == null
                            ? ParserResult.Fail<A>("choice not satisfied", inp)
                            : new Choice<A>(ps.First(), ps.Skip(1)).Parse(inp)
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
                    inp.IsEmpty
                        ? Prim.Failure<ParserChar>(ParserError.Create(expecting, inp)).Parse(inp)
                        : (from res in Prim.Item().Parse(inp).Value
                           select pred(res.Item1.Value)
                              ? Prim.Return(res.Item1).Parse(inp.Tail())
                              : ParserResult.Fail<ParserChar>(expecting, inp))
                          .First()
            )
        { }
    }

    public class OneOf : Satisfy
    {
        public OneOf(string chars)
            :
            base(ch => chars.Contains(new string(ch, 1)), "one of: " + chars)
        { }
        public OneOf(IEnumerable<char> chars)
            :
            base(ch => chars.Contains(ch), "one of: " + chars)
        { }
        public OneOf(ImmutableList<ParserChar> chars)
            :
            base(ch => chars.Select(pc=>pc.Value).Contains(ch), "one of: " + chars)
        { }
    }

    public class NoneOf : Satisfy
    {
        public NoneOf(string chars)
            :
            base(ch => !chars.Contains(new string(ch, 1)), "none of: " + chars)
        { }
        public NoneOf(IEnumerable<char> chars)
            :
            base(ch => !chars.Contains(ch), "none of: " + chars)
        { }
        public NoneOf(ImmutableList<ParserChar> chars)
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
            base(c => "01234567".Contains(new string(c, 1)), "an octal-digit")
        {
        }
    }

    public class HexDigit : Satisfy
    {
        public HexDigit()
            :
            base(c => Char.IsDigit(c) || "abcdefABCDEF".Contains(new string(c, 1)) , "a hex-digit")
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
                (from minus in Prim.Try(Prim.Character('-') | Prim.Return( new ParserChar('+') ) )
                 from digits in Prim.Many1(Prim.Digit())
                 let v = DigitsToInt(digits)
                 select minus.Value == '+'
                    ? v
                    : -v)
                .Parse(inp)
            )
        {
        }

        private static int DigitsToInt(ImmutableList<ParserChar> digits)
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
                        ? new ParserResult<A>(new Tuple<A, ImmutableList<ParserChar>>[0])
                        : res;
                }
            )
        {
        }

        public Try<A> OrTry(Parser<A> p)
        {
            return Prim.Try<A>(p);
        }
    }

    public class Many<A> : Parser<ImmutableList<A>>
    {
        public Many(Parser<A> parser)
            :
            base(inp => (Prim.Many1(parser) | Prim.Return(new ImmutableList<A>(new A[0]))).Parse(inp))
        { }
    }

    public class Many1<A> : Parser<ImmutableList<A>>
    {
        public Many1(Parser<A> parser)
            :
            base(inp =>
            {
                //
                // This is the original parser.  Beautiful, but less efficient unfortunately.
                // I will leave it here for future generations to admire.
                //
                //    (from v in parser
                //     from vs in Prim.Many(parser)
                //     select v.Cons(vs))
                //    .Parse(inp)
                //

                var v = parser.Parse(inp);
                if (v.IsFaulted)
                    return new ParserResult<ImmutableList<A>>(v.Errors);
                if (v.Value.Length == 0)
                    return new ParserResult<ImmutableList<A>>(
                        ImmutableList.Empty<Tuple<ImmutableList<A>, ImmutableList<ParserChar>>>()
                        );

                List<A> vs = new List<A>();
                var fst = v.Value.Head();
                vs.Add(fst.Item1);

                while (true)
                {
                    v = parser.Parse(fst.Item2);
                    if (v.IsFaulted || v.Value.Length == 0)
                        return new ParserResult<ImmutableList<A>>(
                            Tuple.Create(new ImmutableList<A>(vs), fst.Item2
                            ).Cons());
                    if (v.Value.Length == 0)
                        return new ParserResult<ImmutableList<A>>(
                            Tuple.Create(new ImmutableList<A>(vs), fst.Item2
                            ).Cons());
                    fst = v.Value.First();
                    vs.Add(fst.Item1);
                }
            })
        { }
    }

    public class StringParse : Parser<ImmutableList<ParserChar>>
    {
        public StringParse(string str)
            :
            this(str.Cast<char>())
        {
        }

        public StringParse(IEnumerable<char> str)
            :
            base(
                inp => str.Count() == 0
                          ? Prim.Return(ImmutableList.Empty<ParserChar>()).Parse(inp)
                          : (from x in Prim.Character(str.First())
                             from xs in Prim.String(str.Skip(1))
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
                inp => Prim.SkipMany(
                            Prim.Character(' ')
                            | Prim.Character('\t')
                            | Prim.Character('\n')
                            | Prim.Character('\r')
                        )
                        .Parse(inp)
            )
        { }
    }

    public class SimpleSpace : Parser<ImmutableList<ParserChar>>
    {
        public SimpleSpace()
            :
            base(
                inp => Prim.Many( Prim.Character(' ') )
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
                        return Tuple.Create(Unit.Default, inp).Cons().Success();
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