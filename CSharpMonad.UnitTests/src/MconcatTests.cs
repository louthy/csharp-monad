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

namespace CSharpMonad.UnitTests
{
    [TestFixture]
    public class MconcatTests
    {
        [Test]
        public void TestOptionMconcat()
        {
            Option<int> opt1 = 1;
            Option<int> opt2 = 2;
            Option<int> opt3 = 3;

            var opts = new Option<int>[] { opt1, opt2, opt3 }.AsEnumerable();

            var res = opts.Mconcat();

            Assert.IsTrue(res == 6);

            var zero = Option.Mempty<int>();

            opts = zero.Cons(opts);
            Assert.IsTrue(res == 6);

            var nothing = Option<int>.Nothing;

            res = nothing.Cons(opts).Mconcat();

            res = new Option<int>[] { opt1, opt2, opt3, nothing, zero }.AsEnumerable().Mconcat();
            Assert.IsTrue(res == nothing);

            res = new Option<int>[] { opt1, opt2, nothing, opt3, zero }.AsEnumerable().Mconcat();
            Assert.IsTrue(res == nothing);

            Option<AType> a1 = new AType(1);
            Option<AType> a2 = new AType(2);
            Option<AType> a3 = new AType(3);

            var res2 = new Option<AType>[] { a1, a2, a3 }.AsEnumerable().Mconcat();

            Assert.IsTrue(res2.HasValue && res2.Value.Value == 6);
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
