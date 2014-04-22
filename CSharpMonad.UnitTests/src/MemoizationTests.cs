using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.UnitTests
{
    [TestFixture]
    public class MemoizationTests
    {
        int breakIt = 0;

        [Test]
        public void MemoizationTest1()
        {
            var mon = (from one in One()
                       from two in Two()
                       from thr in Three()
                       select one + two + thr)
                      .Memo();

            var res = mon();
            Assert.IsTrue(res == 6);
            breakIt++;

            res = mon();
            Assert.IsTrue(res == 6);
        }

        public Monad<int> One()
        {
            return () => 1 + breakIt;
        }

        public Monad<int> Two()
        {
            return () => 2 + breakIt;
        }

        public Monad<int> Three()
        {
            return () => 3 + breakIt;
        }
    }

    public delegate T Monad<T>();

    public static class TestMonadExt
    {
        public static Monad<U> Select<T, U>(this Monad<T> self, Func<T, U> map)
        {
            return () =>
            {
                var resT = self();

                var resU = map(resT);
                
                return resU;
            };
        }

        public static Monad<V> SelectMany<T,U,V>(
            this Monad<T> self, 
            Func<T,Monad<U>> select,
            Func<T,U,V> project
            )
        {
            return () =>
            {
                var resT = self();
                var resU = select(resT)();
                var resV = project(resT, resU);

                return resV;
            };
        }

        /// <summary>
        /// Memoize the result 
        /// </summary>
        public static Func<T> Memo<T>(this Monad<T> self)
        {
            var res = self();
            return () => res;
        }
    }
}
