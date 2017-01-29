using ABT2.TypeSystem;

namespace ABT2.Expressions {
    public abstract class SLongBinaryOperator : BinaryOperator<TSLong> {
        protected SLongBinaryOperator(IRValueExpr<TSLong> left, IRValueExpr<TSLong> right)
            : base(left, right) { }

        public override TSLong Type => TSLong.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSLong(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSLong(this);
        }
    }
}
