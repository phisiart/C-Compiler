using ABT2.TypeSystem;

namespace ABT2.Expressions {
    public abstract class FloatBinaryOperator : BinaryOperator<TFloat> {
        protected FloatBinaryOperator(IRValueExpr<TFloat> left, IRValueExpr<TFloat> right)
            : base(left, right) { }

        public override sealed TFloat Type => TFloat.Get;

        public override sealed void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitFloat(this);
        }

        public override sealed R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitFloat(this);
        }
    }
}