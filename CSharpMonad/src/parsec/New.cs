using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec
{
    public static class New
    {
        public static Item Item()
        {
            return new Item();
        }
        public static Failure<T> Failure<T>()
        {
            return new Failure<T>();
        }
        public static Return<T> Return<T>(T v)
        {
            return new Return<T>(v);
        }
        public static Choice<A> Choice<A>(Parser<A> p, Parser<A> q)
        {
            return new Choice<A>(p, q);
        }
        public static Satisfy Satisfy(Func<char, bool> predicate)
        {
            return new Satisfy(predicate);
        }
        public static Digit Digit()
        {
            return new Digit();
        }
        public static Character Character(char c)
        {
            return new Character(c);
        }
        public static Many<T> Many<T>(Parser<T> parser)
        {
            return new Many<T>(parser);
        }
        public static Many1<T> Many1<T>(Parser<T> parser)
        {
            return new Many1<T>(parser);
        }
        public static StringParse String(string str)
        {
            return new StringParse(str);
        }
        public static StringParse String(IEnumerable<char> str)
        {
            return new StringParse(str);
        }
    }
}
