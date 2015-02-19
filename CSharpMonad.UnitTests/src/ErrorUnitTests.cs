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
using Xunit;


namespace Monad.UnitTests
{
    public class ErrorTests
    {

        private Try<int> DoSomething(int value)
        {
            return () => value + 1;
        }

        private Try<int> DoSomethingElse(int value)
        {
            return () => value + 10;
        }

        private Try<int> DoSomethingError(int value)
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

        private Try<int> DoNotEverEnterThisFunction(int value)
        {
            return () => 10000;
        }

        [Fact]
        public void TestWithoutRunTry()
        {
            var t = from v in DoSomethingError(10)
                    select v;

            var e = t();

            t = from x in DoSomething(10)
                from u in DoSomethingElse(10)
                from v in DoSomethingElse(10)
                from w in DoSomethingError(10)
                select w;

            e = t();
        }


        [Fact]
        public void TestErrorMonadLaws()
        {
            var value = 1000;

            // Return
            Try<int> errorM = () => value;

            Assert.True(
                errorM.Try().Value == 1000 &&
                errorM.Try().IsFaulted == false
                );


            errorM = DoSomethingError(0);
            Assert.True(
                errorM.Try().IsFaulted == true &&
                errorM.Try().Exception != null
                );

            // Bind
            var boundM = (from e in errorM
                          from b in DoSomethingError(0)
                          select b)
                         .Try();

            // Value
            Assert.True(
                boundM.IsFaulted == true &&
                boundM.Exception != null
                );

        }

        [Fact]
        public void TestErrorMonadSuccess()
        {
            var result = (from val1 in DoSomething(10)
                          from val2 in DoSomethingElse(val1)
                          select val2)
                         .Try();

            Assert.True(result.IsFaulted == false, "Should have succeeded");
            Assert.True(result.Value == 21, "Value should be 21");
        }

        [Fact]
        public void TestErrorMonadFail()
        {
            var result = (from val1 in DoSomething(10)
                          from val2 in DoSomethingError(val1)
                          from val3 in DoNotEverEnterThisFunction(val2)
                          select val3)
                         .Try();

            Assert.True(result.Value != 10000, "Entered the function: DoNotEverEnterThisFunction()");
            Assert.True(result.IsFaulted == true, "Should throw an error");

        }

        [Fact]
        public void TestErrorMonadSuccessFluent()
        {
            var result = DoSomething(10).Then(val2 => val2 + 10).Try();

            Assert.True(result.IsFaulted == false, "Should have succeeded");
            Assert.True(result.Value == 21, "Value should be 21");

        }

        [Fact]
        public void TestErrorMonadFailFluent()
        {
            var result = DoSomething(10)
                            .Then(ThrowError)
                            .Then(_ => 10000)
                            .Try();

            Assert.True(result.Value != 10000, "Entered the function: DoNotEverEnterThisFunction()");
            Assert.True(result.IsFaulted == true, "Should throw an error");

        }

        public Try<int> One()
        {
            return () => 1;
        }

        public Try<int> Two()
        {
            return () => 2;
        }

        public Try<int> Error()
        {
            return () => { throw new Exception("Error!!"); };
        }

        [Fact]
        public void TestErrorMatch1()
        {
           (from one in One()
            from err in Error()
            from two in Two()
            select one + two + err)
           .Match(
               Success: v => Assert.True(false),
               Fail:    e => Assert.True(e.Message == "Error!!")
            );
        }

        [Fact]
        public void TestErrorMatch2()
        {
            var unit =
                (from one in One()
                 from err in Error()
                 from two in Two()
                 select one + two + err)
                .Match(
                    val => Assert.False(false),
                    err => Assert.True(err.Message == "Error!!")
                );

            Console.WriteLine(unit.ToString());
        }

        [Fact]
        public void TestErrorMatch3()
        {
            (from one in One()
             from two in Two()
             select one + two)
            .Match(
                Success: v => Assert.True(v == 3),
                Fail: e => Assert.True(false)
            );
        }

        [Fact]
        public void TestErrorMatch4()
        {
            var unit =
                (from one in One()
                 from two in Two()
                 select one + two)
                .Match(
                    val => Assert.True(val == 3),
                    err => Assert.True(false)
                );

            Console.WriteLine(unit.ToString());
        }
    }
}
