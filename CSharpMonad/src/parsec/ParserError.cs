using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monad.Parsec
{
    public class ParserError
    {
        public readonly string Message;
        public readonly string Expected;
        public readonly IEnumerable<ParserChar> Input;
        public readonly int Line;
        public readonly int Column;
        public readonly string Location;

        public ParserError(string expected, IEnumerable<ParserChar> input, string message = "")
        {
            Message = message;
            Expected = expected;
            Input = input;
            if (input.Count() > 0)
            {
                var fst = input.First();
                Line = fst.Line;
                Column = fst.Column;
                Location = Line + ":" + Column;
            }
            else
            {
                Location = "end of input";
            }
        }

        public static ParserError Create(string expected, IEnumerable<ParserChar> input, string message = "")
        {
            return new ParserError(expected, input, message);
        }

        public static ParserError Create(char expected, IEnumerable<ParserChar> input, string message = "")
        {
            return new ParserError(expected.ToString(), input, message);
        }
    }
}
