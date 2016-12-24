using ABT2.TypeSystem;
using ABT2.Environment;

namespace ABT2.Expressions {
    public interface IBinaryOperator<out T> : IRValueExpr<T> where T : IExprType {
        IRValueExpr<T> Left { get; }

        IRValueExpr<T> Right { get; }
    }

    public abstract class BinaryOperator<T> : IBinaryOperator<T> where T : IExprType {
        public BinaryOperator(IRValueExpr<T> left, IRValueExpr<T> right) {
            this.Left = left;
            this.Right = right;
        }

        public IRValueExpr<T> Left { get; }

        public IRValueExpr<T> Right { get; }

        public abstract T Type { get; }

        public Env Env => this.Right.Env;

        public abstract void Visit(IRValueExprByTypeVisitor visitor);

        public abstract R Visit<R>(IRValueExprByTypeVisitor<R> visitor);
    }
}