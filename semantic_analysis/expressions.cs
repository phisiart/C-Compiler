using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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

public class Expression : ASTNode {
    public TType type;
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
}


public class Variable : Expression {
    public Variable(String _name) {
        name = _name;
    }

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        Symbol symbol = scope.FindSymbol(name);
        if (symbol == null) {
            Log.SemantError("Error: cannot find variable " + name);
        }

        if (symbol.kind == Symbol.Kind.TYPEDEF) {
            Log.SemantError("Error: expected a variable, but found a type");
        }

        type = symbol.type;

        return scope;
    }

    public String name;
}


public class Constant : Expression {}

public class ConstChar : Constant {
    public ConstChar(Char _val) {
        val = _val;
    }

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        type = new TChar();
        return scope;
    }

    public Char val;
}

public class ConstFloat : Constant {
    public ConstFloat(Double _val, FloatType _float_type) {
        val = _val;
        float_type = _float_type;
    }

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        switch (float_type) {
        case FloatType.F:
            type = new TFloat();
            break;
        case FloatType.LF:
            type = new TLongDouble();
            break;
        case FloatType.NONE:
            type = new TDouble();
            break;
        }
        return scope;
    }

    public FloatType float_type;
    public Double val;
}

public class ConstInt : Constant {
    public ConstInt(long _val, IntType _int_type) {
        val = _val;
        int_type = _int_type;
    }

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        switch (int_type) {
        case IntType.NONE:
            type = new TInt();
            break;
        case IntType.L:
            type = new TLong();
            break;
        case IntType.U:
            type = new TUInt();
            break;
        case IntType.UL:
            type = new TULong();
            break;
        }
        return scope;
    }
    
    public IntType int_type;

    public long val;
}

public class StringLiteral : Expression {
    public StringLiteral(String _val) {
        val = _val;
    }

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        type = new TPointer(new TChar());
        type.is_const = true;
        return scope;
    }

    public String val;
}


public class AssignmentList : Expression {
    public AssignmentList(List<Expression> _exprs) {
        exprs = _exprs;
    }
    public List<Expression> exprs;
}


public class ConditionalExpression : Expression {
    public ConditionalExpression(Expression _cond, Expression _true_expr, Expression _false_expr) {
        cond = _cond;
        true_expr = _true_expr;
        false_expr = _false_expr;
    }
    public Expression cond;
    public Expression true_expr;
    public Expression false_expr;
}


public class Assignment : Expression {
    public Assignment(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;
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

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;

        scope = func.Semant(scope);
        foreach (Expression arg in args) {
            scope = arg.Semant(scope);
        }

        // args are assigned

        return scope;
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

//public class PointerAttribute : Expression {
//    public PointerAttribute(Expression _expr, Variable _attrib) {
//        expr = _expr;
//        attrib = _attrib;
//    }
//    public Expression expr;
//    public Variable attrib;
//}

public class Increment : Expression {
    public Increment(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        scope = expr.Semant(scope);
        if (!expr.type.IsScalar()) {
            Log.SemantError("Error: increment expected scalar type.");
        }
        type = expr.type;
        return scope;
    }
}

public class Decrement : Expression {
    public Decrement(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        scope = expr.Semant(scope);
        if (!expr.type.IsScalar()) {
            Log.SemantError("Error: decrement expected scalar type.");
        }
        type = expr.type;
        return scope;
    }
}


//public class ArgumentList : Expression {
//    public ArgumentList(List<Expression> _exprs) {
//        exprs = _exprs;
//    }
//    public List<Expression> exprs;
//}



public class SizeofType : Expression {
    public SizeofType(TypeName _type_name) {
        type_name = _type_name;
    }

    // after parsing
    public TypeName type_name;

    // after semant
    public Int64 size;

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        scope = type_name.Semant(scope);
        size = type_name.type.SizeOf();
        return scope;
    }

}

public class SizeofExpression : Expression {
    public SizeofExpression(Expression _expr) {
        expr = _expr;
    }

