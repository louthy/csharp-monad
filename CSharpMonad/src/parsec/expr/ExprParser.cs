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

using Monad;
using Monad.Parsec;
using Monad.Parsec.Token;
using Monad.Parsec.Expr;

namespace Monad.Parsec.Expr
{
    public static class Ex
    {
        public static Parser<A> BuildExpressionParser<A>(
            OperatorTable<A> operators,
            Parser<A> simpleExpr
            )
        {
            return operators.AsEnumerable()
                            .Foldl<IEnumerable<Operator<A>>, Parser<A>>(simpleExpr, MakeParser);
        }

        private static Parser<A> MakeParser<A>(IEnumerable<Operator<A>> ops, Parser<A> term)
        {
            throw new NotImplementedException();
            /*
             * Currently struggling to parse the haskell equivalent.  I'll make it work once I 
             * stop my head exploding.
             * 
             * http://hackage.haskell.org/package/parsec-3.0.0/docs/src/Text-Parsec-Expr.html
             * 
             *

            return ops.Foldr(
                Tuple.Create(
                    new Operator<A>[0].AsEnumerable(),
                    new Operator<A>[0].AsEnumerable(),
                    new Operator<A>[0].AsEnumerable(),
                    new Operator<A>[0].AsEnumerable(),
                    new Operator<A>[0].AsEnumerable()
                    ),
                    SplitOp).Apply((rassoc, lassoc, nassoc, prefix, postfix) =>
                {
                    var rassocOp = New.Choice(rassoc);
                    var lassocOp = New.Choice(lassoc);
                    var nassocOp = New.Choice(nassoc);
                    var prefixOp = New.Choice(prefix).Fail("");
                    var postfixOp = New.Choice(postfix).Fail("");

                    Func<string, Choice<A>, Parser<IEnumerable<A>>> ambiguous = (string assoc, Choice<A> op) =>
                        from p in
                            New.Try<A>(
                                (from o in op
                                 select o)
                                .Fail("ambiguous use of a " + assoc + " associative operator")
                                )
                        select p;


                    var ambiguousRight = ambiguous("right", rassocOp);
                    var ambiguousLeft = ambiguous("left", lassocOp);
                    var ambiguousNon = ambiguous("non", nassocOp);

                    var postfixP = postfixOp;//.Or(New.Return(id));
                    var prefixP = prefixOp;//.Or(New.Return(id));

                    Parser<A> termP = from pre in prefixP
                                      from x in term
                                      from post in postfixP
                                      select (post(pre(x)));

                    Func<Parser<A>,Parser<A>> rassocP = a => from f in rassocOp
                                                             from y in (from z in termP
                                                                        from rz in rassocP1(z)
                                                                        select rz)
                                                             select f(x,y);


                    //return new Parser<A>();
                }
            );*/
        }



        /// <summary>
        /// Assigns the operator to one of the five buckets in state provided
        /// </summary>
        private static Tuple<
            IEnumerable<Operator<A>>,
            IEnumerable<Operator<A>>,
            IEnumerable<Operator<A>>,
            IEnumerable<Operator<A>>,
            IEnumerable<Operator<A>>>
            SplitOp<A>(
            Operator<A> op,
            Tuple<IEnumerable<Operator<A>>,
            IEnumerable<Operator<A>>,
            IEnumerable<Operator<A>>,
            IEnumerable<Operator<A>>,
            IEnumerable<Operator<A>>> state)
        {
            switch (op.OpType)
            {
                case OperatorType.Infix:
                    switch ((op as Infix<A>).Assoc)
                    {
                        case Assoc.None:
                            return Tuple.Create(state.Item1, state.Item2, op.Cons(state.Item3), state.Item4, state.Item5);
                        case Assoc.Left:
                            return Tuple.Create(state.Item1, op.Cons(state.Item2), state.Item3, state.Item4, state.Item5);
                        case Assoc.Right:
                            return Tuple.Create(op.Cons(state.Item1), state.Item2, state.Item3, state.Item4, state.Item5);
                        default:
                            throw new NotSupportedException();
                    }

                case OperatorType.Prefix:
                    return Tuple.Create(state.Item1, state.Item2, state.Item3, op.Cons(state.Item4), state.Item5);

                case OperatorType.Postfix:
                    return Tuple.Create(state.Item1, state.Item2, state.Item3, state.Item4, op.Cons(state.Item5));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
