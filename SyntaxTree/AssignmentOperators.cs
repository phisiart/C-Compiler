using System;
using System.Collections.Generic;

namespace SyntaxTree {

    /// <summary>
    /// Assignment: lhs = rhs
    /// </summary>
    /// <remarks>
    /// lhs must be a lvalue, but this check is left to the cgen phase.
    /// </remarks>
	public class Assignment : Expr {
		public Assignment(Expr lhs, Expr rhs) {
			this.lhs = lhs;
			this.rhs = rhs;
		}
		public readonly Expr lhs;
		public readonly Expr rhs;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr lhs = this.lhs.GetExpr(env);
            AST.Expr rhs = this.rhs.GetExpr(env);
            rhs = AST.TypeCast.MakeCast(rhs, lhs.type);
            return new AST.Assignment(lhs, rhs, lhs.type);
        }
	}

    /// <summary>
    /// Assignment operator
    /// </summary>
    public abstract class AssignOp: Expr {
        public AssignOp(Expr lhs, Expr rhs) {
            this.lhs = lhs;
            this.rhs = rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;

        public abstract Expr ConstructBinaryOp();

        public override AST.Expr GetExpr(AST.Env env) =>
            (new Assignment(lhs, ConstructBinaryOp())).GetExpr(env);
        
    }

    /// <summary>
    /// MultAssign: a *= b
    /// </summary>
	public class MultAssign : AssignOp {
        public MultAssign(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }
        public override Expr ConstructBinaryOp() => new Multiply(lhs, rhs);
	}

    /// <summary>
    /// DivAssign: a /= b
    /// </summary>
	public class DivAssign : AssignOp {
        public DivAssign(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }
        public override Expr ConstructBinaryOp() => new Divide(lhs, rhs);
    }

    /// <summary>
    /// ModAssign: a %= b
    /// </summary>
    public class ModAssign : AssignOp {
        public ModAssign(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }
        public override Expr ConstructBinaryOp() => new Modulo(lhs, rhs);
    }

    /// <summary>
    /// AddAssign: a += b
    /// </summary>
    public class AddAssign : AssignOp {
        public AddAssign(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }
        public override Expr ConstructBinaryOp() => new Add(lhs, rhs);
    }

    /// <summary>
    /// SubAssign: a -= b
    /// </summary>
    public class SubAssign : AssignOp {
        public SubAssign(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }
        public override Expr ConstructBinaryOp() => new Sub(lhs, rhs);
    }

    /// <summary>
    /// LShiftAssign: a <<= b
    /// </summary>
    public class LShiftAssign : AssignOp {
        public LShiftAssign(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }
        public override Expr ConstructBinaryOp() => new LShift(lhs, rhs);
    }

    /// <summary>
    /// RShiftAssign: a >>= b
    /// </summary>
    public class RShiftAssign : AssignOp {
        public RShiftAssign(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }
        public override Expr ConstructBinaryOp() => new RShift(lhs, rhs);
    }

    /// <summary>
    /// BitwiseAndAssign: a &= b
    /// </summary>
    public class BitwiseAndAssign : AssignOp {
        public BitwiseAndAssign(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }
        public override Expr ConstructBinaryOp() => new BitwiseAnd(lhs, rhs);
    }

    /// <summary>
    /// XorAssign: a ^= b
    /// </summary>
    public class XorAssign : AssignOp {
        public XorAssign(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }
        public override Expr ConstructBinaryOp() => new Xor(lhs, rhs);
    }

    /// <summary>
    /// BitwiseOrAssign: a |= b
    /// </summary>
    public class BitwiseOrAssign : AssignOp {
        public BitwiseOrAssign(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }
        public override Expr ConstructBinaryOp() => new BitwiseOr(lhs, rhs);
    }
}
