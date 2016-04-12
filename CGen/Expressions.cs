using System;
using System.Collections.Generic;
using System.Linq;
using CodeGeneration;

namespace ABT {
    public abstract partial class Expr {
        public abstract Reg CGenValue(CGenState state);

        public abstract void CGenAddress(CGenState state);
    }

    public sealed partial class Variable {
        public override void CGenAddress(CGenState state) {
            Env.Entry entry = this.Env.Find(this.Name).Value;
            Int32 offset = entry.Offset;

            switch (entry.Kind) {
                case Env.EntryKind.FRAME:
                case Env.EntryKind.STACK:
                    state.LEA(offset, Reg.EBP, Reg.EAX);
                    return;

                case Env.EntryKind.GLOBAL:
                    state.LEA(this.Name, Reg.EAX);
                    return;

                case Env.EntryKind.ENUM:
                case Env.EntryKind.TYPEDEF:
                default:
                    throw new InvalidProgramException("cannot get the address of " + entry.Kind);
            }
        }

        public override Reg CGenValue(CGenState state) {
            Env.Entry entry = this.Env.Find(this.Name).Value;

            Int32 offset = entry.Offset;
            //if (entry.Kind == Env.EntryKind.STACK) {
            //    offset = -offset;
            //}

            switch (entry.Kind) {
                case Env.EntryKind.ENUM:
                    // 1. If the variable is an enum constant,
                    //    return the Value in %eax.
                    state.MOVL(offset, Reg.EAX);
                    return Reg.EAX;

                case Env.EntryKind.FRAME:
                case Env.EntryKind.STACK:
                    // 2. If the variable is a function argument or a local variable,
                    //    the address would be offset(%ebp).
                    switch (this.Type.Kind) {
                        case ExprTypeKind.LONG:
                        case ExprTypeKind.ULONG:
                        case ExprTypeKind.POINTER:
                            // %eax = offset(%ebp)
                            state.MOVL(offset, Reg.EBP, Reg.EAX);
                            return Reg.EAX;

                        case ExprTypeKind.FLOAT:
                            // %st(0) = offset(%ebp)
                            state.FLDS(offset, Reg.EBP);
                            return Reg.ST0;

                        case ExprTypeKind.DOUBLE:
                            // %st(0) = offset(%ebp)
                            state.FLDL(offset, Reg.EBP);
                            return Reg.ST0;

                        case ExprTypeKind.STRUCT_OR_UNION:
                            // %eax = address
                            state.LEA(offset, Reg.EBP, Reg.EAX);
                            return Reg.EAX;

                        //state.LEA(offset, Reg.EBP, Reg.ESI); // source address
                        //state.CGenExpandStackBy(Utils.RoundUp(Type.SizeOf, 4));
                        //state.LEA(0, Reg.ESP, Reg.EDI); // destination address
                        //state.MOVL(Type.SizeOf, Reg.ECX); // nbytes
                        //state.CGenMemCpy();
                        //return Reg.STACK;

                        case ExprTypeKind.VOID:
                            throw new InvalidProgramException("How could a variable be void?");
                        // %eax = $0
                        // state.MOVL(0, Reg.EAX);
                        // return Reg.EAX;

                        case ExprTypeKind.FUNCTION:
                            throw new InvalidProgramException("How could a variable be a function designator?");
                        // %eax = function_name
                        // state.MOVL(name, Reg.EAX);
                        // return Reg.EAX;

                        case ExprTypeKind.CHAR:
                            // %eax = [char -> long](off(%ebp))
                            state.MOVSBL(offset, Reg.EBP, Reg.EAX);
                            return Reg.EAX;

                        case ExprTypeKind.UCHAR:
                            // %eax = [uchar -> ulong](off(%ebp))
                            state.MOVZBL(offset, Reg.EBP, Reg.EAX);
                            return Reg.EAX;

                        case ExprTypeKind.SHORT:
                            // %eax = [short -> long](off(%ebp))
                            state.MOVSWL(offset, Reg.EBP, Reg.EAX);
                            return Reg.EAX;

                        case ExprTypeKind.USHORT:
                            // %eax = [ushort -> ulong](off(%ebp))
                            state.MOVZWL(offset, Reg.EBP, Reg.EAX);
                            return Reg.EAX;

                        case ExprTypeKind.ARRAY:
                            // %eax = (off(%ebp))
                            state.LEA(offset, Reg.EBP, Reg.EAX); // source address
                            return Reg.EAX;

                        default:
                            throw new InvalidOperationException($"Cannot get value of {this.Type.Kind}");
                    }

                case Env.EntryKind.GLOBAL:
                    switch (this.Type.Kind) {
                        case ExprTypeKind.CHAR:
                            state.MOVSBL(this.Name, Reg.EAX);
                            return Reg.EAX;

                        case ExprTypeKind.UCHAR:
                            state.MOVZBL(this.Name, Reg.EAX);
                            return Reg.EAX;

                        case ExprTypeKind.SHORT:
                            state.MOVSWL(this.Name, Reg.EAX);
                            return Reg.EAX;

                        case ExprTypeKind.USHORT:
                            state.MOVZWL(this.Name, Reg.EAX);
                            return Reg.EAX;

                        case ExprTypeKind.LONG:
                        case ExprTypeKind.ULONG:
                        case ExprTypeKind.POINTER:
                            state.MOVL(this.Name, Reg.EAX);
                            return Reg.EAX;

                        case ExprTypeKind.FUNCTION:
                            state.MOVL("$" + this.Name, Reg.EAX);
                            return Reg.EAX;

                        case ExprTypeKind.FLOAT:
                            state.FLDS(this.Name);
                            return Reg.ST0;

                        case ExprTypeKind.DOUBLE:
                            state.FLDL(this.Name);
                            return Reg.ST0;

                        case ExprTypeKind.STRUCT_OR_UNION:
                            state.MOVL($"${this.Name}", Reg.EAX);
                            return Reg.EAX;

                        //state.LEA(name, Reg.ESI); // source address
                        //state.CGenExpandStackBy(Utils.RoundUp(Type.SizeOf, 4));
                        //state.LEA(0, Reg.ESP, Reg.EDI); // destination address
                        //state.MOVL(Type.SizeOf, Reg.ECX); // nbytes
                        //state.CGenMemCpy();
                        //return Reg.STACK;

                        case ExprTypeKind.VOID:
                            throw new InvalidProgramException("How could a variable be void?");
                        //state.MOVL(0, Reg.EAX);
                        //return Reg.EAX;

                        case ExprTypeKind.ARRAY:
                            state.MOVL($"${this.Name}", Reg.EAX);
                            return Reg.EAX;

                        default:
                            throw new InvalidProgramException("cannot get the Value of a " + this.Type.Kind);
                    }

                case Env.EntryKind.TYPEDEF:
                default:
                    throw new InvalidProgramException("cannot get the Value of a " + entry.Kind);
            }
        }
    }

