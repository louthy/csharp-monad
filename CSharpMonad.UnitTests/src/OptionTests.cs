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

using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monad;
using Monad.Utility;

namespace Monad.UnitTests
{
    public class OptionTests
    {
        [Fact]
        public void TestBinding()
        {
            Option<int> option = () => 1000.ToOption();
            Option<int> option2 = () => 2000.ToOption();

            var result = from o in option
                         select o;

            Assert.True(result.HasValue() && result.Value() == 1000);
            Assert.True(result.Match(Just: () => true, Nothing: () => false)());
            Assert.True(result.Match(Just: () => true, Nothing: false)());

            result = from o in option
                     from o2 in option2
                     select o2;

            Assert.True(result.HasValue() && result.Value() == 2000);
            Assert.True(result.Match(Just: () => true, Nothing: () => false)());
            Assert.True(result.Match(Just: () => true, Nothing: false)());

            result = from o in option
                     from o2 in Nothing()
                     select o2;

            Assert.True(result.HasValue() == false);
        }

        [Fact]
        public void TestEquals()
        {
            OptionStrict<int> option = 1000.ToOptionStrict();
            OptionStrict<int> option2 = 1000.ToOptionStrict();

            Assert.True(option == option2);
        }

        public Option<int> Nothing()
        {
            return Option.Nothing<int>();
        }
    }
}