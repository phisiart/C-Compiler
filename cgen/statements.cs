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

		public override void CGenStmt(Env env, CGenState state) {
			stmt_expr.CGenValue(env, state);
		}
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

    public class WhileStmt : Stmt {
        public WhileStmt(Expr _cond, Stmt _body) {
            while_cond = _cond;
            while_body = _body;
        }
        public readonly Expr while_cond;
        public readonly Stmt while_body;
    }

    public class DoWhileStmt : Stmt {
        public DoWhileStmt(Stmt _body, Expr _expr) {
            do_body = _body;
            do_expr = _expr;
        }
        public readonly Stmt do_body;
        public readonly Expr do_expr;
    }

    public class ForStmt : Stmt {
        public ForStmt(Expr _init, Expr _cond, Expr _loop, Stmt _body) {
            for_init = _init;
            for_cond = _cond;
            for_loop = _loop;
            for_body = _body;
        }
        public readonly Expr for_init;
        public readonly Expr for_cond;
        public readonly Expr for_loop;
        public readonly Stmt for_body;
    }
}