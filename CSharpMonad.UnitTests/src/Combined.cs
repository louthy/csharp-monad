using System;

using Monad;
using NUnit.Framework;

namespace Monad.UnitTests
{
    [TestFixture]
    public class Combined
    {
        [Test]
        public void Combined1()
        {
            var t1 = ErrIO(() => 1);
            var t2 = ErrIO(() => 2);
            var t3 = ErrIO(() => 3);

            var res = from one in t1
                      from two in t2
                      from thr in t3
                      select one + two + thr;

            Assert.IsTrue(res.Return().Value == 6);
        }

        [Test]
        public void Combined2()
        {

            var t1 = ErrIO(() => 1);
            var t2 = ErrIO(() => 2);
            var t3 = ErrIO(() => 3);
            var fail = ErrIO<int>(() =>
                {
                    throw new Exception("Error");
                });

            var res = from one in t1
                      from two in t2
                      from thr in t3
                      from err in fail
                      select one + two + thr + err;

            Assert.IsTrue(res.Return().IsFaulted);
        }

        private Error<T> ErrIO<T>(IO<T> fn)
        {
            return new Error<T>( () => fn() );
        }
    }
}

