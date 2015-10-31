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
        public override Env Env => rhs.Env;
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
    public abstract class BinaryIntegralOp : BinaryOp {
        public BinaryIntegralOp(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public abstract void OperateLong(CGenState state);
        public abstract void OperateULong(CGenState state);

        public void CGenPrepareWord(Env env, CGenState state) {
            // 1. Load Left to EAX.
            // 
            // regs:
            // %eax = Left
            // 
            // stack:
            // +-----+
            // | ... | <- %esp
            // +-----+
            // 
            if (lhs.CGenValue(env, state) != Reg.EAX) {
                throw new InvalidOperationException();
            }

            // 2. Push Left to stack.
            // 
            // regs:
            // %eax = Left
            // 
            // stack:
            // +-----+
            // | ... |
            // +-----+
            // | Left | <- %esp has decreased by 4
            // +-----+
            // 
            Int32 stack_size = state.CGenPushLong(Reg.EAX);

            // 3. Load Right to EAX.
            // 
            // regs:
            // %eax = Right
            // 
            // stack:
            // +-----+
            // | ... |
            // +-----+
            // | Left | <- %esp
            // +-----+
            // 
            if (rhs.CGenValue(env, state) != Reg.EAX) {
                throw new InvalidOperationException();
            }

            // 4. Move Right into EBX. Pop Left from stack, into EAX.
            // 
            // regs:
            // %eax = Left
            // %ebx = Right
            // 
            // stack:
            // +-----+
            // | ... | <- %esp has moved back.
            // +-----+
            // 
            state.MOVL(Reg.EAX, Reg.EBX);
            state.CGenPopLong(stack_size, Reg.EAX);
        }

        public Reg CGenLong(Env env, CGenState state) {
            CGenPrepareWord(env, state);
            OperateLong(state);
            return Reg.EAX;
        }

        public Reg CGenULong(Env env, CGenState state) {
            CGenPrepareWord(env, state);
            OperateULong(state);
            return Reg.EAX;
        }

        public override Reg CGenValue(Env env, CGenState state) {
            switch (lhs.type.kind) {
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
    /// %st(0) = Left, %st(1) = Right
    /// %st(0) = %st(0) op %st(1), invalidate %st(1)
    /// </summary>
    public abstract class BinaryArithmeticOp : BinaryIntegralOp {
        public BinaryArithmeticOp(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }
        public abstract void OperateFloat(CGenState state);
        public abstract void OperateDouble(CGenState state);

        public Reg CGenFloat(Env env, CGenState state) {
            Reg ret;

            // 1. Load Left to ST0. Now the float stack should only contain one element.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... | <- %esp
            // +-----+
            //
            // float stack:
            // +-----+
            // | Left | <- %st(0)
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
            // | Left | <- %esp has decreased by 4
            // +-----+
            //
            // float stack:
            // +-----+    empty
            // 
            Int32 stack_size = state.CGenPushFloatP();

            // 3. Load Right to ST0. Now the float stack should only contain one element.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... |
            // | Left | <- %esp
            // +-----+
            //
            // float stack:
            // +-----+
            // | Right | <- %st(0)
            // +-----+
            // 
            ret = rhs.CGenValue(env, state);
            if (ret != Reg.ST0) {
                throw new InvalidOperationException();
            }

            // 4. Pop double from memory stack, push into float stack. Now both Left and Right are in float stack.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... | <- %esp
            // +-----+
            //
            // float stack:
            // +-----+
            // | Right | <- %st(1)
            // +-----+
            // | Left | <- %st(0)
            // +-----+
            // 
            state.CGenPopFloat(stack_size);

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

            // 1. Load Left to ST0. Now the float stack should only contain one element.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... | <- %esp
            // +-----+
            //
            // float stack:
            // +-----+
            // | Left | <- %st(0)
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
            // | Left | <- %esp has decreased by 8
            // +-----+
            //
            // float stack:
            // +-----+    empty
            // 
            Int32 stack_size = state.CGenPushDoubleP();

            // 3. Load Right to ST0. Now the float stack should only contain one element.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... |
            // | Left | <- %esp
            // +-----+
            //
            // float stack:
            // +-----+
            // | Right | <- %st(0)
            // +-----+
            // 
            ret = rhs.CGenValue(env, state);
            if (ret != Reg.ST0) {
                throw new InvalidOperationException();
            }

            // 4. Pop double from memory stack, push into float stack. Now both Left and Right are in float stack.
            //
            // memory stack:
            // +-----+
            // |     |
            // | ... | <- %esp
            // +-----+
            //
            // float stack:
            // +-----+
            // | Right | <- %st(1)
            // +-----+
            // | Left | <- %st(0)
            // +-----+
            // 
            state.CGenPopDouble(stack_size);

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
    /// 
    /// https://msdn.microsoft.com/en-us/library/17zwb64t.aspx
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
    /// 
    /// https://msdn.microsoft.com/en-us/library/17zwb64t.aspx
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
    /// 
    /// https://msdn.microsoft.com/en-us/library/17zwb64t.aspx
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
    public abstract class BinaryArithmeticComp : BinaryArithmeticOp {
        public BinaryArithmeticComp(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public abstract void SetLong(CGenState state);
        public abstract void SetULong(CGenState state);
        public abstract void SetFloat(CGenState state);
        public abstract void SetDouble(CGenState state);

        public override void OperateLong(CGenState state) {
            state.CMPL(Reg.EBX, Reg.EAX);
            SetLong(state);
            state.MOVZBL(Reg.AL, Reg.EAX);
        }

        public override void OperateULong(CGenState state) {
            state.CMPL(Reg.EBX, Reg.EAX);
            SetULong(state);
            state.MOVZBL(Reg.AL, Reg.EAX);
        }

        public override void OperateFloat(CGenState state) {
            // In the beginning, %st(0) = Left, %st(1) = Right.
            // 
            // float stack:
            // +-----+
            // | Right | <- %st(1)
            // +-----+
            // | Left | <- %st(0)
            // +-----+
            // 

            // 1. Do comparison between %st(0) and %st(1).
            //    Pop one value from FPU stack.
            // 
            // float stack:
            // +-----+
            // | Right | <- %st(0)
            // +-----+
            // 
            state.FUCOMIP();

            // 2. Pop another value from FPU stack.
            // 
            // float stack:
            // +-----+ empty
            // 
            state.FSTP(Reg.ST0);

            // 3. Set bit based on comparison result.
            SetFloat(state);
            state.MOVZBL(Reg.AL, Reg.EAX);
        }

        public override void OperateDouble(CGenState state) {
            // In the beginning, %st(0) = Left, %st(1) = Right.
            // 
            // float stack:
            // +-----+
            // | Right | <- %st(1)
            // +-----+
            // | Left | <- %st(0)
            // +-----+
            // 

            // 1. Do comparison between %st(0) and %st(1).
            //    Pop one value from FPU stack.
            // 
            // float stack:
            // +-----+
            // | Right | <- %st(0)
            // +-----+
            // 
            state.FUCOMIP();

            // 2. Pop another value from FPU stack.
            // 
            // float stack:
            // +-----+ empty
            // 
            state.FSTP(Reg.ST0);

            // 3. Set bit based on comparison result.
            SetDouble(state);
            state.MOVZBL(Reg.AL, Reg.EAX);
        }

    }

    /// <summary>
    /// The "greater than or equal to" operator can either take
    /// 1) integral- or 2) floating-type operands.
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
    public class GEqual : BinaryArithmeticComp {
        public GEqual(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public override void SetLong(CGenState state) => state.SETGE(Reg.AL);
        public override void SetULong(CGenState state) => state.SETNB(Reg.AL);
        public override void SetFloat(CGenState state) => state.SETNB(Reg.AL);
        public override void SetDouble(CGenState state) => state.SETNB(Reg.AL);
    }

    /// <summary>
    /// The "greater than" operator can either take
    /// 1) integral- or 2) floating-type operands.
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
    public class Greater : BinaryArithmeticComp {
        public Greater(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public override void SetLong(CGenState state) => state.SETG(Reg.AL);
        public override void SetULong(CGenState state) => state.SETA(Reg.AL);
        public override void SetFloat(CGenState state) => state.SETA(Reg.AL);
        public override void SetDouble(CGenState state) => state.SETA(Reg.AL);
    }

    /// <summary>
    /// The "less than or equal to" operator can either take
    /// 1) integral- or 2) floating-type operands.
    /// 
    /// After semantic analysis, pointer comparisons are converted into
    ///   integer comparisons. So in AST, only four cases are possible:
    /// 1) long <= long
    /// 2) ulong <= ulong
    /// 3) float <= float
    /// 4) double <= double
    /// 
    /// http://x86.renejeschke.de/html/file_module_x86_id_288.html
    /// </summary>
    public class LEqual : BinaryArithmeticComp {
        public LEqual(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public override void SetLong(CGenState state) => state.SETLE(Reg.AL);
        public override void SetULong(CGenState state) => state.SETNA(Reg.AL);
        public override void SetFloat(CGenState state) => state.SETNA(Reg.AL);
        public override void SetDouble(CGenState state) => state.SETNA(Reg.AL);
    }

    /// <summary>
    /// The "less than" operator can either take
    /// 1) integral- or 2) floating-type operands.
    /// 
    /// After semantic analysis, pointer comparisons are converted into
    ///   integer comparisons. So in AST, only four cases are possible:
    /// 1) long < long
    /// 2) ulong < ulong
    /// 3) float < float
    /// 4) double < double
    /// 
    /// http://x86.renejeschke.de/html/file_module_x86_id_288.html
    /// </summary>
    public class Less : BinaryArithmeticComp {
        public Less(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public override void SetLong(CGenState state) => state.SETL(Reg.AL);
        public override void SetULong(CGenState state) => state.SETB(Reg.AL);
        public override void SetFloat(CGenState state) => state.SETB(Reg.AL);
        public override void SetDouble(CGenState state) => state.SETB(Reg.AL);
    }

    /// <summary>
    /// The "equal to" operator can either take
    /// 1) integral- or 2) floating-type operands.
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
    public class Equal : BinaryArithmeticComp {
        public Equal(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public override void SetLong(CGenState state) => state.SETE(Reg.AL);
        public override void SetULong(CGenState state) => state.SETE(Reg.AL);
        public override void SetFloat(CGenState state) => state.SETE(Reg.AL);
        public override void SetDouble(CGenState state) => state.SETE(Reg.AL);
    }

    /// <summary>
    /// The "not equal to" operator can either take
    /// 1) integral- or 2) floating-type operands.
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
    public class NotEqual : BinaryArithmeticComp {
        public NotEqual(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) { }

        public override void SetLong(CGenState state) => state.SETNE(Reg.AL);
        public override void SetULong(CGenState state) => state.SETNE(Reg.AL);
        public override void SetFloat(CGenState state) => state.SETNE(Reg.AL);
        public override void SetDouble(CGenState state) => state.SETNE(Reg.AL);
    }


    /// <summary>
    /// Left && Right: can only take scalars (to compare with 0).
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
    public class LogicalAnd : Expr {
        public LogicalAnd(Expr lhs, Expr rhs, ExprType type)
            : base(type) {
            this.lhs = lhs;
            this.rhs = rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;
        public override Env Env => rhs.Env;

        public override Reg CGenValue(Env env, CGenState state) {
            Int32 label_reset = state.label_idx;
            state.label_idx++;
            Int32 label_finish = state.label_idx;
            state.label_idx++;

            Reg ret = lhs.CGenValue(env, state);
            switch (ret) {
                case Reg.EAX:
                    state.TESTL(Reg.EAX, Reg.EAX);
                    state.JZ(label_reset);
                    break;

                case Reg.ST0:
                    state.FLDZ();
                    state.FUCOMIP();
                    state.FSTP(Reg.ST0);
                    state.JZ(label_reset);
                    break;

                default:
                    throw new InvalidProgramException();
            }

            ret = rhs.CGenValue(env, state);
            switch (ret) {
                case Reg.EAX:
                    state.TESTL(Reg.EAX, Reg.EAX);
                    state.JZ(label_reset);
                    break;

                case Reg.ST0:
                    state.FLDZ();
                    state.FUCOMIP();
                    state.FSTP(Reg.ST0);
                    state.JZ(label_reset);
                    break;

                default:
                    throw new InvalidProgramException();
            }

            state.MOVL(1, Reg.EAX);

            state.JMP(label_finish);

            state.CGenLabel(label_reset);

            state.MOVL(0, Reg.EAX);

            state.CGenLabel(label_finish);

            return Reg.EAX;
        }
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
    ///        | cmp Left |-------+
    ///        +---------+       |
    ///             |            |
    ///             | 0          |
    ///             |            |
    ///        +----+----+   1   |
    ///        | cmp Right |-------+
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
    public class LogicalOr : Expr {
        public LogicalOr(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }

        public readonly Expr lhs;
        public readonly Expr rhs;
        public override Env Env => rhs.Env;

        public override Reg CGenValue(Env env, CGenState state) {
            Int32 label_set = state.label_idx;
            state.label_idx++;
            Int32 label_finish = state.label_idx;
            state.label_idx++;

            Reg ret = lhs.CGenValue(env, state);
            switch (ret) {
                case Reg.EAX:
                    state.TESTL(Reg.EAX, Reg.EAX);
                    state.JNZ(label_set);
                    break;

                case Reg.ST0:
                    state.FLDZ();
                    state.FUCOMIP();
                    state.FSTP(Reg.ST0);
                    state.JNZ(label_set);
                    break;

                default:
                    throw new InvalidProgramException();
            }

            ret = rhs.CGenValue(env, state);
            switch (ret) {
                case Reg.EAX:
                    state.TESTL(Reg.EAX, Reg.EAX);
                    state.JNZ(label_set);
                    break;

                case Reg.ST0:
                    state.FLDZ();
                    state.FUCOMIP();
                    state.FSTP(Reg.ST0);
                    state.JNZ(label_set);
                    break;

                default:
                    throw new InvalidProgramException();
            }

            state.MOVL(0, Reg.EAX);

            state.JMP(label_finish);

            state.CGenLabel(label_set);

            state.MOVL(1, Reg.EAX);

            state.CGenLabel(label_finish);

            return Reg.EAX;
        }
    }


}
