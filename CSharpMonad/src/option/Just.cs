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

namespace Monad
{
    /// <summary>
    /// Just case of the Option<T> monad
    /// </summary>
    public class Just<T> : Option<T>
    {
        private readonly T value;

        public Just(T value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override T Value
        {
            get
            {
                return value;
            }
        }

        public override bool HasValue
        {
            get
            {
                return true;
            }
        }

        public override R Match<R>(Func<R> Just, Func<R> Nothing)
        {
            return Just();
        }

        public override R Match<R>(Func<T, R> Just, Func<R> Nothing)
        {
            return Just(Value);
        }

        public override R Match<R>(Func<R> Just, R Nothing)
        {
            return Just();
        }

        public override R Match<R>(Func<T, R> Just, R Nothing)
        {
            return Just(Value);
        }
    }
}