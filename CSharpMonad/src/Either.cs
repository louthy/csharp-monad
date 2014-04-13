using System;

namespace Monad
{
    /*    

        First pass at an Either monad, it's pretty crude and untested at the moment, so use with caution

    */

    public class Either
    {
        public static Either<L,R> Left<L,R>(L left)
        {
            return new Either<L,R>(left);
        }
        public static Either<L,R> Right<L,R>(R right)
        {
            return new Either<L,R>(right);
        }
    }

    public class Either<L,R>
    {
        public readonly object Value;
        public readonly bool IsLeft;

        public Either(L left)
        {
            IsLeft = true;
            Value = left;
        }

        public Either(R right)
        {
            IsLeft = false;
            Value = right;
        }

        public bool IsRight
        {
            get
            {
                return !IsLeft;
            }
        }

        public L GetLeft()
        {
            if (!IsLeft)
                throw new InvalidOperationException("Not in a left state");
            return (L)Value;
        }

        public R GetRight()
        {
            if (!IsRight)
                throw new InvalidOperationException("Not in a right state");
            return (R)Value;
        }
    }

    public static class EitherExt
    {
        public static Either<L, UR> Select<TR, UR, L>(
            this Either<L, TR> self, 
            Func<TR, UR> selector)
        {
            if (self.IsLeft) return Either.Left<L,UR>(
                (L)self.Value
            );

            var val = (TR)self.Value;
            return Either.Right<L,UR>( selector( val ) );
        }

        public static Either<L, VR> SelectMany<TR, UR, VR, L>(
            this Either<L, TR> self, 
            Func<TR, Either<L, UR>> selector, 
            Func<TR,UR,VR> projector)
        {
            if (self.IsLeft) return Either.Left<L,VR>(
                (L)self.Value
            );

            var val = (TR)self.Value;
            var res2 = selector( val );
            if (res2.IsLeft) return Either.Left<L,VR>(
                (L)res2.Value
            );

            var val2 = (UR)res2.Value;

            return Either.Right<L,VR>(projector(val, val2));
        }
    }
}

