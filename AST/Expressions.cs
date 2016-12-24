using System;
using System.Collections.Immutable;

namespace AST {

    // 3.2.1.5
    /* First, if either operand has Type long double, the other operand is converted to long double.
     * Otherwise, if either operand has Type double, the other operand is converted to double.
     * Otherwise, if either operand has Type float, the other operand is converted to float.
     * Otherwise, the integral promotions are performed on both operands.
     * Then the following rules are applied:
     * If either operand has Type unsigned long Int32, the other operand is converted to unsigned long Int32.
     * Otherwise, if one operand has Type long Int32 and the other has Type unsigned Int32, if a long Int32 can represent all values of an unsigned Int32, the operand of Type unsigned Int32 is converted to long Int32;
     * if a long Int32 cannot represent all the values of an unsigned Int32, both operands are converted to unsigned long Int32. Otherwise, if either operand has Type long Int32, the other operand is converted to long Int32.
     * Otherwise, if either operand has Type unsigned Int32, the other operand is converted to unsigned Int32.
     * Otherwise, both operands have Type Int32.*/

    // My simplification:
    // I let long = int, long double = double

    public abstract partial class Expr : ISyntaxTreeNode { }

    /// <summary>
    /// Only a name
    /// </summary>
    public sealed partial class Variable : Expr {
        private Variable(String name) {
            this.Name = name;
        }

        public String Name { get; }

        public static Expr Create(String name) =>
            new Variable(name);
    }

    /// <summary>
    /// A list of assignment expressions.
    /// e.g.
    ///   a = 3, b = 4;
    /// </summary>
	public sealed partial class AssignList : Expr {
        private AssignList(ImmutableList<Expr> exprs) {
            if (exprs.Count < 2) {
                throw new InvalidOperationException("Expected at least 2 expressions.");
            }
            this.Exprs = exprs;
        }

        public ImmutableList<Expr> Exprs { get; }

        public static Expr Create(ImmutableList<Expr> exprs) =>
            new AssignList(exprs);
    }

    /// <summary>
    /// Conditional Expression
    /// 
    /// Cond ? TrueExpr : FalseExpr
    /// 
    /// Cond must be of scalar Type
    /// 
    /// 1. if both true_expr and false_expr have arithmetic types
    ///    perform usual arithmetic conversion
    /// 2. 
    /// </summary>
    public sealed partial class ConditionalExpr : Expr {
        public ConditionalExpr(Expr cond, Expr trueExpr, Expr falseExpr) {
            this.Cond = cond;
            this.TrueExpr = trueExpr;
            this.FalseExpr = falseExpr;
        }

        public Expr Cond { get; }

        public Expr TrueExpr { get; }

        public Expr FalseExpr { get; }

        public static Expr Create(Expr cond, Expr trueExpr, Expr falseExpr) =>
            new ConditionalExpr(cond, trueExpr, falseExpr);
    }

    /// <summary>
    /// Function call: func(args)
    /// </summary>
    public sealed partial class FuncCall : Expr {
        private FuncCall(Expr func, ImmutableList<Expr> args) {
            this.Func = func;
            this.Args = args;
        }

        public Expr Func { get; }

        public ImmutableList<Expr> Args { get; }

        public static Expr Create(Expr func, ImmutableList<Expr> args) =>
            new FuncCall(func, args);
    }

    /// <summary>
    /// Expr.attrib: get an attribute from a struct or union
    /// </summary>
    public sealed partial class Attribute : Expr {
        private Attribute(Expr expr, String member) {
            this.Expr = expr;
            this.Member = member;
        }

        public Expr Expr { get; }

        public String Member { get; }

        public static Expr Create(Expr expr, String member) =>
            new Attribute(expr, member);
    }
}