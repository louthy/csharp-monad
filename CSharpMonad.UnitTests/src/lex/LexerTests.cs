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
using Monad.Parsec.Expr;

namespace Monad.UnitTests.Lex
{
    [TestFixture]
    public class LexerTests
    {
        /// <summary>
        /// This test is work in progress.  It looks very pretty but does nothing at the moment.
        /// </summary>
        [Test]
        public void LexerTest()
        {
            Parser<Term> expr = null;
            Func<Parser<Term>, Parser<Term>> contents;

            Func<Parser<Term>,Parser<IEnumerable<Term>>> many = (Parser<Term> p) => New.Many(p);
            Func<Parser<Term>,Parser<IEnumerable<Term>>> @try = (Parser<Term> p) => New.Try(p);
            Func<IEnumerable<Term>,Term> fail = ts => ts.IsEmpty() ? new FailTerm() : ts.Head();

            var def = new Lang();
            var lexer = Tok.MakeTokenParser(def);
            var binops = BuildOperatorsTable<Term>();

            // Lexer
            var intlex = lexer.Integer;
            var floatlex = lexer.Float;
            var parens = lexer.ParensM;
            var commaSep = lexer.CommaSep;
            var semiSep = lexer.SemiSep;
            var identifier = lexer.Identifier;
            var reserved = lexer.Reserved;
            var reservedOp = lexer.ReservedOp;
            var whiteSpace = lexer.WhiteSpace;

            // Parser

            var integer = from n in intlex
                          select new Integer(n) as Term;

            var variable = from v in identifier
                           select new Var(v) as Term;

            var manyargs = parens(from ts in many(variable) 
                                  select ts as IEnumerable<Token>);

            var commaSepExpr = parens(from cs in commaSep(from t in expr select t as Token)
                                      select cs as IEnumerable<Token>);

            var function = from d in reserved("def")
                           from name in identifier
                           from args in manyargs
                           from body in expr
                           select new Function(name, args, body) as Term;

            var externFn = from _ in reserved("extern")
                           from name in identifier
                           from args in manyargs
                           select new Extern(name, args) as Term;

            var call = from name in identifier
                       from args in commaSepExpr
                       select new Call(name, args) as Term;

            var subexpr = (from ps in parens(from es in expr select es as IEnumerable<Token>)
                           select (from t in ps select new Expression(t) as Term));

            var factor = from f in @try(integer)
                         | @try(externFn)
                         | @try(function)
                         | @try(call)
                         | @try(variable)
                         | subexpr
                         select fail(f);

            var defn = from f in @try(externFn)
                       | @try(function)
                       | @try(expr)
                       select f.Head();

            contents = p =>
                from ws in whiteSpace
                from r in p
                select r;

            var toplevel = from ts in many(
                               from fn in defn
                               from semi in reservedOp(";")
                               select fn
                           )
                           select ts.Head();

            expr = Ex.BuildExpressionParser<Term>(binops, factor);

            Func<string,ParserResult<Term>> parseExpr = src => contents(expr).Parse(src);

            var result = parseExpr(TestData1);

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
        }

        class Lang : EmptyDef
        {
            public Lang()
            {
                ReservedOpNames = new string[] { "+", "*", "-", ";" };
                ReservedNames = new string[] { "def", "extern" };
                CommentLine = "#";
            }
        }

        private static OperatorTable<T> BuildOperatorsTable<T>()
        {
            var equals = new Operator<T>(new OperatorDef<T>(OperatorType.Infix, "=", a => a, Assoc.Left));
            var mult = new Operator<T>(new OperatorDef<T>(OperatorType.Infix, "*", a => a, Assoc.Left));
            var divide = new Operator<T>(new OperatorDef<T>(OperatorType.Infix, "/", a => a, Assoc.Left));
            var plus = new Operator<T>(new OperatorDef<T>(OperatorType.Infix, "+", a => a, Assoc.Left));
            var minus = new Operator<T>(new OperatorDef<T>(OperatorType.Infix, "-", a => a, Assoc.Left));
            var lessThan = new Operator<T>(new OperatorDef<T>(OperatorType.Infix, "<", a => a, Assoc.Left));

            var prec0 = equals.Cons();
            var prec1 = mult.Cons(); prec1 = divide.Cons(prec1);
            var prec2 = plus.Cons(); prec2 = minus.Cons(prec2);
            var prec3 = lessThan.Cons();
            var binops = new OperatorTable<T>(new IEnumerable<Operator<T>>[] { prec0, prec1, prec2, prec3 });
            return binops;
        }

        public class Term : Token
        {
            public readonly SrcLoc Location;

            public Term(SrcLoc location)
                :
                base(location)
            {
                Location = location;
            }
        }

        public class FailTerm : Term
        {
            // TODO: This class existing means there's bugs in the code somewhere
            public FailTerm()
                :
                base(null)
            {}
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

        public class Function : Term
        {
            public IdentifierToken Id;
            public IEnumerable<Token> Args;
            public Token Body;

            public Function(IdentifierToken id, IEnumerable<Token> args, Token body, SrcLoc location = null)
                :
                base(location)
            {
                Id = id;
            }
        }

        public class Extern : Term
        {
            public IdentifierToken Id;
            public IEnumerable<Token> Args;

            public Extern(IdentifierToken id, IEnumerable<Token> args, SrcLoc location = null)
                :
                base(location)
            {
                Id = id;
            }
        }

        public class Call : Term
        {
            public IdentifierToken Name;
            public IEnumerable<Token> Args;

            public Call(IdentifierToken name, IEnumerable<Token> args, SrcLoc location = null)
                :
                base(location)
            {
                Name = name;
                Args = args;
            }
        }

        public class Expression : Term
        {
            public Token Expr;
            public Expression(Token expr, SrcLoc location = null)
                :
                base(location)
            {
                Expr = expr;
            }
        }

        private static string TestData1 = @"def foo(x y) x+foo(y, 4.0);
            def foo(x y) x+y y;
            def foo(x y) x+y );
            extern sin(a);
            }";

    }
}
