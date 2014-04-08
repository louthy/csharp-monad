using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monad;
using Monad.Parsec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpMonad.UnitTests
{
    [TestClass]
    public class ParsecTests
    {
        [TestMethod]
        public void TestBinding()
        {
            var p = from x in New.Item()
                    from _ in New.Item()
                    from y in New.Item()
                    select new char[] { x, y };

            var res = p.Parse("abcdef").Single();

            Assert.IsTrue(res.Item1.First() == 'a' &&
                           res.Item1.Second() == 'c');

            int found = p.Parse("ab").Count();

            Assert.IsTrue(found == 0);

        }

        [TestMethod]
        public void TestDigitList()
        {
            var p = from open in New.Character('[')
                    from d in New.Digit()
                    from ds in
                        New.Many(
                            from comma in New.Character(',')
                            from digit in New.Digit()
                            select digit
                            )
                    from close in New.Character(']')
                    select d.Cons(ds);

            var r = p.Parse("[1,2,3,4]").Single();

            Assert.IsTrue(r.Item1.First() == '1');
            Assert.IsTrue(r.Item1.Skip(1).First() == '2');
            Assert.IsTrue(r.Item1.Skip(2).First() == '3');
            Assert.IsTrue(r.Item1.Skip(3).First() == '4');

            var count = p.Parse("[1,2,3,4").Count();

            Assert.IsTrue(count == 0);

        }

        [TestMethod]
        public void TestString()
        {
            var r = New.String("he").Parse("hell").Single();
            Assert.IsTrue(r.Item1.AsString() == "he");
            Assert.IsTrue(r.Item2.AsString() == "ll");

            r = New.String("hello").Parse("hello, world").Single();
            Assert.IsTrue(r.Item1.AsString() == "hello");
            Assert.IsTrue(r.Item2.AsString() == ", world");
        }

        [TestMethod]
        public void TestMany()
        {
            var r = New.Many( New.Character('a') ).Parse("aaabcde").Single();
            Assert.IsTrue(r.Item1.AsString() == "aaa");
            Assert.IsTrue(r.Item2.AsString() == "bcde");
        }

        [TestMethod]
        public void TestMany1()
        {
            var r = New.Many1(New.Character('a')).Parse("aaabcde").Single();
            Assert.IsTrue(r.Item1.AsString() == "aaa");
            Assert.IsTrue(r.Item2.AsString() == "bcde");

            var r2 = New.Many1(New.Character('a')).Parse("bcde");
            Assert.IsTrue(r2.Count() == 0);
        }

        [TestMethod]
        public void TestDigit()
        {
            var r = New.Digit().Parse("1").Single();
            Assert.IsTrue(r.Item1 == '1');
        }

        [TestMethod]
        public void TestChar()
        {
            var r = New.Character('X').Parse("X").Single();
            Assert.IsTrue(r.Item1 == 'X');
        }

        [TestMethod]
        public void TestSatisfy()
        {
            var r = New.Satisfy( c => c == 'x' ).Parse( "xbxcxdxe" ).Single();
            Assert.IsTrue(r.Item1 == 'x');
            Assert.IsTrue(r.Item2.AsString() == "bxcxdxe");
        }

        [TestMethod]
        public void TestItem()
        {
            Assert.IsTrue(
                New.Item().Parse("").Count() == 0
                );

            var r = New.Item().Parse("abc").Single();
            Assert.IsTrue(
                r.Item1 == 'a' &&
                r.Item2.AsString() == "bc"
                );
        }

        [TestMethod]
        public void TestFailure()
        {
            Assert.IsTrue(
                New.Failure<bool>().Parse("abc").Count() == 0
                );
        }

        [TestMethod]
        public void TestReturn()
        {
            var r = New.Return(1).Parse("abc").Single();
            Assert.IsTrue(
                r.Item1 == 1 &&
                r.Item2.AsString() == "abc"
                );
        }

        [TestMethod]
        public void TestChoice()
        {
            var r = New.Choice(New.Item(), New.Return('d')).Parse("abc").Single();
            Assert.IsTrue(
                r.Item1 == 'a' &&
                r.Item2.AsString() == "bc"
                );

            r = New.Choice(New.Failure<char>(), New.Return('d')).Parse("abc").Single();
            Assert.IsTrue(
                r.Item1 == 'd' &&
                r.Item2.AsString() == "abc"
                );
        }

        [TestMethod]
        public void Log(object obj)
        {
            if (obj == null)
                Console.WriteLine("[null]");
            else
                Console.WriteLine(obj.ToString());
        }
    }

}
