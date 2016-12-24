using System;
using ABT2.TypeSystem;
using ABT2.Environment;

namespace ABT2.Expressions {
    public sealed class IntSub : SIntBinaryOperator {
        public IntSub(IRValueExpr<TSInt> left, IRValueExpr<TSInt> right)
            : base(left, right) { }
    }

    public sealed class UIntSub : UIntBinaryOperator {
        public UIntSub(IRValueExpr<TUInt> left, IRValueExpr<TUInt> right)
            : base(left, right) { }
    }

    public interface ISub : IRValueExpr<IExprType> {
        
    }

    public static class Sub {
        public static IRValueExpr<IExprType> Create(IRValueExpr<IExprType> left, IRValueExpr<IExprType> right) {
            // TODO: implement this
            throw new NotImplementedException();
        }
    }
}