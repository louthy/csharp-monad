using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monad.Parsec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpMonad.UnitTests
{
    [TestClass]
    public class TestExpr
    {
        [TestMethod]
        public void ExpressionTests()
        {
            var ten = Eval("2*3+4");

            Assert.IsTrue(ten == 10);

            var fourteen = Eval("2*(3+4)");

            Assert.IsTrue(fourteen == 14);
        }


        public int Eval(string expr)
        {
            var r = NewT.Expr().Parse(expr);
            if (r.Count() == 0)
            {
                return 999;
            }
            else
            {
                return r.First().Item1;
            }
        }
    }


    public class NewT
    {
        public static Expr Expr()
        {
            return new Expr();
        }
        public static Term Term()
        {
            return new Term();
        }
        public static Factor Factor()
        {
            return new Factor();
        }
    }

    public class Expr : Parser<int>
    {
        public Expr()
            :
            base(
                inp => (from t in NewT.Term()
                        from e in
                            New.Choice<int>(
                                from plus in New.Character('+')
                                from expr in NewT.Expr()
                                select expr,
                                New.Return<int>(0)
                                )
                        select t + e)
                       .Parse(inp)
            )
        { }
    }

    public class Term : Parser<int>
    {
        public Term()
            :
            base(
                inp => (from f in NewT.Factor()
                        from t in
                            New.Choice<int>(
                                from mult in New.Character('*')
                                from term in NewT.Term()
                                select term,
                                New.Return<int>(1)
                                )
                        select f * t)
                       .Parse(inp)
            )
        { }
    }

    public class Factor : Parser<int>
    {
        public Factor()
            :
            base(
                inp => (from choice in
                            New.Choice<int>(
                                from d in New.Digit()
                                select Int32.Parse(d.ToString()),
                                from open in New.Character('(')
                                from expr in NewT.Expr()
                                from close in New.Character(')')
                                select expr
                                )
                        select choice)
                        .Parse(inp)

            )
        { }

    }
}
