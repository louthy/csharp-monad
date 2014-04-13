using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec
{
    public class SrcLoc
    {
        private static SrcLoc unit = new SrcLoc(0, 0,"(location:null)");
        private static SrcLoc endOfSource = new SrcLoc(0, 0, "(end-of-source)");

        public readonly int Line;
        public readonly int Column;

        private SrcLoc(int line, int column, string displayTemplate)
        {
            Line = line;
            Column = column;
        }

        public static SrcLoc Return(int line, int column)
        {
            return new SrcLoc(line, column,"({0},{1})");
        }

        public static SrcLoc Null
        {
            get
            {
                return unit;
            }
        }

        public static SrcLoc EndOfSource
        {
            get
            {
                return endOfSource;
            }
        }

        public override string ToString()
        {
            return String.Format("({0},{1})", Line, Column);
        }
    }
}
