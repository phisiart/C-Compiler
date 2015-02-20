using System;
using System.Collections.Generic;

// 3.2.1.5
/* First, if either operand has type long double, the other operand is converted to long double.
 * Otherwise, if either operand has type double, the other operand is converted to double.
 * Otherwise, if either operand has type float, the other operand is converted to float.
 * Otherwise, the integral promotions are performed on both operands.
 * Then the following rules are applied:
 * If either operand has type unsigned long int, the other operand is converted to unsigned long int.
 * Otherwise, if one operand has type long int and the other has type unsigned int, if a long int can represent all values of an unsigned int, the operand of type unsigned int is converted to long int;
 * if a long int cannot represent all the values of an unsigned int, both operands are converted to unsigned long int. Otherwise, if either operand has type long int, the other operand is converted to long int.
 * Otherwise, if either operand has type unsigned int, the other operand is converted to unsigned int.
 * Otherwise, both operands have type int.*/

// My simplification:
// I let long = int, long double = double

public class Expression : PTNode {
    public TType type;

    // consider lhs & rhs are double/float/integral, make their types match.
    public void SemantUsualArithmeticConversion(ref Expression lhs, ref Expression rhs) {

        // if either operand has type double, the other operand is converted to double.
        if (lhs.type.kind == TType.Kind.FLOAT64 || rhs.type.kind == TType.Kind.FLOAT64) {
            lhs = new TypeCast(new TFloat64(), lhs);
            rhs = new TypeCast(new TFloat64(), rhs);
            type = new TFloat64();
            return;
        }

        // if either operand has type float, the other operand is converted to float.
        if (lhs.type.kind == TType.Kind.FLOAT32 || rhs.type.kind == TType.Kind.FLOAT32) {
            lhs = new TypeCast(new TFloat32(), lhs);
            rhs = new TypeCast(new TFloat32(), rhs);
            type = new TFloat32();
            return;
        }

        // if either operand has type unsigned long int, the other operand is converted to unsigned long int.
        if (lhs.type.kind == TType.Kind.UINT32 || rhs.type.kind == TType.Kind.UINT32) {
            lhs = new TypeCast(new TUInt32(), lhs);
            rhs = new TypeCast(new TUInt32(), rhs);
            type = new TUInt32();
            return;
        }

        // both operands have type int.
        lhs = new TypeCast(new TInt32(), lhs);
        rhs = new TypeCast(new TInt32(), rhs);
        type = new TInt32();

    }

    // consider lhs & rhs are both pointers or both arithmetic
    public void SemantPointerOrArithmeticConversion(ref Expression lhs, ref Expression rhs) {
        if (lhs.type.kind == TType.Kind.POINTER && rhs.type.kind == TType.Kind.POINTER) {
            lhs = new TypeCast(new TUInt32(), lhs);
            rhs = new TypeCast(new TUInt32(), rhs);
            type = new TUInt32();
        } else {
            SemantUsualArithmeticConversion(ref lhs, ref rhs);
        }
    }

    // consider lhs & rhs are both integrals
    public void SemantIntegralConversion(ref Expression lhs, ref Expression rhs) {
        if (lhs.type.IsInt() && rhs.type.IsInt()) {
            SemantUsualArithmeticConversion(ref lhs, ref rhs);
        } else {
            Log.SemantError("Error: expected integral type.");
        }
    }

    // consider lhs & rhs are pointer/integral, make their types match and converted to int
    public void SemantIntegralOrPointerConversion(ref Expression lhs, ref Expression rhs) {
        if (lhs.type.kind == TType.Kind.POINTER) {
            lhs = new TypeCast(new TUInt32(), lhs);
        }
        if (rhs.type.kind == TType.Kind.POINTER) {
            rhs = new TypeCast(new TUInt32(), rhs);
        }
        if (lhs.type.IsInt() && rhs.type.IsInt()) {
            SemantUsualArithmeticConversion(ref lhs, ref rhs);
        } else {
            Log.SemantError("Error: Expected pointer or integral type.");
        }
    }

    // TODO : Expression.GetExpression(env) -> (env, expr)
    public virtual Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
        return new Tuple<AST.Env, AST.Expr>(env, null);
    }
}

public class NullExpression : Expression {

    // TODO : NullExpression.GetExpression(env) -> (env, expr)
    public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
        return new Tuple<AST.Env, AST.Expr>(env, new AST.NullExpr());
    }
}

public class Variable : Expression {
    public Variable(String _name) {
        name = _name;
    }

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    Symbol symbol = scope.FindSymbol(name);
    //    if (symbol == null) {
    //        Log.SemantError("Error: cannot find variable " + name);
    //    }

    //    if (symbol.kind == Symbol.Kind.TYPEDEF) {
    //        Log.SemantError("Error: expected a variable, but found a type");
    //    }

    //    type = symbol.type;

    //    return scope;
    //}

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

        return new Tuple<AST.Env, AST.Expr>(env, new AST.Variable(entry.entry_type, name));
    }

    public String name;
}


public class Constant : Expression {
}

