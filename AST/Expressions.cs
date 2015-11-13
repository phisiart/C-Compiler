using System;
using System.Collections.Generic;
using System.Linq;
using CodeGeneration;

namespace AST {
    // Expr 
    // ========================================================================

    /// <summary>
    /// The cdecl calling convention:
    /// 1. arguments are passed on the stack, right to left.
    /// 2. int values and pointer values are returned in %eax.
    /// 3. floats are returned in %st(0).
    /// 4. when calling a function, %st(0) ~ %st(7) are all free.
    /// 5. functions are free to use %eax, %ecx, %edx, because caller needs to save them.
    /// 6. stack must be aligned to 4 bytes (before gcc 4.5, for gcc 4.5+, aligned to 16 bytes).
    /// </summary>

    public abstract class Expr {
        protected Expr(ExprType type) {
            this.Type = type;
        }
        public virtual Boolean IsConstExpr => false;
        public abstract Env Env { get; }
        public abstract Reg CGenValue(Env env, CGenState state);

        public virtual void CGenAddress(Env env, CGenState state) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The default implementation of CGenPush uses CGenValue.
        /// </summary>
        // TODO: struct and union
        [Obsolete]
        public virtual void CGenPush(Env env, CGenState state) {
            Reg ret = CGenValue(env, state);

            switch (this.Type.kind) {
                case ExprType.Kind.CHAR:
                case ExprType.Kind.UCHAR:
                case ExprType.Kind.SHORT:
                case ExprType.Kind.USHORT:
                case ExprType.Kind.LONG:
                case ExprType.Kind.ULONG:
                    // Integral
                    if (ret != Reg.EAX) {
                        throw new InvalidProgramException("Integral values should be returned to %eax");
                    }
                    state.CGenPushLong(Reg.EAX);
                    break;

                case ExprType.Kind.FLOAT:
                    // Float
                    if (ret != Reg.ST0) {
                        throw new InvalidProgramException("Floats should be returned to %st(0)");
                    }
                    state.CGenExpandStackBy4Bytes();
                    state.FSTS(0, Reg.ESP);
                    break;

                case ExprType.Kind.DOUBLE:
                    // Double
                    if (ret != Reg.ST0) {
                        throw new InvalidProgramException("Doubles should be returned to %st(0)");
                    }
                    state.CGenExpandStackBy8Bytes();
                    state.FSTL(0, Reg.ESP);
                    break;

                case ExprType.Kind.ARRAY:
                case ExprType.Kind.FUNCTION:
                case ExprType.Kind.POINTER:
                    // Pointer
                    if (ret != Reg.EAX) {
                        throw new InvalidProgramException("Pointer values should be returned to %eax");
                    }
                    state.CGenPushLong(Reg.EAX);
                    break;

                case ExprType.Kind.INCOMPLETE_ARRAY:
                case ExprType.Kind.VOID:
                    throw new InvalidProgramException(this.Type.kind + " can't be pushed onto the stack");

                case ExprType.Kind.STRUCT_OR_UNION:
                    throw new NotImplementedException();
            }

        }

        public ExprType Type { get; }
    }

    public class Variable : Expr {
        public Variable(ExprType type, String name, Env env)
            : base(type) {
            this.name = name;
            this.Env = env;
        }
        public readonly String name;

        public override Env Env { get; }

        public override void CGenAddress(Env env, CGenState state) {
            Env.Entry entry = env.Find(this.name).Value;
            Int32 offset = entry.offset;

            switch (entry.kind) {
                case Env.EntryKind.FRAME:
                case Env.EntryKind.STACK:
                    state.LEA(offset, Reg.EBP, Reg.EAX);
                    return;

                case Env.EntryKind.GLOBAL:
                    state.LEA(this.name, Reg.EAX);
                    return;

                case Env.EntryKind.ENUM:
                case Env.EntryKind.TYPEDEF:
                default:
                    throw new InvalidProgramException("cannot get the address of " + entry.kind);
            }
        }

