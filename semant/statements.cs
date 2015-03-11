using System;
using System.Collections.Generic;
using System.Linq;

namespace SyntaxTree {

    public class Statement : PTNode {
        public virtual Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            throw new NotImplementedException();
        }
    }


    public class GotoStatement : Statement {
        public GotoStatement(String _label) {
            label = _label;
        }
        public String label;

    }


    public class ContinueStatement : Statement {
    }


    public class BreakStatement : Statement {
    }


    public class ReturnStatement : Statement {
        public ReturnStatement(Expression _expr) {
            ret_expr = _expr;
        }
        public Expression ret_expr;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = ret_expr.GetExpr(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;
            expr = AST.TypeCast.MakeCast(expr, env.GetCurrentFunction().ret_type);
            return new Tuple<AST.Env, AST.Stmt>(env, new AST.ReturnStmt(expr));
        }
    }


    public class CompoundStatement : Statement {
        public CompoundStatement(List<Declaration> _decl_list, List<Statement> _stmt_list) {
            stmt_declns = _decl_list;
            stmt_stmts = _stmt_list;
        }
        List<Declaration> stmt_declns;
        List<Statement> stmt_stmts;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            env = env.InScope();
            List<Tuple<AST.Env, AST.Decln>> declns = new List<Tuple<AST.Env, AST.Decln>>();
            List<Tuple<AST.Env, AST.Stmt>> stmts = new List<Tuple<AST.Env, AST.Stmt>>();

            foreach (Declaration decln in stmt_declns) {
                Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_decln = decln.GetDeclns(env);
                env = r_decln.Item1;
                declns.AddRange(r_decln.Item2);
            }

            foreach (Statement stmt in stmt_stmts) {
                Tuple<AST.Env, AST.Stmt> r_stmt = stmt.GetStmt(env);
                env = r_stmt.Item1;
                stmts.Add(r_stmt);
            }

            env = env.OutScope();

            return new Tuple<AST.Env, AST.Stmt>(env, new AST.CompoundStmt(declns, stmts));

        }

    }


    public class ExpressionStatement : Statement {
        public ExpressionStatement(Expression _expr) {
            stmt_expr = _expr;
        }
        public Expression stmt_expr;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = stmt_expr.GetExpr(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            return new Tuple<AST.Env, AST.Stmt>(env, new AST.ExprStmt(expr));
        }
    }


    /// <summary>
    /// while (cond) {
    ///     body
    /// }
    /// 
    /// cond must be of scalar type
    /// </summary>
    public class WhileStatement : Statement {
        public WhileStatement(Expression _cond, Statement _body) {
            while_cond = _cond;
            while_body = _body;
        }
        public readonly Expression while_cond;
        public readonly Statement while_body;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr cond;
            AST.Stmt body;

            Tuple<AST.Env, AST.Expr> r_cond = while_cond.GetExpr(env);
            env = r_cond.Item1;
            cond = r_cond.Item2;

            if (!cond.type.IsScalar()) {
                throw new InvalidOperationException("Error: conditional expression in while loop must be scalar.");
            }

            Tuple<AST.Env, AST.Stmt> r_body = while_body.GetStmt(env);
            env = r_body.Item1;
            body = r_body.Item2;

            return new Tuple<AST.Env, AST.Stmt>(env, new AST.WhileStmt(cond, body));
        }

    }


    /// <summary>
    /// do {
    ///     body
    /// } while (cond);
    /// 
    /// cond must be of scalar type
    /// </summary>
    public class DoWhileStatement : Statement {
        public DoWhileStatement(Statement _body, Expression _cond) {
            do_body = _body;
            do_cond = _cond;
        }
        public readonly Statement do_body;
        public readonly Expression do_cond;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Stmt body;
            AST.Expr cond;

            Tuple<AST.Env, AST.Stmt> r_body = do_body.GetStmt(env);
            env = r_body.Item1;
            body = r_body.Item2;

            Tuple<AST.Env, AST.Expr> r_cond = do_cond.GetExpr(env);
            env = r_cond.Item1;
            cond = r_cond.Item2;

            if (!cond.type.IsScalar()) {
                throw new InvalidOperationException("Error: conditional expression in while loop must be scalar.");
            }

            return new Tuple<AST.Env, AST.Stmt>(env, new AST.DoWhileStmt(body, cond));
        }
    }


    /// <summary>
    /// for (init; cond; loop) {
    ///     body
    /// }
    /// 
    /// cond must be of scalar type
    /// </summary>
    public class ForStatement : Statement {
        public ForStatement(Expression _init, Expression _cond, Expression _loop, Statement _body) {
            for_init = _init;
            for_cond = _cond;
            for_loop = _loop;
            for_body = _body;
        }
        public readonly Expression for_init;
        public readonly Expression for_cond;
        public readonly Expression for_loop;
        public readonly Statement for_body;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr init;
            AST.Expr cond;
            AST.Expr loop;
            AST.Stmt body;

            Tuple<AST.Env, AST.Expr> r_init = for_init.GetExpr(env);
            env = r_init.Item1;
            init = r_init.Item2;

            Tuple<AST.Env, AST.Expr> r_cond = for_cond.GetExpr(env);
            env = r_cond.Item1;
            cond = r_cond.Item2;

            if (!cond.type.IsScalar()) {
                throw new InvalidOperationException("Error: conditional expression in while loop must be scalar.");
            }

            Tuple<AST.Env, AST.Expr> r_loop = for_loop.GetExpr(env);
            env = r_loop.Item1;
            loop = r_loop.Item2;

            Tuple<AST.Env, AST.Stmt> r_body = for_body.GetStmt(env);
            env = r_body.Item1;
            body = r_body.Item2;

            return new Tuple<AST.Env, AST.Stmt>(env, new AST.ForStmt(init, cond, loop, body));
        }

    }


    public class SwitchStatement : Statement {
        public SwitchStatement(Expression _expr, Statement _stmt) {
            expr = _expr;
            stmt = _stmt;
        }
        public Expression expr;
        public Statement stmt;

    }


    public class IfStatement : Statement {
        public IfStatement(Expression _cond, Statement _stmt) {
            cond = _cond;
            stmt = _stmt;
        }
        public Expression cond;
        public Statement stmt;

    }

    // Finished.
    public class IfElseStatement : Statement {
        public IfElseStatement(Expression _cond, Statement _true_stmt, Statement _false_stmt) {
            cond = _cond;
            true_stmt = _true_stmt;
            false_stmt = _false_stmt;
        }
        public Expression cond;
        public Statement true_stmt;
        public Statement false_stmt;

    }

    // Finished.
    public class LabeledStatement : Statement {
        public LabeledStatement(String _label, Statement _stmt) {
            label = _label;
            stmt = _stmt;
        }
        public String label;
        public Statement stmt;

    }

    // Finished.
    public class CaseStatement : Statement {
        public CaseStatement(Expression _expr, Statement _stmt) {
            expr = _expr;
            stmt = _stmt;
        }
        // expr == null means 'default'
        public Expression expr;
        public Statement stmt;

    }

}