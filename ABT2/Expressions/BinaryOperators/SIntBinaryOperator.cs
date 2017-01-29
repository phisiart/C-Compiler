using ABT2.TypeSystem;

namespace ABT2.Expressions {
    public abstract class SIntBinaryOperator : BinaryOperator<TSInt> {
        protected SIntBinaryOperator(IRValueExpr<TSInt> left,
                                     IRValueExpr<TSInt> right)
            : base(left, right) { }

        public override TSInt Type => TSInt.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSInt(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSInt(this);
        }
    }
}
