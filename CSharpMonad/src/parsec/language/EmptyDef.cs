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

using Monad.Parsec.Token;

namespace Monad.Parsec.Language
{
    /// <summary>
    /// This is the most minimal token definition. It is recommended to use
    /// this definition as the basis for other definitions. EmptyDef has
    /// no reserved names or operators, is case sensitive and doesn't accept
    /// comments, identifiers or operators    
    /// </summary>
    public class EmptyDef : LanguageDef
    {
        public EmptyDef()
        {
            CommentStart = "";
            CommentEnd = "";
            CommentLine = "";
            NestedComments = true;
            IdentStart = Prim.Letter().Or( Prim.Character('_') );
            IdentLetter = Prim.LetterOrDigit().Or(Prim.Character('_').Or(Prim.Character('\'')));
            OpStart = OpLetter = Prim.OneOf(":!#$%&*+./<=>?@\\^|-~");
            ReservedOpNames = new string[0];
            ReservedNames = new string[0];
            CaseSensitive = true;

        }
    }
}
