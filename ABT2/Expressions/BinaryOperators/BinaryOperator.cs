using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions {
    public interface IBinaryOperator : IRValueExpr {
        IRValueExpr Left { get; }

        IRValueExpr Right { get; }
    }

    public interface IBinaryOperator<out T, out T1, out T2>
        : IBinaryOperator, IRValueExpr<T>
        where T : class, IExprType
        where T1 : class, IExprType
        where T2 : class, IExprType {

        new IRValueExpr<T1> Left { get; }

        new IRValueExpr<T2> Right { get; }
    }

    public abstract class BinaryOperator<T, T1, T2>
        : RValueExpr<T>, IBinaryOperator<T, T1, T2>
        where T : class, IExprType
        where T1 : class, IExprType
        where T2 : class, IExprType {

        protected BinaryOperator(IRValueExpr<T1> left, IRValueExpr<T2> right) {
            this.Left = left;
            this.Right = right;
        }

        public IRValueExpr<T1> Left { get; }

        public IRValueExpr<T2> Right { get; }

        IRValueExpr IBinaryOperator.Left => this.Left;

        IRValueExpr IBinaryOperator.Right => this.Right;

        public override Env Env => this.Right.Env;
    }

    public interface IBinaryOperator<out T>
        : IBinaryOperator<T, T, T> where T : class, IExprType {

        new IRValueExpr<T> Left { get; }

        new IRValueExpr<T> Right { get; }
    }

    public abstract class BinaryOperator<T>
        : BinaryOperator<T, T, T>, IBinaryOperator<T> where T : class, IExprType {

        protected BinaryOperator(IRValueExpr<T> left, IRValueExpr<T> right)
            : base(left, right) { }
    }
}