    // after parsing
    public Expression expr;

    // after semant
    public Int64 size;

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        size = expr.type.SizeOf();
        return scope;
    }

}

public class PrefixIncrement : Expression {
    public PrefixIncrement(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        scope = expr.Semant(scope);
        if (!expr.type.IsScalar()) {
            Log.SemantError("Error: increment expected scalar type.");
        }
        type = expr.type;
        return scope;
    }

}

public class PrefixDecrement : Expression {
    public PrefixDecrement(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        scope = expr.Semant(scope);
        if (!expr.type.IsScalar()) {
            Log.SemantError("Error: decrement expected scalar type.");
        }
        type = expr.type;
        return scope;
    }

}

public class Reference : Expression {
    public Reference(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        scope = expr.Semant(scope);
        type = new TPointer(expr.type);
        return scope;
    }

}

public class Dereference : Expression {
    public Dereference(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        scope = expr.Semant(scope);
        if (expr.type.kind != TType.Kind.POINTER) {
            Log.SemantError("Error: dereferencing expected pointer type.");
        }
        TPointer ptype = (TPointer)expr.type;
        type = ptype.ref_t;
        return scope;
    }

}

public class Positive : Expression {
    public Positive(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        scope = expr.Semant(scope);
        return scope;
    }
}

public class Negative : Expression {
    public Negative(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        scope = expr.Semant(scope);
        return scope;
    }
}

public class BitwiseNot : Expression {
    public BitwiseNot(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}

public class Not : Expression {
    public Not(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}


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
        SemantConversion();
    }

    // after parsing
    public TypeName type_name;
    public Expression expr;

