using System;
using ABT2.TypeSystem;
using ABT2.Environment;

namespace ABT2.Expressions {
    public sealed class SIntAdd : SIntBinaryOperator {
        public SIntAdd(IRValueExpr<TSInt> left, IRValueExpr<TSInt> right)
            : base(left, right) { }
    }

    public sealed class UIntAdd : UIntBinaryOperator {
        public UIntAdd(IRValueExpr<TUInt> left, IRValueExpr<TUInt> right)
            : base(left, right) { }
    }

    public sealed class SLongAdd : SLongBinaryOperator {
        public SLongAdd(IRValueExpr<TSLong> left, IRValueExpr<TSLong> right)
            : base(left, right) { }
    }

    public sealed class ULongAdd : ULongBinaryOperator {
        public ULongAdd(IRValueExpr<TULong> left, IRValueExpr<TULong> right)
            : base(left, right) { }
    }
    
    public static class Add {
        public static IRValueExpr<IExprType> Create(IRValueExpr<IExprType> left, IRValueExpr<IExprType> right) {
            // TODO: implement this
            throw new NotImplementedException();
        }
    }
}
