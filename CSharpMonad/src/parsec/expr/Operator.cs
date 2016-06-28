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

namespace Monad.Parsec.Expr
{
    public enum OperatorType
    {
        None,
        Infix,
        Postfix,
        Prefix
    }

    public abstract class Operator<A>
    {
        public readonly OperatorType Type;
        public readonly string Name;

        public Operator(OperatorType type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    public class Infix<A> : Operator<A>
    {
        public readonly Assoc Assoc;
        public readonly Parser<Func<A, A, A>> Parser;

        public Infix(string name, Parser<Func<A, A, A>> parser, Assoc assoc)
            :
            base(OperatorType.Infix, name)
        {
            Parser = parser;
            Assoc = assoc;
        }
    }

    public class Prefix<A> : Operator<A>
    {
        public readonly Parser<Func<A, A>> Parser;

        public Prefix(string name, Parser<Func<A, A>> parser)
            :
            base(OperatorType.Prefix, name)
        {
            Parser = parser;
        }
    }

    public class Postfix<A> : Operator<A>
    {
        public readonly Parser<Func<A, A>> Parser;

        public Postfix(string name, Parser<Func<A, A>> parser)
            :
            base(OperatorType.Postfix, name)
        {
            Parser = parser;
        }
    }
}
