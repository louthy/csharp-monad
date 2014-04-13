using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec.Token.Idents
{
    internal static class IdentHelper
    {

        /// <summary>
        /// TODO: Make this pay attention to the case-sensitivity settings
        /// </summary>
        internal static Parser<IEnumerable<ParserChar>> CaseString(string str, GeneralLanguageDef languageDef)
        {
            return New.String(str);
        }

        /// <summary>
        /// TODO: Make this pay attention to the case-sensitivity settings
        /// </summary>
        internal static bool IsReservedName(IEnumerable<ParserChar> name, GeneralLanguageDef languageDef)
        {
            return languageDef.ReservedNames.Contains(name.AsString());
        }
    }

    public class Reserved : Parser<IEnumerable<ReservedToken>>
    {
        public Reserved(string name, GeneralLanguageDef languageDef)
            :
            base(
                inp => Tok.Lexeme(
                    New.Try(
                        from cs in IdentHelper.CaseString(name, languageDef)
                        from nf in New.NotFollowedBy( languageDef.IdentLetter )
                                      .Fail("end of " + cs.AsString())
                        select new ReservedToken(cs,inp.First().Location)
                    ))
                    .Parse(inp)
            )
        { }
    }

    public class Identifier : Parser<IEnumerable<IdentifierToken>>
    {
        public Identifier(GeneralLanguageDef languageDef)
            :
            base(
                inp =>
                {
                    var res = Tok.Lexeme(
                        New.Try<IdentifierToken>(
                            from name in new Ident(languageDef)
                            where !IdentHelper.IsReservedName(name, languageDef)
                            select new IdentifierToken(name, inp.First().Location)
                        ))
                        .Parse(inp);

                    if (res.IsFaulted)
                        return res;

                    if (res.Value.Count() == 0)
                        return ParserResult.Fail<IEnumerable<IdentifierToken>>("unexpected: reserved word",inp);

                    return res;
                }
            )
        { }
    }

    public class Ident : Parser<IEnumerable<ParserChar>>
    {
        public Ident(GeneralLanguageDef languageDef)
            :
            base( 
                inp => (from c in languageDef.IdentStart
                        from cs in New.Many(languageDef.IdentLetter)
                        select c.Cons(cs))
                       .Fail("identifier")
                       .Parse(inp)
            )
        {}
    }

}
