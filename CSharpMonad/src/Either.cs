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
    public class Either<L,R>
    {
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

