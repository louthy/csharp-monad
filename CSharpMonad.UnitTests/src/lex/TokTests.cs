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

    }
}

