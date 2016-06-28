////////////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
// 
// Copyright (c) 2014 Paul Louth
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using Monad.Parsec;

namespace Monad.Utility
{
    public static class ImmutableList
    {
        public static ImmutableList<T> Empty<T>()
        {
            return new ImmutableList<T>(new T[0]);
        }

        public static ImmutableList<T> Single<T>(T value)
        {
            return new ImmutableList<T>(new T[1] { value });
        }
    }

    /// <summary>
    /// Thread-safe immutable list implementation
    /// TODO: This can definitely be optimised.
    /// </summary>
    public class ImmutableList<T> : IEnumerable<T>
    {
        const int CollapseAtDepth = 256;

        private readonly T[] source;
        private readonly int length;
        private readonly int sourceLength;
        private readonly int totalLength;
        private readonly ImmutableList<T> next;
        private readonly int pos;
        private readonly int depth;

        public ImmutableList(IEnumerable<T> source)
        {
            this.source = source.ToArray();
            this.sourceLength = this.length = this.source.Length;
            this.totalLength = this.length;
        }

        private ImmutableList(T[] source, int pos, ImmutableList<T> next, int depth)
        {
            if (depth >= CollapseAtDepth)
            {
                T[] ts = new T[source.Length + next.Length];
                int index = source.Length;
                Array.Copy(source, ts, source.Length);

                while (next != null)
                {
                    Array.Copy(next.source, next.pos, ts, index, next.length);
                    index += next.length;
                    next = next.next;
                }
                source = ts;
                next = null;
                depth = 0;
            }

            this.sourceLength = source.Length;
            this.length = source.Length - pos;
            this.totalLength = this.length + (next == null ? 0 : next.totalLength);
            this.source = source;
            this.pos = pos;
            this.next = next;
            this.depth = depth;
        }

        public int Length
        {
            get
            {
                return totalLength;
            }
        }

        public ImmutableList<R> Select<R>(Func<T, R> project)
        {
            var list = new List<R>();
            foreach (var item in this)
            {
                list.Add(project(item));
            }
            return new ImmutableList<R>(list);
        }

        public ImmutableList<T> InsertAtHead(T value)
        {
            return new ImmutableList<T>(new T[1] { value }, 0, this, this.depth + 1);
        }

        public ImmutableList<T> Concat(ImmutableList<T> right)
        {
            return new ImmutableList<T>(source, pos, right, right.depth + 1);
        }

        public T Head()
        {
            if (IsEmpty)
            {
                throw new ParserException("Nothing left to parse");
            }
            if (length == 0)
            {
                return next.Head();
            }
            else
            {
                return source[pos];
            }
        }

        public T HeadSafe()
        {
            if (IsEmpty)
            {
                return default(T);
            }
            if (length == 0)
            {
                return next.Head();
            }
            else
            {
                return source[pos];
            }
        }

        public ImmutableList<T> Tail()
        {
            if (length == 1)
            {
                if (next == null)
                {
                    return new ImmutableList<T>(source, pos + 1, next, next == null ? 0 : next.depth + 1);
                }
                else
                {
                    return next;
                }
            }
            if (length == 0)
            {
                throw new ParserException("Nothing left to parse (tail access)");
            }
            return new ImmutableList<T>(source, pos + 1, next, next == null ? 0 : next.depth + 1);
        }

        public bool IsEmpty
        {
            get
            {
                return length == 0
                    ? next == null
                        ? true
                        : next.IsEmpty
                    : false;
            }
        }

        public bool IsAlmostEmpty
        {
            get
            {
                return next == null
                    ? length == 1
                    : next.IsAlmostEmpty;
            }
        }

        public IEnumerator<T> GetReverseEnumerator()
        {
            return new ReverseIter(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Iter(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Iter(this);
        }

        private T GetIndex(int index)
        {
            var offset = pos + index;

            if (offset >= sourceLength)
            {
                return next.GetIndex(offset - sourceLength);
            }
            else
            {
                return source[offset];
            }
        }

        private class Iter : IEnumerator<T>
        {
            ImmutableList<T> str;
            int current = -1;
            int length;

            public Iter(ImmutableList<T> str)
            {
                this.str = str;
                this.length = this.str.Length;
            }

            public T Current
            {
                get
                {
                    return str.GetIndex(current);
                }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return str.GetIndex(current);
                }
            }

            public bool MoveNext()
            {
                current++;
                if (current == length)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            public void Reset()
            {
                current = -1;
            }
        }


        private class ReverseIter : IEnumerator<T>
        {
            ImmutableList<T> str;
            int current;

            public ReverseIter(ImmutableList<T> str)
            {
                this.str = str;
                this.current = str.Length;
            }

            public T Current
            {
                get
                {
                    return str.GetIndex(current);
                }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return str.GetIndex(current);
                }
            }

            public bool MoveNext()
            {
                current--;
                if (current < 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            public void Reset()
            {
                current = str.Length;
            }
        }
    }
}