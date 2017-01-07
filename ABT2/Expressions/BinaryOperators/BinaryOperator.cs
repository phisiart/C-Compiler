using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions {
    public interface IBinaryOperator : IRValueExpr {
        IRValueExpr Left { get; }

        IRValueExpr Right { get; }
    }

    public interface IBinaryOperator<out T>
        : IBinaryOperator, IRValueExpr<T> where T : IExprType {

        new IRValueExpr<T> Left { get; }

        new IRValueExpr<T> Right { get; }
    }

    public abstract class BinaryOperator<T>
        : RValueExpr<T>, IBinaryOperator<T> where T : IExprType {

        protected BinaryOperator(IRValueExpr<T> left, IRValueExpr<T> right) {
            this.Left = left;
            this.Right = right;
        }

        public IRValueExpr<T> Left { get; }
        IRValueExpr IBinaryOperator.Left => this.Left;

        public IRValueExpr<T> Right { get; }
        IRValueExpr IBinaryOperator.Right => this.Right;

        //public abstract T Type { get; }
        //IExprType IRValueExpr.Type => this.Type;

        public override Env Env => this.Right.Env;

        //public abstract void Visit(IRValueExprByTypeVisitor visitor);

        //public abstract R Visit<R>(IRValueExprByTypeVisitor<R> visitor);
    }
}