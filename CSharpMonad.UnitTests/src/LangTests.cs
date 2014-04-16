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

namespace Monad.UnitTests.Lang
{
    [TestFixture]
    public class LangTests
    {
        Parser<IEnumerable<ParserChar>> Id;
        Parser<IEnumerable<ParserChar>> Ident;
        Parser<IEnumerable<ParserChar>> LetId;
        Parser<IEnumerable<ParserChar>> Semi;
        Parser<IEnumerable<ParserChar>> LambdaArrow;
        Parser<IEnumerable<ParserChar>> InId;
        Parser<IEnumerable<ParserChar>> Op;
        Parser<Term> Integer;
        Parser<Term> String;
        Parser<Term> Term;
        Parser<Term> Term1;
        Parser<Term> Parser;

        [Test]
        public void BuildLangParser()
        {
            var opChars = ";.,<>?/\\|\"':=+-_*&^%$£@!".AsEnumerable();

            Id = from w in Gen.WhiteSpace()
                 from c in Gen.Letter()
                 from cs in Gen.Many(Gen.LetterOrDigit())
                 select c.Cons(cs);

            Op = (from w in Gen.WhiteSpace()
                  from o in Gen.Satisfy(c => opChars.Contains(c), "an operator")
                  from os in Gen.Many(Gen.Satisfy(c => opChars.Contains(c), "an operator"))
                  select o.Cons(os))
                 .Fail("an operator");

            Ident = (from s in Id
                     where
                        s.IsNotEqualTo("let") &&
                        s.IsNotEqualTo("in")
                     select s)
                    .Fail("identifier");

            LetId = (from s in Id
                     where s.IsEqualTo("let")
                     select s)
                    .Fail("let");

            InId = (from s in Id
                    where s.IsEqualTo("in")
                    select s)
                   .Fail("'in'");

            Semi = (from s in Op
                    where s.IsEqualTo(";")
                    select s)
                   .Fail("';'");

            LambdaArrow = (from s in Op
                           where s.IsEqualTo("=>")
                           select s)
                          .Fail("a lambda arrow '=>'");

            Integer = (from w in Gen.WhiteSpace()
                       from d in Gen.Integer()
                       select new IntegerTerm(d) as Term)
                      .Fail("an integer");

            String = (from w in Gen.WhiteSpace()
                      from o in Gen.Character('"')
                      from cs in Gen.Many(Gen.Satisfy(c => c != '"'))
                      from c in Gen.Character('"')
                      select new StringTerm(cs) as Term)
                     .Fail("a string literal");

            Term1 = Integer
                    | String
                    | (from x in Ident
                       select new VarTerm(x) as Term)
                    | (from u1 in Lang.WsChr('(')
                       from t in Term
                       from u2 in Lang.WsChr(')')
                       select t)
                    .Fail("a term");

            Term = (from x in Ident
                    from arrow in LambdaArrow
                    from t in Term
                    select new LambdaTerm(x, t) as Term)
                    | (from lid in LetId
                       from x in Ident
                       from u1 in Lang.WsChr('=')
                       from t in Term
                       from s in Semi
                       from c in Term
                       select new LetTerm(x, t, c) as Term)
                    | (from t in Term1
                       from ts in Gen.Many(Term1)
                       select new AppTerm(t, ts) as Term)
                    .Fail("a term");

            Parser = from t in Term
                     from u in Lang.WsChr(';')
                     from w in Gen.WhiteSpace()
                     select t;
        }


        [Test]
        public void RunLangParser()
        {
            BuildLangParser();

            var input = @"
                let value = 1234;
                let str = ""hello, world"";
                let t = str;
                let fn = x => x;
                fn value;";

            var result = Parser.Parse(input);

            if (result.IsFaulted)
            {
                string errs = System.String.Join("\n",
                    result.Errors.Select(e=>
                        e.Message + "Expected " + e.Expected + " at " + e.Location + 
                        " - " + e.Input.AsString().Substring(0, Math.Min(30, e.Input.AsString().Length))
                        + "...")
                    );
                Console.WriteLine(errs);
            }

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

    public class StringTerm : Term
    {
        public readonly IEnumerable<ParserChar> Value;

        public StringTerm(IEnumerable<ParserChar> cs)
        {
            Value = cs;
        }
    }

    public class IntegerTerm : Term
    {
        public readonly int Value;

        public IntegerTerm(int v)
        {
            Value = v;
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
                inp => Gen.WhiteSpace()
                .And(Gen.Character(c))
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