using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SyntaxTree {
    public interface ISemantReturn<out T> {
        T Value { get; }
        AST.Env Env { get; }
    }

    public class SemantReturn<T> : ISemantReturn<T> {
        protected SemantReturn(AST.Env env, T value) {
            this.Value = value;
            this.Env = env;
        }

        public static ISemantReturn<T> Create(AST.Env env, T value) =>
            new SemantReturn<T>(env, value);

        public T Value { get; }
        public AST.Env Env { get; }
    }

    public class SemantReturn {
        public static ISemantReturn<T> Create<T>(AST.Env env, T value) =>
            SemantReturn<T>.Create(env, value);
    }

    public static class SemanticAnalysis {
        public class SemantMethod : System.Attribute { }

        public static R Semant<R>(Func<AST.Env, ISemantReturn<R>> semantFunc, ref AST.Env env) {
            var semantReturn = semantFunc(env);
            env = semantReturn.Env;
            return semantReturn.Value;
        }

        public static R Semant<I, R>(Func<AST.Env, I, ISemantReturn<R>> semantFunc, I arg, ref AST.Env env) {
            var semantReturn = semantFunc(env, arg);
            env = semantReturn.Env;
            return semantReturn.Value;
        }

        public static AST.Expr SemantExpr(Func<AST.Env, AST.Expr> semantFunc, ref AST.Env env) {
            var semantReturn = semantFunc(env);
            env = semantReturn.Env;
            return semantReturn;
        }

        public static AST.Stmt SemantStmt(Func<AST.Env, Tuple<AST.Env, AST.Stmt>> semantFunc, ref AST.Env env) {
            var semantReturn = semantFunc(env);
            env = semantReturn.Item1;
            return semantReturn.Item2;
        }
    }

}