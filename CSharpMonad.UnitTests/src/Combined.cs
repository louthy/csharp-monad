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

using System;
using System.Linq;

using Monad;
using NUnit.Framework;
using System.Reflection;

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
            return new Try<T>(() => fn());
        }

        private Try<IO<T>> Trans<T>(IO<T> inner)
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
            var errT = Trans<string>(from h in Hello()
                                     from w in World()
                                     select h + w);

            var rdrT = Trans<string, IO<string>>(errT);

            Assert.IsTrue(rdrT("environ")().Value() == "Hello, World");
        }

        public Try<Option<IO<string>>> OpenFile()
        {
            return () => Option.Return(() => I.O(() => "Data"));
        }

        [Test]
        public void TransTest2()
        {
            Func<string, string> id = a => a;

            var mon = from ed1 in OpenFile()
                      from ed2 in OpenFile()
                      select ed1.LiftM2(id) + ed2.LiftIO(id);

            var res = mon();

            Assert.IsTrue(res.Value == "DataData");
        }
    }
}