    public sealed partial class AssignList {
        public override Reg CGenValue(CGenState state) {
            Reg reg = Reg.EAX;
            foreach (Expr expr in this.Exprs) {
                reg = expr.CGenValue(state);
            }
            return reg;
        }

        public override void CGenAddress(CGenState state) {
            throw new InvalidOperationException("Cannot get the address of an assignment list.");
        }
    }

    public sealed partial class Assign {
        public override Reg CGenValue(CGenState state) {
            // 1. %eax = &left
            this.Left.CGenAddress(state);

            // 2. push %eax
            Int32 pos = state.CGenPushLong(Reg.EAX);

            Reg ret = this.Right.CGenValue(state);
            switch (this.Left.Type.Kind) {
                case ExprTypeKind.CHAR:
                case ExprTypeKind.UCHAR:
                    // pop %ebx
                    // now %ebx = %Left
                    state.CGenPopLong(pos, Reg.EBX);

                    // *%ebx = %al
                    state.MOVB(Reg.AL, 0, Reg.EBX);

                    return Reg.EAX;

                case ExprTypeKind.SHORT:
                case ExprTypeKind.USHORT:
                    // pop %ebx
                    // now %ebx = %Left
                    state.CGenPopLong(pos, Reg.EBX);

                    // *%ebx = %al
                    state.MOVW(Reg.AX, 0, Reg.EBX);

                    return Reg.EAX;

                case ExprTypeKind.LONG:
                case ExprTypeKind.ULONG:
                case ExprTypeKind.POINTER:
                    // pop %ebx
                    // now %ebx = &Left
                    state.CGenPopLong(pos, Reg.EBX);

                    // *%ebx = %al
                    state.MOVL(Reg.EAX, 0, Reg.EBX);

                    return Reg.EAX;

                case ExprTypeKind.FLOAT:
                    // pop %ebx
                    // now %ebx = &Left
                    state.CGenPopLong(pos, Reg.EBX);

                    // *%ebx = %st(0)
                    state.FSTS(0, Reg.EBX);

                    return Reg.ST0;

                case ExprTypeKind.DOUBLE:
                    // pop %ebx
                    // now %ebx = &Left
                    state.CGenPopLong(pos, Reg.EBX);

                    // *%ebx = %st(0)
                    state.FSTL(0, Reg.EBX);

                    return Reg.ST0;

                case ExprTypeKind.STRUCT_OR_UNION:
                    // pop %edi
                    // now %edi = &Left
                    state.CGenPopLong(pos, Reg.EDI);

                    // %esi = &Right
                    state.MOVL(Reg.EAX, Reg.ESI);

                    // %ecx = nbytes
                    state.MOVL(this.Left.Type.SizeOf, Reg.ECX);

                    state.CGenMemCpy();

                    // %eax = &Left
                    state.MOVL(Reg.EDI, Reg.EAX);

                    return Reg.EAX;

                case ExprTypeKind.FUNCTION:
                case ExprTypeKind.VOID:
                case ExprTypeKind.ARRAY:
                case ExprTypeKind.INCOMPLETE_ARRAY:
                default:
                    throw new InvalidProgramException("cannot assign to a " + this.Type.Kind);
            }
        }
    
