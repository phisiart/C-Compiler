using System;
using System.Diagnostics;
using CodeGeneration;

namespace AST {

    public abstract class IncDecExpr : Expr {
        protected IncDecExpr(Expr expr)
            : base(expr.Type) {
            Debug.Assert(expr.Type.IsScalar);
            this.expr = expr;
        }
        public readonly Expr expr;
        public override Env Env => this.expr.Env;

        // Integral
        // Before the actual calculation, the state is set to this.
        // 
        // regs:
        // %eax = Expr
        // %ebx = Expr
        // %ecx = &Expr
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
        // %ecx = &Expr
        // 
        // stack:
        // +-------+
        // | ..... | <- %esp
        // +-------+
        // 
        // float stack:
        // +-------+
        // | Expr  | <- %st(1)
        // +-------+
        // |  1.0  | <- %st(0)
        // +-------+
        // 
        // After the calculation, the result should be in %st(0),
        // and memory should be updated.
        // 
        public abstract void CalcAndSaveFloat(CGenState state);
        public abstract void CalcAndSaveDouble(CGenState state);

        public override Reg CGenValue(Env env, CGenState state) {

            // 1. Get the address of Expr.
            // 
            // regs:
            // %eax = &Expr
            // 
            // stack:
            // +-------+
            // | ..... | <- %esp
            // +-------+
            // 
            this.expr.CGenAddress(env, state);

            // 2. Push address.
            // 
            // regs:
            // %eax = &Expr
            // 
            // stack:
            // +-------+
            // | ..... |
            // +-------+
            // | &Expr | <- %esp
            // +-------+
            // 
            Int32 stack_size = state.CGenPushLong(Reg.EAX);

            // 3. Get current value of Expr.
            // 
            // 1) If Expr is an integral or pointer:
            // 
            // regs:
            // %eax = Expr
            // 
            // stack:
            // +-------+
            // | ..... |
            // +-------+
            // | &Expr | <- %esp
            // +-------+
            // 
            // 
            // 2) If Expr is a float:
            // 
            // regs:
            // %eax = &Expr
            // 
            // stack:
            // +-------+
            // | ..... |
            // +-------+
            // | &Expr | <- %esp
            // +-------+
            // 
            // float stack:
            // +-------+
            // | Expr  | <- %st(0)
            // +-------+
            // 
            Reg ret = this.expr.CGenValue(env, state);

            switch (ret) {
                case Reg.EAX:
                    // Expr is an integral or pointer.

                    // 4. Pop address to %ecx.
                    // 
                    // regs:
                    // %eax = Expr
                    // %ecx = &Expr
                    // 
                    // stack:
                    // +-------+
                    // | ..... | <- %esp
                    // +-------+
                    // 
                    state.CGenPopLong(stack_size, Reg.ECX);

                    // 5. Cache current value of Expr in %ebx.
                    // 
                    // regs:
                    // %eax = Expr
                    // %ebx = Expr
                    // %ecx = &Expr
                    // 
                    // stack:
                    // +-------+
                    // | ..... | <- %esp
                    // +-------+
                    // 
                    state.MOVL(Reg.EAX, Reg.EBX);

                    // 6. Calculate the new value in %ebx or %eax and save.
                    //    Set %eax to be the return value.
                    // 
                    // regs:
                    // %eax = Expr or (Expr +- 1)
                    // %ebx = (Expr +- 1) or Expr
                    // %ecx = &Expr
                    // 
                    // stack:
                    // +-------+
                    // | ..... | <- %esp
                    // +-------+
                    // 
                    switch (this.expr.Type.kind) {
                        case ExprType.Kind.CHAR:
                        case ExprType.Kind.UCHAR:
                            CalcAndSaveByte(state);
                            return Reg.EAX;

                        case ExprType.Kind.SHORT:
                        case ExprType.Kind.USHORT:
                            CalcAndSaveWord(state);
                            return Reg.EAX;

                        case ExprType.Kind.LONG:
                        case ExprType.Kind.ULONG:
                            CalcAndSaveByte(state);
                            return Reg.EAX;

                        case ExprType.Kind.POINTER:
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
                    // %ecx = &Expr
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
                    // %ecx = &Expr
                    // 
                    // stack:
                    // +-------+
                    // | ..... | <- %esp
                    // +-------+
                    // 
                    // float stack:
                    // +-------+
                    // | Expr  | <- %st(1)
                    // +-------+
                    // |  1.0  | <- %st(0)
                    // +-------+
                    // 
                    state.FLD1();

                    // 6. Calculate the new value and save back.
                    //    Set %st(0) to be the new or original value.
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
                    // | Expr or (epxr +- 1) | <- %st(0)
                    // +---------------------+
                    // 
                    switch (this.expr.Type.kind) {
                        case ExprType.Kind.FLOAT:
                            CalcAndSaveFloat(state);
                            return Reg.ST0;

                        case ExprType.Kind.DOUBLE:
                            CalcAndSaveDouble(state);
                            return Reg.ST0;

                        default:
                            throw new InvalidProgramException();
                    }

                default:
                    throw new InvalidProgramException();
            }

        }
    }

