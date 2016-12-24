using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions.TypeCasts {
    public sealed class TypeCastException : Exception {
        public TypeCastException() { }

        public TypeCastException(String message)
            : base(message) { }

        public TypeCastException(String message, Exception inner)
            : base(message, inner) { }
    }
}
