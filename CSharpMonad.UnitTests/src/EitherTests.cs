using System;
using NUnit.Framework;
using Monad;

namespace Monad.UnitTests
{
    [TestFixture]
    public class EitherTests
    {
        [Test]
        public void TestBinding1()
        {
            var r = 
                from lhs in Either.Right<string, int>(2)
                from  rhs in Either.Right<string, int>(2)
                select lhs+rhs;

            Assert.IsTrue(r.IsRight && r.GetRight() == 4);
        }

        [Test]
        public void TestBinding2()
        {
            var r = 
                from lhs in Either.Right<string, int>(2)
                from rhs in Either.Left<string, int>("Error!!")
                select lhs+rhs;

            Assert.IsTrue(r.IsLeft && r.GetLeft() == "Error!!");
        }


        [Test]
        public void TestBinding3()
        {
            var r = 
                from lhs in Either.Right<string, int>(2)
                select lhs;

            Assert.IsTrue(r.IsRight && r.GetRight() == 2);
        }    

        [Test]
        public void TestBinding4()
        {
            var r = 
                from lhs in Either.Left<string, int>("Error!!")
                select lhs;

            Assert.IsTrue(r.IsLeft && r.GetLeft() == "Error!!");
        }    
    }
}