        public override Reg CGenValue(Env env, CGenState state) {
            Env.Entry entry = env.Find(this.name).Value;

            Int32 offset = entry.offset;
            //if (entry.kind == Env.EntryKind.STACK) {
            //    offset = -offset;
            //}

            switch (entry.kind) {
                case Env.EntryKind.ENUM:
                    // 1. If the variable is an enum constant,
                    //    return the value in %eax.
                    state.MOVL(offset, Reg.EAX);
                    return Reg.EAX;

                case Env.EntryKind.FRAME:
                case Env.EntryKind.STACK:
                    // 2. If the variable is a function argument or a local variable,
                    //    the address would be offset(%ebp).
                    switch (this.Type.kind) {
                        case ExprType.Kind.LONG:
                        case ExprType.Kind.ULONG:
                        case ExprType.Kind.POINTER:
                            // %eax = offset(%ebp)
                            state.MOVL(offset, Reg.EBP, Reg.EAX);
                            return Reg.EAX;

                        case ExprType.Kind.FLOAT:
                            // %st(0) = offset(%ebp)
                            state.FLDS(offset, Reg.EBP);
                            return Reg.ST0;

                        case ExprType.Kind.DOUBLE:
                            // %st(0) = offset(%ebp)
                            state.FLDL(offset, Reg.EBP);
                            return Reg.ST0;

                        case ExprType.Kind.STRUCT_OR_UNION:
                            // %eax = address
                            state.LEA(offset, Reg.EBP, Reg.EAX);
                            return Reg.EAX;

                            //state.LEA(offset, Reg.EBP, Reg.ESI); // source address
                            //state.CGenExpandStackBy(Utils.RoundUp(type.SizeOf, 4));
                            //state.LEA(0, Reg.ESP, Reg.EDI); // destination address
                            //state.MOVL(type.SizeOf, Reg.ECX); // nbytes
                            //state.CGenMemCpy();
                            //return Reg.STACK;

                        case ExprType.Kind.VOID:
                            throw new InvalidProgramException("How could a variable be void?");
                            // %eax = $0
                            // state.MOVL(0, Reg.EAX);
                            // return Reg.EAX;

                        case ExprType.Kind.FUNCTION:
                            throw new InvalidProgramException("How could a variable be a function designator?");
                            // %eax = function_name
                            // state.MOVL(name, Reg.EAX);
                            // return Reg.EAX;

                        case ExprType.Kind.CHAR:
                            // %eax = [char -> long](off(%ebp))
                            state.MOVSBL(offset, Reg.EBP, Reg.EAX);
                            return Reg.EAX;

                        case ExprType.Kind.UCHAR:
                            // %eax = [uchar -> ulong](off(%ebp))
                            state.MOVZBL(offset, Reg.EBP, Reg.EAX);
                            return Reg.EAX;

                        case ExprType.Kind.SHORT:
                            // %eax = [short -> long](off(%ebp))
                            state.MOVSWL(offset, Reg.EBP, Reg.EAX);
                            return Reg.EAX;

                        case ExprType.Kind.USHORT:
                            // %eax = [ushort -> ulong](off(%ebp))
                            state.MOVZWL(offset, Reg.EBP, Reg.EAX);
                            return Reg.EAX;

                        case ExprType.Kind.ARRAY:
                            // %eax = (off(%ebp))
                            state.LEA(offset, Reg.EBP, Reg.EAX); // source address
                            return Reg.EAX;

                        default:
                            throw new InvalidOperationException($"Cannot get value of {this.Type.kind}");
                    }

                case Env.EntryKind.GLOBAL:
                    switch (this.Type.kind) {
                        case ExprType.Kind.CHAR:
                            state.MOVSBL(this.name, Reg.EAX);
                            return Reg.EAX;

                        case ExprType.Kind.UCHAR:
                            state.MOVZBL(this.name, Reg.EAX);
                            return Reg.EAX;

                        case ExprType.Kind.SHORT:
                            state.MOVSWL(this.name, Reg.EAX);
                            return Reg.EAX;

                        case ExprType.Kind.USHORT:
                            state.MOVZWL(this.name, Reg.EAX);
                            return Reg.EAX;

                        case ExprType.Kind.LONG:
                        case ExprType.Kind.ULONG:
                        case ExprType.Kind.POINTER:
                            state.MOVL(this.name, Reg.EAX);
                            return Reg.EAX;

                        case ExprType.Kind.FUNCTION:
                            state.MOVL("$" + this.name, Reg.EAX);
                            return Reg.EAX;

                        case ExprType.Kind.FLOAT:
                            state.FLDS(this.name);
                            return Reg.ST0;

                        case ExprType.Kind.DOUBLE:
                            state.FLDL(this.name);
                            return Reg.ST0;

                        case ExprType.Kind.STRUCT_OR_UNION:
                            state.MOVL($"${this.name}", Reg.EAX);
                            return Reg.EAX;

                            //state.LEA(name, Reg.ESI); // source address
                            //state.CGenExpandStackBy(Utils.RoundUp(type.SizeOf, 4));
                            //state.LEA(0, Reg.ESP, Reg.EDI); // destination address
                            //state.MOVL(type.SizeOf, Reg.ECX); // nbytes
                            //state.CGenMemCpy();
                            //return Reg.STACK;

                        case ExprType.Kind.VOID:
                            throw new InvalidProgramException("How could a variable be void?");
                            //state.MOVL(0, Reg.EAX);
                            //return Reg.EAX;

                        case ExprType.Kind.ARRAY:
                            state.MOVL($"${this.name}", Reg.EAX);
                            return Reg.EAX;

                        default:
                            throw new InvalidProgramException("cannot get the value of a " + this.Type.kind);
                    }

                case Env.EntryKind.TYPEDEF:
                default:
                    throw new InvalidProgramException("cannot get the value of a " + entry.kind);
            }
        }
    }

