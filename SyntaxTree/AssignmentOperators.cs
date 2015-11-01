namespace SyntaxTree {
    using static SemanticAnalysis;

    /// <summary>
    /// Assignment: Left = Right
    /// </summary>
    /// <remarks>
    /// Left must be a lvalue, but this check is left to the cgen phase.
    /// </remarks>
    public class Assignment : Expr {
        public Assignment(Expr left, Expr right) {
            this.Left = left;
            this.Right = right;
        }

        public Expr Left { get; }
        public Expr Right { get; }

        public static Expr Create(Expr left, Expr right) =>
            new Assignment(left, right);

        public override AST.Expr GetExpr(AST.Env env) {
            var left = SemantExpr(this.Left, ref env);
            var right = SemantExpr(this.Right, ref env);
            right = AST.TypeCast.MakeCast(right, left.type);
            return new AST.Assign(left, right, left.type);
        }
    }

    /// <summary>
    /// Assignment operator
    /// </summary>
    public abstract class AssignOp : Expr {
        protected AssignOp(Expr left, Expr right) {
            this.Left = left;
            this.Right = right;
        }

        public Expr Left { get; }
        public Expr Right { get; }

        public abstract Expr ConstructBinaryOp();

        public override AST.Expr GetExpr(AST.Env env) =>
            Assignment.Create(this.Left, ConstructBinaryOp()).GetExpr(env);
    }

    /// <summary>
    /// MultAssign: a *= b
    /// </summary>
	public class MultAssign : AssignOp {
        public MultAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new MultAssign(left, right);
        public override Expr ConstructBinaryOp() => new Multiply(this.Left, this.Right);
    }

    /// <summary>
    /// DivAssign: a /= b
    /// </summary>
	public class DivAssign : AssignOp {
        public DivAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new DivAssign(left, right);
        public override Expr ConstructBinaryOp() => new Divide(this.Left, this.Right);
    }

    /// <summary>
    /// ModAssign: a %= b
    /// </summary>
    public class ModAssign : AssignOp {
        public ModAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new ModAssign(left, right);
        public override Expr ConstructBinaryOp() => new Modulo(this.Left, this.Right);
    }

    /// <summary>
    /// AddAssign: a += b
    /// </summary>
    public class AddAssign : AssignOp {
        public AddAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new AddAssign(left, right);
        public override Expr ConstructBinaryOp() => new Add(this.Left, this.Right);
    }

    /// <summary>
    /// SubAssign: a -= b
    /// </summary>
    public class SubAssign : AssignOp {
        public SubAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new SubAssign(left, right);
        public override Expr ConstructBinaryOp() => new Sub(this.Left, this.Right);
    }

    /// <summary>
    /// LShiftAssign: a &lt;&lt;= b
    /// </summary>
    public class LShiftAssign : AssignOp {
        public LShiftAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new LShiftAssign(left, right);
        public override Expr ConstructBinaryOp() => new LShift(this.Left, this.Right);
    }

    /// <summary>
    /// RShiftAssign: a >>= b
    /// </summary>
    public class RShiftAssign : AssignOp {
        public RShiftAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new RShiftAssign(left, right);
        public override Expr ConstructBinaryOp() => new RShift(this.Left, this.Right);
    }

    /// <summary>
    /// BitwiseAndAssign: a &= b
    /// </summary>
    public class BitwiseAndAssign : AssignOp {
        public BitwiseAndAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new BitwiseAndAssign(left, right);
        public override Expr ConstructBinaryOp() => new BitwiseAnd(this.Left, this.Right);
    }

    /// <summary>
    /// XorAssign: a ^= b
    /// </summary>
    public class XorAssign : AssignOp {
        public XorAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new XorAssign(left, right);
        public override Expr ConstructBinaryOp() => new Xor(this.Left, this.Right);
    }

    /// <summary>
    /// BitwiseOrAssign: a |= b
    /// </summary>
    public class BitwiseOrAssign : AssignOp {
        public BitwiseOrAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new BitwiseOrAssign(left, right);
        public override Expr ConstructBinaryOp() => new BitwiseOr(this.Left, this.Right);
    }
}