        public override void CGenAddress(CGenState state) {
            throw new InvalidOperationException("Cannot get the address of an assignment expression.");
        }
    }

    public sealed partial class ConditionalExpr {
        // 
        //          test Cond
        //          jz false ---+
        //          true_expr   |
        // +------- jmp finish  |
        // |    false: <--------+
        // |        false_expr
        // +--> finish:
        // 
        public override Reg CGenValue(CGenState state) {
            Int32 stack_size = state.StackSize;
            Reg ret = this.Cond.CGenValue(state);
            state.CGenForceStackSizeTo(stack_size);

            // test Cond
            switch (ret) {
                case Reg.EAX:
                    state.TESTL(Reg.EAX, Reg.EAX);
                    break;

                case Reg.ST0:
                    /// Compare Expr with 0.0
                    /// < see cref = "BinaryComparisonOp.OperateFloat(CGenState)" />
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

            this.TrueExpr.CGenValue(state);

            state.JMP(finish_label);

            state.CGenLabel(false_label);

            ret = this.FalseExpr.CGenValue(state);

            state.CGenLabel(finish_label);

            return ret;
        }

        public override void CGenAddress(CGenState state) {
            throw new InvalidOperationException("Cannot get the address of a conditional expression.");
        }
    }

    public sealed partial class FuncCall {
        public override void CGenAddress(CGenState state) {
            throw new InvalidOperationException("Error: cannot get the address of a function call.");
        }

