using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec.Token
{
    public abstract class Token
    {
        public readonly SrcLoc Location;

        public Token(SrcLoc location)
        {
            Location = location;
        }

    }

    public class IntegerToken : Token
    {
        public readonly int Value;

        public IntegerToken(int value, SrcLoc location = null)
            :
            base(location)
        {
            Value = value;
        }
    }

    public class FloatToken : Token
    {
        public readonly double Value;

        public FloatToken(double value, SrcLoc location = null)
            :
            base(location)
        {
            Value = value;
        }
    }

    public class NaturalOrFloatToken : Token
    {
        public readonly Token Value;

        public NaturalOrFloatToken(double value, SrcLoc location = null)
            :
            base(location)
        {
            Value = new FloatToken(value,location);
        }

        public NaturalOrFloatToken(int value, SrcLoc location = null)
            :
            base(location)
        {
            Value = new IntegerToken(value, location);
        }
    }

    public class StringToken : Token
    {
        public readonly IEnumerable<ParserChar> Value;

        public StringToken(IEnumerable<ParserChar> value, SrcLoc location = null)
            :
            base(location)
        {
            Value = value;
        }
    }

    public class IdentifierToken : StringToken
    {
        public IdentifierToken(IEnumerable<ParserChar> value, SrcLoc location = null)
            :
            base(value,location)
        {
        }
    }

    public class ReservedToken : StringToken
    {
        public ReservedToken(IEnumerable<ParserChar> value, SrcLoc location = null)
            :
            base(value, location)
        {
        }
    }

    public class OperatorToken : StringToken
    {
        public OperatorToken(IEnumerable<ParserChar> value, SrcLoc location = null)
            :
            base(value, location)
        {
        }
    }

    public class ReservedOpToken : StringToken
    {
        public ReservedOpToken(IEnumerable<ParserChar> value, SrcLoc location = null)
            :
            base(value, location)
        {
        }
    }
}
