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
using Xunit;
using Monad.Parsec;
using Monad.Parsec.Language;

namespace Monad.UnitTests
{
    public class TokTests
    {
        [Fact]
        public void LexemeTest()
        {
            var lex = from l in Tok.Lexeme<ParserChar>(Prim.Character('A'))
                               select l;

            var res = lex.Parse("A");
            Assert.True(!res.IsFaulted);

            res = lex.Parse("A   ");
            Assert.True(!res.IsFaulted);
                                    
        }

        [Fact]
        public void SymbolTest()
        {
            var sym = from s in Tok.Symbol("***")
                               select s;

            var res = sym.Parse("***   ");

            Assert.True(!res.IsFaulted);
        }

        [Fact]
        public void OneLineComment()
        {
            var p = from v in Tok.OneLineComment(new HaskellDef())
                    select v;

            var res = p.Parse("-- This whole line is a comment");

            Assert.True(!res.IsFaulted);
        }


        [Fact]
        public void MultiLineComment()
        {
            var p = from v in Tok.MultiLineComment(new HaskellDef())
                    select v;

            var res = p.Parse(
                @"{- This whole {- line is a comment
                     and so is -} this one with nested comments
                     this too -}  let x=1");

            var left = res.Value.Head().Item2.AsString();

            Assert.True(!res.IsFaulted &&  left == "  let x=1");
        }

        [Fact]
        public void WhiteSpaceTest()
        {
            var p = from v in Tok.WhiteSpace(new HaskellDef())
                    select v;

            var res = p.Parse(
                @"                              {- This whole {- line is a comment
                                                and so is -} this one with nested comments
                                                this too -}  let x=1");

            var left = res.Value.Head().Item2.AsString();

            Assert.True(!res.IsFaulted && left == "let x=1");
        }

        [Fact]
        public void CharLiteralTest()
        {
            var p = from v in Tok.Chars.CharLiteral()
                    select v;

            var res = p.Parse("'a'  abc");
            var left = res.Value.Head().Item2.AsString();

            Assert.True(!res.IsFaulted && left == "abc");
            Assert.True(res.Value.Head().Item1.Value.Value == 'a');

            res = p.Parse("'\\n'  abc");
            Assert.True(res.Value.Head().Item1.Value.Value == '\n');
        }

        [Fact]
        public void StringLiteralTest()
        {
            var p = from v in Tok.Strings.StringLiteral()
                    select v;

            var res = p.Parse("\"abc\"  def");
            var left = res.Value.Head().Item2.AsString();

            Assert.True(!res.IsFaulted && left == "def");
            Assert.True(res.Value.Head().Item1.Value.AsString() == "abc");

            res = p.Parse("\"ab\\t\\nc\"  def");
            Assert.True(res.Value.Head().Item1.Value.AsString() == "ab\t\nc");
        }


        [Fact]
        public void NumbersIntegerTest()
        {
            var p = from v in Tok.Numbers.Integer()
                    select v;

            var res = p.Parse("1234  def");
            var left = res.Value.Head().Item2.AsString();

            Assert.True(!res.IsFaulted && left == "def");
            Assert.True(res.Value.Head().Item1.Value == 1234);
        }

        [Fact]
        public void NumbersHexTest()
        {
            var p = from v in Tok.Numbers.Hexadecimal()
                    select v;

            var res = p.Parse("xAB34");
            var left = res.Value.Head().Item2.AsString();

            Assert.True(!res.IsFaulted);
            Assert.True(res.Value.Head().Item1.Value == 0xAB34);
        }

        [Fact]
        public void NumbersOctalTest()
        {
            var p = from v in Tok.Numbers.Octal()
                    select v;

            var res = p.Parse("o777");
            var left = res.Value.Head().Item2.AsString();

            Assert.True(!res.IsFaulted);
            Assert.True(res.Value.Head().Item1.Value == 511);
        }

        [Fact]
        public void TestParens()
        {
            var r = Tok.Bracketing.Parens(Tok.Chars.CharLiteral()).Parse("( 'a' )");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.Head().Item1.Value.Value == 'a');
        }

        [Fact]
        public void TestBraces()
        {
            var r = Tok.Bracketing.Braces(Tok.Chars.CharLiteral()).Parse("{ 'a' }");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.Head().Item1.Value.Value == 'a');
        }

        [Fact]
        public void TestBrackets()
        {
            var r = Tok.Bracketing.Brackets(Tok.Chars.CharLiteral()).Parse("[ 'a' ]");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.Head().Item1.Value.Value == 'a');
        }

        [Fact]
        public void TestAngles()
        {
            var r = Tok.Bracketing.Angles(Tok.Chars.CharLiteral()).Parse("< 'a' >");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.Head().Item1.Value.Value == 'a');
        }

        [Fact]
        public void TestIdentifier()
        {
            var r = Tok.Id.Identifier(new HaskellDef()).Parse("onetWothree123  ");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.Head().Item1.Value.AsString() == "onetWothree123");
        }

        [Fact]
        public void TestReserved()
        {
            var def = new HaskellDef();
            var r = Tok.Id.Reserved(def.ReservedNames.Head(), def).Parse(def.ReservedNames.Head() + "  ");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.Head().Item1.Value.AsString() == def.ReservedNames.Head());
        }

        [Fact]
        public void TestOperator()
        {
            var def = new HaskellDef();
            var r = Tok.Ops.Operator(def).Parse("&&*  ");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.Head().Item1.Value.AsString() == "&&*");
        }

        [Fact]
        public void TestReservedOperator()
        {
            var def = new HaskellDef();
            var r = Tok.Ops.ReservedOp("=>",def).Parse("=>  ");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.Head().Item1.Value.AsString() == "=>");
        }
    }
}

