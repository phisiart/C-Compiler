using ABT2.TypeSystem;
using ABT2.Environment;

namespace ABT2.Expressions {
    public abstract class UIntBinaryOperator : BinaryOperator<TUInt> {
        public UIntBinaryOperator(IRValueExpr<TUInt> left, IRValueExpr<TUInt> right)
            : base(left, right) { }

        public override TUInt Type => TUInt.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitUInt(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitUInt(this);
        }
    }
}