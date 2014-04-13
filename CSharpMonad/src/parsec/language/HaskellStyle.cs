using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monad.Parsec.Token;
using Monad.Parsec;

namespace Monad.Parsec.Language
{
    public class HaskellStyle : EmptyDef
    {
        public HaskellStyle()
        {
            CommentStart = "{-";
            CommentEnd = "-}";
            CommentLine = "--";
            NestedComments = true;
            IdentStart = New.Letter();
            IdentLetter = New.LetterOrDigit().Or(New.Character('_').Or(New.Character('\'')));
            OpStart = OpLetter = New.OneOf(":!#$%&*+./<=>?@\\^|-~");
            ReservedOpNames = new string[0];
            ReservedNames = new string[0];
            CaseSensitive = true;
        }
    }
}
