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
using NUnit.Framework;
using Monad.Parsec;
using Monad.Parsec.Language;

namespace Monad.UnitTests
{
    [TestFixture]
    public class TokTests
    {
        [Test]
        public void LexemeTest()
        {
            var lex = from l in Tok.Lexeme<ParserChar>(New.Character('A'))
                               select l;

            var res = lex.Parse("A");
            Assert.IsTrue(!res.IsFaulted);

            res = lex.Parse("A   ");
            Assert.IsTrue(!res.IsFaulted);
                                    
        }

        [Test]
        public void SymbolTest()
        {
            var sym = from s in Tok.Symbol("***")
                               select s;

            var res = sym.Parse("***   ");

            Assert.IsTrue(!res.IsFaulted);
        }

        [Test]
        public void OneLineComment()
        {
            var p = from v in Tok.OneLineComment(new HaskellDef())
                    select v;

            var res = p.Parse("-- This whole line is a comment");

            Assert.IsTrue(!res.IsFaulted);
        }


        [Test]
        public void MultiLineComment()
        {
            var p = from v in Tok.MultiLineComment(new HaskellDef())
                    select v;

            var res = p.Parse(
                @"{- This whole {- line is a comment
                     and so is -} this one with nested comments
                     this too -}  let x=1");

            var left = res.Value.Head().Item2.AsString();

            Assert.IsTrue(!res.IsFaulted &&  left == "  let x=1");
        }

        [Test]
        public void WhiteSpace()
        {
            var p = from v in Tok.WhiteSpace(new HaskellDef())
                    select v;

            var res = p.Parse(
                @"                              {- This whole {- line is a comment
                                                and so is -} this one with nested comments
                                                this too -}  let x=1");

            var left = res.Value.Head().Item2.AsString();

            Assert.IsTrue(!res.IsFaulted && left == "let x=1");
        }
    }
}

