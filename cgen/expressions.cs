using System;
using System.Collections.Generic;

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

            switch (type.type_kind) {
            case ExprType.ExprTypeKind.CHAR:
            case ExprType.ExprTypeKind.UCHAR:
            case ExprType.ExprTypeKind.SHORT:
            case ExprType.ExprTypeKind.USHORT:
            case ExprType.ExprTypeKind.LONG:
            case ExprType.ExprTypeKind.ULONG:
                // Integral
                if (ret != Reg.EAX) {
                    throw new InvalidProgramException("Integral values should be returned to %eax");
                }
                state.PUSHL(Reg.EAX);
                break;

            case ExprType.ExprTypeKind.FLOAT:
                // Float
                if (ret != Reg.ST0) {
                    throw new InvalidProgramException("Floats should be returned to %st(0)");
                }
                state.CGenExpandStack(4);
                state.FSTS(0, Reg.ESP);
                break;

            case ExprType.ExprTypeKind.DOUBLE:
                // Double
                if (ret != Reg.ST0) {
                    throw new InvalidProgramException("Doubles should be returned to %st(0)");
                }
                state.CGenExpandStack(8);
                state.FSTL(0, Reg.ESP);
                break;

            case ExprType.ExprTypeKind.ARRAY:
            case ExprType.ExprTypeKind.FUNCTION:
            case ExprType.ExprTypeKind.POINTER:
                // Pointer
                if (ret != Reg.EAX) {
                    throw new InvalidProgramException("Pointer values should be returned to %eax");
                }
                state.PUSHL(Reg.EAX);
                break;

            case ExprType.ExprTypeKind.ERROR:
            case ExprType.ExprTypeKind.INCOMPLETE_ARRAY:
            case ExprType.ExprTypeKind.INCOMPLETE_STRUCT:
            case ExprType.ExprTypeKind.INCOMPLETE_UNION:
            case ExprType.ExprTypeKind.INIT_LIST:
            case ExprType.ExprTypeKind.VOID:
                throw new InvalidProgramException(type.type_kind.ToString() + " can't be pushed onto the stack");

            case ExprType.ExprTypeKind.STRUCT:
            case ExprType.ExprTypeKind.UNION:
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
            switch (entry.entry_loc) {
            case Env.EntryLoc.FRAME:
                break;
            case Env.EntryLoc.STACK:
                break;
            case Env.EntryLoc.GLOBAL:
                switch (entry.entry_type.type_kind) {
                case ExprType.ExprTypeKind.FUNCTION:
                    state.LEA(name);
                    break;

                case ExprType.ExprTypeKind.CHAR:
                case ExprType.ExprTypeKind.DOUBLE:
                case ExprType.ExprTypeKind.ERROR:
                case ExprType.ExprTypeKind.FLOAT:

                case ExprType.ExprTypeKind.LONG:
                case ExprType.ExprTypeKind.POINTER:
                case ExprType.ExprTypeKind.SHORT:
                case ExprType.ExprTypeKind.STRUCT:
                case ExprType.ExprTypeKind.UCHAR:
                case ExprType.ExprTypeKind.ULONG:
                case ExprType.ExprTypeKind.UNION:
                case ExprType.ExprTypeKind.USHORT:
                case ExprType.ExprTypeKind.VOID:
                default:
                    throw new NotImplementedException();
                }
                break;
            case Env.EntryLoc.ENUM:
            case Env.EntryLoc.NOT_FOUND:
            case Env.EntryLoc.TYPEDEF:
            default:
                throw new InvalidProgramException("cannot get the address of " + entry.entry_loc);
            }
        }

        // TODO: struct and union
        public override Reg CGenValue(Env env, CGenState state) {
            Env.Entry entry = env.Find(name);

            Int32 offset = entry.entry_offset;
            if (entry.entry_loc == Env.EntryLoc.STACK) {
                offset = -offset;
            }

            switch (entry.entry_loc) {
            case Env.EntryLoc.ENUM:
                // enum constant : just an integer
                state.MOVL(offset, Reg.EAX);
                return Reg.EAX;

            case Env.EntryLoc.FRAME:
            case Env.EntryLoc.STACK:
                switch (type.type_kind) {
                case ExprType.ExprTypeKind.LONG:
                case ExprType.ExprTypeKind.ULONG:
                case ExprType.ExprTypeKind.POINTER:
                    state.MOVL(offset, Reg.EBP, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.FLOAT:
                    state.FLDS(offset, Reg.EBP);
                    return Reg.ST0;

                case ExprType.ExprTypeKind.DOUBLE:
                    state.FLDL(offset, Reg.EBP);
                    return Reg.ST0;

                case ExprType.ExprTypeKind.STRUCT:
                case ExprType.ExprTypeKind.UNION:
                    throw new NotImplementedException();

                case ExprType.ExprTypeKind.VOID:
                    state.MOVL(0, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.FUNCTION:
                    state.MOVL(name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.CHAR:
                    state.MOVSBL(offset, Reg.EBP, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.UCHAR:
                    state.MOVZBL(offset, Reg.EBP, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.SHORT:
                    state.MOVSWL(offset, Reg.EBP, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.USHORT:
                    state.MOVZWL(offset, Reg.EBP, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.ERROR:
                default:
                    throw new InvalidProgramException("cannot get the value of a " + type.type_kind.ToString());
                }

            case Env.EntryLoc.GLOBAL:
                switch (type.type_kind) {
                case ExprType.ExprTypeKind.CHAR:
                    state.MOVSBL(name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.UCHAR:
                    state.MOVZBL(name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.SHORT:
                    state.MOVSWL(name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.USHORT:
                    state.MOVZWL(name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.LONG:
                case ExprType.ExprTypeKind.ULONG:
                case ExprType.ExprTypeKind.POINTER:
                    state.MOVL(name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.FUNCTION:
                    state.MOVL("$" + name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.FLOAT:
                    state.FLDS(name);
                    return Reg.ST0;

                case ExprType.ExprTypeKind.DOUBLE:
                    state.FLDL(name);
                    return Reg.ST0;

                case ExprType.ExprTypeKind.STRUCT:
                case ExprType.ExprTypeKind.UNION:
                    throw new NotImplementedException();

                case ExprType.ExprTypeKind.VOID:
                    state.MOVL(0, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.ARRAY:
                    state.MOVL("$" + name, Reg.EAX);
                    return Reg.EAX;

                case ExprType.ExprTypeKind.ERROR:
                case ExprType.ExprTypeKind.INCOMPLETE_ARRAY:
                case ExprType.ExprTypeKind.INCOMPLETE_STRUCT:
                case ExprType.ExprTypeKind.INCOMPLETE_UNION:
                case ExprType.ExprTypeKind.INIT_LIST:
                default:
                    throw new InvalidProgramException("cannot get the value of a " + type.type_kind.ToString());
                }

            case Env.EntryLoc.TYPEDEF:
            case Env.EntryLoc.NOT_FOUND:
            default:
                throw new InvalidProgramException("cannot get the value of a " + entry.entry_loc.ToString());
            }
        }
    }

    public abstract class Constant : Expr {
        public Constant(ExprType _type)
            : base(_type) {
        }
        public override Boolean IsConstExpr() {
            return true;
        }
        public override void CGenAddress(Env env, CGenState state) {
            throw new InvalidOperationException("Error: cannot get the address of a constant");
        }
    }

    public class ConstLong : Constant {
        public ConstLong(Int32 _value)
            : base(new TLong(true)) {
            value = _value;
        }

        public override String ToString() {
            return "Int32(" + value + ")";
        }
        public readonly Int32 value;

        public override Reg CGenValue(Env env, CGenState state) {
            state.MOVL(value, Reg.EAX);
            return Reg.EAX;
        }

        public override void CGenPush(Env env, CGenState state) {
            state.PUSHL(value);
        }
    }

    public class ConstULong : Constant {
        public ConstULong(UInt32 _value)
            : base(new TULong(true)) {
            value = _value;
        }

        public override String ToString() {
            return "uint(" + value + ")";
        }
        public readonly UInt32 value;

        public override Reg CGenValue(Env env, CGenState state) {
            state.MOVL((Int32)value, Reg.EAX);
            return Reg.EAX;
        }

        public override void CGenPush(Env env, CGenState state) {
            state.PUSHL((Int32)value);
        }
    }

    public class ConstPtr : Constant {
        public ConstPtr(UInt32 _value, ExprType _type)
            : base(_type) {
            value = _value;
        }

        public override String ToString() {
            return this.type.ToString() + "(" + value + ")";
        }
        public readonly UInt32 value;

        public override Reg CGenValue(Env env, CGenState state) {
            state.MOVL((Int32)value, Reg.EAX);
            return Reg.EAX;
        }

        public override void CGenPush(Env env, CGenState state) {
            state.PUSHL((Int32)value);
        }
    }

    /// <summary>
    /// Constant Float
    /// </summary>
    public class ConstFloat : Constant {
        public ConstFloat(Single _value)
            : base(new TFloat(true)) {
            value = _value;
        }
        public override String ToString() {
            return "float(" + value + ")";
        }
        public readonly Single value;

        /// <summary>
        /// flds addr
        /// </summary>
        public override Reg CGenValue(Env env, CGenState state) {
            byte[] bytes = BitConverter.GetBytes(value);
            Int32 intval = BitConverter.ToInt32(bytes, 0);
            String name = state.CGenLongConst(intval);
            state.FLDS(name);
            return Reg.ST0;
        }
    }

    /// <summary>
    /// Constant Double
    /// </summary>
    public class ConstDouble : Constant {
        public ConstDouble(Double _value)
            : base(new TDouble(true)) {
            value = _value;
        }
        public override String ToString() {
            return "double(" + value + ")";
        }
        public readonly Double value;

        /// <summary>
        /// fldl addr
        /// </summary>
        public override Reg CGenValue(Env env, CGenState state) {
            byte[] bytes = BitConverter.GetBytes(value);
            Int32 first_int = BitConverter.ToInt32(bytes, 0);
            Int32 second_int = BitConverter.ToInt32(bytes, 4);
            String name = state.CGenLongLongConst(first_int, second_int);
            state.FLDL(name);
            return Reg.ST0;
        }
    }

    public class ConstStringLiteral : Constant {
        public ConstStringLiteral(String _value)
            : base(new TPointer(new TChar(true), true)) {
            value = _value;
        }
        public readonly String value;

        public override Reg CGenValue(Env env, CGenState state) {
            String name = state.CGenString(value);
            state.MOVL(name, Reg.EAX);
            return Reg.EAX;
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
            switch (lvalue.type.type_kind) {
            case ExprType.ExprTypeKind.CHAR:
            case ExprType.ExprTypeKind.UCHAR:
                // pop %ebx
                // now %ebx = %lhs
                state.POPL(Reg.EBX);

                // *%ebx = %al
                state.MOVB(Reg.AL, 0, Reg.EBX);

                return Reg.EAX;

            case ExprType.ExprTypeKind.SHORT:
            case ExprType.ExprTypeKind.USHORT:
                // pop %ebx
                // now %ebx = %lhs
                state.POPL(Reg.EBX);

                // *%ebx = %al
                state.MOVW(Reg.AX, 0, Reg.EBX);

                return Reg.EAX;

            case ExprType.ExprTypeKind.LONG:
            case ExprType.ExprTypeKind.ULONG:
            case ExprType.ExprTypeKind.POINTER:
                // pop %ebx
                // now %ebx = %lhs
                state.POPL(Reg.EBX);

                // *%ebx = %al
                state.MOVL(Reg.EAX, 0, Reg.EBX);

                return Reg.EAX;

            case ExprType.ExprTypeKind.FLOAT:
                // pop %ebx
                // now %ebx = %lhs
                state.POPL(Reg.EBX);

                // *%ebx = %st(0)
                state.FSTS(0, Reg.EBX);

                return Reg.ST0;

            case ExprType.ExprTypeKind.DOUBLE:
                // pop %ebx
                // now %ebx = %lhs
                state.POPL(Reg.EBX);

                // *%ebx = %st(0)
                state.FSTL(0, Reg.EBX);

                return Reg.ST0;

            case ExprType.ExprTypeKind.STRUCT:
            case ExprType.ExprTypeKind.UNION:
                throw new NotImplementedException();

            case ExprType.ExprTypeKind.FUNCTION:
            case ExprType.ExprTypeKind.VOID:
            case ExprType.ExprTypeKind.ARRAY:
            case ExprType.ExprTypeKind.ERROR:
            case ExprType.ExprTypeKind.INCOMPLETE_ARRAY:
            case ExprType.ExprTypeKind.INCOMPLETE_STRUCT:
            case ExprType.ExprTypeKind.INCOMPLETE_UNION:
            case ExprType.ExprTypeKind.INIT_LIST:
            default:
                throw new InvalidProgramException("cannot assign to a " + type.type_kind.ToString());
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

    public class PostIncrement : Expr {
        public PostIncrement(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        public readonly Expr expr;
    }

    public class PostDecrement : Expr {
        public PostDecrement(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class PreIncrement : Expr {
        public PreIncrement(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class PreDecrement : Expr {
        public PreDecrement(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
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

    public class GEqual : Expr {
        public GEqual(Expr _lhs, Expr _rhs, ExprType _type)
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

    public class Add : Expr {
        public Add(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            add_lhs = _lhs;
            add_rhs = _rhs;
        }
        public readonly Expr add_lhs;
        public readonly Expr add_rhs;

        public static AST.Expr GetPointerAddition(AST.Expr ptr, AST.Expr offset) {
            if (ptr.type.type_kind != AST.ExprType.ExprTypeKind.POINTER) {
                throw new InvalidOperationException("Error: expect a pointer");
            }
            if (offset.type.type_kind != AST.ExprType.ExprTypeKind.LONG) {
                throw new InvalidOperationException("Error: expect an integer");
            }

            if (ptr.IsConstExpr() && offset.IsConstExpr()) {
                Int32 _base = (Int32)((AST.ConstPtr)ptr).value;
                Int32 _scale = ((AST.TPointer)(ptr.type)).referenced_type.SizeOf;
                Int32 _offset = ((AST.ConstLong)offset).value;
                return new AST.ConstPtr((UInt32)(_base + _scale * _offset), ptr.type);
            }

            return AST.TypeCast.ToPointer(
                new AST.Add(
                    AST.TypeCast.FromPointer(ptr, new AST.TLong(ptr.type.is_const, ptr.type.is_volatile)),
                    new AST.Multiply(
                        offset,
                        new AST.ConstLong(((AST.TPointer)(ptr.type)).referenced_type.SizeOf),
                        new AST.TLong(offset.type.is_const, offset.type.is_volatile)
                    ),
                    new AST.TLong(offset.type.is_const, offset.type.is_volatile)
                ),
                ptr.type
            );
        }

        public static Tuple<Env, Expr> MakeAdd(Env env, Expr lhs, Expr rhs) {
            if (lhs.type.type_kind == AST.ExprType.ExprTypeKind.POINTER) {
                if (!rhs.type.IsIntegral()) {
                    throw new InvalidOperationException("Error: must add an integral to a pointer");
                }
                rhs = AST.TypeCast.MakeCast(rhs, new AST.TLong(rhs.type.is_const, rhs.type.is_volatile));

                // lhs = base, rhs = offset
                return new Tuple<AST.Env, AST.Expr>(env, GetPointerAddition(lhs, rhs));

            } else if (rhs.type.type_kind == AST.ExprType.ExprTypeKind.POINTER) {
                if (!lhs.type.IsIntegral()) {
                    throw new InvalidOperationException("Error: must add an integral to a pointer");
                }
                lhs = AST.TypeCast.MakeCast(lhs, new AST.TLong(lhs.type.is_const, rhs.type.is_volatile));

                // rhs = base, lhs = offset
                return new Tuple<AST.Env, AST.Expr>(env, GetPointerAddition(rhs, lhs));

            } else {
                return SyntaxTree.Expression.GetArithmeticBinOpExpr(
                    env,
                    lhs,
                    rhs,
                    (x, y) => x + y,
                    (x, y) => x + y,
                    (x, y) => x + y,
                    (x, y) => x + y,
                    (_lhs, _rhs, _type) => new AST.Add(_lhs, _rhs, _type)
                );
            }
        }
    }

    public class Sub : Expr {
        public Sub(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            sub_lhs = _lhs;
            sub_rhs = _rhs;
        }
        public readonly Expr sub_lhs;
        public readonly Expr sub_rhs;

        public static AST.Expr GetPointerSubtraction(AST.Expr ptr, AST.Expr offset) {
            if (ptr.type.type_kind != AST.ExprType.ExprTypeKind.POINTER) {
                throw new InvalidOperationException("Error: expect a pointer");
            }
            if (offset.type.type_kind != AST.ExprType.ExprTypeKind.LONG) {
                throw new InvalidOperationException("Error: expect an integer");
            }

            if (ptr.IsConstExpr() && offset.IsConstExpr()) {
                Int32 _base = (Int32)((AST.ConstPtr)ptr).value;
                Int32 _scale = ((AST.TPointer)(ptr.type)).referenced_type.SizeOf;
                Int32 _offset = ((AST.ConstLong)offset).value;
                return new AST.ConstPtr((UInt32)(_base - _scale * _offset), ptr.type);
            }

            return AST.TypeCast.ToPointer(
                new AST.Sub(
                    AST.TypeCast.FromPointer(ptr, new AST.TLong(ptr.type.is_const, ptr.type.is_volatile)),
                    new AST.Multiply(
                        offset,
                        new AST.ConstLong(((AST.TPointer)(ptr.type)).referenced_type.SizeOf),
                        new AST.TLong(offset.type.is_const, offset.type.is_volatile)
                    ),
                    new AST.TLong(offset.type.is_const, offset.type.is_volatile)
                ),
                ptr.type
            );
        }

        public static Tuple<Env, Expr> MakeSub(Env env, Expr lhs, Expr rhs) {
            if (lhs.type.type_kind == AST.ExprType.ExprTypeKind.POINTER) {
                if (rhs.type.type_kind == AST.ExprType.ExprTypeKind.POINTER) {
                    // both operands are pointers

                    AST.TPointer lhs_type = (AST.TPointer)(lhs.type);
                    AST.TPointer rhs_type = (AST.TPointer)(rhs.type);
                    if (!lhs_type.referenced_type.EqualType(rhs_type.referenced_type)) {
                        throw new InvalidOperationException("Error: the two pointers points to different types");
                    }

                    Int32 scale = lhs_type.referenced_type.SizeOf;

                    if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
                        return new Tuple<AST.Env, AST.Expr>(
                            env,
                            new AST.ConstLong(
                                (Int32)(((AST.ConstPtr)lhs).value - ((AST.ConstPtr)rhs).value) / scale
                            )
                        );

                    } else {
                        return new Tuple<AST.Env, AST.Expr>(
                            env,
                            new AST.Divide(
                            // long(lhs) - long(rhs)
                                new AST.Sub(
                                    AST.TypeCast.MakeCast(lhs, new AST.TLong()),
                                    AST.TypeCast.MakeCast(rhs, new AST.TLong()),
                                    new AST.TLong()
                                ),
                            // / scale
                                new AST.ConstLong(scale),
                                new AST.TLong()
                            )
                        );
                    }

                } else {
                    // pointer - integral

                    if (!rhs.type.IsIntegral()) {
                        throw new InvalidOperationException("Error: expected an integral");
                    }

                    rhs = AST.TypeCast.MakeCast(rhs, new AST.TLong(rhs.type.is_const, rhs.type.is_volatile));

                    return new Tuple<AST.Env, AST.Expr>(env, GetPointerSubtraction(lhs, rhs));
                }

            } else {
                // lhs is not a pointer.

                // we need usual arithmetic cast
                return SyntaxTree.Expression.GetArithmeticBinOpExpr(
                    env,
                    lhs,
                    rhs,
                    (x, y) => x - y,
                    (x, y) => x - y,
                    (x, y) => x - y,
                    (x, y) => x - y,
                    (_lhs, _rhs, _type) => new AST.Sub(_lhs, _rhs, _type)
                );

            }
        }
    }

    public class Multiply : Expr {
        public Multiply(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            mult_lhs = _lhs;
            mult_rhs = _rhs;
        }
        public readonly Expr mult_lhs;
        public readonly Expr mult_rhs;

        public static Tuple<Env, Expr> MakeMultiply(Env env, Expr lhs, Expr rhs) {
            return SyntaxTree.Expression.GetArithmeticBinOpExpr(
                env,
                lhs,
                rhs,
                (x, y) => x * y,
                (x, y) => x * y,
                (x, y) => x * y,
                (x, y) => x * y,
                (_lhs, _rhs, _type) => new Multiply(_lhs, _rhs, _type)
            );
        }
    }

    public class Divide : Expr {
        public Divide(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            div_lhs = _lhs;
            div_rhs = _rhs;
        }
        public readonly Expr div_lhs;
        public readonly Expr div_rhs;

        public static Tuple<Env, Expr> MakeDivide(Env env, Expr lhs, Expr rhs) {
            return SyntaxTree.Expression.GetArithmeticBinOpExpr(
                env,
                lhs,
                rhs,
                (x, y) => x / y,
                (x, y) => x / y,
                (x, y) => x / y,
                (x, y) => x / y,
                (_lhs, _rhs, _type) => new Divide(lhs, rhs, _type)
            );
        }
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

    // TODO: use DIV to get modulo
    public class Modulo : IntBinOp {
        public Modulo(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) {
        }

        public override void CGenOp(CGenState state) {
            throw new NotImplementedException();
        }

        public static Tuple<Env, Expr> MakeModulo(Env env, Expr lhs, Expr rhs) {
            return SyntaxTree.Expression.GetIntegralBinOpExpr(
                env,
                lhs,
                rhs,
                (x, y) => x % y,
                (x, y) => x % y,
                (_lhs, _rhs, _type) => new Modulo(_lhs, _rhs, _type)
            );
        }
    }

    public class LShift : IntBinOp {
        public LShift(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) {
        }

        public override void CGenOp(CGenState state) {
            state.SALL(Reg.EBX, Reg.EAX);
        }

        public static Tuple<Env, Expr> MakeLShift(Env env, Expr lhs, Expr rhs) {
            return SyntaxTree.Expression.GetIntegralBinOpExpr(
                env,
                lhs,
                rhs,
                (x, y) => (UInt32)((Int32)x << (Int32)y),
                (x, y) => x << y,
                (_lhs, _rhs, type) => new LShift(lhs, rhs, type)
            );
        }
    }

    // TODO: arithmetic rshift and logical rshift
    public class RShift : IntBinOp {
        public RShift(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) {
        }

        public override void CGenOp(CGenState state) {
            throw new NotImplementedException();
        }

        public static Tuple<Env, Expr> MakeRShift(Env env, Expr lhs, Expr rhs) {
            return SyntaxTree.Expression.GetIntegralBinOpExpr(
                env,
                lhs,
                rhs,
                (x, y) => (UInt32)((Int32)x >> (Int32)y),
                (x, y) => x >> y,
                (_lhs, _rhs, _type) => new RShift(_lhs, _rhs, _type)
            );
        }
    }

    public class Xor : IntBinOp {
        public Xor(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) {
        }

        public override void CGenOp(CGenState state) {
            state.XORL(Reg.EBX, Reg.EAX);
        }

        public static Tuple<Env, Expr> MakeXor(Env env, Expr lhs, Expr rhs) {
            return SyntaxTree.Expression.GetIntegralBinOpExpr(
                env,
                lhs,
                rhs,
                (x, y) => x ^ y,
                (x, y) => x ^ y,
                (_lhs, _rhs, _type) => new Xor(_lhs, _rhs, _type)
            );
        }
    }

    public class BitwiseOr : IntBinOp {
        public BitwiseOr(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) {
        }

        public override void CGenOp(CGenState state) {
            state.ORL(Reg.EBX, Reg.EAX);
        }

        public static Tuple<Env, Expr> MakeOr(Env env, Expr lhs, Expr rhs) {
            return SyntaxTree.Expression.GetIntegralBinOpExpr(
                env,
                lhs,
                rhs,
                (x, y) => x | y,
                (x, y) => x | y,
                (_lhs, _rhs, _type) => new AST.BitwiseOr(_lhs, _rhs, _type)
            );
        }

    }

    /// <summary>
    /// Bitwise And: lhs & rhs
    /// lhs & rhs must both be integral (%eax)
    /// </summary>
    public class BitwiseAnd : IntBinOp {
        public BitwiseAnd(Expr lhs, Expr rhs, ExprType type)
            : base(lhs, rhs, type) {
        }

        public override void CGenOp(CGenState state) {
            state.ANDL(Reg.EBX, Reg.EAX);
        }

        public static Tuple<Env, Expr> MakeBitwiseAnd(Env env, Expr lhs, Expr rhs) {
            return SyntaxTree.Expression.GetIntegralBinOpExpr(
                env,
                lhs,
                rhs,
                (x, y) => x & y,
                (x, y) => x & y,
                (_lhs, _rhs, _type) => new BitwiseAnd(_lhs, _rhs, _type)
            );
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