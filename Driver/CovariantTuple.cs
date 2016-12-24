using System;

public interface ICovariantTuple<out T1, out T2> {
    T1 Item1 { get; }

    T2 Item2 { get; }

    T1 Head { get; }

    T2 Tail { get; }
}

public class CovariantTuple<T1, T2> : Tuple<T1, T2>, ICovariantTuple<T1, T2> {
    public CovariantTuple(T1 head, T2 tail) : base(head, tail) { }

    public T1 Head => this.Item1;

    public T2 Tail => this.Item2;
}

public class CovariantTuple {
    public static ICovariantTuple<T1, T2> Create<T1, T2>(T1 head, T2 tail) =>
        new CovariantTuple<T1, T2>(head, tail);
}