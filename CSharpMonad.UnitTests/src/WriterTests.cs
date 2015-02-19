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
using System.Linq;
using NUnit.Framework;
using Monad.Utility;
using Monad;

namespace Monad.UnitTests
{
    [TestFixture]
    public class WriterTests
    {
        [Test]
        public void Binding1()
        {
            var res = from a in LogNumber(3)
                      from b in LogNumber(5)
                      select a * b;

            var memo = res.Memo();

            Assert.IsTrue(memo().Value == 15 && memo().Output.Count() == 2);
            Assert.IsTrue(memo().Output.First() == "Got number: 3");
            Assert.IsTrue(memo().Output.Skip(1).First() == "Got number: 5");
        }

        [Test]
        public void Binding2()
        {
            var res = from a in LogNumber(3)
                      from b in LogNumber(5)
                      from _ in Writer.Tell("Gonna multiply these two")
                      select a * b;

            var memo = res.Memo();

            Assert.IsTrue(memo().Value == 15 && memo().Output.Count() == 3);
            Assert.IsTrue(memo().Output.First() == "Got number: 3");
            Assert.IsTrue(memo().Output.Skip(1).First() == "Got number: 5");
            Assert.IsTrue(memo().Output.Skip(2).First() == "Gonna multiply these two");
        }

        private static Writer<string, int> LogNumber(int num)
        {
            return () => Writer.Tell(num, "Got number: " + num);
        }
    }
}
