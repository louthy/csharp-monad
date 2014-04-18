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
using System.Collections.Generic;

namespace Monad
{
    /*
    /// <summary>
    /// Either constructor methods
    /// </summary>
    public class Either
    {
        /// <summary>
        /// Construct an Either Left monad
        /// </summary>
        public static Either<R, L> Left<R, L>(L left)
        {
            return new Either<R, L>(left);
        }

        /// <summary>
        /// Construct an Either Right monad
        /// </summary>
        public static Either<R, L> Right<R, L>(R right)
        {
            return new Either<R, L>(right);
        }

        /// <summary>
        /// Monadic zero
        /// </summary>
        public static Either<R, L> Mempty<R, L>()
        {
            return new Either<R, L>(default(R));
        }
    }

    /// <summary>
    /// The Either monad represents values with two possibilities: a value of Left or Right
    /// Either is sometimes used to represent a value which is either correct or an error, 
    /// by convention, 'Left' is used to hold an error value 'Right' is used to hold a 
    /// correct value.
    /// So you can see that Either has a very close relationship to the Error monad.  However,
    /// the Either monad won't capture exceptions.  Either would primarily be used for 
    /// known error values rather than exceptional ones.
    /// Once the Either monad is in the Left state it cancels the monad bind function and 
    /// returns immediately.
    /// </summary>
    /// <typeparam name="L"></typeparam>
    /// <typeparam name="R"></typeparam>
    public class Either<R, L> : IEquatable<Either<R, L>>
    {
        static readonly string TypeOfR = typeof(R).ToString();
        static readonly bool IsAppendable = typeof(IAppendable<R>).IsAssignableFrom(typeof(R));

        readonly L left;
        readonly R right;

        /// <summary>
        /// Returns true if the monad object is in the Left state
        /// </summary>
        public readonly bool IsLeft;

        /// <summary>
        /// Left constructor
        /// </summary>
        internal Either(L left)
        {
            IsLeft = true;
            this.left = left;
        }

        /// <summary>
        /// Right constructor
        /// </summary>
        internal Either(R right)
        {
            IsLeft = false;
            this.right = right;
        }

        /// <summary>
        /// Returns true if the monad object is in the Right state
        /// </summary>
        public bool IsRight
        {
            get
            {
                return !IsLeft;
            }
        }

        /// <summary>
        /// Get the Left value
        /// NOTE: This throws an InvalidOperationException if the object is in the 
        /// Right state
        /// </summary>
        public L Left
        {
            get
            {
                if (!IsLeft)
                    throw new InvalidOperationException("Not in the left state");
                return left;
            }
        }

        /// <summary>
        /// Get the Right value
        /// NOTE: This throws an InvalidOperationException if the object is in the 
        /// Left state
        /// </summary>
        public R Right
        {
            get
            {
                if (!IsRight)
                    throw new InvalidOperationException("Not in the right state");
                return right;
            }
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// </summary>
        /// <param name="Right">Action to perform if the monad is in the Right state</param>
        /// <param name="Left">Action to perform if the monad is in the Left state</param>
        /// <returns>T</returns>
        public T Match<T>(Func<R,T> Right, Func<L,T> Left)
        {
            return IsLeft
                ? Left(this.Left)
                : Right(this.Right);
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// NOTE: This throws an InvalidOperationException if the object is in the 
        /// Left state
        /// </summary>
        /// <param name="right">Action to perform if the monad is in the Right state</param>
        /// <returns>T</returns>
        public T MatchRight<T>(Func<R, T> right)
        {
            return right(this.Right);
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// NOTE: This throws an InvalidOperationException if the object is in the 
        /// Right state
        /// </summary>
        /// <param name="left">Action to perform if the monad is in the Left state</param>
        /// <returns>T</returns>
        public T MatchLeft<T>(Func<L, T> left)
        {
            return left(this.Left);
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// Returns the defaultValue if the monad is in the Left state
        /// </summary>
        /// <param name="right">Action to perform if the monad is in the Right state</param>
        /// <returns>T</returns>
        public T MatchRight<T>(Func<R, T> right, T defaultValue)
        {
            if (IsLeft) 
                return defaultValue;
            return right(this.Right);
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// Returns the defaultValue if the monad is in the Right state
        /// </summary>
        /// <param name="left">Action to perform if the monad is in the Left state</param>
        /// <returns>T</returns>
        public T MatchLeft<T>(Func<L, T> left, T defaultValue)
        {
            if (IsRight)
                return defaultValue;
            return left(this.Left);
        }


        /// <summary>
        /// Pattern matching method for a branching expression
        /// </summary>
        /// <param name="Right">Action to perform if the monad is in the Right state</param>
        /// <param name="Left">Action to perform if the monad is in the Left state</param>
        /// <returns>Unit</returns>
        public Unit Match(Action<R> Right, Action<L> Left)
        {
            return Unit.Return( () =>
            {
                if (IsLeft)
                    Left(this.Left);
                else
                    Right(this.Right);
            });
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// NOTE: This throws an InvalidOperationException if the object is in the 
        /// Left state
        /// </summary>
        /// <param name="right">Action to perform if the monad is in the Right state</param>
        /// <returns>Unit</returns>
        public Unit MatchRight(Action<R> right)
        {
            return Unit.Return(() => right(this.Right));
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// NOTE: This throws an InvalidOperationException if the object is in the 
        /// Right state
        /// </summary>
        /// <param name="left">Action to perform if the monad is in the Left state</param>
        /// <returns>Unit</returns>
        public Unit MatchLeft(Action<L> left)
        {
            return Unit.Return(() => left(this.Left));
        }

        /// <summary>
        /// Monadic append
        /// If the left-hand side or right-hand side are in a Left state, then Left propogates
        /// </summary>
        public static Either<R, L> operator +(Either<R, L> lhs, Either<R, L> rhs)
        {
            return lhs.Mappend(rhs);
        }

        /// <summary>
        /// Left coalescing operator
        /// Returns the left-hand operand if the operand is not Left; otherwise it returns the right hand operand.
        /// In other words it returns the first valid option in the operand sequence.
        /// </summary>
        public static Either<R, L> operator |(Either<R, L> lhs, Either<R, L> rhs)
        {
            return lhs.IsRight
                ? lhs
                : rhs;
        }

        /// <summary>
        /// Returns the right-hand side if the left-hand and right-hand side are not Left.
        /// In order words every operand must hold a value for the result to be Right.
        /// In the case where all operands return Left, then the last operand will provide
        /// its value.
        /// </summary>
        public static Either<R, L> operator &(Either<R, L> lhs, Either<R, L> rhs)
        {
            return lhs.IsRight && rhs.IsRight
                ? rhs
                : lhs.IsRight
                    ? rhs
                    : lhs;
        }

        /// <summary>
        /// Monadic append
        /// If the left-hand side or right-hand side are in a Left state, then Left propagates
        /// </summary>
        public virtual Either<R, L> Mappend(Either<R, L> rhs)
        {
            if (IsLeft)
            {
                return this;
            }
            else
            {
                if (rhs.IsLeft)
                {
                    return rhs.Left;
                }
                else
                {
                    if (IsAppendable)
                    {
                        var lhs = this.Right as IAppendable<R>;
                        return new Either<R, L>(lhs.Append(rhs.Right));
                    }
                    else
                    {
                        // TODO: Consider replacing this with a static Reflection.Emit which does this job efficiently.
                        switch (TypeOfR)
                        {
                            case "System.Int64":
                                return new Either<R, L>((R)Convert.ChangeType((Convert.ToInt64(right) + Convert.ToInt64(rhs.right)), typeof(R)));
                            case "System.UInt64":
                                return new Either<R, L>((R)Convert.ChangeType((Convert.ToUInt64(right) + Convert.ToUInt64(rhs.right)), typeof(R)));
                            case "System.Int32":
                                return new Either<R, L>((R)Convert.ChangeType((Convert.ToInt32(right) + Convert.ToInt32(rhs.right)), typeof(R)));
                            case "System.UInt32":
                                return new Either<R, L>((R)Convert.ChangeType((Convert.ToUInt32(right) + Convert.ToUInt32(rhs.right)), typeof(R)));
                            case "System.Int16":
                                return new Either<R, L>((R)Convert.ChangeType((Convert.ToInt16(right) + Convert.ToInt16(rhs.right)), typeof(R)));
                            case "System.UInt16":
                                return new Either<R, L>((R)Convert.ChangeType((Convert.ToUInt16(right) + Convert.ToUInt16(rhs.right)), typeof(R)));
                            case "System.Decimal":
                                return new Either<R, L>((R)Convert.ChangeType((Convert.ToDecimal(right) + Convert.ToDecimal(rhs.right)), typeof(R)));
                            case "System.Double":
                                return new Either<R, L>((R)Convert.ChangeType((Convert.ToDouble(right) + Convert.ToDouble(rhs.right)), typeof(R)));
                            case "System.Single":
                                return new Either<R, L>((R)Convert.ChangeType((Convert.ToSingle(right) + Convert.ToSingle(rhs.right)), typeof(R)));
                            case "System.Char":
                                return new Either<R, L>((R)Convert.ChangeType((Convert.ToChar(right) + Convert.ToChar(rhs.right)), typeof(R)));
                            case "System.Byte":
                                return new Either<R, L>((R)Convert.ChangeType((Convert.ToByte(right) + Convert.ToByte(rhs.right)), typeof(R)));
                            case "System.String":
                                return new Either<R, L>((R)Convert.ChangeType((Convert.ToString(right) + Convert.ToString(rhs.right)), typeof(R)));
                            default:
                                throw new InvalidOperationException("Type " + typeof(R).Name + " is not appendable.  Consider implementing the IAppendable interface.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts the Either to an enumerable of R
        /// </summary>
        /// <returns>
        /// Right: A list with one R in
        /// Left: An empty list
        /// </returns>
        public IEnumerable<R> AsEnumerable()
        {
            if (IsRight)
                yield return Right;
            else
                yield break;
        }

        /// <summary>
        /// Converts the Either to an infinite enumerable
        /// </summary>
        /// <returns>
        /// Just: An infinite list of R
        /// Nothing: An empty list
        /// </returns>
        public IEnumerable<R> AsEnumerableInfinte()
        {
            if (IsRight)
                while (true) yield return Right;
            else
                yield break;
        }

        /// <summary>
        /// Monadic equality
        /// </summary>
        public static bool operator ==(Either<R, L> lhs, Either<R, L> rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Monadic equality
        /// </summary>
        public static bool operator !=(Either<R, L> lhs, Either<R, L> rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// Monadic equality
        /// </summary>
        public static bool operator ==(Either<R, L> lhs, L rhs)
        {
            return lhs.Equals(new Either<R, L>(rhs));
        }

        /// <summary>
        /// Monadic equality
        /// </summary>
        public static bool operator !=(Either<R, L> lhs, L rhs)
        {
            return !lhs.Equals(new Either<R, L>(rhs));
        }

        /// <summary>
        /// Monadic equality
        /// </summary>
        public static bool operator ==(Either<R, L> lhs, R rhs)
        {
            return lhs.Equals(new Either<R, L>(rhs));
        }

        /// <summary>
        /// Monadic equality
        /// </summary>
        public static bool operator !=(Either<R, L> lhs, R rhs)
        {
            return !lhs.Equals(new Either<R, L>(rhs));
        }

        /// <summary>
        /// Implicit left operator conversion
        /// </summary>
        public static implicit operator Either<R, L>(L left)
        {
            return new Either<R, L>(left);
        }

        /// <summary>
        /// Implicit right operator conversion
        /// </summary>
        public static implicit operator Either<R, L>(R right)
        {
            return new Either<R, L>(right);
        }

        public override int GetHashCode()
        {
            return IsLeft
                ? Left == null ? 0 : Left.GetHashCode()
                : Right == null ? 0 : Right.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                if (obj is Either<R, L>)
                {
                    var rhs = (Either<R, L>)obj;
                    return IsRight && rhs.IsRight
                        ? Right.Equals(rhs.Right)
                        : IsLeft && rhs.IsLeft
                            ? true
                            : false;
                }
                else if (obj is R)
                {
                    var rhs = (R)obj;
                    return IsRight
                        ? Right.Equals(rhs)
                        : false;
                }
                else if (obj is L)
                {
                    var rhs = (L)obj;
                    return IsLeft
                        ? Left.Equals(rhs)
                        : false;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool Equals(Either<R, L> rhs)
        {
            return Equals((object)rhs);
        }

        public bool Equals(L rhs)
        {
            return Equals((object)rhs);
        }

        public bool Equals(R rhs)
        {
            return Equals((object)rhs);
        }
    }

    /// <summary>
    /// Either extension methods
    /// </summary>
    public static class EitherExt
    {
        /// <summary>
        /// Select
        /// </summary>
        public static Either<UR, L> Select<TR, UR, L>(
            this Either<TR, L> self, 
            Func<TR, UR> selector)
        {
            if (self.IsLeft) 
                return Either.Left<UR,L>( self.Left );

            return Either.Right<UR,L>( selector( self.Right ) );
        }

        /// <summary>
        /// SelectMany
        /// </summary>
        public static Either<VR, L> SelectMany<TR, UR, VR, L>(
            this Either<TR, L> self, 
            Func<TR, Either<UR, L>> selector, 
            Func<TR,UR,VR> projector)
        {
            if (self.IsLeft) 
                return Either.Left<VR,L>(self.Left);

            var res = selector(self.Right);
            if (res.IsLeft) 
                return Either.Left<VR,L>(res.Left);

            return Either.Right<VR,L>(projector(self.Right, res.Right));
        }

        /// <summary>
        /// Mconcat
        /// </summary>
        public static Either<R, L> Mconcat<R, L>(this IEnumerable<Either<R, L>> ms)
        {
            var value = ms.Head();

            foreach (var m in ms.Tail())
            {
                if (value.IsLeft)
                    return value;

                value = value.Mappend(m);
            }
            return value;
        }
    }*/
}

