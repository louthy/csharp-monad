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
using Monad.Parsec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.UnitTests
{
    [TestFixture]
    public class TestExpr
    {
        [Test]
        public void ExpressionTests()
        {
            var ten = Eval("2*3+4");

            Assert.IsTrue(ten == 10);

            var fourteen = Eval("2*(3+4)");

            Assert.IsTrue(fourteen == 14);
        }


        public int Eval(string expr)
        {
            var r = New.Expr().Parse(expr);
            if (r.Value.Count() == 0)
            {
                return 999;
            }
            else
            {
                return r.Value.First().Item1;
            }
        }
    }


    public class New
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
                inp => (from t in New.Term()
                        from e in
                            (from plus in Prim.Character('+')
                             from expr in New.Expr()
                             select expr)
                             | Prim.Return<int>(0)
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
                inp => (from f in New.Factor()
                        from t in
                            (from mult in Prim.Character('*')
                             from term in New.Term()
                             select term)
                             | Prim.Return<int>(1)
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
                            (from d in Prim.Digit()
                             select Int32.Parse(d.Value.ToString()))
                             | (from open in Prim.Character('(')
                                from expr in New.Expr()
                                from close in Prim.Character(')')
                                select expr)
                         select choice)
                        .Parse(inp)

            )
        { }

    }
}
