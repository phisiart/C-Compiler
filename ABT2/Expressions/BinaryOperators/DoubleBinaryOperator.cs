using ABT2.TypeSystem;

namespace ABT2.Expressions {
    public abstract class DoubleBinaryOperator : BinaryOperator<TDouble> {
        protected DoubleBinaryOperator(IRValueExpr<TDouble> left, IRValueExpr<TDouble> right)
            : base(left, right) { }

        public override sealed TDouble Type => TDouble.Get;

        public override sealed void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitDouble(this);
        }

        public override sealed R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitDouble(this);
        }
    }
}
