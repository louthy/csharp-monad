using Monad.Parsec.Token;
using Monad.Parsec.Token.Chars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec
{
    public static class Tok
    {
        public static TokenParser<A> MakeTokenParser<A>(LanguageDef def)
        {
            return new TokenParser<A>(def);
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
        /// <param name="commentStart"></param>
        /// <returns></returns>
        public static OneLineComment OneLineComment(string commentStart)
        {
            return new OneLineComment(commentStart);
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
            public static Token.Idents.Identifier Identifier(GeneralLanguageDef def)
            {
                return new Token.Idents.Identifier(def);
            }
            public static Token.Idents.Reserved Reserved(string name, GeneralLanguageDef def)
            {
                return new Token.Idents.Reserved(name, def);
            }
        }

        /// <summary>
        /// Identifiers and reserved words
        /// </summary>
        public static class Ops
        {
            public static Token.Ops.Operator Operator(GeneralLanguageDef def)
            {
                return new Token.Ops.Operator(def);
            }
            public static Token.Ops.ReservedOp ReservedOp(string name, GeneralLanguageDef def)
            {
                return new Token.Ops.ReservedOp(name, def);
            }
        }
    }
}
