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

namespace Monad.Parsec.Token.Strings
{
    public class StringLiteral : Parser<StringLiteralToken>
    {
        public StringLiteral()
            :
            base(
                inp => (from l in Tok.Lexeme(
                            from str in Prim.Between(
                                Prim.Character('"'),
                                Prim.Character('"').Fail("end of string"),
                                Prim.Many( new StringChar() )
                                )
                            select str
                        )
                        select new StringLiteralToken(l,inp.Head().Location)
                       )
                       .Fail("literal string")
                       .Parse(inp)
            )
        { }
    }

    public class StringChar : Parser<ParserChar>
    {
        public StringChar()
            :
            base(
                inp => (from c in new StringLetter()
                        select c)
                       .Or( new StringEscape() )
                       .Fail("string character")
                       .Parse(inp)
            )
        { }
    }

    public class StringLetter : Satisfy
    {
        public StringLetter()
            :
            base( c => (c != '"') && (c != '\\') && (c > 26), "string character" )
        { }
    }

    public class StringEscape : Parser<ParserChar>
    {
        public StringEscape()
            :
            base(
                inp =>
                    Prim.Character('\\')
                       .And(
                            new EscapeGap().And(Prim.Failure<ParserChar>(ParserError.Create("", inp)))
                           .Or(new EscapeEmpty().And(Prim.Failure<ParserChar>(ParserError.Create("", inp))))
                           .Or(Tok.Chars.EscapeCode())
                       )
                       .Parse(inp)
           )
        { }
    }

    public class EscapeGap : Parser<ParserChar>
    {
        public EscapeGap()
            :
            base(
                inp => (from sp in Prim.Many1(Prim.Character(' '))
                        from ch in Prim.Character('\\')
                        select ch)
                       .Fail("end of string gap")
                       .Parse(inp)
            )
        {}
    }

    public class EscapeEmpty : Character
    {
        public EscapeEmpty()
            :
            base('&')
        { }
    }
}
