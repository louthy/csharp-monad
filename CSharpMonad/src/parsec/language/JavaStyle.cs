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
    /// This is a minimal token definition for Java style languages. It
    /// defines the style of comments, valid identifiers and case
    /// sensitivity. It does not define any reserved words or operators.
    /// </summary>
    public class JavaStyle : EmptyDef
    {
        public JavaStyle()
        {
            CommentStart = "/*";
            CommentEnd = "*/";
            CommentLine = "//";
            NestedComments = true;
            IdentStart = New.Letter();
            IdentLetter = New.LetterOrDigit().Or(New.Character('_').Or(New.Character('\'')));
            ReservedOpNames = new string[0];
            ReservedNames = new string[0];
            CaseSensitive = true;
        }
    }
}
