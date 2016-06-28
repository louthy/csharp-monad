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

using System.Collections.Generic;
using System.Linq;
using Monad.Utility;

namespace Monad.Parsec.Token
{
    public interface IToken
    {
    }

    public abstract class Token : IToken
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

    public class CharToken : Token
    {
        public readonly ParserChar Value;

        public CharToken(ParserChar value, SrcLoc location = null)
            :
            base(location)
        {
            Value = value;
        }
    }

    public class CharLiteralToken : CharToken
    {
        public CharLiteralToken(ParserChar value, SrcLoc location = null)
            :
            base(value,location)
        {
        }
    }

    public class StringToken : Token
    {
        public readonly ImmutableList<ParserChar> Value;

        public StringToken(ImmutableList<ParserChar> value, SrcLoc location = null)
            :
            base(location)
        {
            Value = value;
        }
    }

    public class WhiteSpaceToken : StringToken
    {
        public WhiteSpaceToken(ImmutableList<ParserChar> value, SrcLoc location = null)
            :
            base(value,location)
        {
        }
    }

    public class StringLiteralToken : StringToken
    {
        public StringLiteralToken(ImmutableList<ParserChar> value, SrcLoc location = null)
            :
            base(value, location)
        {
        }
    }

    public class IdentifierToken : StringToken
    {
        public IdentifierToken(ImmutableList<ParserChar> value, SrcLoc location = null)
            :
            base(value,location)
        {
        }
    }

    public class ReservedToken : StringToken
    {
        public ReservedToken(ImmutableList<ParserChar> value, SrcLoc location = null)
            :
            base(value, location)
        {
        }
    }

    public class OperatorToken : StringToken
    {
        public OperatorToken(ImmutableList<ParserChar> value, SrcLoc location = null)
            :
            base(value, location)
        {
        }
    }

    public class ReservedOpToken : StringToken
    {
        public ReservedOpToken(ImmutableList<ParserChar> value, SrcLoc location = null)
            :
            base(value, location)
        {
        }
    }

    public class SymbolToken : StringToken
    {
        public SymbolToken(ImmutableList<ParserChar> value, SrcLoc location = null)
            :
            base(value, location == null ? location = value.First().Location : location)
        {
        }
    }

    public class BracketingToken : Token
    {
        public readonly IEnumerable<Token> Tokens;

        public BracketingToken(IEnumerable<Token> tokens, SrcLoc location = null)
            :
            base(location)
        {
            Tokens = tokens;
        }
    }

    public class AnglesToken : BracketingToken
    {
        public AnglesToken(IEnumerable<Token> tokens, SrcLoc location = null)
            :
            base(tokens,location)
        {
        }
    }

    public class BracesToken : BracketingToken
    {
        public BracesToken(IEnumerable<Token> tokens, SrcLoc location = null)
            :
            base(tokens, location)
        {
        }
    }

    public class ParensToken : BracketingToken
    {
        public ParensToken(IEnumerable<Token> tokens, SrcLoc location = null)
            :
            base(tokens, location)
        {
        }
    }

    public class BracketsToken : BracketingToken
    {
        public BracketsToken(IEnumerable<Token> tokens, SrcLoc location = null)
            :
            base(tokens, location)
        {
        }
    }
}
