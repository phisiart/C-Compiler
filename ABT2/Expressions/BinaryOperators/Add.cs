using System;
using ABT2.TypeSystem;

namespace ABT2.Expressions {
    public interface IAdd : IBinaryOperator { }

    public interface IAdd<out T, out T1, out T2>
        : IAdd, IBinaryOperator<T, T1, T2>
        where T : class, IExprType
        where T1 : class, IExprType
        where T2 : class, IExprType { }
    
    public interface IAdd<out T> : IAdd<T, T, T>, IBinaryOperator<T>
        where T : class, IIntegralPromotionReturnType { }

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

    public sealed class PtrAddULong
        : BinaryOperator<TPointer, TPointer, TULong>,
          IAdd<TPointer, TPointer, TULong> {

        public PtrAddULong(IRValueExpr<TPointer> left, IRValueExpr<TULong> right)
            : base(left, right) { }
        
        public override TPointer Type { get; }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitPointer(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitPointer(this);
        }
    }

    public sealed class PtrAddSLong
        : BinaryOperator<TPointer, TPointer, TSLong>,
          IAdd<TPointer, TPointer, TSLong> {

        public PtrAddSLong(IRValueExpr<TPointer> left, IRValueExpr<TSLong> right)
            : base(left, right) { }

        public override TPointer Type { get; }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitPointer(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitPointer(this);
        }
    }

    public sealed class ULongAddPtr
        : BinaryOperator<TPointer, TULong, TPointer>,
          IAdd<TPointer, TULong, TPointer> {

        public ULongAddPtr(IRValueExpr<TULong> left, IRValueExpr<TPointer> right)
            : base(left, right) { }

        public override TPointer Type { get; }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitPointer(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitPointer(this);
        }
    }

    public sealed class SLongAddPtr
        : BinaryOperator<TPointer, TSLong, TPointer>,
          IAdd<TPointer, TSLong, TPointer> {

        public SLongAddPtr(IRValueExpr<TSLong> left, IRValueExpr<TPointer> right)
            : base(left, right) { }

        public override TPointer Type { get; }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitPointer(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitPointer(this);
        }
    }

    public static class Add {
        public static IAdd<IExprType> Create(IRValueExpr<IExprType> left,
                                             IRValueExpr<IExprType> right) {
            // TODO: implement this
            throw new NotImplementedException();
        }
    }
}
