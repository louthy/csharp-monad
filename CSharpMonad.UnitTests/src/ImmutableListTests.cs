using Monad.Parsec;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monad.Utility;


namespace Monad.UnitTests
{
    public class ImmutableListTests
    {
        [Fact]
        public void EnumeratorTest1()
        {
            var list1 = new ImmutableList<int>(new int[] { 0, 1, 2 });

            int index = 0;
            foreach (var item in list1)
            {
                Assert.True(item == index);
                index++;
            }
            Assert.True(index == 3);
        }

        [Fact]
        public void EnumeratorTest2()
        {
            var list1 = new ImmutableList<int>(new int[] { 0, 1, 2 });
            var list2 = new ImmutableList<int>(new int[] { 3, 4, 5 });

            var list = list1.Concat(list2);

            int index = 0;
            foreach (var item in list)
            {
                Assert.True(item == index);
                index++;
            }

            Assert.True(index == 6);
        }

        [Fact]
        public void EnumeratorTest3()
        {
            var list1 = new ImmutableList<int>(new int[] { 1, 2, 3 });
            var list2 = new ImmutableList<int>(new int[] { 4, 5, 6 });

            var list = 0.Cons( list1.Concat(list2) );

            int index = 0;
            foreach (var item in list)
            {
                Assert.True(item == index);
                index++;
            }

            Assert.True(index == 7);
        }

        [Fact]
        public void EnumeratorLengthTest1()
        {
            var list1 = new ImmutableList<int>(new int[] { 0, 1, 2 });
            var list2 = new ImmutableList<int>(new int[] { 3, 4, 5 });

            var list = list1.Concat(list2);

            Assert.True(list.Length == 6);

            list = list.Tail();
            Assert.True(list.Length == 5);

            list = list.Tail();
            list = list.Tail();
            list = list.Tail();
            list = list.Tail();

            Assert.True(list.Length == 1);
            Assert.True(list.IsAlmostEmpty);

            list = list.Tail();
            Assert.True(list.IsEmpty);

			Assert.Throws<ParserException>(() => list.Tail());
        }
    }
}
