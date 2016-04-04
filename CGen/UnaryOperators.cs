using System;
using System.Diagnostics;
using CodeGeneration;

namespace AST {
    public abstract partial class IncDecExpr {

        // Integral
        // Before the actual calculation, the state is set to this.
        // 
        // regs:
        // %eax = expr
        // %ebx = expr
        // %ecx = &expr
        // (Yes, both %eax and %ebx are expr.)
        // 
        // stack:
        // +-------+
        // | ..... | <- %esp
        // +-------+
        // 
        // After the calculation, the result should be in %eax,
        // and memory should be updated.
        //
        public abstract void CalcAndSaveLong(CGenState state);

        public abstract void CalcAndSaveWord(CGenState state);

        public abstract void CalcAndSaveByte(CGenState state);

        public abstract void CalcAndSavePtr(CGenState state);

        // Float
        // Before the actual calculation, the state is set to this.
        // 
        // regs:
        // %ecx = &expr
        // 
        // stack:
        // +-------+
        // | ..... | <- %esp
        // +-------+
        // 
        // float stack:
        // +-------+
        // | expr  | <- %st(1)
        // +-------+
        // |  1.0  | <- %st(0)
        // +-------+
        // 
        // After the calculation, the result should be in %st(0),
        // and memory should be updated.
        // 
        public abstract void CalcAndSaveFloat(CGenState state);

        public abstract void CalcAndSaveDouble(CGenState state);

        public override sealed Reg CGenValue(Env env, CGenState state) {

            // 1. Get the address of expr.
            // 
            // regs:
            // %eax = &expr
            // 
            // stack:
            // +-------+
            // | ..... | <- %esp
            // +-------+
            // 
            this.Expr.CGenAddress(state);

            // 2. Push address.
            // 
            // regs:
            // %eax = &expr
            // 
            // stack:
            // +-------+
            // | ..... |
            // +-------+
            // | &expr | <- %esp
            // +-------+
            // 
            Int32 stack_size = state.CGenPushLong(Reg.EAX);

            // 3. Get current Value of expr.
            // 
            // 1) If expr is an integral or pointer:
            // 
            // regs:
            // %eax = expr
            // 
            // stack:
            // +-------+
            // | ..... |
            // +-------+
            // | &expr | <- %esp
            // +-------+
            // 
            // 
            // 2) If expr is a float:
            // 
            // regs:
            // %eax = &expr
            // 
            // stack:
            // +-------+
            // | ..... |
            // +-------+
            // | &expr | <- %esp
            // +-------+
            // 
            // float stack:
            // +-------+
            // | expr  | <- %st(0)
            // +-------+
            // 
            Reg ret = this.Expr.CGenValue(env, state);

            switch (ret) {
                case Reg.EAX:
                    // expr is an integral or pointer.

                    // 4. Pop address to %ecx.
                    // 
                    // regs:
                    // %eax = expr
                    // %ecx = &expr
                    // 
                    // stack:
                    // +-------+
                    // | ..... | <- %esp
                    // +-------+
                    // 
                    state.CGenPopLong(stack_size, Reg.ECX);

                    // 5. Cache current Value of Expr in %ebx.
                    // 
                    // regs:
                    // %eax = expr
                    // %ebx = expr
                    // %ecx = &expr
                    // 
                    // stack:
                    // +-------+
                    // | ..... | <- %esp
                    // +-------+
                    // 
                    state.MOVL(Reg.EAX, Reg.EBX);

                    // 6. Calculate the new value in %ebx or %eax and save.
                    //    Set %eax to be the return Value.
                    // 
                    // regs:
                    // %eax = expr or (expr +- 1)
                    // %ebx = (expr +- 1) or expr
                    // %ecx = &expr
                    // 
                    // stack:
                    // +-------+
                    // | ..... | <- %esp
                    // +-------+
                    // 
                    switch (this.Expr.Type.Kind) {
                        case ExprTypeKind.CHAR:
                        case ExprTypeKind.UCHAR:
                            CalcAndSaveByte(state);
                            return Reg.EAX;

                        case ExprTypeKind.SHORT:
                        case ExprTypeKind.USHORT:
                            CalcAndSaveWord(state);
                            return Reg.EAX;

                        case ExprTypeKind.LONG:
                        case ExprTypeKind.ULONG:
                            CalcAndSaveByte(state);
                            return Reg.EAX;

                        case ExprTypeKind.POINTER:
                            CalcAndSavePtr(state);
                            return Reg.EAX;

                        default:
                            throw new InvalidProgramException();
                    }

                case Reg.ST0:
                    // Expr is a float.

                    // 4. Pop address to %ecx.
                    // 
                    // regs:
                    // %ecx = &expr
                    // 
                    // stack:
                    // +-------+
                    // | ..... | <- %esp
                    // +-------+
                    // 
                    state.CGenPopLong(stack_size, Reg.ECX);

                    // 5. Load 1.0 to FPU stack.
                    // 
                    // regs:
                    // %ecx = &expr
                    // 
                    // stack:
                    // +-------+
                    // | ..... | <- %esp
                    // +-------+
                    // 
                    // float stack:
                    // +-------+
                    // | expr  | <- %st(1)
                    // +-------+
                    // |  1.0  | <- %st(0)
                    // +-------+
                    // 
                    state.FLD1();

                    // 6. Calculate the new value and save back.
                    //    Set %st(0) to be the new or original Value.
                    // 
                    // regs:
                    // %ecx = &Expr
                    // 
                    // stack:
                    // +-------+
                    // | ..... | <- %esp
                    // +-------+
                    // 
                    // float stack:
                    // +---------------------+
                    // | expr or (epxr +- 1) | <- %st(0)
                    // +---------------------+
                    // 
                    switch (this.Expr.Type.Kind) {
                        case ExprTypeKind.FLOAT:
                            CalcAndSaveFloat(state);
                            return Reg.ST0;

                        case ExprTypeKind.DOUBLE:
                            CalcAndSaveDouble(state);
                            return Reg.ST0;

                        default:
                            throw new InvalidProgramException();
                    }

                default:
                    throw new InvalidProgramException();
            }

        }

