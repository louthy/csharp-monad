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

namespace Monad.UnitTests.src
{
    [TestFixture]
    public class LexerTests
    {
        [Test]
        public void Haskell98LexerTest()
        {
            var lexer = new TokenParser(new Haskell98Def());

            var result = lexer.Parse(
@"call2 :: Expr -> Parser Expr
call2 lhs = do
    rhs <- expr
    rhsr <- call2 rhs
    return $ Call lhs rhsr"
);

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
    }
}
