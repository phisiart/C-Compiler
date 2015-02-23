using System;
using System.Collections.Generic;

namespace AST {
    public class Stmt {
        public virtual void CGenStmt(Env env, CGenState state) {
            throw new NotImplementedException();
        }
    }

    public class ExprStmt : Stmt {
        public ExprStmt(Expr expr) {
            stmt_expr = expr;
        }
        public readonly Expr stmt_expr;
    }

    public class CompoundStmt : Stmt {
        public CompoundStmt(List<Tuple<Env, Decln>> declns, List<Tuple<Env, Stmt>> stmts) {
            stmt_declns = declns;
            stmt_stmts = stmts;
        }

        public readonly List<Tuple<Env, Decln>> stmt_declns;
        public readonly List<Tuple<Env, Stmt>> stmt_stmts;

        public override void CGenStmt(Env env, CGenState state) {
            foreach (Tuple<Env, Decln> decln in stmt_declns) {
                decln.Item2.CGenExternDecln(decln.Item1, state);
            }
            foreach (Tuple<Env, Stmt> stmt in stmt_stmts) {
                stmt.Item2.CGenStmt(stmt.Item1, state);
            }
        }
    }

    public class ReturnStmt : Stmt {
        public ReturnStmt(Expr expr) {
            stmt_expr = expr;
        }
        public readonly Expr stmt_expr;

        public override void CGenStmt(Env env, CGenState state) {

        }
    }
}