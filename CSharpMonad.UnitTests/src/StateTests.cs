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
using Monad;
using Xunit;
using Monad.Utility;

namespace Monad.UnitTests
{
    public class StateTests
    {
        [Fact]
        public void StateTest1()
        {
            var state = State.Return<string,int>();

            var sm = from w in state
                     from x in DoSomething()
                     from y in DoSomethingElse()
                     select x + y;

            var res = sm("Hello");


            Assert.True(res.State == "Hello, World");
            Assert.True(res.Value == 3);
        }

        [Fact]
        public void StateTest2()
        {
            var sm = from x in State.Get<string>()
                     from y in State.Put("Hello"+x)
                     select y;

            var res = sm(", World");

            Assert.True(res.State == "Hello, World");
        }

        [Fact]
        public void StateTest3()
        {
            var initial = State.Return<string,int>(10);

            var sm = from x in initial
                     from t in State.Get<string>()
                     from y in State.Put("Hello " + (x * 10) + t)
                     select y;

            var res = sm(", World");

            Assert.True(res.State == "Hello 100, World");
        }

        [Fact]
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

            Assert.True(res.State == "Hello 30, World");
        }

        [Fact]
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

            Assert.True(res.State == "Hello 30, World 500");
            Assert.True(res.Value == 15000);
        }

        [Fact]
        public void StateTest6()
        {            
            var first  = State.Return<string,int>(10);
            var second = State.Return<string,int>(3);
            var third  = State.Return<string,int>(5);
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

            var res = sm(", World"); // Invoke with the initial state

            Assert.True(res.State == "Hello 30, Worldyyy 500");
            Assert.True(res.Value == 15000);
        }

        static State<Unit, S> Put<S>( S state )
        {
            return _ => StateResult.Create<Unit, S>(Unit.Default, state);
        } 

        State<string,int> DoSomethingElse()
        {
            return state => StateResult.Create(state + "rld",1);
        }

        State<string,int> DoSomething()
        {
            return state => StateResult.Create(state + ", Wo",2);
        }
    }
}

