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

namespace Monad.Parsec.Token.Bracketing
{
    public class Parens<A> : Parser<A>
    {
        public Parens(Parser<A> betweenParser)
            :
            base( inp => Prim.Between( Tok.Symbol("("), Tok.Symbol(")"), betweenParser  )
                             .Parse(inp)  )
        { }
    }
    public class Braces<A> : Parser<A>
    {
        public Braces(Parser<A> betweenParser)
            :
            base(inp => Prim.Between(Tok.Symbol("{"), Tok.Symbol("}"), betweenParser)
                            .Parse(inp))
        { }
    }
    public class Angles<A> : Parser<A>
    {
        public Angles(Parser<A> betweenParser)
            :
            base(inp => Prim.Between(Tok.Symbol("<"), Tok.Symbol(">"), betweenParser)
                            .Parse(inp))
        { }
    }
    public class Brackets<A> : Parser<A>
    {
        public Brackets(Parser<A> betweenParser)
            :
            base(inp => Prim.Between(Tok.Symbol("["), Tok.Symbol("]"), betweenParser)
                            .Parse(inp))
        { }
    }
}
