using System;
using System.Collections.Generic;
using System.Linq;

namespace SyntaxTree {

    public class Stmt : PTNode {
        public virtual Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            throw new NotImplementedException();
        }
    }


    public class GotoStatement : Stmt {
        public GotoStatement(String _label) {
            goto_label = _label;
        }
        public readonly String goto_label;
		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			return new Tuple<AST.Env, AST.Stmt>(env, new AST.GotoStmt(goto_label));
		}
    }


    public class ContinueStatement : Stmt {
		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			return new Tuple<AST.Env, AST.Stmt>(env, new AST.ContStmt());
		}
    }


    public class BreakStatement : Stmt {
		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			return new Tuple<AST.Env, AST.Stmt>(env, new AST.BreakStmt());
		}
    }


    public class ReturnStatement : Stmt {
        public ReturnStatement(Expr expr) {
            this.expr = expr;
        }

        // TODO: change this into Option. Currently parser might give null.
        public readonly Expr expr;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);
            expr = AST.TypeCast.MakeCast(expr, env.GetCurrentFunction().ret_type);
            return new Tuple<AST.Env, AST.Stmt>(env, new AST.ReturnStmt(expr));
        }
    }


    public class CompoundStatement : Stmt {
        public CompoundStatement(List<Decln> _decl_list, List<Stmt> _stmt_list) {
            stmt_declns = _decl_list;
            stmt_stmts = _stmt_list;
        }
        public readonly List<Decln> stmt_declns;
        public readonly List<Stmt> stmt_stmts;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            env = env.InScope();
            List<Tuple<AST.Env, AST.Decln>> declns = new List<Tuple<AST.Env, AST.Decln>>();
            List<Tuple<AST.Env, AST.Stmt>> stmts = new List<Tuple<AST.Env, AST.Stmt>>();

            foreach (Decln decln in stmt_declns) {
                Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_decln = decln.GetDeclns(env);
                env = r_decln.Item1;
                declns.AddRange(r_decln.Item2);
            }

            foreach (Stmt stmt in stmt_stmts) {
                Tuple<AST.Env, AST.Stmt> r_stmt = stmt.GetStmt(env);
                env = r_stmt.Item1;
                stmts.Add(r_stmt);
            }

            env = env.OutScope();

            return new Tuple<AST.Env, AST.Stmt>(env, new AST.CompoundStmt(declns, stmts));

        }

    }


    public class ExpressionStatement : Stmt {
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
    public class WhileStatement : Stmt {
        public WhileStatement(Expr _cond, Stmt _body) {
            while_cond = _cond;
            while_body = _body;
        }
        public readonly Expr while_cond;
        public readonly Stmt while_body;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr cond;
            AST.Stmt body;

            cond = while_cond.GetExpr(env);

            if (!cond.type.IsScalar) {
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
    public class DoWhileStatement : Stmt {
        public DoWhileStatement(Stmt _body, Expr _cond) {
            do_body = _body;
            do_cond = _cond;
        }
        public readonly Stmt do_body;
        public readonly Expr do_cond;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Stmt body;
            AST.Expr cond;

            Tuple<AST.Env, AST.Stmt> r_body = do_body.GetStmt(env);
            env = r_body.Item1;
            body = r_body.Item2;

            cond = do_cond.GetExpr(env);

            if (!cond.type.IsScalar) {
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
    public class ForStmt : Stmt {
        public ForStmt(Option<Expr> init, Option<Expr> cond, Option<Expr> loop, Stmt body) {
            this.init = init;
            this.cond = cond;
            this.loop = loop;
            this.body = body;
        }

        public readonly Option<Expr> init;
        public readonly Option<Expr> cond;
        public readonly Option<Expr> loop;
        public readonly Stmt body;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            Option<AST.Expr> init;
            Option<AST.Expr> cond;
            Option<AST.Expr> loop;
            AST.Stmt body;

            init = this.init.Map(_ => _.GetExpr(env));

            cond = this.cond.Map(_ => _.GetExpr(env));

            if (cond.IsSome && !cond.Value.type.IsScalar) {
                throw new InvalidOperationException("Error: conditional expression in while loop must be scalar.");
            }

            loop = this.loop.Map(_ => _.GetExpr(env));

            Tuple<AST.Env, AST.Stmt> r_body = this.body.GetStmt(env);
            env = r_body.Item1;
            body = r_body.Item2;

            return new Tuple<AST.Env, AST.Stmt>(env, new AST.ForStmt(init, cond, loop, body));
        }

    }


    public class SwitchStatement : Stmt {
        public SwitchStatement(Expr _expr, Stmt _stmt) {
            switch_expr = _expr;
            switch_stmt = _stmt;
        }
        public readonly Expr switch_expr;
        public readonly Stmt switch_stmt;

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


    public class IfStatement : Stmt {
        public IfStatement(Expr _cond, Stmt _stmt) {
            if_cond = _cond;
            if_stmt = _stmt;
        }
        public readonly Expr if_cond;
        public readonly Stmt if_stmt;

		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			AST.Expr cond;
			AST.Stmt stmt;

			cond = if_cond.GetExpr(env);

            if (!cond.type.IsScalar) {
				throw new InvalidOperationException("Error: expected scalar type");
			}

			Tuple<AST.Env, AST.Stmt> r_stmt = if_stmt.GetStmt(env);
			env = r_stmt.Item1;
			stmt = r_stmt.Item2;

			return new Tuple<AST.Env, AST.Stmt>(env, new AST.IfStmt(cond, stmt));
		}
    }

    
    public class IfElseStatement : Stmt {
        public IfElseStatement(Expr _cond, Stmt _true_stmt, Stmt _false_stmt) {
            if_cond = _cond;
            if_true_stmt = _true_stmt;
            if_false_stmt = _false_stmt;
        }
        public readonly Expr if_cond;
        public readonly Stmt if_true_stmt;
        public readonly Stmt if_false_stmt;

		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			AST.Expr cond;
			AST.Stmt true_stmt;
			AST.Stmt false_stmt;

			cond = if_cond.GetExpr(env);

            if (!cond.type.IsScalar) {
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


	public class LabeledStatement : Stmt {
        public LabeledStatement(String _label, Stmt _stmt) {
            label = _label;
            stmt = _stmt;
        }
        public readonly String label;
        public readonly Stmt stmt;

		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			Tuple<AST.Env, AST.Stmt> r_stmt = stmt.GetStmt(env);
			env = r_stmt.Item1;
			return new Tuple<AST.Env, AST.Stmt>(env, new AST.LabeledStmt(label, r_stmt.Item2));
		}
    }


	public class CaseStatement : Stmt {
        public CaseStatement(Expr _expr, Stmt _stmt) {
            expr = _expr;
            stmt = _stmt;
        }
        // expr == null means 'default'
        // TODO: change this to Option
        public readonly Expr expr;
        public readonly Stmt stmt;

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
				if (!case_expr.IsConstExpr) {
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