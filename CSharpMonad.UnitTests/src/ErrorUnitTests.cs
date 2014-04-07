using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monad;


namespace Monad.UnitTests
{
	[TestClass]
	public class ErrorTests
	{
		[TestMethod]
		public void TestMonadLaws()
		{
			var value = 1000;

			var errorM = Error.Return(() => value);
			Assert.IsTrue(errorM.Value == 1000 && errorM.IsFaulted == false);

			var boundM = from e in errorM
						 from b in Error.Return(() => e + 1000)
						 select b;

			Assert.IsTrue(boundM.Value == 2000 && boundM.IsFaulted == false);


			errorM = DoSomethingError(0);
			Assert.IsTrue(errorM.IsFaulted == true && errorM.Exception != null);

			boundM = from e in errorM
					 from b in DoSomethingError(0)
					 select b;

			Assert.IsTrue(errorM.IsFaulted == true && errorM.Exception != null);

		}

		[TestMethod]
		public void TestErrorMonadSuccess()
		{
			var result = from val1 in DoSomething(10)
						 from val2 in DoSomethingElse(val1)
						 select val2;

			Assert.IsTrue(result.IsFaulted == false, "Should have succeeded");
			Assert.IsTrue(result.Value == 21, "Value should be 21");
		}

		[TestMethod]
		public void TestErrorMonadFail()
		{
			var result = from val1 in DoSomething(10)
						 from val2 in DoSomethingError(val1)
						 from val3 in DoNotEverEnterThisFunction(val2)
						 select val3;

			Assert.IsTrue(result.Value != 10000, "Entered the function: DoNotEverEnterThisFunction()");
			Assert.IsTrue(result.IsFaulted == true, "Should throw an error");

		}

		[TestMethod]
		public void TestErrorMonadSuccessFluent()
		{
			var result = DoSomething(10).Then(val2 => val2 + 10);

			Assert.IsTrue(result.IsFaulted == false, "Should have succeeded");
			Assert.IsTrue(result.Value == 21, "Value should be 21");

		}

		[TestMethod]
		public void TestErrorMonadFailFluent()
		{
			var result = DoSomething(10)
							.Then(ThrowError)
							.Then(_ => 10000);

			Assert.IsTrue(result.Value != 10000, "Entered the function: DoNotEverEnterThisFunction()");
			Assert.IsTrue(result.IsFaulted == true, "Should throw an error");

		}

		private Error<int> DoSomething(int value)
		{
			return Error.Return(() =>
				value + 1
			);
		}

		private Error<int> DoSomethingElse(int value)
		{
			return Error.Return(() =>
				value + 10
			);
		}

		private Error<int> DoSomethingError(int value)
		{
			return Error.Return<int>(() =>
			{
				throw new Exception("Whoops");
			});
		}

		private int ThrowError(int val)
		{
			throw new Exception("Whoops");
		}

		private Error<int> DoNotEverEnterThisFunction(int value)
		{
			return Error.Return<int>(() =>
			{
				return 10000;
			});
		}
	}
}
