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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Monad;

namespace Monad.UnitTests
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

            var zero = Option.Mempty<int>();

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

            var zero = Either.Mempty<int, string>();

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

        [Test]
        public void TestErrorMappend()
        {
            Error<int> opt1 = () => 1;
            Error<int> opt2 = () => 2;
            Error<int> opt3 = () => 3;
            Error<int> fail = () => { throw new Exception("Error"); };

            var res = opt1 + opt2 + opt3;

            var v = res.Return().Value;

            Assert.IsTrue(v == 6);
            Assert.IsFalse(res == null);

            var zero = Error.Mempty<int>();

            res = opt1 + opt2 + opt3 + zero;
            Assert.IsTrue(res.Return().Value == 6);

            res = opt1 + opt2 + opt3 + zero + fail;
            Assert.IsTrue(res.Return().IsFaulted);

            res = fail + opt1 + opt2 + opt3 + zero;
            Assert.IsTrue(res.Return().IsFaulted);

            res = opt1 + opt2 + fail + opt3 + zero;
            Assert.IsTrue(res.Return().IsFaulted);

            Error<AType> a1 = () => new AType(1);
            Error<AType> a2 = () => new AType(2);
            Error<AType> a3 = () => new AType(3);

            var res2 = a1 + a2 + a3;

            Assert.IsTrue(res2.Return().Value.Value == 6);
        }

        [Test]
        public void TestIOMappend()
        {
            IO<int> opt1 = () => 1;
            IO<int> opt2 = () => 2;
            IO<int> opt3 = () => 3;

            var res = opt1 + opt2 + opt3;

            var v = res.Return();

            Assert.IsTrue(v == 6);
            Assert.IsFalse(res == null);

            var zero = IO.Mempty<int>();

            res = opt1 + opt2 + opt3 + zero;
            Assert.IsTrue(res.Return() == 6);

            IO<AType> a1 = () => new AType(1);
            IO<AType> a2 = () => new AType(2);
            IO<AType> a3 = () => new AType(3);

            var res2 = a1 + a2 + a3;

            Assert.IsTrue(res2.Return().Value == 6);
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
