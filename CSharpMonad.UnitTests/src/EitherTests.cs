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
using Monad;

namespace Monad.UnitTests
{
    [TestFixture]
    public class EitherTests
    {
        [Test]
        public void TestEitherBinding1()
        {
            var r = from lhs in Two()
                    from rhs in Two()
                    select lhs+rhs;

            Assert.IsTrue(r.IsRight() && r.Right() == 4);
        }

        [Test]
        public void TestEitherBinding2()
        {
            var r = (from lhs in Two()
                     from rhs in Error()
                     select lhs + rhs)
                    .Memo();

            Assert.IsTrue(r().IsLeft && r().Left == "Error!!");
        }


        [Test]
        public void TestEitherBinding3()
        {
            var r = 
                from lhs in Two()
                select lhs;

            Assert.IsTrue(r.IsRight() && r.Right() == 2);
        }    

        [Test]
        public void TestEitherBinding4()
        {
            var r = 
                from lhs in Error()
                select lhs;

            Assert.IsTrue(r.IsLeft() && r.Left() == "Error!!");
        }

        [Test]
        public void TestEitherBinding5()
        {
            var r =
                from one in Two()
                from two in Error()
                from thr in Two()
                select one + two + thr;

            Assert.IsTrue(r.IsLeft() && r.Left() == "Error!!");
        }

        [Test]
        public void TestEitherMatch1()
        {
            var unit =
                (from one in Two()
                 from two in Error()
                 from thr in Two()
                 select one + two + thr)
                .Match(
                    Right: r => Assert.IsTrue(false),
                    Left: l => Assert.IsTrue(l == "Error!!")
                );

            Console.WriteLine(unit.ToString());
        }

        [Test]
        public void TestEitherMatch2()
        {
            var unit =
                (from one in Two()
                 from two in Error()
                 from thr in Two()
                 select one + two + thr)
                .Match(
                    right => Assert.IsFalse(true),
                    left => Assert.IsTrue(left == "Error!!")
                );
            Console.WriteLine(unit.ToString());
        }

        [Test]
        public void TestEitherMatch3()
        {
            var unit =
                (from one in Two()
                 from two in Two()
                 select one + two)
                .Match(
                    Right: r => Assert.IsTrue(r == 4),
                    Left: l => Assert.IsFalse(true)
                );
            Console.WriteLine(unit.ToString());
        }

        [Test]
        public void TestEitherMatch4()
        {
            var unit =
                (from one in Two()
                 from two in Two()
                 select one + two)
                .Match(
                    right => Assert.IsTrue(right == 4),
                    left => Assert.IsFalse(true)
                );
            Console.WriteLine(unit.ToString());
        }

        [Test]
        public void TestEitherMatch5()
        {
            var result =
                (from one in Two()
                 from two in Two()
                 select one + two)
                .Match(
                    Right: r => r * 2,
                    Left: l => 0
                );

            Assert.IsTrue(result() == 8);
        }

        [Test]
        public void TestEitherMatch6()
        {
            var result =
                (from one in Two()
                 from err in Error()
                 from two in Two()
                 select one + two)
                .Match(
                    Right: r => r * 2,
                    Left: l => 0
                );

            Assert.IsTrue(result() == 0);
        }

        public Either<int, string> Two()
        {
            return () => 2;
        }

        public Either<int, string> Error()
        {
            return () => "Error!!";
        }
    }
}