// NOTE : there is no const char in C, there is only const int ...
//public class ConstChar : Constant {
//    public ConstChar(Char _val) {
//        val = _val;
//    }

//    public override ScopeSandbox Semant(ScopeSandbox _scope) {
//        scope = _scope;
//        type = new TChar();
//        return scope;
//    }

//    public Char val;
//}


// ConstFloat
// ==========
// TODO : [finished] const float
public class ConstFloat : Constant {
    public ConstFloat(Double _val, FloatSuffix _float_type) {
        val = _val;
        float_type = _float_type;
    }

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    switch (float_type) {
    //        case FloatType.F:
    //            type = new TFloat();
    //            break;
    //        case FloatType.LF:
    //            type = new TLongDouble();
    //            break;
    //        case FloatType.NONE:
    //            type = new TDouble();
    //            break;
    //    }
    //    return scope;
    //}

    // GetExpr
    // =======
    public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
        switch (float_type) {
        case FloatSuffix.F:
            return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstFloat((float)val));
        default:
            return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstDouble(val));
        }
    }

    public FloatSuffix float_type;
    public Double val;
}

// ConstInt
// ========
// TODO : [finished] const int
public class ConstInt : Constant {
    public ConstInt(long _val, IntSuffix _int_type) {
        val = _val;
        int_type = _int_type;
    }

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    switch (int_type) {
    //    case IntType.NONE:
    //        type = new TInt();
    //        break;
    //    case IntType.L:
    //        type = new TLong();
    //        break;
    //    case IntType.U:
    //        type = new TUInt();
    //        break;
    //    case IntType.UL:
    //        type = new TULong();
    //        break;
    //    }
    //    return scope;
    //}

    // GetExpr
    // =======
    public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
        switch (int_type) {
        case IntSuffix.U:
        case IntSuffix.UL:
            return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstULong((uint)val));
        default:
            return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstLong((int)val));
        }
    }

    public IntSuffix int_type;
    public long val;
}

// StringLiteral
// =============
// TODO : [finished] string literal
public class StringLiteral : Expression {
    public StringLiteral(String _val) {
        val = _val;
    }

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    type = new TPointer(new TChar());
    //    type.is_const = true;
    //    return scope;
    //}

    // GetExpr
    // =======
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

// Finished.
public class Assignment : Expression {
    public Assignment(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lvalue.Semant(scope);
    //    scope = rvalue.Semant(scope);
    //    rvalue = new TypeCast(lvalue.type, rvalue);
    //    type = lvalue.type;
    //    return scope;
    //}
}

// Finished.
public class MultAssign : Expression {
    public MultAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;

    // after semant
    public TType ltype;
    public TypeCast cast;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lvalue.Semant(scope);
    //    scope = rvalue.Semant(scope);
    //    ltype = lvalue.type;
    //    SemantUsualArithmeticConversion(ref lvalue, ref rvalue);
    //    cast = new TypeCast(ltype, this);
    //    cast.expr = null;
    //    return scope;
    //}
}

// Finished.
public class DivAssign : Expression {
    public DivAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;

    // after semant
    public TType ltype;
    public TypeCast cast;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lvalue.Semant(scope);
    //    scope = rvalue.Semant(scope);
    //    ltype = lvalue.type;
    //    SemantUsualArithmeticConversion(ref lvalue, ref rvalue);
    //    cast = new TypeCast(ltype, this);
    //    cast.expr = null;
    //    return scope;
    //}

}

// Finished.
public class ModAssign : Expression {
    public ModAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;

    public TType ltype;
    public TypeCast cast;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lvalue.Semant(scope);
    //    scope = rvalue.Semant(scope);
    //    ltype = lvalue.type;
    //    if (!lvalue.type.IsInt() || !rvalue.type.IsInt()) {
    //        Log.SemantError("Error: expected integral type.");
    //    }
    //    SemantUsualArithmeticConversion(ref lvalue, ref rvalue);
    //    cast = new TypeCast(ltype, this);
    //    cast.expr = null;
    //    return scope;
    //}
}

// Finished.
public class AddAssign : Expression {
    public AddAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;

    // after semant
    public TType ltype;
    public TypeCast cast;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lvalue.Semant(scope);
    //    scope = rvalue.Semant(scope);
    //    ltype = lvalue.type;
    //    SemantUsualArithmeticConversion(ref lvalue, ref rvalue);
    //    cast = new TypeCast(ltype, this);
    //    cast.expr = null;
    //    return scope;
    //}

}

// Finished.
public class SubAssign : Expression {
    public SubAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;

    // after semant
    public TType ltype;
    public TypeCast cast;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lvalue.Semant(scope);
    //    scope = rvalue.Semant(scope);
    //    ltype = lvalue.type;
    //    SemantUsualArithmeticConversion(ref lvalue, ref rvalue);
    //    cast = new TypeCast(ltype, this);
    //    cast.expr = null;
    //    return scope;
    //}

}

// Finished.
public class LeftShiftAssign : Expression {
    public LeftShiftAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;

