using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec.Token
{
    public class LanguageDef : GeneralLanguageDef
    {
        //public readonly string s
        //public readonly st u
        //public readonly Identity m
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
    }

}
