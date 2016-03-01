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
            Tuple<AST.Expr, AST.Expr, AST.ExprTypeKind> castReturn = AST.TypeCast.UsualArithmeticConversion(left, right);
            left = castReturn.Item1;
            right = castReturn.Item2;
            var typeKind = castReturn.Item3;

            var isConst = left.Type.IsConst || right.Type.IsConst;
            var isVolatile = left.Type.IsVolatile || right.Type.IsVolatile;

            // 3. if both operands are constants
            if (left.IsConstExpr && right.IsConstExpr) {
                switch (typeKind) {
                    case AST.ExprTypeKind.ULONG:
                        return new AST.ConstULong(OperateULong(((AST.ConstULong)left).Value, ((AST.ConstULong)right).Value), env);
                    case AST.ExprTypeKind.LONG:
                        return new AST.ConstLong(OperateLong(((AST.ConstLong)left).Value, ((AST.ConstLong)right).Value), env);
                    default:
                        throw new InvalidOperationException("Expected long or unsigned long.");
                }
            }

            // 4. if not both operands are constants
            switch (typeKind) {
                case AST.ExprTypeKind.ULONG:
                    return ConstructExpr(left, right, new AST.ULongType(isConst, isVolatile));
                case AST.ExprTypeKind.LONG:
                    return ConstructExpr(left, right, new AST.ULongType(isConst, isVolatile));
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
            Tuple<AST.Expr, AST.Expr, AST.ExprTypeKind> castReturn = AST.TypeCast.UsualArithmeticConversion(left, right);
            left = castReturn.Item1;
            right = castReturn.Item2;
            var typeKind = castReturn.Item3;

            var isConst = left.Type.IsConst || right.Type.IsConst;
            var isVolatile = left.Type.IsVolatile || right.Type.IsVolatile;

            // 3. if both operands are constants
            if (left.IsConstExpr && right.IsConstExpr) {
                switch (typeKind) {
                    case AST.ExprTypeKind.DOUBLE:
                        return new AST.ConstDouble(OperateDouble(((AST.ConstDouble)left).Value, ((AST.ConstDouble)right).Value), env);
                    case AST.ExprTypeKind.FLOAT:
                        return new AST.ConstFloat(OperateFloat(((AST.ConstFloat)left).Value, ((AST.ConstFloat)right).Value), env);
                    case AST.ExprTypeKind.ULONG:
                        return new AST.ConstULong(OperateULong(((AST.ConstULong)left).Value, ((AST.ConstULong)right).Value), env);
                    case AST.ExprTypeKind.LONG:
                        return new AST.ConstLong(OperateLong(((AST.ConstLong)left).Value, ((AST.ConstLong)right).Value), env);
                    default:
                        throw new InvalidOperationException("Expected arithmetic Type.");
                }
            }

            // 4. if not both operands are constants
            switch (typeKind) {
                case AST.ExprTypeKind.DOUBLE:
                    return ConstructExpr(left, right, new AST.DoubleType(isConst, isVolatile));
                case AST.ExprTypeKind.FLOAT:
                    return ConstructExpr(left, right, new AST.FloatType(isConst, isVolatile));
                case AST.ExprTypeKind.ULONG:
                    return ConstructExpr(left, right, new AST.ULongType(isConst, isVolatile));
                case AST.ExprTypeKind.LONG:
                    return ConstructExpr(left, right, new AST.LongType(isConst, isVolatile));
                default:
                    throw new InvalidOperationException("Expected arithmetic Type.");
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
            Tuple<AST.Expr, AST.Expr, AST.ExprTypeKind> castReturn = AST.TypeCast.UsualScalarConversion(left, right);
            left = castReturn.Item1;
            right = castReturn.Item2;
            var typeKind = castReturn.Item3;

            var isConst = left.Type.IsConst || right.Type.IsConst;
            var isVolatile = left.Type.IsVolatile || right.Type.IsVolatile;

            // 3. if both operands are constants
            if (left.IsConstExpr && right.IsConstExpr) {
                switch (typeKind) {
                    case AST.ExprTypeKind.DOUBLE:
                        return new AST.ConstLong(OperateDouble(((AST.ConstDouble)left).Value, ((AST.ConstDouble)right).Value), env);
                    case AST.ExprTypeKind.FLOAT:
                        return new AST.ConstLong(OperateFloat(((AST.ConstFloat)left).Value, ((AST.ConstFloat)right).Value), env);
                    case AST.ExprTypeKind.ULONG:
                        return new AST.ConstLong(OperateULong(((AST.ConstULong)left).Value, ((AST.ConstULong)right).Value), env);
                    case AST.ExprTypeKind.LONG:
                        return new AST.ConstLong(OperateLong(((AST.ConstLong)left).Value, ((AST.ConstLong)right).Value), env);
                    default:
                        throw new InvalidOperationException("Expected arithmetic Type.");
                }
            }

            // 4. if not both operands are constants
            switch (typeKind) {
                case AST.ExprTypeKind.DOUBLE:
                    return ConstructExpr(left, right, new AST.LongType(isConst, isVolatile));
                case AST.ExprTypeKind.FLOAT:
                    return ConstructExpr(left, right, new AST.LongType(isConst, isVolatile));
                case AST.ExprTypeKind.ULONG:
                    return ConstructExpr(left, right, new AST.LongType(isConst, isVolatile));
                case AST.ExprTypeKind.LONG:
                    return ConstructExpr(left, right, new AST.LongType(isConst, isVolatile));
                default:
                    throw new InvalidOperationException("Expected arithmetic Type.");
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
    /// 1. both operands are of arithmetic Type
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
            if (ptr.Type.Kind != AST.ExprTypeKind.POINTER) {
                throw new InvalidOperationException();
            }
            if (offset.Type.Kind != AST.ExprTypeKind.LONG) {
                throw new InvalidOperationException();
            }

            var env = order ? ptr.Env : offset.Env;

            if (ptr.IsConstExpr && offset.IsConstExpr) {
                var baseValue = (Int32)((AST.ConstPtr)ptr).Value;
                Int32 scaleValue = ((AST.PointerType)(ptr.Type)).RefType.SizeOf;
                Int32 offsetValue = ((AST.ConstLong)offset).Value;
                return new AST.ConstPtr((UInt32)(baseValue + scaleValue * offsetValue), ptr.Type, env);
            }

            var baseAddress = AST.TypeCast.FromPointer(ptr, new AST.LongType(ptr.Type.IsConst, ptr.Type.IsVolatile), ptr.Env);
            var scaleFactor = new AST.Multiply(
                offset,
                new AST.ConstLong(((AST.PointerType)(ptr.Type)).RefType.SizeOf, env),
                new AST.LongType(offset.Type.IsConst, offset.Type.IsVolatile)
            );
            var type = new AST.LongType(offset.Type.IsConst, offset.Type.IsVolatile);
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

            if (left.Type is AST.ArrayType) {
                left = AST.TypeCast.MakeCast(left, new AST.PointerType(((AST.ArrayType)left.Type).ElemType, left.Type.IsConst, left.Type.IsVolatile));
            }

            if (right.Type is AST.ArrayType) {
                right = AST.TypeCast.MakeCast(right, new AST.PointerType(((AST.ArrayType)right.Type).ElemType, right.Type.IsConst, right.Type.IsVolatile));
            }

            // 2. ptr + int
            if (left.Type.Kind == AST.ExprTypeKind.POINTER) {
                if (!right.Type.IsIntegral) {
                    throw new InvalidOperationException("Expected integral to be added to a pointer.");
                }
                right = AST.TypeCast.MakeCast(right, new AST.LongType(right.Type.IsConst, right.Type.IsVolatile));
                return GetPointerAddition(left, right);
            }

            // 3. int + ptr
            if (right.Type.Kind == AST.ExprTypeKind.POINTER) {
                if (!left.Type.IsIntegral) {
                    throw new InvalidOperationException("Expected integral to be added to a pointer.");
                }
                left = AST.TypeCast.MakeCast(left, new AST.LongType(left.Type.IsConst, left.Type.IsVolatile));
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
            if (ptr.Type.Kind != AST.ExprTypeKind.POINTER) {
                throw new InvalidOperationException("Error: expect a pointer");
            }
            if (offset.Type.Kind != AST.ExprTypeKind.LONG) {
                throw new InvalidOperationException("Error: expect an integer");
            }

            if (ptr.IsConstExpr && offset.IsConstExpr) {
                Int32 baseAddressValue = (Int32)((AST.ConstPtr)ptr).Value;
                Int32 scaleFactorValue = ((AST.PointerType)(ptr.Type)).RefType.SizeOf;
                Int32 offsetValue = ((AST.ConstLong)offset).Value;
                return new AST.ConstPtr((UInt32)(baseAddressValue - scaleFactorValue * offsetValue), ptr.Type, offset.Env);
            }

            return AST.TypeCast.ToPointer(new AST.Sub(
                    AST.TypeCast.FromPointer(ptr, new AST.LongType(ptr.Type.IsConst, ptr.Type.IsVolatile), ptr.Env),
                    new AST.Multiply(
                        offset,
                        new AST.ConstLong(((AST.PointerType)(ptr.Type)).RefType.SizeOf, offset.Env),
                        new AST.LongType(offset.Type.IsConst, offset.Type.IsVolatile)
                    ),
                    new AST.LongType(offset.Type.IsConst, offset.Type.IsVolatile)
                ), ptr.Type, offset.Env
            );
        }

        public override AST.Expr GetExpr(AST.Env env) {

            var left = SemantExpr(this.Left, ref env);
            var right = SemantExpr(this.Right, ref env);

            if (left.Type is AST.ArrayType) {
                left = AST.TypeCast.MakeCast(left, new AST.PointerType((left.Type as AST.ArrayType).ElemType, left.Type.IsConst, left.Type.IsVolatile));
            }

            if (right.Type is AST.ArrayType) {
                right = AST.TypeCast.MakeCast(right, new AST.PointerType((right.Type as AST.ArrayType).ElemType, right.Type.IsConst, right.Type.IsVolatile));
            }

            var isConst = left.Type.IsConst || right.Type.IsConst;
            var isVolatile = left.Type.IsVolatile || right.Type.IsVolatile;

            if (left.Type.Kind == AST.ExprTypeKind.POINTER) {

                // 1. ptr - ptr
                if (right.Type.Kind == AST.ExprTypeKind.POINTER) {
                    AST.PointerType leftType = (AST.PointerType)(left.Type);
                    AST.PointerType rightType = (AST.PointerType)(right.Type);
                    if (!leftType.RefType.EqualType(rightType.RefType)) {
                        throw new InvalidOperationException("The 2 pointers don't match.");
                    }

                    Int32 scale = leftType.RefType.SizeOf;

                    if (left.IsConstExpr && right.IsConstExpr) {
                        return new AST.ConstLong((Int32)(((AST.ConstPtr)left).Value - ((AST.ConstPtr)right).Value) / scale, env);
                    }

                    return new AST.Divide(
                        new AST.Sub(
                            AST.TypeCast.MakeCast(left, new AST.LongType(isConst, isVolatile)),
                            AST.TypeCast.MakeCast(right, new AST.LongType(isConst, isVolatile)),
                            new AST.LongType(isConst, isVolatile)
                        ),
                        new AST.ConstLong(scale, env),
                        new AST.LongType(isConst, isVolatile)
                    );
                }

                // 2. ptr - integral
                if (!right.Type.IsIntegral) {
                    throw new InvalidOperationException("Expected an integral.");
                }
                right = AST.TypeCast.MakeCast(right, new AST.LongType(right.Type.IsConst, right.Type.IsVolatile));
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
