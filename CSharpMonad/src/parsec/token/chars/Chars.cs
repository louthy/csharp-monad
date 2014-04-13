using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monad.Parsec.Token;

namespace Monad.Parsec.Token.Chars
{
    public class CharLiteral : Parser<CharLiteralToken>
    {
        public CharLiteral()
            :
            base(
                inp => Tok.Lexeme(
                    New.Between(
                        New.Character('\''),
                        New.Character('\'').Fail("end of character"),
                        new CharacterChar()
                    ))
                    .Select(ch => new CharLiteralToken(ch,inp.First().Location))
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
                inp => (from c in New.Character('\\')
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
                "abfnrtv\\\"\'".Zip(
                "\a\b\f\n\r\t\v\\\"\'",
                (l, r) => Tuple.Create(l, r));
        }

        public CharEsc()
            :
            base(
                inp =>
                    New.Choice<ParserChar>(
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
                        var r = New.Character(c).Parse(inp);
                        if (r.IsFaulted)
                        {
                            return r;
                        }
                        else
                        {
                            var tuple = r.Value.First();
                            return ParserResult.Success<ParserChar>(
                                Tuple.Create<ParserChar, IEnumerable<ParserChar>>(
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
