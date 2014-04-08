using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec
{
    public static class Empty
    {
        public static IEnumerable<Tuple<A, IEnumerable<char>>> Return<A>()
        {
            return new Tuple<A, IEnumerable<char>>[0];
        }
    }
}
