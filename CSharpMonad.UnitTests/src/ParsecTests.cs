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

using NUnit.Framework;
using Monad;
using Monad.Parsec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.UnitTests
{
    [TestFixture]
    public class ParsecTests
    {
		[Test]
		public void TestOR()
		{
			var p = 
				(from fst in Prim.String("robert")
				 select fst)
					.Or(from snd in Prim.String("jimmy")
				        select snd)
					.Or(from thrd in Prim.String("john paul")
				        select thrd)
					.Or(from fth in Prim.String("john")
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
            var p = from x in Prim.Item()
                    from _ in Prim.Item()
                    from y in Prim.Item()
                    select new ParserChar[] { x, y };

            var res = p.Parse("abcdef").Value.Single();

            Assert.IsTrue(res.Item1.First().Value == 'a' &&
                          res.Item1.Second().Value == 'c');

            Assert.IsTrue(res.Item1.First().Location.Line == 1);
            Assert.IsTrue(res.Item1.First().Location.Column == 1);

            Assert.IsTrue(res.Item1.Second().Location.Line == 1);
            Assert.IsTrue(res.Item1.Second().Location.Column == 3);

            bool found = p.Parse("ab").Value.IsEmpty;

            Assert.IsTrue(found);

        }

        [Test]
        public void TestInteger()
        {
            var p = Prim.Integer();

            Assert.IsTrue(!p.Parse("123").IsFaulted && p.Parse("123").Value.Single().Item1 == 123);
            Assert.IsTrue(!p.Parse("-123").IsFaulted && p.Parse("-123").Value.Single().Item1 == -123);
            Assert.IsTrue(!p.Parse(int.MaxValue.ToString()).IsFaulted && p.Parse(int.MaxValue.ToString()).Value.Single().Item1 == int.MaxValue);

            // Bug here in both .NET and Mono, neither can parse an Int32.MinValue, overflow exception is thrown.
            //Assert.IsTrue(!p.Parse(int.MinValue.ToString()).IsFaulted && p.Parse(int.MinValue.ToString()).Value.Single().Item1 == int.MinValue);
        }

        [Test]
        public void TestDigitList()
        {
            var p = from open in Prim.Character('[')
                    from d in Prim.Digit()
                    from ds in
                        Prim.Many(
                            from comma in Prim.Character(',')
                            from digit in Prim.Digit()
                            select digit
                            )
                    from close in Prim.Character(']')
                    select d.Cons(ds);

            var r = p.Parse("[1,2,3,4]").Value.Single();

            Assert.IsTrue(r.Item1.First().Value == '1');
            Assert.IsTrue(r.Item1.Skip(1).First().Value == '2');
            Assert.IsTrue(r.Item1.Skip(2).First().Value == '3');
            Assert.IsTrue(r.Item1.Skip(3).First().Value == '4');

            var r2 = p.Parse("[1,2,3,4");
            Assert.IsTrue(r2.IsFaulted);
            Assert.IsTrue(r2.Errors.First().Expected == "']'");
            Assert.IsTrue(r2.Errors.First().Input.IsEmpty);

            var r3 = p.Parse("[1,2,3,4*");
            Assert.IsTrue(r3.IsFaulted);
            Assert.IsTrue(r3.Errors.First().Expected == "']'");
            Assert.IsTrue(r3.Errors.First().Location.Line == 1);
            Assert.IsTrue(r3.Errors.First().Location.Column == 9);

        }

        [Test]
        public void TestString()
        {
            var r = Prim.String("he").Parse("hell").Value.Single();
            Assert.IsTrue(r.Item1.AsString() == "he");
            Assert.IsTrue(r.Item2.AsString() == "ll");

            r = Prim.String("hello").Parse("hello, world").Value.Single();
            Assert.IsTrue(r.Item1.AsString() == "hello");
            Assert.IsTrue(r.Item2.AsString() == ", world");
        }

        [Test]
        public void TestMany()
        {
            var r = Prim.Many(Prim.Character('a')).Parse("aaabcde").Value.Single();
            Assert.IsTrue(r.Item1.AsString() == "aaa");
            Assert.IsTrue(r.Item2.AsString() == "bcde");
        }

        [Test]
        public void TestMany1()
        {
            var r = Prim.Many1(Prim.Character('a')).Parse("aaabcde").Value.Single();
            Assert.IsTrue(r.Item1.AsString() == "aaa");
            Assert.IsTrue(r.Item2.AsString() == "bcde");

            var r2 = Prim.Many1(Prim.Character('a')).Parse("bcde");
            Assert.IsTrue(r2.Value.IsEmpty);
        }

        [Test]
        public void TestSkipMany1()
        {
            var p = Prim.SkipMany1(Prim.Character('*'));

            var r = p.Parse("****hello, world");
            Assert.IsTrue(!r.IsFaulted);
            var after = r.Value.Head().Item2.AsString();
            Assert.IsTrue(after == "hello, world");

            r = p.Parse("*hello, world");
            Assert.IsTrue(!r.IsFaulted);
            after = r.Value.Head().Item2.AsString();
            Assert.IsTrue(after == "hello, world");

            r = p.Parse("hello, world");
            Assert.IsTrue(r.IsFaulted);
        }

        [Test]
        public void TestSkipMany()
        {
            var p = Prim.SkipMany(Prim.Character('*'));

            var r = p.Parse("****hello, world");
            Assert.IsTrue(!r.IsFaulted);
            var after = r.Value.Head().Item2.AsString();
            Assert.IsTrue(after == "hello, world");

            r = p.Parse("*hello, world");
            Assert.IsTrue(!r.IsFaulted);
            after = r.Value.Head().Item2.AsString();
            Assert.IsTrue(after == "hello, world");

            r = p.Parse("hello, world");
            Assert.IsTrue(!r.IsFaulted);
            after = r.Value.Head().Item2.AsString();
            Assert.IsTrue(after == "hello, world");
        }

        [Test]
        public void TestOneOf()
        {
            var p = Prim.OneOf("xyz");
            var r = p.Parse("zzz");
            Assert.IsTrue(!r.IsFaulted && r.Value.Head().Item1.Value == 'z');
            r = p.Parse("xxx");
            Assert.IsTrue(!r.IsFaulted && r.Value.Head().Item1.Value == 'x');
            r = p.Parse("www");
            Assert.IsTrue(r.IsFaulted);
        }

        [Test]
        public void TestNoneOf()
        {
            var p = Prim.NoneOf("xyz");
            var r = p.Parse("zzz");
            Assert.IsTrue(r.IsFaulted);
            r = p.Parse("xxx");
            Assert.IsTrue(r.IsFaulted);
            r = p.Parse("www");
            Assert.IsTrue(!r.IsFaulted && r.Value.Head().Item1.Value == 'w' && r.Value.Head().Item2.AsString() == "ww");
        }

        [Test]
        public void TestDigit()
        {
            var r = Prim.Digit().Parse("1").Value.Single();
            Assert.IsTrue(r.Item1.Value == '1');
        }

        [Test]
        public void TestChar()
        {
            var r = Prim.Character('X').Parse("X").Value.Single();
            Assert.IsTrue(r.Item1.Value == 'X');
        }

        [Test]
        public void TestSatisfy()
        {
            var r = Prim.Satisfy(c => c == 'x', "'x'").Parse("xbxcxdxe").Value.Single();
            Assert.IsTrue(r.Item1.Value == 'x');
            Assert.IsTrue(r.Item2.AsString() == "bxcxdxe");
        }

        [Test]
        public void TestItem()
        {
            Assert.IsTrue(
                Prim.Item().Parse("").Value.IsEmpty
                );

            var r = Prim.Item().Parse("abc").Value.Single();
            Assert.IsTrue(
                r.Item1.Value == 'a' &&
                r.Item2.AsString() == "bc"
                );
        }

        [Test]
        public void TestFailure()
        {
            var inp = "abc".ToParserChar();

            var parser = Prim.Failure<bool>(ParserError.Create("failed because...", inp));

            var result = parser.Parse(inp);

            Assert.IsTrue( result.Value.IsEmpty );
        }

        [Test]
        public void TestReturn()
        {
            var r = Prim.Return(1).Parse("abc").Value.Single();
            Assert.IsTrue(
                r.Item1 == 1 &&
                r.Item2.AsString() == "abc"
                );
        }

        [Test]
        public void TestChoice()
        {
            var r = Prim.Choice(Prim.Item(), Prim.Return(Prim.ParserChar('d'))).Parse("abc").Value.Single();
            Assert.IsTrue(
                r.Item1.Value == 'a' &&
                r.Item2.AsString() == "bc"
                );

            var inp = "abc".ToParserChar();

            var parser = Prim.Choice(
                    Prim.Failure<ParserChar>( ParserError.Create("failed because...",inp) ), 
                    Prim.Return(Prim.ParserChar('d'))
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
            var r = Prim.WhiteSpace().Parse(" ");
            Assert.IsFalse(r.IsFaulted);
            Assert.IsTrue(r.Value.Count() == 1);

        }

        [Test]
        public void TestWhiteSpace2()
        {
            var r = Prim.WhiteSpace().Parse("a");
            Assert.IsFalse(r.IsFaulted);
            Assert.IsTrue(r.Value.Count() == 1);

            var empty = r.Value.Count() == 1;
            Assert.IsTrue(empty);
            Assert.IsTrue(r.Value.Single().Item2.AsString() == "a");
        }

        [Test]
        public void TestBetween()
        {
            var r = Prim.Between(Prim.Character('['), Prim.Character(']'), Prim.String("abc")).Parse("[abc]");
            Assert.IsTrue(!r.IsFaulted);
            Assert.IsTrue(r.Value.First().Item1.AsString() == "abc");
        }
    }

}
