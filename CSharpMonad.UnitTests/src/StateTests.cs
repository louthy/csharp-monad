using System;
using Monad;
using NUnit.Framework;
using Monad.Utility;

namespace Monad.UnitTests
{
    [TestFixture]
    public class StateTests
    {
        [Test]
        public void StateTest1()
        {
            var state = State.Return<string,int>();

            var sm = from w in state
                     from x in DoSomething()
                     from y in DoSomethingElse()
                     select x + y;

            var res = sm("Hello");


            Assert.IsTrue(res.Item1 == "Hello, World");
            Assert.IsTrue(res.Item2 == 3);
        }

        [Test]
        public void StateTest2()
        {
            var sm = from x in State.Get<string>()
                     from y in State.Put("Hello"+x)
                     select y;

            var res = sm(", World");

            Assert.IsTrue(res.Item1 == "Hello, World");
        }

        [Test]
        public void StateTest3()
        {
            var initial = State.Return<string,int>(10);

            var sm = from x in initial
                     from t in State.Get<string>()
                     from y in State.Put("Hello " + (x * 10) + t)
                     select y;

            var res = sm(", World");

            Assert.IsTrue(res.Item1 == "Hello 100, World");
        }

        [Test]
        public void StateTest4()
        {
            var first = State.Return<string,int>(10);
            var second = State.Return<string,int>(3);

            var sm = from x in first
                     from t in State.Get<string>()
                     from y in second
                     from s in State.Put("Hello " + (x * y) + t)
                     select s;

            var res = sm(", World");

            Assert.IsTrue(res.Item1 == "Hello 30, World");
        }

        [Test]
        public void StateTest5()
        {
            var first = State.Return<string,int>(10);
            var second = State.Return<string,int>(3);
            var third = State.Return<string,int>(5);
            var fourth = State.Return<string,int>(100);

            var sm = from x in first
                     from t in State.Get<string>()
                     from y in second
                     from s in State.Put("Hello " + (x * y) + t)
                     from z in third
                     from w in fourth
                     from s1 in State.Get<string>()
                     from s2 in State.Put( s1 + " " + (z * w) )
                     select x * y * z * w;

            var res = sm(", World");

            Assert.IsTrue(res.Item1 == "Hello 30, World 500");
            Assert.IsTrue(res.Item2 == 15000);
        }

        [Test]
        public void StateTest6()
        {            
            var first = State.Return<string,int>(10);
            var second = State.Return<string,int>(3);
            var third = State.Return<string,int>(5);
            var fourth = State.Return<string,int>(100);

            var sm = from x in first
                     from t in State.Get<string>( s => s + "yyy" )
                     from y in second
                     from s in State.Put("Hello " + (x * y) + t)
                     from z in third
                     from w in fourth
                     from s1 in State.Get<string>()
                     from s2 in State.Put( s1 + " " + (z * w) )
                     select x * y * z * w;

            var res = sm(", World");

            Assert.IsTrue(res.Item1 == "Hello 30, Worldyyy 500");
            Assert.IsTrue(res.Item2 == 15000);
        }

        static State<Unit, S> Put<S>( S state )
        {
            return _ => Tuple.Create<Unit,S>( Unit.Return(), state );
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

