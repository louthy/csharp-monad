using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad
{
	/// <summary>
	/// Nothing case of the Option<T> monad
	/// </summary>
	public class Nothing<T> : Option<T>
	{
		public override string ToString()
		{
			return "[Nothing]";
		}

		public override T Value
		{
			get
			{
				throw new InvalidOperationException("Option<T>.Nothing has no value");
			}
		}

		public override bool HasValue
		{
			get
			{
				return false;
			}
		}

		public override R Match<R>(Func<R> Just, Func<R> Nothing)
		{
			return Nothing();
		}

		public override R Match<R>(Func<T, R> Just, Func<R> Nothing)
		{
			return Nothing();
		}

		public override R Match<R>(Func<R> Just, R Nothing)
		{
			return Nothing;
		}

		public override R Match<R>(Func<T, R> Just, R Nothing)
		{
			return Nothing;
		}
	}
}
