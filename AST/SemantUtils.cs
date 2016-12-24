using System;

namespace AST {
    public interface ISemantReturn<out T> {
        T Value { get; }
        ABT.Env Env { get; }
    }

    public class SemantReturn<T> : ISemantReturn<T> {
        protected SemantReturn(ABT.Env env, T value) {
            this.Value = value;
            this.Env = env;
        }

        public static ISemantReturn<T> Create(ABT.Env env, T value) =>
            new SemantReturn<T>(env, value);

        public T Value { get; }

        public ABT.Env Env { get; }
    }

    public static class SemantReturn {
        public static ISemantReturn<T> Create<T>(ABT.Env env, T value) =>
            SemantReturn<T>.Create(env, value);
    }

    public static class SemanticAnalysis {
        public class SemantMethod : System.Attribute { }

        public static R Semant<R>(
            Func<ABT.Env, ISemantReturn<R>> semantFunc,
            ref ABT.Env env
        ) {
            var semantReturn = semantFunc(env);
            env = semantReturn.Env;
            return semantReturn.Value;
        }

        public static R Semant<I, R>(
            Func<ABT.Env, I, ISemantReturn<R>> semantFunc,
            I arg,
            ref ABT.Env env
        ) {
            var semantReturn = semantFunc(env, arg);
            env = semantReturn.Env;
            return semantReturn.Value;
        }

        public static ABT.Expr SemantExpr(
            Func<ABT.Env, ABT.Expr> semantFunc,
            ref ABT.Env env
        ) {
            var semantReturn = semantFunc(env);
            env = semantReturn.Env;
            return semantReturn;
        }

        public static ABT.Expr SemantExpr(Expr expr, ref ABT.Env env) =>
            SemantExpr(expr.GetExpr, ref env);

        public static ABT.Stmt SemantStmt(
            Func<ABT.Env, Tuple<ABT.Env, ABT.Stmt>> semantFunc,
            ref ABT.Env env
        ) {
            var semantReturn = semantFunc(env);
            env = semantReturn.Item1;
            return semantReturn.Item2;
        }
    }

}