        public override sealed void CGenAddress(CGenState state) {
            throw new InvalidOperationException(
                "Cannot get the address of an increment/decrement expression."
            );
        }
    }

    public sealed partial class PostIncrement {
        public override void CalcAndSaveLong(CGenState state) {
            state.ADDL(1, Reg.EBX);
            state.MOVL(Reg.EBX, 0, Reg.ECX);
        }

        public override void CalcAndSaveWord(CGenState state) {
            state.ADDL(1, Reg.EBX);
            state.MOVW(Reg.BX, 0, Reg.ECX);
        }

        public override void CalcAndSaveByte(CGenState state) {
            state.ADDL(1, Reg.EBX);
            state.MOVB(Reg.BL, 0, Reg.ECX);
        }

        public override void CalcAndSavePtr(CGenState state) {
            state.ADDL(this.Expr.Type.SizeOf, Reg.EBX);
            state.MOVL(Reg.EBX, 0, Reg.ECX);
        }

        public override void CalcAndSaveFloat(CGenState state) {
            state.FADD(1, 0);
            state.FSTPS(0, Reg.ECX);
        }

        public override void CalcAndSaveDouble(CGenState state) {
            state.FADD(1, 0);
            state.FSTPL(0, Reg.ECX);
        }
    }

    public sealed partial class PostDecrement {
        public override void CalcAndSaveLong(CGenState state) {
            state.SUBL(1, Reg.EBX);
            state.MOVL(Reg.EBX, 0, Reg.ECX);
        }

        public override void CalcAndSaveWord(CGenState state) {
            state.SUBL(1, Reg.EBX);
            state.MOVW(Reg.BX, 0, Reg.ECX);
        }

        public override void CalcAndSaveByte(CGenState state) {
            state.SUBL(1, Reg.EBX);
            state.MOVB(Reg.BL, 0, Reg.ECX);
        }

        public override void CalcAndSavePtr(CGenState state) {
            state.SUBL(this.Expr.Type.SizeOf, Reg.EBX);
            state.MOVL(Reg.EBX, 0, Reg.ECX);
        }

        public override void CalcAndSaveFloat(CGenState state) {
            state.FSUB(1, 0);
            state.FSTPS(0, Reg.ECX);
        }

