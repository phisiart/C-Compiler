using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AST {

    public abstract class BinaryOp : Expr {
        public BinaryOp(Expr lhs, Expr rhs, ExprType type)
            : base(type) {
            this.lhs = lhs;
            this.rhs = rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    /// <summary>
    /// A binary integral operator only takes integral operands.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long op long
    /// 2) ulong op ulong
    /// 
    /// The procedure is always:
    /// %eax = lhs, %ebx = rhs
    /// %eax = %eax op %ebx
    /// </summary>
    public abstract class BinaryIntegralOp : BinaryOp {
        public BinaryIntegralOp(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public abstract void OperateLong(CGenState state);
        public abstract void OperateULong(CGenState state);

        public Reg CGenLong(Env env, CGenState state) {
            Reg ret;

            // 1. Load lhs to EAX.
            // 
            // regs:
            // %eax = lhs
            // 
            // stack:
            // +-----+
            // | ... | <- %esp
            // +-----+
            // 
            ret = lhs.CGenValue(env, state);
            if (ret != Reg.EAX) {
                throw new InvalidOperationException();
            }

            // 2. Push lhs to stack.
            // 
            // regs:
            // %eax = lhs
            // 
            // stack:
            // +-----+
            // | ... |
            // +-----+
            // | lhs | <- %esp has decreased by 4
            // +-----+
            // 
            state.PUSHL(Reg.EAX);

            // 3. Load rhs to EAX.
            // 
            // regs:
            // %eax = rhs
            // 
            // stack:
            // +-----+
            // | ... |
            // +-----+
            // | lhs | <- %esp
            // +-----+
            // 
            ret = rhs.CGenValue(env, state);
            if (ret != Reg.EAX) {
                throw new InvalidOperationException();
            }

            // 4. Move rhs into EBX. Pop lhs from stack, into EAX.
            // 
            // regs:
            // %eax = lhs
            // %ebx = rhs
            // 
            // stack:
            // +-----+
            // | ... | <- %esp has moved back.
            // +-----+
            // 
            state.MOVL(Reg.EAX, Reg.EBX);
            state.POPL(Reg.EAX);

            // 5. Perform operation. Result will be stored in EAX.
            // 
            // regs:
            // %eax = ans
            // %ebx = lhs
            // 
            // stack:
            // +-----+
            // | ... | <- %esp
            // +-----+
            // 
            OperateLong(state);

            return Reg.EAX;
        }

        public Reg CGenULong(Env env, CGenState state) {
            Reg ret;

            // 1. Load lhs to EAX.
            // 
            // regs:
            // %eax = lhs
            // 
            // stack:
            // +-----+
            // | ... | <- %esp
            // +-----+
            // 
            ret = lhs.CGenValue(env, state);
            if (ret != Reg.EAX) {
                throw new InvalidOperationException();
            }

            // 2. Push lhs to stack.
            // 
            // regs:
            // %eax = lhs
            // 
            // stack:
            // +-----+
            // | ... |
            // +-----+
            // | lhs | <- %esp has decreased by 4
            // +-----+
            // 
            state.PUSHL(Reg.EAX);

            // 3. Load rhs to EAX.
            // 
            // regs:
            // %eax = rhs
            // 
            // stack:
            // +-----+
            // | ... |
            // +-----+
            // | lhs | <- %esp
            // +-----+
            // 
            ret = rhs.CGenValue(env, state);
            if (ret != Reg.EAX) {
                throw new InvalidOperationException();
            }

            // 4. Move rhs into EAX. Pop lhs from stack, into EAX.
            // 
            // regs:
            // %eax = lhs
            // %ebx = rhs
            // 
            // stack:
            // +-----+
            // | ... | <- %esp has moved back.
            // +-----+
            // 
            state.MOVL(Reg.EBX, Reg.EAX);
            state.POPL(Reg.EAX);

            // 5. Perform operation. Result will be stored in EAX.
            // 
            // regs:
            // %eax = ans
            // %ebx = lhs
            // 
            // stack:
            // +-----+
            // | ... | <- %esp
            // +-----+
            // 
            OperateULong(state);

            return Reg.EAX;
        }

        public override Reg CGenValue(Env env, CGenState state) {
            switch (type.kind) {
                case ExprType.Kind.LONG:
                    if (lhs.type.kind != ExprType.Kind.LONG || rhs.type.kind != ExprType.Kind.LONG) {
                        throw new InvalidOperationException();
                    }
                    return CGenLong(env, state);

                case ExprType.Kind.ULONG:
                    if (lhs.type.kind != ExprType.Kind.ULONG || rhs.type.kind != ExprType.Kind.ULONG) {
                        throw new InvalidOperationException();
                    }
                    return CGenULong(env, state);

                default:
                    throw new InvalidOperationException();
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
    /// %st(0) = lhs, %st(1) = rhs
    /// %st(0) = %st(0) op %st(1), invalidate %st(1)
    /// </summary>
    public abstract class BinaryArithmeticOp : BinaryIntegralOp {
        public BinaryArithmeticOp(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }
        public abstract void OperateFloat(CGenState state);
        public abstract void OperateDouble(CGenState state);

        public Reg CGenFloat(Env env, CGenState state) {
            Reg ret;

            // 1. Load lhs to ST0. Now the float stack should only contain one element.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... | <- %esp
            // +-----+
            //
            // float stack:
            // +-----+
            // | lhs | <- %st(0)
            // +-----+
            // 
            ret = lhs.CGenValue(env, state);
            if (ret != Reg.ST0) {
                throw new InvalidOperationException();
            }

            // 2. Pop from float stack, push into memory stack. Now the float stack should be empty.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... |
            // | lhs | <- %esp has decreased by 4
            // +-----+
            //
            // float stack:
            // +-----+    empty
            // 
            state.CGenExpandStackBy4Bytes();
            state.FSTPS(0, Reg.ESP);

            // 3. Load rhs to ST0. Now the float stack should only contain one element.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... |
            // | lhs | <- %esp
            // +-----+
            //
            // float stack:
            // +-----+
            // | rhs | <- %st(0)
            // +-----+
            // 
            ret = rhs.CGenValue(env, state);
            if (ret != Reg.ST0) {
                throw new InvalidOperationException();
            }

            // 4. Pop double from memory stack, push into float stack. Now both lhs and rhs are in float stack.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... | <- %esp
            // +-----+
            //
            // float stack:
            // +-----+
            // | rhs | <- %st(1)
            // +-----+
            // | lhs | <- %st(0)
            // +-----+
            // 
            state.FLDS(0, Reg.ESP);
            state.CGenShrinkStackBy4Bytes();

            // 5. Perform operation. FPU would pop both operands and push answer back in.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... | <- %esp
            // +-----+
            //
            // float stack:
            // +-----+
            // | ans | <- %st(0)
            // +-----+
            // 
            OperateFloat(state);

            return Reg.ST0;
        }

        public Reg CGenDouble(Env env, CGenState state) {
            Reg ret;

            // 1. Load lhs to ST0. Now the float stack should only contain one element.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... | <- %esp
            // +-----+
            //
            // float stack:
            // +-----+
            // | lhs | <- %st(0)
            // +-----+
            // 
            ret = lhs.CGenValue(env, state);
            if (ret != Reg.ST0) {
                throw new InvalidOperationException();
            }

            // 2. Pop from float stack, push into memory stack. Now the float stack should be empty.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... |
            // | lhs | <- %esp has decreased by 8
            // +-----+
            //
            // float stack:
            // +-----+    empty
            // 
            state.CGenExpandStackBy8Bytes();
            state.FSTPL(0, Reg.ESP);

            // 3. Load rhs to ST0. Now the float stack should only contain one element.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... |
            // | lhs | <- %esp
            // +-----+
            //
            // float stack:
            // +-----+
            // | rhs | <- %st(0)
            // +-----+
            // 
            ret = rhs.CGenValue(env, state);
            if (ret != Reg.ST0) {
                throw new InvalidOperationException();
            }

            // 4. Pop double from memory stack, push into float stack. Now both lhs and rhs are in float stack.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... | <- %esp
            // +-----+
            //
            // float stack:
            // +-----+
            // | rhs | <- %st(1)
            // +-----+
            // | lhs | <- %st(0)
            // +-----+
            // 
            state.FLDL(0, Reg.ESP);
            state.CGenShrinkStackBy8Bytes();

            // 5. Perform operation. FPU would pop both operands and push answer back in.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... | <- %esp
            // +-----+
            //
            // float stack:
            // +-----+
            // | ans | <- %st(0)
            // +-----+
            // 
            OperateDouble(state);

            return Reg.ST0;
        }

        public override sealed Reg CGenValue(Env env, CGenState state) {
            switch (type.kind) {
                case ExprType.Kind.FLOAT:
                    if (lhs.type.kind != ExprType.Kind.FLOAT || rhs.type.kind != ExprType.Kind.FLOAT) {
                        throw new InvalidOperationException();
                    }
                    return CGenFloat(env, state);

                case ExprType.Kind.DOUBLE:
                    if (lhs.type.kind != ExprType.Kind.DOUBLE || rhs.type.kind != ExprType.Kind.DOUBLE) {
                        throw new InvalidOperationException();
                    }
                    return CGenDouble(env, state);

                default:
                    return base.CGenValue(env, state);
            }
        }
    }

    /// <summary>
    /// The multiplication (*) operator can either take
    /// 1) integral- or 2) floating-type operands.
    /// 
    /// After semantic analysis, only four cases are possible:
    /// 1) long * long
    /// 2) ulong * ulong
    /// 3) float * float
    /// 4) double * double
    /// </summary>
    public class Multiply : BinaryArithmeticOp {
        public Multiply(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) {
            Debug.Assert((type.kind == ExprType.Kind.LONG && lhs.type.kind == ExprType.Kind.LONG && rhs.type.kind == ExprType.Kind.LONG)
                        || (type.kind == ExprType.Kind.ULONG && lhs.type.kind == ExprType.Kind.ULONG && rhs.type.kind == ExprType.Kind.ULONG)
                        || (type.kind == ExprType.Kind.FLOAT && lhs.type.kind == ExprType.Kind.FLOAT && rhs.type.kind == ExprType.Kind.FLOAT)
                        || (type.kind == ExprType.Kind.DOUBLE && lhs.type.kind == ExprType.Kind.DOUBLE && rhs.type.kind == ExprType.Kind.DOUBLE));
        }

        public override void OperateLong(CGenState state) => state.IMUL(Reg.EBX);
        public override void OperateULong(CGenState state) => state.MUL(Reg.EBX);
        public override void OperateFloat(CGenState state) => state.FMULP();
        public override void OperateDouble(CGenState state) => state.FMULP();

    }

    /// <summary>
    /// The division (/) operator can either take
    /// 1) integral- or 2) floating-type operands.
    /// 
    /// After semantic analysis, only four cases are possible:
    /// 1) long / long
    /// 2) ulong / ulong
    /// 3) float / float
    /// 4) double / double
    /// </summary>
    public class Divide : BinaryArithmeticOp {
        public Divide(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public override void OperateLong(CGenState state) {
            state.CLTD();
            state.IDIVL(Reg.EBX);
        }
        public override void OperateULong(CGenState state) {
            state.CLTD();
            state.DIVL(Reg.EBX);
        }
        public override void OperateFloat(CGenState state) => state.FDIVP();
        public override void OperateDouble(CGenState state) => state.FDIVP();
    }

    /// <summary>
    /// The modulo (%) operator can only take integral operands.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long % long
    /// 2) ulong % ulong
    /// </summary>
    public class Modulo : BinaryIntegralOp {
        public Modulo(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) {
        }

        public override void OperateLong(CGenState state) {
            state.CLTD();
            state.IDIVL(Reg.EBX);
            state.MOVL(Reg.EDX, Reg.EAX);
        }

        public override void OperateULong(CGenState state) {
            state.CLTD();
            state.DIVL(Reg.EBX);
            state.MOVL(Reg.EDX, Reg.EAX);
        }
    }

    /// <summary>
    /// The xor (^) operator can only take integral operands.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long ^ long
    /// 2) ulong ^ ulong
    /// </summary>
    public class Xor : BinaryIntegralOp {
        public Xor(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public override void OperateLong(CGenState state) => state.XORL(Reg.EBX, Reg.EAX);
        public override void OperateULong(CGenState state) => state.XORL(Reg.EBX, Reg.EAX);
    }

    /// <summary>
    /// The bitwise or (|) operator can only take integral operands.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long | long
    /// 2) ulong | ulong
    /// </summary>
    public class BitwiseOr : BinaryIntegralOp {
        public BitwiseOr(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public override void OperateLong(CGenState state) => state.ORL(Reg.EBX, Reg.EAX);
        public override void OperateULong(CGenState state) => state.ORL(Reg.EBX, Reg.EAX);
    }

    /// <summary>
    /// The bitwise and (&) operator can only take integral operands.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long & long
    /// 2) ulong & ulong
    /// </summary>
    public class BitwiseAnd : BinaryIntegralOp {
        public BitwiseAnd(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public override void OperateLong(CGenState state) => state.ANDL(Reg.EBX, Reg.EAX);
        public override void OperateULong(CGenState state) => state.ANDL(Reg.EBX, Reg.EAX);
    }

    /// <summary>
    /// The left shift operator can only take integral operands.
    /// Append 0's on the right.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long << long
    /// 2) ulong << ulong
    /// </summary>
    public class LShift : BinaryIntegralOp {
        public LShift(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public override void OperateLong(CGenState state) => state.SALL(Reg.EBX, Reg.EAX);
        public override void OperateULong(CGenState state) => state.SALL(Reg.EBX, Reg.EAX);
    }

    /// <summary>
    /// The right shift operator can only take integral operands.
    /// 
    /// After semantic analysis, only two cases are possible:
    /// 1) long >> long (arithmetic shift, append sign bit)
    /// 2) ulong >> ulong (logical shift, append 0)
    /// </summary>
    public class RShift : BinaryIntegralOp {
        public RShift(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public override void OperateLong(CGenState state) => state.SARL(Reg.EBX, Reg.EAX);
        public override void OperateULong(CGenState state) => state.SHRL(Reg.EBX, Reg.EAX);
    }

    /// <summary>
    /// The addition operator can either take
    /// 1) integral- or 2) floating-type operands.
    /// 
    /// After semantic analysis, pointer additions are converted into
    /// combinations of type-casts and series of operations. So in AST,
    /// only four cases are possible:
    /// 1) long + long
    /// 2) ulong + ulong
    /// 3) float + float
    /// 4) double + double
    /// </summary>
    public class Add : BinaryArithmeticOp {
        public Add(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public override void OperateLong(CGenState state) => state.ADDL(Reg.EBX, Reg.EAX);
        public override void OperateULong(CGenState state) => state.ADDL(Reg.EBX, Reg.EAX);
        public override void OperateFloat(CGenState state) => state.FADDP();
        public override void OperateDouble(CGenState state) => state.FADDP();
    }

    /// <summary>
    /// The subtraction operator can either take
    /// 1) integral- or 2) floating-type operands.
    /// 
    /// After semantic analysis, pointer subtractions are converted into
    ///   combinations of type-casts and series of operations. So in AST,
    ///   only four cases are possible:
    /// 1) long - long
    /// 2) ulong - ulong
    /// 3) float - float
    /// 4) double - double
    /// </summary>
    public class Sub : BinaryArithmeticOp {
        public Sub(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public override void OperateLong(CGenState state) => state.SUBL(Reg.EBX, Reg.EAX);
        public override void OperateULong(CGenState state) => state.SUBL(Reg.EBX, Reg.EAX);
        public override void OperateFloat(CGenState state) => state.FSUBP();
        public override void OperateDouble(CGenState state) => state.FSUBP();
    }

    /// <summary>
    /// The "greater or equal to" operator can either take
    /// 1) integral- or 2) floating-type operands.
    /// 
    /// After semantic analysis, pointer comparisons are converted into
    ///   integer comparisons. So in AST, only four cases are possible:
    /// 1) long >= long
    /// 2) ulong >= ulong
    /// 3) float >= float
    /// 4) double >= double
    /// </summary>
    public class GEqual : BinaryArithmeticOp {
        public GEqual(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }
        public override void OperateLong(CGenState state) {
        }
        public override void OperateULong(CGenState state) {
        }
        public override void OperateFloat(CGenState state) {
        }
        public override void OperateDouble(CGenState state) {
        }
    }
}
