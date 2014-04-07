using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monad;

namespace CSharpMonad.UnitTests
{
    [TestClass]
    public class OptionTests
    {
        [TestMethod]
        public void TestBinding()
        {
            var option = 1000.ToOption();
            var option2 = 2000.ToOption();

            var result = from o in option
                         select o;

            Assert.IsTrue(result.HasValue && result.Value == 1000);
            Assert.IsTrue(result.Match(Just: () => true, Nothing: () => false));
            Assert.IsTrue(result.Match(Just: () => true, Nothing: false));

            result = from o in option
                     from o2 in option2
                     select o2;

            Assert.IsTrue(result.HasValue && result.Value == 2000);
            Assert.IsTrue(result.Match(Just: () => true, Nothing: () => false));
            Assert.IsTrue(result.Match(Just: () => true, Nothing: false));

            bool passed = false;
            try
            {
                result = from o in option
                         from o2 in Option<int>.Nothing
                         select o2;
            }
            catch (InvalidOperationException)
            {
                passed = true;
            }
            finally
            {
                Assert.IsTrue(passed);
            }
        }
    }
}