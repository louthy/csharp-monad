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

using Xunit;
using Monad;
using Monad.Parsec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.UnitTests
{
    public class ParsecTests
    {
		[Fact]
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
			Assert.True(!r.IsFaulted && r.Value.Single().Item1.IsEqualTo("robert"));
			r = p.Parse("jimmy");
			Assert.True(!r.IsFaulted && r.Value.Single().Item1.IsEqualTo("jimmy"));
			r = p.Parse("john paul");
			Assert.True(!r.IsFaulted && r.Value.Single().Item1.IsEqualTo("john paul"));
			r = p.Parse("john");
			Assert.True(!r.IsFaulted && r.Value.Single().Item1.IsEqualTo("john"));
		}

        [Fact]
        public void TestBinding()
        {
            var p = from x in Prim.Item()
                    from _ in Prim.Item()
                    from y in Prim.Item()
                    select new ParserChar[] { x, y };

            var res = p.Parse("abcdef").Value.Single();

            Assert.True(res.Item1.First().Value == 'a' &&
                          res.Item1.Second().Value == 'c');

            Assert.True(res.Item1.First().Location.Line == 1);
            Assert.True(res.Item1.First().Location.Column == 1);

            Assert.True(res.Item1.Second().Location.Line == 1);
            Assert.True(res.Item1.Second().Location.Column == 3);

            bool found = p.Parse("ab").Value.IsEmpty;

            Assert.True(found);

        }

        [Fact]
        public void TestInteger()
        {
            var p = Prim.Integer();

            Assert.True(!p.Parse("123").IsFaulted && p.Parse("123").Value.Single().Item1 == 123);
            Assert.True(!p.Parse("-123").IsFaulted && p.Parse("-123").Value.Single().Item1 == -123);
            Assert.True(!p.Parse(int.MaxValue.ToString()).IsFaulted && p.Parse(int.MaxValue.ToString()).Value.Single().Item1 == int.MaxValue);

            // Bug here in both .NET and Mono, neither can parse an Int32.MinValue, overflow exception is thrown.
            //Assert.True(!p.Parse(int.MinValue.ToString()).IsFaulted && p.Parse(int.MinValue.ToString()).Value.Single().Item1 == int.MinValue);
        }

        [Fact]
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

            Assert.True(r.Item1.First().Value == '1');
            Assert.True(r.Item1.Skip(1).First().Value == '2');
            Assert.True(r.Item1.Skip(2).First().Value == '3');
            Assert.True(r.Item1.Skip(3).First().Value == '4');

            var r2 = p.Parse("[1,2,3,4");
            Assert.True(r2.IsFaulted);
            Assert.True(r2.Errors.First().Expected == "']'");
            Assert.True(r2.Errors.First().Input.IsEmpty);

            var r3 = p.Parse("[1,2,3,4*");
            Assert.True(r3.IsFaulted);
            Assert.True(r3.Errors.First().Expected == "']'");
            Assert.True(r3.Errors.First().Location.Line == 1);
            Assert.True(r3.Errors.First().Location.Column == 9);

        }

        [Fact]
        public void TestString()
        {
            var r = Prim.String("he").Parse("hell").Value.Single();
            Assert.True(r.Item1.AsString() == "he");
            Assert.True(r.Item2.AsString() == "ll");

            r = Prim.String("hello").Parse("hello, world").Value.Single();
            Assert.True(r.Item1.AsString() == "hello");
            Assert.True(r.Item2.AsString() == ", world");
        }

        [Fact]
        public void TestMany()
        {
            var r = Prim.Many(Prim.Character('a')).Parse("aaabcde").Value.Single();
            Assert.True(r.Item1.AsString() == "aaa");
            Assert.True(r.Item2.AsString() == "bcde");
        }

        [Fact]
        public void TestMany1()
        {
            var r = Prim.Many1(Prim.Character('a')).Parse("aaabcde").Value.Single();
            Assert.True(r.Item1.AsString() == "aaa");
            Assert.True(r.Item2.AsString() == "bcde");

            var r2 = Prim.Many1(Prim.Character('a')).Parse("bcde");
            Assert.True(r2.Value.IsEmpty);
        }

        [Fact]
        public void TestSkipMany1()
        {
            var p = Prim.SkipMany1(Prim.Character('*'));

            var r = p.Parse("****hello, world");
            Assert.True(!r.IsFaulted);
            var after = r.Value.Head().Item2.AsString();
            Assert.True(after == "hello, world");

            r = p.Parse("*hello, world");
            Assert.True(!r.IsFaulted);
            after = r.Value.Head().Item2.AsString();
            Assert.True(after == "hello, world");

            r = p.Parse("hello, world");
            Assert.True(r.IsFaulted);
        }

        [Fact]
        public void TestSkipMany()
        {
            var p = Prim.SkipMany(Prim.Character('*'));

            var r = p.Parse("****hello, world");
            Assert.True(!r.IsFaulted);
            var after = r.Value.Head().Item2.AsString();
            Assert.True(after == "hello, world");

            r = p.Parse("*hello, world");
            Assert.True(!r.IsFaulted);
            after = r.Value.Head().Item2.AsString();
            Assert.True(after == "hello, world");

            r = p.Parse("hello, world");
            Assert.True(!r.IsFaulted);
            after = r.Value.Head().Item2.AsString();
            Assert.True(after == "hello, world");
        }

        [Fact]
        public void TestOneOf()
        {
            var p = Prim.OneOf("xyz");
            var r = p.Parse("zzz");
            Assert.True(!r.IsFaulted && r.Value.Head().Item1.Value == 'z');
            r = p.Parse("xxx");
            Assert.True(!r.IsFaulted && r.Value.Head().Item1.Value == 'x');
            r = p.Parse("www");
            Assert.True(r.IsFaulted);
        }

        [Fact]
        public void TestNoneOf()
        {
            var p = Prim.NoneOf("xyz");
            var r = p.Parse("zzz");
            Assert.True(r.IsFaulted);
            r = p.Parse("xxx");
            Assert.True(r.IsFaulted);
            r = p.Parse("www");
            Assert.True(!r.IsFaulted && r.Value.Head().Item1.Value == 'w' && r.Value.Head().Item2.AsString() == "ww");
        }

        [Fact]
        public void TestDigit()
        {
            var r = Prim.Digit().Parse("1").Value.Single();
            Assert.True(r.Item1.Value == '1');
        }

        [Fact]
        public void TestChar()
        {
            var r = Prim.Character('X').Parse("X").Value.Single();
            Assert.True(r.Item1.Value == 'X');
        }

        [Fact]
        public void TestSatisfy()
        {
            var r = Prim.Satisfy(c => c == 'x', "'x'").Parse("xbxcxdxe").Value.Single();
            Assert.True(r.Item1.Value == 'x');
            Assert.True(r.Item2.AsString() == "bxcxdxe");
        }

        [Fact]
        public void TestItem()
        {
            Assert.True(
                Prim.Item().Parse("").Value.IsEmpty
                );

            var r = Prim.Item().Parse("abc").Value.Single();
            Assert.True(
                r.Item1.Value == 'a' &&
                r.Item2.AsString() == "bc"
                );
        }

        [Fact]
        public void TestFailure()
        {
            var inp = "abc".ToParserChar();

            var parser = Prim.Failure<bool>(ParserError.Create("failed because...", inp));

            var result = parser.Parse(inp);

            Assert.True( result.Value.IsEmpty );
        }

        [Fact]
        public void TestReturn()
        {
            var r = Prim.Return(1).Parse("abc").Value.Single();
            Assert.True(
                r.Item1 == 1 &&
                r.Item2.AsString() == "abc"
                );
        }

        [Fact]
        public void TestChoice()
        {
            var r = Prim.Choice(Prim.Item(), Prim.Return(Prim.ParserChar('d'))).Parse("abc").Value.Single();
            Assert.True(
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

            Assert.True(
                r.Item1.Value == 'd' &&
                r.Item2.AsString() == "abc"
                );
        }

        [Fact]
        public void TestWhiteSpace()
        {
            var r = Prim.WhiteSpace().Parse(" ");
            Assert.False(r.IsFaulted);
            Assert.True(r.Value.Count() == 1);

        }

        [Fact]
        public void TestWhiteSpace2()
        {
            var r = Prim.WhiteSpace().Parse("a");
            Assert.False(r.IsFaulted);
            Assert.True(r.Value.Count() == 1);

            var empty = r.Value.Count() == 1;
            Assert.True(empty);
            Assert.True(r.Value.Single().Item2.AsString() == "a");
        }

        [Fact]
        public void TestBetween()
        {
            var r = Prim.Between(Prim.Character('['), Prim.Character(']'), Prim.String("abc")).Parse("[abc]");
            Assert.True(!r.IsFaulted);
            Assert.True(r.Value.First().Item1.AsString() == "abc");
        }
    }

}
