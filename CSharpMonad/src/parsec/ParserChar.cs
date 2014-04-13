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
        public readonly SrcLoc Location;

        public ParserChar(char c, SrcLoc location = null)
        {
            Value = c;
            Location = location == null
                ? SrcLoc.Null
                : location;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