    public TType ltype;
    public TypeCast cast;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lvalue.Semant(scope);
    //    scope = rvalue.Semant(scope);
    //    ltype = lvalue.type;
    //    if (!lvalue.type.IsInt() || !rvalue.type.IsInt()) {
    //        Log.SemantError("Error: expected integral type.");
    //    }
    //    SemantUsualArithmeticConversion(ref lvalue, ref rvalue);
    //    cast = new TypeCast(ltype, this);
    //    cast.expr = null;
    //    return scope;
    //}
}

// Finished.
public class RightShiftAssign : Expression {
    public RightShiftAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;

    public TType ltype;
    public TypeCast cast;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lvalue.Semant(scope);
    //    scope = rvalue.Semant(scope);
    //    ltype = lvalue.type;
    //    if (!lvalue.type.IsInt() || !rvalue.type.IsInt()) {
    //        Log.SemantError("Error: expected integral type.");
    //    }
    //    SemantUsualArithmeticConversion(ref lvalue, ref rvalue);
    //    cast = new TypeCast(ltype, this);
    //    cast.expr = null;
    //    return scope;
    //}
}

// Finished.
public class BitwiseAndAssign : Expression {
    public BitwiseAndAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;

    public TType ltype;
    public TypeCast cast;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lvalue.Semant(scope);
    //    scope = rvalue.Semant(scope);
    //    ltype = lvalue.type;
    //    if (!lvalue.type.IsInt() || !rvalue.type.IsInt()) {
    //        Log.SemantError("Error: expected integral type.");
    //    }
    //    SemantUsualArithmeticConversion(ref lvalue, ref rvalue);
    //    cast = new TypeCast(ltype, this);
    //    cast.expr = null;
    //    return scope;
    //}
}

// Finished.
public class XorAssign : Expression {
    public XorAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;

    public TType ltype;
    public TypeCast cast;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lvalue.Semant(scope);
    //    scope = rvalue.Semant(scope);
    //    ltype = lvalue.type;
    //    if (!lvalue.type.IsInt() || !rvalue.type.IsInt()) {
    //        Log.SemantError("Error: expected integral type.");
    //    }
    //    SemantUsualArithmeticConversion(ref lvalue, ref rvalue);
    //    cast = new TypeCast(ltype, this);
    //    cast.expr = null;
    //    return scope;
    //}
}

// Finished.
public class BitwiseOrAssign : Expression {
    public BitwiseOrAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;

    public TType ltype;
    public TypeCast cast;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lvalue.Semant(scope);
    //    scope = rvalue.Semant(scope);
    //    ltype = lvalue.type;
    //    if (!lvalue.type.IsInt() || !rvalue.type.IsInt()) {
    //        Log.SemantError("Error: expected integral type.");
    //    }
    //    SemantUsualArithmeticConversion(ref lvalue, ref rvalue);
    //    cast = new TypeCast(ltype, this);
    //    cast.expr = null;
    //    return scope;
    //}
}


//public class ArrayElement : Expression {
//    public ArrayElement(Expression _var, Expression _idx) {
//        var = _var;
//        idx = _idx;
//    }
//    public Expression var;
//    public Expression idx;
//}

// not finished.
public class FunctionCall : Expression {
    public FunctionCall(Expression _func, List<Expression> _args) {
        func = _func;
        args = _args;
    }
    public Expression func;
    public List<Expression> args;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;

    //    scope = func.Semant(scope);
    //    foreach (Expression arg in args) {
    //        scope = arg.Semant(scope);
    //    }

    //    // args are assigned

    //    return scope;
    //}
}

public class Attribute : Expression {
    public Attribute(Expression _expr, Variable _attrib) {
        expr = _expr;
        attrib = _attrib;
    }
    public Expression expr;
    public Variable attrib;
}

//public class PointerAttribute : Expression {
//    public PointerAttribute(Expression _expr, Variable _attrib) {
//        expr = _expr;
//        attrib = _attrib;
//    }
//    public Expression expr;
//    public Variable attrib;
//}

// Finished.
public class Increment : Expression {
    public Increment(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = expr.Semant(scope);
    //    if (!expr.type.IsScalar()) {
    //        Log.SemantError("Error: increment expected scalar type.");
    //    }
    //    type = expr.type;
    //    return scope;
    //}
}

// Finished
public class Decrement : Expression {
    public Decrement(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = expr.Semant(scope);
    //    if (!expr.type.IsScalar()) {
    //        Log.SemantError("Error: decrement expected scalar type.");
    //    }
    //    type = expr.type;
    //    return scope;
    //}
}


//public class ArgumentList : Expression {
//    public ArgumentList(List<Expression> _exprs) {
//        exprs = _exprs;
//    }
//    public List<Expression> exprs;
//}


// Finished.
public class SizeofType : Expression {
    public SizeofType(TypeName _type_name) {
        type_name = _type_name;
    }

    // after parsing
    public TypeName type_name;

    // after semant
    public Int64 size;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = type_name.Semant(scope);
    //    size = type_name.type.SizeOf();
    //    return scope;
    //}

}

// Finished.
public class SizeofExpression : Expression {
    public SizeofExpression(Expression _expr) {
        expr = _expr;
    }

