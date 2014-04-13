using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec.Token
{
    public class TokenParser<A> : GenTokenParser<A>
    {
        public TokenParser(GeneralLanguageDef def)
            :
            base(def)
        {
        }
    }

    public class GenTokenParser<A> : Parser<A>
    {
        Parser<IEnumerable<IdentifierToken>> identifier;
        Dictionary<string, Parser<IEnumerable<ReservedToken>>> reserved;
        Parser<IEnumerable<OperatorToken>> op;
        Dictionary<string, Parser<IEnumerable<ReservedOpToken>>> reservedOp;
        Parser<ParserChar> charLiteral;
        Parser<IEnumerable<ParserChar>> stringLiteral; //TODO
        Parser<IntegerToken> natural;
        Parser<IntegerToken> integer;
        Parser<FloatToken> floating;
        Parser<NaturalOrFloatToken> naturalOrFloat;
        Parser<IntegerToken> decimalNumber;
        Parser<IntegerToken> hexadecimal;
        Parser<IntegerToken> octal;
        Func<string, Parser<IEnumerable<ParserChar>>> symbol;
        Func<Parser<A>,Parser<A>> lexeme;
        Parser<IEnumerable<ParserChar>> whiteSpace;
        Func<Parser<A>,Parser<A>> parens;
        Func<Parser<A>, Parser<A>> braces;
        Func<Parser<A>, Parser<A>> angles;
        Func<Parser<A>, Parser<A>> brackets;
        Symbol semi;
        Symbol comma;
        Symbol colon;
        Symbol dot;
        Func<Parser<A>, Parser<IEnumerable<A>>> semiSep;
        Func<Parser<A>, Parser<IEnumerable<A>>> semiSep1;
        Func<Parser<A>, Parser<IEnumerable<A>>> commaSep;
        Func<Parser<A>, Parser<IEnumerable<A>>> commaSep1;

        public GenTokenParser(GeneralLanguageDef def)
            :
            base(ParseInput)
        {
            identifier = Tok.Id.Identifier(def);
            reserved = def.ReservedNames.ToDictionary(name => name, name => Tok.Id.Reserved(name, def) as Parser<IEnumerable<ReservedToken>>);

            op = Tok.Ops.Operator(def);
            reservedOp = def.ReservedOpNames.ToDictionary(name => name, name => Tok.Ops.ReservedOp(name, def) as Parser<IEnumerable<ReservedOpToken>>);

            charLiteral = Tok.Chars.CharLiteral();

            // stringLiteral = Tok.Strings.StringLiteral() TODO

            natural = Tok.Numbers.Natural();
            integer = Tok.Numbers.Integer();
            
            // floating = Tok.Numbers.Floating(); TODO
            // naturalOrFloat = Tok.Numbers.NaturalOrFloating(); TODO

            // TODO - Use the proper whiteSpace code from Text-Parsec-Token
            whiteSpace = New.WhiteSpace() as Parser<IEnumerable<ParserChar>>;  

            decimalNumber = Tok.Numbers.Decimal();
            hexadecimal = Tok.Numbers.Hexadecimal();
            octal = Tok.Numbers.Octal();
            symbol = (string name) => Tok.Symbol(name) as Parser<IEnumerable<ParserChar>>;
            lexeme = (Parser<A> p) => Tok.Lexeme(p);
            parens = (Parser<A> p) => Tok.Bracketing.Parens(p);
            braces = (Parser<A> p) => Tok.Bracketing.Braces(p);
            angles = (Parser<A> p) => Tok.Bracketing.Angles(p);
            brackets = (Parser<A> p) => Tok.Bracketing.Brackets(p);
            semi = Tok.Symbol(";");
            comma = Tok.Symbol(",");
            colon = Tok.Symbol(":");
            dot = Tok.Symbol(".");
            commaSep = (Parser<A> p) => New.SepBy(p, comma);
            semiSep = (Parser<A> p) => New.SepBy(p, semi);
            commaSep1 = (Parser<A> p) => New.SepBy1(p, comma);
            semiSep1 = (Parser<A> p) => New.SepBy1(p, semi);
        }

        private static ParserResult<A> ParseInput(IEnumerable<ParserChar> inp)
        {
            throw new NotImplementedException();
        }
    }
}
