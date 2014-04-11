using Monad.Parsec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad
{
    public static class EnumerableExt
    {
        public static T Head<T>(this IEnumerable<T> self)
        {
            return self.First();
        }

        public static IEnumerable<T> Tail<T>(this IEnumerable<T> self)
        {
            return self.Skip(1);
        }

        public static string AsString(this IEnumerable<char> self)
        {
            return String.Concat(self);
        }

        public static string AsString(this IEnumerable<ParserChar> self)
        {
            return String.Concat(self.Select(pc=>pc.Value));
        }

        public static T Second<T>(this IEnumerable<T> self)
        {
            return self.Tail().Head();
        }

        public static IEnumerable<ParserChar> ToParserChar(this IEnumerable<char> input)
        {
            int line = 1;
            int col = 1;

            foreach (var c in input)
            {
                if (c == '\r')
                {
                    continue;
                }

                yield return new ParserChar(c, col, line);

                if (c == '\n')
                {
                    line++;
                    col = 1;
                }
                col++;
            }
        }

    }
}
