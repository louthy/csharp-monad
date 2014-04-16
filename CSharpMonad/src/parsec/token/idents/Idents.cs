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

namespace Monad.Parsec.Token.Idents
{
    internal static class IdentHelper
    {

        /// <summary>
        /// TODO: Make this pay attention to the case-sensitivity settings
        /// </summary>
        internal static Parser<IEnumerable<ParserChar>> CaseString(string str, GeneralLanguageDef languageDef)
        {
            return Gen.String(str);
        }

        /// <summary>
        /// TODO: Make this pay attention to the case-sensitivity settings
        /// </summary>
        internal static bool IsReservedName(IEnumerable<ParserChar> name, GeneralLanguageDef languageDef)
        {
            return languageDef.ReservedNames.Contains(name.AsString());
        }
    }

    public class Reserved : Parser<ReservedToken>
    {
        public Reserved(string name, GeneralLanguageDef languageDef)
            :
            base(
                inp => Tok.Lexeme(
                        from cs in IdentHelper.CaseString(name, languageDef)
                        from nf in Gen.NotFollowedBy( languageDef.IdentLetter )
                                      .Fail("end of " + cs.AsString())
                        select new ReservedToken(cs,inp.First().Location)
                    )
                    .Parse(inp)
            )
        { }
    }

    public class Identifier : Parser<IdentifierToken>
    {
        public Identifier(GeneralLanguageDef languageDef)
            :
            base(
                inp =>
                {
                    var res = Tok.Lexeme(
                            from name in new Ident(languageDef)
                            where !IdentHelper.IsReservedName(name, languageDef)
                            select new IdentifierToken(name, inp.First().Location)
                        )
                        .Parse(inp);

                    if (res.IsFaulted)
                        return res;

                    if (res.Value.IsEmpty())
                        return ParserResult.Fail<IdentifierToken>("unexpected: reserved word",inp);

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
                inp =>(from chs in languageDef.IdentStart.Mconcat( Gen.Many(languageDef.IdentLetter) )
                       select chs)
                      .Fail("identifier")
                      .Parse(inp)
            )
        {}
    }

}
