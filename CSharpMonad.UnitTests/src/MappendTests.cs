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

            Option<AType> a1 = new AType(1);
            Option<AType> a2 = new AType(2);
            Option<AType> a3 = new AType(3);

            var res2 = a1 + a2 + a3;

            Assert.IsTrue(res2.HasValue && res2.Value.Value == 6);
        }

        [Test]
        public void TestEitherMappend()
        {
            Either<int,string> opt1 = 1;
            Either<int,string> opt2 = 2;
            Either<int,string> opt3 = 3;
            Either<int,string> fail = "fail!";

            var res = opt1 + opt2 + opt3;

            Assert.IsTrue(res == 6);
            Assert.IsFalse(res == null);

            var zero = Either<int, string>.Mempty();

            res = opt1 + opt2 + opt3 + zero;
            Assert.IsTrue(res == 6);

            res = opt1 + opt2 + opt3 + zero + fail;
            Assert.IsTrue(res == "fail!");

            res = fail + opt1 + opt2 + opt3 + zero;
            Assert.IsTrue(res == "fail!");

            res = opt1 + opt2 + fail + opt3 + zero;
            Assert.IsTrue(res == "fail!");

            Either<AType, string> a1 = new AType(1);
            Either<AType, string> a2 = new AType(2);
            Either<AType, string> a3 = new AType(3);

            var res2 = a1 + a2 + a3;

            Assert.IsTrue(res2.IsRight && res2.Right.Value == 6);
        }

        class AType : IAppendable<AType>
        {
            public readonly int Value;

            public AType(int value)
            {
                Value = value;
            }

            public AType Append(AType rhs)
            {
                return new AType(Value + rhs.Value);
            }
        }
    }
}
