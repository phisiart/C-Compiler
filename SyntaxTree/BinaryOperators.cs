using System;
using System.Collections.Generic;

namespace SyntaxTree {

    /// <summary>
    /// Binary operator: lhs op rhs
    /// </summary>
    public abstract class BinaryOp : Expr {
        public BinaryOp(Expr lhs, Expr rhs) {
            this.lhs = lhs;
            this.rhs = rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    /// <summary>
    /// Binary integral operator: takes in two integrals, returns an integer.
    /// </summary>
    public abstract class BinaryIntegralOp : BinaryOp {
        public BinaryIntegralOp(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public abstract Int32 OperateLong(Int32 lhs, Int32 rhs);
        public abstract UInt32 OperateULong(UInt32 lhs, UInt32 rhs);
        public abstract AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type);

        public override AST.Expr GetExpr(AST.Env env) {

            // 1. semant operands
            AST.Expr lhs = this.lhs.GetExpr(env);
            AST.Expr rhs = this.rhs.GetExpr(env);

            // 2. perform usual arithmetic conversion
            Tuple<AST.Expr, AST.Expr, AST.ExprType.Kind> r_cast = AST.TypeCast.UsualArithmeticConversion(lhs, rhs);
            lhs = r_cast.Item1;
            rhs = r_cast.Item2;
            AST.ExprType.Kind kind = r_cast.Item3;

            Boolean is_const = lhs.type.is_const || rhs.type.is_const;
            Boolean is_volatile = lhs.type.is_volatile || rhs.type.is_volatile;

            // 3. if both operands are constants
            if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
                switch (kind) {
                    case AST.ExprType.Kind.ULONG:
                        return new AST.ConstULong(OperateULong(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value));
                    case AST.ExprType.Kind.LONG:
                        return new AST.ConstLong(OperateLong(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value));
                    default:
                        throw new InvalidOperationException("Expected long or unsigned long.");
                }
            }

            // 4. if not both operands are constants
            switch (kind) {
                case AST.ExprType.Kind.ULONG:
                    return ConstructExpr(lhs, rhs, new AST.TULong(is_const, is_volatile));
                case AST.ExprType.Kind.LONG:
                    return ConstructExpr(lhs, rhs, new AST.TULong(is_const, is_volatile));
                default:
                    throw new InvalidOperationException("Expected long or unsigned long.");
            }

        }
    }

    /// <summary>
    /// Binary integral operator: takes in two int/uint/float/double, returns an int/uint/float/double.
    /// </summary>
    public abstract class BinaryArithmeticOp : BinaryIntegralOp {
        public BinaryArithmeticOp(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public abstract Single OperateFloat(Single lhs, Single rhs);
        public abstract Double OperateDouble(Double lhs, Double rhs);

        public override AST.Expr GetExpr(AST.Env env) {

            // 1. semant operands
            AST.Expr lhs = this.lhs.GetExpr(env);
            AST.Expr rhs = this.rhs.GetExpr(env);

            // 2. perform usual arithmetic conversion
            Tuple<AST.Expr, AST.Expr, AST.ExprType.Kind> r_cast = AST.TypeCast.UsualArithmeticConversion(lhs, rhs);
            lhs = r_cast.Item1;
            rhs = r_cast.Item2;
            AST.ExprType.Kind kind = r_cast.Item3;

            Boolean is_const = lhs.type.is_const || rhs.type.is_const;
            Boolean is_volatile = lhs.type.is_volatile || rhs.type.is_volatile;

            // 3. if both operands are constants
            if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
                switch (kind) {
                    case AST.ExprType.Kind.DOUBLE:
                        return new AST.ConstDouble(OperateDouble(((AST.ConstDouble)lhs).value, ((AST.ConstDouble)rhs).value));
                    case AST.ExprType.Kind.FLOAT:
                        return new AST.ConstFloat(OperateFloat(((AST.ConstFloat)lhs).value, ((AST.ConstFloat)rhs).value));
                    case AST.ExprType.Kind.ULONG:
                        return new AST.ConstULong(OperateULong(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value));
                    case AST.ExprType.Kind.LONG:
                        return new AST.ConstLong(OperateLong(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value));
                    default:
                        throw new InvalidOperationException("Expected arithmetic type.");
                }
            }

            // 4. if not both operands are constants
            switch (kind) {
                case AST.ExprType.Kind.DOUBLE:
                    return ConstructExpr(lhs, rhs, new AST.TDouble(is_const, is_volatile));
                case AST.ExprType.Kind.FLOAT:
                    return ConstructExpr(lhs, rhs, new AST.TFloat(is_const, is_volatile));
                case AST.ExprType.Kind.ULONG:
                    return ConstructExpr(lhs, rhs, new AST.TULong(is_const, is_volatile));
                case AST.ExprType.Kind.LONG:
                    return ConstructExpr(lhs, rhs, new AST.TLong(is_const, is_volatile));
                default:
                    throw new InvalidOperationException("Expected arithmetic type.");
            }

        }
    }

