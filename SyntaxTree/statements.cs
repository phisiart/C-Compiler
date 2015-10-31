using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static SyntaxTree.SemanticAnalysis;

namespace SyntaxTree {

    public abstract class Stmt : SyntaxTreeNode {
        public abstract Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env);
    }


    public class GotoStmt : Stmt {
        public GotoStmt(String label) {
            this.Label = label;
        }
        public String Label { get; }

        public static Stmt Create(String label) =>
            new GotoStmt(label);

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            return new Tuple<AST.Env, AST.Stmt>(env, new AST.GotoStmt(this.Label));
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
        public ReturnStmt(Option<Expr> expr) {
            this.Expr = expr;
        }

        public static Stmt Create(Option<Expr> expr) =>
            new ReturnStmt(expr);

        public readonly Option<Expr> Expr;

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            var expr = this.Expr.Map(_ => _.GetExpr(env));
            expr = expr.Map(_ => AST.TypeCast.MakeCast(_, env.GetCurrentFunction().ret_t));
            return new Tuple<AST.Env, AST.Stmt>(env, new AST.ReturnStmt(expr));
        }
    }


    public class CompoundStmt : Stmt {
        public CompoundStmt(List<Decln> declns, List<Stmt> stmts) {
            this.Declns = declns;
            this.Stmts = stmts;
        }
        public List<Decln> Declns { get; }
        public List<Stmt> Stmts { get; }

        public static Stmt Create(ImmutableList<Decln> declns, ImmutableList<Stmt> stmts) =>
            new CompoundStmt(declns.ToList(), stmts.ToList());

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            env = env.InScope();
            List<Tuple<AST.Env, AST.Decln>> declns = new List<Tuple<AST.Env, AST.Decln>>();
            List<Tuple<AST.Env, AST.Stmt>> stmts = new List<Tuple<AST.Env, AST.Stmt>>();

            foreach (Decln decln in this.Declns) {
                //Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_decln = decln.GetDeclns_(env);
                //env = r_decln.Item1;
                //declns.AddRange(r_decln.Item2);

                var declns_ = Semant(decln.GetDeclns, ref env);
                declns.AddRange(declns_);
            }

            foreach (Stmt stmt in this.Stmts) {
                Tuple<AST.Env, AST.Stmt> r_stmt = stmt.GetStmt(env);
                env = r_stmt.Item1;
                stmts.Add(r_stmt);
            }

            env = env.OutScope();

            return new Tuple<AST.Env, AST.Stmt>(env, new AST.CompoundStmt(declns, stmts));
        }
    }


    public class ExprStmt : Stmt {
        public ExprStmt(Option<Expr> expr) {
            this.Expr = expr;
        }
        public Option<Expr> Expr { get; }
        public static Stmt Create(Option<Expr> expr) =>
            new ExprStmt(expr);

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            var expr = this.Expr.Map(_ => _.GetExpr(env));
            env = expr.IsSome ? expr.Value.Env : env;
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
            this.Cond = cond;
            this.Body = body;
        }

        public static Stmt Create(Expr cond, Stmt body) =>
            new WhileStmt(cond, body);

        public Expr Cond { get; }
        public Stmt Body { get; }

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr cond = this.Cond.GetExpr(env);
            env = cond.Env;

            if (!cond.type.IsScalar) {
                throw new InvalidOperationException("Error: conditional expression in while loop must be scalar.");
            }

            Tuple<AST.Env, AST.Stmt> r_body = this.Body.GetStmt(env);
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
            this.Body = body;
            this.Cond = cond;
        }

        public Stmt Body { get; }
        public Expr Cond { get; }

        public static Stmt Create(Stmt body, Expr cond) =>
            new DoWhileStmt(body, cond);

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            Tuple<AST.Env, AST.Stmt> r_body = this.Body.GetStmt(env);
            env = r_body.Item1;
            AST.Stmt body = r_body.Item2;

            AST.Expr cond = this.Cond.GetExpr(env);
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
            this.Init = init;
            this.Cond = cond;
            this.Loop = loop;
            this.Body = body;
        }

        public Option<Expr> Init { get; }
        public Option<Expr> Cond { get; }
        public Option<Expr> Loop { get; }
        public Stmt Body { get; }

        public static Stmt Create(Option<Expr> init, Option<Expr> cond, Option<Expr> loop, Stmt body) =>
            new ForStmt(init, cond, loop, body);

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            Option<AST.Expr> init = this.Init.Map(_ => _.GetExpr(env));
            if (init.IsSome) {
                env = init.Value.Env;
            }

            Option<AST.Expr> cond = this.Cond.Map(_ => _.GetExpr(env));
            if (cond.IsSome) {
                env = cond.Value.Env;
            }

            if (cond.IsSome && !cond.Value.type.IsScalar) {
                throw new InvalidOperationException("Error: conditional expression in while loop must be scalar.");
            }

            Option<AST.Expr> loop = this.Loop.Map(_ => _.GetExpr(env));
            if (loop.IsSome) {
                env = loop.Value.Env;
            }

            Tuple<AST.Env, AST.Stmt> r_body = this.Body.GetStmt(env);
            env = r_body.Item1;
            AST.Stmt body = r_body.Item2;

            return new Tuple<AST.Env, AST.Stmt>(env, new AST.ForStmt(init, cond, loop, body));
        }

    }


    public class SwitchStmt : Stmt {
        public SwitchStmt(Expr expr, Stmt stmt) {
            this.Expr = expr;
            this.Stmt = stmt;
        }
        public Expr Expr { get; }
        public Stmt Stmt { get; }

        public static Stmt Create(Expr expr, Stmt stmt) =>
            new SwitchStmt(expr, stmt);

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr expr = this.Expr.GetExpr(env);

            Tuple<AST.Env, AST.Stmt> r_stmt = this.Stmt.GetStmt(env);
            env = r_stmt.Item1;
            AST.Stmt stmt = r_stmt.Item2;

            return new Tuple<AST.Env, AST.Stmt>(env, new AST.SwitchStmt(expr, stmt));
        }
    }


    public class IfStmt : Stmt {
        public IfStmt(Expr cond, Stmt stmt) {
            this.Cond = cond;
            this.Stmt = stmt;
        }

        public Expr Cond { get; }
        public Stmt Stmt { get; }

        public static Stmt Create(Expr cond, Stmt stmt) =>
            new IfStmt(cond, stmt);

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr cond = this.Cond.GetExpr(env);

            if (!cond.type.IsScalar) {
                throw new InvalidOperationException("Error: expected scalar type");
            }

            Tuple<AST.Env, AST.Stmt> r_stmt = this.Stmt.GetStmt(env);
            env = r_stmt.Item1;
            AST.Stmt stmt = r_stmt.Item2;

            return new Tuple<AST.Env, AST.Stmt>(env, new AST.IfStmt(cond, stmt));
        }
    }


    public class IfElseStmt : Stmt {
        public IfElseStmt(Expr cond, Stmt trueStmt, Stmt falseStmt) {
            this.Cond = cond;
            this.TrueStmt = trueStmt;
            this.FalseStmt = falseStmt;
        }

        public Expr Cond { get; }
        public Stmt TrueStmt { get; }
        public Stmt FalseStmt { get; }

        public static Stmt Create(Expr cond, Stmt trueStmt, Stmt falseStmt) =>
            new IfElseStmt(cond, trueStmt, falseStmt);

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            AST.Expr cond = this.Cond.GetExpr(env);

            if (!cond.type.IsScalar) {
                throw new InvalidOperationException("Error: expected scalar type");
            }

            Tuple<AST.Env, AST.Stmt> r_true_stmt = this.TrueStmt.GetStmt(env);
            env = r_true_stmt.Item1;
            AST.Stmt true_stmt = r_true_stmt.Item2;

            Tuple<AST.Env, AST.Stmt> r_false_stmt = this.FalseStmt.GetStmt(env);
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
            this.Label = label;
            this.Stmt = stmt;
        }

        public String Label { get; }
        public Stmt Stmt { get; }

        public static Stmt Create(String label, Stmt stmt) =>
            new LabeledStmt(label, stmt);

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            Tuple<AST.Env, AST.Stmt> r_stmt = this.Stmt.GetStmt(env);
            env = r_stmt.Item1;
            return new Tuple<AST.Env, AST.Stmt>(env, new AST.LabeledStmt(this.Label, r_stmt.Item2));
        }
    }

    /// <summary>
    /// case Expr:
    ///     stmt
    /// </summary>
	public class CaseStmt : Stmt {
        public CaseStmt(Option<Expr> expr, Stmt stmt) {
            this.Expr = expr;
            this.Stmt = stmt;
        }

        // Expr.IsNone means 'default'
        public Option<Expr> Expr { get; }
        public Stmt Stmt { get; }

        public static Stmt Create(Option<Expr> expr, Stmt stmt) =>
            new CaseStmt(expr, stmt);

        public override Tuple<AST.Env, AST.Stmt> GetStmt(AST.Env env) {
            if (this.Expr.IsNone) {
                Tuple<AST.Env, AST.Stmt> r_stmt = this.Stmt.GetStmt(env);
                env = r_stmt.Item1;
                return new Tuple<AST.Env, AST.Stmt>(env, new AST.DefaultStmt(r_stmt.Item2));

            } else {
                AST.Expr expr = this.Expr.Value.GetExpr(env);
                env = expr.Env;

                expr = AST.TypeCast.MakeCast(expr, new AST.TLong());
                if (!expr.IsConstExpr) {
                    throw new InvalidOperationException("case Expr not const");
                }
                Int32 value = ((AST.ConstLong)expr).value;

                Tuple<AST.Env, AST.Stmt> r_stmt = this.Stmt.GetStmt(env);
                env = r_stmt.Item1;

                return new Tuple<AST.Env, AST.Stmt>(env, new AST.CaseStmt(value, r_stmt.Item2));
            }

        }
    }

}