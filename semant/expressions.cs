using System;
using System.Collections.Generic;

namespace SyntaxTree {

    // 3.2.1.5
    /* First, if either operand has type long double, the other operand is converted to long double.
     * Otherwise, if either operand has type double, the other operand is converted to double.
     * Otherwise, if either operand has type float, the other operand is converted to float.
     * Otherwise, the integral promotions are performed on both operands.
     * Then the following rules are applied:
     * If either operand has type unsigned long Int32, the other operand is converted to unsigned long Int32.
     * Otherwise, if one operand has type long Int32 and the other has type unsigned Int32, if a long Int32 can represent all values of an unsigned Int32, the operand of type unsigned Int32 is converted to long Int32;
     * if a long Int32 cannot represent all the values of an unsigned Int32, both operands are converted to unsigned long Int32. Otherwise, if either operand has type long Int32, the other operand is converted to long Int32.
     * Otherwise, if either operand has type unsigned Int32, the other operand is converted to unsigned Int32.
     * Otherwise, both operands have type Int32.*/

    // My simplification:
    // I let long = Int32, long double = double

    public class Expression : PTNode {
        // public TType type;

        // TODO : [finished] Expression.GetExpression(env) -> (env, expr)
        public virtual Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            throw new NotImplementedException();
        }

        public delegate TValue ConstOperation<TValue>(TValue lhs, TValue rhs);

        public delegate Int32 ConstLogialOperation<TValue>(TValue lhs, TValue rhs);

        public delegate TRet BinExprConstructor<TRet>(AST.Expr lhs, AST.Expr rhs, AST.ExprType type);

        public delegate AST.Expr UnaryExprConstructor(AST.Expr expr);

        public static Tuple<AST.Env, AST.Expr> GetIntegralBinOpExpr<TRet>(
            AST.Env env,
            Expression expr_lhs,
            Expression expr_rhs,
            ConstOperation<UInt32> uint32_op,
            ConstOperation<Int32> int32_op,
            BinExprConstructor<TRet> construct
        ) where TRet : AST.Expr {

            Tuple<AST.Env, AST.Expr> r_lhs = expr_lhs.GetExpr(env);
            env = r_lhs.Item1;
            AST.Expr lhs = r_lhs.Item2;

            Tuple<AST.Env, AST.Expr> r_rhs = expr_rhs.GetExpr(env);
            env = r_rhs.Item1;
            AST.Expr rhs = r_rhs.Item2;

            Tuple<AST.Expr, AST.Expr, AST.ExprType.EnumExprType> r_cast = AST.TypeCast.UsualArithmeticConversion(lhs, rhs);
            lhs = r_cast.Item1;
            rhs = r_cast.Item2;
            Boolean c1 = lhs.type.is_const;
            Boolean c2 = rhs.type.is_const;
            Boolean v1 = lhs.type.is_volatile;
            Boolean v2 = rhs.type.is_volatile;
            Boolean is_const = c1 || c2;
            Boolean is_volatile = v1 || v2;

            AST.ExprType.EnumExprType enum_type = r_cast.Item3;

            AST.Expr expr;
            if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
                switch (enum_type) {
                case AST.ExprType.EnumExprType.ULONG:
                    expr = new AST.ConstULong(uint32_op(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value));
                    break;
                case AST.ExprType.EnumExprType.LONG:
                    expr = new AST.ConstLong(int32_op(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value));
                    break;
                default:
                    Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
                    return null;
                }

            } else {
                switch (enum_type) {
                case AST.ExprType.EnumExprType.ULONG:
                    expr = construct(lhs, rhs, new AST.TULong(is_const, is_volatile));
                    break;
                case AST.ExprType.EnumExprType.LONG:
                    expr = construct(lhs, rhs, new AST.TULong(is_const, is_volatile));
                    break;
                default:
                    Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
                    return null;
                }
            }

