/*using Monad.Parsec.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec.Expr
{
    public class OperatorToken<A> : Monad.Parsec.Token.Token
    {
        public readonly string Op;
        public readonly A Lhs;
        public readonly A Rhs;
        public readonly OperatorType Type;

        public OperatorToken(string op, A lhs, A rhs, OperatorType type, SrcLoc location)
            :
            base(location)
        {
            Op = op;
            Rhs = rhs;
            Lhs = lhs;
            Type = type;
        }
    }
}
*/