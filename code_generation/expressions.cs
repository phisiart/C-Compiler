using System;
using System.Collections.Generic;

namespace AST {
    // Expr 
    // ========================================================================
    public class Expr {
        public Expr(ExprType _type) {
            type = _type;
        }
        public virtual Boolean IsConstExpr() { return false; }
        public virtual Reg CGenValue(Env env, CGenState state) {
            throw new NotImplementedException();
        }
        public virtual void CGenAddress(Env env, CGenState state) {
            throw new NotImplementedException();
        }
        public virtual void CGenPush(Env env, CGenState state) {
            throw new NotImplementedException();
        }
        public readonly ExprType type;
    }

    public class NullExpr : Expr {
        public NullExpr() : base(new TVoid()) { }
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
                switch (entry.entry_type.expr_type) {
                case ExprType.EnumExprType.FUNCTION:
                    state.LEA(name);
                    break;
                case ExprType.EnumExprType.CHAR:
                case ExprType.EnumExprType.DOUBLE:
                case ExprType.EnumExprType.ERROR:
                case ExprType.EnumExprType.FLOAT:
                
                case ExprType.EnumExprType.LONG:
                case ExprType.EnumExprType.POINTER:
                case ExprType.EnumExprType.SHORT:
                case ExprType.EnumExprType.STRUCT:
                case ExprType.EnumExprType.UCHAR:
                case ExprType.EnumExprType.ULONG:
                case ExprType.EnumExprType.UNION:
                case ExprType.EnumExprType.USHORT:
                case ExprType.EnumExprType.VOID:
                default:
                    throw new NotImplementedException();
                }
                break;
            case Env.EntryLoc.ENUM:
            case Env.EntryLoc.NOT_FOUND:
            case Env.EntryLoc.TYPEDEF:
            default:
                throw new InvalidOperationException("Error: cannot get the address of " + entry.entry_loc);
            }
        }

        public override void CGenPush(Env env, CGenState state) {
            //state.COMMENT("push " + name);
            Env.Entry entry = env.Find(name);
            switch (entry.entry_loc) {
            case Env.EntryLoc.ENUM:
                // enum constant : just an integer
                state.PUSHL(entry.entry_offset);
                break;
            case Env.EntryLoc.FRAME:
                switch (entry.entry_type.expr_type) {
                case ExprType.EnumExprType.LONG:
                case ExprType.EnumExprType.ULONG:
                case ExprType.EnumExprType.POINTER:
                    state.LOAD(entry.entry_offset, Reg.EBP, Reg.EAX);
                    state.PUSHL(Reg.EAX);
                    break;

                case ExprType.EnumExprType.FLOAT:
                case ExprType.EnumExprType.DOUBLE:
                case ExprType.EnumExprType.STRUCT:
                case ExprType.EnumExprType.UNION:
                    throw new NotImplementedException();

                case ExprType.EnumExprType.VOID:
                case ExprType.EnumExprType.FUNCTION:
                case ExprType.EnumExprType.CHAR:
                case ExprType.EnumExprType.UCHAR:
                case ExprType.EnumExprType.SHORT:
                case ExprType.EnumExprType.USHORT:
                case ExprType.EnumExprType.ERROR:
                default:
                    throw new InvalidOperationException("Error: cannot push type " + entry.entry_type.expr_type);
                }
                break;

            case Env.EntryLoc.GLOBAL:
                throw new NotImplementedException();

            case Env.EntryLoc.STACK:
                switch (entry.entry_type.expr_type) {
                case ExprType.EnumExprType.LONG:
                case ExprType.EnumExprType.ULONG:
                case ExprType.EnumExprType.POINTER:
                    state.LOAD(-entry.entry_offset, Reg.EBP, Reg.EAX);
                    state.PUSHL(Reg.EAX);
                    break;

                case ExprType.EnumExprType.FLOAT:
                case ExprType.EnumExprType.DOUBLE:
                case ExprType.EnumExprType.STRUCT:
                case ExprType.EnumExprType.UNION:
                    throw new NotImplementedException();

                case ExprType.EnumExprType.VOID:
                case ExprType.EnumExprType.FUNCTION:
                case ExprType.EnumExprType.CHAR:
                case ExprType.EnumExprType.UCHAR:
                case ExprType.EnumExprType.SHORT:
                case ExprType.EnumExprType.USHORT:
                case ExprType.EnumExprType.ERROR:
                default:
                    throw new InvalidOperationException("Error: cannot push type " + entry.entry_type.expr_type + " from stack");
                }
                break;
            case Env.EntryLoc.TYPEDEF:
            case Env.EntryLoc.NOT_FOUND:
            default:
                throw new InvalidOperationException();
            }
        }
    }

    public class Constant : Expr {
        public Constant(ExprType _type)
            : base(_type) { }
        public override Boolean IsConstExpr() { return true; }
    }

    public class ConstLong : Constant {
        public ConstLong(Int32 _value)
            : base(new TLong(true)) {
            value = _value;
        }

        public override string ToString() {
            return "int(" + value + ")";
        }
        public readonly Int32 value;

        public override void CGenAddress(Env env, CGenState state) {
            throw new Exception("Error: cannot get the address of a long literal.");
        }

        public override Reg CGenValue(Env env, CGenState state) {
            state.MOVL(value, Reg.EAX);
            return Reg.EAX;
        }
    }

    public class ConstULong : Constant {
        public ConstULong(UInt32 _value)
            : base(new TULong(true)) {
            value = _value;
        }

        public override string ToString() {
            return "uint(" + value + ")";
        }
        public readonly UInt32 value;
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
    }

    public class ConstFloat : Constant {
        public ConstFloat(Single _value)
            : base(new TFloat(true)) {
            value = _value;
        }
        public override string ToString() {
            return "float(" + value + ")";
        }
        public readonly Single value;
    }

    public class ConstDouble : Constant {
        public ConstDouble(Double _value)
            : base(new TDouble(true)) {
            value = _value;
        }
        public override string ToString() {
            return "double(" + value + ")";
        }
        public readonly Double value;
    }

    public class ConstStringLiteral : Constant {
        public ConstStringLiteral(String _value)
            : base(new TPointer(new TChar(true), true)) {
            value = _value;
        }
        public readonly String value;
    }

    public class AssignmentList : Expr {
        public AssignmentList(List<Expr> _exprs, ExprType _type)
            : base(_type) {
            exprs = _exprs;
        }

        public readonly List<Expr> exprs;
    }

    public class Assignment : Expr {
        public Assignment(Expr _lvalue, Expr _rvalue, ExprType _type)
            : base(_type) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
        protected Expr lvalue;
        protected Expr rvalue;
    }

    public class FunctionCall : Expr {
        public FunctionCall(Expr _function, TFunction _func_type, List<Expr> _arguments, ExprType _type)
            : base(_type) {
            call_func = _function;
            call_func_type = _func_type;
            call_args = _arguments;
        }
        public readonly Expr       call_func;
        public readonly TFunction  call_func_type;
        public readonly List<Expr> call_args;

        public override void CGenAddress(Env env, CGenState state) {
            throw new Exception("Error: cannot get the address of a function call.");
        }

        public override Reg CGenValue(Env env, CGenState state) {
            
            // Push the arguments onto the stack in reverse order
            for (int i = call_args.Count; i --> 0;) {
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
            expr = _expr;
            attrib_name = _attrib_name;
        }
        protected Expr expr;
        protected String attrib_name;
    }

    public class PostIncrement : Expr {
        public PostIncrement(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
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
    
    public class Multiply : Expr {
        public Multiply(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        protected Expr lhs;
        protected Expr rhs;
    }

    public class Divide : Expr {
        public Divide(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        protected Expr lhs;
        protected Expr rhs;
    }

    public class Modulo : Expr {
        public Modulo(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        protected Expr lhs;
        protected Expr rhs;
    }

    public class LShift : Expr {
        public LShift(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }

        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class RShift : Expr {
        public RShift(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }

        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class Xor : Expr {
        public Xor(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }

        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class BitwiseOr : Expr {
        public BitwiseOr(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }

        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class BitwiseAnd : Expr {
        public BitwiseAnd(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }

        public readonly Expr lhs;
        public readonly Expr rhs;
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