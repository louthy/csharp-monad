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

namespace Monad.Parsec.Token
{
    /// <summary>
    /// Token parser
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class TokenParser : GenTokenParser<Token>
    {
        public TokenParser(LanguageDef def)
            :
            base(def)
        {
        }
    }

    /// <summary>
    /// Generic token parser
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class GenTokenParser<A>
    {
        public readonly Parser<IEnumerable<IdentifierToken>> Identifier;
        public readonly Parser<IEnumerable<OperatorToken>> Operator;
        public readonly Parser<CharLiteralToken> CharLiteral;
        public readonly Parser<StringLiteralToken> StringLiteral;
        public readonly Parser<IntegerToken> Natural;
        public readonly Parser<IntegerToken> Integer;
        public readonly Parser<FloatToken> Float;
        public readonly Parser<NaturalOrFloatToken> NaturalOrFloat;
        public readonly Parser<IntegerToken> Decimal;
        public readonly Parser<IntegerToken> Hexadecimal;
        public readonly Parser<IntegerToken> Octal;
        public readonly Func<string, Parser<SymbolToken>> Symbol;
        public readonly Func<Parser<A>, Parser<A>> Lexeme;
        public readonly Parser<Unit> WhiteSpace;
        
        public readonly Func<Parser<A>, Monad.Parsec.Token.Bracketing.Parens<A>> Parens;
        public readonly Func<Parser<A>, Monad.Parsec.Token.Bracketing.Braces<A>> Braces;
        public readonly Func<Parser<A>, Monad.Parsec.Token.Bracketing.Angles<A>> Angles;
        public readonly Func<Parser<A>, Monad.Parsec.Token.Bracketing.Brackets<A>> Brackets;

        public readonly Func<Parser<IEnumerable<A>>, Monad.Parsec.Token.Bracketing.Parens<IEnumerable<A>>> ParensM;
        public readonly Func<Parser<IEnumerable<A>>, Monad.Parsec.Token.Bracketing.Braces<IEnumerable<A>>> BracesM;
        public readonly Func<Parser<IEnumerable<A>>, Monad.Parsec.Token.Bracketing.Angles<IEnumerable<A>>> AnglesM;
        public readonly Func<Parser<IEnumerable<A>>, Monad.Parsec.Token.Bracketing.Brackets<IEnumerable<A>>> BracketsM;
        
        public readonly Symbol Semi;
        public readonly Symbol Comma;
        public readonly Symbol Colon;
        public readonly Symbol Dot;
        public readonly Func<Parser<A>, Parser<IEnumerable<A>>> SemiSep;
        public readonly Func<Parser<A>, Parser<IEnumerable<A>>> SemiSep1;
        public readonly Func<Parser<A>, Parser<IEnumerable<A>>> CommaSep;
        public readonly Func<Parser<A>, Parser<IEnumerable<A>>> CommaSep1;

        private readonly IReadOnlyDictionary<string, Parser<IEnumerable<ReservedToken>>> reserved;
        private readonly IReadOnlyDictionary<string, Parser<IEnumerable<ReservedOpToken>>> reservedOp;

        public readonly Func<string, Parser<IEnumerable<ReservedToken>>> Reserved;
        public readonly Func<string, Parser<IEnumerable<ReservedOpToken>>> ReservedOp;

        public GenTokenParser(LanguageDef def)
        {
            Identifier = Tok.Id.Identifier(def);
            reserved = def.ReservedNames.ToDictionary(name => name, name => Tok.Id.Reserved(name, def) as Parser<IEnumerable<ReservedToken>>);
            Operator = Tok.Ops.Operator(def);
            reservedOp = def.ReservedOpNames.ToDictionary(name => name, name => Tok.Ops.ReservedOp(name, def) as Parser<IEnumerable<ReservedOpToken>>);
            CharLiteral = Tok.Chars.CharLiteral();
            StringLiteral = Tok.Strings.StringLiteral();
            Natural = Tok.Numbers.Natural();
            Integer = Tok.Numbers.Integer();
            
            // floating = Tok.Numbers.Floating(); TODO
            // naturalOrFloat = Tok.Numbers.NaturalOrFloating(); TODO

            WhiteSpace = Tok.WhiteSpace(def);
            Decimal = Tok.Numbers.Decimal();
            Hexadecimal = Tok.Numbers.Hexadecimal();
            Octal = Tok.Numbers.Octal();
            Symbol = (string name) => Tok.Symbol(name);
            Lexeme = (Parser<A> p) => Tok.Lexeme(p);

            Parens = (Parser<A> p) => Tok.Bracketing.Parens(p);
            Braces = (Parser<A> p) => Tok.Bracketing.Braces(p);
            Angles = (Parser<A> p) => Tok.Bracketing.Angles(p);
            Brackets = (Parser<A> p) => Tok.Bracketing.Brackets(p);

            ParensM = (Parser<IEnumerable<A>> p) => Tok.Bracketing.Parens(p);
            BracesM = (Parser<IEnumerable<A>> p) => Tok.Bracketing.Braces(p);
            AnglesM = (Parser<IEnumerable<A>> p) => Tok.Bracketing.Angles(p);
            BracketsM = (Parser<IEnumerable<A>> p) => Tok.Bracketing.Brackets(p);

            Semi = Tok.Symbol(";");
            Comma = Tok.Symbol(",");
            Colon = Tok.Symbol(":");
            Dot = Tok.Symbol(".");
            CommaSep = (Parser<A> p) => New.SepBy(p, Comma);
            SemiSep = (Parser<A> p) => New.SepBy(p, Semi);
            CommaSep1 = (Parser<A> p) => New.SepBy1(p, Comma);
            SemiSep1 = (Parser<A> p) => New.SepBy1(p, Semi);

            Reserved = name => reserved[name];
            ReservedOp = name => reservedOp[name];
        }
    }
}
