using System;
using System.Collections.Generic;

namespace AST {
    public class Stmt {

    }

    public class ExprStmt : Stmt {
        public ExprStmt(Expr expr) {
            stmt_expr = expr;
        }
        public readonly Expr stmt_expr;
    }

    public class CompoundStmt : Stmt {
        public CompoundStmt(List<Tuple<AST.Env, AST.Decln>> declns, List<Tuple<AST.Env, AST.Stmt>> stmts) {
            stmt_declns = declns;
            stmt_stmts = stmts;
        }

        public readonly List<Tuple<AST.Env, AST.Decln>> stmt_declns;
        public readonly List<Tuple<AST.Env, AST.Stmt>> stmt_stmts;
    }
}