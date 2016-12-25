using ABT2.TypeSystem;
using ABT2.Environment;

namespace ABT2.Expressions {
    public abstract class ULongBinaryOperator : BinaryOperator<TULong> {
        public ULongBinaryOperator(IRValueExpr<TULong> left, IRValueExpr<TULong> right)
            : base(left, right) { }

        public override TULong Type => TULong.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitULong(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitULong(this);
        }
    }
}
