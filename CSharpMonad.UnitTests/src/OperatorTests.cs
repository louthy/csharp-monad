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

using Monad;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.UnitTests
{
    [TestFixture]
    public class OperatorTests
    {
        [Test]
        public void TestEitherAnd()
        {
            Either<int, string> one = 1;
            Either<int, string> two = 2;
            Either<int, string> thr = 3;
            Either<int, string> fail = "fail";

            var res = one & two & thr;

            Assert.IsTrue(res == 3);

            res = one & two & thr & fail;

            Assert.IsTrue(res.IsLeft);

            res = fail & one & two & thr;

            Assert.IsTrue(res.IsLeft);

            res = one & fail & two & thr;

            Assert.IsTrue(res.IsLeft);
        }

        [Test]
        public void TestEitherOr()
        {
            Either<int, string> one = 1;
            Either<int, string> two = 2;
            Either<int, string> thr = 3;
            Either<int, string> fail = "fail";

            var res = one | two | thr;

            Assert.IsTrue(res == 1);

            res = one | two | thr | fail;

            Assert.IsTrue(res == 1);

            res = fail | one | two | thr;

            Assert.IsTrue(res == 1);

            res = one | fail | two | thr;

            Assert.IsTrue(res == 1);

            res = fail | fail | fail | fail;

            Assert.IsTrue(res.IsLeft);

            res = fail | fail | fail | one;

            Assert.IsTrue(res == 1);
        }

    }
}
