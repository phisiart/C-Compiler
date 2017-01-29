namespace AST {
    using static SemanticAnalysis;

    /// <summary>
    /// Assignment: Left = Right
    /// </summary>
    /// <remarks>
    /// Left must be a lvalue, but this check is left to the cgen phase.
    /// </remarks>
    public sealed class Assignment : Expr {
        private Assignment(Expr left, Expr right) {
            this.Left = left;
            this.Right = right;
        }

        public Expr Left { get; }

        public Expr Right { get; }

        public static Expr Create(Expr left, Expr right) =>
            new Assignment(left, right);

        public override ABT.Expr GetExpr(ABT.Env env) {
            var left = SemantExpr(this.Left, ref env);
            var right = SemantExpr(this.Right, ref env);
            right = ABT.TypeCast.MakeCast(right, left.Type);
            return new ABT.Assign(left, right);
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

        public override sealed ABT.Expr GetExpr(ABT.Env env) =>
            Assignment.Create(this.Left, ConstructBinaryOp()).GetExpr(env);
    }

    /// <summary>
    /// MultAssign: a *= b
    /// </summary>
	public sealed class MultAssign : AssignOp {
        private MultAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new MultAssign(left, right);
        public override Expr ConstructBinaryOp() => Multiply.Create(this.Left, this.Right);
    }

    /// <summary>
    /// DivAssign: a /= b
    /// </summary>
	public sealed class DivAssign : AssignOp {
        private DivAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new DivAssign(left, right);
        public override Expr ConstructBinaryOp() => Divide.Create(this.Left, this.Right);
    }

    /// <summary>
    /// ModAssign: a %= b
    /// </summary>
    public sealed class ModAssign : AssignOp {
        private ModAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new ModAssign(left, right);
        public override Expr ConstructBinaryOp() => Modulo.Create(this.Left, this.Right);
    }

    /// <summary>
    /// AddAssign: a += b
    /// </summary>
    public sealed class AddAssign : AssignOp {
        private AddAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new AddAssign(left, right);
        public override Expr ConstructBinaryOp() => Add.Create(this.Left, this.Right);
    }

    /// <summary>
    /// SubAssign: a -= b
    /// </summary>
    public sealed class SubAssign : AssignOp {
        private SubAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new SubAssign(left, right);
        public override Expr ConstructBinaryOp() => Sub.Create(this.Left, this.Right);
    }

    /// <summary>
    /// LShiftAssign: a &lt;&lt;= b
    /// </summary>
    public sealed class LShiftAssign : AssignOp {
        private LShiftAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new LShiftAssign(left, right);
        public override Expr ConstructBinaryOp() => LShift.Create(this.Left, this.Right);
    }

    /// <summary>
    /// RShiftAssign: a >>= b
    /// </summary>
    public sealed class RShiftAssign : AssignOp {
        private RShiftAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new RShiftAssign(left, right);
        public override Expr ConstructBinaryOp() => RShift.Create(this.Left, this.Right);
    }

    /// <summary>
    /// BitwiseAndAssign: <c>a &amp;= b</c>
    /// </summary>
    public sealed class BitwiseAndAssign : AssignOp {
        private BitwiseAndAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new BitwiseAndAssign(left, right);
        public override Expr ConstructBinaryOp() => BitwiseAnd.Create(this.Left, this.Right);
    }

    /// <summary>
    /// XorAssign: a ^= b
    /// </summary>
    public sealed class XorAssign : AssignOp {
        private XorAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new XorAssign(left, right);
        public override Expr ConstructBinaryOp() => Xor.Create(this.Left, this.Right);
    }

    /// <summary>
    /// BitwiseOrAssign: a |= b
    /// </summary>
    public sealed class BitwiseOrAssign : AssignOp {
        private BitwiseOrAssign(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new BitwiseOrAssign(left, right);
        public override Expr ConstructBinaryOp() => BitwiseOr.Create(this.Left, this.Right);
    }
}
