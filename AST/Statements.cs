using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static AST.SemanticAnalysis;

namespace AST {

    public abstract class Stmt : ISyntaxTreeNode {
        public abstract Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env);
    }

    /// <summary>
    /// goto label;
    /// </summary>
    public sealed class GotoStmt : Stmt {
        public GotoStmt(String label) {
            this.Label = label;
        }
        public String Label { get; }

        public static Stmt Create(String label) =>
            new GotoStmt(label);

        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            return new Tuple<ABT.Env, ABT.Stmt>(env, new ABT.GotoStmt(this.Label));
        }

        //public override ISemantReturn<AST.Stmt> SemantStmt(AST.Env env) {
        //    return SemantReturn.Create(env, new AST.GotoStmt(this.Label));
        //}
    }

    /// <summary>
    /// continue;
    /// </summary>
    public sealed class ContStmt : Stmt {
        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            return new Tuple<ABT.Env, ABT.Stmt>(env, new ABT.ContStmt());
        }
    }

    /// <summary>
    /// break;
    /// </summary>
    public sealed class BreakStmt : Stmt {
        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            return new Tuple<ABT.Env, ABT.Stmt>(env, new ABT.BreakStmt());
        }
    }

    /// <summary>
    /// return [expr];
    /// </summary>
    public sealed class ReturnStmt : Stmt {
        public ReturnStmt(Option<Expr> expr) {
            this.Expr = expr;
        }

        public static Stmt Create(Option<Expr> expr) =>
            new ReturnStmt(expr);

        public readonly Option<Expr> Expr;

        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            var expr = this.Expr.Map(_ => _.GetExpr(env));
            expr = expr.Map(_ => ABT.TypeCast.MakeCast(_, env.GetCurrentFunction().ReturnType));
            return new Tuple<ABT.Env, ABT.Stmt>(env, new ABT.ReturnStmt(expr));
        }
    }

    /// <summary>
    /// {
    ///     declaration*
    ///     statement*
    /// }
    /// </summary>
    public sealed class CompoundStmt : Stmt {
        public CompoundStmt(List<Decln> declns, List<Stmt> stmts) {
            this.Declns = declns;
            this.Stmts = stmts;
        }
        public List<Decln> Declns { get; }
        public List<Stmt> Stmts { get; }

        public static Stmt Create(ImmutableList<Decln> declns, ImmutableList<Stmt> stmts) =>
            new CompoundStmt(declns.ToList(), stmts.ToList());

        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            env = env.InScope();
            List<Tuple<ABT.Env, ABT.Decln>> declns = new List<Tuple<ABT.Env, ABT.Decln>>();
            List<Tuple<ABT.Env, ABT.Stmt>> stmts = new List<Tuple<ABT.Env, ABT.Stmt>>();

            foreach (Decln decln in this.Declns) {
                //Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_decln = decln.GetDeclns_(env);
                //env = r_decln.Item1;
                //declns.AddRange(r_decln.Item2);

                var declns_ = Semant(decln.GetDeclns, ref env);
                declns.AddRange(declns_);
            }

            foreach (Stmt stmt in this.Stmts) {
                Tuple<ABT.Env, ABT.Stmt> r_stmt = stmt.GetStmt(env);
                env = r_stmt.Item1;
                stmts.Add(r_stmt);
            }

            env = env.OutScope();

            return new Tuple<ABT.Env, ABT.Stmt>(env, new ABT.CompoundStmt(declns, stmts));
        }
    }

    /// <summary>
    /// expr;
    /// </summary>
    public sealed class ExprStmt : Stmt {
        public ExprStmt(Option<Expr> expr) {
            this.Expr = expr;
        }
        public Option<Expr> Expr { get; }
        public static Stmt Create(Option<Expr> expr) =>
            new ExprStmt(expr);

        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            var expr = this.Expr.Map(_ => _.GetExpr(env));
            env = expr.IsSome ? expr.Value.Env : env;
            return new Tuple<ABT.Env, ABT.Stmt>(env, new ABT.ExprStmt(expr));
        }
    }

    /// <summary>
    /// while (Cond) {
    ///     Body
    /// }
    /// 
    /// Cond must be of scalar Type
    /// </summary>
    public sealed class WhileStmt : Stmt {
        public WhileStmt(Expr cond, Stmt body) {
            this.Cond = cond;
            this.Body = body;
        }

        public static Stmt Create(Expr cond, Stmt body) =>
            new WhileStmt(cond, body);

        public Expr Cond { get; }
        public Stmt Body { get; }

        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            ABT.Expr cond = this.Cond.GetExpr(env);
            env = cond.Env;

            if (!cond.Type.IsScalar) {
                throw new InvalidOperationException("Error: conditional expression in while Loop must be scalar.");
            }

            Tuple<ABT.Env, ABT.Stmt> r_body = this.Body.GetStmt(env);
            env = r_body.Item1;
            ABT.Stmt body = r_body.Item2;

            return new Tuple<ABT.Env, ABT.Stmt>(env, new ABT.WhileStmt(cond, body));
        }

    }

    /// <summary>
    /// do {
    ///     Body
    /// } while (Cond);
    /// 
    /// Cond must be of scalar Type
    /// </summary>
    public sealed class DoWhileStmt : Stmt {
        public DoWhileStmt(Stmt body, Expr cond) {
            this.Body = body;
            this.Cond = cond;
        }

        public Stmt Body { get; }
        public Expr Cond { get; }

        public static Stmt Create(Stmt body, Expr cond) =>
            new DoWhileStmt(body, cond);

        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            Tuple<ABT.Env, ABT.Stmt> r_body = this.Body.GetStmt(env);
            env = r_body.Item1;
            ABT.Stmt body = r_body.Item2;

            ABT.Expr cond = this.Cond.GetExpr(env);
            env = cond.Env;

            if (!cond.Type.IsScalar) {
                throw new InvalidOperationException("Error: conditional expression in while Loop must be scalar.");
            }

            return new Tuple<ABT.Env, ABT.Stmt>(env, new ABT.DoWhileStmt(body, cond));
        }
    }

    /// <summary>
    /// for (Init; Cond; Loop) {
    ///     Body
    /// }
    /// 
    /// Cond must be of scalar Type
    /// </summary>
    public sealed class ForStmt : Stmt {
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

        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            IOption<ABT.Expr> init = this.Init.Map(_ => _.GetExpr(env));
            if (init.IsSome) {
                env = init.Value.Env;
            }

            Option<ABT.Expr> cond = this.Cond.Map(_ => _.GetExpr(env));
            if (cond.IsSome) {
                env = cond.Value.Env;
            }

            if (cond.IsSome && !cond.Value.Type.IsScalar) {
                throw new InvalidOperationException("Error: conditional expression in while Loop must be scalar.");
            }

            IOption<ABT.Expr> loop = this.Loop.Map(_ => _.GetExpr(env));
            if (loop.IsSome) {
                env = loop.Value.Env;
            }

            Tuple<ABT.Env, ABT.Stmt> r_body = this.Body.GetStmt(env);
            env = r_body.Item1;
            ABT.Stmt body = r_body.Item2;

            return new Tuple<ABT.Env, ABT.Stmt>(env, new ABT.ForStmt(init, cond, loop, body));
        }

    }

    /// <summary>
    /// switch (expr)
    ///     stmt
    /// </summary>
    public sealed class SwitchStmt : Stmt {
        public SwitchStmt(Expr expr, Stmt stmt) {
            this.Expr = expr;
            this.Stmt = stmt;
        }
        public Expr Expr { get; }
        public Stmt Stmt { get; }

        public static Stmt Create(Expr expr, Stmt stmt) =>
            new SwitchStmt(expr, stmt);

        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            ABT.Expr expr = this.Expr.GetExpr(env);

            Tuple<ABT.Env, ABT.Stmt> r_stmt = this.Stmt.GetStmt(env);
            env = r_stmt.Item1;
            ABT.Stmt stmt = r_stmt.Item2;

            return new Tuple<ABT.Env, ABT.Stmt>(env, new ABT.SwitchStmt(expr, stmt));
        }
    }

    /// <summary>
    /// if (Cond)
    ///     stmt
    /// </summary>
    public sealed class IfStmt : Stmt {
        public IfStmt(Expr cond, Stmt stmt) {
            this.Cond = cond;
            this.Stmt = stmt;
        }

        public Expr Cond { get; }
        public Stmt Stmt { get; }

        public static Stmt Create(Expr cond, Stmt stmt) =>
            new IfStmt(cond, stmt);

        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            ABT.Expr cond = this.Cond.GetExpr(env);

            if (!cond.Type.IsScalar) {
                throw new InvalidOperationException("Error: expected scalar Type");
            }

            Tuple<ABT.Env, ABT.Stmt> r_stmt = this.Stmt.GetStmt(env);
            env = r_stmt.Item1;
            ABT.Stmt stmt = r_stmt.Item2;

            return new Tuple<ABT.Env, ABT.Stmt>(env, new ABT.IfStmt(cond, stmt));
        }
    }

    /// <summary>
    /// if (Cond)
    ///     true-stmt
    /// else
    ///     false-stmt
    /// </summary>
    public sealed class IfElseStmt : Stmt {
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

        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            ABT.Expr cond = this.Cond.GetExpr(env);

            if (!cond.Type.IsScalar) {
                throw new InvalidOperationException("Error: expected scalar Type");
            }

            Tuple<ABT.Env, ABT.Stmt> r_true_stmt = this.TrueStmt.GetStmt(env);
            env = r_true_stmt.Item1;
            ABT.Stmt true_stmt = r_true_stmt.Item2;

            Tuple<ABT.Env, ABT.Stmt> r_false_stmt = this.FalseStmt.GetStmt(env);
            env = r_false_stmt.Item1;
            ABT.Stmt false_stmt = r_false_stmt.Item2;

            return new Tuple<ABT.Env, ABT.Stmt>(env, new ABT.IfElseStmt(cond, true_stmt, false_stmt));
        }
    }

    /// <summary>
    /// label:
    ///     stmt
    /// </summary>
	public sealed class LabeledStmt : Stmt {
        private LabeledStmt(String label, Stmt stmt) {
            this.Label = label;
            this.Stmt = stmt;
        }

        public String Label { get; }
        public Stmt Stmt { get; }

        public static Stmt Create(String label, Stmt stmt) =>
            new LabeledStmt(label, stmt);

        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            Tuple<ABT.Env, ABT.Stmt> r_stmt = this.Stmt.GetStmt(env);
            env = r_stmt.Item1;
            return new Tuple<ABT.Env, ABT.Stmt>(env, new ABT.LabeledStmt(this.Label, r_stmt.Item2));
        }
    }

    /// <summary>
    /// case expr:
    ///     stmt
    /// </summary>
	public sealed class CaseStmt : Stmt {
        private CaseStmt(Expr expr, Stmt stmt) {
            this.Expr = expr;
            this.Stmt = stmt;
        }

        public Expr Expr { get; }
        public Stmt Stmt { get; }

        public static Stmt Create(Expr expr, Stmt stmt) =>
            new CaseStmt(expr, stmt);

        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            ABT.Expr expr = this.Expr.GetExpr(env);
            env = expr.Env;

            expr = ABT.TypeCast.MakeCast(expr, new ABT.LongType());
            if (!expr.IsConstExpr) {
                throw new InvalidOperationException("case Expr not const");
            }
            Int32 value = ((ABT.ConstLong)expr).Value;

            Tuple<ABT.Env, ABT.Stmt> r_stmt = this.Stmt.GetStmt(env);
            env = r_stmt.Item1;

            return new Tuple<ABT.Env, ABT.Stmt>(env, new ABT.CaseStmt(value, r_stmt.Item2));
        }
    }

    /// <summary>
    /// default:
    ///     stmt
    /// </summary>
    public sealed class DefaultStmt : Stmt {
        private DefaultStmt(Stmt stmt) {
            this.Stmt = stmt;
        }

        public static DefaultStmt Create(Stmt stmt) =>
            new DefaultStmt(stmt);

        public Stmt Stmt { get; }

        public override Tuple<ABT.Env, ABT.Stmt> GetStmt(ABT.Env env) {
            var stmt = SemantStmt(this.Stmt.GetStmt, ref env);
            return Tuple.Create(env, new ABT.DefaultStmt(stmt) as ABT.Stmt);
        }
    }
}