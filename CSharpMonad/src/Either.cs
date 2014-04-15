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

namespace Monad
{
    /// <summary>
    /// Either constructor methods
    /// </summary>
    public class Either
    {
        /// <summary>
        /// Construct an Either Left monad
        /// </summary>
        public static Either<L,R> Left<L,R>(L left)
        {
            return new Either<L,R>(left);
        }

        /// <summary>
        /// Construct an Either Right monad
        /// </summary>
        public static Either<L, R> Right<L, R>(R right)
        {
            return new Either<L,R>(right);
        }

        /// <summary>
        /// Monadic zero
        /// </summary>
        public static Either<L, R> Mempty<L, R>()
        {
            return Either<L, R>.Mempty();
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
    public class Either<L,R> : IEquatable<Either<L,R>>
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
        public static Either<L, R> operator +(Either<L, R> lhs, Either<L, R> rhs)
        {
            return lhs.Mappend(rhs);
        }

        /// <summary>
        /// Monadic append
        /// If the left-hand side or right-hand side are in a Left state, then Left propagates
        /// </summary>
        public virtual Either<L, R> Mappend(Either<L, R> rhs)
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
                        return new Either<L, R>(lhs.Append(rhs.Right));
                    }
                    else
                    {
                        // TODO: Consider replacing this with a Reflection.Emit which does this job efficiently.
                        switch (TypeOfR)
                        {
                            case "System.Int64":
                                return new Either<L, R>((R)Convert.ChangeType((Convert.ToInt64(right) + Convert.ToInt64(rhs.right)), typeof(R)));
                            case "System.UInt64":
                                return new Either<L, R>((R)Convert.ChangeType((Convert.ToUInt64(right) + Convert.ToUInt64(rhs.right)), typeof(R)));
                            case "System.Int32":
                                return new Either<L, R>((R)Convert.ChangeType((Convert.ToInt32(right) + Convert.ToInt32(rhs.right)), typeof(R)));
                            case "System.UInt32":
                                return new Either<L, R>((R)Convert.ChangeType((Convert.ToUInt32(right) + Convert.ToUInt32(rhs.right)), typeof(R)));
                            case "System.Int16":
                                return new Either<L, R>((R)Convert.ChangeType((Convert.ToInt16(right) + Convert.ToInt16(rhs.right)), typeof(R)));
                            case "System.UInt16":
                                return new Either<L, R>((R)Convert.ChangeType((Convert.ToUInt16(right) + Convert.ToUInt16(rhs.right)), typeof(R)));
                            case "System.Decimal":
                                return new Either<L, R>((R)Convert.ChangeType((Convert.ToDecimal(right) + Convert.ToDecimal(rhs.right)), typeof(R)));
                            case "System.Double":
                                return new Either<L, R>((R)Convert.ChangeType((Convert.ToDouble(right) + Convert.ToDouble(rhs.right)), typeof(R)));
                            case "System.Single":
                                return new Either<L, R>((R)Convert.ChangeType((Convert.ToSingle(right) + Convert.ToSingle(rhs.right)), typeof(R)));
                            case "System.Char":
                                return new Either<L, R>((R)Convert.ChangeType((Convert.ToChar(right) + Convert.ToChar(rhs.right)), typeof(R)));
                            case "System.Byte":
                                return new Either<L, R>((R)Convert.ChangeType((Convert.ToByte(right) + Convert.ToByte(rhs.right)), typeof(R)));
                            case "System.String":
                                return new Either<L, R>((R)Convert.ChangeType((Convert.ToString(right) + Convert.ToString(rhs.right)), typeof(R)));
                            default:
                                throw new InvalidOperationException("Type " + typeof(R).Name + " is not appendable.  Consider implementing the IAppendable interface.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Monadic zero
        /// </summary>
        public static Either<L,R> Mempty()
        {
            return new Either<L, R>(default(R));
        }

        /// <summary>
        /// Monadic equality
        /// </summary>
        public static bool operator ==(Either<L, R> lhs, Either<L, R> rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Monadic equality
        /// </summary>
        public static bool operator !=(Either<L, R> lhs, Either<L, R> rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// Monadic equality
        /// </summary>
        public static bool operator ==(Either<L, R> lhs, L rhs)
        {
            return lhs.Equals(new Either<L,R>(rhs));
        }

        /// <summary>
        /// Monadic equality
        /// </summary>
        public static bool operator !=(Either<L, R> lhs, L rhs)
        {
            return !lhs.Equals(new Either<L, R>(rhs));
        }

        /// <summary>
        /// Monadic equality
        /// </summary>
        public static bool operator ==(Either<L, R> lhs, R rhs)
        {
            return lhs.Equals(new Either<L, R>(rhs));
        }

        /// <summary>
        /// Monadic equality
        /// </summary>
        public static bool operator !=(Either<L, R> lhs, R rhs)
        {
            return !lhs.Equals(new Either<L, R>(rhs));
        }

        /// <summary>
        /// Implicit left operator conversion
        /// </summary>
        public static implicit operator Either<L, R>(L left)
        {
            return new Either<L, R>(left);
        }

        /// <summary>
        /// Implicit right operator conversion
        /// </summary>
        public static implicit operator Either<L, R>(R right)
        {
            return new Either<L, R>(right);
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
                if (obj is Either<L,R>)
                {
                    var rhs = (Either<L,R>)obj;
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

        public bool Equals(Either<L,R> rhs)
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
        public static Either<L, UR> Select<TR, UR, L>(
            this Either<L, TR> self, 
            Func<TR, UR> selector)
        {
            if (self.IsLeft) 
                return Either.Left<L,UR>( self.Left );

            return Either.Right<L,UR>( selector( self.Right ) );
        }

        /// <summary>
        /// SelectMany
        /// </summary>
        public static Either<L, VR> SelectMany<TR, UR, VR, L>(
            this Either<L, TR> self, 
            Func<TR, Either<L, UR>> selector, 
            Func<TR,UR,VR> projector)
        {
            if (self.IsLeft) 
                return Either.Left<L,VR>(self.Left);

            var res = selector(self.Right);
            if (res.IsLeft) 
                return Either.Left<L,VR>(res.Left);

            return Either.Right<L, VR>(projector(self.Right, res.Right));
        }
    }
}

