using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec.Token
{
    public class Symbol : Parser<IEnumerable<ParserChar>>
    {
        public Symbol(string name)
            :
            base(
                inp => Tok.Lexeme<IEnumerable<ParserChar>>(New.String(name))
                          .Parse(inp)
            )
        { }
    }

    public class Lexeme<A> : Parser<A>
    {
        public Lexeme(Parser<A> p)
            :
            base( 
                inp=>
                    (from    x in p
                     from    w in New.WhiteSpace()
                     select  x)
                    .Parse(inp)
            )
        { }
    }

    public class OneLineComment : Parser<Unit>
    {
        public OneLineComment(string tag)
            :
            base(
                inp => (from c in
                            New.Try(
                                (from t in New.String(tag)
                                 from d in New.Many(New.Satisfy(ch => ch != '\n',"anything but a newline"))
                                 select d))
                        select Unit.Return())
                       .Parse(inp)
            )
        { }
    }


    /*
    public class ReservedOp : Parser<IEnumerable<ParserChar>>
    {
        public ReservedOp(string op)
            :
            base(
                inp => New.Try()
            )
        {
        }
    }*/
}