    // after parsing
    public Expression expr;

    // after semant
    public Int64 size;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    size = expr.type.SizeOf();
    //    return scope;
    //}

}

// Finished.
public class PrefixIncrement : Expression {
    public PrefixIncrement(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = expr.Semant(scope);
    //    if (!expr.type.IsScalar()) {
    //        Log.SemantError("Error: increment expected scalar type.");
    //    }
    //    type = expr.type;
    //    return scope;
    //}

}

// Finished.
public class PrefixDecrement : Expression {
    public PrefixDecrement(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = expr.Semant(scope);
    //    if (!expr.type.IsScalar()) {
    //        Log.SemantError("Error: decrement expected scalar type.");
    //    }
    //    type = expr.type;
    //    return scope;
    //}

}

// Finished.
public class Reference : Expression {
    public Reference(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = expr.Semant(scope);
    //    type = new TPointer(expr.type);
    //    return scope;
    //}

}

// Finished.
public class Dereference : Expression {
    public Dereference(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = expr.Semant(scope);
    //    if (expr.type.kind != TType.Kind.POINTER) {
    //        Log.SemantError("Error: dereferencing expected pointer type.");
    //    }
    //    TPointer ptype = (TPointer)expr.type;
    //    type = ptype.ref_t;
    //    return scope;
    //}

}

// Finished.
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

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = expr.Semant(scope);
    //    return scope;
    //}
}

// Finished.
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

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = expr.Semant(scope);
    //    if (!expr.type.IsArith()) {
    //        Log.SemantError("Error: negation expected arithmetic type.");
    //    }
    //    type = expr.type;
    //    return scope;
    //}
}

// Finished.
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

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = expr.Semant(scope);
    //    if (!expr.type.IsInt()) {
    //        Log.SemantError("Error: bitwise not expected integral type.");
    //    }
    //    type = expr.type;
    //    return scope;
    //}
}

// Finished.
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
            bool value = false;
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

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = expr.Semant(scope);
    //    if (!expr.type.IsArith()) {
    //        Log.SemantError("Error: logical not expected integral type.");
    //    }
    //    type = expr.type;
    //    return scope;
    //}

}

// Finished.
public class TypeCast : Expression {
    public TypeCast(TypeName _type_name, Expression _expr) {
        type_name = _type_name;
        expr = _expr;
    }

    public TypeCast(TType _type, Expression _expr) {
        expr = _expr;
        type_name = null;
        type = _type;
        scope = expr.scope;
        //SemantConversion();
    }

    // after parsing
    public TypeName type_name;
    public Expression expr;

    // after semant
    //public List<Cast> casts;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = expr.Semant(scope);
    //    scope = type_name.Semant(scope);
    //    type = type_name.type;
    //    return SemantConversion();
    //}

    //public ScopeSandbox SemantConversion() {

    //    TType.Kind kind_from = expr.type.kind;
    //    TType.Kind kind_to = type.kind;

    //    if (kind_from == kind_to) {
    //        casts = new List<Cast>();
    //        return scope;
    //    }

    //    if ((casts = SemantConversionFromSignedIntegral(kind_from, kind_to)) != null) {
    //        return scope;
    //    }

    //    if ((casts = SemantConversionFromUnsignedIntegral(kind_from, kind_to)) != null) {
    //        return scope;
    //    }

    //    if ((casts = SemantConversionFromFloat(kind_from, kind_to)) != null) {
    //        return scope;
    //    }

    //    if ((casts = SemantConversionFromPointer(kind_from, kind_to)) != null) {
    //        return scope;
    //    }

    //    Log.SemantError("Error: cannot convert from " + kind_from.ToString() + " to " + kind_to.ToString());
    //    return scope;

    //}

    //public enum Cast {
    //    INT8_TO_INT16,
    //    INT8_TO_INT32,
    //    INT8_TO_UINT8,

    //    INT16_TO_INT8,
    //    INT16_TO_INT32,
    //    INT16_TO_UINT16,

    //    INT32_TO_INT8,
    //    INT32_TO_INT16,
    //    INT32_TO_UINT32,
    //    INT32_TO_FLOAT32,
    //    INT32_TO_FLOAT64,

    //    UINT8_TO_UINT16,
    //    UINT8_TO_UINT32,
    //    UINT8_TO_INT8,

    //    UINT16_TO_UINT8,
    //    UINT16_TO_UINT32,
    //    UINT16_TO_INT16,

    //    UINT32_TO_UINT8,
    //    UINT32_TO_UINT16,
    //    UINT32_TO_INT32,
    //    UINT32_TO_FLOAT32,
    //    UINT32_TO_FLOAT64,

    //    FLOAT32_TO_INT32,
    //    FLOAT32_TO_FLOAT64,

    //    FLOAT64_TO_FLOAT32,
    //    FLOAT64_TO_INT32,

    //}

