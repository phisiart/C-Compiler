using System;
using System.Collections.Generic;
using System.Linq;

namespace SyntaxTree {

    public class Statement : PTNode {
        public virtual Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            throw new NotImplementedException();
        }
//		public virtual AST.Stmt Semant(ref AST.Env env, AST.Stmt parent) {
//			throw new NotImplementedException();
//		}
    }


    public class GotoStatement : Statement {
        public GotoStatement(string _label) {
            goto_label = _label;
        }
        public readonly string goto_label;
		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			return new Tuple<AST.Env, AST.Stmt>(env, new AST.GotoStmt(goto_label));
		}
    }


    public class ContinueStatement : Statement {
		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			return new Tuple<AST.Env, AST.Stmt>(env, new AST.ContStmt());
		}
    }


    public class BreakStatement : Statement {
		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			return new Tuple<AST.Env, AST.Stmt>(env, new AST.BreakStmt());
		}
    }


    public class ReturnStatement : Statement {
        public ReturnStatement(Expr _expr) {
            ret_expr = _expr;
        }
        public readonly Expr ret_expr;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr expr = ret_expr.GetExpr(env);
            expr = AST.TypeCast.MakeCast(expr, env.GetCurrentFunction().ret_type);
            return new Tuple<AST.Env, AST.Stmt>(env, new AST.ReturnStmt(expr));
        }
    }


    public class CompoundStatement : Statement {
        public CompoundStatement(List<Decln> _decl_list, List<Statement> _stmt_list) {
            stmt_declns = _decl_list;
            stmt_stmts = _stmt_list;
        }
        public readonly List<Decln> stmt_declns;
        public readonly List<Statement> stmt_stmts;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            env = env.InScope();
            List<Tuple<AST.Env, AST.Decln>> declns = new List<Tuple<AST.Env, AST.Decln>>();
            List<Tuple<AST.Env, AST.Stmt>> stmts = new List<Tuple<AST.Env, AST.Stmt>>();

            foreach (Decln decln in stmt_declns) {
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

//		public override AST.Stmt Semant(ref AST.Env env, AST.Stmt parent) {
//			return new AST.CompoundStmt(ref env, stmt_declns, stmt_stmts);
//		}
    }


    public class ExpressionStatement : Statement {
        public ExpressionStatement(Expr _expr) {
            stmt_expr = _expr;
        }
        public readonly Expr stmt_expr;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr expr = stmt_expr.GetExpr(env);
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
        public WhileStatement(Expr _cond, Statement _body) {
            while_cond = _cond;
            while_body = _body;
        }
        public readonly Expr while_cond;
        public readonly Statement while_body;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr cond;
            AST.Stmt body;

            cond = while_cond.GetExpr(env);

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
        public DoWhileStatement(Statement _body, Expr _cond) {
            do_body = _body;
            do_cond = _cond;
        }
        public readonly Statement do_body;
        public readonly Expr do_cond;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Stmt body;
            AST.Expr cond;

            Tuple<AST.Env, AST.Stmt> r_body = do_body.GetStmt(env);
            env = r_body.Item1;
            body = r_body.Item2;

            cond = do_cond.GetExpr(env);

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
        public ForStatement(Expr _init, Expr _cond, Expr _loop, Statement _body) {
            for_init = _init;
            for_cond = _cond;
            for_loop = _loop;
            for_body = _body;
        }
        public readonly Expr for_init;
        public readonly Expr for_cond;
        public readonly Expr for_loop;
        public readonly Statement for_body;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr init;
            AST.Expr cond;
            AST.Expr loop;
            AST.Stmt body;

            init = for_init.GetExpr(env);

            cond = for_cond.GetExpr(env);

            if (!cond.type.IsScalar()) {
                throw new InvalidOperationException("Error: conditional expression in while loop must be scalar.");
            }

            loop = for_loop.GetExpr(env);

            Tuple<AST.Env, AST.Stmt> r_body = for_body.GetStmt(env);
            env = r_body.Item1;
            body = r_body.Item2;

            return new Tuple<AST.Env, AST.Stmt>(env, new AST.ForStmt(init, cond, loop, body));
        }

    }


    public class SwitchStatement : Statement {
        public SwitchStatement(Expr _expr, Statement _stmt) {
            switch_expr = _expr;
            switch_stmt = _stmt;
        }
        public readonly Expr switch_expr;
        public readonly Statement switch_stmt;

		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			AST.Expr expr;
			AST.Stmt stmt;

			expr = switch_expr.GetExpr(env);

            Tuple<AST.Env, AST.Stmt> r_stmt = switch_stmt.GetStmt(env);
			env = r_stmt.Item1;
			stmt = r_stmt.Item2;

			return new Tuple<AST.Env, AST.Stmt>(env, new AST.SwitchStmt(expr, stmt));
		}
    }


    public class IfStatement : Statement {
        public IfStatement(Expr _cond, Statement _stmt) {
            if_cond = _cond;
            if_stmt = _stmt;
        }
        public readonly Expr if_cond;
        public readonly Statement if_stmt;

		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			AST.Expr cond;
			AST.Stmt stmt;

			cond = if_cond.GetExpr(env);

            if (!cond.type.IsScalar()) {
				throw new InvalidOperationException("Error: expected scalar type");
			}

			Tuple<AST.Env, AST.Stmt> r_stmt = if_stmt.GetStmt(env);
			env = r_stmt.Item1;
			stmt = r_stmt.Item2;

			return new Tuple<AST.Env, AST.Stmt>(env, new AST.IfStmt(cond, stmt));
		}
    }

    
    public class IfElseStatement : Statement {
        public IfElseStatement(Expr _cond, Statement _true_stmt, Statement _false_stmt) {
            if_cond = _cond;
            if_true_stmt = _true_stmt;
            if_false_stmt = _false_stmt;
        }
        public readonly Expr if_cond;
        public readonly Statement if_true_stmt;
        public readonly Statement if_false_stmt;

		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			AST.Expr cond;
			AST.Stmt true_stmt;
			AST.Stmt false_stmt;

			cond = if_cond.GetExpr(env);

            if (!cond.type.IsScalar()) {
				throw new InvalidOperationException("Error: expected scalar type");
			}

			Tuple<AST.Env, AST.Stmt> r_true_stmt = if_true_stmt.GetStmt(env);
			env = r_true_stmt.Item1;
			true_stmt = r_true_stmt.Item2;

			Tuple<AST.Env, AST.Stmt> r_false_stmt = if_false_stmt.GetStmt(env);
			env = r_false_stmt.Item1;
			false_stmt = r_false_stmt.Item2;

			return new Tuple<AST.Env, AST.Stmt>(env, new AST.IfElseStmt(cond, true_stmt, false_stmt));
		}
    }


	public class LabeledStatement : Statement {
        public LabeledStatement(string _label, Statement _stmt) {
            label = _label;
            stmt = _stmt;
        }
        public readonly string label;
        public readonly Statement stmt;

		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			Tuple<AST.Env, AST.Stmt> r_stmt = stmt.GetStmt(env);
			env = r_stmt.Item1;
			return new Tuple<AST.Env, AST.Stmt>(env, new AST.LabeledStmt(label, r_stmt.Item2));
		}
    }


	public class CaseStatement : Statement {
        public CaseStatement(Expr _expr, Statement _stmt) {
            expr = _expr;
            stmt = _stmt;
        }
        // expr == null means 'default'
        public readonly Expr expr;
        public readonly Statement stmt;

		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			if (expr == null) {
				Tuple<AST.Env, AST.Stmt> r_stmt = stmt.GetStmt(env);
				env = r_stmt.Item1;
				return new Tuple<AST.Env, AST.Stmt>(env, new AST.DefaultStmt(r_stmt.Item2));
			
			} else {

                AST.Expr case_expr = expr.GetExpr(env);
				//Tuple<AST.Env, AST.Expr> r_expr = expr.GetExprEnv(env);
				//env = r_expr.Item1;

				case_expr = AST.TypeCast.MakeCast(case_expr, new AST.TLong());
				if (!case_expr.IsConstExpr()) {
					throw new InvalidOperationException("case expr not const");
				}
				Int32 case_value = ((AST.ConstLong)case_expr).value;

				Tuple<AST.Env, AST.Stmt> r_stmt = stmt.GetStmt(env);
				env = r_stmt.Item1;

				return new Tuple<AST.Env, AST.Stmt>(env, new AST.CaseStmt(case_value, r_stmt.Item2));
			}

		}
    }

}