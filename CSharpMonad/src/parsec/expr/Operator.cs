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
        Infix,
        Postfix,
        Prefix
    }

    public abstract class Operator<A> : Parser<A>
    {
        public Operator(Func<IEnumerable<ParserChar>, ParserResult<A>> fn)
            :
            base(fn)
        {

        }

        public abstract OperatorType OpType
        {
            get;
        }
    }

    public class Infix<A> : Operator<OperatorToken<A>>
    {
        public readonly Assoc Assoc;

        public Infix(string operatorStr, Parser<A> lhsParser, Parser<A> rhsParser)
            :
            base(
                inp => (from lhs in lhsParser
                        from op in New.String(operatorStr)
                        from rhs in rhsParser
                        select new OperatorToken<A>(operatorStr,lhs,rhs,OperatorType.Infix,inp.First().Location))
                       .Parse(inp)        
            )
        {
        }

        public override OperatorType OpType
        {
            get
            {
                return OperatorType.Infix;
            }
        }
    }

    public class Prefix<A> : Operator<OperatorToken<A>>
    {
        public Prefix(string operatorStr, Parser<A> rhsParser)
            :
            base(
                inp => (from op in New.String(operatorStr)
                        from rhs in rhsParser
                        select new OperatorToken<A>(operatorStr,default(A),rhs,OperatorType.Prefix,inp.First().Location))
                       .Parse(inp)
            )
        {
        }

        public override OperatorType OpType
        {
            get
            {
                return OperatorType.Prefix;
            }
        }
    }

    public class Postfix<A> : Operator<OperatorToken<A>>
    {
        public Postfix(string operatorStr, Parser<A> lhsParser)
            :
            base(
                inp => (from lhs in lhsParser
                        from op in New.String(operatorStr)
                        select new OperatorToken<A>(operatorStr, lhs, default(A), OperatorType.Postfix, inp.First().Location))
                       .Parse(inp)
            )
        {
        }

        public override OperatorType OpType
        {
            get
            {
                return OperatorType.Postfix;
            }
        }
    }
}
