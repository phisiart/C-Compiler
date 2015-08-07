using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class Option<T> {
    public Option<O> Map<O>(Converter<T, O> Convert) {
        if (IsSome) {
            return new Some<O>(Convert(value));
        } else {
            return new None<O>();
        }
    }
    public abstract T value { get; }
    public abstract Boolean IsSome { get; }
    public abstract Boolean IsNone { get; }
}

public sealed class None<T> : Option<T> {
    public override T value {
        get {
            throw new NotSupportedException("No value in None.");
        }
    }
    public override Boolean IsSome { get { return false; } }
    public override Boolean IsNone { get { return true; } }
}

public sealed class Some<T> : Option<T> {
    private readonly T _value;
    public Some(T value) {
        if (value == null) {
            throw new ArgumentNullException("value", "The value in Some cannot be null.");
        }
        _value = value;
    }
    public override T value { get { return _value; } }
    public override Boolean IsSome { get { return true; } }
    public override Boolean IsNone { get { return false; } }
}
