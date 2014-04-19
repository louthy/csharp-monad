using System;
using Monad;
using NUnit.Framework;

namespace Monad.UnitTests
{
    [TestFixture]
    public class StateTests
    {
        [Test]
        public void StateTest1()
        {
            var state = State.Return<string,int>(0);

            var sm = from w in state
                     from x in DoSomething()
                     from y in DoSomethingElse()
                     select x + y;

            var res = sm("Hello");


            Assert.IsTrue(res.Item1 == "Hello, World");
            Assert.IsTrue(res.Item2 == 3);
        }

        State<string,int> DoSomethingElse()
        {
            return state => Tuple.Create(state + "rld",1);
        }

        State<string,int> DoSomething()
        {
            return state => Tuple.Create(state + ", Wo",2);
        }
    }
}

