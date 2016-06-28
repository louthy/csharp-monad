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

using Monad.Utility;
using System.Collections.Generic;
using System.Linq;

namespace Monad.Parsec.Token
{
    public class LanguageDef : GeneralLanguageDef
    {
    }

    public class GeneralLanguageDef
    {
        /// <summary>
        /// Describes the start of a block comment. Use the empty string if the
        /// language doesn't support block comments. 
        /// </summary>
        public string CommentStart;

        /// <summary>
        /// Describes the end of a block comment. Use the empty string if the
        /// language doesn't support block comments. 
        /// </summary>
        public string CommentEnd;

        /// <summary>
        /// Describes the start of a line comment. Use the empty string if the
        /// language doesn't support line comments. 
        /// </summary>
        public string CommentLine;

        /// <summary>
        /// Set to 'True' if the language supports nested block comments
        /// </summary>
        public bool NestedComments;

        /// <summary>
        /// This parser should accept any start characters of identifiers
        /// </summary>
        public Parser<ParserChar> IdentStart;

        /// <summary>
        /// This parser should accept any legal tail characters of identifiers
        /// </summary>
        public Parser<ParserChar> IdentLetter;

        /// <summary>
        /// This parser should accept any start characters of operators. For
        /// example \":!#$%&*+.\/\<=>?\@\\\\^|-~\"@ 
        /// </summary>
        public Parser<ParserChar> OpStart;

        /// <summary>
        /// This parser should accept any legal tail characters of operators.
        /// Note that this parser should even be defined if the language doesn't
        /// support user-defined operators, or otherwise the 'reservedOp'
        /// parser won't work correctly. 
        /// </summary>
        public Parser<ParserChar> OpLetter;

        /// <summary>
        /// The list of reserved identifiers
        /// </summary>
        public IEnumerable<string> ReservedNames;

        /// <summary>
        /// The list of reserved operators
        /// </summary>
        public IEnumerable<string> ReservedOpNames;

        /// <summary>
        /// The list of reserved operators
        /// </summary>
        public bool CaseSensitive;

        /// <summary>
        /// A distinct set of the characters in the start and end multiline comment anchors
        /// </summary>
        public Memo<IEnumerable<char>> CommentStartEndDistinctChars
        {
            get
            {
                return Memo.ize(() => 
                    Enumerable.Concat(
                        CommentStart.Cast<char>(), 
                        CommentEnd.Cast<char>()
                        ).Distinct()
                    );
            }
        }
    }

}