    public class AssignList : Expr {
        public AssignList(List<Expr> exprs, ExprType type)
            : base(type) {
            this.exprs = exprs;
        }
        public readonly List<Expr> exprs;
        public override Env Env => this.exprs.Last().Env;

        public override Reg CGenValue(Env env, CGenState state) {
            Reg reg = Reg.EAX;
            foreach (Expr expr in this.exprs) {
                reg = expr.CGenValue(env, state);
            }
            return reg;
        }
    }

    public class Assign : Expr {
        public Assign(Expr lvalue, Expr rvalue, ExprType type)
            : base(type) {
            this.lvalue = lvalue;
            this.rvalue = rvalue;
        }
        public readonly Expr lvalue;
        public readonly Expr rvalue;
        public override Env Env => this.rvalue.Env;

        public override Reg CGenValue(Env env, CGenState state) {

            // 1. %eax = &Left
            this.lvalue.CGenAddress(env, state);

            // 2. push %eax
            Int32 pos = state.CGenPushLong(Reg.EAX);

            Reg ret = this.rvalue.CGenValue(env, state);
            switch (this.lvalue.Type.kind) {
                case ExprType.Kind.CHAR:
                case ExprType.Kind.UCHAR:
                    // pop %ebx
                    // now %ebx = %Left
                    state.CGenPopLong(pos, Reg.EBX);

                    // *%ebx = %al
                    state.MOVB(Reg.AL, 0, Reg.EBX);

                    return Reg.EAX;

                case ExprType.Kind.SHORT:
                case ExprType.Kind.USHORT:
                    // pop %ebx
                    // now %ebx = %Left
                    state.CGenPopLong(pos, Reg.EBX);

                    // *%ebx = %al
                    state.MOVW(Reg.AX, 0, Reg.EBX);

                    return Reg.EAX;

                case ExprType.Kind.LONG:
                case ExprType.Kind.ULONG:
                case ExprType.Kind.POINTER:
                    // pop %ebx
                    // now %ebx = &Left
                    state.CGenPopLong(pos, Reg.EBX);

                    // *%ebx = %al
                    state.MOVL(Reg.EAX, 0, Reg.EBX);

                    return Reg.EAX;

                case ExprType.Kind.FLOAT:
                    // pop %ebx
                    // now %ebx = &Left
                    state.CGenPopLong(pos, Reg.EBX);

                    // *%ebx = %st(0)
                    state.FSTS(0, Reg.EBX);

                    return Reg.ST0;

                case ExprType.Kind.DOUBLE:
                    // pop %ebx
                    // now %ebx = &Left
                    state.CGenPopLong(pos, Reg.EBX);

                    // *%ebx = %st(0)
                    state.FSTL(0, Reg.EBX);

                    return Reg.ST0;

                case ExprType.Kind.STRUCT_OR_UNION:
                    // pop %edi
                    // now %edi = &Left
                    state.CGenPopLong(pos, Reg.EDI);

                    // %esi = &Right
                    state.MOVL(Reg.EAX, Reg.ESI);

                    // %ecx = nbytes
                    state.MOVL(this.lvalue.Type.SizeOf, Reg.ECX);

                    state.CGenMemCpy();

                    // %eax = &Left
                    state.MOVL(Reg.EDI, Reg.EAX);

                    return Reg.EAX;

                case ExprType.Kind.FUNCTION:
                case ExprType.Kind.VOID:
                case ExprType.Kind.ARRAY:
                case ExprType.Kind.INCOMPLETE_ARRAY:
                default:
                    throw new InvalidProgramException("cannot assign to a " + this.Type.kind);
            }
        }
    }

