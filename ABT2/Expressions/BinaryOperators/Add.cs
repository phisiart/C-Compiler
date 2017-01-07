using System;
using ABT2.TypeSystem;

namespace ABT2.Expressions {
    public interface IAdd : IBinaryOperator { }

    public interface IAdd<out T> : IAdd, IBinaryOperator<T> where T : IExprType { }

    public sealed class SIntAdd : SIntBinaryOperator, IAdd<TSInt> {
        public SIntAdd(IRValueExpr<TSInt> left, IRValueExpr<TSInt> right)
            : base(left, right) { }
    }

    public sealed class UIntAdd : UIntBinaryOperator, IAdd<TUInt> {
        public UIntAdd(IRValueExpr<TUInt> left, IRValueExpr<TUInt> right)
            : base(left, right) { }
    }

    public sealed class SLongAdd : SLongBinaryOperator, IAdd<TSLong> {
        public SLongAdd(IRValueExpr<TSLong> left, IRValueExpr<TSLong> right)
            : base(left, right) { }
    }

    public sealed class ULongAdd : ULongBinaryOperator, IAdd<TULong> {
        public ULongAdd(IRValueExpr<TULong> left, IRValueExpr<TULong> right)
            : base(left, right) { }
    }
    
    public static class Add {
        public static IAdd<IExprType> Create(IRValueExpr<IExprType> left,
                                             IRValueExpr<IExprType> right) {
            // TODO: implement this
            throw new NotImplementedException();
        }
    }
}
