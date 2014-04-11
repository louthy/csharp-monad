using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Monad;
using Monad.Parsec;

namespace CSharpMonad.UnitTests.ML
{
    //
    // Inspired by http://blogs.msdn.com/b/lukeh/archive/2007/08/19/monadic-parser-combinators-using-c-3-0.aspx
    //

    [TestClass]
    public class MLTests
    {
        Parser<IEnumerable<ParserChar>> Id;
        Parser<IEnumerable<ParserChar>> Ident;
        Parser<IEnumerable<ParserChar>> LetId;
        Parser<IEnumerable<ParserChar>> InId;
        Parser<Term> Term;
        Parser<Term> Term1;
        Parser<Term> Parser;

        [TestMethod]
        public void BuildMLParser()
        {
            Id = from w in New.Whitespace()
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
                       from ts in New.Many1(Term1)
                       select new AppTerm(t, ts) as Term);

            Parser = from t in Term
                     from u in Lang.WsChr(';')
                     select t;
        }

        [TestMethod]
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
            Assert.IsTrue(r.Errors.First().Input.First().Column == 4);
        }

        [TestMethod]
        public void TestTerm1Parser()
        {
            BuildMLParser();

            var r = Term1.Parse("fred");
            Assert.IsTrue(!r.IsFaulted);
            Assert.IsTrue(r.Value.First().Item1.GetType().Name.Contains("VarTerm"));
        }

        [TestMethod]
        public void TestLetTermParser()
        {
            BuildMLParser();

            var r = Term.Parse("let fred=1");
            if (r.IsFaulted)
            {
                throw new Exception(r.Errors.First().Message);
            }
            Assert.IsTrue(!r.IsFaulted);
            Assert.IsTrue(r.Value.First().Item1.GetType().Name.Contains("LetTerm"));

        }

        [TestMethod]
        public void TestLetIdParser()
        {
            BuildMLParser();

            var r = LetId.Parse("let");
            Assert.IsTrue(!r.IsFaulted);
            Assert.IsTrue(r.Value.First().Item1.AsString() == "let");
        }

        [TestMethod]
        public void TestInIdParser()
        {
            BuildMLParser();

            var r = InId.Parse("in");
            Assert.IsTrue(!r.IsFaulted);
            Assert.IsTrue(r.Value.First().Item1.AsString() == "in");
        }

        [TestMethod]
        public void RunMLParser()
        {
            BuildMLParser();

            var input = @"let true = \x.\y.x in 
                          let false = \x.\y.y in 
                          let if = \b.\l.\r.(b l) r in
                          if true then false else true;".ToParserChar();

            var result = Parser.Parse(input);

            Assert.IsTrue(!result.IsFaulted);

            // TODO: This doesn't parse, need to work out why

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
                inp => New.Whitespace()
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