    //public List<Cast> SemantConversionFromSignedIntegral(TType.Kind from, TType.Kind to) {
    //    switch (from) {
    //    case TType.Kind.INT8:
    //        switch (to) {
    //        case TType.Kind.INT16:
    //            return new List<Cast> { Cast.INT8_TO_INT16 };
    //        case TType.Kind.INT32:
    //            return new List<Cast> { Cast.INT8_TO_INT32 };
    //        case TType.Kind.UINT8:
    //            return new List<Cast> { Cast.INT8_TO_UINT8 };
    //        case TType.Kind.UINT16:
    //            return new List<Cast> { Cast.INT8_TO_INT16, Cast.INT16_TO_UINT16 };
    //        case TType.Kind.UINT32:
    //        case TType.Kind.POINTER:
    //            return new List<Cast> { Cast.INT8_TO_INT32, Cast.INT32_TO_UINT32 };
    //        case TType.Kind.FLOAT32:
    //            return new List<Cast> { Cast.INT8_TO_INT32, Cast.INT32_TO_FLOAT32 };
    //        case TType.Kind.FLOAT64:
    //            return new List<Cast> { Cast.INT8_TO_INT32, Cast.INT32_TO_FLOAT64 };
    //        default:
    //            return null;
    //        }
    //    case TType.Kind.INT16:
    //        switch (to) {
    //        case TType.Kind.INT8:
    //            return new List<Cast> { Cast.INT16_TO_INT8 };
    //        case TType.Kind.INT32:
    //            return new List<Cast> { Cast.INT16_TO_INT32 };
    //        case TType.Kind.UINT8:
    //            return new List<Cast> { Cast.INT16_TO_INT8, Cast.INT8_TO_UINT8 };
    //        case TType.Kind.UINT16:
    //            return new List<Cast> { Cast.INT16_TO_UINT16 };
    //        case TType.Kind.UINT32:
    //        case TType.Kind.POINTER:
    //            return new List<Cast> { Cast.INT16_TO_INT32, Cast.INT32_TO_UINT32 };
    //        case TType.Kind.FLOAT32:
    //            return new List<Cast> { Cast.INT16_TO_INT32, Cast.INT32_TO_FLOAT32 };
    //        case TType.Kind.FLOAT64:
    //            return new List<Cast> { Cast.INT16_TO_INT32, Cast.INT32_TO_FLOAT64 };
    //        default:
    //            return null;
    //        }
    //    case TType.Kind.INT32:
    //        switch (to) {
    //        case TType.Kind.INT8:
    //            return new List<Cast> { Cast.INT32_TO_INT8 };
    //        case TType.Kind.INT16:
    //            return new List<Cast> { Cast.INT32_TO_INT16 };
    //        case TType.Kind.UINT8:
    //            return new List<Cast> { Cast.INT32_TO_INT8, Cast.INT8_TO_UINT8 };
    //        case TType.Kind.UINT16:
    //            return new List<Cast> { Cast.INT32_TO_INT16, Cast.INT16_TO_UINT16 };
    //        case TType.Kind.UINT32:
    //        case TType.Kind.POINTER:
    //            return new List<Cast> { Cast.INT32_TO_UINT32 };
    //        case TType.Kind.FLOAT32:
    //            return new List<Cast> { Cast.INT32_TO_FLOAT32 };
    //        case TType.Kind.FLOAT64:
    //            return new List<Cast> { Cast.INT32_TO_FLOAT64 };
    //        default:
    //            return null;
    //        }
    //    default:
    //        return null;
    //    }
    //}

