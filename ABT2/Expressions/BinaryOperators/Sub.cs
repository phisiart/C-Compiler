using System;
using ABT2.TypeSystem;

namespace ABT2.Expressions {
    public interface ISub : IBinaryOperator { }

    public interface ISub<out T, out T1, out T2> : ISub, IBinaryOperator<T, T1, T2>
        where T : class, IExprType
        where T1 : class, IExprType
        where T2 : class, IExprType { }

    public interface ISub<out T> : ISub<T, T, T>, IBinaryOperator<T>
        where T : class, IIntegralPromotionReturnType { }
    
    public sealed class IntSub : SIntBinaryOperator, ISub<TSInt> {
        public IntSub(IRValueExpr<TSInt> left, IRValueExpr<TSInt> right)
            : base(left, right) { }
    }

    public sealed class UIntSub : UIntBinaryOperator, ISub<TUInt> {
        public UIntSub(IRValueExpr<TUInt> left, IRValueExpr<TUInt> right)
            : base(left, right) { }
    }

    public sealed class SLongSub : SLongBinaryOperator, ISub<TSLong> {
        public SLongSub(IRValueExpr<TSLong> left, IRValueExpr<TSLong> right)
            : base(left, right) { }
    }

    public sealed class ULongSub : ULongBinaryOperator, ISub<TULong> {
        public ULongSub(IRValueExpr<TULong> left, IRValueExpr<TULong> right)
            : base(left, right) { }
    }

    public sealed class PtrSubULong
        : BinaryOperator<TPointer, TPointer, TULong>, ISub {

        public PtrSubULong(IRValueExpr<TPointer> left, IRValueExpr<TULong> right)
            : base(left, right) { }

        public override TPointer Type { get; }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitPointer(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitPointer(this);
        }
    }

    public sealed class PtrSubSLong
        : BinaryOperator<TPointer, TPointer, TSLong>, ISub {

        public PtrSubSLong(IRValueExpr<TPointer> left, IRValueExpr<TSLong> right)
            : base(left, right) { }

        public override TPointer Type { get; }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitPointer(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitPointer(this);
        }
    }

    public sealed class PtrSubPtr
        : BinaryOperator<TSLong, TPointer, TPointer>, ISub {

        public PtrSubPtr(IRValueExpr<TPointer> left, IRValueExpr<TPointer> right)
            : base(left, right) { }

        public override TSLong Type => TSLong.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSLong(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSLong(this);
        }
    }

    public static class Sub {
        public static IRValueExpr<IExprType> Create(IRValueExpr<IExprType> left, IRValueExpr<IExprType> right) {
            // TODO: implement this
            throw new NotImplementedException();
        }
    }
}