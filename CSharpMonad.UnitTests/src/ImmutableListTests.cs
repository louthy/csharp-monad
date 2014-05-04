using Monad.Parsec;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monad.Utility;


namespace Monad.UnitTests
{
    [TestFixture]
    public class ImmutableListTests
    {
        [Test]
        public void EnumeratorTest1()
        {
            var list1 = new ImmutableList<int>(new int[] { 0, 1, 2 });

            int index = 0;
            foreach (var item in list1)
            {
                Assert.IsTrue(item == index);
                index++;
            }
            Assert.IsTrue(index == 3);
        }

        [Test]
        public void EnumeratorTest2()
        {
            var list1 = new ImmutableList<int>(new int[] { 0, 1, 2 });
            var list2 = new ImmutableList<int>(new int[] { 3, 4, 5 });

            var list = list1.Concat(list2);

            int index = 0;
            foreach (var item in list)
            {
                Assert.IsTrue(item == index);
                index++;
            }

            Assert.IsTrue(index == 6);
        }

        [Test]
        public void EnumeratorTest3()
        {
            var list1 = new ImmutableList<int>(new int[] { 1, 2, 3 });
            var list2 = new ImmutableList<int>(new int[] { 4, 5, 6 });

            var list = 0.Cons( list1.Concat(list2) );

            int index = 0;
            foreach (var item in list)
            {
                Assert.IsTrue(item == index);
                index++;
            }

            Assert.IsTrue(index == 7);
        }

        [Test]
        public void EnumeratorLengthTest1()
        {
            var list1 = new ImmutableList<int>(new int[] { 0, 1, 2 });
            var list2 = new ImmutableList<int>(new int[] { 3, 4, 5 });

            var list = list1.Concat(list2);

            Assert.IsTrue(list.Length == 6);

            list = list.Tail();
            Assert.IsTrue(list.Length == 5);

            list = list.Tail();
            list = list.Tail();
            list = list.Tail();
            list = list.Tail();

            Assert.IsTrue(list.Length == 1);
            Assert.IsTrue(list.IsAlmostEmpty);

            list = list.Tail();
            Assert.IsTrue(list.IsEmpty);

            try
            {
                list = list.Tail();
                Assert.Fail("Should have exceptioned");
            }
            catch
            {
            }
        }
    }
}
