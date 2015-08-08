using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    /// 6. stack must be aligned to 4 bytes (< gcc 4.5, for gcc 4.5+, aligned to 16 bytes).
    /// </summary>

    public abstract class Expr {
        public Expr(ExprType _type) {
            type = _type;
        }
        public virtual Boolean IsConstExpr() {
            return false;
        }
        public virtual Reg CGenValue(Env env, CGenState state) {
            throw new NotImplementedException();
        }

        public virtual void CGenAddress(Env env, CGenState state) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The default implementation of CGenPush uses CGenValue.
        /// </summary>
        // TODO: struct and union
        public virtual void CGenPush(Env env, CGenState state) {
            Reg ret = CGenValue(env, state);

            switch (type.kind) {
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
                state.PUSHL(Reg.EAX);
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
                state.PUSHL(Reg.EAX);
                break;

            case ExprType.Kind.ERROR:
            case ExprType.Kind.INCOMPLETE_ARRAY:
            case ExprType.Kind.VOID:
                throw new InvalidProgramException(type.kind.ToString() + " can't be pushed onto the stack");

            case ExprType.Kind.STRUCT_OR_UNION:
                throw new NotImplementedException();
            }

        }

        public readonly ExprType type;
    }

    public class EmptyExpr : Expr {
        public EmptyExpr() : base(new TVoid()) {
        }
        public override Reg CGenValue(Env env, CGenState state) {
            state.MOVL(0, Reg.EAX);
            return Reg.EAX;
        }
        public override void CGenAddress(Env env, CGenState state) {
            state.MOVL(0, Reg.EAX);
        }
    }

    public class Variable : Expr {
        public Variable(ExprType _type, String _name)
            : base(_type) {
            name = _name;
        }
        public readonly String name;

        public override void CGenAddress(Env env, CGenState state) {
            Env.Entry entry = env.Find(name);
            switch (entry.kind) {
            case Env.EntryKind.FRAME:
                break;
            case Env.EntryKind.STACK:
                break;
            case Env.EntryKind.GLOBAL:
                switch (entry.type.kind) {
                case ExprType.Kind.FUNCTION:
					state.LEA(name, Reg.EAX);
                    break;

                case ExprType.Kind.CHAR:
                case ExprType.Kind.DOUBLE:
                case ExprType.Kind.ERROR:
                case ExprType.Kind.FLOAT:

                case ExprType.Kind.LONG:
                case ExprType.Kind.POINTER:
                case ExprType.Kind.SHORT:
                case ExprType.Kind.STRUCT_OR_UNION:
                case ExprType.Kind.UCHAR:
                case ExprType.Kind.ULONG:
                case ExprType.Kind.USHORT:
                case ExprType.Kind.VOID:
                default:
                    throw new NotImplementedException();
                }
                break;
            case Env.EntryKind.ENUM:
            case Env.EntryKind.NOT_FOUND:
            case Env.EntryKind.TYPEDEF:
            default:
                throw new InvalidProgramException("cannot get the address of " + entry.kind);
            }
        }

        public override Reg CGenValue(Env env, CGenState state) {
            Env.Entry entry = env.Find(name);

            Int32 offset = entry.offset;
            if (entry.kind == Env.EntryKind.STACK) {
                offset = -offset;
            }

            switch (entry.kind) {
            case Env.EntryKind.ENUM:
                // enum constant : just an integer
                state.MOVL(offset, Reg.EAX);
                return Reg.EAX;

            case Env.EntryKind.FRAME:
            case Env.EntryKind.STACK:
                switch (type.kind) {
                case ExprType.Kind.LONG:
                case ExprType.Kind.ULONG:
                case ExprType.Kind.POINTER:
                    state.MOVL(offset, Reg.EBP, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.FLOAT:
                    state.FLDS(offset, Reg.EBP);
                    return Reg.ST0;

                case ExprType.Kind.DOUBLE:
                    state.FLDL(offset, Reg.EBP);
                    return Reg.ST0;

				case ExprType.Kind.STRUCT_OR_UNION:
					state.LEA(offset, Reg.ESP, Reg.ESI); // source address
					state.CGenExpandStackBy(Utils.RoundUp(type.SizeOf, 4));
					state.LEA(0, Reg.ESP, Reg.EDI); // destination address
					state.MOVL(type.SizeOf, Reg.ECX); // nbytes
					state.MemCpy();
					return Reg.STACK;

                case ExprType.Kind.VOID:
                    state.MOVL(0, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.FUNCTION:
                    state.MOVL(name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.CHAR:
                    state.MOVSBL(offset, Reg.EBP, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.UCHAR:
                    state.MOVZBL(offset, Reg.EBP, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.SHORT:
                    state.MOVSWL(offset, Reg.EBP, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.USHORT:
                    state.MOVZWL(offset, Reg.EBP, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.ERROR:
                default:
                    throw new InvalidProgramException("cannot get the value of a " + type.kind.ToString());
                }

            case Env.EntryKind.GLOBAL:
                switch (type.kind) {
                case ExprType.Kind.CHAR:
                    state.MOVSBL(name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.UCHAR:
                    state.MOVZBL(name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.SHORT:
                    state.MOVSWL(name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.USHORT:
                    state.MOVZWL(name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.LONG:
                case ExprType.Kind.ULONG:
                case ExprType.Kind.POINTER:
                    state.MOVL(name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.FUNCTION:
                    state.MOVL("$" + name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.FLOAT:
                    state.FLDS(name);
                    return Reg.ST0;

                case ExprType.Kind.DOUBLE:
                    state.FLDL(name);
                    return Reg.ST0;

                case ExprType.Kind.STRUCT_OR_UNION:
					state.LEA(name, Reg.ESI); // source address
					state.CGenExpandStackBy(Utils.RoundUp(type.SizeOf, 4));
					state.LEA(0, Reg.ESP, Reg.EDI); // destination address
					state.MOVL(type.SizeOf, Reg.ECX); // nbytes
					state.MemCpy();
					return Reg.STACK;

                case ExprType.Kind.VOID:
                    state.MOVL(0, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.ARRAY:
                    state.MOVL("$" + name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.ERROR:
                case ExprType.Kind.INCOMPLETE_ARRAY:
                default:
                    throw new InvalidProgramException("cannot get the value of a " + type.kind.ToString());
                }

            case Env.EntryKind.TYPEDEF:
            case Env.EntryKind.NOT_FOUND:
            default:
                throw new InvalidProgramException("cannot get the value of a " + entry.kind.ToString());
            }
        }
    }

    public class AssignmentList : Expr {
        public AssignmentList(List<Expr> _exprs, ExprType _type)
            : base(_type) {
            exprs = _exprs;
        }
        public readonly List<Expr> exprs;

        public override Reg CGenValue(Env env, CGenState state) {
            Reg reg = Reg.EAX;
            foreach (Expr expr in exprs) {
                reg = expr.CGenValue(env, state);
            }
            return reg;
        }
    }

    public class Assignment : Expr {
        public Assignment(Expr _lvalue, Expr _rvalue, ExprType _type)
            : base(_type) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
        public readonly Expr lvalue;
        public readonly Expr rvalue;

        // TODO: struct and union
        public override Reg CGenValue(Env env, CGenState state) {

            // 1. %eax = &lhs
            lvalue.CGenAddress(env, state);

            // 2. push %eax
            state.PUSHL(Reg.EAX);

            Reg ret = rvalue.CGenValue(env, state);
            switch (lvalue.type.kind) {
            case ExprType.Kind.CHAR:
            case ExprType.Kind.UCHAR:
                // pop %ebx
                // now %ebx = %lhs
                state.POPL(Reg.EBX);

                // *%ebx = %al
                state.MOVB(Reg.AL, 0, Reg.EBX);

                return Reg.EAX;

            case ExprType.Kind.SHORT:
            case ExprType.Kind.USHORT:
                // pop %ebx
                // now %ebx = %lhs
                state.POPL(Reg.EBX);

                // *%ebx = %al
                state.MOVW(Reg.AX, 0, Reg.EBX);

                return Reg.EAX;

            case ExprType.Kind.LONG:
            case ExprType.Kind.ULONG:
            case ExprType.Kind.POINTER:
                // pop %ebx
                // now %ebx = %lhs
                state.POPL(Reg.EBX);

                // *%ebx = %al
                state.MOVL(Reg.EAX, 0, Reg.EBX);

                return Reg.EAX;

            case ExprType.Kind.FLOAT:
                // pop %ebx
                // now %ebx = %lhs
                state.POPL(Reg.EBX);

                // *%ebx = %st(0)
                state.FSTS(0, Reg.EBX);

                return Reg.ST0;

            case ExprType.Kind.DOUBLE:
                // pop %ebx
                // now %ebx = %lhs
                state.POPL(Reg.EBX);

                // *%ebx = %st(0)
                state.FSTL(0, Reg.EBX);

                return Reg.ST0;

            case ExprType.Kind.STRUCT_OR_UNION:
                throw new NotImplementedException();

            case ExprType.Kind.FUNCTION:
            case ExprType.Kind.VOID:
            case ExprType.Kind.ARRAY:
            case ExprType.Kind.ERROR:
            case ExprType.Kind.INCOMPLETE_ARRAY:
            default:
                throw new InvalidProgramException("cannot assign to a " + type.kind.ToString());
            }
        }
    }

    public class ConditionalExpr : Expr {
        public ConditionalExpr(Expr _cond, Expr _true_expr, Expr _false_expr, ExprType _type)
            : base(_type) {
            cond_cond = _cond;
            cond_true_expr = _true_expr;
            cond_false_expr = _false_expr;
        }
        public readonly Expr cond_cond;
        public readonly Expr cond_true_expr;
        public readonly Expr cond_false_expr;
    }

    public class FunctionCall : Expr {
        public FunctionCall(Expr _function, TFunction _func_type, List<Expr> _arguments, ExprType _type)
            : base(_type) {
            call_func = _function;
            call_func_type = _func_type;
            call_args = _arguments;
        }
        public readonly Expr call_func;
        public readonly TFunction call_func_type;
        public readonly List<Expr> call_args;

        public override void CGenAddress(Env env, CGenState state) {
            throw new Exception("Error: cannot get the address of a function call.");
        }

        public override Reg CGenValue(Env env, CGenState state) {

            // Push the arguments onto the stack in reverse order
            for (Int32 i = call_args.Count; i-- > 0; ) {
                Expr arg = call_args[i];
                arg.CGenPush(env, state);
            }

            // Get function address
            call_func.CGenAddress(env, state);

            state.CALL("*%eax");

            return Reg.EAX;
        }
    }

    /// <summary>
    /// expr.name: expr must be a struct or union.
    /// </summary>
    public class Attribute : Expr {
        public Attribute(Expr expr, String name, ExprType type)
            : base(type) {
            this.expr = expr;
            this.name = name;
        }
        public readonly Expr expr;
        public readonly String name;

        public override Reg CGenValue(Env env, CGenState state) {
            Reg ret = expr.CGenValue(env, state);
            if (ret != Reg.STACK) {
                throw new InvalidProgramException();
            }

            Int32 size = expr.type.SizeOf; // size of the struct or union
            Int32 offset;                   // offset inside the pack
            switch (expr.type.kind) {
                case ExprType.Kind.STRUCT_OR_UNION:
                    // TODO: complete here.
                    offset = 0;
                    break;

                    //TStruct type = (TStruct)expr.type;
                    //Utils.StoreEntry entry = type.attribs.Find(_ => _.name == name);
                    //offset = entry.offset;
                    //break;

                default:
                    throw new InvalidProgramException();
            }

            // can't be a function designator.
            switch (type.kind) {
                case ExprType.Kind.ARRAY:
                    // TODO: how to handle this?
                    throw new NotImplementedException("I've not figured out how to do this.");

                case ExprType.Kind.CHAR:
                    state.MOVSBL(offset, Reg.EAX, Reg.EAX);
                    state.CGenShrinkStackBy(Utils.RoundUp(size, 4));
                    return Reg.EAX;

                case ExprType.Kind.UCHAR:
                    state.MOVZBL(offset, Reg.EAX, Reg.EAX);
                    state.CGenShrinkStackBy(Utils.RoundUp(size, 4));
                    return Reg.EAX;

                case ExprType.Kind.SHORT:
                    state.MOVSWL(offset, Reg.EAX, Reg.EAX);
                    state.CGenShrinkStackBy(Utils.RoundUp(size, 4));
                    return Reg.EAX;

                case ExprType.Kind.USHORT:
                    state.MOVZWL(offset, Reg.EAX, Reg.EAX);
                    state.CGenShrinkStackBy(Utils.RoundUp(size, 4));
                    return Reg.EAX;

                case ExprType.Kind.LONG:
                case ExprType.Kind.ULONG:
                case ExprType.Kind.POINTER:
                    state.MOVL(offset, Reg.EAX, Reg.EAX);
                    state.CGenShrinkStackBy(Utils.RoundUp(size, 4));
                    return Reg.EAX;

                case ExprType.Kind.FLOAT:
                    state.FLDS(offset, Reg.ST0);
                    state.CGenShrinkStackBy(Utils.RoundUp(size, 4));
                    return Reg.ST0;

                case ExprType.Kind.DOUBLE:
                    state.FLDL(offset, Reg.ST0);
                    state.CGenShrinkStackBy(Utils.RoundUp(size, 4));
                    return Reg.ST0;

                case ExprType.Kind.STRUCT_OR_UNION:
                    throw new NotImplementedException("Struct or union might mess up with the stack.");

                    //state.LEA(offset, Reg.ESP, Reg.ESI);
                    //state.LEA(Utils.RoundUp(size, 4) - Utils.RoundUp(type.size_of, 4), Reg.ESP, Reg.EDI);
                    //state.MemCpyReversed();
                    //state.CGenShrinkStackBy(Utils.RoundUp(size, 4) - Utils.RoundUp(type.size_of, 4));
                    //return Reg.STACK;
                    

                default:
                    throw new InvalidProgramException();
            }
        }

        public override void CGenAddress(Env env, CGenState state) {
            expr.CGenAddress(env, state);

            Int32 offset; // offset inside the pack
            switch (expr.type.kind) {
                case ExprType.Kind.STRUCT_OR_UNION:
                    // TODO: finish here.
                    offset = 0;
                    break;

                    //TStruct type = (TStruct)expr.type;
                    //Utils.StoreEntry entry = type.attribs.Find(_ => _.name == name);
                    //offset = entry.offset;
                    //break;


                default:
                    throw new InvalidProgramException();
            }

            state.ADDL(offset, Reg.EAX);
        }
    }

    /// <summary>
    /// &expr: get the address of expr.
    /// </summary>
    public class Reference : Expr {
        public Reference(Expr expr, ExprType type)
            : base(type) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override Reg CGenValue(Env env, CGenState state) {
            expr.CGenAddress(env, state);
            return Reg.EAX;
        }
    }

    /// <summary>
    /// *expr: expr must be a pointer.
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

        public override Reg CGenValue(Env env, CGenState state) {
            Reg ret = expr.CGenValue(env, state);
            if (ret != Reg.EAX) {
                throw new InvalidProgramException();
            }
            if (expr.type.kind != ExprType.Kind.POINTER) {
                throw new InvalidProgramException();
            }

            ExprType type = ((TPointer)expr.type).ref_t;
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
                    // %esi = src address
                    state.MOVL(Reg.EAX, Reg.ESI);

                    // %edi = dst address
                    state.CGenExpandStackBy(Utils.RoundUp(type.SizeOf, 4));
                    state.LEA(0, Reg.ESP, Reg.EDI);

                    // %ecx = nbytes
                    state.MOVL(type.SizeOf, Reg.ECX);

                    state.MemCpy();

                    return Reg.STACK;

                case ExprType.Kind.VOID:
                default:
                    throw new InvalidProgramException();
            }
        }

        public override void CGenAddress(Env env, CGenState state) {
            Reg ret = expr.CGenValue(env, state);
            if (ret != Reg.EAX) {
                throw new InvalidProgramException();
            }
        }
    }

    /// <summary>
    /// Integral Binary Operation
    /// </summary>
    //public abstract class IntBinOp : Expr {
    //    public IntBinOp(Expr lhs, Expr rhs, ExprType type)
    //        : base(type) {
    //        this.lhs = lhs;
    //        this.rhs = rhs;
    //    }

    //    public readonly Expr lhs;
    //    public readonly Expr rhs;

    //    public abstract void CGenOp(CGenState state);

    //    public override Reg CGenValue(Env env, CGenState state) {
    //        // 1. %eax = lhs
    //        Reg ret = lhs.CGenValue(env, state);
    //        if (ret != Reg.EAX) {
    //            throw new InvalidProgramException("lhs operand should return to %eax");
    //        }

    //        // 2. pushl %eax
    //        state.PUSHL(Reg.EAX);

    //        //   3. %eax = rhs
    //        ret = rhs.CGenValue(env, state);
    //        if (ret != Reg.EAX) {
    //            throw new InvalidProgramException("rhs operand should return to %eax");
    //        }

    //        // 4. popl %ebx
    //        state.POPL(Reg.EAX);

    //        // 5. andl %ebx, %eax
    //        state.ANDL(Reg.EBX, Reg.EAX);

    //        return Reg.EAX;
    //    }
    //}

}