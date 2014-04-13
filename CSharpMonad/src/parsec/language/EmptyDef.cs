using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monad.Parsec.Token;
using Monad.Parsec;

namespace Monad.Parsec.Language
{
    /// <summary>
    /// This is the most minimal token definition. It is recommended to use
    /// this definition as the basis for other definitions. EmptyDef has
    /// no reserved names or operators, is case sensitive and doesn't accept
    /// comments, identifiers or operators    
    /// </summary>
    public class EmptyDef : GeneralLanguageDef
    {
        public EmptyDef()
        {
            CommentStart = "";
            CommentEnd = "";
            CommentLine = "";
            NestedComments = true;
            IdentStart = New.Letter().Or( New.Character('_') );
            IdentLetter = New.LetterOrDigit().Or(New.Character('_').Or(New.Character('\'')));
            OpStart = OpLetter = New.OneOf(":!#$%&*+./<=>?@\\^|-~");
            ReservedOpNames = new string[0];
            ReservedNames = new string[0];
            CaseSensitive = true;

        }
    }
}
