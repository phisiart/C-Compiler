using System;

public interface IOption<out T> {
    IOption<O> Map<O>(Converter<T, O> converter);

    IOption<O> FlatMap<O>(Converter<T, IOption<O>> converter);

    void ForEach(Action<T> action);

    T Value { get; }

    Boolean IsSome { get; }

    Boolean IsNone { get; }
}

public abstract class Option<T> : IOption<T> {
    public IOption<O> Map<O>(Converter<T, O> converter) {
        if (this.IsSome) {
            return new Some<O>(converter(this.Value));
        }
        return new None<O>();
    }

    public IOption<O> FlatMap<O>(Converter<T, IOption<O>> converter) {
        if (this.IsSome) {
            return converter(this.Value);
        }
        return new None<O>();
    }

    public void ForEach(Action<T> action) {
        if (this.IsSome) {
            action(this.Value);
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
            throw new NotSupportedException("No Value in None.");
        }
    }

    public override Boolean IsSome => false;

    public override Boolean IsNone => true;
}

public sealed class Some<T> : Option<T> {
    public Some(T value) {
        this.Value = value;
    }

    public override T Value { get; }

    public override Boolean IsSome => true;

    public override Boolean IsNone => false;
}
