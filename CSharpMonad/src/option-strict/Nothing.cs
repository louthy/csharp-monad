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

namespace Monad
{
    /// <summary>
    /// Nothing case of the OptionStrict<T> monad
    /// </summary>
    public class NothingStrict<T> : OptionStrict<T>
    {
        public override string ToString()
        {
            return "[Nothing]";
        }

        public override T Value
        {
            get
            {
                throw new InvalidOperationException("OptionStrict<T>.Nothing has no value");
            }
        }

        public override bool HasValue
        {
            get
            {
                return false;
            }
        }

        public override R Match<R>(Func<R> Just, Func<R> Nothing)
        {
            if (Just == null) throw new ArgumentNullException("Just");
            if (Nothing == null) throw new ArgumentNullException("Nothing");
            return Nothing();
        }

        public override R Match<R>(Func<T, R> Just, Func<R> Nothing)
        {
            if (Just == null) throw new ArgumentNullException("Just");
            if (Nothing == null) throw new ArgumentNullException("Nothing");
            return Nothing();
        }

        public override R Match<R>(Func<R> Just, R Nothing)
        {
            if (Just == null) throw new ArgumentNullException("Just");
            return Nothing;
        }

        public override R Match<R>(Func<T, R> Just, R Nothing)
        {
            if (Just == null) throw new ArgumentNullException("Just");
            return Nothing;
        }

        /// <summary>
        /// Monadic append
        /// If the lhs or rhs are in a Nothing state then Nothing propagates
        /// </summary>
        public override OptionStrict<T> Mappend(OptionStrict<T> rhs)
        {
            if (rhs == null) throw new ArgumentNullException("rhs");
            return this;
        }
    }
}