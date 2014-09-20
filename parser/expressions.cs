using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

public class Expression : ASTNode {}

// primary_expression: identifier           /* Variable : Expression */
//
//                   | constant             /* ConstChar : Expression
//                                             ConstFloat : Expression
//                                             ConstInt : Expression */
//
//                   | string_literal       /* StringLiteral : Expression */
//
//                   | '(' expression ')'   /* Expression */
// 
// RETURN: Expression
//
// FAILURE: null
// 
// NOTE:
// 1. This grammar is LL(1)
// 2. identifier shouldn't be previously defined as a typedef_name
//    this is to resolve the ambiguity of something like a * b
// 3. first set : id, const, string, '('
//
public class _primary_expression : PTNode {
    public static bool Test() {
        Expression expr;

        var src = Parser.GetTokensFromString("test_id");
        int current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("'h'");
        current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("3.0f");
        current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("10");
        current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("\"string\"");
        current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("(test_id)");
        current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        return true;
    }

    public static int Parse(List<Token> src, int begin, out Expression expr) {

        // 1. match identifier
        String var_name = Parser.GetIdentifierValue(src[begin]);
        if (var_name != null) {
            if (!ScopeEnvironment.HasTypedefName(var_name)) {
                expr = new Variable(var_name);
                return begin + 1;
            } else {
                expr = null;
                return -1;
            }
        }

        // 2. match const
        // 2.1. match char
        if (src[begin].type == TokenType.CHAR) {
            expr = new ConstChar(((TokenChar)src[begin]).val);
            return begin + 1;
        }

        // 2.2. match float
        if (src[begin].type == TokenType.FLOAT) {
            expr = new ConstFloat(((TokenFloat)src[begin]).val, ((TokenFloat)src[begin]).float_type);
            return begin + 1;
        }

        // 2.3. match int
        if (src[begin].type == TokenType.INT) {
            expr = new ConstInt(((TokenInt)src[begin]).val, ((TokenInt)src[begin]).int_type);
            return begin + 1;
        }

        // 3. match string literal
        if (src[begin].type == TokenType.STRING) {
            expr = new StringLiteral(((TokenString)src[begin]).val);
            return begin + 1;
        }

        // 4 & last. match '(' expression ')'
        // step 1. match '('
        if (!Parser.IsOperator(src[begin], OperatorVal.LPAREN)) {
            expr = null;
            return -1;
        }
        begin++;

        // step 2. match expression
        if ((begin = _expression.Parse(src, begin, out expr)) == -1) {
            expr = null;
            return -1;
        }

        // step 3. match ')'
        if (!Parser.IsOperator(src[begin], OperatorVal.RPAREN)) {
            expr = null;
            return -1;
        }
        begin++;

        return begin;

    }
}

public class Variable : Expression {
    public Variable(String _name) {
        name = _name;
    }
    public String name;
}

public class Constant : Expression {
}

public class ConstChar : Constant {
    public ConstChar(Char _val) {
        val = _val;
    }
    public Char val;
}

public class ConstFloat : Constant {
    public ConstFloat(Double _val, FloatType _float_type) {
        val = _val;
        float_type = _float_type;
    }
    public FloatType float_type;
    public Double val;
}

public class ConstInt : Constant {
    public ConstInt(long _val, IntType _int_type) {
        val = _val;
        int_type = _int_type;
    }
    public IntType int_type;
    public long val;
}

public class StringLiteral : Expression {
    public StringLiteral(String _val) {
        val = _val;
    }
    public String val;
}


// expression: assignment_expression < , assignment_expression >*
// [ note: it's okay if there is a lonely ',', just leave it be ]
public class _expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        node = null;
        Expression expr;
        List<Expression> exprs = new List<Expression>();
        int current = _assignment_expression.Parse(src, begin, out expr);
        if (current == -1) {
            return -1;
        }
        exprs.Add(expr);
        int saved;

        while (true) {
            if (Parser.IsCOMMA(src[current])) {
                saved = current;
                current++;
                current = _assignment_expression.Parse(src, current, out expr);
                if (current == -1) {
                    node = new AssignmentList(exprs);
                    return saved;
                }
                exprs.Add(expr);
            } else {
                node = new AssignmentList(exprs);
                return current;
            }
        }
    }
}

