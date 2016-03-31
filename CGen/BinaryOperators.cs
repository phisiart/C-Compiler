using System;
using System.Diagnostics;
using CodeGeneration;

namespace AST {
    public abstract partial class BinaryIntegralOp {
        /// <summary>
        /// Before calling this method, %eax = Left, %ebx = Right
        /// This method should let %eax = %eax op %ebx
        /// </summary>
        public abstract void OperateLong(CGenState state);

        /// <summary>
        /// Before calling this method, %eax = Left, %ebx = Right
        /// This method should let %eax = %eax op %ebx
        /// </summary>
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
            if (this.Left.CGenValue(env, state) != Reg.EAX) {
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
            if (this.Right.CGenValue(env, state) != Reg.EAX) {
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
            switch (this.Left.Type.Kind) {
                case ExprTypeKind.LONG:
                    if (this.Left.Type.Kind != ExprTypeKind.LONG || this.Right.Type.Kind != ExprTypeKind.LONG) {
                        throw new InvalidOperationException();
                    }
                    return CGenLong(env, state);

                case ExprTypeKind.ULONG:
                    if (this.Left.Type.Kind != ExprTypeKind.ULONG || this.Right.Type.Kind != ExprTypeKind.ULONG) {
                        throw new InvalidOperationException();
                    }
                    return CGenULong(env, state);

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public abstract partial class BinaryArithmeticOp {
        /// <summary>
        /// Before calling this method, %st(0) = Left, %st(1) = Right
        /// This method should let %st(0) = %st(0) op %st(1)
        /// After calling this method, %st(1) would not be used.
        /// </summary>
        public abstract void OperateFloat(CGenState state);

        /// <summary>
        /// Before calling this method, %st(0) = Left, %st(1) = Right
        /// This method should let %st(0) = %st(0) op %st(1)
        /// After calling this method, %st(1) would not be used.
        /// </summary>
        public abstract void OperateDouble(CGenState state);

        public Reg CGenFloat(Env env, CGenState state) {
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
            var ret = this.Left.CGenValue(env, state);
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
            ret = this.Right.CGenValue(env, state);
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
            var ret = this.Left.CGenValue(env, state);
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
            ret = this.Right.CGenValue(env, state);
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
            switch (this.Type.Kind) {
                case ExprTypeKind.FLOAT:
                    if (this.Left.Type.Kind != ExprTypeKind.FLOAT || this.Right.Type.Kind != ExprTypeKind.FLOAT) {
                        throw new InvalidOperationException();
                    }
                    return CGenFloat(env, state);

                case ExprTypeKind.DOUBLE:
                    if (this.Left.Type.Kind != ExprTypeKind.DOUBLE || this.Right.Type.Kind != ExprTypeKind.DOUBLE) {
                        throw new InvalidOperationException();
                    }
                    return CGenDouble(env, state);

                default:
                    return base.CGenValue(env, state);
            }
        }
    }

    public sealed partial class Multiply {
        public override void OperateLong(CGenState state) {
            state.IMUL(Reg.EBX);
        }

        public override void OperateULong(CGenState state) {
            state.MUL(Reg.EBX);
        }

        public override void OperateFloat(CGenState state) {
            state.FMULP();
        }

        public override void OperateDouble(CGenState state) {
            state.FMULP();
        }
    }

    public sealed partial class Divide {
        public override void OperateLong(CGenState state) {
            state.CLTD();
            state.IDIVL(Reg.EBX);
        }

        public override void OperateULong(CGenState state) {
            state.CLTD();
            state.DIVL(Reg.EBX);
        }

        public override void OperateFloat(CGenState state) {
            state.FDIVP();
        }

        public override void OperateDouble(CGenState state) {
            state.FDIVP();
        }
    }

    public sealed partial class Modulo {
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

    public sealed partial class Xor {
        public override void OperateLong(CGenState state) {
            state.XORL(Reg.EBX, Reg.EAX);   
        }

        public override void OperateULong(CGenState state) {
            state.XORL(Reg.EBX, Reg.EAX);
        }
    }

    public sealed partial class BitwiseOr {
        public override void OperateLong(CGenState state) {
            state.ORL(Reg.EBX, Reg.EAX);
        }

        public override void OperateULong(CGenState state) {
            state.ORL(Reg.EBX, Reg.EAX);
        }
    }

    public sealed partial class BitwiseAnd {
        public override void OperateLong(CGenState state) {
            state.ANDL(Reg.EBX, Reg.EAX);
        }

        public override void OperateULong(CGenState state) {
            state.ANDL(Reg.EBX, Reg.EAX);
        }
    }

    public sealed partial class LShift {
        public override void OperateLong(CGenState state) {
            state.SALL(Reg.EBX, Reg.EAX);
        }

        public override void OperateULong(CGenState state) {
            state.SALL(Reg.EBX, Reg.EAX);
        }
    }

    public sealed partial class RShift {
        public override void OperateLong(CGenState state) {
            state.SARL(Reg.EBX, Reg.EAX);
        }

        public override void OperateULong(CGenState state) {
            state.SHRL(Reg.EBX, Reg.EAX);
        }
    }

    public sealed partial class Add {
        public override void OperateLong(CGenState state) {
            state.ADDL(Reg.EBX, Reg.EAX);
        }

        public override void OperateULong(CGenState state) {
            state.ADDL(Reg.EBX, Reg.EAX);
        }

        public override void OperateFloat(CGenState state) {
            state.FADDP();
        }

        public override void OperateDouble(CGenState state) {
            state.FADDP();
        }
    }

    public sealed partial class Sub {
        public override void OperateLong(CGenState state) {
            state.SUBL(Reg.EBX, Reg.EAX);
        }

        public override void OperateULong(CGenState state) {
            state.SUBL(Reg.EBX, Reg.EAX);
        }

        public override void OperateFloat(CGenState state) {
            state.FSUBP();
        }

        public override void OperateDouble(CGenState state) {
            state.FSUBP();
        }
    }

    public abstract partial class BinaryArithmeticComp {
        public abstract void SetLong(CGenState state);

        public abstract void SetULong(CGenState state);

        public abstract void SetFloat(CGenState state);

        public abstract void SetDouble(CGenState state);

        public override sealed void OperateLong(CGenState state) {
            state.CMPL(Reg.EBX, Reg.EAX);
            SetLong(state);
            state.MOVZBL(Reg.AL, Reg.EAX);
        }

        public override sealed void OperateULong(CGenState state) {
            state.CMPL(Reg.EBX, Reg.EAX);
            SetULong(state);
            state.MOVZBL(Reg.AL, Reg.EAX);
        }

        public override sealed void OperateFloat(CGenState state) {
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
            //    Pop one Value from FPU stack.
            // 
            // float stack:
            // +-----+
            // | Right | <- %st(0)
            // +-----+
            // 
            state.FUCOMIP();

            // 2. Pop another Value from FPU stack.
            // 
            // float stack:
            // +-----+ empty
            // 
            state.FSTP(Reg.ST0);

            // 3. Set bit based on comparison result.
            SetFloat(state);
            state.MOVZBL(Reg.AL, Reg.EAX);
        }

        public override sealed void OperateDouble(CGenState state) {
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
            //    Pop one Value from FPU stack.
            // 
            // float stack:
            // +-----+
            // | Right | <- %st(0)
            // +-----+
            // 
            state.FUCOMIP();

            // 2. Pop another Value from FPU stack.
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

    public sealed partial class GEqual {
        public override void SetLong(CGenState state) {
            state.SETGE(Reg.AL);
        }

        public override void SetULong(CGenState state) {
            state.SETNB(Reg.AL);
        }

        public override void SetFloat(CGenState state) {
            state.SETNB(Reg.AL);
        }

        public override void SetDouble(CGenState state) {
            state.SETNB(Reg.AL);
        }
    }

    public sealed partial class Greater {
        public override void SetLong(CGenState state) {
            state.SETG(Reg.AL);
        }

        public override void SetULong(CGenState state) {
            state.SETA(Reg.AL);
        }

        public override void SetFloat(CGenState state) {
            state.SETA(Reg.AL);
        }

        public override void SetDouble(CGenState state) {
            state.SETA(Reg.AL);
        }
    }

    public sealed partial class LEqual {
        public override void SetLong(CGenState state) {
            state.SETLE(Reg.AL);
        }

        public override void SetULong(CGenState state) {
            state.SETNA(Reg.AL);
        }

        public override void SetFloat(CGenState state) {
            state.SETNA(Reg.AL);
        }

        public override void SetDouble(CGenState state) {
            state.SETNA(Reg.AL);
        }
    }

    public sealed partial class Less {
        public override void SetLong(CGenState state) {
            state.SETL(Reg.AL);
        }

        public override void SetULong(CGenState state) {
            state.SETB(Reg.AL);
        }

        public override void SetFloat(CGenState state) {
            state.SETB(Reg.AL);
        }

        public override void SetDouble(CGenState state) {
            state.SETB(Reg.AL);
        }
    }

    public sealed partial class Equal {
        public override void SetLong(CGenState state) {
            state.SETE(Reg.AL);
        }

        public override void SetULong(CGenState state) {
            state.SETE(Reg.AL);
        }

        public override void SetFloat(CGenState state) {
            state.SETE(Reg.AL);
        }

        public override void SetDouble(CGenState state) {
            state.SETE(Reg.AL);
        }
    }

    public sealed partial class NotEqual {
        public override void SetLong(CGenState state) {
            state.SETNE(Reg.AL);
        }

        public override void SetULong(CGenState state) {
            state.SETNE(Reg.AL);
        }

        public override void SetFloat(CGenState state) {
            state.SETNE(Reg.AL);
        }

        public override void SetDouble(CGenState state) {
            state.SETNE(Reg.AL);
        }
    }

    public sealed partial class LogicalAnd {
        public override Reg CGenValue(Env env, CGenState state) {
            Int32 label_reset = state.label_idx;
            state.label_idx++;
            Int32 label_finish = state.label_idx;
            state.label_idx++;

            Reg ret = this.Left.CGenValue(env, state);
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

            ret = this.Right.CGenValue(env, state);
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

    public sealed partial class LogicalOr {
        public override Reg CGenValue(Env env, CGenState state) {
            Int32 label_set = state.label_idx;
            state.label_idx++;
            Int32 label_finish = state.label_idx;
            state.label_idx++;

            Reg ret = this.Left.CGenValue(env, state);
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

            ret = this.Right.CGenValue(env, state);
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