        public override Reg CGenValue(CGenState state) {

            // GCC's IA-32 calling convention
            // Caller is responsible to push all arguments to the stack in reverse order.
            // Each argument is at least aligned to 4 bytes - even a char would take 4 bytes.
            // The return Value is stored in %eax, or %st(0), if it is a scalar.
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

            var r_pack = Utils.PackArguments(this.Args.Select(_ => _.Type).ToList());
            Int32 pack_size = r_pack.Item1;
            IReadOnlyList<Int32> offsets = r_pack.Item2;

            if (this.Type is StructOrUnionType) {
                // If the function returns a struct

                // Allocate space for return Value.
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
            if (this.Type is StructOrUnionType) {
                state.COMMENT("Putting extra argument for struct return address.");
                state.MOVL(Reg.EAX, 0, Reg.ESP);
                state.NEWLINE();
            }

            // This is the stack size before calling the function.
            Int32 header_base = -state.StackSize;

            // Push the arguments onto the stack in reverse order
            for (Int32 i = this.Args.Count; i-- > 0;) {
                Expr arg = this.Args[i];
                Int32 pos = header_base + offsets[i];

                state.COMMENT($"Argument {i} is at {pos}");

                Reg ret = arg.CGenValue(state);
                switch (arg.Type.Kind) {
                    case ExprTypeKind.ARRAY:
                    case ExprTypeKind.CHAR:
                    case ExprTypeKind.UCHAR:
                    case ExprTypeKind.SHORT:
                    case ExprTypeKind.USHORT:
                    case ExprTypeKind.LONG:
                    case ExprTypeKind.ULONG:
                    case ExprTypeKind.POINTER:
                        if (ret != Reg.EAX) {
                            throw new InvalidProgramException();
                        }
                        state.MOVL(Reg.EAX, pos, Reg.EBP);
                        break;

                    case ExprTypeKind.DOUBLE:
                        if (ret != Reg.ST0) {
                            throw new InvalidProgramException();
                        }
                        state.FSTPL(pos, Reg.EBP);
                        break;

                    case ExprTypeKind.FLOAT:
                        if (ret != Reg.ST0) {
                            throw new InvalidProgramException();
                        }
                        state.FSTPL(pos, Reg.EBP);
                        break;

                    case ExprTypeKind.STRUCT_OR_UNION:
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
            if (this.Func.Type is FunctionType) {
                this.Func.CGenAddress(state);
            } else if (this.Func.Type is PointerType) {
                this.Func.CGenValue(state);
            } else {
                throw new InvalidProgramException();
            }

            state.CALL("*%eax");

            state.COMMENT("Function returned.");
            state.NEWLINE();

            if (this.Type.Kind == ExprTypeKind.FLOAT || this.Type.Kind == ExprTypeKind.DOUBLE) {
                return Reg.ST0;
            }
            return Reg.EAX;
        }
    }

    public sealed partial class Attribute {
        public override Reg CGenValue(CGenState state) {

            // %eax is the address of the struct/union
            if (this.Expr.CGenValue(state) != Reg.EAX) {
                throw new InvalidProgramException();
            }

            if (this.Expr.Type.Kind != ExprTypeKind.STRUCT_OR_UNION) {
                throw new InvalidProgramException();
            }

            // size of the struct or union
            Int32 struct_size = this.Expr.Type.SizeOf;

            // offset inside the pack
            Int32 attrib_offset = ((StructOrUnionType)this.Expr.Type)
                        .Attribs
                        .First(_ => _.name == this.Name)
                        .offset;

            // can't be a function designator.
            switch (this.Type.Kind) {
                case ExprTypeKind.ARRAY:
                case ExprTypeKind.STRUCT_OR_UNION:
                    state.ADDL(attrib_offset, Reg.EAX);
                    return Reg.EAX;

                case ExprTypeKind.CHAR:
                    state.MOVSBL(attrib_offset, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprTypeKind.UCHAR:
                    state.MOVZBL(attrib_offset, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprTypeKind.SHORT:
                    state.MOVSWL(attrib_offset, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprTypeKind.USHORT:
                    state.MOVZWL(attrib_offset, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprTypeKind.LONG:
                case ExprTypeKind.ULONG:
                case ExprTypeKind.POINTER:
                    state.MOVL(attrib_offset, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprTypeKind.FLOAT:
                    state.FLDS(attrib_offset, Reg.EAX);
                    return Reg.ST0;

                case ExprTypeKind.DOUBLE:
                    state.FLDL(attrib_offset, Reg.EAX);
                    return Reg.ST0;

                default:
                    throw new InvalidProgramException();
            }
        }

        public override void CGenAddress(CGenState state) {
            if (this.Expr.Type.Kind != ExprTypeKind.STRUCT_OR_UNION) {
                throw new InvalidProgramException();
            }

            // %eax = address of struct or union
            this.Expr.CGenAddress(state);

            // offset inside the pack
            Int32 offset = ((StructOrUnionType)this.Expr.Type)
                        .Attribs
                        .First(_ => _.name == this.Name)
                        .offset;

            state.ADDL(offset, Reg.EAX);
        }
    }

    public sealed partial class Reference {
        public override Reg CGenValue(CGenState state) {
            this.Expr.CGenAddress(state);
            return Reg.EAX;
        }

        public override void CGenAddress(CGenState state) {
            throw new InvalidOperationException("Cannot get the address of a pointer value.");
        }
    }

    public sealed partial class Dereference {
        public override Reg CGenValue(CGenState state) {
            Reg ret = this.Expr.CGenValue(state);
            if (ret != Reg.EAX) {
                throw new InvalidProgramException();
            }
            if (this.Expr.Type.Kind != ExprTypeKind.POINTER) {
                throw new InvalidProgramException();
            }

            ExprType type = ((PointerType)this.Expr.Type).RefType;
            switch (type.Kind) {
                case ExprTypeKind.ARRAY:
                case ExprTypeKind.FUNCTION:
                    return Reg.EAX;

                case ExprTypeKind.CHAR:
                    state.MOVSBL(0, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprTypeKind.UCHAR:
                    state.MOVZBL(0, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprTypeKind.SHORT:
                    state.MOVSWL(0, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprTypeKind.USHORT:
                    state.MOVZWL(0, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprTypeKind.LONG:
                case ExprTypeKind.ULONG:
                case ExprTypeKind.POINTER:
                    state.MOVL(0, Reg.EAX, Reg.EAX);
                    return Reg.EAX;

                case ExprTypeKind.FLOAT:
                    state.FLDS(0, Reg.EAX);
                    return Reg.ST0;

                case ExprTypeKind.DOUBLE:
                    state.FLDL(0, Reg.EAX);
                    return Reg.ST0;

                case ExprTypeKind.STRUCT_OR_UNION:
                    //// %esi = src address
                    //state.MOVL(Reg.EAX, Reg.ESI);

                    //// %edi = dst address
                    //state.CGenExpandStackBy(Utils.RoundUp(Type.SizeOf, 4));
                    //state.LEA(0, Reg.ESP, Reg.EDI);

                    //// %ecx = nbytes
                    //state.MOVL(Type.SizeOf, Reg.ECX);

                    //state.CGenMemCpy();

                    //return Reg.STACK;
                    return Reg.EAX;

                case ExprTypeKind.VOID:
                default:
                    throw new InvalidProgramException();
            }
        }

        public override void CGenAddress(CGenState state) {
            Reg ret = this.Expr.CGenValue(state);
            if (ret != Reg.EAX) {
                throw new InvalidProgramException();
            }
        }
    }
}