public class AssignmentList : Expression {
    public AssignmentList(List<Expression> _exprs) {
        exprs = _exprs;
    }
    public List<Expression> exprs;
}


// constant_expression: conditional_expression
// [ note: when declaring an array, the size should be a const ]
public class _constant_expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        return _conditional_expression.Parse(src, begin, out node);
    }
}


// conditional_expression: logical_or_expression < ? expression : conditional_expression >?
public class _conditional_expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        int current = _logical_or_expression.Parse(src, begin, out node);
        if (current == -1) {
            return -1;
        }

        if (!Parser.IsQuestionMark(src[current])) {
            return current;
        }
        current++;

        Expression true_expr;
        current = _expression.Parse(src, current, out true_expr);
        if (current == -1) {
            return -1;
        }

        if (!Parser.IsCOLON(src[current])) {
            return -1;
        }
        current++;

        Expression false_expr;
        current = _conditional_expression.Parse(src, current, out false_expr);
        if (current == -1) {
            return -1;
        }

        node = new ConditionalExpression(node, true_expr, false_expr);
        return current;
    }
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


// assignment_expression: conditional_expression
//                      | unary_expression assignment_operator assignment_expression
// [ note: assignment_operator is = *= /= %= += -= <<= >>= &= ^= |= ]
// [ note: how to predict which one to choose? ]
// [ note: unary_expression is a special type of conditional_expression ]
// [ note: first try unary ]
// first(conditional_expression) = first(cast_expression)
public class _assignment_expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        node = null;
        Expression lvalue;
        Expression rvalue;
        int current = _unary_expression.Parse(src, begin, out lvalue);
        if (current != -1) {
            if (src[current].type == TokenType.OPERATOR) {
                OperatorVal val = ((TokenOperator)src[current]).val;
                switch (val) {
                case OperatorVal.ASSIGN:
                    current++;
                    current = _assignment_expression.Parse(src, current, out rvalue);
                    if (current == -1) {
                        return -1;
                    }
                    node = new Assignment(lvalue, rvalue);
                    return current;

                case OperatorVal.MULTASSIGN:
                    current++;
                    current = _assignment_expression.Parse(src, current, out rvalue);
                    if (current == -1) {
                        return -1;
                    }
                    node = new MultAssign(lvalue, rvalue);
                    return current;

                case OperatorVal.DIVASSIGN:
                    current++;
                    current = _assignment_expression.Parse(src, current, out rvalue);
                    if (current == -1) {
                        return -1;
                    }
                    node = new DivAssign(lvalue, rvalue);
                    return current;

                case OperatorVal.MODASSIGN:
                    current++;
                    current = _assignment_expression.Parse(src, current, out rvalue);
                    if (current == -1) {
                        return -1;
                    }
                    node = new ModAssign(lvalue, rvalue);
                    return current;

                case OperatorVal.ADDASSIGN:
                    current++;
                    current = _assignment_expression.Parse(src, current, out rvalue);
                    if (current == -1) {
                        return -1;
                    }
                    node = new AddAssign(lvalue, rvalue);
                    return current;

                case OperatorVal.SUBASSIGN:
                    current++;
                    current = _assignment_expression.Parse(src, current, out rvalue);
                    if (current == -1) {
                        return -1;
                    }
                    node = new SubAssign(lvalue, rvalue);
                    return current;

                case OperatorVal.LSHIFTASSIGN:
                    current++;
                    current = _assignment_expression.Parse(src, current, out rvalue);
                    if (current == -1) {
                        return -1;
                    }
                    node = new LeftShiftAssign(lvalue, rvalue);
                    return current;

                case OperatorVal.RSHIFTASSIGN:
                    current++;
                    current = _assignment_expression.Parse(src, current, out rvalue);
                    if (current == -1) {
                        return -1;
                    }
                    node = new RightShiftAssign(lvalue, rvalue);
                    return current;

                case OperatorVal.ANDASSIGN:
                    current++;
                    current = _assignment_expression.Parse(src, current, out rvalue);
                    if (current == -1) {
                        return -1;
                    }
                    node = new BitwiseAndAssign(lvalue, rvalue);
                    return current;

                case OperatorVal.XORASSIGN:
                    current++;
                    current = _assignment_expression.Parse(src, current, out rvalue);
                    if (current == -1) {
                        return -1;
                    }
                    node = new XorAssign(lvalue, rvalue);
                    return current;

                case OperatorVal.ORASSIGN:
                    current++;
                    current = _assignment_expression.Parse(src, current, out rvalue);
                    if (current == -1) {
                        return -1;
                    }
                    node = new BitwiseOrAssign(lvalue, rvalue);
                    return current;

                default:
                    break;
                // node = lvalue;
                // return current;
                }
            }
        }

        return _conditional_expression.Parse(src, begin, out node);
    }
}

