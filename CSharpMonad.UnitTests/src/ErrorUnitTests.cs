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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monad;
using NUnit.Framework;


namespace Monad.UnitTests
{
    [TestFixture]
    public class ErrorTests
    {

        private Error<int> DoSomething(int value)
        {
            return () => value + 1;
        }

        private Error<int> DoSomethingElse(int value)
        {
            return () => value + 10;
        }

        private Error<int> DoSomethingError(int value)
        {
            return () =>
            {
                throw new Exception("Whoops");
            };
        }

        private int ThrowError(int val)
        {
            throw new Exception("Whoops");
        }

        private Error<int> DoNotEverEnterThisFunction(int value)
        {
            return () => 10000;
        }

        [Test]
        public void TestErrorMonadLaws()
        {
            var value = 1000;

            // Return
            Error<int> errorM = () => value;

            Assert.IsTrue(
                errorM.Return().Value == 1000 && 
                errorM.Return().IsFaulted == false
                );


            errorM = DoSomethingError(0);
            Assert.IsTrue(
                errorM.Return().IsFaulted == true && 
                errorM.Return().Exception != null
                );

            // Bind
            var boundM = (from e in errorM
                          from b in DoSomethingError(0)
                          select b)
                         .Return();

            // Value
            Assert.IsTrue(
                boundM.IsFaulted == true &&
                boundM.Exception != null
                );

        }

        [Test]
        public void TestErrorMonadSuccess()
        {
            var result = (from val1 in DoSomething(10)
                          from val2 in DoSomethingElse(val1)
                          select val2)
                         .Return();

            Assert.IsTrue(result.IsFaulted == false, "Should have succeeded");
            Assert.IsTrue(result.Value == 21, "Value should be 21");
        }

        [Test]
        public void TestErrorMonadFail()
        {
            var result = (from val1 in DoSomething(10)
                          from val2 in DoSomethingError(val1)
                          from val3 in DoNotEverEnterThisFunction(val2)
                          select val3)
                         .Return();

            Assert.IsTrue(result.Value != 10000, "Entered the function: DoNotEverEnterThisFunction()");
            Assert.IsTrue(result.IsFaulted == true, "Should throw an error");

        }

        [Test]
        public void TestErrorMonadSuccessFluent()
        {
            var result = DoSomething(10).Then(val2 => val2 + 10).Return();

            Assert.IsTrue(result.IsFaulted == false, "Should have succeeded");
            Assert.IsTrue(result.Value == 21, "Value should be 21");

        }

        [Test]
        public void TestErrorMonadFailFluent()
        {
            var result = DoSomething(10)
                            .Then(ThrowError)
                            .Then(_ => 10000)
                            .Return();

            Assert.IsTrue(result.Value != 10000, "Entered the function: DoNotEverEnterThisFunction()");
            Assert.IsTrue(result.IsFaulted == true, "Should throw an error");

        }
    }
}