            return new Tuple<AST.Env, AST.Expr>(env, expr);

        }


        public static Tuple<AST.Env, AST.Expr> GetScalarBinLogicalOpExpr<TRet>(
            AST.Env env,
            Expression expr_lhs,
            Expression expr_rhs,
            ConstLogialOperation<Double> double_op,
            ConstLogialOperation<Single> float_op,
            ConstLogialOperation<UInt32> uint32_op,
            ConstLogialOperation<Int32> int32_op,
            BinExprConstructor<TRet> construct
        ) where TRet : AST.Expr {

            Tuple<AST.Env, AST.Expr> r_lhs = expr_lhs.GetExpr(env);
            env = r_lhs.Item1;
            AST.Expr lhs = r_lhs.Item2;

            Tuple<AST.Env, AST.Expr> r_rhs = expr_rhs.GetExpr(env);
            env = r_rhs.Item1;
            AST.Expr rhs = r_rhs.Item2;

            Tuple<AST.Expr, AST.Expr, AST.ExprType.EnumExprType> r_cast = AST.TypeCast.UsualScalarConversion(lhs, rhs);

            lhs = r_cast.Item1;
            rhs = r_cast.Item2;
            Boolean c1 = lhs.type.is_const;
            Boolean c2 = rhs.type.is_const;
            Boolean v1 = lhs.type.is_volatile;
            Boolean v2 = rhs.type.is_volatile;
            Boolean is_const = c1 || c2;
            Boolean is_volatile = v1 || v2;

            AST.ExprType.EnumExprType enum_type = r_cast.Item3;

            AST.Expr expr;
            if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
                switch (enum_type) {
                case AST.ExprType.EnumExprType.DOUBLE:
                    expr = new AST.ConstLong(double_op(((AST.ConstDouble)lhs).value, ((AST.ConstDouble)rhs).value));
                    break;
                case AST.ExprType.EnumExprType.FLOAT:
                    expr = new AST.ConstLong(float_op(((AST.ConstFloat)lhs).value, ((AST.ConstFloat)rhs).value));
                    break;
                case AST.ExprType.EnumExprType.ULONG:
                    expr = new AST.ConstLong(uint32_op(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value));
                    break;
                case AST.ExprType.EnumExprType.LONG:
                    expr = new AST.ConstLong(int32_op(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value));
                    break;
                default:
                    Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
                    return null;
                }

            } else {
                switch (enum_type) {
                case AST.ExprType.EnumExprType.DOUBLE:
                case AST.ExprType.EnumExprType.FLOAT:
                case AST.ExprType.EnumExprType.ULONG:
                case AST.ExprType.EnumExprType.LONG:
                    expr = construct(lhs, rhs, new AST.TLong(is_const, is_volatile));
                    break;
                default:
                    Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
                    return null;
                }
            }

            return new Tuple<AST.Env, AST.Expr>(env, expr);
        }

        public static Tuple<AST.Env, AST.Expr> GetUnaryOpExpr(
            AST.Env env,
            Expression expr,
            Dictionary<AST.ExprType.EnumExprType, UnaryExprConstructor> constructors,
            Dictionary<AST.ExprType.EnumExprType, UnaryExprConstructor> const_constructors
        ) {
            throw new NotImplementedException();
        }
    }


    public class EmptyExpression : Expression {
        public EmptyExpression() { }

        // TODO : [finished] NullExpression.GetExpression(env) -> (env, expr)
        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return new Tuple<AST.Env, AST.Expr>(env, new AST.EmptyExpr());
        }
    }

    public class Variable : Expression {
        public Variable(String _name) {
            name = _name;
        }

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            AST.Env.Entry entry = env.Find(name);
            if (entry == null) {
                Log.SemantError("Error: cannot find variable '" + name + "'");
                return null;
            }
            if (entry.entry_loc == AST.Env.EntryLoc.TYPEDEF) {
                Log.SemantError("Error: expected a variable, not a typedef.");
                return null;
            }
            if (entry.entry_loc == AST.Env.EntryLoc.ENUM) {
                return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstLong(entry.entry_offset));
            } else {
                return new Tuple<AST.Env, AST.Expr>(env, new AST.Variable(entry.entry_type, name));
            }
        }

        public String name;
    }


    public class Constant : Expression {
    }


    // ConstFloat
    // ==========
    // TODO : [finished] const float
    public class ConstFloat : Constant {
        public ConstFloat(Double _val, FloatSuffix _suffix) {
            val = _val;
            suffix = _suffix;
        }

        // GetExpr
        // =======
        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            switch (suffix) {
            case FloatSuffix.F:
                return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstFloat((Single)val));
            default:
                return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstDouble(val));
            }
        }

        public FloatSuffix suffix;
        public Double val;
    }

    // ConstInt
    // ========
    // TODO : [finished] const Int32
    public class ConstInt : Constant {
        public ConstInt(long _val, IntSuffix _int_type) {
            val = _val;
            int_type = _int_type;
        }

        // GetExpr
        // =======
        // 
        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            switch (int_type) {
            case IntSuffix.U:
            case IntSuffix.UL:
                return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstULong((uint)val));
            default:
                return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstLong((Int32)val));
            }
        }

        public IntSuffix int_type;
        public long val;
    }

    // StringLiteral
    // =============
    // TODO : [finished] String literal
    // 
    public class StringLiteral : Expression {
        public StringLiteral(String _val) {
            val = _val;
        }

        // GetExpr
        // =======
        // 
        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstStringLiteral(val));
        }

        public String val;
    }


    public class AssignmentList : Expression {
        public AssignmentList(List<Expression> _exprs) {
            assign_exprs = _exprs;
        }
        public List<Expression> assign_exprs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            List<AST.Expr> exprs = new List<AST.Expr>();
            AST.ExprType type = new AST.TVoid();
            foreach (Expression expr in assign_exprs) {
                Tuple<AST.Env, AST.Expr> r_expr = expr.GetExpr(env);
                env = r_expr.Item1;
                type = r_expr.Item2.type;
                exprs.Add(r_expr.Item2);
            }
            return new Tuple<AST.Env, AST.Expr>(env, new AST.AssignmentList(exprs, type));
        }
    }

    // Finished.
    public class ConditionalExpression : Expression {
        public ConditionalExpression(Expression _cond, Expression _true_expr, Expression _false_expr) {
            cond = _cond;
            true_expr = _true_expr;
            false_expr = _false_expr;
        }
        public Expression cond;
        public Expression true_expr;
        public Expression false_expr;

        //public override ScopeSandbox Semant(ScopeSandbox _scope) {
        //    scope = _scope;
        //    scope = cond.Semant(scope);
        //    scope = true_expr.Semant(scope);
        //    scope = false_expr.Semant(scope);

        //    if (!cond.type.IsArith()) {
        //        Log.SemantError("Error: expected arithmetic type.");
        //    }

        //    if (true_expr.type.IsArith() || false_expr.type.IsArith()) {
        //        SemantUsualArithmeticConversion(ref true_expr, ref false_expr);
        //    } else if ((true_expr.type.kind == TType.Kind.STRUCT && false_expr.type.kind == TType.Kind.STRUCT)
        //        || (true_expr.type.kind == TType.Kind.UNION && false_expr.type.kind == TType.Kind.UNION)
        //        || (true_expr.type.kind == TType.Kind.POINTER && false_expr.type.kind == TType.Kind.POINTER)) {
        //        Log.SemantError("Not implemented.");
        //    } else if (true_expr.type.kind == TType.Kind.VOID && false_expr.type.kind == TType.Kind.VOID) {
        //        type = new TVoid();
        //    } else {
        //        Log.SemantError("Error: conditional expression types not match.");
        //    }

        //    return scope;
        //}
    }

    public class Assignment : Expression {
        public Assignment(Expression _lvalue, Expression _rvalue) {
            assign_lvalue = _lvalue;
            assign_rvalue = _rvalue;
        }
        public Expression assign_lvalue;
        public Expression assign_rvalue;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_lhs = assign_lvalue.GetExpr(env);
            env = r_lhs.Item1;
            AST.Expr lvalue = r_lhs.Item2;

            Tuple<AST.Env, AST.Expr> r_rhs = assign_rvalue.GetExpr(env);
            env = r_rhs.Item1;
            AST.Expr rvalue = r_rhs.Item2;

            rvalue = AST.TypeCast.MakeCast(rvalue, lvalue.type);

            return new Tuple<AST.Env, AST.Expr>(env, new AST.Assignment(lvalue, rvalue, lvalue.type));
        }

    }


    public class MultAssign : Expression {
        public MultAssign(Expression _lvalue, Expression _rvalue) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
        public Expression lvalue;
        public Expression rvalue;

    }


    public class DivAssign : Expression {
        public DivAssign(Expression _lvalue, Expression _rvalue) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
        public Expression lvalue;
        public Expression rvalue;

    }


    public class ModAssign : Expression {
        public ModAssign(Expression _lvalue, Expression _rvalue) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
        public Expression lvalue;
        public Expression rvalue;

    }


    public class AddAssign : Expression {
        public AddAssign(Expression _lvalue, Expression _rvalue) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
        public Expression lvalue;
        public Expression rvalue;

    }


    public class SubAssign : Expression {
        public SubAssign(Expression _lvalue, Expression _rvalue) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
        public Expression lvalue;
        public Expression rvalue;

    }


    public class LeftShiftAssign : Expression {
        public LeftShiftAssign(Expression _lvalue, Expression _rvalue) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
        public Expression lvalue;
        public Expression rvalue;

    }


    public class RightShiftAssign : Expression {
        public RightShiftAssign(Expression _lvalue, Expression _rvalue) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
        public Expression lvalue;
        public Expression rvalue;

    }


    public class BitwiseAndAssign : Expression {
        public BitwiseAndAssign(Expression _lvalue, Expression _rvalue) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
        public Expression lvalue;
        public Expression rvalue;

    }


    public class XorAssign : Expression {
        public XorAssign(Expression _lvalue, Expression _rvalue) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
        public Expression lvalue;
        public Expression rvalue;

    }


    public class BitwiseOrAssign : Expression {
        public BitwiseOrAssign(Expression _lvalue, Expression _rvalue) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
        public Expression lvalue;
        public Expression rvalue;

    }


    public class FunctionCall : Expression {
        public FunctionCall(Expression _func, List<Expression> _args) {
            call_func = _func;
            call_args = _args;
        }
        public Expression call_func;
        public List<Expression> call_args;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_func = call_func.GetExpr(env);
            env = r_func.Item1;
            AST.Expr func = r_func.Item2;

            if (func.type.expr_type != AST.ExprType.EnumExprType.FUNCTION) {
                throw new Exception("Error: calling a non-function.");
            }

            AST.TFunction func_type = (AST.TFunction)(func.type);

            List<AST.Expr> args = new List<AST.Expr>();
            foreach (Expression expr in call_args) {
                Tuple<AST.Env, AST.Expr> r_expr = expr.GetExpr(env);
                env = r_expr.Item1;
                args.Add(r_expr.Item2);
            }

            if (func_type.args.Count != args.Count) {
                throw new Exception("Error: number of arguments mismatch.");
            }

            for (Int32 iarg = 0; iarg < args.Count; ++iarg) {
                args[iarg] = AST.TypeCast.MakeCast(args[iarg], func_type.args[iarg].entry_type);
            }

            return new Tuple<AST.Env, AST.Expr>(env, new AST.FunctionCall(func, func_type, args, func_type.ret_type));

        }

    }

    public class Attribute : Expression {
        public Attribute(Expression _expr, Variable _attrib) {
            expr = _expr;
            attrib = _attrib;
        }
        public Expression expr;
        public Variable attrib;
    }



    public class Increment : Expression {
        public Increment(Expression _expr) {
            expr = _expr;
        }
        public Expression expr;

    }


    public class Decrement : Expression {
        public Decrement(Expression _expr) {
            expr = _expr;
        }
        public Expression expr;

    }



    public class SizeofType : Expression {
        public SizeofType(TypeName _type_name) {
            type_name = _type_name;
        }

        public TypeName type_name;

    }


    public class SizeofExpression : Expression {
        public SizeofExpression(Expression _expr) {
            expr = _expr;
        }

        public Expression expr;

    }


    public class PrefixIncrement : Expression {
        public PrefixIncrement(Expression _expr) {
            expr = _expr;
        }
        public Expression expr;

    }


    public class PrefixDecrement : Expression {
        public PrefixDecrement(Expression _expr) {
            expr = _expr;
        }
        public Expression expr;

    }


    // Reference
    // =========
    // any type
    // 
    public class Reference : Expression {
        public Reference(Expression _expr) {
            ref_expr = _expr;
        }
        public Expression ref_expr;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = ref_expr.GetExpr(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            return new Tuple<AST.Env, AST.Expr>(env, new AST.Reference(expr, new AST.TPointer(expr.type)));
        }
    }


    // Dereference
    // ===========
    // requires pointer type
    // 
    public class Dereference : Expression {
        public Dereference(Expression _expr) {
            deref_expr = _expr;
        }
        public Expression deref_expr;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = deref_expr.GetExpr(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            if (expr.type.expr_type != AST.ExprType.EnumExprType.POINTER) {
                throw new Exception("Error: dereferencing a non-pointer");
            }

            // no matter constant or not
            return new Tuple<AST.Env, AST.Expr>(env, new AST.Dereference(expr, ((AST.TPointer)expr.type).referenced_type));
        }
    }


    // Positive
    // ========
    // requires arithmetic type
    // 
    public class Positive : Expression {
        public Positive(Expression _expr) {
            pos_expr = _expr;
        }
        public Expression pos_expr;

        // TODO : [finished] Positive.GetExpr(env) -> (env, expr)
        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = pos_expr.GetExpr(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            if (!expr.type.IsArith()) {
                Log.SemantError("Error: negation expectes arithmetic type.");
                return null;
            }

            return new Tuple<AST.Env, AST.Expr>(env, expr);
        }

    }

    // Negative
    // ========
    // requires aritmetic type
    // 
    public class Negative : Expression {
        public Negative(Expression _expr) {
            neg_expr = _expr;
        }
        public Expression neg_expr;

        // TODO : [finished] Negative.GetExpr(env) -> (env, expr)
        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = neg_expr.GetExpr(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            if (!expr.type.IsArith()) {
                Log.SemantError("Error: negation expectes arithmetic type.");
                return null;
            }

            if (expr.IsConstExpr()) {
                switch (expr.type.expr_type) {
                case AST.ExprType.EnumExprType.LONG:
                    AST.ConstLong long_expr = (AST.ConstLong)expr;
                    expr = new AST.ConstLong(-long_expr.value);
                    break;
                case AST.ExprType.EnumExprType.ULONG:
                    AST.ConstULong ulong_expr = (AST.ConstULong)expr;
                    expr = new AST.ConstLong(-(Int32)ulong_expr.value);
                    break;
                case AST.ExprType.EnumExprType.FLOAT:
                    AST.ConstFloat float_expr = (AST.ConstFloat)expr;
                    expr = new AST.ConstFloat(-float_expr.value);
                    break;
                case AST.ExprType.EnumExprType.DOUBLE:
                    AST.ConstDouble double_expr = (AST.ConstDouble)expr;
                    expr = new AST.ConstDouble(-double_expr.value);
                    break;
                default:
                    Log.SemantError("Error: wrong constant type?");
                    break;
                }
            } else {
                expr = new AST.Negative(expr, expr.type);
            }

            return new Tuple<AST.Env, AST.Expr>(env, expr);
        }

    }

    // BitwiseNot
    // ==========
    // requires integral type
    // 
    public class BitwiseNot : Expression {
        public BitwiseNot(Expression _expr) {
            not_expr = _expr;
        }
        public Expression not_expr;

        // TODO : [finished] BitwiseNot.GetExpr(env) -> (env, expr)
        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = not_expr.GetExpr(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            if (!expr.type.IsIntegral()) {
                Log.SemantError("Error: operator '~' expectes integral type.");
                return null;
            }

            if (expr.IsConstExpr()) {
                switch (expr.type.expr_type) {
                case AST.ExprType.EnumExprType.LONG:
                    AST.ConstLong long_expr = (AST.ConstLong)expr;
                    expr = new AST.ConstLong(~long_expr.value);
                    break;
                case AST.ExprType.EnumExprType.ULONG:
                    AST.ConstULong ulong_expr = (AST.ConstULong)expr;
                    expr = new AST.ConstULong(~ulong_expr.value);
                    break;
                default:
                    Log.SemantError("Error: wrong constant type?");
                    break;
                }
            } else {
                expr = new AST.BitwiseNot(expr, expr.type);
            }

            return new Tuple<AST.Env, AST.Expr>(env, expr);
        }

    }

    // Not
    // ===
    // requires scalar type
    // 
    public class Not : Expression {
        public Not(Expression _expr) {
            not_expr = _expr;
        }
        public Expression not_expr;

        // TODO : [finished] Not.GetExpr(env) -> (env, expr(type=long))
        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = not_expr.GetExpr(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            if (!expr.type.IsArith()) {
                Log.SemantError("Error: operator '!' expectes arithmetic type.");
                return null;
            }

            if (expr.IsConstExpr()) {
                Boolean value = false;
                switch (expr.type.expr_type) {
                case AST.ExprType.EnumExprType.LONG:
                    AST.ConstLong long_expr = (AST.ConstLong)expr;
                    value = long_expr.value != 0;
                    break;
                case AST.ExprType.EnumExprType.ULONG:
                    AST.ConstULong ulong_expr = (AST.ConstULong)expr;
                    value = ulong_expr.value != 0;
                    break;
                case AST.ExprType.EnumExprType.FLOAT:
                    AST.ConstFloat float_expr = (AST.ConstFloat)expr;
                    value = float_expr.value != 0;
                    break;
                case AST.ExprType.EnumExprType.DOUBLE:
                    AST.ConstDouble double_expr = (AST.ConstDouble)expr;
                    value = double_expr.value != 0;
                    break;
                default:
                    Log.SemantError("Error: wrong constant type?");
                    break;
                }
                if (value) {
                    expr = new AST.ConstLong(1);
                } else {
                    expr = new AST.ConstLong(0);
                }
            } else {
                expr = new AST.LogicalNot(expr, new AST.TLong(expr.type.is_const, expr.type.is_volatile));
            }

            return new Tuple<AST.Env, AST.Expr>(env, expr);
        }

    }


    public class TypeCast : Expression {
        public TypeCast(TypeName _type_name, Expression _expr) {
            type_name = _type_name;
            expr = _expr;
        }

        public TypeName type_name;
        public Expression expr;

    }


    public class Multiplication : Expression {
        public Multiplication(Expression _lhs, Expression _rhs) {
            mult_lhs = _lhs;
            mult_rhs = _rhs;
        }
        public Expression mult_lhs;
        public Expression mult_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_lhs = mult_lhs.GetExpr(env);
            env = r_lhs.Item1;
            AST.Expr lhs = r_lhs.Item2;

            Tuple<AST.Env, AST.Expr> r_rhs = mult_rhs.GetExpr(env);
            env = r_rhs.Item1;
            AST.Expr rhs = r_rhs.Item2;

            Tuple<AST.Expr, AST.Expr, AST.ExprType.EnumExprType> r_cast = AST.TypeCast.UsualArithmeticConversion(lhs, rhs);
            lhs = r_cast.Item1;
            rhs = r_cast.Item2;
            Boolean c1 = lhs.type.is_const;
            Boolean c2 = rhs.type.is_const;
            Boolean v1 = lhs.type.is_volatile;
            Boolean v2 = rhs.type.is_volatile;
            Boolean is_const = c1 || c2;
            Boolean is_volatile = v1 || v2;

            AST.ExprType.EnumExprType enum_type = r_cast.Item3;

            AST.Expr expr;
            if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
                switch (enum_type) {
                case AST.ExprType.EnumExprType.DOUBLE:
                    expr = new AST.ConstDouble(((AST.ConstDouble)lhs).value * ((AST.ConstDouble)rhs).value);
                    break;
                case AST.ExprType.EnumExprType.FLOAT:
                    expr = new AST.ConstFloat(((AST.ConstFloat)lhs).value * ((AST.ConstFloat)rhs).value);
                    break;
                case AST.ExprType.EnumExprType.ULONG:
                    expr = new AST.ConstULong(((AST.ConstULong)lhs).value * ((AST.ConstULong)rhs).value);
                    break;
                case AST.ExprType.EnumExprType.LONG:
                    expr = new AST.ConstLong(((AST.ConstLong)lhs).value * ((AST.ConstLong)rhs).value);
                    break;
                default:
                    Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
                    return null;
                }

            } else {
                switch (enum_type) {
                case AST.ExprType.EnumExprType.DOUBLE:
                    expr = new AST.Multiply(lhs, rhs, new AST.TDouble(is_const, is_volatile));
                    break;
                case AST.ExprType.EnumExprType.FLOAT:
                    expr = new AST.Multiply(lhs, rhs, new AST.TFloat(is_const, is_volatile));
                    break;
                case AST.ExprType.EnumExprType.ULONG:
                    expr = new AST.Multiply(lhs, rhs, new AST.TULong(is_const, is_volatile));
                    break;
                case AST.ExprType.EnumExprType.LONG:
                    expr = new AST.Multiply(lhs, rhs, new AST.TLong(is_const, is_volatile));
                    break;
                default:
                    Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
                    return null;
                }
            }

            return new Tuple<AST.Env, AST.Expr>(env, expr);

        }

    }


    public class Division : Expression {
        public Division(Expression _lhs, Expression _rhs) {
            div_lhs = _lhs;
            div_rhs = _rhs;
        }
        public Expression div_lhs;
        public Expression div_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_lhs = div_lhs.GetExpr(env);
            env = r_lhs.Item1;
            AST.Expr lhs = r_lhs.Item2;

            Tuple<AST.Env, AST.Expr> r_rhs = div_rhs.GetExpr(env);
            env = r_rhs.Item1;
            AST.Expr rhs = r_rhs.Item2;

            Tuple<AST.Expr, AST.Expr, AST.ExprType.EnumExprType> r_cast = AST.TypeCast.UsualArithmeticConversion(lhs, rhs);
            lhs = r_cast.Item1;
            rhs = r_cast.Item2;
            Boolean c1 = lhs.type.is_const;
            Boolean c2 = rhs.type.is_const;
            Boolean v1 = lhs.type.is_volatile;
            Boolean v2 = rhs.type.is_volatile;
            Boolean is_const = c1 || c2;
            Boolean is_volatile = v1 || v2;

            AST.ExprType.EnumExprType enum_type = r_cast.Item3;

            AST.Expr expr;
            if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
                switch (enum_type) {
                case AST.ExprType.EnumExprType.DOUBLE:
                    expr = new AST.ConstDouble(((AST.ConstDouble)lhs).value / ((AST.ConstDouble)rhs).value);
                    break;
                case AST.ExprType.EnumExprType.FLOAT:
                    expr = new AST.ConstFloat(((AST.ConstFloat)lhs).value / ((AST.ConstFloat)rhs).value);
                    break;
                case AST.ExprType.EnumExprType.ULONG:
                    expr = new AST.ConstULong(((AST.ConstULong)lhs).value / ((AST.ConstULong)rhs).value);
                    break;
                case AST.ExprType.EnumExprType.LONG:
                    expr = new AST.ConstLong(((AST.ConstLong)lhs).value / ((AST.ConstLong)rhs).value);
                    break;
                default:
                    Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
                    return null;
                }

            } else {
                switch (enum_type) {
                case AST.ExprType.EnumExprType.DOUBLE:
                    expr = new AST.Divide(lhs, rhs, new AST.TDouble(is_const, is_volatile));
                    break;
                case AST.ExprType.EnumExprType.FLOAT:
                    expr = new AST.Divide(lhs, rhs, new AST.TFloat(is_const, is_volatile));
                    break;
                case AST.ExprType.EnumExprType.ULONG:
                    expr = new AST.Divide(lhs, rhs, new AST.TULong(is_const, is_volatile));
                    break;
                case AST.ExprType.EnumExprType.LONG:
                    expr = new AST.Divide(lhs, rhs, new AST.TLong(is_const, is_volatile));
                    break;
                default:
                    Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
                    return null;
                }
            }

            return new Tuple<AST.Env, AST.Expr>(env, expr);

        }

    }


    // Modulo
    // ======
    // requires integral type
    // 
    public class Modulo : Expression {
        public Modulo(Expression _lhs, Expression _rhs) {
            mod_lhs = _lhs;
            mod_rhs = _rhs;
        }
        public Expression mod_lhs;
        public Expression mod_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return Expression.GetIntegralBinOpExpr(
                env,
                mod_lhs,
                mod_rhs,
                (x, y) => x % y,
                (x, y) => x % y,
                (lhs, rhs, type) => new AST.Modulo(lhs, rhs, type)
            );
        }

    }


    public class Addition : Expression {
        public Addition(Expression _lhs, Expression _rhs) {
            lhs = _lhs;
            rhs = _rhs;
        }

        public Expression lhs;
        public Expression rhs;

    }


    public class Subtraction : Expression {
        public Subtraction(Expression _lhs, Expression _rhs) {
            lhs = _lhs;
            rhs = _rhs;
        }

        public Expression lhs;
        public Expression rhs;
    }


    // LeftShift
    // =========
    // requires integral type
    // 
    public class LeftShift : Expression {
        public LeftShift(Expression _lhs, Expression _rhs) {
            shift_lhs = _lhs;
            shift_rhs = _rhs;
        }
        public Expression shift_lhs;
        public Expression shift_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return Expression.GetIntegralBinOpExpr(
                env,
                shift_lhs,
                shift_rhs,
                (x, y) => (UInt32)((Int32)x << (Int32)y),
                (x, y) => x << y,
                (lhs, rhs, type) => new AST.LShift(lhs, rhs, type)
            );
        }
    }

    // RightShift
    // ==========
    // requires integral type
    // 
    public class RightShift : Expression {
        public RightShift(Expression _lhs, Expression _rhs) {
            shift_lhs = _lhs;
            shift_rhs = _rhs;
        }
        public Expression shift_lhs;
        public Expression shift_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return Expression.GetIntegralBinOpExpr(
                env,
                shift_lhs,
                shift_rhs,
                (x, y) => (UInt32)((Int32)x >> (Int32)y),
                (x, y) => x >> y,
                (lhs, rhs, type) => new AST.RShift(lhs, rhs, type)
            );
        }

    }


    public class LessThan : Expression {
        public LessThan(Expression _lhs, Expression _rhs) {
            lt_lhs = _lhs;
            lt_rhs = _rhs;
        }
        public Expression lt_lhs;
        public Expression lt_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return GetScalarBinLogicalOpExpr(
                env,
                lt_lhs,
                lt_rhs,
                (x, y) => x < y ? 1 : 0,
                (x, y) => x < y ? 1 : 0,
                (x, y) => x < y ? 1 : 0,
                (x, y) => x < y ? 1 : 0,
                (lhs, rhs, type) => new AST.Less(lhs, rhs, type)
            );
        }
    }


    public class LessEqualThan : Expression {
        public LessEqualThan(Expression _lhs, Expression _rhs) {
            leq_lhs = _lhs;
            leq_rhs = _rhs;
        }

        public Expression leq_lhs;
        public Expression leq_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return GetScalarBinLogicalOpExpr(
                env,
                leq_lhs,
                leq_rhs,
                (x, y) => x <= y ? 1 : 0,
                (x, y) => x <= y ? 1 : 0,
                (x, y) => x <= y ? 1 : 0,
                (x, y) => x <= y ? 1 : 0,
                (lhs, rhs, type) => new AST.LEqual(lhs, rhs, type)
            );
        }

    }


    public class GreaterThan : Expression {
        public GreaterThan(Expression _lhs, Expression _rhs) {
            gt_lhs = _lhs;
            gt_rhs = _rhs;
        }

        public Expression gt_lhs;
        public Expression gt_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return GetScalarBinLogicalOpExpr(
                env,
                gt_lhs,
                gt_rhs,
                (x, y) => x < y ? 1 : 0,
                (x, y) => x < y ? 1 : 0,
                (x, y) => x < y ? 1 : 0,
                (x, y) => x < y ? 1 : 0,
                (lhs, rhs, type) => new AST.Greater(lhs, rhs, type)
            );
        }
    }


    // Equal
    // =====
    // requires arithmetic or pointer type
    // 
    public class GreaterEqualThan : Expression {
        public GreaterEqualThan(Expression _lhs, Expression _rhs) {
            geq_lhs = _lhs;
            geq_rhs = _rhs;
        }

        public Expression geq_lhs;
        public Expression geq_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return GetScalarBinLogicalOpExpr(
                env,
                geq_lhs,
                geq_rhs,
                (x, y) => x >= y ? 1 : 0,
                (x, y) => x >= y ? 1 : 0,
                (x, y) => x >= y ? 1 : 0,
                (x, y) => x >= y ? 1 : 0,
                (lhs, rhs, type) => new AST.GEqual(lhs, rhs, type)
            );
        }
    }

    // Equal
    // =====
    // requires arithmetic or pointer type
    // 
    public class Equal : Expression {
        public Equal(Expression _lhs, Expression _rhs) {
            eq_lhs = _lhs;
            eq_rhs = _rhs;
        }

        public Expression eq_lhs;
        public Expression eq_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return GetScalarBinLogicalOpExpr(
                env,
                eq_lhs,
                eq_rhs,
                (x, y) => x == y ? 1 : 0,
                (x, y) => x == y ? 1 : 0,
                (x, y) => x == y ? 1 : 0,
                (x, y) => x == y ? 1 : 0,
                (lhs, rhs, type) => new AST.Equal(lhs, rhs, type)
            );
        }
    }

    // NotEqual
    // ========
    // requires arithmetic or pointer type
    // 
    public class NotEqual : Expression {
        public NotEqual(Expression _lhs, Expression _rhs) {
            neq_lhs = _lhs;
            neq_rhs = _rhs;
        }

        public Expression neq_lhs;
        public Expression neq_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return GetScalarBinLogicalOpExpr(
                env,
                neq_lhs,
                neq_rhs,
                (x, y) => x != y ? 1 : 0,
                (x, y) => x != y ? 1 : 0,
                (x, y) => x != y ? 1 : 0,
                (x, y) => x != y ? 1 : 0,
                (lhs, rhs, type) => new AST.NotEqual(lhs, rhs, type)
            );
        }
    }

    // BitwiseAnd
    // ==========
    // requires integral type
    // 
    public class BitwiseAnd : Expression {
        public BitwiseAnd(Expression _lhs, Expression _rhs) {
            and_lhs = _lhs;
            and_rhs = _rhs;
        }

        public Expression and_lhs;
        public Expression and_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return GetIntegralBinOpExpr(
                env,
                and_lhs,
                and_rhs,
                (x, y) => x & y,
                (x, y) => x & y,
                (lhs, rhs, type) => new AST.BitwiseAnd(lhs, rhs, type)
            );
        }
    }

    // Xor
    // ===
    // requires integral type
    // 
    public class Xor : Expression {
        public Xor(Expression _lhs, Expression _rhs) {
            xor_lhs = _lhs;
            xor_rhs = _rhs;
        }
        public Expression xor_lhs;
        public Expression xor_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return GetIntegralBinOpExpr(
                env,
                xor_lhs,
                xor_rhs,
                (x, y) => x ^ y,
                (x, y) => x ^ y,
                (lhs, rhs, type) => new AST.Xor(lhs, rhs, type)
            );
        }
    }

    // BitwiseOr
    // =========
    // requires integral type
    // 
    public class BitwiseOr : Expression {
        public BitwiseOr(Expression _lhs, Expression _rhs) {
            or_lhs = _lhs;
            or_rhs = _rhs;
        }
        public Expression or_lhs;
        public Expression or_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return GetIntegralBinOpExpr(
                env,
                or_lhs,
                or_rhs,
                (x, y) => x | y,
                (x, y) => x | y,
                (lhs, rhs, type) => new AST.BitwiseOr(lhs, rhs, type)
            );
        }
    }

    // LogicalAnd
    // ==========
    // requires arithmetic or pointer type
    // 
    public class LogicalAnd : Expression {
        public LogicalAnd(Expression _lhs, Expression _rhs) {
            and_lhs = _lhs;
            and_rhs = _rhs;
        }
        public Expression and_lhs;
        public Expression and_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return GetScalarBinLogicalOpExpr(
                env,
                and_lhs,
                and_rhs,
                (x, y) => x != 0 && y != 0 ? 1 : 0,
                (x, y) => x != 0 && y != 0 ? 1 : 0,
                (x, y) => x != 0 && y != 0 ? 1 : 0,
                (x, y) => x != 0 && y != 0 ? 1 : 0,
                (lhs, rhs, type) => new AST.LogicalAnd(lhs, rhs, type)
            );
        }
    }

    // LogicalOr
    // =========
    // requires arithmetic or pointer type
    // 
    public class LogicalOr : Expression {
        public LogicalOr(Expression _lhs, Expression _rhs) {
            or_lhs = _lhs;
            or_rhs = _rhs;
        }
        public Expression or_lhs;
        public Expression or_rhs;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return GetScalarBinLogicalOpExpr(
                env,
                or_lhs,
                or_rhs,
                (x, y) => x != 0 || y != 0 ? 1 : 0,
                (x, y) => x != 0 || y != 0 ? 1 : 0,
                (x, y) => x != 0 || y != 0 ? 1 : 0,
                (x, y) => x != 0 || y != 0 ? 1 : 0,
                (lhs, rhs, type) => new AST.LogicalOr(lhs, rhs, type)
            );
        }
    }

}