    // after semant
    public List<Cast> casts;

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        scope = expr.Semant(scope);
        scope = type_name.Semant(scope);
        type = type_name.type;
        return SemantConversion();
    }

    public ScopeSandbox SemantConversion() {

        TType.Kind kind_from = expr.type.kind;
        TType.Kind kind_to = type.kind;

        if (kind_from == kind_to) {
            casts = new List<Cast>();
            return scope;
        }

        if ((casts = SemantConversionFromSignedIntegral(kind_from, kind_to)) != null) {
            return scope;
        }

        if ((casts = SemantConversionFromUnsignedIntegral(kind_from, kind_to)) != null) {
            return scope;
        }

        if ((casts = SemantConversionFromFloat(kind_from, kind_to)) != null) {
            return scope;
        }

        if ((casts = SemantConversionFromPointer(kind_from, kind_to)) != null) {
            return scope;
        }

        Log.SemantError("Error: cannot convert from " + kind_from.ToString() + " to " + kind_to.ToString());
        return scope;

    }

    public enum Cast {
        INT8_TO_INT16,
        INT8_TO_INT32,
        INT8_TO_UINT8,

        INT16_TO_INT8,
        INT16_TO_INT32,
        INT16_TO_UINT16,

        INT32_TO_INT8,
        INT32_TO_INT16,
        INT32_TO_UINT32,
        INT32_TO_FLOAT32,
        INT32_TO_FLOAT64,

        UINT8_TO_UINT16,
        UINT8_TO_UINT32,
        UINT8_TO_INT8,

        UINT16_TO_UINT8,
        UINT16_TO_UINT32,
        UINT16_TO_INT16,

        UINT32_TO_UINT8,
        UINT32_TO_UINT16,
        UINT32_TO_INT32,
        UINT32_TO_FLOAT32,
        UINT32_TO_FLOAT64,

        FLOAT32_TO_INT32,
        FLOAT32_TO_FLOAT64,

        FLOAT64_TO_FLOAT32,
        FLOAT64_TO_INT32,

    }

    public List<Cast> SemantConversionFromSignedIntegral(TType.Kind from, TType.Kind to) {
        switch (from) {
        case TType.Kind.INT8:
            switch (to) {
            case TType.Kind.INT16:
                return new List<Cast> { Cast.INT8_TO_INT16 };
            case TType.Kind.INT32:
                return new List<Cast> { Cast.INT8_TO_INT32 };
            case TType.Kind.UINT8:
                return new List<Cast> { Cast.INT8_TO_UINT8 };
            case TType.Kind.UINT16:
                return new List<Cast> { Cast.INT8_TO_INT16, Cast.INT16_TO_UINT16 };
            case TType.Kind.UINT32:
            case TType.Kind.POINTER:
                return new List<Cast> { Cast.INT8_TO_INT32, Cast.INT32_TO_UINT32 };
            case TType.Kind.FLOAT32:
                return new List<Cast> { Cast.INT8_TO_INT32, Cast.INT32_TO_FLOAT32 };
            case TType.Kind.FLOAT64:
                return new List<Cast> { Cast.INT8_TO_INT32, Cast.INT32_TO_FLOAT64 };
            default:
                return null;
            }
        case TType.Kind.INT16:
            switch (to) {
            case TType.Kind.INT8:
                return new List<Cast> { Cast.INT16_TO_INT8 };
            case TType.Kind.INT32:
                return new List<Cast> { Cast.INT16_TO_INT32 };
            case TType.Kind.UINT8:
                return new List<Cast> { Cast.INT16_TO_INT8, Cast.INT8_TO_UINT8 };
            case TType.Kind.UINT16:
                return new List<Cast> { Cast.INT16_TO_UINT16 };
            case TType.Kind.UINT32:
            case TType.Kind.POINTER:
                return new List<Cast> { Cast.INT16_TO_INT32, Cast.INT32_TO_UINT32 };
            case TType.Kind.FLOAT32:
                return new List<Cast> { Cast.INT16_TO_INT32, Cast.INT32_TO_FLOAT32 };
            case TType.Kind.FLOAT64:
                return new List<Cast> { Cast.INT16_TO_INT32, Cast.INT32_TO_FLOAT64 };
            default:
                return null;
            }
        case TType.Kind.INT32:
            switch (to) {
            case TType.Kind.INT8:
                return new List<Cast> { Cast.INT32_TO_INT8 };
            case TType.Kind.INT16:
                return new List<Cast> { Cast.INT32_TO_INT16 };
            case TType.Kind.UINT8:
                return new List<Cast> { Cast.INT32_TO_INT8, Cast.INT8_TO_UINT8 };
            case TType.Kind.UINT16:
                return new List<Cast> { Cast.INT32_TO_INT16, Cast.INT16_TO_UINT16 };
            case TType.Kind.UINT32:
            case TType.Kind.POINTER:
                return new List<Cast> { Cast.INT32_TO_UINT32 };
            case TType.Kind.FLOAT32:
                return new List<Cast> { Cast.INT32_TO_FLOAT32 };
            case TType.Kind.FLOAT64:
                return new List<Cast> { Cast.INT32_TO_FLOAT64 };
            default:
                return null;
            }
        default:
            return null;
        }
    }

    public List<Cast> SemantConversionFromUnsignedIntegral(TType.Kind from, TType.Kind to) {
        switch (from) {
        case TType.Kind.UINT8:
            switch (to) {
            case TType.Kind.INT8:
                return new List<Cast> { Cast.UINT8_TO_INT8 };
            case TType.Kind.INT16:
                return new List<Cast> { Cast.UINT8_TO_UINT16, Cast.UINT16_TO_INT16 };
            case TType.Kind.INT32:
                return new List<Cast> { Cast.UINT8_TO_UINT32, Cast.UINT32_TO_INT32 };
            case TType.Kind.UINT16:
                return new List<Cast> { Cast.UINT8_TO_UINT16 };
            case TType.Kind.UINT32:
            case TType.Kind.POINTER:
                return new List<Cast> { Cast.UINT8_TO_UINT32 };
            case TType.Kind.FLOAT32:
                return new List<Cast> { Cast.UINT8_TO_UINT32, Cast.UINT32_TO_INT32, Cast.INT32_TO_FLOAT32 };
            case TType.Kind.FLOAT64:
                return new List<Cast> { Cast.UINT8_TO_UINT32, Cast.UINT32_TO_INT32, Cast.INT32_TO_FLOAT64 };
            default:
                return null;
            }
        case TType.Kind.UINT16:
            switch (to) {
            case TType.Kind.INT8:
                return new List<Cast> { Cast.UINT16_TO_UINT8, Cast.UINT8_TO_INT8 };
            case TType.Kind.INT16:
                return new List<Cast> { Cast.UINT16_TO_INT16 };
            case TType.Kind.INT32:
                return new List<Cast> { Cast.UINT16_TO_UINT32, Cast.UINT32_TO_INT32 };
            case TType.Kind.UINT8:
                return new List<Cast> { Cast.UINT16_TO_UINT8 };
            case TType.Kind.UINT32:
            case TType.Kind.POINTER:
                return new List<Cast> { Cast.UINT16_TO_UINT32 };
            case TType.Kind.FLOAT32:
                return new List<Cast> { Cast.UINT16_TO_UINT32, Cast.UINT32_TO_INT32, Cast.INT32_TO_FLOAT32 };
            case TType.Kind.FLOAT64:
                return new List<Cast> { Cast.UINT16_TO_UINT32, Cast.UINT32_TO_INT32, Cast.INT32_TO_FLOAT64 };
            default:
                return null;
            }
        case TType.Kind.UINT32:
            switch (to) {
            case TType.Kind.INT8:
                return new List<Cast> { Cast.UINT32_TO_UINT8, Cast.UINT8_TO_INT8 };
            case TType.Kind.INT16:
                return new List<Cast> { Cast.UINT32_TO_UINT16, Cast.UINT16_TO_INT16 };
            case TType.Kind.INT32:
                return new List<Cast> { Cast.UINT32_TO_INT32 };
            case TType.Kind.UINT8:
                return new List<Cast> { Cast.UINT32_TO_UINT8 };
            case TType.Kind.UINT16:
                return new List<Cast> { Cast.UINT32_TO_UINT16 };
            case TType.Kind.POINTER:
                return new List<Cast>();
            case TType.Kind.FLOAT32:
                return new List<Cast> { Cast.UINT32_TO_INT32, Cast.INT32_TO_FLOAT32 };
            case TType.Kind.FLOAT64:
                return new List<Cast> { Cast.UINT32_TO_INT32, Cast.INT32_TO_FLOAT64 };
            default:
                return null;
            }
        default:
            return null;
        }
    }

    public List<Cast> SemantConversionFromFloat(TType.Kind from, TType.Kind to) {
        switch (from) {
        case TType.Kind.FLOAT32:
            switch (to) {
            case TType.Kind.INT8:
                return new List<Cast> { Cast.FLOAT32_TO_INT32, Cast.INT32_TO_INT8 };
            case TType.Kind.INT16:
                return new List<Cast> { Cast.FLOAT32_TO_INT32, Cast.INT32_TO_INT16 };
            case TType.Kind.INT32:
                return new List<Cast> { Cast.FLOAT32_TO_INT32 };
            case TType.Kind.UINT8:
                return new List<Cast> { Cast.FLOAT32_TO_INT32, Cast.INT32_TO_INT8, Cast.INT8_TO_UINT8 };
            case TType.Kind.UINT16:
                return new List<Cast> { Cast.FLOAT32_TO_INT32, Cast.INT32_TO_INT16, Cast.INT16_TO_UINT16 };
            case TType.Kind.UINT32:
                return new List<Cast> { Cast.FLOAT32_TO_INT32, Cast.INT32_TO_UINT32 };
            case TType.Kind.FLOAT64:
                return new List<Cast> { Cast.FLOAT32_TO_FLOAT64 };
            default:
                return null;
            }
        case TType.Kind.FLOAT64:
            switch (to) {
            case TType.Kind.INT8:
                return new List<Cast> { Cast.FLOAT64_TO_FLOAT32, Cast.FLOAT32_TO_INT32, Cast.INT32_TO_INT8 };
            case TType.Kind.INT16:
                return new List<Cast> { Cast.FLOAT64_TO_FLOAT32, Cast.FLOAT32_TO_INT32, Cast.INT32_TO_INT16 };
            case TType.Kind.INT32:
                return new List<Cast> { Cast.FLOAT64_TO_INT32 };
            case TType.Kind.UINT8:
                return new List<Cast> { Cast.FLOAT64_TO_INT32, Cast.INT32_TO_INT8, Cast.INT8_TO_UINT8 };
            case TType.Kind.UINT16:
                return new List<Cast> { Cast.FLOAT64_TO_INT32, Cast.INT32_TO_INT16, Cast.INT16_TO_UINT16 };
            case TType.Kind.UINT32:
                return new List<Cast> { Cast.FLOAT64_TO_INT32, Cast.INT32_TO_UINT32 };
            case TType.Kind.FLOAT32:
                return new List<Cast> { Cast.FLOAT64_TO_FLOAT32 };
            default:
                return null;
            }
        default:
            return null;
        }
    }

    public List<Cast> SemantConversionFromPointer(TType.Kind from, TType.Kind to) {
        switch (from) {
        case TType.Kind.POINTER:
            switch (to) {
            case TType.Kind.INT8:
                return new List<Cast> { Cast.UINT32_TO_UINT8, Cast.UINT8_TO_INT8 };
            case TType.Kind.INT16:
                return new List<Cast> { Cast.UINT32_TO_UINT16, Cast.UINT16_TO_INT16 };
            case TType.Kind.INT32:
                return new List<Cast> { Cast.UINT32_TO_INT32 };
            case TType.Kind.UINT8:
                return new List<Cast> { Cast.UINT32_TO_UINT8 };
            case TType.Kind.UINT16:
                return new List<Cast> { Cast.UINT32_TO_UINT16 };
            case TType.Kind.UINT32:
                return new List<Cast>();
            default:
                return null;
            }
        default:
            return null;
        }
    }

}