class Assignment : Expression {
    public Assignment(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;
}

class MultAssign : Expression {
    public MultAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;
}

class DivAssign : Expression {
    public DivAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;
}

class ModAssign : Expression {
    public ModAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;
}

class AddAssign : Expression {
    public AddAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;
}

class SubAssign : Expression {
    public SubAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;
}

class LeftShiftAssign : Expression {
    public LeftShiftAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;
}

class RightShiftAssign : Expression {
    public RightShiftAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;
}

class BitwiseAndAssign : Expression {
    public BitwiseAndAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;
}

class XorAssign : Expression {
    public XorAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;
}

class BitwiseOrAssign : Expression {
    public BitwiseOrAssign(Expression _lvalue, Expression _rvalue) {
        lvalue = _lvalue;
        rvalue = _rvalue;
    }
    public Expression lvalue;
    public Expression rvalue;
}


// postfix_expression: primary_expression                                       /* Expression */
//                   | postfix_expression '[' expression ']'                    /* ArrayElement */
//                   | postfix_expression '(' [argument_expression_list>]? ')'  /* FunctionCall */
//                   | postfix_expression '.' identifier                        /* Attribute */
//                   | postfix_expression '->' identifier                       /* PointerAttribute */
//                   | postfix_expression '++'                                  /* Increment */
//                   | postfix_expression '--'                                  /* Decrement */
//
// RETURN: Expression
//
// FAIL: null
//
// NOTE:
// 1. from this grammar we can see that postfix operators are of the highest priority
// 2. this is left-recursive
//
// MY SOLUTION:
// postfix_expression: primary_expression [ one of these postfixes ]*
//
public class _postfix_expression : PTNode {
    public static bool Test() {
        var src = Parser.GetTokensFromString("a");
        Expression expr;
        int current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("a[3]");
        current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("a(b)");
        current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("a.b");
        current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("a->b");
        current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("a++");
        current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("a--");
        current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("a++ -- -> b[3](c)");
        current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        return true;
    }
    
