using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec.Token.Bracketing
{
    public class Parens<A> : Parser<A>
    {
        public Parens(Parser<A> betweenParser)
            :
            base( inp => New.Between( Tok.Symbol("("), Tok.Symbol(")"), betweenParser  )
                            .Parse(inp)  )
        { }
    }
    public class Braces<A> : Parser<A>
    {
        public Braces(Parser<A> betweenParser)
            :
            base(inp => New.Between(Tok.Symbol("{"), Tok.Symbol("}"), betweenParser)
                            .Parse(inp))
        { }
    }
    public class Angles<A> : Parser<A>
    {
        public Angles(Parser<A> betweenParser)
            :
            base(inp => New.Between(Tok.Symbol("<"), Tok.Symbol(">"), betweenParser)
                            .Parse(inp))
        { }
    }
    public class Brackets<A> : Parser<A>
    {
        public Brackets(Parser<A> betweenParser)
            :
            base(inp => New.Between(Tok.Symbol("["), Tok.Symbol("]"), betweenParser)
                            .Parse(inp))
        { }
    }
}
