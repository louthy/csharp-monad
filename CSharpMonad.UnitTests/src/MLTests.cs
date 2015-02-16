﻿////////////////////////////////////////////////////////////////////////////////////////
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

﻿using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Monad;
using Monad.Parsec;
using Monad.Utility;

namespace Monad.UnitTests.ML
{
    //
    // Inspired by http://blogs.msdn.com/b/lukeh/archive/2007/08/19/monadic-parser-combinators-using-c-3-0.aspx
    //
	
    public class MLTests
    {
        Parser<ImmutableList<ParserChar>> Id;
        Parser<ImmutableList<ParserChar>> Ident;
        Parser<ImmutableList<ParserChar>> LetId;
        Parser<ImmutableList<ParserChar>> InId;
        Parser<Term> Term;
        Parser<Term> Term1;
        Parser<Term> Parser;

        [Fact]
        public void BuildMLParser()
        {
            var ws = Prim.WhiteSpace();// | New.Return(new ParserChar(' '));

            Id = from w in ws
                 from c in Prim.Letter()
                 from cs in Prim.Many(Prim.LetterOrDigit())
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
                    | (from u1 in Lang.WsChr('(')
                       from t in Term
                       from u2 in Lang.WsChr(')')
                       select t);

            Term = (from u1 in Lang.WsChr('\\')
                    from x in Ident
                    from u2 in Lang.WsChr('.')
                    from t in Term
                    select new LambdaTerm(x, t) as Term)
                    | (from lid in LetId
                       from x in Ident
                       from u1 in Lang.WsChr('=')
                       from t in Term
                       from inid in InId
                       from c in Term
                       select new LetTerm(x, t, c) as Term)
                    | (from t in Term1
                       from ts in Prim.Many(Term1)
                       select new AppTerm(t, ts) as Term);

            Parser = from t in Term
                     from u in Lang.WsChr(';')
                     select t;
        }

        [Fact]
        public void TestIdParser()
        {
            BuildMLParser();

            var r = Id.Parse("Testing123");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.First().Item1.AsString() == "Testing123");

            r = Id.Parse("    Testing123   ");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.First().Item1.AsString() == "Testing123");

            r = Id.Parse("   1Testing123   ");
            Assert.True(r.IsFaulted);
            Assert.True(r.Errors.First().Location.Column == 4);
        }

        [Fact]
        public void TestTerm1Parser()
        {
            BuildMLParser();

            var r = Term1.Parse("fred");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.First().Item1.GetType().Name.Contains("VarTerm"));
            Assert.True(r.Value.First().Item2.IsEmpty);
        }

        [Fact]
        public void TestApplTermParser()
        {
            BuildMLParser();

            var r = Term.Parse("add x y z w");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.First().Item2.IsEmpty);
        }

        [Fact]
        public void TestLetTermParser()
        {
            BuildMLParser();

            var r = Term.Parse("let add = x in y");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.First().Item2.IsEmpty);
        }

        [Fact]
        public void TestLetIdParser()
        {
            BuildMLParser();

            var r = LetId.Parse("let");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.First().Item1.AsString() == "let");
            Assert.True(r.Value.First().Item2.IsEmpty);
        }

        [Fact]
        public void TestInIdParser()
        {
            BuildMLParser();

            var r = InId.Parse("in");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.First().Item1.AsString() == "in");
            Assert.True(r.Value.First().Item2.IsEmpty);
        }

        [Fact]
        public void RunMLParser()
        {
            BuildMLParser();

            var input = @"let true = \x.\y.x in 
                          let false = \x.\y.y in 
                          let if = \b.\l.\r.(b l) r in
                          if true then false else true;".ToParserChar();

            var result = Parser.Parse(input);

            Assert.True(!result.IsFaulted);
            Assert.True(result.Value.First().Item2.IsEmpty);

            Term ast = result.Value.Single().Item1;

            // TODO: Check the validity of the produced AST

        }
    }

    public abstract class Term { }
    public class LambdaTerm : Term
    {
        public readonly ImmutableList<ParserChar> Ident; 
        public readonly Term Term;
        public LambdaTerm(ImmutableList<ParserChar> i, Term t)
        {
            Ident = i; 
            Term = t;
        }
    }

    public class LetTerm : Term
    {
        public readonly ImmutableList<ParserChar> Ident; 
        public readonly Term Rhs; 
        public Term Body;
        public LetTerm(ImmutableList<ParserChar> i, Term r, Term b)
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
        public readonly ImmutableList<ParserChar> Ident;
        public VarTerm(ImmutableList<ParserChar> ident)
        {
            Ident = ident;
        }
    }


    public class WsChrParser : Parser<ParserChar>
    {
        public WsChrParser(char c)
            :
            base(
                inp => Prim.WhiteSpace()
                          .And(Prim.Character(c))
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