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

using Monad;
using Monad.Utility;

namespace Monad.Parsec.Expr
{
    /// <summary>
    /// Builds a parser given a table of operators and associativities.
    /// Based on the expression builder from Haskell's Parsec library.
    /// http://hackage.haskell.org/package/parsec-3.0.0/docs/src/Text-Parsec-Expr.html
    /// </summary>
    public static class Ex
    {
        public static Parser<A> BuildExpressionParser<A>(
            OperatorTable<A> operators,
            Parser<A> simpleExpr
            )
        {
            return operators.AsEnumerable()
                            .Foldl<IEnumerable<Operator<A>>, Parser<A>>(MakeParser, simpleExpr);
        }

        private static Parser<A> MakeParser<A>(IEnumerable<Operator<A>> ops, Parser<A> term)
        {
            var empty2 = ImmutableList.Empty<Parser<Func<A, A>>>();
            var empty3 = ImmutableList.Empty<Parser<Func<A, A, A>>>();

            return ops.Foldr(
                   SplitOp,
                   Tuple.Create(empty3, empty3, empty3, empty2, empty2)
                )
                .Apply((rassoc, lassoc, nassoc, prefix, postfix) =>
                {
                    var rassocOp = Prim.Choice(rassoc);
                    var lassocOp = Prim.Choice(lassoc);
                    var nassocOp = Prim.Choice(nassoc);
                    var prefixOp = Prim.Choice(prefix).Fail("");
                    var postfixOp = Prim.Choice(postfix).Fail("");

                    Func<string, Choice<Func<A, A, A>>, Parser<A>> ambiguous = (string assoc, Choice<Func<A, A, A>> op) =>
                        Prim.Try<A>(
                            (from o in op
                             from fail in 
                                Prim.Failure<A>( 
                                    ParserError.Create("ambiguous use of a " + assoc + " associative operator", 
                                    new ParserChar(' ').Cons() 
                                ))
                             select fail)
                            );


                    var ambiguousRight = ambiguous("right", rassocOp);
                    var ambiguousLeft = ambiguous("left", lassocOp);
                    var ambiguousNon = ambiguous("non", nassocOp);

                    var postfixP = postfixOp | Prim.Return<Func<A,A>>( a => a );
                    var prefixP = prefixOp | Prim.Return<Func<A, A>>(a => a);

                    Parser<A> termP = from pre in prefixP
                                      from x in term
                                      from post in postfixP
                                      select (post(pre(x)));

                    Func<A, Parser<A>> rassocP1 = null;

                    Func<A, Parser<A>> rassocP = x => (from f in rassocOp
                                                       from y in
                                                           (from z in termP
                                                            from rz in rassocP1(z)
                                                            select rz)
                                                       select f(x, y))
                                                        | ambiguousLeft
                                                        | ambiguousNon;

                    rassocP1 = x => rassocP(x) | Prim.Return(x);

                    Func<A, Parser<A>> lassocP1 = null;

                    Func<A, Parser<A>> lassocP = x => (from f in lassocOp
                                                       from y in termP
                                                       from l in lassocP1(f(x, y))
                                                       select l)
                                                        | ambiguousRight;

                    lassocP1 = x => (from l in lassocP(x)
                                     select l)
                                     | Prim.Return(x);

                    Func<A, Parser<A>> nassocP = x => (from f in nassocOp
                                                       from y in termP
                                                       from r in ambiguousRight
                                                               | ambiguousLeft
                                                               | ambiguousNon
                                                               | Prim.Return(f(x, y))
                                                       select r);

                    return from x in termP
                           from r in (rassocP(x) | lassocP(x) | nassocP(x) | Prim.Return(x)).Fail("operator")
                           select r;
                }
            );
        }

        /// <summary>
        /// Assigns the operator to one of the five buckets in state provided
        /// </summary>
        private static Tuple<
                ImmutableList<Parser<Func<A, A, A>>>,
                ImmutableList<Parser<Func<A, A, A>>>,
                ImmutableList<Parser<Func<A, A, A>>>,
                ImmutableList<Parser<Func<A, A>>>,
                ImmutableList<Parser<Func<A, A>>>
            >
            SplitOp<A>(
            Operator<A> def,
            Tuple<
                ImmutableList<Parser<Func<A, A, A>>>,
                ImmutableList<Parser<Func<A, A, A>>>,
                ImmutableList<Parser<Func<A, A, A>>>,
                ImmutableList<Parser<Func<A, A>>>,
                ImmutableList<Parser<Func<A, A>>>> state)
        {
            // Oh for pattern matching in C#, sigh.
            switch (def.Type)
            {
                case OperatorType.Infix:

                    var infix = def as Infix<A>;
                    var iop = infix.Parser;

                    switch ( infix.Assoc )
                    {
                        case Assoc.None:
                            return Tuple.Create(state.Item1, state.Item2, iop.Cons(state.Item3), state.Item4, state.Item5);
                        case Assoc.Left:
                            return Tuple.Create(state.Item1, iop.Cons(state.Item2), state.Item3, state.Item4, state.Item5);
                        case Assoc.Right:
                            return Tuple.Create(iop.Cons(state.Item1), state.Item2, state.Item3, state.Item4, state.Item5);
                        default:
                            throw new NotSupportedException();
                    }

                case OperatorType.Prefix:
                    var prefix = def as Prefix<A>;
                    var preop = prefix.Parser;

                    return Tuple.Create(state.Item1, state.Item2, state.Item3, preop.Cons(state.Item4), state.Item5);

                case OperatorType.Postfix:
                    var postfix = def as Postfix<A>;
                    var postop = postfix.Parser;

                    return Tuple.Create(state.Item1, state.Item2, state.Item3, state.Item4, postop.Cons(state.Item5));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}