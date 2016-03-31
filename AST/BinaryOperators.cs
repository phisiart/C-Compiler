using System;
using System.Diagnostics;
using CodeGeneration;

namespace AST {

    public abstract class BinaryOp : Expr {
        protected BinaryOp(Expr left, Expr right, ExprType type)
            : base(type) {
            this.Left = left;
            this.Right = right;
        }

        public Expr Left { get; }

        public Expr Right { get; }

        public override Env Env => this.Right.Env;

        public override Boolean IsLValue => false;
    }

    /// <summary>
    /// A binary integral operator only takes integral operands.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long op long
    /// 2) ulong op ulong
    /// 
    /// The procedure is always:
    /// %eax = Left, %ebx = Right
    /// %eax = %eax op %ebx
    /// </summary>
    public abstract partial class BinaryIntegralOp : BinaryOp {
        protected BinaryIntegralOp(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// These operators perform usual arithmetic conversion.
    /// 
    /// After semantic analysis, only four cases are possible:
    /// 1) long op long
    /// 2) ulong op ulong
    /// 3) float op float
    /// 4) double op double
    /// 
    /// The procedure for long or ulong is the same as that of binary integral operators.
    /// The procedure for float and double is always:
    /// %st(0) = Left, %st(1) = Right
    /// %st(0) = %st(0) op %st(1), invalidate %st(1)
    /// </summary>
    public abstract partial class BinaryArithmeticOp : BinaryIntegralOp {
        protected BinaryArithmeticOp(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// The multiplication (*) operator can either take
    /// 1) integral- or 2) floating-Type operands.
    /// 
    /// After semantic analysis, only four cases are possible:
    /// 1) long * long
    /// 2) ulong * ulong
    /// 3) float * float
    /// 4) double * double
    /// </summary>
    public sealed partial class Multiply : BinaryArithmeticOp {
        public Multiply(Expr left, Expr right, ExprType type)
            : base(left, right, type) {
            Debug.Assert((type.Kind == ExprTypeKind.LONG && left.Type.Kind == ExprTypeKind.LONG && right.Type.Kind == ExprTypeKind.LONG)
                        || (type.Kind == ExprTypeKind.ULONG && left.Type.Kind == ExprTypeKind.ULONG && right.Type.Kind == ExprTypeKind.ULONG)
                        || (type.Kind == ExprTypeKind.FLOAT && left.Type.Kind == ExprTypeKind.FLOAT && right.Type.Kind == ExprTypeKind.FLOAT)
                        || (type.Kind == ExprTypeKind.DOUBLE && left.Type.Kind == ExprTypeKind.DOUBLE && right.Type.Kind == ExprTypeKind.DOUBLE));
        }
    }

    /// <summary>
    /// The division (/) operator can either take
    /// 1) integral- or 2) floating-Type operands.
    /// 
    /// After semantic analysis, only four cases are possible:
    /// 1) long / long
    /// 2) ulong / ulong
    /// 3) float / float
    /// 4) double / double
    /// </summary>
    public sealed partial class Divide : BinaryArithmeticOp {
        public Divide(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// The modulo (%) operator can only take integral operands.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long % long
    /// 2) ulong % ulong
    /// </summary>
    public sealed partial class Modulo : BinaryIntegralOp {
        public Modulo(Expr left, Expr right, ExprType type)
            : base(left, right, type) {
        }
    }

    /// <summary>
    /// The xor (^) operator can only take integral operands.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long ^ long
    /// 2) ulong ^ ulong
    /// 
    /// https://msdn.microsoft.com/en-us/library/17zwb64t.aspx
    /// </summary>
    public sealed partial class Xor : BinaryIntegralOp {
        public Xor(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// The bitwise or (|) operator can only take integral operands.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long | long
    /// 2) ulong | ulong
    /// 
    /// https://msdn.microsoft.com/en-us/library/17zwb64t.aspx
    /// </summary>
    public sealed partial class BitwiseOr : BinaryIntegralOp {
        public BitwiseOr(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// The bitwise and (&amp;) operator can only take integral operands.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long &amp; long
    /// 2) ulong &amp; ulong
    /// 
    /// https://msdn.microsoft.com/en-us/library/17zwb64t.aspx
    /// </summary>
    public sealed partial class BitwiseAnd : BinaryIntegralOp {
        public BitwiseAnd(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// The left shift operator can only take integral operands.
    /// Append 0's on the right.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long %lt;%lt; long
    /// 2) ulong %lt;%lt; ulong
    /// </summary>
    public sealed partial class LShift : BinaryIntegralOp {
        public LShift(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// The right shift operator can only take integral operands.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long >> long (arithmetic shift, append sign bit)
    /// 2) ulong >> ulong (logical shift, append 0)
    /// </summary>
    public sealed partial class RShift : BinaryIntegralOp {
        public RShift(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// The addition operator can either take
    /// 1) integral- or 2) floating-Type operands.
    /// 
    /// After semantic analysis, pointer additions are converted into
    /// combinations of Type-casts and series of operations. So in AST,
    /// only four cases are possible:
    /// 1) long + long
    /// 2) ulong + ulong
    /// 3) float + float
    /// 4) double + double
    /// </summary>
    public sealed partial class Add : BinaryArithmeticOp {
        public Add(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// The subtraction operator can either take
    /// 1) integral- or 2) floating-Type operands.
    /// 
    /// After semantic analysis, pointer subtractions are converted into
    ///   combinations of Type-casts and series of operations. So in AST,
    ///   only four cases are possible:
    /// 1) long - long
    /// 2) ulong - ulong
    /// 3) float - float
    /// 4) double - double
    /// </summary>
    public sealed partial class Sub : BinaryArithmeticOp {
        public Sub(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// Binary arithmetic comparison operation.
    /// 
    /// After semantic analysis, only four cases are possible:
    /// 1) long op long
    /// 2) ulong op ulong
    /// 3) float op float
    /// 4) double op double
    /// 
    /// http://x86.renejeschke.de/html/file_module_x86_id_288.html
    /// </summary>
    public abstract partial class BinaryArithmeticComp : BinaryArithmeticOp {
        protected BinaryArithmeticComp(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// The "greater than or equal to" operator can either take
    /// 1) integral- or 2) floating-Type operands.
    /// 
    /// After semantic analysis, pointer comparisons are converted into
    ///   integer comparisons. So in AST, only four cases are possible:
    /// 1) long >= long
    /// 2) ulong >= ulong
    /// 3) float >= float
    /// 4) double >= double
    /// 
    /// http://x86.renejeschke.de/html/file_module_x86_id_288.html
    /// </summary>
    public sealed partial class GEqual : BinaryArithmeticComp {
        public GEqual(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// The "greater than" operator can either take
    /// 1) integral- or 2) floating-Type operands.
    /// 
    /// After semantic analysis, pointer comparisons are converted into
    ///   integer comparisons. So in AST, only four cases are possible:
    /// 1) long > long
    /// 2) ulong > ulong
    /// 3) float > float
    /// 4) double > double
    /// 
    /// http://x86.renejeschke.de/html/file_module_x86_id_288.html
    /// </summary>
    public sealed partial class Greater : BinaryArithmeticComp {
        public Greater(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// The "less than or equal to" operator can either take
    /// 1) integral- or 2) floating-Type operands.
    /// 
    /// After semantic analysis, pointer comparisons are converted into
    ///   integer comparisons. So in AST, only four cases are possible:
    /// 1) long %lt;= long
    /// 2) ulong %lt;= ulong
    /// 3) float %lt;= float
    /// 4) double %lt;= double
    /// 
    /// http://x86.renejeschke.de/html/file_module_x86_id_288.html
    /// </summary>
    public sealed partial class LEqual : BinaryArithmeticComp {
        public LEqual(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// The "less than" operator can either take
    /// 1) integral- or 2) floating-Type operands.
    /// 
    /// After semantic analysis, pointer comparisons are converted into
    ///   integer comparisons. So in AST, only four cases are possible:
    /// 1) long %lt; long
    /// 2) ulong %lt; ulong
    /// 3) float %lt; float
    /// 4) double %lt; double
    /// 
    /// http://x86.renejeschke.de/html/file_module_x86_id_288.html
    /// </summary>
    public sealed partial class Less : BinaryArithmeticComp {
        public Less(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// The "equal to" operator can either take
    /// 1) integral- or 2) floating-Type operands.
    /// 
    /// After semantic analysis, pointer comparisons are converted into
    ///   integer comparisons. So in AST, only four cases are possible:
    /// 1) long == long
    /// 2) ulong == ulong
    /// 3) float == float
    /// 4) double == double
    /// 
    /// It's surprising that the C equal operator doesn't support structs and unions.
    /// http://x86.renejeschke.de/html/file_module_x86_id_288.html
    /// </summary>
    public sealed partial class Equal : BinaryArithmeticComp {
        public Equal(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// The "not equal to" operator can either take
    /// 1) integral- or 2) floating-Type operands.
    /// 
    /// After semantic analysis, pointer comparisons are converted into
    ///   integer comparisons. So in AST, only four cases are possible:
    /// 1) long != long
    /// 2) ulong != ulong
    /// 3) float != float
    /// 4) double != double
    /// 
    /// It's surprising that the C equal operator doesn't support structs and unions.
    /// http://x86.renejeschke.de/html/file_module_x86_id_288.html
    /// </summary>
    public sealed partial class NotEqual : BinaryArithmeticComp {
        public NotEqual(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
    }

    /// <summary>
    /// Left &amp;&amp; Right: can only take scalars (to compare with 0).
    /// 
    /// After semantic analysis, each operand can only be
    /// long, ulong, float, double.
    /// Pointers are casted to ulongs.
    /// 
    /// if Left == 0:
    ///     return 0
    /// else:
    ///     return Right != 0
    /// 
    /// Generate the assembly in this fashion,
    /// then every route would only have one jump.
    /// 
    ///        +---------+   0
    ///        | cmp Left |-------+
    ///        +---------+       |
    ///             |            |
    ///             | 1          |
    ///             |            |
    ///        +----+----+   0   |
    ///        | cmp Right |-------+
    ///        +---------+       |
    ///             |            |
    ///             | 1          |
    ///             |            |
    ///        +----+----+       |
    ///        | eax = 1 |       |
    ///        +---------+       |
    ///             |            |
    ///   +---------+            |
    ///   |                      |
    ///   |         +------------+ label_reset
    ///   |         |
    ///   |    +---------+
    ///   |    | eax = 0 |
    ///   |    +---------+
    ///   |         |
    ///   +---------+ label_finish
    ///             |
    /// 
    /// </summary>
    public sealed partial class LogicalAnd : BinaryOp {
        public LogicalAnd(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }

        public override Env Env => this.Right.Env;
    }

    /// <summary>
    /// Left || Right: can only take scalars (to compare with 0).
    /// 
    /// After semantic analysis, each operand can only be
    /// long, ulong, float, double.
    /// Pointers are casted to ulongs.
    /// 
    /// if Left != 0:
    ///     return 1
    /// else:
    ///     return Right != 0
    /// 
    /// Generate the assembly in this fashion,
    /// then every route would only have one jump.
    /// 
    ///        +---------+   1
    ///        | cmp lhs |-------+
    ///        +---------+       |
    ///             |            |
    ///             | 0          |
    ///             |            |
    ///        +----+----+   1   |
    ///        | cmp rhs |-------+
    ///        +---------+       |
    ///             |            |
    ///             | 0          |
    ///             |            |
    ///        +----+----+       |
    ///        | eax = 0 |       |
    ///        +---------+       |
    ///             |            |
    ///   +---------+            |
    ///   |                      |
    ///   |         +------------+ label_set
    ///   |         |
    ///   |    +---------+
    ///   |    | eax = 1 |
    ///   |    +---------+
    ///   |         |
    ///   +---------+ label_finish
    ///             |
    /// 
    /// </summary>
    public sealed partial class LogicalOr : BinaryOp {
        public LogicalOr(Expr left, Expr right, ExprType type)
            : base(left, right, type) { }
        
        public override Env Env => this.Right.Env;
    }
}