    public static int Parse(List<Token> src, int begin, out Expression expr) {

        // step 1. match primary_expression
        int current = _primary_expression.Parse(src, begin, out expr);
        if (current == -1) {
            expr = null;
            return -1;
        }

        // step 2. match postfixes
        while (true) {

            if (src[current].type != TokenType.OPERATOR) {
                return current;
            }

            OperatorVal val = ((TokenOperator)src[current]).val;
            switch (val) {
            case OperatorVal.LBRACKET:
                // '['
                current++;

                // 1. match expression
                Expression idx;
                current = _expression.Parse(src, current, out idx);
                if (current == -1) {
                    expr = null;
                    return -1;
                }

                // 2. match ']'
                if (!Parser.IsOperator(src[current], OperatorVal.RBRACKET)) {
                    expr = null;
                    return -1;
                }
                current++;

                // successful match
                expr = new ArrayElement(expr, idx);
                break;

            case OperatorVal.LPAREN:
                // '('
                current++;

                // 1. match arglist, if no match, assume empty arglist
                List<Expression> args;
                int saved = current;
                current = _argument_expression_list.Parse(src, current, out args);
                if (current == -1) {
                    args = new List<Expression>();
                    current = saved;
                }
                
                // 2. match ')'
                if (!Parser.IsOperator(src[current], OperatorVal.RPAREN)) {
                    expr = null;
                    return -1;
                }
                current++;

                // successful match
                expr = new FunctionCall(expr, args);
                break;

            case OperatorVal.PERIOD:
                // '.'
                current++;

                // match identifier
                if (src[current].type != TokenType.IDENTIFIER) {
                    expr = null;
                    return -1;
                }
                String attrib = ((TokenIdentifier)src[current]).val;
                current++;

                // successful match
                expr = new Attribute(expr, new Variable(attrib));
                break;

            case OperatorVal.RARROW:
                // '->'
                current++;

                if (src[current].type != TokenType.IDENTIFIER) {
                    return -1;
                }
                String pattrib = ((TokenIdentifier)src[current]).val;
                current++;

                // successful match
                expr = new PointerAttribute(expr, new Variable(pattrib));
                break;

            case OperatorVal.INC:
                // '++'
                current++;

                // successful match
                expr = new Increment(expr);
                break;

            case OperatorVal.DEC:
                // '--'

                current++;
                
                // successful match
                expr = new Decrement(expr);
                break;

            default:

                // no more postfix
                return current;

            } // case (val)

        } // while (true)

    }
}

class ArrayElement : Expression {
    public ArrayElement(Expression _var, Expression _idx) {
        var = _var;
        idx = _idx;
    }
    public Expression var;
    public Expression idx;
}

class FunctionCall : Expression {
    public FunctionCall(Expression _func, List<Expression> _args) {
        func = _func;
        args = _args;
    }
    public Expression func;
    public List<Expression> args;
}

class Attribute : Expression {
    public Attribute(Expression _expr, Variable _attrib) {
        expr = _expr;
        attrib = _attrib;
    }
    public Expression expr;
    public Variable attrib;
}

class PointerAttribute : Expression {
    public PointerAttribute(Expression _expr, Variable _attrib) {
        expr = _expr;
        attrib = _attrib;
    }
    public Expression expr;
    public Variable attrib;
}

class Increment : Expression {
    public Increment(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}

class Decrement : Expression {
    public Decrement(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}


// argument_expression_list: assignment_expression < , assignment_expression >*
class _argument_expression_list : PTNode {
    public static int Parse(List<Token> src, int begin, out List<Expression> node) {
        node = null;
        Expression expr;
        List<Expression> exprs = new List<Expression>();
        int current = _assignment_expression.Parse(src, begin, out expr);
        if (current == -1) {
            return -1;
        }
        exprs.Add(expr);
        int saved;

        while (true) {
            if (Parser.IsCOMMA(src[current])) {
                saved = current;
                current++;
                current = _assignment_expression.Parse(src, current, out expr);
                if (current == -1) {
                    node = exprs;
                    return saved;
                }
                exprs.Add(expr);
            } else {
                node = exprs;
                return current;
            }
        }
    }
}

class ArgumentList : Expression {
    public ArgumentList(List<Expression> _exprs) {
        exprs = _exprs;
    }
    public List<Expression> exprs;
}



// unary_expression: postfix_expression                     /* Expression */
//                 | ++ unary_expression                    /* PrefixIncrement */
//                 | -- unary_expression                    /* PrefixDecrement */
//                 | unary_operator cast_expression         /* Reference
//                                                             Dereference
//                                                             Positive
//                                                             Negative
//                                                             BitwiseNot
//                                                             Not */
//                 | sizeof unary_expression                /* SizeofExpression */
//                 | sizeof ( type_name )                   /* SizeofType */
//
// RETURN: Expression
//
// FAIL: null
//
// NOTE:
// 1. from this grammar, we can see that the 2nd priority operators are prefix unary operators
// 2. notice the last two productions, they form an ambiguity. we need to use environment
//    first try the type_name version
// 3. unary_operators are & | * | + | - | ~ | ! 
//
// first set = first(postfix_expression) + { ++ -- & * + - ~ ! sizeof }
//           = first(primary_expression) + { ++ -- & * + - ~ ! sizeof }
//           = { id const string ( ++ -- & * + - ~ ! sizeof }
//
class _unary_expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        node = null;

        int current;
        int saved;

        if (Parser.IsSizeof(src[begin])) {
            current = begin + 1;
            if (Parser.IsLPAREN(src[current])) {
                current++;
                TypeName type_name;
                saved = current;
                current = _type_name.Parse(src, current, out type_name);
                if (current != -1) {
                    if (Parser.IsRPAREN(src[current])) {
                        current++;
                        node = new SizeofType(type_name);
                        return current;
                    }
                }
            }

            current = begin + 1;
            current = _unary_expression.Parse(src, current, out node);
            if (current == -1) {
                return -1;
            }
            node = new SizeofExpression(node);
            return current;
        }

        current = _postfix_expression.Parse(src, begin, out node);
        if (current != -1) {
            return current;
        }

