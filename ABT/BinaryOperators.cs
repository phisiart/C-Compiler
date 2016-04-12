using System;
using System.Diagnostics;
using CodeGeneration;

namespace ABT {

    // BinaryOp [type = abstract]
    //   |
    //   +-- BinaryOpSupportingIntegralOperands [typeof(left) == typeof(right)]
    //   |     | ([abstract Operate{Long, ULong},
    //   |     |  CGenIntegral)
    //   |     |
    //   |     +-- BinaryOpSupportingOnlyIntegralOperands [type = typeof(left)]
    //   |     |   ([sealed] CGenValue = CGenIntegral)
    //   |     |   (Modulo, Xor, BitwiseOr, BitwiseAnd, LShift, RShift)
    //   |     |
    //   |     +-- BinaryOpSupportingArithmeticOperands
    //   |           | (CGenValue { try float, then base.CGenValue })
    //   |           |
    //   |           +-- BinaryArithmeticOp [type = typeof(left)]
    //   |           |   ([abstract] Operate{Long, ULong, Float, Double})
    //   |           |   (Add, Sub, Mult, Div)
    //   |           |
    //   |           +-- BinaryComparisonOp [type = int]
    //   |               ([sealed]   CGenValue,
    //   |                [sealed]   Operate{Long, ULong, Float, Double},
    //   |                [abstract] Set{Long, ULong, Float, Double})
    //   |               (GEqual, Greater, LEqual, Less, Equal, NotEqual)
    //   |           
    //   +-- BinaryLogicalOp [type = int]
    //       ([abstract] CGenValue)
    //       (LogicalAnd, LogicalOr)

    public abstract partial class BinaryOp : Expr {
        protected BinaryOp(Expr left, Expr right) {
            this.Left = left;
            this.Right = right;
        }

        public Expr Left { get; }

        public Expr Right { get; }

        public override abstract ExprType Type { get; }

        public override sealed Env Env => this.Right.Env;

        public override sealed Boolean IsLValue => false;
    }

    public abstract partial class BinaryOpSupportingIntegralOperands : BinaryOp {
        protected BinaryOpSupportingIntegralOperands(Expr left, Expr right)
            : base(left, right) {
            if (!left.Type.EqualType(right.Type)) {
                throw new InvalidOperationException("Operand types mismatch.");
            }
        }
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
    public abstract partial class BinaryOpSupportingOnlyIntegralOperands : BinaryOpSupportingIntegralOperands {
        protected BinaryOpSupportingOnlyIntegralOperands(Expr left, Expr right)
            : base(left, right) {
            if (!(left.Type is LongType || left.Type is ULongType)) {
                throw new InvalidOperationException("Only support long or ulong.");
            }
            this.Type = left.Type.GetQualifiedType(true, false);
        }

        public override sealed ExprType Type { get; }
    }

    public abstract partial class BinaryOpSupportingArithmeticOperands : BinaryOpSupportingIntegralOperands {
        protected BinaryOpSupportingArithmeticOperands(Expr left, Expr right)
            : base(left, right) {
            if (!(left.Type is LongType || left.Type is ULongType
                  || left.Type is FloatType || left.Type is DoubleType)) {
                throw new InvalidOperationException("Only support long, ulong, float, double.");
            }
        }
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
    public abstract partial class BinaryArithmeticOp : BinaryOpSupportingArithmeticOperands {
        protected BinaryArithmeticOp(Expr left, Expr right)
            : base(left, right) {
            this.Type = left.Type.GetQualifiedType(true, false);
        }

        public override sealed ExprType Type { get; }
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
    public abstract partial class BinaryComparisonOp : BinaryOpSupportingArithmeticOperands {
        protected BinaryComparisonOp(Expr left, Expr right)
            : base(left, right) { }

        private static ExprType _type = new ABT.LongType(true);
        public override sealed ExprType Type => _type;
    }

    public abstract partial class BinaryLogicalOp : BinaryOp {
        protected BinaryLogicalOp(Expr left, Expr right)
            : base(left, right) {
            if (!(left.Type is LongType || left.Type is ULongType
                  || left.Type is FloatType || left.Type is DoubleType)) {
                throw new InvalidOperationException("Invalid operand type.");
            }
            if (!(right.Type is LongType || right.Type is ULongType
                  || right.Type is FloatType || right.Type is DoubleType)) {
                throw new InvalidOperationException("Invalid operand type.");
            }
        }

        private static ExprType _type = new ABT.LongType(true);
        public override sealed ExprType Type => _type;
    }

    /// <summary>
    /// The modulo (%) operator can only take integral operands.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long % long
    /// 2) ulong % ulong
    /// </summary>
    public sealed partial class Modulo : BinaryOpSupportingOnlyIntegralOperands {
        public Modulo(Expr left, Expr right)
            : base(left, right) { }
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
    public sealed partial class Xor : BinaryOpSupportingOnlyIntegralOperands {
        public Xor(Expr left, Expr right)
            : base(left, right) { }
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
    public sealed partial class BitwiseOr : BinaryOpSupportingOnlyIntegralOperands {
        public BitwiseOr(Expr left, Expr right)
            : base(left, right) { }
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
    public sealed partial class BitwiseAnd : BinaryOpSupportingOnlyIntegralOperands {
        public BitwiseAnd(Expr left, Expr right)
            : base(left, right) { }
    }

    /// <summary>
    /// The left shift operator can only take integral operands.
    /// Append 0's on the right.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long %lt;%lt; long
    /// 2) ulong %lt;%lt; ulong
    /// </summary>
    public sealed partial class LShift : BinaryOpSupportingOnlyIntegralOperands {
        public LShift(Expr left, Expr right)
            : base(left, right) { }
    }

    /// <summary>
    /// The right shift operator can only take integral operands.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long >> long (arithmetic shift, append sign bit)
    /// 2) ulong >> ulong (logical shift, append 0)
    /// </summary>
    public sealed partial class RShift : BinaryOpSupportingOnlyIntegralOperands {
        public RShift(Expr left, Expr right)
            : base(left, right) { }
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
        public Add(Expr left, Expr right)
            : base(left, right) { }
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
        public Sub(Expr left, Expr right)
            : base(left, right) { }
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
        public Multiply(Expr left, Expr right)
            : base(left, right) { }
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
        public Divide(Expr left, Expr right)
            : base(left, right) { }
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
    public sealed partial class GEqual : BinaryComparisonOp {
        public GEqual(Expr left, Expr right)
            : base(left, right) { }
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
    public sealed partial class Greater : BinaryComparisonOp {
        public Greater(Expr left, Expr right)
            : base(left, right) { }
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
    public sealed partial class LEqual : BinaryComparisonOp {
        public LEqual(Expr left, Expr right)
            : base(left, right) { }
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
    public sealed partial class Less : BinaryComparisonOp {
        public Less(Expr left, Expr right)
            : base(left, right) { }
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
    public sealed partial class Equal : BinaryComparisonOp {
        public Equal(Expr left, Expr right)
            : base(left, right) { }
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
    public sealed partial class NotEqual : BinaryComparisonOp {
        public NotEqual(Expr left, Expr right)
            : base(left, right) { }
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
    public sealed partial class LogicalAnd : BinaryLogicalOp {
        public LogicalAnd(Expr left, Expr right)
            : base(left, right) { }
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
    /// </summary>
    public sealed partial class LogicalOr : BinaryLogicalOp {
        public LogicalOr(Expr left, Expr right)
            : base(left, right) { }        
    }
}