    /// <summary>
    /// Expr++: must be integral, float or pointer.
    /// 
    /// If Expr is an array, it is converted to a pointer in semantic analysis.
    /// </summary>
    public sealed class PostIncrement : IncDecExpr {
        public PostIncrement(Expr expr)
            : base(expr) { }

        // Before the actual calculation, the state is set to this.
        // 
        // regs:
        // %eax = Expr
        // %ebx = Expr
        // %ecx = &Expr
        // 
        // stack:
        // +-------+
        // | ..... | <- %esp
        // +-------+
        // 
        // Calculate the new value in %ebx, and save.
        // Leave %eax to be the original value.
        // 
        // regs:
        // %eax = Expr
        // %ebx = Expr + 1
        // %ecx = &Expr
        // 
        // stack:
        // +-------+
        // | ..... | <- %esp
        // +-------+
        // 
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
            state.ADDL(this.expr.Type.SizeOf, Reg.EBX);
            state.MOVL(Reg.EBX, 0, Reg.ECX);
        }

        // Before the actual calculation, the state is set to this.
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
        // +-------+
        // | Expr  | <- %st(1)
        // +-------+
        // |  1.0  | <- %st(0)
        // +-------+
        // 
        // 1. Compute %st(1) + %st(0) and stores in %st(0).
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
        // +------------+
        // |    Expr    | <- %st(1)
        // +------------+
        // | Expr + 1.0 | <- %st(0)
        // +------------+
        // 
        // 2. Pop result from FPU stack and store in memory.
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
        // +------------+
        // |    Expr    | <- %st(0)
        // +------------+
        // 
        public override void CalcAndSaveFloat(CGenState state) {
            state.FADD(1, 0);
            state.FSTPS(0, Reg.ECX);
        }

        public override void CalcAndSaveDouble(CGenState state) {
            state.FADD(1, 0);
            state.FSTPL(0, Reg.ECX);
        }
    }

    /// <summary>
    /// Expr--: must be a scalar
    /// </summary>
    public sealed class PostDecrement : IncDecExpr {
        public PostDecrement(Expr expr)
            : base(expr) { }

        // Before the actual calculation, the state is set to this.
        // 
        // regs:
        // %eax = Expr
        // %ebx = Expr
        // %ecx = &Expr
        // 
        // stack:
        // +-------+
        // | ..... | <- %esp
        // +-------+
        // 
        // Calculate the new value in %ebx, and save.
        // Leave %eax to be the original value.
        // 
        // regs:
        // %eax = Expr
        // %ebx = Expr - 1
        // %ecx = &Expr
        // 
        // stack:
        // +-------+
        // | ..... | <- %esp
        // +-------+
        // 
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
            state.SUBL(this.expr.Type.SizeOf, Reg.EBX);
            state.MOVL(Reg.EBX, 0, Reg.ECX);
        }

        // Before the actual calculation, the state is set to this.
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
        // +-------+
        // | Expr  | <- %st(1)
        // +-------+
        // |  1.0  | <- %st(0)
        // +-------+
        // 
        // 1. Compute %st(1) - %st(0) and stores in %st(0).
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
        // +------------+
        // |    Expr    | <- %st(1)
        // +------------+
        // | Expr - 1.0 | <- %st(0)
        // +------------+
        // 
        // 2. Pop result from FPU stack and store in memory.
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
        // +------------+
        // |    Expr    | <- %st(0)
        // +------------+
        // 
        public override void CalcAndSaveFloat(CGenState state) {
            state.FSUB(1, 0);
            state.FSTPS(0, Reg.ECX);
        }