    /// <summary>
    /// Binary logical operator: first turn pointers to ulongs, then always returns long.
    /// </summary>
    public abstract class BinaryLogicalOp : BinaryOp {
        public BinaryLogicalOp(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public abstract Int32 OperateLong(Int32 lhs, Int32 rhs);
        public abstract Int32 OperateULong(UInt32 lhs, UInt32 rhs);
        public abstract Int32 OperateFloat(Single lhs, Single rhs);
        public abstract Int32 OperateDouble(Double lhs, Double rhs);

        public abstract AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type);

        public override AST.Expr GetExpr(AST.Env env) {

            // 1. semant operands
            AST.Expr lhs = this.lhs.GetExpr(env);
            AST.Expr rhs = this.rhs.GetExpr(env);

            // 2. perform usual scalar conversion
            Tuple<AST.Expr, AST.Expr, AST.ExprType.Kind> r_cast = AST.TypeCast.UsualScalarConversion(lhs, rhs);
            lhs = r_cast.Item1;
            rhs = r_cast.Item2;
            AST.ExprType.Kind kind = r_cast.Item3;

            Boolean is_const = lhs.type.is_const || rhs.type.is_const;
            Boolean is_volatile = lhs.type.is_volatile || rhs.type.is_volatile;

            // 3. if both operands are constants
            if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
                switch (kind) {
                    case AST.ExprType.Kind.DOUBLE:
                        return new AST.ConstLong(OperateDouble(((AST.ConstDouble)lhs).value, ((AST.ConstDouble)rhs).value));
                    case AST.ExprType.Kind.FLOAT:
                        return new AST.ConstLong(OperateFloat(((AST.ConstFloat)lhs).value, ((AST.ConstFloat)rhs).value));
                    case AST.ExprType.Kind.ULONG:
                        return new AST.ConstLong(OperateULong(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value));
                    case AST.ExprType.Kind.LONG:
                        return new AST.ConstLong(OperateLong(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value));
                    default:
                        throw new InvalidOperationException("Expected arithmetic type.");
                }
            }

            // 4. if not both operands are constants
            switch (kind) {
                case AST.ExprType.Kind.DOUBLE:
                    return ConstructExpr(lhs, rhs, new AST.TLong(is_const, is_volatile));
                case AST.ExprType.Kind.FLOAT:
                    return ConstructExpr(lhs, rhs, new AST.TLong(is_const, is_volatile));
                case AST.ExprType.Kind.ULONG:
                    return ConstructExpr(lhs, rhs, new AST.TLong(is_const, is_volatile));
                case AST.ExprType.Kind.LONG:
                    return ConstructExpr(lhs, rhs, new AST.TLong(is_const, is_volatile));
                default:
                    throw new InvalidOperationException("Expected arithmetic type.");
            }

        }
    }

    /// <summary>
    /// Multiplication: perform usual arithmetic conversion.
    /// </summary>
    public class Multiply : BinaryArithmeticOp {
        public Multiply(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => lhs * rhs;
        public override UInt32 OperateULong(UInt32 lhs, UInt32 rhs) => lhs * rhs;
        public override Single OperateFloat(Single lhs, Single rhs) => lhs * rhs;
        public override Double OperateDouble(Double lhs, Double rhs) => lhs * rhs;

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.Multiply(lhs, rhs, type);

    }

	/// <summary>
	/// Division: perform usual arithmetic conversion.
	/// </summary>
	public class Divide : BinaryArithmeticOp {
        public Divide(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => lhs / rhs;
        public override UInt32 OperateULong(UInt32 lhs, UInt32 rhs) => lhs / rhs;
        public override Single OperateFloat(Single lhs, Single rhs) => lhs / rhs;
        public override Double OperateDouble(Double lhs, Double rhs) => lhs / rhs;

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.Divide(lhs, rhs, type);

    }

    /// <summary>
    /// Modulo: only accepts integrals.
    /// </summary>
    public class Modulo : BinaryIntegralOp {
        public Modulo(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => lhs % rhs;
        public override UInt32 OperateULong(UInt32 lhs, UInt32 rhs) => lhs % rhs;

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.Modulo(lhs, rhs, type);

    }

