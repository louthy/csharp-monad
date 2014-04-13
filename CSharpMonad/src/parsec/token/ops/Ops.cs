using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec.Token.Ops
{
    internal static class OpsHelper
    {
        /// <summary>
        /// TODO: Make this pay attention to the case-sensitivity settings
        /// </summary>
        internal static bool IsReservedOp(IEnumerable<ParserChar> name, GeneralLanguageDef languageDef)
        {
            return languageDef.ReservedOpNames.Contains(name.AsString());
        }
    }

    public class ReservedOp : Parser<IEnumerable<ReservedOpToken>>
    {
        public ReservedOp(string name, GeneralLanguageDef languageDef)
            :
            base(
                inp => Tok.Lexeme(
                    New.Try(
                        from op in New.String(name)
                        from nf in New.NotFollowedBy( languageDef.OpLetter )
                                      .Fail("end of " + op.AsString())
                        select new ReservedOpToken(op, inp.First().Location)
                    ))
                    .Parse(inp)
            )
        { }
    }

    public class Operator : Parser<IEnumerable<OperatorToken>>
    {
        public Operator(GeneralLanguageDef languageDef)
            :
            base(
                inp =>
                {
                    var res = Tok.Lexeme(
                        New.Try<OperatorToken>(
                            from name in new Oper(languageDef)
                            where !OpsHelper.IsReservedOp(name, languageDef)
                            select new OperatorToken(name, inp.First().Location)
                        ))
                        .Parse(inp);

                    if (res.IsFaulted)
                        return res;

                    if (res.Value.Count() == 0)
                        return ParserResult.Fail<IEnumerable<OperatorToken>>("unexpected: reserved operator", inp);

                    return res;
                }
            )
        { }
    }

    public class Oper : Parser<IEnumerable<ParserChar>>
    {
        public Oper(GeneralLanguageDef languageDef)
            :
            base( 
                inp => (from c in languageDef.OpStart
                        from cs in New.Many(languageDef.OpLetter)
                        select c.Cons(cs))
                       .Fail("operator")
                       .Parse(inp)
            )
        {}
    }

}
