using NUnit.Framework;
using Monad;
using Monad.Parsec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpMonad.UnitTests
{
    [TestFixture]
    public class ParsecTests
    {
		[Test]
		public void TestOR()
		{
			var p = 
				(from fst in New.String("robert")
				 select fst)
					.Or(from snd in New.String("jimmy")
				        select snd)
					.Or(from thrd in New.String("john paul")
				        select thrd)
					.Or(from fth in New.String("john")
				        select fth);

			var r = p.Parse("robert");
			Assert.IsTrue(!r.IsFaulted && r.Value.Single().Item1.IsEqualTo("robert"));
			r = p.Parse("jimmy");
			Assert.IsTrue(!r.IsFaulted && r.Value.Single().Item1.IsEqualTo("jimmy"));
			r = p.Parse("john paul");
			Assert.IsTrue(!r.IsFaulted && r.Value.Single().Item1.IsEqualTo("john paul"));
			r = p.Parse("john");
			Assert.IsTrue(!r.IsFaulted && r.Value.Single().Item1.IsEqualTo("john"));
		}

        [Test]
        public void TestBinding()
        {
            var p = from x in New.Item()
                    from _ in New.Item()
                    from y in New.Item()
                    select new ParserChar[] { x, y };

            var res = p.Parse("abcdef").Value.Single();

            Assert.IsTrue(res.Item1.First().Value == 'a' &&
                          res.Item1.Second().Value == 'c');

            Assert.IsTrue(res.Item1.First().Location.Line == 1);
            Assert.IsTrue(res.Item1.First().Location.Column == 1);

            Assert.IsTrue(res.Item1.Second().Location.Line == 1);
            Assert.IsTrue(res.Item1.Second().Location.Column == 3);

            int found = p.Parse("ab").Value.Count();

            Assert.IsTrue(found == 0);

        }

        [Test]
        public void TestInteger()
        {
            var p = New.Integer();

            Assert.IsTrue(!p.Parse("123").IsFaulted && p.Parse("123").Value.Single().Item1 == 123);
            Assert.IsTrue(!p.Parse("-123").IsFaulted && p.Parse("-123").Value.Single().Item1 == -123);
            Assert.IsTrue(!p.Parse(int.MaxValue.ToString()).IsFaulted && p.Parse(int.MaxValue.ToString()).Value.Single().Item1 == int.MaxValue);

            // TODO: Possible Mono bug here, overflow exception is thrown.  Need to check on .NET
            //    Assert.IsTrue(!p.Parse(int.MinValue.ToString()).IsFaulted && p.Parse(int.MinValue.ToString()).Value.Single().Item1 == int.MinValue);
        }

        [Test]
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

            var r = p.Parse("[1,2,3,4]").Value.Single();

            Assert.IsTrue(r.Item1.First().Value == '1');
            Assert.IsTrue(r.Item1.Skip(1).First().Value == '2');
            Assert.IsTrue(r.Item1.Skip(2).First().Value == '3');
            Assert.IsTrue(r.Item1.Skip(3).First().Value == '4');

            var r2 = p.Parse("[1,2,3,4");
            Assert.IsTrue(r2.IsFaulted);
            Assert.IsTrue(r2.Errors.First().Expected == "']'");
            Assert.IsTrue(r2.Errors.First().Input.Count() == 0);

            var r3 = p.Parse("[1,2,3,4*");
            Assert.IsTrue(r3.IsFaulted);
            Assert.IsTrue(r3.Errors.First().Expected == "']'");
            Assert.IsTrue(r3.Errors.First().Location.Line == 1);
            Assert.IsTrue(r3.Errors.First().Location.Column == 9);

        }

        [Test]
        public void TestString()
        {
            var r = New.String("he").Parse("hell").Value.Single();
            Assert.IsTrue(r.Item1.AsString() == "he");
            Assert.IsTrue(r.Item2.AsString() == "ll");

            r = New.String("hello").Parse("hello, world").Value.Single();
            Assert.IsTrue(r.Item1.AsString() == "hello");
            Assert.IsTrue(r.Item2.AsString() == ", world");
        }

        [Test]
        public void TestMany()
        {
            var r = New.Many(New.Character('a')).Parse("aaabcde").Value.Single();
            Assert.IsTrue(r.Item1.AsString() == "aaa");
            Assert.IsTrue(r.Item2.AsString() == "bcde");
        }

        [Test]
        public void TestMany1()
        {
            var r = New.Many1(New.Character('a')).Parse("aaabcde").Value.Single();
            Assert.IsTrue(r.Item1.AsString() == "aaa");
            Assert.IsTrue(r.Item2.AsString() == "bcde");

            var r2 = New.Many1(New.Character('a')).Parse("bcde");
            Assert.IsTrue(r2.Value.Count() == 0);
        }

        [Test]
        public void TestDigit()
        {
            var r = New.Digit().Parse("1").Value.Single();
            Assert.IsTrue(r.Item1.Value == '1');
        }

        [Test]
        public void TestChar()
        {
            var r = New.Character('X').Parse("X").Value.Single();
            Assert.IsTrue(r.Item1.Value == 'X');
        }

        [Test]
        public void TestSatisfy()
        {
            var r = New.Satisfy(c => c == 'x', "'x'").Parse("xbxcxdxe").Value.Single();
            Assert.IsTrue(r.Item1.Value == 'x');
            Assert.IsTrue(r.Item2.AsString() == "bxcxdxe");
        }

        [Test]
        public void TestItem()
        {
            Assert.IsTrue(
                New.Item().Parse("").Value.Count() == 0
                );

            var r = New.Item().Parse("abc").Value.Single();
            Assert.IsTrue(
                r.Item1.Value == 'a' &&
                r.Item2.AsString() == "bc"
                );
        }

        [Test]
        public void TestFailure()
        {
            var inp = "abc".ToParserChar();

            var parser = New.Failure<bool>(ParserError.Create("failed because...", inp));

            var result = parser.Parse(inp);

            Assert.IsTrue(
                result.Value.Count() == 0
                );
        }

        [Test]
        public void TestReturn()
        {
            var r = New.Return(1).Parse("abc").Value.Single();
            Assert.IsTrue(
                r.Item1 == 1 &&
                r.Item2.AsString() == "abc"
                );
        }

        [Test]
        public void TestChoice()
        {
            var r = New.Choice(New.Item(), New.Return(New.ParserChar('d'))).Parse("abc").Value.Single();
            Assert.IsTrue(
                r.Item1.Value == 'a' &&
                r.Item2.AsString() == "bc"
                );

            var inp = "abc".ToParserChar();

            var parser = New.Choice(
                    New.Failure<ParserChar>( ParserError.Create("failed because...",inp) ), 
                    New.Return(New.ParserChar('d'))
                )
                .Parse(inp);

            r = parser.Value.Single();

            Assert.IsTrue(
                r.Item1.Value == 'd' &&
                r.Item2.AsString() == "abc"
                );
        }

        [Test]
        public void TestWhiteSpace()
        {
            var r = New.WhiteSpace().Parse(" ");
            Assert.IsFalse(r.IsFaulted);
            Assert.IsTrue(r.Value.Count() == 1);
            Assert.IsTrue(r.Value.Single().Item1.AsString() == " ");

        }

        [Test]
        public void TestWhiteSpace2()
        {
            var r = New.WhiteSpace().Parse("a");
            Assert.IsFalse(r.IsFaulted);
            Assert.IsTrue(r.Value.Count() == 1);

            var empty = r.Value.Single().Item1.AsString();
            Assert.IsTrue(empty == "");
            Assert.IsTrue(r.Value.Single().Item2.AsString() == "a");
        }
    }

}
