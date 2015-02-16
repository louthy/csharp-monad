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

using Xunit;
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
using Monad.Utility;
using System.Diagnostics;

namespace Monad.UnitTests.Lex
{
    public class LexerTests
    {
        private static string TestData1 =
            @"def foo(x y) 1=2;";

        private static string TestData2 =
          @"def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);";

        private static string TestData3=
            @"1+1";

        /// <summary>
        /// Fully working language lexer and parser
        /// </summary>
        [Fact]
        public void LexerTest()
        {
            Parser<Term> exprlazy = null;
            Parser<Term> expr = Prim.Lazy<Term>(() => exprlazy);
            Func<Parser<Term>, Parser<Term>> contents;
            Func<Parser<Term>,Parser<ImmutableList<Term>>> many = Prim.Many;
            Func<Parser<Term>,Parser<Term>> @try = Prim.Try;

            var def = new Lang();
            var lexer = Tok.MakeTokenParser<Term>(def);
            var binops = BuildOperatorsTable<Term>(lexer);

            // Lexer
            var intlex = lexer.Integer;
            var floatlex = lexer.Float;
            var parens = lexer.Parens;
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
                                  select new Arguments(ts) as Term);

            var commaSepExpr = parens(from cs in commaSep(expr)
                                      select new Exprs(cs) as Term);

            var function = from _ in reserved("def")
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
                       select new Call(name, args as Exprs) as Term;

            var subexpr = (from p in parens(expr)
                           select new Expression(p) as Term);

            var factor = from f in @try(integer)
                         | @try(externFn)
                         | @try(function)
                         | @try(call)
                         | @try(variable)
                         | subexpr
                         select f;

            var defn = from f in @try(externFn)
                       | @try(function)
                       | @try(expr)
                       select f;

            contents = p =>
                from ws in whiteSpace
                from r in p
                select r;

            var toplevel = from ts in many(
                               from fn in defn
                               from semi in reservedOp(";")
                               select fn
                           )
                           select ts;

            exprlazy = Ex.BuildExpressionParser<Term>(binops, factor);

            var watch = Stopwatch.StartNew();
            var result = toplevel.Parse(TestData4);
            watch.Stop();
            var time = watch.ElapsedMilliseconds;

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

            var resu = result.Value.First().Item1;
            var left = result.Value.First().Item2.AsString();

            Assert.True(left.Length == 0);
            Assert.True(!result.IsFaulted);
        }

        class Lang : EmptyDef
        {
            public Lang()
            {
                ReservedOpNames = new string[] { "+", "*", "-", ";", "/", "<", "=" };
                ReservedNames = new string[] { "def", "extern" };
                CommentLine = "#";
            }
        }

        private static OperatorTable<T> BuildOperatorsTable<T>(TokenParser<T> lexer)
            where T : Token
        {
            Func<T, T, ReservedOpToken, T> fn = (lhs, rhs, op) =>
            {
                return new BinaryOp(lhs,rhs,op) as T;
            };

            Func<ReservedOpToken,Func<T,T,T>> binop = op => new Func<T,T,T>( (T lhs, T rhs) =>
            {
                return fn(lhs, rhs, op);
            });

            Func<string, Parser<Func<T, T, T>>> resOp = name => from op in lexer.ReservedOp(name) select binop(op);

            var equals =    new Infix<T>("=", resOp("="), Assoc.Left);
            var mult =      new Infix<T>("*", resOp("*"), Assoc.Left);
            var divide =    new Infix<T>("/", resOp("/"), Assoc.Left);
            var plus =      new Infix<T>("+", resOp("+"), Assoc.Left);
            var minus =     new Infix<T>("-", resOp("-"), Assoc.Left);
            var lessThan =  new Infix<T>("<", resOp("<"), Assoc.Left);

            var binops = new OperatorTable<T>();
            binops.AddRow().Add(equals)
                  .AddRow().Add(mult).Add(divide)
                  .AddRow().Add(plus).Add(minus)
                  .AddRow().Add(lessThan);

            return binops;
        }

        public class BinaryOp : Term
        {
            public readonly Token Lhs;
            public readonly Token Rhs;
            public readonly Token Op;

            public BinaryOp(Token lhs, Token rhs, Token op, SrcLoc loc = null)
                :
                base(loc)
            {
                Lhs = lhs;
                Rhs = rhs;
                Op = op;
            }
        }

        public class Term : Token
        {
            public Term(SrcLoc location)
                :
                base(location)
            {
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

        public class Function : Term
        {
            public IdentifierToken Id;
            public Arguments Args;
            public Token Body;

            public Function(IdentifierToken id, Term args, Token body, SrcLoc location = null)
                :
                base(location)
            {
                Id = id;
                Args = args as Arguments;
                Body = body;
            }
        }

        public class Extern : Term
        {
            public IdentifierToken Id;
            public Arguments Args;

            public Extern(IdentifierToken id, Term args, SrcLoc location = null)
                :
                base(location)
            {
                Id = id;
                Args = args as Arguments;
            }
        }

        public class Call : Term
        {
            public IdentifierToken Name;
            public Exprs Args;

            public Call(IdentifierToken name, Exprs args, SrcLoc location = null)
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

        public class Arguments : Term
        {
            public IEnumerable<Token> Args;
            public Arguments(IEnumerable<Token> args, SrcLoc location = null)
                :
                base(location)
            {
                Args = args;
            }
        }

        public class Exprs : Term
        {
            public IEnumerable<Token> List;
            public Exprs(IEnumerable<Token> exprs, SrcLoc location = null)
                :
                base(location)
            {
                List = exprs;
            }
        }

        private static string TestData4 =
          @"def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);
            def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;
            extern sin(a);def foo(x y) x+foo(y, 4);
            def foo(x y) x+y*2;
            def foo(x y) x+y;";

    }
}
