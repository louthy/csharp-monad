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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Monad;
using Monad.Parsec;
using Monad.Parsec.Language;
using Monad.Parsec.Token;

namespace Monad.UnitTests.Lex
{
    [TestFixture]
    public class LexerTests
    {
        class Lang : EmptyDef
        {
            public Lang()
            {
                ReservedOpNames = new string[] { "+", "*", "-", ";" };
                ReservedNames = new string[] { "def", "extern" };
                CommentLine = "#";
            }
        }

        public class Term : Token
        {
            SrcLoc location;

            public Term(SrcLoc location)
                :
                base(location)
            {
                this.location = location;
            }
        }

        public class Integer : Term
        {
            public IntegerToken Value;
            public Integer(IntegerToken t, SrcLoc location = null)
                :
                base(location)
            {
                Value = t;
            }
        }

        public class Var : Term
        {
            public IdentifierToken Id;
            public Var(IdentifierToken id, SrcLoc location = null)
                :
                base(location)
            {
                Id = id;
            }
        }

        [Test]
        public void LexerTest()
        {
            var def = new Lang();
            var lexer = Tok.MakeTokenParser(def);

            // Lexer
            var intlex = lexer.Integer;
            var floatlex = lexer.Float;
            var parens = lexer.ParensM;
            var commaSep = lexer.CommaSep;
            var semiSep = lexer.SemiSep;
            var identifier = lexer.Identifier;
            var reserved = lexer.Reserved;
            var reservedOp = lexer.ReservedOp;

            // Parser
            var integer = from n in intlex
                          select new Integer(n);

            //var expr = Ex.BuildExpressionParser 

            var variable = from v in identifier
                           select new Var(v.First());

            var manyargs = parens(from ts in New.Many(variable) 
                                  select ts as IEnumerable<Token>);

//            var function = from d in reserved("def")
//                           from name in identifier
//                           from args in manyargs
//                           from body in expr
 //                          select new Function(name, args, body);

            /*

            if (result.IsFaulted)
            {
                string errs = System.String.Join("\n",
                    result.Errors.Select(e =>
                        e.Message + "Expected " + e.Expected + " at " + e.Location +
                        " - " + e.Input.AsString().Substring(0, Math.Min(30, e.Input.AsString().Length))
                        + "...")
                    );
                Console.WriteLine(errs);
            }

            Assert.IsTrue(!result.IsFaulted);
            */
        }
    }
}
