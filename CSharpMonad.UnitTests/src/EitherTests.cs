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
            var r =
                from lhs in Two()
                from rhs in Two()
                select lhs+rhs;

            Assert.IsTrue(r.IsRight && r.Right == 4);
        }

        [Test]
        public void TestEitherBinding2()
        {
            var r =
                from lhs in Two()
                from rhs in Error()
                select lhs+rhs;

            Assert.IsTrue(r.IsLeft && r.Left == "Error!!");
        }


        [Test]
        public void TestEitherBinding3()
        {
            var r = 
                from lhs in Two()
                select lhs;

            Assert.IsTrue(r.IsRight && r.Right == 2);
        }    

        [Test]
        public void TestEitherBinding4()
        {
            var r = 
                from lhs in Error()
                select lhs;

            Assert.IsTrue(r.IsLeft && r.Left == "Error!!");
        }

        [Test]
        public void TestEitherBinding5()
        {
            var r =
                from one in Two()
                from two in Error()
                from thr in Two()
                select one + two + thr;

            Assert.IsTrue(r.IsLeft && r.Left == "Error!!");
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

            Assert.IsTrue(result == 8);
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

            Assert.IsTrue(result == 0);
        }

        public Either<string, int> Two()
        {
            return 2;
        }

        public Either<string, int> Error()
        {
            return "Error!!";
        }
    }
}

