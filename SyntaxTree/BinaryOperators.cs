using System;

namespace SyntaxTree {
    using static SemanticAnalysis;

    /// <summary>
    /// Binary operator: Left op Right
    /// </summary>
    public abstract class BinaryOp : Expr {
        protected BinaryOp(Expr left, Expr right) {
            this.Left = left;
            this.Right = right;
        }
        public Expr Left { get; }
        public Expr Right { get; }
    }

    /// <summary>
    /// Binary integral operator: takes in two integrals, returns an integer.
    /// </summary>
    public abstract class BinaryIntegralOp : BinaryOp {
        protected BinaryIntegralOp(Expr left, Expr right)
            : base(left, right) { }

        public abstract Int32 OperateLong(Int32 left, Int32 right);
        public abstract UInt32 OperateULong(UInt32 left, UInt32 right);
        public abstract AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type);

        public override AST.Expr GetExpr(AST.Env env) {

            // 1. semant operands
            var left = SemantExpr(this.Left, ref env);
            var right = SemantExpr(this.Right, ref env);

            // 2. perform usual arithmetic conversion
            Tuple<AST.Expr, AST.Expr, AST.ExprType.Kind> castReturn = AST.TypeCast.UsualArithmeticConversion(left, right);
            left = castReturn.Item1;
            right = castReturn.Item2;
            var typeKind = castReturn.Item3;

            var isConst = left.Type.is_const || right.Type.is_const;
            var isVolatile = left.Type.is_volatile || right.Type.is_volatile;

            // 3. if both operands are constants
            if (left.IsConstExpr && right.IsConstExpr) {
                switch (typeKind) {
                    case AST.ExprType.Kind.ULONG:
                        return new AST.ConstULong(OperateULong(((AST.ConstULong)left).value, ((AST.ConstULong)right).value), env);
                    case AST.ExprType.Kind.LONG:
                        return new AST.ConstLong(OperateLong(((AST.ConstLong)left).value, ((AST.ConstLong)right).value), env);
                    default:
                        throw new InvalidOperationException("Expected long or unsigned long.");
                }
            }

            // 4. if not both operands are constants
            switch (typeKind) {
                case AST.ExprType.Kind.ULONG:
                    return ConstructExpr(left, right, new AST.TULong(isConst, isVolatile));
                case AST.ExprType.Kind.LONG:
                    return ConstructExpr(left, right, new AST.TULong(isConst, isVolatile));
                default:
                    throw new InvalidOperationException("Expected long or unsigned long.");
            }

        }
    }

    /// <summary>
    /// Binary integral operator: takes in two int/uint/float/double, returns an int/uint/float/double.
    /// </summary>
    public abstract class BinaryArithmeticOp : BinaryIntegralOp {
        protected BinaryArithmeticOp(Expr left, Expr right)
            : base(left, right) { }

        public abstract Single OperateFloat(Single left, Single right);
        public abstract Double OperateDouble(Double left, Double right);

        public override AST.Expr GetExpr(AST.Env env) {

            // 1. semant operands
            var left = SemantExpr(this.Left, ref env);
            var right = SemantExpr(this.Right, ref env);

            // 2. perform usual arithmetic conversion
            Tuple<AST.Expr, AST.Expr, AST.ExprType.Kind> castReturn = AST.TypeCast.UsualArithmeticConversion(left, right);
            left = castReturn.Item1;
            right = castReturn.Item2;
            var typeKind = castReturn.Item3;

            var isConst = left.Type.is_const || right.Type.is_const;
            var isVolatile = left.Type.is_volatile || right.Type.is_volatile;

            // 3. if both operands are constants
            if (left.IsConstExpr && right.IsConstExpr) {
                switch (typeKind) {
                    case AST.ExprType.Kind.DOUBLE:
                        return new AST.ConstDouble(OperateDouble(((AST.ConstDouble)left).value, ((AST.ConstDouble)right).value), env);
                    case AST.ExprType.Kind.FLOAT:
                        return new AST.ConstFloat(OperateFloat(((AST.ConstFloat)left).value, ((AST.ConstFloat)right).value), env);
                    case AST.ExprType.Kind.ULONG:
                        return new AST.ConstULong(OperateULong(((AST.ConstULong)left).value, ((AST.ConstULong)right).value), env);
                    case AST.ExprType.Kind.LONG:
                        return new AST.ConstLong(OperateLong(((AST.ConstLong)left).value, ((AST.ConstLong)right).value), env);
                    default:
                        throw new InvalidOperationException("Expected arithmetic type.");
                }
            }

            // 4. if not both operands are constants
            switch (typeKind) {
                case AST.ExprType.Kind.DOUBLE:
                    return ConstructExpr(left, right, new AST.TDouble(isConst, isVolatile));
                case AST.ExprType.Kind.FLOAT:
                    return ConstructExpr(left, right, new AST.TFloat(isConst, isVolatile));
                case AST.ExprType.Kind.ULONG:
                    return ConstructExpr(left, right, new AST.TULong(isConst, isVolatile));
                case AST.ExprType.Kind.LONG:
                    return ConstructExpr(left, right, new AST.TLong(isConst, isVolatile));
                default:
                    throw new InvalidOperationException("Expected arithmetic type.");
            }

        }
    }

    /// <summary>
    /// Binary logical operator: first turn pointers to ulongs, then always returns long.
    /// </summary>
    public abstract class BinaryLogicalOp : BinaryOp {
        protected BinaryLogicalOp(Expr left, Expr right)
            : base(left, right) { }

        public abstract Int32 OperateLong(Int32 left, Int32 right);
        public abstract Int32 OperateULong(UInt32 left, UInt32 right);
        public abstract Int32 OperateFloat(Single left, Single right);
        public abstract Int32 OperateDouble(Double left, Double right);

        public abstract AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type);

        public override AST.Expr GetExpr(AST.Env env) {

            // 1. semant operands
            var left = SemantExpr(this.Left, ref env);
            var right = SemantExpr(this.Right, ref env);

            // 2. perform usual scalar conversion
            Tuple<AST.Expr, AST.Expr, AST.ExprType.Kind> castReturn = AST.TypeCast.UsualScalarConversion(left, right);
            left = castReturn.Item1;
            right = castReturn.Item2;
            var typeKind = castReturn.Item3;

            var isConst = left.Type.is_const || right.Type.is_const;
            var isVolatile = left.Type.is_volatile || right.Type.is_volatile;

            // 3. if both operands are constants
            if (left.IsConstExpr && right.IsConstExpr) {
                switch (typeKind) {
                    case AST.ExprType.Kind.DOUBLE:
                        return new AST.ConstLong(OperateDouble(((AST.ConstDouble)left).value, ((AST.ConstDouble)right).value), env);
                    case AST.ExprType.Kind.FLOAT:
                        return new AST.ConstLong(OperateFloat(((AST.ConstFloat)left).value, ((AST.ConstFloat)right).value), env);
                    case AST.ExprType.Kind.ULONG:
                        return new AST.ConstLong(OperateULong(((AST.ConstULong)left).value, ((AST.ConstULong)right).value), env);
                    case AST.ExprType.Kind.LONG:
                        return new AST.ConstLong(OperateLong(((AST.ConstLong)left).value, ((AST.ConstLong)right).value), env);
                    default:
                        throw new InvalidOperationException("Expected arithmetic type.");
                }
            }

            // 4. if not both operands are constants
            switch (typeKind) {
                case AST.ExprType.Kind.DOUBLE:
                    return ConstructExpr(left, right, new AST.TLong(isConst, isVolatile));
                case AST.ExprType.Kind.FLOAT:
                    return ConstructExpr(left, right, new AST.TLong(isConst, isVolatile));
                case AST.ExprType.Kind.ULONG:
                    return ConstructExpr(left, right, new AST.TLong(isConst, isVolatile));
                case AST.ExprType.Kind.LONG:
                    return ConstructExpr(left, right, new AST.TLong(isConst, isVolatile));
                default:
                    throw new InvalidOperationException("Expected arithmetic type.");
            }

        }
    }

    /// <summary>
    /// Multiplication: perform usual arithmetic conversion.
    /// </summary>
    public sealed class Multiply : BinaryArithmeticOp {
        private Multiply(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new Multiply(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => left * right;
        public override UInt32 OperateULong(UInt32 left, UInt32 right) => left * right;
        public override Single OperateFloat(Single left, Single right) => left * right;
        public override Double OperateDouble(Double left, Double right) => left * right;

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.Multiply(left, right, type);
    }

    /// <summary>
    /// Division: perform usual arithmetic conversion.
    /// </summary>
    public sealed class Divide : BinaryArithmeticOp {
        private Divide(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new Divide(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => left / right;
        public override UInt32 OperateULong(UInt32 left, UInt32 right) => left / right;
        public override Single OperateFloat(Single left, Single right) => left / right;
        public override Double OperateDouble(Double left, Double right) => left / right;

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.Divide(left, right, type);
    }

    /// <summary>
    /// Modulo: only accepts integrals.
    /// </summary>
    public sealed class Modulo : BinaryIntegralOp {
        private Modulo(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new Modulo(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => left % right;
        public override UInt32 OperateULong(UInt32 left, UInt32 right) => left % right;

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.Modulo(left, right, type);
    }

    /// <summary>
    /// Addition
    /// 
    /// There are two kinds of addition:
    /// 1. both operands are of arithmetic type
    /// 2. one operand is a pointer, and the other is an integral
    /// 
    /// </summary>
    public sealed class Add : BinaryArithmeticOp {
        private Add(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new Add(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => left + right;
        public override UInt32 OperateULong(UInt32 left, UInt32 right) => left + right;
        public override Single OperateFloat(Single left, Single right) => left + right;
        public override Double OperateDouble(Double left, Double right) => left + right;

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.Add(left, right, type);

        public AST.Expr GetPointerAddition(AST.Expr ptr, AST.Expr offset, Boolean order = true) {
            if (ptr.Type.kind != AST.ExprType.Kind.POINTER) {
                throw new InvalidOperationException();
            }
            if (offset.Type.kind != AST.ExprType.Kind.LONG) {
                throw new InvalidOperationException();
            }

            var env = order ? ptr.Env : offset.Env;

            if (ptr.IsConstExpr && offset.IsConstExpr) {
                var baseValue = (Int32)((AST.ConstPtr)ptr).value;
                Int32 scaleValue = ((AST.TPointer)(ptr.Type)).ref_t.SizeOf;
                Int32 offsetValue = ((AST.ConstLong)offset).value;
                return new AST.ConstPtr((UInt32)(baseValue + scaleValue * offsetValue), ptr.Type, env);
            }

            var baseAddress = AST.TypeCast.FromPointer(ptr, new AST.TLong(ptr.Type.is_const, ptr.Type.is_volatile), ptr.Env);
            var scaleFactor = new AST.Multiply(
                offset,
                new AST.ConstLong(((AST.TPointer)(ptr.Type)).ref_t.SizeOf, env),
                new AST.TLong(offset.Type.is_const, offset.Type.is_volatile)
            );
            var type = new AST.TLong(offset.Type.is_const, offset.Type.is_volatile);
            var add =
                order
                ? new AST.Add(baseAddress, scaleFactor, type)
                : new AST.Add(scaleFactor, baseAddress, type);

            return AST.TypeCast.ToPointer(add, ptr.Type, env);
        }

        public override AST.Expr GetExpr(AST.Env env) {

            // 1. semant the operands
            var left = SemantExpr(this.Left, ref env);
            var right = SemantExpr(this.Right, ref env);

            if (left.Type is AST.TArray) {
                left = AST.TypeCast.MakeCast(left, new AST.TPointer((left.Type as AST.TArray).elem_type, left.Type.is_const, left.Type.is_volatile));
            }

            if (right.Type is AST.TArray) {
                right = AST.TypeCast.MakeCast(right, new AST.TPointer((right.Type as AST.TArray).elem_type, right.Type.is_const, right.Type.is_volatile));
            }

            // 2. ptr + int
            if (left.Type.kind == AST.ExprType.Kind.POINTER) {
                if (!right.Type.IsIntegral) {
                    throw new InvalidOperationException("Expected integral to be added to a pointer.");
                }
                right = AST.TypeCast.MakeCast(right, new AST.TLong(right.Type.is_const, right.Type.is_volatile));
                return GetPointerAddition(left, right);
            }

            // 3. int + ptr
            if (right.Type.kind == AST.ExprType.Kind.POINTER) {
                if (!left.Type.IsIntegral) {
                    throw new InvalidOperationException("Expected integral to be added to a pointer.");
                }
                left = AST.TypeCast.MakeCast(left, new AST.TLong(left.Type.is_const, left.Type.is_volatile));
                return GetPointerAddition(right, left, false);
            }

            // 4. usual arithmetic conversion
            return base.GetExpr(env);

        }
    }

    /// <summary>
    /// Subtraction
    /// 
    /// There are three kinds of subtractions:
    /// 1. arithmetic - arithmetic
    /// 2. pointer - integral
    /// 3. pointer - pointer
    /// </summary>
    public sealed class Sub : BinaryArithmeticOp {
        private Sub(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new Sub(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => left - right;
        public override UInt32 OperateULong(UInt32 left, UInt32 right) => left - right;
        public override Single OperateFloat(Single left, Single right) => left - right;
        public override Double OperateDouble(Double left, Double right) => left - right;

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.Sub(left, right, type);

        public static AST.Expr GetPointerSubtraction(AST.Expr ptr, AST.Expr offset) {
            if (ptr.Type.kind != AST.ExprType.Kind.POINTER) {
                throw new InvalidOperationException("Error: expect a pointer");
            }
            if (offset.Type.kind != AST.ExprType.Kind.LONG) {
                throw new InvalidOperationException("Error: expect an integer");
            }

            if (ptr.IsConstExpr && offset.IsConstExpr) {
                Int32 baseAddressValue = (Int32)((AST.ConstPtr)ptr).value;
                Int32 scaleFactorValue = ((AST.TPointer)(ptr.Type)).ref_t.SizeOf;
                Int32 offsetValue = ((AST.ConstLong)offset).value;
                return new AST.ConstPtr((UInt32)(baseAddressValue - scaleFactorValue * offsetValue), ptr.Type, offset.Env);
            }

            return AST.TypeCast.ToPointer(new AST.Sub(
                    AST.TypeCast.FromPointer(ptr, new AST.TLong(ptr.Type.is_const, ptr.Type.is_volatile), ptr.Env),
                    new AST.Multiply(
                        offset,
                        new AST.ConstLong(((AST.TPointer)(ptr.Type)).ref_t.SizeOf, offset.Env),
                        new AST.TLong(offset.Type.is_const, offset.Type.is_volatile)
                    ),
                    new AST.TLong(offset.Type.is_const, offset.Type.is_volatile)
                ), ptr.Type, offset.Env
            );
        }

        public override AST.Expr GetExpr(AST.Env env) {

            var left = SemantExpr(this.Left, ref env);
            var right = SemantExpr(this.Right, ref env);

            if (left.Type is AST.TArray) {
                left = AST.TypeCast.MakeCast(left, new AST.TPointer((left.Type as AST.TArray).elem_type, left.Type.is_const, left.Type.is_volatile));
            }

            if (right.Type is AST.TArray) {
                right = AST.TypeCast.MakeCast(right, new AST.TPointer((right.Type as AST.TArray).elem_type, right.Type.is_const, right.Type.is_volatile));
            }

            var isConst = left.Type.is_const || right.Type.is_const;
            var isVolatile = left.Type.is_volatile || right.Type.is_volatile;

            if (left.Type.kind == AST.ExprType.Kind.POINTER) {

                // 1. ptr - ptr
                if (right.Type.kind == AST.ExprType.Kind.POINTER) {
                    AST.TPointer leftType = (AST.TPointer)(left.Type);
                    AST.TPointer rightType = (AST.TPointer)(right.Type);
                    if (!leftType.ref_t.EqualType(rightType.ref_t)) {
                        throw new InvalidOperationException("The 2 pointers don't match.");
                    }

                    Int32 scale = leftType.ref_t.SizeOf;

                    if (left.IsConstExpr && right.IsConstExpr) {
                        return new AST.ConstLong((Int32)(((AST.ConstPtr)left).value - ((AST.ConstPtr)right).value) / scale, env);
                    }

                    return new AST.Divide(
                        new AST.Sub(
                            AST.TypeCast.MakeCast(left, new AST.TLong(isConst, isVolatile)),
                            AST.TypeCast.MakeCast(right, new AST.TLong(isConst, isVolatile)),
                            new AST.TLong(isConst, isVolatile)
                        ),
                        new AST.ConstLong(scale, env),
                        new AST.TLong(isConst, isVolatile)
                    );
                }

                // 2. ptr - integral
                if (!right.Type.IsIntegral) {
                    throw new InvalidOperationException("Expected an integral.");
                }
                right = AST.TypeCast.MakeCast(right, new AST.TLong(right.Type.is_const, right.Type.is_volatile));
                return GetPointerSubtraction(left, right);

            }

            // 3. arith - arith
            return base.GetExpr(env);
        }
    }

    /// <summary>
    /// Left Shift: takes in two integrals, returns an integer.
    /// </summary>
    public sealed class LShift : BinaryIntegralOp {
        private LShift(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new LShift(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => left << right;
        public override UInt32 OperateULong(UInt32 left, UInt32 right) => (UInt32)((Int32)left << (Int32)right);

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.LShift(left, right, type);
    }

    /// <summary>
    /// Right Shift: takes in two integrals, returns an integer;
    /// </summary>
    public sealed class RShift : BinaryIntegralOp {
        private RShift(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new RShift(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => left >> right;
        public override UInt32 OperateULong(UInt32 left, UInt32 right) => (UInt32)((Int32)left >> (Int32)right);

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.RShift(left, right, type);

    }

    /// <summary>
    /// Less than
    /// </summary>
    public sealed class Less : BinaryLogicalOp {
        private Less(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new Less(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => Convert.ToInt32(left < right);
        public override Int32 OperateULong(UInt32 left, UInt32 right) => Convert.ToInt32(left < right);
        public override Int32 OperateFloat(Single left, Single right) => Convert.ToInt32(left < right);
        public override Int32 OperateDouble(Double left, Double right) => Convert.ToInt32(left < right);

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.Less(left, right, type);
    }

    /// <summary>
    /// Less or Equal than
    /// </summary>
    public sealed class LEqual : BinaryLogicalOp {
        private LEqual(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new LEqual(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => Convert.ToInt32(left <= right);
        public override Int32 OperateULong(UInt32 left, UInt32 right) => Convert.ToInt32(left <= right);
        public override Int32 OperateFloat(Single left, Single right) => Convert.ToInt32(left <= right);
        public override Int32 OperateDouble(Double left, Double right) => Convert.ToInt32(left <= right);

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.LEqual(left, right, type);
    }

    /// <summary>
    /// Greater than
    /// </summary>
	public sealed class Greater : BinaryLogicalOp {
        private Greater(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new Greater(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => Convert.ToInt32(left > right);
        public override Int32 OperateULong(UInt32 left, UInt32 right) => Convert.ToInt32(left > right);
        public override Int32 OperateFloat(Single left, Single right) => Convert.ToInt32(left > right);
        public override Int32 OperateDouble(Double left, Double right) => Convert.ToInt32(left > right);

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.Greater(left, right, type);
    }

    /// <summary>
    /// Greater or Equal than
    /// </summary>
    public sealed class GEqual : BinaryLogicalOp {
        private GEqual(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new GEqual(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => Convert.ToInt32(left >= right);
        public override Int32 OperateULong(UInt32 left, UInt32 right) => Convert.ToInt32(left >= right);
        public override Int32 OperateFloat(Single left, Single right) => Convert.ToInt32(left >= right);
        public override Int32 OperateDouble(Double left, Double right) => Convert.ToInt32(left >= right);

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.GEqual(left, right, type);
    }

    /// <summary>
    /// Equal
    /// </summary>
	public sealed class Equal : BinaryLogicalOp {
        private Equal(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new Equal(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => Convert.ToInt32(left == right);
        public override Int32 OperateULong(UInt32 left, UInt32 right) => Convert.ToInt32(left == right);
        public override Int32 OperateFloat(Single left, Single right) => Convert.ToInt32(left == right);
        public override Int32 OperateDouble(Double left, Double right) => Convert.ToInt32(left == right);

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.Equal(left, right, type);
    }

    /// <summary>
    /// Not equal
    /// </summary>
    public sealed class NotEqual : BinaryLogicalOp {
        private NotEqual(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new NotEqual(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => Convert.ToInt32(left != right);
        public override Int32 OperateULong(UInt32 left, UInt32 right) => Convert.ToInt32(left != right);
        public override Int32 OperateFloat(Single left, Single right) => Convert.ToInt32(left != right);
        public override Int32 OperateDouble(Double left, Double right) => Convert.ToInt32(left != right);

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.NotEqual(left, right, type);
    }

    /// <summary>
    /// Bitwise And: returns an integer.
    /// </summary>
    public sealed class BitwiseAnd : BinaryIntegralOp {
        private BitwiseAnd(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new BitwiseAnd(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => left & right;
        public override UInt32 OperateULong(UInt32 left, UInt32 right) => left & right;

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.BitwiseAnd(left, right, type);

    }

    /// <summary>
    /// Xor: returns an integer.
    /// </summary>
    public sealed class Xor : BinaryIntegralOp {
        private Xor(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new Xor(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => left ^ right;
        public override UInt32 OperateULong(UInt32 left, UInt32 right) => left ^ right;

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.Xor(left, right, type);
    }

    /// <summary>
    /// Bitwise Or: accepts two integrals, returns an integer.
    /// </summary>
    public sealed class BitwiseOr : BinaryIntegralOp {
        private BitwiseOr(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new BitwiseOr(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => left | right;
        public override UInt32 OperateULong(UInt32 left, UInt32 right) => left | right;

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.BitwiseOr(left, right, type);
    }

    /// <summary>
    /// Logical and: both operands need to be non-zero.
    /// </summary>
	public sealed class LogicalAnd : BinaryLogicalOp {
        private LogicalAnd(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new LogicalAnd(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => Convert.ToInt32(left != 0 && right != 0);
        public override Int32 OperateULong(UInt32 left, UInt32 right) => Convert.ToInt32(left != 0 && right != 0);
        public override Int32 OperateFloat(Single left, Single right) => Convert.ToInt32(left != 0 && right != 0);
        public override Int32 OperateDouble(Double left, Double right) => Convert.ToInt32(left != 0 && right != 0);

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.LogicalAnd(left, right, type);
    }

    /// <summary>
    /// Logical or: at least one of operands needs to be non-zero.
    /// </summary>
	public sealed class LogicalOr : BinaryLogicalOp {
        private LogicalOr(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) =>
            new LogicalOr(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => Convert.ToInt32(left != 0 || right != 0);
        public override Int32 OperateULong(UInt32 left, UInt32 right) => Convert.ToInt32(left != 0 || right != 0);
        public override Int32 OperateFloat(Single left, Single right) => Convert.ToInt32(left != 0 || right != 0);
        public override Int32 OperateDouble(Double left, Double right) => Convert.ToInt32(left != 0 || right != 0);

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.LogicalOr(left, right, type);
    }
}
