using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec
{
    public class ParserChar
    {
        public readonly char Value;
        public readonly int Column;
        public readonly int Line;

        public ParserChar(char c, int column, int line)
        {
            Value = c;
            Column = column;
            Line = line;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
