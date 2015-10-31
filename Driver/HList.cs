public static class HList {
    public static HNil HNil() => hnil;
    public static HCons<H, T> HCons<H, T>(H head, T tail) where T : HList<T> => new HCons<H, T>(head, tail);
    private static HNil hnil = new HNil();
}

public class HList<T> { }

public class HNil : HList<HNil> { }

public class HCons<H, T> : HList<HCons<H, T>> where T : HList<T> {
    public HCons(H head, T tail) {
        this.head = head;
        this.tail = tail;
    }
    public readonly H head;
    public readonly T tail;
}
