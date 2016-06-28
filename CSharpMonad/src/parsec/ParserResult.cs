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
using Monad.Utility;

namespace Monad.Parsec
{
    public class ParserResult<A>
    {
        public readonly ImmutableList<Tuple<A, ImmutableList<ParserChar>>> Value;
        public readonly ImmutableList<ParserError> Errors;

        public ParserResult(ImmutableList<Tuple<A, ImmutableList<ParserChar>>> value)
        {
            Value = value;
        }

        public ParserResult(Tuple<A, ImmutableList<ParserChar>>[] value)
        {
            Value = new ImmutableList<Tuple<A, ImmutableList<ParserChar>>>(value);
        }

        public ParserResult(ImmutableList<ParserError> errors)
        {
            Value = new ImmutableList<Tuple<A, ImmutableList<ParserChar>>>(new Tuple<A, ImmutableList<ParserChar>>[0]);
            Errors = errors;
        }

        public bool IsFaulted
        {
            get
            {
                return Errors != null;
            }
        }
    }

    public static class ParserResult
    {
        public static ParserResult<A> Fail<A>(ImmutableList<ParserError> errors)
        {
            return new ParserResult<A>(errors);
        }

        public static ParserResult<A> Fail<A>(ParserError[] errors)
        {
            return new ParserResult<A>(new ImmutableList<ParserError>(errors));
        }

        public static ParserResult<A> Fail<A>(ParserError error)
        {
            return new ParserResult<A>(error.Cons());
        }

        public static ParserResult<A> Fail<A>(string expected, ImmutableList<ParserChar> input, string message = "")
        {
            return new ParserResult<A>(new ParserError(expected,input,message).Cons());
        }

        public static ParserResult<A> Success<A>(this ImmutableList<Tuple<A, ImmutableList<ParserChar>>> self)
        {
            return new ParserResult<A>(self);
        }

        public static ParserResult<A> Success<A>(this IEnumerable<Tuple<A, ImmutableList<ParserChar>>> self)
        {
            return new ParserResult<A>(new ImmutableList<Tuple<A, ImmutableList<ParserChar>>>(self));
        }
    }
}
