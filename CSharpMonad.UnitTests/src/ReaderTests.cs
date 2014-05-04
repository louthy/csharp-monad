using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Monad;

namespace Monad.UnitTests
{
    [TestFixture]
    public class ReaderTests
    {
        [Test]
        public void ReaderBindTest1()
        {
            var person = new Person { Name = "Joe", Surname = "Bloggs" };

            var reader = from n in Name()
                         from s in Surname()
                         select n + " " + s;

            Assert.IsTrue(reader(person) == "Joe Bloggs");

        }

        [Test]
        public void ReaderAskTest1()
        {
            var person = new Person { Name = "Joe", Surname = "Bloggs" };

            var reader = from p in Reader.Ask<Person>()
                         select p.Name  + " " + p.Surname;

            Assert.IsTrue(reader(person) == "Joe Bloggs");

        }

        [Test]
        public void ReaderAskTest2()
        {
            var person = new Person { Name = "Joe", Surname = "Bloggs" };

            var reader = from p in Reader.Ask<Person>()
                         let nl = p.Name.Length
                         let sl = p.Surname.Length
                         select nl * sl;

            Assert.IsTrue(reader(person) == 18);

        }

        [Test]
        public void ReaderAskReturnAndBindTest()
        {
            var person = new Person { Name = "Joe", Surname = "Bloggs" };

            var initial = Reader.Return<Person,int>(10);

            var reader = from x in initial
                         from p in Reader.Ask<Person>()
                         let nl = p.Name.Length
                         let sl = p.Surname.Length
                         select nl * sl * x;

            Assert.IsTrue(reader(person) == 180);
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
