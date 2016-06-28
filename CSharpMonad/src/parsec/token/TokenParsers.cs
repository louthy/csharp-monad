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

using Monad.Utility;
using System;

namespace Monad.Parsec.Token
{
    public class Symbol : Parser<SymbolToken>
    {
        public Symbol(string name)
            :
            base(
                inp => (from sym in Tok.Lexeme<ImmutableList<ParserChar>>(Prim.String(name))
                        select new SymbolToken(sym))
                       .Parse(inp)
            )
        {
        }
    }

    public class Lexeme<A> : Parser<A>
    {
        public Lexeme(Parser<A> p)
            :
            base(
                inp =>
                    (from x in p
                     from w in Prim.WhiteSpace()
                     select x)
                    .Parse(inp)
            )
        {
        }
    }

    public class OneLineComment : Parser<Unit>
    {
        public OneLineComment(LanguageDef def)
            :
            base(
                inp => (from t in Prim.String(def.CommentLine)
                        from d in Prim.Many(Prim.Satisfy(ch => ch != '\n', "anything but a newline"))
                        select Unit.Default)
                       .Parse(inp)
            )
        {
        }
    }

    public class MultiLineComment : Parser<Unit>
    {
        public MultiLineComment(LanguageDef def)
            :
            base(
                inp => (from open in Prim.Try(Prim.String(def.CommentStart))
                        from incom in
                            def.NestedComments
                                ? new InCommentMulti(def) as Parser<Unit>
                                : new InCommentSingle(def) as Parser<Unit>
                        select incom)
                       .Parse(inp)
            )
        {
        }
    }

    public class InCommentMulti : Parser<Unit>
    {
        public InCommentMulti(LanguageDef def)
            :
            base(
                inp =>
                {
                    int depth = 1;
                    while (depth > 0)
                    {
                        var res = Prim.String(def.CommentEnd).Parse(inp);
                        if (!res.IsFaulted)
                        {
                            depth--;
                            inp = res.Value.Head().Item2;
                            continue;
                        }

                        res = Prim.String(def.CommentStart).Parse(inp);
                        if (!res.IsFaulted)
                        {
                            depth++;
                            inp = res.Value.Head().Item2;
                            continue;
                        }

                        var resU = Prim.SkipMany(Prim.NoneOf(def.CommentStartEndDistinctChars.Value)).Parse(inp);
                        if (resU.Value.Head().Item2.IsEmpty)
                        {
                            return Prim.Failure<Unit>(ParserError.Create("end of comment", inp)).Parse(inp);
                        }
                        inp = resU.Value.Head().Item2;
                    }
                    return Prim.Return<Unit>(Unit.Default).Parse(inp);
                })
        {
        }
    }

    public class InCommentSingle : Parser<Unit>
    {
        public InCommentSingle(LanguageDef def)
            :
        base(
            inp =>
            {
                while (true)
                {
                    var res = Prim.String(def.CommentEnd).Parse(inp);
                    if (!res.IsFaulted)
                    {
                        return Prim.Return<Unit>(Unit.Default).Parse(res.Value.Head().Item2);
                    }

                    var resU = Prim.SkipMany(Prim.NoneOf(def.CommentStartEndDistinctChars.Value)).Parse(inp);
                    if (resU.Value.Head().Item2.IsEmpty)
                    {
                        return Prim.Failure<Unit>(ParserError.Create("end of comment", inp)).Parse(inp);
                    }
                    inp = resU.Value.Head().Item2;
                }
            })
        {
        }
    }

    public class WhiteSpace : Parser<Unit>
    {
        public WhiteSpace(LanguageDef def)
            :
            base(
                inp =>
                {
                    var simpleSpace = Tok.SimpleSpace();

                    if (String.IsNullOrEmpty(def.CommentLine) && String.IsNullOrEmpty(def.CommentStart))
                    {
                        return Prim.SkipMany(
                                simpleSpace.Fail("")
                            ).Parse(inp);
                    }
                    else if (String.IsNullOrEmpty(def.CommentLine))
                    {
                        return Prim.SkipMany<Unit>(
                                simpleSpace | Tok.MultiLineComment(def).Fail("")
                            ).Parse(inp);
                    }
                    else if (String.IsNullOrEmpty(def.CommentStart))
                    {
                        return Prim.SkipMany<Unit>(
                                simpleSpace | Tok.OneLineComment(def).Fail("")
                            )
                            .Parse(inp);
                    }
                    else
                    {
                        return Prim.SkipMany<Unit>(
                                simpleSpace | Tok.OneLineComment(def) | Tok.MultiLineComment(def).Fail("")
                            )
                            .Parse(inp);
                    }
                }
            )
        { }
    }
}