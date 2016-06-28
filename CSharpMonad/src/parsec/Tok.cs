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

using Monad.Parsec.Token;
using Monad.Parsec.Token.Chars;
using Monad.Utility;

namespace Monad.Parsec
{
    /// <summary>
    /// Utility class for quick intellisensed access to token parsers
    /// </summary>
    public static class Tok
    {
        public static TokenParser<T> MakeTokenParser<T>(LanguageDef def)
            where T : Token.Token
        {
            return new TokenParser<T>(def);
        }

        /// <summary>
        /// Lexeme
        /// </summary>
        public static Lexeme<A> Lexeme<A>(Parser<A> parser)
        {
            return new Lexeme<A>(parser);
        }

        /// <summary>
        /// Symbol
        /// </summary>
        public static Symbol Symbol(string name)
        {
            return new Symbol(name);
        }

        /// <summary>
        /// One line comment
        /// </summary>
        public static OneLineComment OneLineComment(LanguageDef def)
        {
            return new OneLineComment(def);
        }

        /// <summary>
        /// Multi line comment
        /// </summary>
        public static MultiLineComment MultiLineComment(LanguageDef def)
        {
            return new MultiLineComment(def);
        }

        /// <summary>
        /// Whitespace 
        /// </summary>
        public static Monad.Parsec.Token.WhiteSpace WhiteSpace(LanguageDef def)
        {
            return new Monad.Parsec.Token.WhiteSpace(def);
        }

        /// <summary>
        /// Skip spaces - Different to the general-case SkipSpace which will return
        /// the space it consumes.
        /// </summary>
        public static Parser<Unit> SimpleSpace()
        {
            return Prim.SkipMany1( Prim.OneOf(" \t\n\r") );
        }

        /// <summary>
        /// Chars
        /// </summary>
        public static class Chars
        {
            /// <summary>
            /// Char literal ('c')
            /// </summary>
            /// <returns></returns>
            public static CharLiteral CharLiteral()
            {
                return new CharLiteral();
            }

            /// <summary>
            /// Escape character (\) followed by an accepted escape code
            /// </summary>
            /// <returns></returns>
            public static CharEscape CharEscape()
            {
                return new CharEscape();
            }

            /// <summary>
            /// Escape code set parser without the \ prefix
            /// </summary>
            /// <returns></returns>
            public static EscapeCode EscapeCode()
            {
                return new EscapeCode();
            }
        }

        /// <summary>
        /// Strings
        /// </summary>
        public static class Strings
        {
            public static Token.Strings.StringLiteral StringLiteral()
            {
                return new Token.Strings.StringLiteral();
            }
        }

        /// <summary>
        /// Numbers
        /// </summary>
        public static class Numbers
        {
            /// <summary>
            /// Integer token
            /// </summary>
            public static Token.Numbers.Integer Integer()
            {
                return new Token.Numbers.Integer();
            }

            /// <summary>
            /// Natural number
            /// </summary>
            public static Token.Numbers.Natural Natural()
            {
                return new Token.Numbers.Natural();
            }

            /// <summary>
            /// Decimal token
            /// Base 10 integer
            /// </summary>
            public static Token.Numbers.Decimal Decimal()
            {
                return new Token.Numbers.Decimal();
            }

            /// <summary>
            /// Hexadecimal token
            /// Base 16 integer
            /// </summary>
            public static Token.Numbers.Hexadecimal Hexadecimal()
            {
                return new Token.Numbers.Hexadecimal();
            }

            /// <summary>
            /// Octal token
            /// Base 8 integer
            /// </summary>
            public static Token.Numbers.Octal Octal()
            {
                return new Token.Numbers.Octal();
            }
        }

        /// <summary>
        /// Bracketing
        /// </summary>
        public static class Bracketing
        {
            public static Token.Bracketing.Parens<A> Parens<A>(Parser<A> betweenParser)
            {
                return new Token.Bracketing.Parens<A>(betweenParser);
            }
            public static Token.Bracketing.Braces<A> Braces<A>(Parser<A> betweenParser)
            {
                return new Token.Bracketing.Braces<A>(betweenParser);
            }
            public static Token.Bracketing.Angles<A> Angles<A>(Parser<A> betweenParser)
            {
                return new Token.Bracketing.Angles<A>(betweenParser);
            }
            public static Token.Bracketing.Brackets<A> Brackets<A>(Parser<A> betweenParser)
            {
                return new Token.Bracketing.Brackets<A>(betweenParser);
            }
        }

        /// <summary>
        /// Identifiers and reserved words
        /// </summary>
        public static class Id
        {
            public static Token.Idents.Identifier Identifier(LanguageDef def)
            {
                return new Token.Idents.Identifier(def);
            }
            public static Token.Idents.Reserved Reserved(string name, LanguageDef def)
            {
                return new Token.Idents.Reserved(name, def);
            }
        }

        /// <summary>
        /// Identifiers and reserved words
        /// </summary>
        public static class Ops
        {
            public static Token.Ops.Operator Operator(LanguageDef def)
            {
                return new Token.Ops.Operator(def);
            }
            public static Token.Ops.ReservedOp ReservedOp(string name, LanguageDef def)
            {
                return new Token.Ops.ReservedOp(name, def);
            }
        }
    }
}
