using System.Collections.Generic;

namespace Common
{
    public class Tuple<T, T2>
    {
        public T Item1;
        public T2 Item2;

        public Tuple(T item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    public class TupleList<T, T2> : List<Tuple<T, T2>>
    {
        public void Add(T item1, T2 item2)
        {
            Add(new Tuple<T, T2>(item1, item2));
        }
    }

    public class Tuple<T, T2, T3>
    {
        public T Item1;
        public T2 Item2;
        public T3 Item3;

        public Tuple(T item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
    }

    public class TupleList<T, T2, T3> : List<Tuple<T, T2, T3>>
    {
        public void Add(T item1, T2 item2, T3 item3)
        {
            Add(new Tuple<T, T2, T3>(item1, item2, item3));
        }
    }
}