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
using System.Reflection;

namespace Monad
{
    /// <summary>
    /// Just case of the Option<T> monad
    /// </summary>
    public class JustResult<T> : OptionResult<T>
    {
        static readonly string TypeOfT = typeof(T).ToString();
        static readonly bool IsAppendable = typeof(IAppendable<T>).GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo());
        private readonly T value;

        public JustResult(T value)
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

        /// <summary>
        /// Monadic append
        /// If the lhs or rhs are in a Nothing state then Nothing propagates
        /// </summary>
        public override OptionResult<T> Mappend(OptionResult<T> rhs)
        {
            if (rhs == null) throw new ArgumentNullException("rhs");
            if (!rhs.HasValue)
            {
                return rhs;
            }
            else
            {
                if (IsAppendable)
                {
                    var lhs = this.Value as IAppendable<T>;
                    return new JustResult<T>(lhs.Append(rhs.Value));
                }
                else
                {
                    // TODO: Consider replacing this with a Reflection.Emit which does this job efficiently.
                    switch (TypeOfT)
                    {
                        case "System.Int64":
                            return new JustResult<T>((T)Convert.ChangeType((Convert.ToInt64(Value) + Convert.ToInt64(rhs.Value)), typeof(T)));
                        case "System.UInt64":
                            return new JustResult<T>((T)Convert.ChangeType((Convert.ToUInt64(Value) + Convert.ToUInt64(rhs.Value)), typeof(T)));
                        case "System.Int32":
                            return new JustResult<T>((T)Convert.ChangeType((Convert.ToInt32(Value) + Convert.ToInt32(rhs.Value)), typeof(T)));
                        case "System.UInt32":
                            return new JustResult<T>((T)Convert.ChangeType((Convert.ToUInt32(Value) + Convert.ToUInt32(rhs.Value)), typeof(T)));
                        case "System.Int16":
                            return new JustResult<T>((T)Convert.ChangeType((Convert.ToInt16(Value) + Convert.ToInt16(rhs.Value)), typeof(T)));
                        case "System.UInt16":
                            return new JustResult<T>((T)Convert.ChangeType((Convert.ToUInt16(Value) + Convert.ToUInt16(rhs.Value)), typeof(T)));
                        case "System.Decimal":
                            return new JustResult<T>((T)Convert.ChangeType((Convert.ToDecimal(Value) + Convert.ToDecimal(rhs.Value)), typeof(T)));
                        case "System.Double":
                            return new JustResult<T>((T)Convert.ChangeType((Convert.ToDouble(Value) + Convert.ToDouble(rhs.Value)), typeof(T)));
                        case "System.Single":
                            return new JustResult<T>((T)Convert.ChangeType((Convert.ToSingle(Value) + Convert.ToSingle(rhs.Value)), typeof(T)));
                        case "System.Char":
                            return new JustResult<T>((T)Convert.ChangeType((Convert.ToChar(Value) + Convert.ToChar(rhs.Value)), typeof(T)));
                        case "System.Byte":
                            return new JustResult<T>((T)Convert.ChangeType((Convert.ToByte(Value) + Convert.ToByte(rhs.Value)), typeof(T)));
                        case "System.String":
                            return new JustResult<T>((T)Convert.ChangeType((Convert.ToString(Value) + Convert.ToString(rhs.Value)), typeof(T)));
                        default:
                            throw new InvalidOperationException("Type " + typeof(T).Name + " is not appendable.  Consider implementing the IAppendable interface.");
                    }
                }
            }
        }
    }
}