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
            /*
             * Currently struggling to parse the haskell equivalent.  I'll make it work once I 
             * stop my head exploding.
             * 
             * http://hackage.haskell.org/package/parsec-3.0.0/docs/src/Text-Parsec-Expr.html
             * 
             */
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

                 Func<string, Choice<OperatorDef<A>>, Parser<OperatorDef<A>>> ambiguous = (string assoc, Choice<OperatorDef<A>> op) =>
                     from p in
                         New.Try<OperatorDef<A>>(
                             (from o in op
                              select o)
                             .Fail("ambiguous use of a " + assoc + " associative operator")
                             )
                     select p;


                 var ambiguousRight = from a in ambiguous("right", rassocOp) select default(A); // Not sure about what the result should be
                 var ambiguousLeft = from a in ambiguous("left", lassocOp) select default(A); // Not sure about what the result should be
                 var ambiguousNon = from a in ambiguous("non", nassocOp) select default(A); // Not sure about what the result should be

                 var postfixP = postfixOp.Or(New.Return(OperatorDef.Id<A>()));
                 var prefixP = prefixOp.Or(New.Return(OperatorDef.Id<A>()));

                 Parser<A> termP = from pre in prefixP
                                   from x in term
                                   from post in postfixP
                                   select (post.UnaryFn(pre.UnaryFn(x)));

                 Func<A,Parser<A>> rassocP1 = null;

                 Func<A,Parser<A>> rassocP = x => (from f in rassocOp
                                                   from y in (from z in termP
                                                              from rz in rassocP1(z)
                                                              select rz)
                                                   select f.BinaryFn(x,y))   // f(x,y)
                                                  .Or(ambiguousLeft)
                                                  .Or(ambiguousNon);

                 rassocP1 = x => rassocP(x).Or(New.Return(x));

                 Func<A, Parser<A>> lassocP1 = null;

                 Func<A, Parser<A>> lassocP = x => (from f in lassocOp
                                                    from y in termP
                                                    from l in lassocP1(f.BinaryFn(x, y))
                                                    select l)
                                                   .Or(ambiguousRight);  // Not sure about this

                 lassocP1 = x => (from l in lassocP(x)
                                  select l)
                                 .Or(New.Return(x));

                 Func<A, Parser<A>> nassocP = x => (from f in nassocOp
                                                    from y in termP
                                                    from r in ambiguousRight
                                                             .Or(ambiguousLeft)
                                                             .Or(ambiguousNon)
                                                             .Or(New.Return(f.BinaryFn(x, y)))
                                                    select r);

                 return from x in termP
                        from r in rassocP(x).Or(lassocP(x).Or(nassocP(x).Or(New.Return(x)))).Fail("operator")
                        select r;
             }
         );
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
            switch (op.Def.Type)
            {
                case OperatorType.Infix:
                    switch (op.Def.Assoc)
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
