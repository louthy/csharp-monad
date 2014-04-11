using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec
{
    public class ParserResult<A>
    {
        public readonly IEnumerable<Tuple<A, IEnumerable<ParserChar>>> Value;
        public readonly IEnumerable<ParserError> Errors;

        public ParserResult(IEnumerable<Tuple<A, IEnumerable<ParserChar>>> value)
        {
            Value = value;
        }

        public ParserResult(IEnumerable<ParserError> errors)
        {
            Value = new Tuple<A, IEnumerable<ParserChar>>[0];
            Errors = errors;
        }

        public bool IsFaulted
        {
            get
            {
                return Value.Count() == 0;
            }
        }
    }

    public static class ParserResult
    {
        public static ParserResult<A> Fail<A>(IEnumerable<ParserError> errors)
        {
            return new ParserResult<A>(errors);
        }

        public static ParserResult<A> Fail<A>(ParserError error)
        {
            return new ParserResult<A>(error.Cons());
        }

        public static ParserResult<A> Fail<A>(string expected, IEnumerable<ParserChar> input,string message = "")
        {
            return new ParserResult<A>(new ParserError(expected,input,message).Cons());
        }

        public static ParserResult<A> Success<A>(this IEnumerable<Tuple<A, IEnumerable<ParserChar>>> self)
        {
            return new ParserResult<A>(self);
        }
    }
}