        if (src[begin].type != TokenType.OPERATOR) {
            return -1;
        }

        current = begin;
        OperatorVal val = ((TokenOperator)src[begin]).val;
        switch (val) {
        case OperatorVal.INC:
            current++;
            current = _unary_expression.Parse(src, current, out node);
            if (current == -1) {
                return -1;
            }
            node = new PrefixIncrement(node);
            return current;

        case OperatorVal.DEC:
            current++;
            current = _unary_expression.Parse(src, current, out node);
            if (current == -1) {
                return -1;
            }
            node = new PrefixDecrement(node);
            return current;

        case OperatorVal.BITAND:
            current++;
            current = _cast_expression.Parse(src, current, out node);
            if (current == -1) {
                return -1;
            }
            node = new Reference(node);
            return current;

        case OperatorVal.MULT:
            current++;
            current = _cast_expression.Parse(src, current, out node);
            if (current == -1) {
                return -1;
            }
            node = new Dereference(node);
            return current;

        case OperatorVal.ADD:
            current++;
            current = _cast_expression.Parse(src, current, out node);
            if (current == -1) {
                return -1;
            }
            node = new Positive(node);
            return current;

        case OperatorVal.SUB:
            current++;
            current = _cast_expression.Parse(src, current, out node);
            if (current == -1) {
                return -1;
            }
            node = new Negative(node);
            return current;

        case OperatorVal.TILDE:
            current++;
            current = _cast_expression.Parse(src, current, out node);
            if (current == -1) {
                return -1;
            }
            node = new BitwiseNot(node);
            return current;

        case OperatorVal.NOT:
            current++;
            current = _cast_expression.Parse(src, current, out node);
            if (current == -1) {
                return -1;
            }
            node = new Not(node);
            return current;

        default:
            return -1;
        }

    }
}

class SizeofType : Expression {
    public SizeofType(TypeName _type_name) {
        type_name = _type_name;
    }
    public TypeName type_name;
}

class SizeofExpression : Expression {
    public SizeofExpression(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}

class PrefixIncrement : Expression {
    public PrefixIncrement(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}

class PrefixDecrement : Expression {
    public PrefixDecrement(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}

class Reference : Expression {
    public Reference(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}

class Dereference : Expression {
    public Dereference(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}

class Positive : Expression {
    public Positive(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}

class Negative : Expression {
    public Negative(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}

class BitwiseNot : Expression {
    public BitwiseNot(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}

class Not : Expression {
    public Not(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}


// cast_expression: unary_expression
//                | ( type_name ) cast_expression
// [ note: my solution ]
// cast_expression: < ( type_name ) >* unary_expression
class _cast_expression : Expression {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        node = null;
        int current = begin;

        if (Parser.IsLPAREN(src[begin])) {
            current++;
            TypeName type_name;
            current = _type_name.Parse(src, current, out type_name);
            if (current != -1) {
                if (Parser.IsRPAREN(src[current])) {
                    current++;
                    current = _cast_expression.Parse(src, current, out node);
                    if (current == -1) {
                        return -1;
                    }
                    node = new TypeCast(type_name, node);
                    return current;
                }
            }
        }

        return _unary_expression.Parse(src, begin, out node);
    }
}

class TypeCast : Expression {
    public TypeCast(TypeName _type_name, Expression _expr) {
        type_name = _type_name;
        expr = _expr;
    }
    public TypeName type_name;
    public Expression expr;
}

// multiplicative_expression: cast_expression
//                          | multiplicative_expression * cast_expression
//                          | multiplicative_expression / cast_expression
//                          | multiplicative_expression % cast_expression
// [ note: my solution ]
// multiplicative_Expression: cast_expression < < * | / | % > cast_expression >*
class _multiplicative_expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        int current = _cast_expression.Parse(src, begin, out node);
        if (current == -1) {
            return -1;
        }

