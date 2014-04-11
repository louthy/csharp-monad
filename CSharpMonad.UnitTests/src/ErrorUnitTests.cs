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
