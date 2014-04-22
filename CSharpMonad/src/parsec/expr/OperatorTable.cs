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

namespace Monad.Parsec.Expr
{
    public class OperatorTable<A> : IEnumerable<IEnumerable<Operator<A>>>
    {
        List<IEnumerable<Operator<A>>> ops = new List<IEnumerable<Operator<A>>>();
        int current = -1;

        public OperatorTable()
        {
        }

        public OperatorTable<A> AddRow()
        {
            current++;
            ops.Add(new Operator<A>[0]);
            return this;
        }

        public OperatorTable<A> Add(Operator<A> op)
        {
            ops[current] = ops[current].Concat(op.Cons());
            return this;
        }

        public IEnumerable<IEnumerable<Operator<A>>> AsImmutableList()
        {
            return ops;
        }

        public IEnumerator<IEnumerable<Operator<A>>> GetEnumerator()
        {
            return ((IEnumerable<IEnumerable<Operator<A>>>)ops).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IEnumerable<Operator<A>>>)ops).GetEnumerator();
        }
    }
}
