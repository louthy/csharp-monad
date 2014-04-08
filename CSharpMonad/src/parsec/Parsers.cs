using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec
{
    public class Item : Parser<char>
    {
        public Item()
            :
            base(
                inp => inp.Count() == 0
                    ? Empty.Return<char>()
                    : Tuple.Create(inp.Head(), inp.Tail()).Cons()
            )
        { }
    }

    public class Failure<A> : Parser<A>
    {
        public Failure()
            :
            base(
                inp => Empty.Return<A>()
            )
        { }


    }

    public class Return<A> : Parser<A>
    {
        public Return(A v)
            :
            base(
                inp => Tuple.Create(v, inp).Cons()
            )
        { }

    }

    public class Choice<A> : Parser<A>
    {
        public Choice(Parser<A> p, Parser<A> q)
            :
            base(
                inp =>
                {
                    var pres = p.Parse(inp);
                    if (pres.Count() == 0)
                        return q.Parse(inp);
                    else
                        return pres;
                }
            )
        { }
    }

    public class Satisfy : Parser<char>
    {
        public Satisfy(Func<char, bool> pred)
            :
            base(
                inp =>
                    inp.Count() == 0 
                        ? New.Failure<char>().Parse(inp)
                        : (from res in New.Item().Parse(inp)
                           select pred(res.Item1)
                              ? New.Return(res.Item1).Parse(inp.Tail())
                              : New.Failure<char>().Parse(inp))
                          .First()
            )
        { }
    }

    public class Digit : Satisfy
    {
        public Digit()
            :
            base(c=> Char.IsDigit(c))
        {
        }
    }

    public class Character : Satisfy
    {
        public Character(char isChar)
            :
            base(c => c == isChar)
        { }
    }

    public class Many<A> : Parser<IEnumerable<A>>
    {
        public Many(Parser<A> parser)
            :
            base(
                inp =>
                    New.Choice(
                        New.Many1(parser),
                        New.Return<IEnumerable<A>>( new A[0] )
                    )
                    .Parse(inp)
            )
        { }
    }

    public class Many1<A> : Parser<IEnumerable<A>>
    {
        public Many1(Parser<A> parser)
            :
            base(
                inp =>
                {
                    var v = parser.Parse(inp);
                    if (v.Count() == 0) return Empty.Return<IEnumerable<A>>();

                    var fst = v.First();
                    var vs = New.Many(parser).Parse(fst.Item2);
                    if (v.Count() == 0) return New.Return(fst.Item1.Cons()).Parse(fst.Item2);

                    var snd = vs.First();

                    return New.Return(fst.Item1.Cons(snd.Item1)).Parse(snd.Item2);
                }
            )
        { }
    }

    public class StringParse : Parser<IEnumerable<char>>
    {
        public StringParse(string str)
            :
            this( str as IEnumerable<char> )
        {
        }

        public StringParse(IEnumerable<char> str)
            :
            base(
                inp => str.Count() == 0
                          ? New.Return(new char[0] as IEnumerable<char>).Parse(inp)
                          : (from x in New.Character(str.Head())
                             from xs in New.String(str.Tail())
                             select x.Cons(xs) )
                            .Parse(inp)
            )
        { }
    }
}
