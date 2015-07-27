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
                state.CGenExpandStack(4);
                state.FSTS(0, Reg.ESP);
                break;

            case ExprType.Kind.DOUBLE:
                // Double
                if (ret != Reg.ST0) {
                    throw new InvalidProgramException("Doubles should be returned to %st(0)");
                }
                state.CGenExpandStack(8);
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
            case ExprType.Kind.INCOMPLETE_STRUCT:
            case ExprType.Kind.INCOMPLETE_UNION:
            case ExprType.Kind.INIT_LIST:
            case ExprType.Kind.VOID:
                throw new InvalidProgramException(type.kind.ToString() + " can't be pushed onto the stack");

            case ExprType.Kind.STRUCT:
            case ExprType.Kind.UNION:
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
                    state.LEA(name);
                    break;

                case ExprType.Kind.CHAR:
                case ExprType.Kind.DOUBLE:
                case ExprType.Kind.ERROR:
                case ExprType.Kind.FLOAT:

                case ExprType.Kind.LONG:
                case ExprType.Kind.POINTER:
                case ExprType.Kind.SHORT:
                case ExprType.Kind.STRUCT:
                case ExprType.Kind.UCHAR:
                case ExprType.Kind.ULONG:
                case ExprType.Kind.UNION:
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

        // TODO: struct and union
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

                case ExprType.Kind.STRUCT:
                case ExprType.Kind.UNION:
                    throw new NotImplementedException();

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

                case ExprType.Kind.STRUCT:
                case ExprType.Kind.UNION:
                    throw new NotImplementedException();

                case ExprType.Kind.VOID:
                    state.MOVL(0, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.ARRAY:
                    state.MOVL("$" + name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.Kind.ERROR:
                case ExprType.Kind.INCOMPLETE_ARRAY:
                case ExprType.Kind.INCOMPLETE_STRUCT:
                case ExprType.Kind.INCOMPLETE_UNION:
                case ExprType.Kind.INIT_LIST:
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

            case ExprType.Kind.STRUCT:
            case ExprType.Kind.UNION:
                throw new NotImplementedException();

            case ExprType.Kind.FUNCTION:
            case ExprType.Kind.VOID:
            case ExprType.Kind.ARRAY:
            case ExprType.Kind.ERROR:
            case ExprType.Kind.INCOMPLETE_ARRAY:
            case ExprType.Kind.INCOMPLETE_STRUCT:
            case ExprType.Kind.INCOMPLETE_UNION:
            case ExprType.Kind.INIT_LIST:
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

    public class Attribute : Expr {
        public Attribute(Expr _expr, String _attrib_name, ExprType _type)
            : base(_type) {
            attrib_expr = _expr;
            attrib_name = _attrib_name;
        }
        public readonly Expr attrib_expr;
        public readonly String attrib_name;
    }

    /// <summary>
    /// expr++: must be scalar
    /// </summary>
    public class PostIncrement : Expr {
        public PostIncrement(Expr expr)
            : base(expr.type) {
            Debug.Assert(expr.type.IsScalar());
            this.expr = expr;
        }
        public readonly Expr expr;
    }

    /// <summary>
    /// expr--: must be a scalar
    /// </summary>
    public class PostDecrement : Expr {
        public PostDecrement(Expr expr)
            : base(expr.type) {
            Debug.Assert(expr.type.IsScalar());
            this.expr = expr;
        }
        public readonly Expr expr;
    }

    /// <summary>
    /// ++expr: must be a scalar
    /// </summary>
    public class PreIncrement : Expr {
        public PreIncrement(Expr expr)
            : base(expr.type) {
            Debug.Assert(expr.type.IsScalar());
            this.expr = expr;
        }
        public readonly Expr expr;
    }

    /// <summary>
    /// --expr: must be a scalar
    /// </summary>
    public class PreDecrement : Expr {
        public PreDecrement(Expr expr)
            : base(expr.type) {
            Debug.Assert(expr.type.IsScalar());
            this.expr = expr;
        }
        public readonly Expr expr;
    }

    public class Reference : Expr {
        public Reference(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class Dereference : Expr {
        public Dereference(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class Negative : Expr {
        public Negative(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class BitwiseNot : Expr {
        public BitwiseNot(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class LogicalNot : Expr {
        public LogicalNot(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class Equal : Expr {
        public Equal(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class Greater : Expr {
        public Greater(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class LEqual : Expr {
        public LEqual(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class Less : Expr {
        public Less(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    /// <summary>
    /// Integral Binary Operation
    /// </summary>
    public abstract class IntBinOp : Expr {
        public IntBinOp(Expr lhs, Expr rhs, ExprType type)
            : base(type) {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public readonly Expr lhs;
        public readonly Expr rhs;

        public abstract void CGenOp(CGenState state);

        public override Reg CGenValue(Env env, CGenState state) {
            // 1. %eax = lhs
            Reg ret = lhs.CGenValue(env, state);
            if (ret != Reg.EAX) {
                throw new InvalidProgramException("lhs operand should return to %eax");
            }

            // 2. pushl %eax
            state.PUSHL(Reg.EAX);

            //   3. %eax = rhs
            ret = rhs.CGenValue(env, state);
            if (ret != Reg.EAX) {
                throw new InvalidProgramException("rhs operand should return to %eax");
            }

            // 4. popl %ebx
            state.POPL(Reg.EAX);

            // 5. andl %ebx, %eax
            state.ANDL(Reg.EBX, Reg.EAX);

            return Reg.EAX;
        }
    }

    public class LogicalAnd : Expr {
        public LogicalAnd(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }

        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class LogicalOr : Expr {
        public LogicalOr(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }

        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class NotEqual : Expr {
        public NotEqual(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;
    }
}