    public class ConditionalExpr : Expr {
        public ConditionalExpr(Expr cond, Expr true_expr, Expr false_expr, ExprType type)
            : base(type) {
            this.cond = cond;
            this.true_expr = true_expr;
            this.false_expr = false_expr;
        }
        public readonly Expr cond;
        public readonly Expr true_expr;
        public readonly Expr false_expr;
        public override Env Env => this.false_expr.Env;

        // 
        //          test cond
        //          jz false ---+
        //          true_expr   |
        // +------- jmp finish  |
        // |    false: <--------+
        // |        false_expr
        // +--> finish:
        // 
        public override Reg CGenValue(Env env, CGenState state) {
            Int32 stack_size = state.StackSize;
            Reg ret = this.cond.CGenValue(env, state);
            state.CGenForceStackSizeTo(stack_size);

            // test cond
            switch (ret) {
                case Reg.EAX:
                    state.TESTL(Reg.EAX, Reg.EAX);
                    break;

                case Reg.ST0:
                    /// Compare Expr with 0.0
                    /// < see cref = "BinaryArithmeticComp.OperateFloat(CGenState)" />
                    state.FLDZ();
                    state.FUCOMIP();
                    state.FSTP(Reg.ST0);
                    break;

                default:
                    throw new InvalidProgramException();
            }

            Int32 false_label = state.RequestLabel();
            Int32 finish_label = state.RequestLabel();

            state.JZ(false_label);

            this.true_expr.CGenValue(env, state);

            state.JMP(finish_label);

            state.CGenLabel(false_label);

            ret = this.false_expr.CGenValue(env, state);

            state.CGenLabel(finish_label);

            return ret;
        }
    }
        
    public class FuncCall : Expr {
        public FuncCall(Expr func, TFunction func_type, List<Expr> args)
            : base(func_type.ret_t) {
            this.func = func;
            this.func_type = func_type;
            this.args = args;
        }
        public readonly Expr func;
        public readonly TFunction func_type;
        public readonly IReadOnlyList<Expr> args;

        public override Env Env => this.args.Any() ? this.args.Last().Env : this.func.Env;

        public override void CGenAddress(Env env, CGenState state) {
            throw new Exception("Error: cannot get the address of a function call.");
        }

