using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad
{
	/// <summary>
	/// Static helper class for generating Error<T> monads
	/// </summary>
	public static class Error
	{
		/// <summary>
		/// Return - creates a monad from the result of invoking a function
		/// </summary>
		public static Error<T> Return<T>(Func<T> getValue)
		{
			return new Error<T>(getValue);
		}
	}

	/// <summary>
	/// Error monad, for error handling responses
	/// </summary>
	public class Error<T>
	{
		/// <summary>
		/// Wrapped value
		/// </summary>
		public readonly T Value;

		/// <summary>
		/// Exception (if IsFaulted == true)
		/// </summary>
		public readonly Exception Exception;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="value"></param>
		internal Error(T value)
		{
			Value = value;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		public Error(Exception exception)
		{
			Exception = exception;
		}

		/// <summary>
		/// Higher order ctor which retreives the value
		/// </summary>
		/// <param name="getValue"></param>
		public Error(Func<T> getValue)
		{
			try
			{
				Value = getValue();
			}
			catch (Exception exc)
			{
				Exception = exc;
			}
		}

		/// <summary>
		/// Is in an exceptional state
		/// </summary>
		public bool IsFaulted
		{
			get
			{
				return Exception != null;
			}
		}

		/// <summary>
		/// ToString override
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return IsFaulted
				? Exception.ToString()
				: Value != null
					? Value.ToString()
					: "[null]";
		}
	}

	/// <summary>
	/// Error monad extensions
	/// </summary>
	public static class ErrorMonadExtensions
	{
		/// <summary>
		/// Monadic bind implementation
		/// </summary>
		public static Error<U> SelectMany<T, U>(this Error<T> value, Func<T, Error<U>> k)
		{
			return (value.IsFaulted)
				? new Error<U>(value.Exception)
				: k(value.Value);
		}

		/// <summary>
		/// Monadic bind implementation
		/// </summary>
		public static Error<V> SelectMany<T, U, V>(this Error<T> value, Func<T, Error<U>> k, Func<T, U, V> m)
		{
			return value.SelectMany(t =>
					k(t).SelectMany(u =>
						Error.Return(() => m(t, u))
					)
				);
		}

		/// <summary>
		/// Allows simple chaining of Error monads
		/// </summary>
		public static Error<U> Then<T, U>(this Error<T> value, Func<T, U> getValue)
		{
			return value.IsFaulted
				? new Error<U>(value.Exception)
				: Error.Return<U>(() => getValue(value.Value));
		}
	}
}