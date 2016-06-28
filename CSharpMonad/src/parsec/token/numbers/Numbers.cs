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
using System.Linq;

namespace Monad.Parsec.Token.Numbers
{
    public class Integer : Parser<IntegerToken>
    {
        public Integer()
            :
            base(
                inp => (from lex in Tok.Lexeme( new Int() )
                        select lex)
                       .Fail("integer")
                       .Parse(inp)
            )
        { }
    }

    public class Natural : Parser<IntegerToken>
    {
        public Natural()
            :
            base(
                inp => (from lex in Tok.Lexeme(new Int())
                        select lex)
                       .Fail("natural")
                       .Parse(inp)
            )
        { }
    }


    public class Int : Parser<IntegerToken>
    {
        public Int()
            :
            base(
                inp => (from f in Tok.Lexeme<ParserChar>(new Sign())
                        from n in new Nat()
                        select new IntegerToken( 
                            f.Value == '-' ? -n.Value : n.Value, 
                            f.Location
                        ))
                       .Parse(inp)
            )
        { }
    }

    public class Sign : Parser<ParserChar>
    {
        public Sign()
            :
            base(
                inp =>
                {
                    var r = Prim.Character('-')
                               .Or(Prim.Character('+'))
                               .Parse(inp);

                    return r.IsFaulted
                        ? ParserResult.Success(Tuple.Create(new ParserChar('+'), inp).Cons())
                        : r;
                }
        )
        { }
    }

    public class Nat : Parser<IntegerToken>
    {
        public Nat()
            :
            base( inp => new ZeroNumber().Or(new Decimal()).Parse(inp) )
        {}
    }

    public class ZeroNumber : Parser<IntegerToken>
    {
        public ZeroNumber()
            :
            base(
                inp => (from z in Prim.Character('0')
                        from y in Prim.Choice(
                            Tok.Numbers.Hexadecimal() as Parser<IntegerToken>,
                            Tok.Numbers.Octal() as Parser<IntegerToken>,
                            Tok.Numbers.Decimal() as Parser<IntegerToken>,
                            Prim.Return(new IntegerToken(0, SrcLoc.Null)) as Parser<IntegerToken>
                            )
                        select new IntegerToken(0, !inp.IsEmpty ? inp.Head().Location : SrcLoc.EndOfSource))
                        .Fail("")
                        .Parse(inp)
            )
        {}
    }

    public class Decimal : Parser<IntegerToken>
    {
        public Decimal()
            :
            base( 
                inp =>{
                    var r = Prim.Many1(Prim.Digit()).Parse(inp);
                    if( r.IsFaulted )
                    {
                        return ParserResult.Fail<IntegerToken>(r.Errors);
                    }
                    else
                    {
                        var val = r.Value.First();
                        return ParserResult.Success<IntegerToken>(
                            Tuple.Create(
                                new IntegerToken( Int32.Parse(val.Item1.AsString()), inp.First().Location ),
                                val.Item2
                            ).Cons()
                        );
                    }
                }
            )
        {}
    }

    public class Hexadecimal : Parser<IntegerToken>
    {
        public Hexadecimal()
            :
            base(
                inp =>
                {
                    var r = (from x in Prim.Character('x').Or(Prim.Character('X'))
                             from d in Prim.Many1(Prim.HexDigit())
                             select d)
                             .Parse(inp);

                    if (r.IsFaulted)
                    {
                        return ParserResult.Fail<IntegerToken>(r.Errors);
                    }
                    else
                    {
                        var val = r.Value.First();
                        return ParserResult.Success<IntegerToken>(
                            Tuple.Create(
                                new IntegerToken(Convert.ToInt32(val.Item1.AsString(),16), inp.First().Location),
                                val.Item2
                            ).Cons()
                        );
                    }
                }
            )
        { }
    }

    public class Octal : Parser<IntegerToken>
    {
        public Octal()
            :
            base(
                inp =>
                {
                    var r = (from x in Prim.Character('o').Or(Prim.Character('O'))
                             from d in Prim.Many1(Prim.OctalDigit())
                             select d)
                             .Parse(inp);

                    if (r.IsFaulted)
                    {
                        return ParserResult.Fail<IntegerToken>(r.Errors);
                    }
                    else
                    {
                        var val = r.Value.First();
                        return ParserResult.Success<IntegerToken>(
                            Tuple.Create(
                                new IntegerToken(Convert.ToInt32(val.Item1.AsString(), 8), inp.First().Location),
                                val.Item2
                            ).Cons()
                        );
                    }
                }
            )
        { }
    }
}