	/// <summary>
	/// Addition
	/// 
	/// There are two kinds of addition:
	/// 1. both operands are of arithmetic type
	/// 2. one operand is a pointer, and the other is an integral
	/// 
	/// </summary>
	public class Add : BinaryArithmeticOp {
        public Add(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => lhs + rhs;
        public override UInt32 OperateULong(UInt32 lhs, UInt32 rhs) => lhs + rhs;
        public override Single OperateFloat(Single lhs, Single rhs) => lhs + rhs;
        public override Double OperateDouble(Double lhs, Double rhs) => lhs + rhs;

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.Add(lhs, rhs, type);

        public AST.Expr GetPointerAddition(AST.Expr ptr, AST.Expr offset, Boolean order = true) {
            if (ptr.type.kind != AST.ExprType.Kind.POINTER) {
                throw new InvalidOperationException();
            }
            if (offset.type.kind != AST.ExprType.Kind.LONG) {
                throw new InvalidOperationException();
            }

            if (ptr.IsConstExpr() && offset.IsConstExpr()) {
                Int32 _base = (Int32)((AST.ConstPtr)ptr).value;
                Int32 _scale = ((AST.TPointer)(ptr.type)).ref_t.SizeOf;
                Int32 _offset = ((AST.ConstLong)offset).value;
                return new AST.ConstPtr((UInt32)(_base + _scale * _offset), ptr.type);
            }

            AST.Expr base_addr = AST.TypeCast.FromPointer(ptr, new AST.TLong(ptr.type.is_const, ptr.type.is_volatile));
            AST.Expr scale = new AST.Multiply(
                offset,
                new AST.ConstLong(((AST.TPointer)(ptr.type)).ref_t.SizeOf),
                new AST.TLong(offset.type.is_const, offset.type.is_volatile)
            );
            AST.ExprType add_type = new AST.TLong(offset.type.is_const, offset.type.is_volatile);
            AST.Expr add =
                order
                ? new AST.Add(base_addr, scale, add_type)
                : new AST.Add(scale, base_addr, add_type);

            return AST.TypeCast.ToPointer(add, ptr.type);
        }

        public override AST.Expr GetExpr(AST.Env env) {

            // 1. semant the operands
            AST.Expr lhs = this.lhs.GetExpr(env);
            AST.Expr rhs = this.rhs.GetExpr(env);

            // 2. ptr + int
            if (lhs.type.kind == AST.ExprType.Kind.POINTER) {
                if (!rhs.type.IsIntegral) {
                    throw new InvalidOperationException("Expected integral to be added to a pointer.");
                }
                rhs = AST.TypeCast.MakeCast(rhs, new AST.TLong(rhs.type.is_const, rhs.type.is_volatile));
                return GetPointerAddition(lhs, rhs);
            }

            // 3. int + ptr
            if (rhs.type.kind == AST.ExprType.Kind.POINTER) {
                if (!lhs.type.IsIntegral) {
                    throw new InvalidOperationException("Expected integral to be added to a pointer.");
                }
                lhs = AST.TypeCast.MakeCast(lhs, new AST.TLong(lhs.type.is_const, lhs.type.is_volatile));
                return GetPointerAddition(rhs, lhs, false);
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
	public class Sub : BinaryArithmeticOp {
        public Sub(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => lhs - rhs;
        public override UInt32 OperateULong(UInt32 lhs, UInt32 rhs) => lhs - rhs;
        public override Single OperateFloat(Single lhs, Single rhs) => lhs - rhs;
        public override Double OperateDouble(Double lhs, Double rhs) => lhs - rhs;

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.Sub(lhs, rhs, type);

        public static AST.Expr GetPointerSubtraction(AST.Expr ptr, AST.Expr offset) {
            if (ptr.type.kind != AST.ExprType.Kind.POINTER) {
                throw new InvalidOperationException("Error: expect a pointer");
            }
            if (offset.type.kind != AST.ExprType.Kind.LONG) {
                throw new InvalidOperationException("Error: expect an integer");
            }

            if (ptr.IsConstExpr() && offset.IsConstExpr()) {
                Int32 _base = (Int32)((AST.ConstPtr)ptr).value;
                Int32 _scale = ((AST.TPointer)(ptr.type)).ref_t.SizeOf;
                Int32 _offset = ((AST.ConstLong)offset).value;
                return new AST.ConstPtr((UInt32)(_base - _scale * _offset), ptr.type);
            }

            return AST.TypeCast.ToPointer(
                new AST.Sub(
                    AST.TypeCast.FromPointer(ptr, new AST.TLong(ptr.type.is_const, ptr.type.is_volatile)),
                    new AST.Multiply(
                        offset,
                        new AST.ConstLong(((AST.TPointer)(ptr.type)).ref_t.SizeOf),
                        new AST.TLong(offset.type.is_const, offset.type.is_volatile)
                    ),
                    new AST.TLong(offset.type.is_const, offset.type.is_volatile)
                ),
                ptr.type
            );
        }

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr lhs = this.lhs.GetExpr(env);
            AST.Expr rhs = this.rhs.GetExpr(env);

            Boolean is_const = lhs.type.is_const || rhs.type.is_const;
            Boolean is_volatile = lhs.type.is_volatile || rhs.type.is_volatile;

            if (lhs.type.kind == AST.ExprType.Kind.POINTER) {

                // 1. ptr - ptr
                if (rhs.type.kind == AST.ExprType.Kind.POINTER) {
                    AST.TPointer lhs_t = (AST.TPointer)(lhs.type);
                    AST.TPointer rhs_t = (AST.TPointer)(rhs.type);
                    if (!lhs_t.ref_t.EqualType(rhs_t.ref_t)) {
                        throw new InvalidOperationException("The 2 pointers don't match.");
                    }

                    Int32 scale = lhs_t.ref_t.SizeOf;

                    if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
                        return new AST.ConstLong((Int32)(((AST.ConstPtr)lhs).value - ((AST.ConstPtr)rhs).value) / scale);
                    }

                    return new AST.Divide(
                        new AST.Sub(
                            AST.TypeCast.MakeCast(lhs, new AST.TLong(is_const, is_volatile)),
                            AST.TypeCast.MakeCast(rhs, new AST.TLong(is_const, is_volatile)),
                            new AST.TLong(is_const, is_volatile)
                        ),
                        new AST.ConstLong(scale),
                        new AST.TLong(is_const, is_volatile)
                    );
                }

                // 2. ptr - integral
                if (!rhs.type.IsIntegral) {
                    throw new InvalidOperationException("Expected an integral.");
                }
                rhs = AST.TypeCast.MakeCast(rhs, new AST.TLong(rhs.type.is_const, rhs.type.is_volatile));
                return GetPointerSubtraction(lhs, rhs);

            }

            // 3. arith - arith
            return base.GetExpr(env);
        }
    }

	/// <summary>
	/// Left Shift: takes in two integrals, returns an integer.
	/// </summary>
	public class LShift : BinaryIntegralOp {
        public LShift(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => lhs << rhs;
        public override UInt32 OperateULong(UInt32 lhs, UInt32 rhs) => (UInt32)((Int32)lhs << (Int32)rhs);

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.LShift(lhs, rhs, type);
    }

	/// <summary>
	/// Right Shift: takes in two integrals, returns an integer;
	/// </summary>
	public class RShift : BinaryIntegralOp {
        public RShift(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => lhs >> rhs;
        public override UInt32 OperateULong(UInt32 lhs, UInt32 rhs) => (UInt32)((Int32)lhs >> (Int32)rhs);

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.RShift(lhs, rhs, type);

    }

	/// <summary>
	/// Less than
	/// </summary>
	public class Less : BinaryLogicalOp {
        public Less(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => Convert.ToInt32(lhs < rhs);
        public override Int32 OperateULong(UInt32 lhs, UInt32 rhs) => Convert.ToInt32(lhs < rhs);
        public override Int32 OperateFloat(Single lhs, Single rhs) => Convert.ToInt32(lhs < rhs);
        public override Int32 OperateDouble(Double lhs, Double rhs) => Convert.ToInt32(lhs < rhs);

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.Less(lhs, rhs, type);
    }

    /// <summary>
    /// Less or Equal than
    /// </summary>
    public class LEqual : BinaryLogicalOp {
        public LEqual(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => Convert.ToInt32(lhs <= rhs);
        public override Int32 OperateULong(UInt32 lhs, UInt32 rhs) => Convert.ToInt32(lhs <= rhs);
        public override Int32 OperateFloat(Single lhs, Single rhs) => Convert.ToInt32(lhs <= rhs);
        public override Int32 OperateDouble(Double lhs, Double rhs) => Convert.ToInt32(lhs <= rhs);

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.LEqual(lhs, rhs, type);
    }

    /// <summary>
    /// Greater than
    /// </summary>
	public class Greater : BinaryLogicalOp {
        public Greater(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => Convert.ToInt32(lhs > rhs);
        public override Int32 OperateULong(UInt32 lhs, UInt32 rhs) => Convert.ToInt32(lhs > rhs);
        public override Int32 OperateFloat(Single lhs, Single rhs) => Convert.ToInt32(lhs > rhs);
        public override Int32 OperateDouble(Double lhs, Double rhs) => Convert.ToInt32(lhs > rhs);

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.Greater(lhs, rhs, type);
    }

    /// <summary>
    /// Greater or Equal than
    /// </summary>
    public class GEqual : BinaryLogicalOp {
        public GEqual(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => Convert.ToInt32(lhs >= rhs);
        public override Int32 OperateULong(UInt32 lhs, UInt32 rhs) => Convert.ToInt32(lhs >= rhs);
        public override Int32 OperateFloat(Single lhs, Single rhs) => Convert.ToInt32(lhs >= rhs);
        public override Int32 OperateDouble(Double lhs, Double rhs) => Convert.ToInt32(lhs >= rhs);

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.GEqual(lhs, rhs, type);
    }

    /// <summary>
    /// Equal
    /// </summary>
	public class Equal : BinaryLogicalOp {
        public Equal(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => Convert.ToInt32(lhs == rhs);
        public override Int32 OperateULong(UInt32 lhs, UInt32 rhs) => Convert.ToInt32(lhs == rhs);
        public override Int32 OperateFloat(Single lhs, Single rhs) => Convert.ToInt32(lhs == rhs);
        public override Int32 OperateDouble(Double lhs, Double rhs) => Convert.ToInt32(lhs == rhs);

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.Equal(lhs, rhs, type);
    }

    /// <summary>
    /// Not equal
    /// </summary>
    public class NotEqual : BinaryLogicalOp {
        public NotEqual(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => Convert.ToInt32(lhs != rhs);
        public override Int32 OperateULong(UInt32 lhs, UInt32 rhs) => Convert.ToInt32(lhs != rhs);
        public override Int32 OperateFloat(Single lhs, Single rhs) => Convert.ToInt32(lhs != rhs);
        public override Int32 OperateDouble(Double lhs, Double rhs) => Convert.ToInt32(lhs != rhs);

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.NotEqual(lhs, rhs, type);
    }

    /// <summary>
    /// Bitwise And: returns an integer.
    /// </summary>
    public class BitwiseAnd : BinaryIntegralOp {
        public BitwiseAnd(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => lhs & rhs;
        public override UInt32 OperateULong(UInt32 lhs, UInt32 rhs) => lhs & rhs;

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.BitwiseAnd(lhs, rhs, type);

    }

    /// <summary>
    /// Xor: returns an integer.
    /// </summary>
    public class Xor : BinaryIntegralOp {
        public Xor(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => lhs ^ rhs;
        public override UInt32 OperateULong(UInt32 lhs, UInt32 rhs) => lhs ^ rhs;

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.Xor(lhs, rhs, type);
    }

	/// <summary>
	/// Bitwise Or: accepts two integrals, returns an integer.
	/// </summary>
	public class BitwiseOr : BinaryIntegralOp {
        public BitwiseOr(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => lhs | rhs;
        public override UInt32 OperateULong(UInt32 lhs, UInt32 rhs) => lhs | rhs;

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.BitwiseOr(lhs, rhs, type);

	}

    /// <summary>
    /// Logical and: both operands need to be non-zero.
    /// </summary>
	public class LogicalAnd : BinaryLogicalOp {
        public LogicalAnd(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => Convert.ToInt32(lhs != 0 && rhs != 0);
        public override Int32 OperateULong(UInt32 lhs, UInt32 rhs) => Convert.ToInt32(lhs != 0 && rhs != 0);
        public override Int32 OperateFloat(Single lhs, Single rhs) => Convert.ToInt32(lhs != 0 && rhs != 0);
        public override Int32 OperateDouble(Double lhs, Double rhs) => Convert.ToInt32(lhs != 0 && rhs != 0);

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.LogicalAnd(lhs, rhs, type);

    }

    /// <summary>
    /// Logical or: at least one of operands needs to be non-zero.
    /// </summary>
	public class LogicalOr : BinaryLogicalOp {
        public LogicalOr(Expr lhs, Expr rhs)
            : base(lhs, rhs) { }

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => Convert.ToInt32(lhs != 0 || rhs != 0);
        public override Int32 OperateULong(UInt32 lhs, UInt32 rhs) => Convert.ToInt32(lhs != 0 || rhs != 0);
        public override Int32 OperateFloat(Single lhs, Single rhs) => Convert.ToInt32(lhs != 0 || rhs != 0);
        public override Int32 OperateDouble(Double lhs, Double rhs) => Convert.ToInt32(lhs != 0 || rhs != 0);

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.LogicalOr(lhs, rhs, type);

    }

}
