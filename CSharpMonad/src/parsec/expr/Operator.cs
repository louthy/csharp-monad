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
using Monad;
using Monad.Parsec;
using Monad.Parsec.Token;

namespace Monad.Parsec.Expr
{
    public enum OperatorType
    {
        None,
        Infix,
        Postfix,
        Prefix
    }

    public class OperatorDef<A>
    {
        public readonly OperatorType Type;
        public readonly string Op;
        public readonly Func<A, A, A> BinaryFn;
        public readonly Func<A, A> UnaryFn;
        public readonly Assoc Assoc;

        public OperatorDef(
            OperatorType type,
            string op,
            Func<A, A, A> fn,
            Assoc assoc
            )
        {
            Type = type;
            Op = op;
            BinaryFn = fn;
            Assoc = assoc;
        }

        public OperatorDef(
            OperatorType type,
            string op,
            Func<A, A> fn,
            Assoc assoc
            )
        {
            Type = type;
            Op = op;
            UnaryFn = fn;
            Assoc = assoc;
        }
    }

    public static class OperatorDef
    {
        public static OperatorDef<A> Id<A>()
        {
            return new OperatorDef<A>(OperatorType.None, "<id>", a => a, Assoc.None);
        }
    }

    public class Operator<A> : Parser<OperatorDef<A>>
    {
        public readonly OperatorDef<A> Def;

        public Operator(OperatorDef<A> operatorDef)
            :
            base(
                inp => (from o in Gen.String(operatorDef.Op)    // TODO: New.String probably isn't good enough
                        select operatorDef)
                       .Parse(inp)
            )
        {
            Def = operatorDef;
        }
    }
}