    //public List<Cast> SemantConversionFromUnsignedIntegral(TType.Kind from, TType.Kind to) {
    //    switch (from) {
    //    case TType.Kind.UINT8:
    //        switch (to) {
    //        case TType.Kind.INT8:
    //            return new List<Cast> { Cast.UINT8_TO_INT8 };
    //        case TType.Kind.INT16:
    //            return new List<Cast> { Cast.UINT8_TO_UINT16, Cast.UINT16_TO_INT16 };
    //        case TType.Kind.INT32:
    //            return new List<Cast> { Cast.UINT8_TO_UINT32, Cast.UINT32_TO_INT32 };
    //        case TType.Kind.UINT16:
    //            return new List<Cast> { Cast.UINT8_TO_UINT16 };
    //        case TType.Kind.UINT32:
    //        case TType.Kind.POINTER:
    //            return new List<Cast> { Cast.UINT8_TO_UINT32 };
    //        case TType.Kind.FLOAT32:
    //            return new List<Cast> { Cast.UINT8_TO_UINT32, Cast.UINT32_TO_INT32, Cast.INT32_TO_FLOAT32 };
    //        case TType.Kind.FLOAT64:
    //            return new List<Cast> { Cast.UINT8_TO_UINT32, Cast.UINT32_TO_INT32, Cast.INT32_TO_FLOAT64 };
    //        default:
    //            return null;
    //        }
    //    case TType.Kind.UINT16:
    //        switch (to) {
    //        case TType.Kind.INT8:
    //            return new List<Cast> { Cast.UINT16_TO_UINT8, Cast.UINT8_TO_INT8 };
    //        case TType.Kind.INT16:
    //            return new List<Cast> { Cast.UINT16_TO_INT16 };
    //        case TType.Kind.INT32:
    //            return new List<Cast> { Cast.UINT16_TO_UINT32, Cast.UINT32_TO_INT32 };
    //        case TType.Kind.UINT8:
    //            return new List<Cast> { Cast.UINT16_TO_UINT8 };
    //        case TType.Kind.UINT32:
    //        case TType.Kind.POINTER:
    //            return new List<Cast> { Cast.UINT16_TO_UINT32 };
    //        case TType.Kind.FLOAT32:
    //            return new List<Cast> { Cast.UINT16_TO_UINT32, Cast.UINT32_TO_INT32, Cast.INT32_TO_FLOAT32 };
    //        case TType.Kind.FLOAT64:
    //            return new List<Cast> { Cast.UINT16_TO_UINT32, Cast.UINT32_TO_INT32, Cast.INT32_TO_FLOAT64 };
    //        default:
    //            return null;
    //        }
    //    case TType.Kind.UINT32:
    //        switch (to) {
    //        case TType.Kind.INT8:
    //            return new List<Cast> { Cast.UINT32_TO_UINT8, Cast.UINT8_TO_INT8 };
    //        case TType.Kind.INT16:
    //            return new List<Cast> { Cast.UINT32_TO_UINT16, Cast.UINT16_TO_INT16 };
    //        case TType.Kind.INT32:
    //            return new List<Cast> { Cast.UINT32_TO_INT32 };
    //        case TType.Kind.UINT8:
    //            return new List<Cast> { Cast.UINT32_TO_UINT8 };
    //        case TType.Kind.UINT16:
    //            return new List<Cast> { Cast.UINT32_TO_UINT16 };
    //        case TType.Kind.POINTER:
    //            return new List<Cast>();
    //        case TType.Kind.FLOAT32:
    //            return new List<Cast> { Cast.UINT32_TO_INT32, Cast.INT32_TO_FLOAT32 };
    //        case TType.Kind.FLOAT64:
    //            return new List<Cast> { Cast.UINT32_TO_INT32, Cast.INT32_TO_FLOAT64 };
    //        default:
    //            return null;
    //        }
    //    default:
    //        return null;
    //    }
    //}

    //public List<Cast> SemantConversionFromFloat(TType.Kind from, TType.Kind to) {
    //    switch (from) {
    //    case TType.Kind.FLOAT32:
    //        switch (to) {
    //        case TType.Kind.INT8:
    //            return new List<Cast> { Cast.FLOAT32_TO_INT32, Cast.INT32_TO_INT8 };
    //        case TType.Kind.INT16:
    //            return new List<Cast> { Cast.FLOAT32_TO_INT32, Cast.INT32_TO_INT16 };
    //        case TType.Kind.INT32:
    //            return new List<Cast> { Cast.FLOAT32_TO_INT32 };
    //        case TType.Kind.UINT8:
    //            return new List<Cast> { Cast.FLOAT32_TO_INT32, Cast.INT32_TO_INT8, Cast.INT8_TO_UINT8 };
    //        case TType.Kind.UINT16:
    //            return new List<Cast> { Cast.FLOAT32_TO_INT32, Cast.INT32_TO_INT16, Cast.INT16_TO_UINT16 };
    //        case TType.Kind.UINT32:
    //            return new List<Cast> { Cast.FLOAT32_TO_INT32, Cast.INT32_TO_UINT32 };
    //        case TType.Kind.FLOAT64:
    //            return new List<Cast> { Cast.FLOAT32_TO_FLOAT64 };
    //        default:
    //            return null;
    //        }
    //    case TType.Kind.FLOAT64:
    //        switch (to) {
    //        case TType.Kind.INT8:
    //            return new List<Cast> { Cast.FLOAT64_TO_FLOAT32, Cast.FLOAT32_TO_INT32, Cast.INT32_TO_INT8 };
    //        case TType.Kind.INT16:
    //            return new List<Cast> { Cast.FLOAT64_TO_FLOAT32, Cast.FLOAT32_TO_INT32, Cast.INT32_TO_INT16 };
    //        case TType.Kind.INT32:
    //            return new List<Cast> { Cast.FLOAT64_TO_INT32 };
    //        case TType.Kind.UINT8:
    //            return new List<Cast> { Cast.FLOAT64_TO_INT32, Cast.INT32_TO_INT8, Cast.INT8_TO_UINT8 };
    //        case TType.Kind.UINT16:
    //            return new List<Cast> { Cast.FLOAT64_TO_INT32, Cast.INT32_TO_INT16, Cast.INT16_TO_UINT16 };
    //        case TType.Kind.UINT32:
    //            return new List<Cast> { Cast.FLOAT64_TO_INT32, Cast.INT32_TO_UINT32 };
    //        case TType.Kind.FLOAT32:
    //            return new List<Cast> { Cast.FLOAT64_TO_FLOAT32 };
    //        default:
    //            return null;
    //        }
    //    default:
    //        return null;
    //    }
    //}

