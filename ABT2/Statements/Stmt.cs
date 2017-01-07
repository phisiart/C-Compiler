using System;
using System.Collections.Immutable;
using ABT2.Environment;
using ABT2.TypeSystem;

namespace ABT2 {
    public abstract class Stmt {
        public abstract Env Env { get; }
    }

    public sealed class CompStmt : Stmt {
        public CompStmt(ImmutableList<Stmt> stmts, Env env) {
            this.Stmts = stmts;
            this.Env = env;
        }

        public ImmutableList<Stmt> Stmts { get; }

        public override Env Env { get; }
    }
}
