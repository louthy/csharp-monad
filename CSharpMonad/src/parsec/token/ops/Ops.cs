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

using System.Linq;
using Monad.Utility;

namespace Monad.Parsec.Token.Ops
{
    internal static class OpsHelper
    {
        /// <summary>
        /// TODO: Make this pay attention to the case-sensitivity settings
        /// </summary>
        internal static bool IsReservedOp(ImmutableList<ParserChar> name, GeneralLanguageDef languageDef)
        {
            return languageDef.ReservedOpNames.Contains(name.AsString());
        }
    }

    public class ReservedOp : Parser<ReservedOpToken>
    {
        public ReservedOp(string name, GeneralLanguageDef languageDef)
            :
            base(
                inp => Tok.Lexeme(
                        from op in Prim.String(name)
                        from nf in Prim.NotFollowedBy( languageDef.OpLetter )
                                      .Fail("end of " + op.AsString())
                        select new ReservedOpToken(op, inp.First().Location)
                    )
                    .Parse(inp)
            )
        { }
    }

    public class Operator : Parser<OperatorToken>
    {
        public Operator(GeneralLanguageDef languageDef)
            :
            base(
                inp =>
                {
                    var res = Tok.Lexeme(
                        from name in new Oper(languageDef)
                        where !OpsHelper.IsReservedOp(name, languageDef)
                        select new OperatorToken(name, inp.First().Location))
                        .Parse(inp);

                    if (res.IsFaulted)
                        return res;

                    if (res.Value.IsEmpty)
                        return ParserResult.Fail<OperatorToken>("unexpected: reserved operator", inp);

                    return res;
                }
            )
        { }
    }

    public class Oper : Parser<ImmutableList<ParserChar>>
    {
        public Oper(GeneralLanguageDef languageDef)
            :
            base(
                inp => (from c in languageDef.OpStart
                        from cs in Prim.Many(languageDef.OpLetter)
                        select c.Cons(cs))
                       .Fail("operator")
                       .Parse(inp)
            )
        {}
    }

}
