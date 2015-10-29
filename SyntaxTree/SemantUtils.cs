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
}