        Expression rhs;
        while (true) {
            if (src[current].type != TokenType.OPERATOR) {
                return current;
            }
            OperatorVal val = ((TokenOperator)src[current]).val;
            switch (val) {
            case OperatorVal.MULT:
                current++;
                current = _cast_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new Multiplication(node, rhs);
                break;
            case OperatorVal.DIV:
                current++;
                current = _cast_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new Division(node, rhs);
                break;
            case OperatorVal.MOD:
                current++;
                current = _cast_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new Modulo(node, rhs);
                break;
            default:
                return current;
            }
        }

    }
}

class Multiplication : Expression {
    public Multiplication(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

class Division : Expression {
    public Division(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

class Modulo : Expression {
    public Modulo(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


// additive_expression: multiplicative_expression
//                    | additive_expression + multiplicative_expression
//                    | additive_expression - multiplicative_expression
// [ note: my solution ]
// additive_expression: multiplicative_expression < < + | - > multiplicative_expression >*
class _additive_expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        int current = _multiplicative_expression.Parse(src, begin, out node);
        if (current == -1) {
            return -1;
        }
        Expression rhs;
        while (true) {
            if (src[current].type != TokenType.OPERATOR) {
                return current;
            }
            OperatorVal val = ((TokenOperator)src[current]).val;
            switch (val) {
            case OperatorVal.ADD:
                current++;
                current = _multiplicative_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new Addition(node, rhs);
                break;
            case OperatorVal.SUB:
                current++;
                current = _multiplicative_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new Subtraction(node, rhs);
                break;
            default:
                return current;
            }
        }

    }
}

class Addition : Expression {
    public Addition(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

class Subtraction : Expression {
    public Subtraction(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


// shift_expression: additive_expression
//                 | shift_expression << additive_expression
//                 | shift_expression >> additive_expression
// [ note: my solution ]
// shift_expression: additive_expression < < << | >> > additive_expression >*
class _shift_expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        int current = _additive_expression.Parse(src, begin, out node);
        if (current == -1) {
            return -1;
        }
        Expression rhs;
        while (true) {
            if (src[current].type != TokenType.OPERATOR) {
                return current;
            }
            OperatorVal val = ((TokenOperator)src[current]).val;
            switch (val) {
            case OperatorVal.LSHIFT:
                current++;
                current = _additive_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new LeftShift(node, rhs);
                break;
            case OperatorVal.RSHIFT:
                current++;
                current = _additive_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new RightShift(node, rhs);
                break;
            default:
                return current;
            }
        }

    }
}

class LeftShift : Expression {
    public LeftShift(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

class RightShift : Expression {
    public RightShift(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


// relational_expression: shift_expression
//                      | relational_expression < shift_expression
//                      | relational_expression > shift_expression
//                      | relational_expression <= shift_expression
//                      | relational_expression >= shift_expression
// [ note: my solution ]
// relational_expression: shift_expression < < < | > | <= | >= > > shift_expression >*
class _relational_expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        int current = _shift_expression.Parse(src, begin, out node);
        if (current == -1) {
            return -1;
        }

        Expression rhs;
        while (true) {
            if (src[current].type != TokenType.OPERATOR) {
                return current;
            }
            OperatorVal val = ((TokenOperator)src[current]).val;
            switch (val) {
            case OperatorVal.LT:
                current++;
                current = _shift_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new LessThan(node, rhs);
                break;
            case OperatorVal.GT:
                current++;
                current = _shift_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new GreaterThan(node, rhs);
                break;
            case OperatorVal.LEQ:
                current++;
                current = _shift_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new LessEqualThan(node, rhs);
                break;
            case OperatorVal.GEQ:
                current++;
                current = _shift_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new GreaterEqualThan(node, rhs);
                break;
            default:
                return current;
            }
        }

    }
}

class LessThan : Expression {
    public LessThan(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

class LessEqualThan : Expression {
    public LessEqualThan(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

class GreaterThan : Expression {
    public GreaterThan(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

class GreaterEqualThan : Expression {
    public GreaterEqualThan(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


// equality_expression: relational_expression
//                    | equality_expression == relational_expression
//                    | equality_expression != relational_expression
// [ note: my solution ]
// equality_expression: relational_expression < < == | != > relational_expression >*
class _equality_expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        int current = _relational_expression.Parse(src, begin, out node);
        if (current == -1) {
            return -1;
        }

        Expression rhs;
        while (true) {
            if (src[current].type != TokenType.OPERATOR) {
                return current;
            }
            OperatorVal val = ((TokenOperator)src[current]).val;
            switch (val) {
            case OperatorVal.EQ:
                current++;
                current = _relational_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new Equal(node, rhs);
                break;
            case OperatorVal.NEQ:
                current++;
                current = _relational_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new NotEqual(node, rhs);
                break;
            default:
                return current;
            }
        }

    }
}

class Equal : Expression {
    public Equal(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

class NotEqual : Expression {
    public NotEqual(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}

// and_expression: equality_expression
//               | and_expression & equality_expression
// [ note: my solution ]
// and_expression: equality_expression < & equality_expression >*
class _and_expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        int current = _equality_expression.Parse(src, begin, out node);
        if (current == -1) {
            return -1;
        }

        Expression rhs;
        while (true) {
            if (src[current].type != TokenType.OPERATOR) {
                return current;
            }
            OperatorVal val = ((TokenOperator)src[current]).val;
            switch (val) {
            case OperatorVal.BITAND:
                current++;
                current = _equality_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new BitwiseAnd(node, rhs);
                break;
            default:
                return current;
            }
        }
    }
}

class BitwiseAnd : Expression {
    public BitwiseAnd(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


// exclusive_or_expression: and_expression
//                         | exclusive_or_expression ^ and_expression
// [ note: my solution ]
// exclusive_or_expression: and_expression < ^ and_expression >*
class _exclusive_or_expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        int current = _and_expression.Parse(src, begin, out node);
        if (current == -1) {
            return -1;
        }

        Expression rhs;
        while (true) {
            if (src[current].type != TokenType.OPERATOR) {
                return current;
            }
            OperatorVal val = ((TokenOperator)src[current]).val;
            switch (val) {
            case OperatorVal.XOR:
                current++;
                current = _and_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new Xor(node, rhs);
                break;
            default:
                return current;
            }
        }
    }
}

class Xor : Expression {
    public Xor(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


// inclusive_or_expression: exclulsive_or_expression
//                        | inclusive_or_expression | exclulsive_or_expression
// [ note: my solution ]
// inclusive_or_expression: exclulsive_or_expression < | exclulsive_or_expression >*
class _inclusive_or_expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        int current = _exclusive_or_expression.Parse(src, begin, out node);
        if (current == -1) {
            return -1;
        }

        Expression rhs;
        while (true) {
            if (src[current].type != TokenType.OPERATOR) {
                return current;
            }
            OperatorVal val = ((TokenOperator)src[current]).val;
            switch (val) {
            case OperatorVal.BITOR:
                current++;
                current = _exclusive_or_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new BitwiseOr(node, rhs);
                break;
            default:
                return current;
            }
        }
    }
}

class BitwiseOr : Expression {
    public BitwiseOr(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


// logical_and_expression: inclusive_or_expression
//                       | logical_and_expression && inclusive_or_expression
// [ note: my solution ]
// logical_and_expression: inclusive_or_expression < && inclusive_or_expression >*
class _logical_and_expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        int current = _inclusive_or_expression.Parse(src, begin, out node);
        if (current == -1) {
            return -1;
        }

        Expression rhs;
        while (true) {
            if (src[current].type != TokenType.OPERATOR) {
                return current;
            }
            OperatorVal val = ((TokenOperator)src[current]).val;
            switch (val) {
            case OperatorVal.AND:
                current++;
                current = _inclusive_or_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new LogicalAnd(node, rhs);
                break;
            default:
                return current;
            }
        }
    }
}

class LogicalAnd : Expression {
    public LogicalAnd(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}


// logical_or_expression: logical_and_expression
//                      | logical_or_expression || logical_and_expression
// [ note: my solution ]
// logical_or_expression: logical_and_expression < || logical_and_expression >*

class _logical_or_expression : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        int current = _logical_and_expression.Parse(src, begin, out node);
        if (current == -1) {
            return -1;
        }

        Expression rhs;
        while (true) {
            if (src[current].type != TokenType.OPERATOR) {
                return current;
            }
            OperatorVal val = ((TokenOperator)src[current]).val;
            switch (val) {
            case OperatorVal.OR:
                current++;
                current = _logical_and_expression.Parse(src, current, out rhs);
                if (current == -1) {
                    return -1;
                }
                node = new LogicalOr(node, rhs);
                break;
            default:
                return current;
            }
        }
    }
}

class LogicalOr : Expression {
    public LogicalOr(Expression _lhs, Expression _rhs) {
        lhs = _lhs;
        rhs = _rhs;
    }
    public Expression lhs;
    public Expression rhs;
}
