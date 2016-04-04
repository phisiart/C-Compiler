using System;
using System.Diagnostics;
using CodeGeneration;

namespace AST {

    // IncDecExpr
    //   |
    //   +-- PostIncrement, PostDecrement, PreIncrement, PreDecrement

    public abstract partial class IncDecExpr : Expr {
        protected IncDecExpr(Expr expr) {
            if (!(expr.Type is ScalarType)) {
                throw new InvalidOperationException("Only supports scalars.");
            }
            this.Expr = expr;
        }

        public Expr Expr { get; }

        public override Env Env => this.Expr.Env;

        public override Boolean IsLValue => false;

        public override ExprType Type => this.Expr.Type;
    }

    /// <summary>
    /// Expr++: must be integral, float or pointer.
    /// 
    /// If Expr is an array, it is converted to a pointer in semantic analysis.
    /// </summary>
    public sealed partial class PostIncrement : IncDecExpr {
        public PostIncrement(Expr expr)
            : base(expr) { }
    }

    /// <summary>
    /// Expr--: must be a scalar
    /// </summary>
    public sealed partial class PostDecrement : IncDecExpr {
        public PostDecrement(Expr expr)
            : base(expr) { }
    }

    /// <summary>
    /// ++Expr: must be a scalar
    /// </summary>
    public sealed partial class PreIncrement : IncDecExpr {
        public PreIncrement(Expr expr)
            : base(expr) { }
    }

    /// <summary>
    /// --Expr: must be a scalar
    /// </summary>
    public sealed partial class PreDecrement : IncDecExpr {
        public PreDecrement(Expr expr)
            : base(expr) { }
    }

    public abstract partial class UnaryArithOp : Expr {
        protected UnaryArithOp(Expr expr) {
            this.Expr = expr;
        }

        public Expr Expr { get; }

        public override Env Env => this.Expr.Env;

        public override Boolean IsLValue => false;

        public override abstract ExprType Type { get; }
    }

    /// <summary>
    /// -Expr: only takes arithmetic Type.
    /// 
    /// After semantic analysis, only the following 4 types are possible:
    /// 1) long
    /// 2) ulong
    /// 3) float
    /// 4) double
    /// </summary>
    public sealed partial class Negative : UnaryArithOp {
        public Negative(Expr expr)
            : base(expr) { }

        public override ExprType Type => this.Expr.Type;
    }

    /// <summary>
    /// ~Expr: only takes integral Type.
    /// 
    /// After semantic analysis, only the following 2 types are possible:
    /// 1) long
    /// 2) ulong
    /// </summary>
    public sealed partial class BitwiseNot : UnaryArithOp {
        public BitwiseNot(Expr expr)
            : base(expr) {
            if (!(expr.Type is LongType || expr.Type is ULongType)) {
                throw new InvalidOperationException("Invalid operand type.");
            }
        }

        public override ExprType Type => this.Expr.Type;
    }

    /// <summary>
    /// !Expr: only takes scalar Type.
    /// 
    /// After semantic analysis, only the following 4 types are possible:
    /// 1) long
    /// 2) ulong
    /// 3) float
    /// 4) double
    /// 
    /// Pointers are converted to ulongs.
    /// </summary>
    public sealed partial class LogicalNot : UnaryArithOp {
        public LogicalNot(Expr expr)
            : base(expr) {
            if (!(expr.Type is LongType || expr.Type is ULongType
                  || expr.Type is FloatType || expr.Type is DoubleType)) {
                throw new InvalidOperationException("Invalid operand type.");
            }
        }

        private static ExprType _type = new LongType(true);
        public override ExprType Type => _type;
    }
}
