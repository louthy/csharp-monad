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

            Assert.IsTrue(res().Value == 6);
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

            Assert.IsTrue(res().IsFaulted);
        }

        private Try<T> ErrIO<T>(IO<T> fn)
        {
            return new Try<T>( () => fn() );
        }

        private Try<IO<T>> Trans<T>( IO<T> inner )
        {
            return () => inner;
        }

        private Reader<E, Try<T>> Trans<E, T>(Try<T> inner)
        {
            return (env) => inner;
        }

        public IO<string> Hello()
        {
            return () => "Hello,";
        }

        public IO<string> World()
        {
            return () => " World";
        }

        // Messing
        [Test]
        public void TransTest()
        {
            var errT = Trans<string>( from h in Hello()
                                      from w in World()
                                      select h + w );

            var rdrT = Trans<string,IO<string>>(errT);

            Assert.IsTrue(rdrT("environ")().Value() == "Hello, World");
        }
    }
}