    //public List<Cast> SemantConversionFromPointer(TType.Kind from, TType.Kind to) {
    //    switch (from) {
    //    case TType.Kind.POINTER:
    //        switch (to) {
    //        case TType.Kind.INT8:
    //            return new List<Cast> { Cast.UINT32_TO_UINT8, Cast.UINT8_TO_INT8 };
    //        case TType.Kind.INT16:
    //            return new List<Cast> { Cast.UINT32_TO_UINT16, Cast.UINT16_TO_INT16 };
    //        case TType.Kind.INT32:
    //            return new List<Cast> { Cast.UINT32_TO_INT32 };
    //        case TType.Kind.UINT8:
    //            return new List<Cast> { Cast.UINT32_TO_UINT8 };
    //        case TType.Kind.UINT16:
    //            return new List<Cast> { Cast.UINT32_TO_UINT16 };
    //        case TType.Kind.UINT32:
    //            return new List<Cast>();
    //        default:
    //            return null;
    //        }
    //    default:
    //        return null;
    //    }
    //}

}

// Finished.
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
        bool c1 = lhs.type.is_const;
        bool c2 = rhs.type.is_const;
        bool v1 = lhs.type.is_volatile;
        bool v2 = rhs.type.is_volatile;
        bool is_const = c1 || c2;
        bool is_volatile = v1 || v2;

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

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    SemantUsualArithmeticConversion(ref lhs, ref rhs);
    //    return scope;
    //}
}

// Finished.
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
        bool c1 = lhs.type.is_const;
        bool c2 = rhs.type.is_const;
        bool v1 = lhs.type.is_volatile;
        bool v2 = rhs.type.is_volatile;
        bool is_const = c1 || c2;
        bool is_volatile = v1 || v2;

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

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    SemantUsualArithmeticConversion(ref lhs, ref rhs);
    //    return scope;
    //}

}

// Finished.
public class Modulo : Expression {
    public Modulo(Expression _lhs, Expression _rhs) {
        mod_lhs = _lhs;
        mod_rhs = _rhs;
    }
    public Expression mod_lhs;
    public Expression mod_rhs;

    public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
        Tuple<AST.Env, AST.Expr> r_lhs = mod_lhs.GetExpr(env);
        env = r_lhs.Item1;
        AST.Expr lhs = r_lhs.Item2;

        Tuple<AST.Env, AST.Expr> r_rhs = mod_rhs.GetExpr(env);
        env = r_rhs.Item1;
        AST.Expr rhs = r_rhs.Item2;

        Tuple<AST.Expr, AST.Expr, AST.ExprType.EnumExprType> r_cast = AST.TypeCast.UsualArithmeticConversion(lhs, rhs);
        lhs = r_cast.Item1;
        rhs = r_cast.Item2;
        bool c1 = lhs.type.is_const;
        bool c2 = rhs.type.is_const;
        bool v1 = lhs.type.is_volatile;
        bool v2 = rhs.type.is_volatile;
        bool is_const = c1 || c2;
        bool is_volatile = v1 || v2;

        AST.ExprType.EnumExprType enum_type = r_cast.Item3;

        AST.Expr expr;
        if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
            switch (enum_type) {
            case AST.ExprType.EnumExprType.ULONG:
                expr = new AST.ConstULong(((AST.ConstULong)lhs).value % ((AST.ConstULong)rhs).value);
                break;
            case AST.ExprType.EnumExprType.LONG:
                expr = new AST.ConstLong(((AST.ConstLong)lhs).value % ((AST.ConstLong)rhs).value);
                break;
            default:
                Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
                return null;
            }

        } else {
            switch (enum_type) {
            case AST.ExprType.EnumExprType.ULONG:
                expr = new AST.Modulo(lhs, rhs, new AST.TULong(is_const, is_volatile));
                break;
            case AST.ExprType.EnumExprType.LONG:
                expr = new AST.Modulo(lhs, rhs, new AST.TLong(is_const, is_volatile));
                break;
            default:
                Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
                return null;
            }
        }

        return new Tuple<AST.Env, AST.Expr>(env, expr);

    }

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    if (!lhs.type.IsInt() || !rhs.type.IsInt()) {
    //        Log.SemantError("Error: modulo expected integral type.");
    //    }
    //    SemantUsualArithmeticConversion(ref lhs, ref rhs);
    //    return scope;
    //}
}

// Finished.
public class Addition : Expression {
    public Addition(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);

    //    // ptr + ptr : error!
    //    if (lhs.type.kind == TType.Kind.POINTER && rhs.type.kind == TType.Kind.POINTER) {
    //        Log.SemantError("Error: you cannot add two pointers.");
    //    }

    //    // ptr + int
    //    if (lhs.type.kind == TType.Kind.POINTER) {
    //        if (!rhs.type.IsInt()) {
    //            Log.SemantError("Error: must add integral to pointer");
    //        }
    //        rhs = new TypeCast(new TInt32(), rhs);
    //        type = lhs.type;
    //        return scope;
    //    }

