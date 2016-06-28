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
using Monad.Utility;

namespace Monad.Parsec.Token.Chars
{
    public class CharLiteral : Parser<CharLiteralToken>
    {
        public CharLiteral()
            :
            base(
                inp => Tok.Lexeme(
                    Prim.Between(
                        Prim.Character('\''),
                        Prim.Character('\'').Fail("end of character"),
                        new CharacterChar()
                    ))
                    .Select(ch => new CharLiteralToken(ch, inp.Head().Location))
                    .Fail("character")
                    .Parse(inp)
            )
        { }
    }

    public class CharacterChar : Parser<ParserChar>
    {
        public CharacterChar()
            :
            base(
                inp => new CharLetter()
                          .Or(Tok.Chars.CharEscape())
                          .Fail("literal character")
                          .Parse(inp)
            )
        { }
    }

    /// <summary>
    /// Valid non-escape, non-quote char
    /// </summary>
    public class CharLetter : Satisfy
    {
        public CharLetter()
            :
            base(
                c => (c != '\'') && (c != '\\') && (c > 26),
                "char letter"
            )
        { }
    }

    /// <summary>
    /// Escape character (\) followed by an accepted escape code
    /// </summary>
    /// <returns></returns>
    public class CharEscape : Parser<ParserChar>
    {
        public CharEscape()
            :
            base(
                inp => (from c in Prim.Character('\\')
                        from ec in Tok.Chars.EscapeCode()
                        select ec)
                       .Parse(inp)
            )
        { }
    }

    /// <summary>
    /// Escape code set parser without the \ prefix
    /// </summary>
    /// <returns></returns>
    public class EscapeCode : Parser<ParserChar>
    {
        public EscapeCode()
            :
            base(
                inp => new CharEsc()
                          .Or(new CharNum())
                    //.Or(Tok.CharAscii()) TODO
                    //.Or(Tok.CharControl()) TODO
                          .Parse(inp)
            )
        { }
    }

    /// <summary>
    /// The single char escape codes without the \ prefix
    /// \a \b \f \n \r \t \v \\ \" \'
    /// </summary>
    /// <returns></returns>
    public class CharEsc : Parser<ParserChar>
    {
        public static IEnumerable<Tuple<char, char>> EscapeMap;

        static CharEsc()
        {
            EscapeMap =
                "abfnrtv\\\"\'".Cast<char>().Zip(
                "\a\b\f\n\r\t\v\\\"\'".Cast<char>(),
                (l, r) => Tuple.Create(l, r));
        }

        public CharEsc()
            :
            base(
                inp =>
                    Prim.Choice<ParserChar>(
                        EscapeMap.Select(pair => new ParseEsc(pair.Item1, pair.Item2))
                    )
                    .Parse(inp)
                )
        { }


        private class ParseEsc : Parser<ParserChar>
        {
            public ParseEsc(char c, char code)
                :
                base(
                    inp =>
                    {
                        var r = Prim.Character(c).Parse(inp);
                        if (r.IsFaulted)
                        {
                            return r;
                        }
                        else
                        {
                            var tuple = r.Value.First();
                            return ParserResult.Success<ParserChar>(
                                Tuple.Create<ParserChar, ImmutableList<ParserChar>>(
                                    new ParserChar(
                                        code,
                                        tuple.Item1.Location
                                    ),
                                    tuple.Item2
                               ).Cons()
                            );
                        }
                    }
                )
            { }
        }
    }

    public class CharNum : Parser<ParserChar>
    {
        public CharNum()
            :
            base(
                inp => (from code in
                            Tok.Numbers.Decimal()
                               .Or(from d in Tok.Numbers.Hexadecimal()
                                   select d)
                               .Or(from d in Tok.Numbers.Octal()
                                   select d)
                        select new ParserChar((char)code.Value, code.Location))
                      .Parse(inp)
            )
        { }
    }
}