using System;

namespace AST {
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
        public abstract ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type);

        public override ABT.Expr GetExpr(ABT.Env env) {

            // 1. semant operands
            var left = SemantExpr(this.Left, ref env);
            var right = SemantExpr(this.Right, ref env);

            // 2. perform usual arithmetic conversion
            Tuple<ABT.Expr, ABT.Expr, ABT.ExprTypeKind> castReturn = ABT.TypeCast.UsualArithmeticConversion(left, right);
            left = castReturn.Item1;
            right = castReturn.Item2;
            var typeKind = castReturn.Item3;

            var isConst = left.Type.IsConst || right.Type.IsConst;
            var isVolatile = left.Type.IsVolatile || right.Type.IsVolatile;

            // 3. if both operands are constants
            if (left.IsConstExpr && right.IsConstExpr) {
                switch (typeKind) {
                    case ABT.ExprTypeKind.ULONG:
                        return new ABT.ConstULong(OperateULong(((ABT.ConstULong)left).Value, ((ABT.ConstULong)right).Value), env);
                    case ABT.ExprTypeKind.LONG:
                        return new ABT.ConstLong(OperateLong(((ABT.ConstLong)left).Value, ((ABT.ConstLong)right).Value), env);
                    default:
                        throw new InvalidOperationException("Expected long or unsigned long.");
                }
            }

            // 4. if not both operands are constants
            switch (typeKind) {
                case ABT.ExprTypeKind.ULONG:
                    return ConstructExpr(left, right, new ABT.ULongType(isConst, isVolatile));
                case ABT.ExprTypeKind.LONG:
                    return ConstructExpr(left, right, new ABT.ULongType(isConst, isVolatile));
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

        public override ABT.Expr GetExpr(ABT.Env env) {

            // 1. semant operands
            var left = SemantExpr(this.Left, ref env);
            var right = SemantExpr(this.Right, ref env);

            // 2. perform usual arithmetic conversion
            Tuple<ABT.Expr, ABT.Expr, ABT.ExprTypeKind> castReturn = ABT.TypeCast.UsualArithmeticConversion(left, right);
            left = castReturn.Item1;
            right = castReturn.Item2;
            var typeKind = castReturn.Item3;

            var isConst = left.Type.IsConst || right.Type.IsConst;
            var isVolatile = left.Type.IsVolatile || right.Type.IsVolatile;

            // 3. if both operands are constants
            if (left.IsConstExpr && right.IsConstExpr) {
                switch (typeKind) {
                    case ABT.ExprTypeKind.DOUBLE:
                        return new ABT.ConstDouble(OperateDouble(((ABT.ConstDouble)left).Value, ((ABT.ConstDouble)right).Value), env);
                    case ABT.ExprTypeKind.FLOAT:
                        return new ABT.ConstFloat(OperateFloat(((ABT.ConstFloat)left).Value, ((ABT.ConstFloat)right).Value), env);
                    case ABT.ExprTypeKind.ULONG:
                        return new ABT.ConstULong(OperateULong(((ABT.ConstULong)left).Value, ((ABT.ConstULong)right).Value), env);
                    case ABT.ExprTypeKind.LONG:
                        return new ABT.ConstLong(OperateLong(((ABT.ConstLong)left).Value, ((ABT.ConstLong)right).Value), env);
                    default:
                        throw new InvalidOperationException("Expected arithmetic Type.");
                }
            }

            // 4. if not both operands are constants
            switch (typeKind) {
                case ABT.ExprTypeKind.DOUBLE:
                    return ConstructExpr(left, right, new ABT.DoubleType(isConst, isVolatile));
                case ABT.ExprTypeKind.FLOAT:
                    return ConstructExpr(left, right, new ABT.FloatType(isConst, isVolatile));
                case ABT.ExprTypeKind.ULONG:
                    return ConstructExpr(left, right, new ABT.ULongType(isConst, isVolatile));
                case ABT.ExprTypeKind.LONG:
                    return ConstructExpr(left, right, new ABT.LongType(isConst, isVolatile));
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

        public abstract ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type);

        public override ABT.Expr GetExpr(ABT.Env env) {

            // 1. semant operands
            var left = SemantExpr(this.Left, ref env);
            var right = SemantExpr(this.Right, ref env);

            // 2. perform usual scalar conversion
            Tuple<ABT.Expr, ABT.Expr, ABT.ExprTypeKind> castReturn = ABT.TypeCast.UsualScalarConversion(left, right);
            left = castReturn.Item1;
            right = castReturn.Item2;
            var typeKind = castReturn.Item3;

            var isConst = left.Type.IsConst || right.Type.IsConst;
            var isVolatile = left.Type.IsVolatile || right.Type.IsVolatile;

            // 3. if both operands are constants
            if (left.IsConstExpr && right.IsConstExpr) {
                switch (typeKind) {
                    case ABT.ExprTypeKind.DOUBLE:
                        return new ABT.ConstLong(OperateDouble(((ABT.ConstDouble)left).Value, ((ABT.ConstDouble)right).Value), env);
                    case ABT.ExprTypeKind.FLOAT:
                        return new ABT.ConstLong(OperateFloat(((ABT.ConstFloat)left).Value, ((ABT.ConstFloat)right).Value), env);
                    case ABT.ExprTypeKind.ULONG:
                        return new ABT.ConstLong(OperateULong(((ABT.ConstULong)left).Value, ((ABT.ConstULong)right).Value), env);
                    case ABT.ExprTypeKind.LONG:
                        return new ABT.ConstLong(OperateLong(((ABT.ConstLong)left).Value, ((ABT.ConstLong)right).Value), env);
                    default:
                        throw new InvalidOperationException("Expected arithmetic Type.");
                }
            }

            // 4. if not both operands are constants
            switch (typeKind) {
                case ABT.ExprTypeKind.DOUBLE:
                    return ConstructExpr(left, right, new ABT.LongType(isConst, isVolatile));
                case ABT.ExprTypeKind.FLOAT:
                    return ConstructExpr(left, right, new ABT.LongType(isConst, isVolatile));
                case ABT.ExprTypeKind.ULONG:
                    return ConstructExpr(left, right, new ABT.LongType(isConst, isVolatile));
                case ABT.ExprTypeKind.LONG:
                    return ConstructExpr(left, right, new ABT.LongType(isConst, isVolatile));
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.Multiply(left, right);
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.Divide(left, right);
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.Modulo(left, right);
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.Add(left, right);

        public ABT.Expr GetPointerAddition(ABT.Expr ptr, ABT.Expr offset, Boolean order = true) {
            if (ptr.Type.Kind != ABT.ExprTypeKind.POINTER) {
                throw new InvalidOperationException();
            }
            if (offset.Type.Kind != ABT.ExprTypeKind.LONG) {
                throw new InvalidOperationException();
            }

            var env = order ? ptr.Env : offset.Env;

            if (ptr.IsConstExpr && offset.IsConstExpr) {
                var baseValue = (Int32)((ABT.ConstPtr)ptr).Value;
                Int32 scaleValue = ((ABT.PointerType)(ptr.Type)).RefType.SizeOf;
                Int32 offsetValue = ((ABT.ConstLong)offset).Value;
                return new ABT.ConstPtr((UInt32)(baseValue + scaleValue * offsetValue), ptr.Type, env);
            }

            var baseAddress = ABT.TypeCast.FromPointer(ptr, new ABT.LongType(ptr.Type.IsConst, ptr.Type.IsVolatile), ptr.Env);
            var scaleFactor = new ABT.Multiply(
                offset,
                new ABT.ConstLong(((ABT.PointerType)(ptr.Type)).RefType.SizeOf, env)
            );
            var type = new ABT.LongType(offset.Type.IsConst, offset.Type.IsVolatile);
            var add =
                order
                ? new ABT.Add(baseAddress, scaleFactor)
                : new ABT.Add(scaleFactor, baseAddress);

            return ABT.TypeCast.ToPointer(add, ptr.Type, env);
        }

        public override ABT.Expr GetExpr(ABT.Env env) {

            // 1. semant the operands
            var left = SemantExpr(this.Left, ref env);
            var right = SemantExpr(this.Right, ref env);

            if (left.Type is ABT.ArrayType) {
                left = ABT.TypeCast.MakeCast(left, new ABT.PointerType(((ABT.ArrayType)left.Type).ElemType, left.Type.IsConst, left.Type.IsVolatile));
            }

            if (right.Type is ABT.ArrayType) {
                right = ABT.TypeCast.MakeCast(right, new ABT.PointerType(((ABT.ArrayType)right.Type).ElemType, right.Type.IsConst, right.Type.IsVolatile));
            }

            // 2. ptr + int
            if (left.Type.Kind == ABT.ExprTypeKind.POINTER) {
                if (!right.Type.IsIntegral) {
                    throw new InvalidOperationException("Expected integral to be added to a pointer.");
                }
                right = ABT.TypeCast.MakeCast(right, new ABT.LongType(right.Type.IsConst, right.Type.IsVolatile));
                return GetPointerAddition(left, right);
            }

            // 3. int + ptr
            if (right.Type.Kind == ABT.ExprTypeKind.POINTER) {
                if (!left.Type.IsIntegral) {
                    throw new InvalidOperationException("Expected integral to be added to a pointer.");
                }
                left = ABT.TypeCast.MakeCast(left, new ABT.LongType(left.Type.IsConst, left.Type.IsVolatile));
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.Sub(left, right);

        public static ABT.Expr GetPointerSubtraction(ABT.Expr ptr, ABT.Expr offset) {
            if (ptr.Type.Kind != ABT.ExprTypeKind.POINTER) {
                throw new InvalidOperationException("Error: expect a pointer");
            }
            if (offset.Type.Kind != ABT.ExprTypeKind.LONG) {
                throw new InvalidOperationException("Error: expect an integer");
            }

            if (ptr.IsConstExpr && offset.IsConstExpr) {
                Int32 baseAddressValue = (Int32)((ABT.ConstPtr)ptr).Value;
                Int32 scaleFactorValue = ((ABT.PointerType)(ptr.Type)).RefType.SizeOf;
                Int32 offsetValue = ((ABT.ConstLong)offset).Value;
                return new ABT.ConstPtr((UInt32)(baseAddressValue - scaleFactorValue * offsetValue), ptr.Type, offset.Env);
            }

            return ABT.TypeCast.ToPointer(new ABT.Sub(
                    ABT.TypeCast.FromPointer(ptr, new ABT.LongType(ptr.Type.IsConst, ptr.Type.IsVolatile), ptr.Env),
                    new ABT.Multiply(
                        offset,
                        new ABT.ConstLong(((ABT.PointerType)(ptr.Type)).RefType.SizeOf, offset.Env)
                    )
                ), ptr.Type, offset.Env
            );
        }

        public override ABT.Expr GetExpr(ABT.Env env) {

            var left = SemantExpr(this.Left, ref env);
            var right = SemantExpr(this.Right, ref env);

            if (left.Type is ABT.ArrayType) {
                left = ABT.TypeCast.MakeCast(left, new ABT.PointerType((left.Type as ABT.ArrayType).ElemType, left.Type.IsConst, left.Type.IsVolatile));
            }

            if (right.Type is ABT.ArrayType) {
                right = ABT.TypeCast.MakeCast(right, new ABT.PointerType((right.Type as ABT.ArrayType).ElemType, right.Type.IsConst, right.Type.IsVolatile));
            }

            var isConst = left.Type.IsConst || right.Type.IsConst;
            var isVolatile = left.Type.IsVolatile || right.Type.IsVolatile;

            if (left.Type.Kind == ABT.ExprTypeKind.POINTER) {

                // 1. ptr - ptr
                if (right.Type.Kind == ABT.ExprTypeKind.POINTER) {
                    ABT.PointerType leftType = (ABT.PointerType)(left.Type);
                    ABT.PointerType rightType = (ABT.PointerType)(right.Type);
                    if (!leftType.RefType.EqualType(rightType.RefType)) {
                        throw new InvalidOperationException("The 2 pointers don't match.");
                    }

                    Int32 scale = leftType.RefType.SizeOf;

                    if (left.IsConstExpr && right.IsConstExpr) {
                        return new ABT.ConstLong((Int32)(((ABT.ConstPtr)left).Value - ((ABT.ConstPtr)right).Value) / scale, env);
                    }

                    return new ABT.Divide(
                        new ABT.Sub(
                            ABT.TypeCast.MakeCast(left, new ABT.LongType(isConst, isVolatile)),
                            ABT.TypeCast.MakeCast(right, new ABT.LongType(isConst, isVolatile))
                        ),
                        new ABT.ConstLong(scale, env)
                    );
                }

                // 2. ptr - integral
                if (!right.Type.IsIntegral) {
                    throw new InvalidOperationException("Expected an integral.");
                }
                right = ABT.TypeCast.MakeCast(right, new ABT.LongType(right.Type.IsConst, right.Type.IsVolatile));
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.LShift(left, right);
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.RShift(left, right);

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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.Less(left, right);
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.LEqual(left, right);
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.Greater(left, right);
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.GEqual(left, right);
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.Equal(left, right);
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.NotEqual(left, right);
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.BitwiseAnd(left, right);

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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.Xor(left, right);
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.BitwiseOr(left, right);
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.LogicalAnd(left, right);
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

        public override ABT.Expr ConstructExpr(ABT.Expr left, ABT.Expr right, ABT.ExprType type) =>
            new ABT.LogicalOr(left, right);
    }
}
