using System;
using ABT2.TypeSystem;
using ABT2.Environment;

namespace ABT2.Expressions {
    public sealed class IntAdd : SIntBinaryOperator {
        public IntAdd(IRValueExpr<TSInt> left, IRValueExpr<TSInt> right)
            : base(left, right) { }
    }

    public sealed class UIntAdd : UIntBinaryOperator {
        public UIntAdd(IRValueExpr<TUInt> left, IRValueExpr<TUInt> right)
            : base(left, right) { }
    }

    public static class Add {
        public static IRValueExpr<IExprType> Create(IRValueExpr<IExprType> left, IRValueExpr<IExprType> right) {
            // TODO: implement this
            throw new NotImplementedException();
        }
    }
}
