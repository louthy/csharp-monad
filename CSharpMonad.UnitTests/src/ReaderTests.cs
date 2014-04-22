using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Monad;

namespace Monad.UnitTests.src
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
