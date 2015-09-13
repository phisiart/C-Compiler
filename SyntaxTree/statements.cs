using System;
using System.Collections.Generic;

namespace SyntaxTree {

    public abstract class Stmt : PTNode {
        public abstract Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env);
    }


    public class GotoStmt : Stmt {
        public GotoStmt(String label) {
            this.label = label;
        }
        public readonly String label;
		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			return new Tuple<AST.Env, AST.Stmt>(env, new AST.GotoStmt(this.label));
		}
    }


    public class ContStmt : Stmt {
		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			return new Tuple<AST.Env, AST.Stmt>(env, new AST.ContStmt());
		}
    }


    public class BreakStmt : Stmt {
		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			return new Tuple<AST.Env, AST.Stmt>(env, new AST.BreakStmt());
		}
    }


    public class ReturnStmt : Stmt {
        public ReturnStmt(Expr expr) {
            this.expr = expr;
        }

        // TODO: change this into Option. Currently parser might give null.
        public readonly Expr expr;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);
            expr = AST.TypeCast.MakeCast(expr, env.GetCurrentFunction().ret_t);
            return new Tuple<AST.Env, AST.Stmt>(env, new AST.ReturnStmt(expr));
        }
    }


    public class CompoundStmt : Stmt {
        public CompoundStmt(List<Decln> declns, List<Stmt> stmts) {
            this.declns = declns;
            this.stmts = stmts;
        }
        public readonly List<Decln> declns;
        public readonly List<Stmt> stmts;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            env = env.InScope();
            List<Tuple<AST.Env, AST.Decln>> declns = new List<Tuple<AST.Env, AST.Decln>>();
            List<Tuple<AST.Env, AST.Stmt>> stmts = new List<Tuple<AST.Env, AST.Stmt>>();

            foreach (Decln decln in this.declns) {
                Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_decln = decln.GetDeclns(env);
                env = r_decln.Item1;
                declns.AddRange(r_decln.Item2);
            }

            foreach (Stmt stmt in this.stmts) {
                Tuple<AST.Env, AST.Stmt> r_stmt = stmt.GetStmt(env);
                env = r_stmt.Item1;
                stmts.Add(r_stmt);
            }

            env = env.OutScope();

            return new Tuple<AST.Env, AST.Stmt>(env, new AST.CompoundStmt(declns, stmts));
        }
    }


    public class ExprStmt : Stmt {
        public ExprStmt(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);
            env = expr.Env;
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
    public class WhileStmt : Stmt {
        public WhileStmt(Expr cond, Stmt body) {
            this.cond = cond;
            this.body = body;
        }
        public readonly Expr cond;
        public readonly Stmt body;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr cond = this.cond.GetExpr(env);
            env = cond.Env;

            if (!cond.type.IsScalar) {
                throw new InvalidOperationException("Error: conditional expression in while loop must be scalar.");
            }

            Tuple<AST.Env, AST.Stmt> r_body = this.body.GetStmt(env);
            env = r_body.Item1;
            AST.Stmt body = r_body.Item2;

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
    public class DoWhileStmt : Stmt {
        public DoWhileStmt(Stmt body, Expr cond) {
            this.body = body;
            this.cond = cond;
        }
        public readonly Stmt body;
        public readonly Expr cond;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            Tuple<AST.Env, AST.Stmt> r_body = this.body.GetStmt(env);
            env = r_body.Item1;
            AST.Stmt body = r_body.Item2;

            AST.Expr cond = this.cond.GetExpr(env);
            env = cond.Env;

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
            Option<AST.Expr> init = this.init.Map(_ => _.GetExpr(env));
            if (init.IsSome) {
                env = init.Value.Env;
            }

            Option<AST.Expr> cond = this.cond.Map(_ => _.GetExpr(env));
            if (cond.IsSome) {
                env = cond.Value.Env;
            }

            if (cond.IsSome && !cond.Value.type.IsScalar) {
                throw new InvalidOperationException("Error: conditional expression in while loop must be scalar.");
            }

            Option<AST.Expr> loop = this.loop.Map(_ => _.GetExpr(env));
            if (loop.IsSome) {
                env = loop.Value.Env;
            }

            Tuple<AST.Env, AST.Stmt> r_body = this.body.GetStmt(env);
            env = r_body.Item1;
            AST.Stmt body = r_body.Item2;

            return new Tuple<AST.Env, AST.Stmt>(env, new AST.ForStmt(init, cond, loop, body));
        }

    }


    public class SwitchStmt : Stmt {
        public SwitchStmt(Expr expr, Stmt stmt) {
            this.expr = expr;
            this.stmt = stmt;
        }
        public readonly Expr expr;
        public readonly Stmt stmt;

		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
		    AST.Expr expr = this.expr.GetExpr(env);

            Tuple<AST.Env, AST.Stmt> r_stmt = this.stmt.GetStmt(env);
			env = r_stmt.Item1;
			AST.Stmt stmt = r_stmt.Item2;

			return new Tuple<AST.Env, AST.Stmt>(env, new AST.SwitchStmt(expr, stmt));
		}
    }


    public class IfStmt : Stmt {
        public IfStmt(Expr cond, Stmt stmt) {
            this.cond = cond;
            this.stmt = stmt;
        }
        public readonly Expr cond;
        public readonly Stmt stmt;

		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
		    AST.Expr cond = this.cond.GetExpr(env);

            if (!cond.type.IsScalar) {
				throw new InvalidOperationException("Error: expected scalar type");
			}

			Tuple<AST.Env, AST.Stmt> r_stmt = this.stmt.GetStmt(env);
			env = r_stmt.Item1;
			AST.Stmt stmt = r_stmt.Item2;

			return new Tuple<AST.Env, AST.Stmt>(env, new AST.IfStmt(cond, stmt));
		}
    }

    
    public class IfElseStmt : Stmt {
        public IfElseStmt(Expr cond, Stmt true_stmt, Stmt false_stmt) {
            this.cond = cond;
            this.true_stmt = true_stmt;
            this.false_stmt = false_stmt;
        }
        public readonly Expr cond;
        public readonly Stmt true_stmt;
        public readonly Stmt false_stmt;

		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
		    AST.Expr cond = this.cond.GetExpr(env);

            if (!cond.type.IsScalar) {
				throw new InvalidOperationException("Error: expected scalar type");
			}

			Tuple<AST.Env, AST.Stmt> r_true_stmt = this.true_stmt.GetStmt(env);
			env = r_true_stmt.Item1;
			AST.Stmt true_stmt = r_true_stmt.Item2;

			Tuple<AST.Env, AST.Stmt> r_false_stmt = this.false_stmt.GetStmt(env);
			env = r_false_stmt.Item1;
			AST.Stmt false_stmt = r_false_stmt.Item2;

			return new Tuple<AST.Env, AST.Stmt>(env, new AST.IfElseStmt(cond, true_stmt, false_stmt));
		}
    }

    /// <summary>
    /// label:
    ///     stmt
    /// </summary>
	public class LabeledStmt : Stmt {
        public LabeledStmt(String label, Stmt stmt) {
            this.label = label;
            this.stmt = stmt;
        }
        public readonly String label;
        public readonly Stmt stmt;

		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			Tuple<AST.Env, AST.Stmt> r_stmt = this.stmt.GetStmt(env);
			env = r_stmt.Item1;
			return new Tuple<AST.Env, AST.Stmt>(env, new AST.LabeledStmt(this.label, r_stmt.Item2));
		}
    }

    /// <summary>
    /// case expr:
    ///     stmt
    /// </summary>
	public class CaseStmt : Stmt {
        public CaseStmt(Option<Expr> expr, Stmt stmt) {
            this.expr = expr;
            this.stmt = stmt;
        }
        
        // expr.IsNone means 'default'
        public readonly Option<Expr> expr;
        public readonly Stmt stmt;

		public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
			if (this.expr.IsNone) {
				Tuple<AST.Env, AST.Stmt> r_stmt = this.stmt.GetStmt(env);
				env = r_stmt.Item1;
				return new Tuple<AST.Env, AST.Stmt>(env, new AST.DefaultStmt(r_stmt.Item2));
			
			} else {
                AST.Expr expr = this.expr.Value.GetExpr(env);
			    env = expr.Env;

				expr = AST.TypeCast.MakeCast(expr, new AST.TLong());
				if (!expr.IsConstExpr) {
					throw new InvalidOperationException("case expr not const");
				}
				Int32 value = ((AST.ConstLong)expr).value;

				Tuple<AST.Env, AST.Stmt> r_stmt = this.stmt.GetStmt(env);
				env = r_stmt.Item1;

				return new Tuple<AST.Env, AST.Stmt>(env, new AST.CaseStmt(value, r_stmt.Item2));
			}

		}
    }

}