        public override void CalcAndSaveDouble(CGenState state) {
            state.FSUB(1, 0);
            state.FSTPL(0, Reg.ECX);
        }
    }

    public sealed partial class PreIncrement {
        public override void CalcAndSaveLong(CGenState state) {
            state.ADDL(1, Reg.EAX);
            state.MOVL(Reg.EAX, 0, Reg.ECX);
        }

        public override void CalcAndSaveWord(CGenState state) {
            state.ADDL(1, Reg.EAX);
            state.MOVW(Reg.AX, 0, Reg.ECX);
        }

        public override void CalcAndSaveByte(CGenState state) {
            state.ADDL(1, Reg.EAX);
            state.MOVB(Reg.AL, 0, Reg.ECX);
        }

        public override void CalcAndSavePtr(CGenState state) {
            state.ADDL(this.Expr.Type.SizeOf, Reg.EAX);
            state.MOVL(Reg.EAX, 0, Reg.ECX);
        }

        public override void CalcAndSaveFloat(CGenState state) {
            state.FADD(1, 0);
            state.FSTS(0, Reg.ECX);
        }

        public override void CalcAndSaveDouble(CGenState state) {
            state.FADD(1, 0);
            state.FSTL(0, Reg.ECX);
        }
    }

    public sealed partial class PreDecrement {
        public override void CalcAndSaveLong(CGenState state) {
            state.SUBL(1, Reg.EAX);
            state.MOVL(Reg.EAX, 0, Reg.ECX);
        }

        public override void CalcAndSaveWord(CGenState state) {
            state.SUBL(1, Reg.EAX);
            state.MOVW(Reg.AX, 0, Reg.ECX);
        }

        public override void CalcAndSaveByte(CGenState state) {
            state.SUBL(1, Reg.EAX);
            state.MOVB(Reg.AL, 0, Reg.ECX);
        }

        public override void CalcAndSavePtr(CGenState state) {
            state.SUBL(this.Expr.Type.SizeOf, Reg.EAX);
            state.MOVL(Reg.EAX, 0, Reg.ECX);
        }

        public override void CalcAndSaveFloat(CGenState state) {
            state.FSUB(1, 0);
            state.FSTS(0, Reg.ECX);
        }

        public override void CalcAndSaveDouble(CGenState state) {
            state.FSUB(1, 0);
            state.FSTL(0, Reg.ECX);
        }
    }

    public abstract partial class UnaryArithOp {
        public override sealed void CGenAddress(CGenState state) {
            throw new InvalidOperationException(
                "Cannot get the address of an unary arithmetic operator."
            );
        }
    }

    public sealed partial class Negative {
        public override Reg CGenValue(Env env, CGenState state) {
            Reg ret = this.Expr.CGenValue(env, state);
            switch (ret) {
                case Reg.EAX:
                    state.NEG(Reg.EAX);
                    return Reg.EAX;

                case Reg.ST0:
                    state.FCHS();
                    return Reg.ST0;

                default:
                    throw new InvalidProgramException();
            }
        }
    }

    public sealed partial class BitwiseNot {
        public override Reg CGenValue(Env env, CGenState state) {
            Reg ret = this.Expr.CGenValue(env, state);
            if (ret != Reg.EAX) {
                throw new InvalidProgramException();
            }
            state.NOT(Reg.EAX);
            return Reg.EAX;
        }
    }

    public sealed partial class LogicalNot {
        public override Reg CGenValue(Env env, CGenState state) {
            Reg ret = this.Expr.CGenValue(env, state);
            switch (ret) {
                case Reg.EAX:
                    state.TESTL(Reg.EAX, Reg.EAX);
                    state.SETE(Reg.AL);
                    state.MOVZBL(Reg.AL, Reg.EAX);
                    return Reg.EAX;

                case Reg.ST0:
                    /// Compare Expr with 0.0
                    /// < see cref = "BinaryComparisonOp.OperateFloat(CGenState)" />
                    state.FLDZ();
                    state.FUCOMIP();
                    state.FSTP(Reg.ST0);
                    state.SETE(Reg.AL);
                    state.MOVZBL(Reg.AL, Reg.EAX);
                    return Reg.EAX;

                default:
                    throw new InvalidProgramException();
            }
        }
    }
}