        public override Reg CGenValue(Env env, CGenState state) {

            // GCC's IA-32 calling convention
            // Caller is responsible to push all arguments to the stack in reverse order.
            // Each argument is at least aligned to 4 bytes - even a char would take 4 bytes.
            // The return value is stored in %eax, or %st(0), if it is a scalar.
            // 
            // The stack would look like this after pushing all the arguments:
            // +--------+
            // |  ....  |
            // +--------+
            // |  argn  |
            // +--------+
            // |  ....  |
            // +--------+
            // |  arg2  |
            // +--------+
            // |  arg1  |
            // +--------+ <- %esp before call
            //
            // Things are different with structs and unions.
            // Since structs may not fit in 4 bytes, it has to be returned in memory.
            // Caller allocates a chunk of memory for the struct and push the address of it as an extra argument.
            // Callee returns %eax with that address.
            // 
            // The stack would look like this after pushing all the arguments:
            //      +--------+
            // +--> | struct | <- struct should be returned here.
            // |    +--------+
            // |    |  argn  |
            // |    +--------+
            // |    |  ....  |
            // |    +--------+
            // |    |  arg2  |
            // |    +--------+
            // |    |  arg1  |
            // |    +--------+
            // +----|  addr  | <- %esp before call
            //      +--------+
            // 

            state.NEWLINE();
            state.COMMENT($"Before pushing the arguments, stack size = {state.StackSize}.");

            var r_pack = Utils.PackArguments(this.args.Select(_ => _.Type).ToList());
            Int32 pack_size = r_pack.Item1;
            IReadOnlyList<Int32> offsets = r_pack.Item2;

            if (this.Type is TStructOrUnion) {
                // If the function returns a struct

                // Allocate space for return value.
                state.COMMENT("Allocate space for returning stack.");
                state.CGenExpandStackWithAlignment(this.Type.SizeOf, this.Type.Alignment);

                // Temporarily store the address in %eax.
                state.MOVL(Reg.ESP, Reg.EAX);

                // add an extra argument and move all other arguments upwards.
                pack_size += ExprType.SIZEOF_POINTER;
                offsets = offsets.Select(_ => _ + ExprType.SIZEOF_POINTER).ToList();
            }

            // Allocate space for arguments.
            // If returning struct, the extra pointer is included.
            state.COMMENT($"Arguments take {pack_size} bytes.");
            state.CGenExpandStackBy(pack_size);
            state.NEWLINE();

            // Store the address as the first argument.
            if (this.Type is TStructOrUnion) {
                state.COMMENT("Putting extra argument for struct return address.");
                state.MOVL(Reg.EAX, 0, Reg.ESP);
                state.NEWLINE();
            }

            // This is the stack size before calling the function.
            Int32 header_base = -state.StackSize;

            // Push the arguments onto the stack in reverse order
            for (Int32 i = this.args.Count; i-- > 0;) {
                Expr arg = this.args[i];
                Int32 pos = header_base + offsets[i];

                state.COMMENT($"Argument {i} is at {pos}");

                Reg ret = arg.CGenValue(env, state);
                switch (arg.Type.kind) {
                    case ExprType.Kind.ARRAY:
                    case ExprType.Kind.CHAR:
                    case ExprType.Kind.UCHAR:
                    case ExprType.Kind.SHORT:
                    case ExprType.Kind.USHORT:
                    case ExprType.Kind.LONG:
                    case ExprType.Kind.ULONG:
                    case ExprType.Kind.POINTER:
                        if (ret != Reg.EAX) {
                            throw new InvalidProgramException();
                        }
                        state.MOVL(Reg.EAX, pos, Reg.EBP);
                        break;

                    case ExprType.Kind.DOUBLE:
                        if (ret != Reg.ST0) {
                            throw new InvalidProgramException();
                        }
                        state.FSTPL(pos, Reg.EBP);
                        break;

                    case ExprType.Kind.FLOAT:
                        if (ret != Reg.ST0) {
                            throw new InvalidProgramException();
                        }
                        state.FSTPL(pos, Reg.EBP);
                        break;

                    case ExprType.Kind.STRUCT_OR_UNION:
                        if (ret != Reg.EAX) {
                            throw new InvalidProgramException();
                        }
                        state.MOVL(Reg.EAX, Reg.ESI);
                        state.LEA(pos, Reg.EBP, Reg.EDI);
                        state.MOVL(arg.Type.SizeOf, Reg.ECX);
                        state.CGenMemCpy();
                        break;

                    default:
                        throw new InvalidProgramException();
                }

                state.NEWLINE();

            }

            // When evaluating arguments, the stack might be changed.
            // We must restore the stack.
            state.CGenForceStackSizeTo(-header_base);

            // Get function address
            if (this.func.Type is TFunction) {
                this.func.CGenAddress(env, state);
            } else if (this.func.Type is TPointer) {
                this.func.CGenValue(env, state);
            } else {
                throw new InvalidProgramException();
            }

            state.CALL("*%eax");

            state.COMMENT("Function returned.");
            state.NEWLINE();

            if (this.Type.kind == ExprType.Kind.FLOAT || this.Type.kind == ExprType.Kind.DOUBLE) {
                return Reg.ST0;
            }
            return Reg.EAX;
        }
    }

    /// <summary>
    /// Expr.name: Expr must be a struct or union.
    /// </summary>
    public class Attribute : Expr {
        public Attribute(Expr expr, String name, ExprType type)
            : base(type) {
            this.expr = expr;
            this.name = name;
        }
        public readonly Expr expr;
        public readonly String name;
        public override Env Env => this.expr.Env;

