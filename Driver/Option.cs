using System;

public abstract class Option<T> {
    public Option<O> Map<O>(Converter<T, O> converter) {
        if (this.IsSome) {
            return new Some<O>(converter(this.Value));
        } else {
            return new None<O>();
        }
    }
    public abstract T Value { get; }
    public abstract Boolean IsSome { get; }
    public abstract Boolean IsNone { get; }

    public static Option<T> None { get; } = new None<T>();
}

public static class Option {
    public static Option<T> Some<T>(T value) => new Some<T>(value);
}

public sealed class None<T> : Option<T> {
    public override T Value {
        get {
            throw new NotSupportedException("No value in None.");
        }
    }
    public override Boolean IsSome => false;
    public override Boolean IsNone => true;
}

public sealed class Some<T> : Option<T> {
    public Some(T value) {
        if (value == null) {
            throw new ArgumentNullException(nameof(value), "The value in Some cannot be null.");
        }
        this.Value = value;
    }
    public override T Value { get; }
    public override Boolean IsSome => true;
    public override Boolean IsNone => false;
}
