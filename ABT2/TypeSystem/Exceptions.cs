using System;
using ABT2.Environment;

namespace ABT2.TypeSystem {
    public class TypeCastException : Exception {
        public TypeCastException(IExprType fromType, IExprType toType, Env env)
            : base() { }

        public IExprType FromType { get; }

        public IExprType ToType { get; }

        public Env Env { get; }
    }
}