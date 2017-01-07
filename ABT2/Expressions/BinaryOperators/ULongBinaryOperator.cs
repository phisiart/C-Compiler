using ABT2.TypeSystem;

namespace ABT2.Expressions {
    public abstract class ULongBinaryOperator : BinaryOperator<TULong> {
        protected ULongBinaryOperator(IRValueExpr<TULong> left, IRValueExpr<TULong> right)
            : base(left, right) { }

        public override sealed TULong Type => TULong.Get;

        public override sealed void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitULong(this);
        }

        public override sealed R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitULong(this);
        }
    }
}
