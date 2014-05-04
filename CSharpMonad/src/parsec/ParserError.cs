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
using Monad.Utility;

namespace Monad.Parsec
{
    public class ParserError
    {
        public readonly string Message;
        public readonly string Expected;
        public readonly ImmutableList<ParserChar> Input;
        public readonly SrcLoc Location;

        public ParserError(string expected, ImmutableList<ParserChar> input, string message = "")
        {
            Message = message;
            Expected = expected;
            Input = input;
            if (input.IsEmpty)
            {
                Location = SrcLoc.EndOfSource;
            }
            else
            {
                Location = input.Head().Location;
            }
        }

        public static ParserError Create(string expected, ImmutableList<ParserChar> input, string message = "")
        {
            return new ParserError(expected, input, message);
        }

        public static ParserError Create(char expected, ImmutableList<ParserChar> input, string message = "")
        {
            return new ParserError(expected.ToString(), input, message);
        }
    }
}
