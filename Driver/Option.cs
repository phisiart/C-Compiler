using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class Option<T> {
    public Option<O> Map<O>(Converter<T, O> Convert) {
        if (IsSome) {
            return new Some<O>(Convert(Value));
        } else {
            return new None<O>();
        }
    }
    public abstract T Value { get; }
    public abstract Boolean IsSome { get; }
    public abstract Boolean IsNone { get; }

    public static readonly Option<T> None = new None<T>();
}

public static class Option {
    public static Option<T> Some<T>(T value) => new Some<T>(value);
    public static Option<T> None<T>() => new None<T>();
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
    private readonly T _value;
    public Some(T value) {
        if (value == null) {
            throw new ArgumentNullException("value", "The value in Some cannot be null.");
        }
        _value = value;
    }
    public override T Value => _value;
    public override Boolean IsSome => true;
    public override Boolean IsNone => false;
}
