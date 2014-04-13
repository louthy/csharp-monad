using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec.Language
{
    public class Haskell98Def : HaskellStyle
    {
        public Haskell98Def()
        {
            ReservedOpNames = new string[] {"::","..","=","\\","|","<-","->","@","~","=>"};
            ReservedNames = new string[] {"let","in","case","of","if","then","else",
                                    "data","type",
                                    "class","default","deriving","do","import",
                                    "infix","infixl","infixr","instance","module",
                                    "newtype","where",
                                    "primitive"
                                    // "as","qualified","hiding"
                            };

        }
    }
}
