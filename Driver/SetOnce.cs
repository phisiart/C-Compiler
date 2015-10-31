using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SetOnce<T> {
    public SetOnce() {
        this.IsSet = false;
        this._value = default(T);
    }

    public Boolean IsSet { get; private set; }
    private T _value;

    public T Value {
        get {
            if (!this.IsSet) {
                throw new InvalidOperationException("Value hasn't been set.");
            }
            return _value;
        }
        set {
            if (this.IsSet) {
                throw new InvalidOperationException("Value cannot be set twice.");
            }
            this._value = value;
            this.IsSet = true;
        }
    }

}