        public override void CalcAndSaveDouble(CGenState state) {
            state.FSUB(1, 0);
            state.FSTPL(0, Reg.ECX);
        }
    }

    /// <summary>
    /// ++Expr: must be a scalar
    /// </summary>
    public sealed class PreIncrement : IncDecExpr {
        public PreIncrement(Expr expr)
            : base(expr) { }

        // Before the actual calculation, the state is set to this.
        // 
        // regs:
        // %eax = Expr
        // %ebx = Expr
        // %ecx = &Expr
        // 
        // stack:
        // +-------+
        // | ..... | <- %esp
        // +-------+
        // 
        // Calculate the new value in %eax, and save.
        // Leave %eax to be the original value.
        // 
        // regs:
        // %eax = Expr + 1
        // %ebx = Expr
        // %ecx = &Expr
        // 
        // stack:
        // +-------+
        // | ..... | <- %esp
        // +-------+
        // 
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
            state.ADDL(this.expr.Type.SizeOf, Reg.EAX);
            state.MOVL(Reg.EAX, 0, Reg.ECX);
        }

        // Before the actual calculation, the state is set to this.
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
        // +-------+
        // | Expr  | <- %st(1)
        // +-------+
        // |  1.0  | <- %st(0)
        // +-------+
        // 
        // 1. Compute %st(1) + %st(0) and stores in %st(0).
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
        // +------------+
        // |    Expr    | <- %st(1)
        // +------------+
        // | Expr + 1.0 | <- %st(0)
        // +------------+
        // 
        // 2. Store %st(0) in memory.
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
        // +------------+
        // | Expr + 1.0 | <- %st(0)
        // +------------+
        // 
        public override void CalcAndSaveFloat(CGenState state) {
            state.FADD(1, 0);
            state.FSTS(0, Reg.ECX);
        }

        public override void CalcAndSaveDouble(CGenState state) {
            state.FADD(1, 0);
            state.FSTL(0, Reg.ECX);
        }
    }

    /// <summary>
    /// --Expr: must be a scalar
    /// </summary>
    public sealed class PreDecrement : IncDecExpr {
        public PreDecrement(Expr expr)
            : base(expr) { }

        // Before the actual calculation, the state is set to this.
        // 
        // regs:
        // %eax = Expr
        // %ebx = Expr
        // %ecx = &Expr
        // 
        // stack:
        // +-------+
        // | ..... | <- %esp
        // +-------+
        // 
        // Calculate the new value in %eax, and save.
        // Leave %eax to be the original value.
        // 
        // regs:
        // %eax = Expr - 1
        // %ebx = Expr
        // %ecx = &Expr
        // 
        // stack:
        // +-------+
        // | ..... | <- %esp
        // +-------+
        // 
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
            state.SUBL(this.expr.Type.SizeOf, Reg.EAX);
            state.MOVL(Reg.EAX, 0, Reg.ECX);
        }

        // Before the actual calculation, the state is set to this.
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
        // +-------+
        // | Expr  | <- %st(1)
        // +-------+
        // |  1.0  | <- %st(0)
        // +-------+
        // 
        // 1. Compute %st(1) - %st(0) and stores in %st(0).
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
        // +------------+
        // |    Expr    | <- %st(1)
        // +------------+
        // | Expr - 1.0 | <- %st(0)
        // +------------+
        // 
        // 2. Store %st(0) in memory.
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
        // +------------+
        // | Expr - 1.0 | <- %st(0)
        // +------------+
        // 
        public override void CalcAndSaveFloat(CGenState state) {
            state.FSUB(1, 0);
            state.FSTS(0, Reg.ECX);
        }

        public override void CalcAndSaveDouble(CGenState state) {
            state.FSUB(1, 0);
            state.FSTL(0, Reg.ECX);
        }
    }

    public abstract class UnaryArithOp : Expr {
        protected UnaryArithOp(Expr expr, ExprType type)
            : base(type) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public override Env Env => this.expr.Env;
    }

    /// <summary>
    /// -Expr: only takes arithmetic type.
    /// 
    /// After semantic analysis, only the following 4 types are possible:
    /// 1) long
    /// 2) ulong
    /// 3) float
    /// 4) double
    /// </summary>
    public sealed class Negative : UnaryArithOp {
        public Negative(Expr expr, ExprType type)
            : base(expr, type) { }

        public override Reg CGenValue(Env env, CGenState state) {
            Reg ret = this.expr.CGenValue(env, state);
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

    /// <summary>
    /// ~Expr: only takes integral type.
    /// 
    /// After semantic analysis, only the following 2 types are possible:
    /// 1) long
    /// 2) ulong
    /// </summary>
    public sealed class BitwiseNot : UnaryArithOp {
        public BitwiseNot(Expr expr, ExprType type)
            : base(expr, type) { }

        public override Reg CGenValue(Env env, CGenState state) {
            Reg ret = this.expr.CGenValue(env, state);
            if (ret != Reg.EAX) {
                throw new InvalidProgramException();
            }
            state.NOT(Reg.EAX);
            return Reg.EAX;
        }
    }

    /// <summary>
    /// !Expr: only takes scalar type.
    /// 
    /// After semantic analysis, only the following 4 types are possible:
    /// 1) long
    /// 2) ulong
    /// 3) float
    /// 4) double
    /// 
    /// Pointers are converted to ulongs.
    /// </summary>
    public sealed class LogicalNot : UnaryArithOp {
        public LogicalNot(Expr expr, ExprType type)
            : base(expr, type) { }

        public override Reg CGenValue(Env env, CGenState state) {
            Reg ret = this.expr.CGenValue(env, state);
            switch (ret) {
                case Reg.EAX:
                    state.TESTL(Reg.EAX, Reg.EAX);
                    state.SETE(Reg.AL);
                    state.MOVZBL(Reg.AL, Reg.EAX);
                    return Reg.EAX;

                case Reg.ST0:
                    /// Compare Expr with 0.0
                    /// < see cref = "BinaryArithmeticComp.OperateFloat(CGenState)" />
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
