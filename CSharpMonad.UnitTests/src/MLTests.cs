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

﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Monad;
using Monad.Parsec;

namespace Monad.UnitTests.ML
{
    //
    // Inspired by http://blogs.msdn.com/b/lukeh/archive/2007/08/19/monadic-parser-combinators-using-c-3-0.aspx
    //

    [TestFixture]
    public class MLTests
    {
        Parser<IEnumerable<ParserChar>> Id;
        Parser<IEnumerable<ParserChar>> Ident;
        Parser<IEnumerable<ParserChar>> LetId;
        Parser<IEnumerable<ParserChar>> InId;
        Parser<Term> Term;
        Parser<Term> Term1;
        Parser<Term> Parser;

        [Test]
        public void BuildMLParser()
        {
            Id = from w in New.WhiteSpace()
                 from c in New.Letter()
                 from cs in New.Many(New.LetterOrDigit())
                 select c.Cons(cs);

            Ident = from s in Id 
                    where s.IsNotEqualTo("let") && s.IsNotEqualTo("in") 
                    select s;

            LetId = from s in Id
                    where s.IsEqualTo("let")
                    select s;

            InId = from s in Id
                   where s.IsEqualTo("in")
                   select s;

            Term1 = (from x in Ident
                     select new VarTerm(x) as Term)
                    .Or(from u1 in Lang.WsChr('(')
                        from t in Term
                        from u2 in Lang.WsChr(')')
                        select t);

            Term = (from u1 in Lang.WsChr('\\')
                    from x in Ident
                    from u2 in Lang.WsChr('.')
                    from t in Term
                    select new LambdaTerm(x, t) as Term)
                   .Or(from lid in LetId
                       from x in Ident
                       from u1 in Lang.WsChr('=')
                       from t in Term
                       from inid in InId
                       from c in Term
                       select new LetTerm(x, t, c) as Term)
                   .Or(from t in Term1
                       from ts in New.Many(Term1)
                       select new AppTerm(t, ts) as Term);

            Parser = from t in Term
                     from u in Lang.WsChr(';')
                     select t;
        }

        [Test]
        public void TestIdParser()
        {
            BuildMLParser();

            var r = Id.Parse("Testing123");
            Assert.IsTrue(!r.IsFaulted);
            Assert.IsTrue(r.Value.First().Item1.AsString() == "Testing123");

            r = Id.Parse("   Testing123   ");
            Assert.IsTrue(!r.IsFaulted);
            Assert.IsTrue(r.Value.First().Item1.AsString() == "Testing123");

            r = Id.Parse("   1Testing123   ");
            Assert.IsTrue(r.IsFaulted);
            Assert.IsTrue(r.Errors.First().Location.Column == 4);
        }

        [Test]
        public void TestTerm1Parser()
        {
            BuildMLParser();

            var r = Term1.Parse("fred");
            Assert.IsTrue(!r.IsFaulted);
            Assert.IsTrue(r.Value.First().Item1.GetType().Name.Contains("VarTerm"));
        }

        [Test]
        public void TestApplTermParser()
        {
            BuildMLParser();

            var r = Term.Parse("add x y z w");
            Assert.IsTrue(!r.IsFaulted);
        }

        [Test]
        public void TestLetTermParser()
        {
            BuildMLParser();

            var r = Term.Parse("let add = x in y");
            Assert.IsTrue(!r.IsFaulted);
        }

        [Test]
        public void TestLetIdParser()
        {
            BuildMLParser();

            var r = LetId.Parse("let");
            Assert.IsTrue(!r.IsFaulted);
            Assert.IsTrue(r.Value.First().Item1.AsString() == "let");
        }

        [Test]
        public void TestInIdParser()
        {
            BuildMLParser();

            var r = InId.Parse("in");
            Assert.IsTrue(!r.IsFaulted);
            Assert.IsTrue(r.Value.First().Item1.AsString() == "in");
        }

        [Test]
        public void RunMLParser()
        {
            BuildMLParser();

            var input = @"let true = \x.\y.x in 
                          let false = \x.\y.y in 
                          let if = \b.\l.\r.(b l) r in
                          if true then false else true;".ToParserChar();

            var result = Parser.Parse(input);

            Assert.IsTrue(!result.IsFaulted);

            Term ast = result.Value.Single().Item1;

            // TODO: Check the validity of the produced AST

        }
    }

    public abstract class Term { }
    public class LambdaTerm : Term
    {
        public readonly IEnumerable<ParserChar> Ident; 
        public readonly Term Term;
        public LambdaTerm(IEnumerable<ParserChar> i, Term t)
        {
            Ident = i; 
            Term = t;
        }
    }

    public class LetTerm : Term
    {
        public readonly IEnumerable<ParserChar> Ident; 
        public readonly Term Rhs; 
        public Term Body;
        public LetTerm(IEnumerable<ParserChar> i, Term r, Term b)
        {
            Ident = i; 
            Rhs = r; 
            Body = b;
        }
    }
    public class AppTerm : Term
    {
        public readonly Term Func; 
        public readonly IEnumerable<Term> Args;
        public AppTerm(Term func, IEnumerable<Term> args)
        {
            Func = func; 
            Args = args;
        }
    }
    public class VarTerm : Term
    {
        public readonly IEnumerable<ParserChar> Ident;
        public VarTerm(IEnumerable<ParserChar> ident)
        {
            Ident = ident;
        }
    }


    public class WsChrParser : Parser<ParserChar>
    {
        public WsChrParser(char c)
            :
            base(
                inp => New.WhiteSpace()
                          .And(New.Character(c))
                          .Parse(inp)
            )
        {
        }
    }


    public static class Lang
    {
        public static WsChrParser WsChr(char c)
        {
            return new WsChrParser(c);
        }
    }
}