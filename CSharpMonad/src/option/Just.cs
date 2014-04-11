using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad
{
    /// <summary>
    /// Just case of the Option<T> monad
    /// </summary>
    public class Just<T> : Option<T>
    {
        private readonly T value;

        public Just(T value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override T Value
        {
            get
            {
                return value;
            }
        }

        public override bool HasValue
        {
            get
            {
                return true;
            }
        }

        public override R Match<R>(Func<R> Just, Func<R> Nothing)
        {
            return Just();
        }

        public override R Match<R>(Func<T, R> Just, Func<R> Nothing)
        {
            return Just(Value);
        }

        public override R Match<R>(Func<R> Just, R Nothing)
        {
            return Just();
        }

        public override R Match<R>(Func<T, R> Just, R Nothing)
        {
            return Just(Value);
        }
    }
}