    //    // int + ptr
    //    if (rhs.type.kind == TType.Kind.POINTER) {
    //        if (!lhs.type.IsInt()) {
    //            Log.SemantError("Error: must add integral to pointer");
    //        }
    //        lhs = new TypeCast(new TInt32(), lhs);
    //        type = rhs.type;
    //        return scope;
    //    }

    //    // arith + arith
    //    SemantUsualArithmeticConversion(ref lhs, ref rhs);

    //    return scope;
    //}

    public Expression lhs;
    public Expression rhs;

}

// Finished.
public class Subtraction : Expression {
    public Subtraction(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);

    //    // ptr - ptr
    //    if (lhs.type.kind == TType.Kind.POINTER && rhs.type.kind == TType.Kind.POINTER) {
    //        if (lhs.type.SizeOf() != rhs.type.SizeOf()) {
    //            Log.SemantError("Error: you cannot subtract two pointers that don't conform.");
    //        }
    //        type = new TInt32();
    //        return scope;
    //    }

    //    // ptr - int
    //    if (lhs.type.kind == TType.Kind.POINTER) {
    //        if (!rhs.type.IsInt()) {
    //            Log.SemantError("Error: must subtract integral from pointer");
    //        }
    //        rhs = new TypeCast(new TInt32(), rhs);
    //        type = lhs.type;
    //        return scope;
    //    }

    //    // arith - arith
    //    SemantUsualArithmeticConversion(ref lhs, ref rhs);

    //    return scope;
    //}

    public Expression lhs;
    public Expression rhs;
}

// Finished.
public class LeftShift : Expression {
    public LeftShift(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    if (!lhs.type.IsInt() || !rhs.type.IsInt()) {
    //        Log.SemantError("Error: expected integral operands.");
    //    }
    //    rhs = new TypeCast(new TInt32(), rhs);
    //    type = lhs.type;
    //    return scope;
    //}
}

// RightShift
// ==========
// requires integral type
public class RightShift : Expression {
    public RightShift(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    if (!lhs.type.IsInt() || !rhs.type.IsInt()) {
    //        Log.SemantError("Error: expected integral operands.");
    //    }
    //    rhs = new TypeCast(new TInt32(), rhs);
    //    type = lhs.type;
    //    return scope;
    //}

}

// Finished.
public class LessThan : Expression {
    public LessThan(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    SemantPointerOrArithmeticConversion(ref lhs, ref rhs);
    //    return scope;
    //}
}

// Finished.
public class LessEqualThan : Expression {
    public LessEqualThan(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    SemantPointerOrArithmeticConversion(ref lhs, ref rhs);
    //    return scope;
    //}
    public Expression lhs;
    public Expression rhs;
}

// Finished.
public class GreaterThan : Expression {
    public GreaterThan(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    SemantPointerOrArithmeticConversion(ref lhs, ref rhs);
    //    return scope;
    //}
    public Expression lhs;
    public Expression rhs;
}

// Finished.
public class GreaterEqualThan : Expression {
    public GreaterEqualThan(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    SemantPointerOrArithmeticConversion(ref lhs, ref rhs);
    //    return scope;
    //}
    public Expression lhs;
    public Expression rhs;
}

// Equal
// =====
// requires arithmetic or pointer type
public class Equal : Expression {
    public Equal(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    SemantPointerOrArithmeticConversion(ref lhs, ref rhs);
    //    return scope;
    //}
    public Expression lhs;
    public Expression rhs;
}

// NotEqual
// ========
// requires arithmetic or pointer type
public class NotEqual : Expression {
    public NotEqual(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    SemantPointerOrArithmeticConversion(ref lhs, ref rhs);
    //    return scope;
    //}
    public Expression lhs;
    public Expression rhs;
}

// BitwiseAnd
// ==========
// requires integral type
public class BitwiseAnd : Expression {
    public BitwiseAnd(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    SemantIntegralConversion(ref lhs, ref rhs);
    //    return scope;
    //}
    public Expression lhs;
    public Expression rhs;
}

// BitwiseAnd
// ==========
// requires integral type
public class Xor : Expression {
    public Xor(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    SemantIntegralConversion(ref lhs, ref rhs);
    //    return scope;
    //}
    public Expression lhs;
    public Expression rhs;
}

// BitwiseOr
// =========
// requires integral type
public class BitwiseOr : Expression {
    public BitwiseOr(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    SemantIntegralConversion(ref lhs, ref rhs);
    //    return scope;
    //}
}

// LogicalAnd
// ==========
// requires integral or pointer type
public class LogicalAnd : Expression {
    public LogicalAnd(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    SemantIntegralConversion(ref lhs, ref rhs);
    //    return scope;
    //}
    public Expression lhs;
    public Expression rhs;
}

// LogicalOr
// =========
// requires integral or pointer type
public class LogicalOr : Expression {
    public LogicalOr(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = lhs.Semant(scope);
    //    scope = rhs.Semant(scope);
    //    SemantIntegralConversion(ref lhs, ref rhs);
    //    return scope;
    //}
    public Expression lhs;
    public Expression rhs;

}
