using System;
using System.Collections.Generic;

namespace SyntaxTree {

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
            AST.Expr lhs = this.Left.GetExpr(env);
            env = lhs.Env;

            AST.Expr rhs = this.Right.GetExpr(env);
            env = rhs.Env;

            // 2. perform usual arithmetic conversion
            Tuple<AST.Expr, AST.Expr, AST.ExprType.Kind> r_cast = AST.TypeCast.UsualArithmeticConversion(lhs, rhs);
            lhs = r_cast.Item1;
            rhs = r_cast.Item2;
            AST.ExprType.Kind kind = r_cast.Item3;

            Boolean is_const = lhs.type.is_const || rhs.type.is_const;
            Boolean is_volatile = lhs.type.is_volatile || rhs.type.is_volatile;

            // 3. if both operands are constants
            if (lhs.IsConstExpr && rhs.IsConstExpr) {
                switch (kind) {
                    case AST.ExprType.Kind.ULONG:
                        return new AST.ConstULong(OperateULong(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value), env);
                    case AST.ExprType.Kind.LONG:
                        return new AST.ConstLong(OperateLong(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value), env);
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
        protected BinaryArithmeticOp(Expr left, Expr right)
            : base(left, right) { }

        public abstract Single OperateFloat(Single left, Single right);
        public abstract Double OperateDouble(Double left, Double right);

        public override AST.Expr GetExpr(AST.Env env) {

            // 1. semant operands
            AST.Expr lhs = this.Left.GetExpr(env);
            env = lhs.Env;

            AST.Expr rhs = this.Right.GetExpr(env);
            env = rhs.Env;

            // 2. perform usual arithmetic conversion
            Tuple<AST.Expr, AST.Expr, AST.ExprType.Kind> r_cast = AST.TypeCast.UsualArithmeticConversion(lhs, rhs);
            lhs = r_cast.Item1;
            rhs = r_cast.Item2;
            AST.ExprType.Kind kind = r_cast.Item3;

            Boolean is_const = lhs.type.is_const || rhs.type.is_const;
            Boolean is_volatile = lhs.type.is_volatile || rhs.type.is_volatile;

            // 3. if both operands are constants
            if (lhs.IsConstExpr && rhs.IsConstExpr) {
                switch (kind) {
                    case AST.ExprType.Kind.DOUBLE:
                        return new AST.ConstDouble(OperateDouble(((AST.ConstDouble)lhs).value, ((AST.ConstDouble)rhs).value), env);
                    case AST.ExprType.Kind.FLOAT:
                        return new AST.ConstFloat(OperateFloat(((AST.ConstFloat)lhs).value, ((AST.ConstFloat)rhs).value), env);
                    case AST.ExprType.Kind.ULONG:
                        return new AST.ConstULong(OperateULong(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value), env);
                    case AST.ExprType.Kind.LONG:
                        return new AST.ConstLong(OperateLong(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value), env);
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
        protected BinaryLogicalOp(Expr left, Expr right)
            : base(left, right) { }

        public abstract Int32 OperateLong(Int32 lhs, Int32 rhs);
        public abstract Int32 OperateULong(UInt32 lhs, UInt32 rhs);
        public abstract Int32 OperateFloat(Single lhs, Single rhs);
        public abstract Int32 OperateDouble(Double lhs, Double rhs);

        public abstract AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type);

        public override AST.Expr GetExpr(AST.Env env) {

            // 1. semant operands
            AST.Expr lhs = this.Left.GetExpr(env);
            env = lhs.Env;

            AST.Expr rhs = this.Right.GetExpr(env);
            env = rhs.Env;

            // 2. perform usual scalar conversion
            Tuple<AST.Expr, AST.Expr, AST.ExprType.Kind> r_cast = AST.TypeCast.UsualScalarConversion(lhs, rhs);
            lhs = r_cast.Item1;
            rhs = r_cast.Item2;
            AST.ExprType.Kind kind = r_cast.Item3;

            Boolean is_const = lhs.type.is_const || rhs.type.is_const;
            Boolean is_volatile = lhs.type.is_volatile || rhs.type.is_volatile;

            // 3. if both operands are constants
            if (lhs.IsConstExpr && rhs.IsConstExpr) {
                switch (kind) {
                    case AST.ExprType.Kind.DOUBLE:
                        return new AST.ConstLong(OperateDouble(((AST.ConstDouble)lhs).value, ((AST.ConstDouble)rhs).value), env);
                    case AST.ExprType.Kind.FLOAT:
                        return new AST.ConstLong(OperateFloat(((AST.ConstFloat)lhs).value, ((AST.ConstFloat)rhs).value), env);
                    case AST.ExprType.Kind.ULONG:
                        return new AST.ConstLong(OperateULong(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value), env);
                    case AST.ExprType.Kind.LONG:
                        return new AST.ConstLong(OperateLong(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value), env);
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
        public Multiply(Expr left, Expr right)
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
    public class Divide : BinaryArithmeticOp {
        public Divide(Expr left, Expr right)
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
    public class Modulo : BinaryIntegralOp {
        public Modulo(Expr left, Expr right)
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
    public class Add : BinaryArithmeticOp {
        public Add(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new Add(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => left + right;
        public override UInt32 OperateULong(UInt32 left, UInt32 right) => left + right;
        public override Single OperateFloat(Single left, Single right) => left + right;
        public override Double OperateDouble(Double left, Double right) => left + right;

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.Add(left, right, type);

        public AST.Expr GetPointerAddition(AST.Expr ptr, AST.Expr offset, Boolean order = true) {
            if (ptr.type.kind != AST.ExprType.Kind.POINTER) {
                throw new InvalidOperationException();
            }
            if (offset.type.kind != AST.ExprType.Kind.LONG) {
                throw new InvalidOperationException();
            }

            AST.Env env = order ? ptr.Env : offset.Env;

            if (ptr.IsConstExpr && offset.IsConstExpr) {
                Int32 _base = (Int32)((AST.ConstPtr)ptr).value;
                Int32 _scale = ((AST.TPointer)(ptr.type)).ref_t.SizeOf;
                Int32 _offset = ((AST.ConstLong)offset).value;
                return new AST.ConstPtr((UInt32)(_base + _scale * _offset), ptr.type, env);
            }

            AST.Expr base_addr = AST.TypeCast.FromPointer(ptr, new AST.TLong(ptr.type.is_const, ptr.type.is_volatile), ptr.Env);
            AST.Expr scale = new AST.Multiply(
                offset,
                new AST.ConstLong(((AST.TPointer)(ptr.type)).ref_t.SizeOf, env),
                new AST.TLong(offset.type.is_const, offset.type.is_volatile)
            );
            AST.ExprType add_type = new AST.TLong(offset.type.is_const, offset.type.is_volatile);
            AST.Expr add =
                order
                ? new AST.Add(base_addr, scale, add_type)
                : new AST.Add(scale, base_addr, add_type);

            return AST.TypeCast.ToPointer(add, ptr.type, env);
        }

        public override AST.Expr GetExpr(AST.Env env) {

            // 1. semant the operands
            AST.Expr lhs = this.Left.GetExpr(env);
            env = lhs.Env;

            AST.Expr rhs = this.Right.GetExpr(env);
            env = rhs.Env;

            if (lhs.type is AST.TArray) {
                lhs = AST.TypeCast.MakeCast(lhs, new AST.TPointer((lhs.type as AST.TArray).elem_type, lhs.type.is_const, lhs.type.is_volatile));
            }

            if (rhs.type is AST.TArray) {
                rhs = AST.TypeCast.MakeCast(rhs, new AST.TPointer((rhs.type as AST.TArray).elem_type, rhs.type.is_const, rhs.type.is_volatile));
            }

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
        public Sub(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new Sub(left, right);

        public override Int32 OperateLong(Int32 left, Int32 right) => left - right;
        public override UInt32 OperateULong(UInt32 left, UInt32 right) => left - right;
        public override Single OperateFloat(Single left, Single right) => left - right;
        public override Double OperateDouble(Double left, Double right) => left - right;

        public override AST.Expr ConstructExpr(AST.Expr left, AST.Expr right, AST.ExprType type) =>
            new AST.Sub(left, right, type);

        public static AST.Expr GetPointerSubtraction(AST.Expr ptr, AST.Expr offset) {
            if (ptr.type.kind != AST.ExprType.Kind.POINTER) {
                throw new InvalidOperationException("Error: expect a pointer");
            }
            if (offset.type.kind != AST.ExprType.Kind.LONG) {
                throw new InvalidOperationException("Error: expect an integer");
            }

            if (ptr.IsConstExpr && offset.IsConstExpr) {
                Int32 _base = (Int32)((AST.ConstPtr)ptr).value;
                Int32 _scale = ((AST.TPointer)(ptr.type)).ref_t.SizeOf;
                Int32 _offset = ((AST.ConstLong)offset).value;
                return new AST.ConstPtr((UInt32)(_base - _scale * _offset), ptr.type, offset.Env);
            }

            return AST.TypeCast.ToPointer(
                expr: new AST.Sub(
                    AST.TypeCast.FromPointer(ptr, new AST.TLong(ptr.type.is_const, ptr.type.is_volatile), ptr.Env),
                    new AST.Multiply(
                        offset,
                        new AST.ConstLong(((AST.TPointer)(ptr.type)).ref_t.SizeOf, offset.Env),
                        new AST.TLong(offset.type.is_const, offset.type.is_volatile)
                    ),
                    new AST.TLong(offset.type.is_const, offset.type.is_volatile)
                ),
                type: ptr.type,
                env: offset.Env
            );
        }

        public override AST.Expr GetExpr(AST.Env env) {

            AST.Expr lhs = this.Left.GetExpr(env);
            env = lhs.Env;

            AST.Expr rhs = this.Right.GetExpr(env);
            env = rhs.Env;

            if (lhs.type is AST.TArray) {
                lhs = AST.TypeCast.MakeCast(lhs, new AST.TPointer((lhs.type as AST.TArray).elem_type, lhs.type.is_const, lhs.type.is_volatile));
            }

            if (rhs.type is AST.TArray) {
                rhs = AST.TypeCast.MakeCast(rhs, new AST.TPointer((rhs.type as AST.TArray).elem_type, rhs.type.is_const, rhs.type.is_volatile));
            }

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

                    if (lhs.IsConstExpr && rhs.IsConstExpr) {
                        return new AST.ConstLong((Int32)(((AST.ConstPtr)lhs).value - ((AST.ConstPtr)rhs).value) / scale, env);
                    }

                    return new AST.Divide(
                        new AST.Sub(
                            AST.TypeCast.MakeCast(lhs, new AST.TLong(is_const, is_volatile)),
                            AST.TypeCast.MakeCast(rhs, new AST.TLong(is_const, is_volatile)),
                            new AST.TLong(is_const, is_volatile)
                        ),
                        new AST.ConstLong(scale, env),
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
        public LShift(Expr left, Expr right)
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
    public class RShift : BinaryIntegralOp {
        public RShift(Expr left, Expr right)
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
    public class Less : BinaryLogicalOp {
        public Less(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new Less(left, right);

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
        public LEqual(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new LEqual(left, right);

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
        public Greater(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new Greater(left, right);

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
        public GEqual(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new GEqual(left, right);

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
        public Equal(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new Equal(left, right);
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
        public NotEqual(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new NotEqual(left, right);

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
        public BitwiseAnd(Expr left, Expr right)
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
    public class Xor : BinaryIntegralOp {
        public Xor(Expr left, Expr right)
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
    public class BitwiseOr : BinaryIntegralOp {
        public BitwiseOr(Expr left, Expr right)
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
	public class LogicalAnd : BinaryLogicalOp {
        public LogicalAnd(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) => new LogicalAnd(left, right);

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
        public LogicalOr(Expr left, Expr right)
            : base(left, right) { }
        public static Expr Create(Expr left, Expr right) =>
            new LogicalOr(left, right);

        public override Int32 OperateLong(Int32 lhs, Int32 rhs) => Convert.ToInt32(lhs != 0 || rhs != 0);
        public override Int32 OperateULong(UInt32 lhs, UInt32 rhs) => Convert.ToInt32(lhs != 0 || rhs != 0);
        public override Int32 OperateFloat(Single lhs, Single rhs) => Convert.ToInt32(lhs != 0 || rhs != 0);
        public override Int32 OperateDouble(Double lhs, Double rhs) => Convert.ToInt32(lhs != 0 || rhs != 0);

        public override AST.Expr ConstructExpr(AST.Expr lhs, AST.Expr rhs, AST.ExprType type) =>
            new AST.LogicalOr(lhs, rhs, type);
    }
}
