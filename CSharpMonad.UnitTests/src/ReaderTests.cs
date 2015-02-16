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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Monad;

namespace Monad.UnitTests
{
    public class ReaderTests
    {
        [Fact]
        public void ReaderBindTest1()
        {
            var person = new Person { Name = "Joe", Surname = "Bloggs" };

            var reader = from n in Name()
                         from s in Surname()
                         select n + " " + s;

            Assert.True(reader(person) == "Joe Bloggs");

        }

        [Fact]
        public void ReaderAskTest1()
        {
            var person = new Person { Name = "Joe", Surname = "Bloggs" };

            var reader = from p in Reader.Ask<Person>()
                         select p.Name  + " " + p.Surname;

            Assert.True(reader(person) == "Joe Bloggs");

        }

        [Fact]
        public void ReaderAskTest2()
        {
            var person = new Person { Name = "Joe", Surname = "Bloggs" };

            var reader = from p in Reader.Ask<Person>()
                         let nl = p.Name.Length
                         let sl = p.Surname.Length
                         select nl * sl;

            Assert.True(reader(person) == 18);

        }

        [Fact]
        public void ReaderAskReturnAndBindTest()
        {
            var person = new Person { Name = "Joe", Surname = "Bloggs" };

            var env = Reader.Return<Person,int>(10);

            var reader = from x in env
                         from p in Reader.Ask<Person>()
                         let nl = p.Name.Length
                         let sl = p.Surname.Length
                         select nl * sl * x;

            Assert.True(reader(person) == 180);
        }

        [Fact]
        public void ReaderAskReturnAndBindTest2()
        {
            var person = new Person { Name = "Joe", Surname = "Bloggs" };

            var env = Reader.Return<Person,int>(10);

            var reader = from x in env
                         from p in env.Ask()
                         let nl = p.Name.Length
                         let sl = p.Surname.Length
                         select nl * sl * x;

            Assert.True(reader(person) == 180);
        }


        class Person
        {
            public string Name;
            public string Surname;
        }

        private static Reader<Person, string> Name()
        {
            return env => env.Name;
        }

        private static Reader<Person, string> Surname()
        {
            return env => env.Surname;
        }

    }
}
