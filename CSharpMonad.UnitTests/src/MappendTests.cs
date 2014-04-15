using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Monad;

namespace CSharpMonad.UnitTests.src
{
    [TestFixture]
    public class MappendTests
    {
        [Test]
        public void TestOptionMappend()
        {
            Option<int> opt1 = 1;
            Option<int> opt2 = 2;
            Option<int> opt3 = 3;

            var res = opt1 + opt2 + opt3;

            Assert.IsTrue(res == 6);
            Assert.IsFalse(res == null);

            var zero = Option<int>.Mempty();

            res = opt1 + opt2 + opt3 + zero;
            Assert.IsTrue(res == 6);

            var nothing = Option<int>.Nothing;

            res = opt1 + opt2 + opt3 + zero + nothing;
            Assert.IsTrue(res == nothing);

            res = nothing + opt1 + opt2 + opt3 + zero;
            Assert.IsTrue(res == nothing);

            res = opt1 + opt2 + nothing + opt3 + zero;
            Assert.IsTrue(res == nothing);
        }

        [Test]
        public void TestEitherMappend()
        {
            Either<string, int> opt1 = 1;
            Either<string, int> opt2 = 2;
            Either<string, int> opt3 = 3;
            Either<string, int> fail = "fail!";

            var res = opt1 + opt2 + opt3;

            Assert.IsTrue(res == 6);
            Assert.IsFalse(res == null);

            var zero = Either<string,int>.Mempty();

            res = opt1 + opt2 + opt3 + zero;
            Assert.IsTrue(res == 6);

            res = opt1 + opt2 + opt3 + zero + fail;
            Assert.IsTrue(res == "fail!");

            res = fail + opt1 + opt2 + opt3 + zero;
            Assert.IsTrue(res == "fail!");

            res = opt1 + opt2 + fail + opt3 + zero;
            Assert.IsTrue(res == "fail!");
        }

    }
}
