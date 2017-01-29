using ABT2.Environment;
using ABT2.TypeSystem;

namespace ABT2.Expressions {
    public interface ILogicalAnd : IBinaryOperator, IRValueExpr<TSInt> { }

    public interface INonZero : IRValueExpr<TSInt> { }

    public interface INonZero<out T> : INonZero { }

    public sealed class LogicalAnd : ILogicalAnd {
        public LogicalAnd(INonZero left, INonZero right) {
            this.Left = left;
            this.Right = right;
        }

        public Env Env => this.Right.Env;

        public TSInt Type => TSInt.Get;

        IExprType IRValueExpr.Type => this.Type;

        public INonZero Left { get; }

        public INonZero Right { get; }

        IRValueExpr IBinaryOperator.Left => this.Left;

        IRValueExpr IBinaryOperator.Right => this.Right;

        public void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSInt(this);
        }

        public R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSInt(this);
        }
    }
}