public class Multiplication : Expression {
    public Multiplication(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

public class Division : Expression {
    public Division(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

public class Modulo : Expression {
    public Modulo(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


public class Addition : Expression {
    public Addition(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        scope = lhs.Semant(scope);
        scope = rhs.Semant(scope);
        
        SemantUsualArithmeticConversion(ref lhs, ref rhs);
        
        return scope;
    }

    public Expression lhs;
    public Expression rhs;

}

public class Subtraction : Expression {
    public Subtraction(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        scope = lhs.Semant(scope);
        scope = rhs.Semant(scope);

        SemantUsualArithmeticConversion(ref lhs, ref rhs);

        return scope;
    }

    public Expression lhs;
    public Expression rhs;
}


public class LeftShift : Expression {
    public LeftShift(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

public class RightShift : Expression {
    public RightShift(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


public class LessThan : Expression {
    public LessThan(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

public class LessEqualThan : Expression {
    public LessEqualThan(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

public class GreaterThan : Expression {
    public GreaterThan(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

public class GreaterEqualThan : Expression {
    public GreaterEqualThan(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


public class Equal : Expression {
    public Equal(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

public class NotEqual : Expression {
    public NotEqual(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


public class BitwiseAnd : Expression {
    public BitwiseAnd(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


public class Xor : Expression {
    public Xor(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


public class BitwiseOr : Expression {
    public BitwiseOr(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        lhs.scope = _scope;
        rhs.scope = _scope;
        lhs.type.kind
    }
}


public class LogicalAnd : Expression {
    public LogicalAnd(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


public class LogicalOr : Expression {
    public LogicalOr(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }

    public Expression lhs;
    public Expression rhs;

}