        public override Reg CGenValue(Env env, CGenState state) {

            // %eax is the address of the struct/union
            if (this.expr.CGenValue(env, state) != Reg.EAX) {
                throw new InvalidProgramException();
            }

            if (this.expr.Type.kind != ExprType.Kind.STRUCT_OR_UNION) {
                throw new InvalidProgramException();
            }

            // size of the struct or union
            Int32 struct_size = this.expr.Type.SizeOf;

            // offset inside the pack
            Int32 attrib_offset = ((TStructOrUnion) this.expr.Type)
                        .Attribs
                        .First(_ => _.name == this.name)
                        .offset;

            // can't be a function designator.
            switch (this.Type.kind) {
                case ExprType.Kind.ARRAY:
                case ExprType.Kind.STRUCT_OR_UNION:
                    state.ADDL(attrib_offset, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.CHAR:
                    state.MOVSBL(attrib_offset, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.UCHAR:
                    state.MOVZBL(attrib_offset, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.SHORT:
                    state.MOVSWL(attrib_offset, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.USHORT:
                    state.MOVZWL(attrib_offset, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.LONG:
                case ExprType.Kind.ULONG:
                case ExprType.Kind.POINTER:
                    state.MOVL(attrib_offset, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.FLOAT:
                    state.FLDS(attrib_offset, Reg.EAX);
                    return Reg.ST0;

                case ExprType.Kind.DOUBLE:
                    state.FLDL(attrib_offset, Reg.EAX);
                    return Reg.ST0;

                default:
                    throw new InvalidProgramException();
            }
        }

        public override void CGenAddress(Env env, CGenState state) {
            if (this.expr.Type.kind != ExprType.Kind.STRUCT_OR_UNION) {
                throw new InvalidProgramException();
            }

            // %eax = address of struct or union
            this.expr.CGenAddress(env, state);

            // offset inside the pack
            Int32 offset = ((TStructOrUnion) this.expr.Type)
                        .Attribs
                        .First(_ => _.name == this.name)
                        .offset;

            state.ADDL(offset, Reg.EAX);
        }
    }

    /// <summary>
    /// &Expr: get the address of Expr.
    /// </summary>
    public class Reference : Expr {
        public Reference(Expr expr)
            : base(new TPointer(expr.Type)) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public override Env Env => this.expr.Env;

        public override Reg CGenValue(Env env, CGenState state) {
            this.expr.CGenAddress(env, state);
            return Reg.EAX;
        }
    }

    /// <summary>
    /// *Expr: Expr must be a pointer.
    /// 
    /// Arrays and functions are implicitly converted to pointers.
    /// 
    /// This is an lvalue, so it has an address.
    /// </summary>
    public class Dereference : Expr {
        public Dereference(Expr expr, ExprType type)
            : base(type) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public override Env Env => this.expr.Env;

        public override Reg CGenValue(Env env, CGenState state) {
            Reg ret = this.expr.CGenValue(env, state);
            if (ret != Reg.EAX) {
                throw new InvalidProgramException();
            }
            if (this.expr.Type.kind != ExprType.Kind.POINTER) {
                throw new InvalidProgramException();
            }

            ExprType type = ((TPointer) this.expr.Type).ref_t;
            switch (type.kind) {
                case ExprType.Kind.ARRAY:
                case ExprType.Kind.FUNCTION:
                    return Reg.EAX;

                case ExprType.Kind.CHAR:
                    state.MOVSBL(0, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.UCHAR:
                    state.MOVZBL(0, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.SHORT:
                    state.MOVSWL(0, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.USHORT:
                    state.MOVZWL(0, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.LONG:
                case ExprType.Kind.ULONG:
                case ExprType.Kind.POINTER:
                    state.MOVL(0, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.FLOAT:
                    state.FLDS(0, Reg.EAX);
                    return Reg.ST0;

                case ExprType.Kind.DOUBLE:
                    state.FLDL(0, Reg.EAX);
                    return Reg.ST0;

                case ExprType.Kind.STRUCT_OR_UNION:
                    //// %esi = src address
                    //state.MOVL(Reg.EAX, Reg.ESI);

                    //// %edi = dst address
                    //state.CGenExpandStackBy(Utils.RoundUp(type.SizeOf, 4));
                    //state.LEA(0, Reg.ESP, Reg.EDI);

                    //// %ecx = nbytes
                    //state.MOVL(type.SizeOf, Reg.ECX);

                    //state.CGenMemCpy();

                    //return Reg.STACK;
                    return Reg.EAX;

                case ExprType.Kind.VOID:
                default:
                    throw new InvalidProgramException();
            }
        }

        public override void CGenAddress(Env env, CGenState state) {
            Reg ret = this.expr.CGenValue(env, state);
            if (ret != Reg.EAX) {
                throw new InvalidProgramException();
            }
        }
    }

}