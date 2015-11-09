using System;
using System.Collections.Generic;
using System.Linq;
using SyntaxTree;
using System.Collections.Immutable;

namespace ObsoleteParser {

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
    // 3. first set : id, const, String, '('
    [Obsolete]
    public class _primary_expression : ParseRule {
        public static Boolean Test() {
            Expr expr;

            var src = Parser.GetTokensFromString("test_id");
            Int32 current = Parse(src, 0, out expr);
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

            src = Parser.GetTokensFromString("\"String\"");
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

        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {

            // 1. match identifier
            String var_name = Parser.GetIdentifierValue(src[begin]);
            if (var_name != null) {
                if (!ParserEnvironment.HasTypedefName(var_name)) {
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
                // Expr = new ConstChar(((TokenChar)src[begin]).val);
                // NOTE : there is no const char in C, there is only const Int32 ...
                expr = new ConstInt(((TokenCharConst)src[begin]).value, TokenInt.Suffix.NONE);
                return begin + 1;
            }

            // 2.2. match float
            if (src[begin].type == TokenType.FLOAT) {
                expr = new ConstFloat(((TokenFloat)src[begin]).value, ((TokenFloat)src[begin]).suffix);
                return begin + 1;
            }

            // 2.3. match Int32
            if (src[begin].type == TokenType.INT) {
                expr = new ConstInt(((TokenInt)src[begin]).val, ((TokenInt)src[begin]).suffix);
                return begin + 1;
            }

            // 3. match String literal
            if (src[begin].type == TokenType.STRING) {
                expr = new StringLiteral(((TokenString)src[begin]).raw);
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


    /// <summary>
    /// expression
    ///   : assignment_expression [ ',' assignment_expression ]*
    /// </summary>
    [Obsolete]
    public class _expression : ParseRule {
        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
            List<Expr> assign_exprs;
            if (
                (begin =
                    Parser.ParseNonEmptyListWithSep(src, begin, out assign_exprs, _assignment_expression.Parse,
                        OperatorVal.COMMA)) == -1) {
                expr = null;
                return -1;
            } else {
                if (assign_exprs.Count == 1) {
                    expr = assign_exprs[0];
                    return begin;
                } else {
                    expr = AssignmentList.Create(assign_exprs.ToImmutableList());
                    return begin;
                }
            }
        }
    }

    /// <summary>
    /// constant_expression:
    ///   : conditional_expression
    /// </summary>
    /// <remarks>
    /// When declaring an array, the size should be a constant.
    /// </remarks>
    [Obsolete]
    public class _constant_expression : ParseRule {
        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
            return _conditional_expression.Parse(src, begin, out expr);
        }
    }


    /// <summary>
    /// conditional_expression:
    ///   : logical_or_expression [ '?' expression ':' conditional_expression ]?
    /// </summary>
    [Obsolete]
    public class _conditional_expression : ParseRule {
        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
            // logical_or_expression
            Int32 current = _logical_or_expression.Parse(src, begin, out expr);
            if (current == -1) {
                return -1;
            }

            // '?'
            if (!Parser.EatOperator(src, ref current, OperatorVal.QUESTION)) {
                return current;
            }

            // expression
            Expr true_expr;
            if ((current = _expression.Parse(src, current, out true_expr)) == -1) {
                return -1;
            }

            // ':'
            if (!Parser.EatOperator(src, ref current, OperatorVal.COLON)) {
                return -1;
            }

            // conditional_expression
            Expr false_expr;
            if ((current = Parse(src, current, out false_expr)) == -1) {
                return -1;
            }

            expr = new ConditionalExpression(expr, true_expr, false_expr);
            return current;
        }
    }

    // assignment_expression: conditional_expression
    //                      | unary_expression assignment_operator assignment_expression
    // [ note: assignment_operator is = *= /= %= += -= <<= >>= &= ^= |= ]
    // [ note: how to predict which one to choose? ]
    // [ note: unary_expression is a special type of conditional_expression ]
    // [ note: first try unary ]
    // first(conditional_expression) = first(cast_expression)
    [Obsolete]
    public class _assignment_expression : ParseRule {
        public static Int32 Parse(List<Token> src, Int32 begin, out Expr node) {
            node = null;
            Expr lvalue;
            Expr rvalue;
            Int32 current = _unary_expression.Parse(src, begin, out lvalue);
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
                            node = Assignment.Create(lvalue, rvalue);
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
                            node = new LShiftAssign(lvalue, rvalue);
                            return current;

                        case OperatorVal.RSHIFTASSIGN:
                            current++;
                            current = _assignment_expression.Parse(src, current, out rvalue);
                            if (current == -1) {
                                return -1;
                            }
                            node = new RShiftAssign(lvalue, rvalue);
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

    // postfix_expression: primary_expression                                       /* Expression */
    //                   | postfix_expression '[' expression ']'                    /* ArrayElement */
    //                   | postfix_expression '(' [argument_expression_list]? ')'  /* FunctionCall */
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
    [Obsolete]
    public class _postfix_expression : ParseRule {
        public static Boolean Test() {
            var src = Parser.GetTokensFromString("a");
            Expr expr;
            Int32 current = Parse(src, 0, out expr);
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

        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {

            // step 1. match primary_expression
            Int32 current = _primary_expression.Parse(src, begin, out expr);
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
                        Expr idx;
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
                        expr = new Dereference(new Add(expr, idx));
                        // Expr = new ArrayElement(Expr, idx);
                        break;

                    case OperatorVal.LPAREN:
                        // '('
                        current++;

                        // 1. match arglist, if no match, assume empty arglist
                        List<Expr> args;
                        Int32 saved = current;
                        current = _argument_expression_list.Parse(src, current, out args);
                        if (current == -1) {
                            args = new List<Expr>();
                            current = saved;
                        }

                        // 2. match ')'
                        if (!Parser.IsOperator(src[current], OperatorVal.RPAREN)) {
                            expr = null;
                            return -1;
                        }
                        current++;

                        // successful match
                        expr = FuncCall.Create(expr, args.ToImmutableList());
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
                        expr = SyntaxTree.Attribute.Create(expr, attrib);
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
                        expr = SyntaxTree.Attribute.Create(new Dereference(expr), pattrib);
                        // Expr = new PointerAttribute(Expr, new Variable(pattrib));
                        break;

                    case OperatorVal.INC:
                        // '++'
                        current++;

                        // successful match
                        expr = new PostIncrement(expr);
                        break;

                    case OperatorVal.DEC:
                        // '--'

                        current++;

                        // successful match
                        expr = new PostDecrement(expr);
                        break;

                    default:

                        // no more postfix
                        return current;

                } // case (val)

            } // while (true)

        }
    }


    /// <summary>
    /// argument_expression_list
    ///   : assignment_expression [ ',' assignment_expression ]*
    /// </summary>
    [Obsolete]
    public class _argument_expression_list : ParseRule {
        public static Int32 Parse(List<Token> src, Int32 begin, out List<Expr> node) {
            return Parser.ParseNonEmptyListWithSep(src, begin, out node, _assignment_expression.Parse, OperatorVal.COMMA);
        }
    }


    // unary_expression: postfix_expression                     /* Expression */
    //                 | '++' unary_expression                  /* PrefixIncrement */
    //                 | '--' unary_expression                  /* PrefixDecrement */
    //                 | unary_operator cast_expression         /* Reference
    //                                                             Dereference
    //                                                             Positive
    //                                                             Negative
    //                                                             BitwiseNot
    //                                                             Not */
    //                 | sizeof unary_expression                /* SizeofExpression */
    //                 | sizeof '(' type_name ')'               /* SizeofType */
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
    //           = { id const String ( ++ -- & * + - ~ ! sizeof }
    //
    [Obsolete]
    public class _unary_expression : ParseRule {
        public static Boolean Test() {
            var src = Parser.GetTokensFromString("a");
            Expr expr;
            Int32 current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("sizeof a");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("sizeof(Int32)");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("++a");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("--a");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("&a");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("*a");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("+a");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("-a");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("~a");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("!a");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("!!~++ --a");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }
            return true;
        }

        // match '(' type_name ')'
        public static Int32 ParseTypeName(List<Token> src, Int32 begin, out TypeName type_name) {
            // step 1. match '('
            if (!Parser.IsOperator(src[begin], OperatorVal.LPAREN)) {
                type_name = null;
                return -1;
            }
            begin++;

            // step 2. match type_name
            begin = _type_name.Parse(src, begin, out type_name);
            if (begin == -1) {
                type_name = null;
                return -1;
            }

            // step 3. match ')'
            if (!Parser.IsOperator(src[begin], OperatorVal.RPAREN)) {
                type_name = null;
                return -1;
            }
            begin++;

            // successful match
            return begin;
        }

        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
            //Expr = null;

            Int32 current;
            Int32 saved;


            if (Parser.IsKeyword(src[begin], KeywordVal.SIZEOF)) {
                // 1. sizeof
                current = begin + 1;

                // 1.1. try to match type_name
                saved = current;
                TypeName type_name;
                current = ParseTypeName(src, current, out type_name);
                if (current != -1) {
                    // 1.1. -- successful match
                    expr = new SizeofType(type_name);
                    return current;
                }

                // 1.2. type_name match failed, try unary_expression
                current = saved;
                current = _unary_expression.Parse(src, current, out expr);
                if (current == -1) {
                    expr = null;
                    return -1;
                }

                // 1.2. -- successful match
                expr = new SizeofExpr(expr);
                return current;

            } // sizeof

            // 2. postfix_expression
            current = _postfix_expression.Parse(src, begin, out expr);
            if (current != -1) {
                // successful match
                return current;
            }

            // now only operators are left
            if (src[begin].type != TokenType.OPERATOR) {
                return -1;
            }

            current = begin;
            OperatorVal val = ((TokenOperator)src[begin]).val;
            switch (val) {
                case OperatorVal.INC:
                    // '++'
                    current++;

                    current = _unary_expression.Parse(src, current, out expr);
                    if (current == -1) {
                        expr = null;
                        return -1;
                    }

                    expr = new PreIncrement(expr);
                    return current;

                case OperatorVal.DEC:
                    // '--'
                    current++;

                    current = _unary_expression.Parse(src, current, out expr);
                    if (current == -1) {
                        expr = null;
                        return -1;
                    }

                    expr = new PreDecrement(expr);
                    return current;

                case OperatorVal.BITAND:
                    // '&' (reference)
                    current++;

                    current = _cast_expression.Parse(src, current, out expr);
                    if (current == -1) {
                        expr = null;
                        return -1;
                    }

                    expr = new Reference(expr);
                    return current;

                case OperatorVal.MULT:
                    // '*' (dereference)
                    current++;

                    current = _cast_expression.Parse(src, current, out expr);
                    if (current == -1) {
                        expr = null;
                        return -1;
                    }

                    expr = new Dereference(expr);
                    return current;

                case OperatorVal.ADD:
                    // '+' (positive)
                    current++;

                    current = _cast_expression.Parse(src, current, out expr);
                    if (current == -1) {
                        expr = null;
                        return -1;
                    }

                    expr = new Positive(expr);
                    return current;

                case OperatorVal.SUB:
                    // '-' (negative)
                    current++;

                    current = _cast_expression.Parse(src, current, out expr);
                    if (current == -1) {
                        expr = null;
                        return -1;
                    }

                    expr = new Negative(expr);
                    return current;

                case OperatorVal.TILDE:
                    // '~' (bitwise not)
                    current++;

                    current = _cast_expression.Parse(src, current, out expr);
                    if (current == -1) {
                        expr = null;
                        return -1;
                    }

                    expr = new BitwiseNot(expr);
                    return current;

                case OperatorVal.NOT:
                    // '!' (logical not)
                    current++;

                    current = _cast_expression.Parse(src, current, out expr);
                    if (current == -1) {
                        expr = null;
                        return -1;
                    }

                    expr = new LogicalNot(expr);
                    return current;

                default:

                    // no match
                    return -1;

            } // case (val)

        }
    }

    // cast_expression: unary_expression                    /* Expression */
    //                | '(' type_name ')' cast_expression   /* TypeCast */
    //
    // RETURN: Expression
    //
    // FAIL: null
    //
    // NOTE:
    // this is right-recursive, which is totally fine
    //
    [Obsolete]
    public class _cast_expression : ParseRule {
        public static Boolean Test() {
            var src = Parser.GetTokensFromString("a");
            Expr expr;
            Int32 current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("(int)a");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("(int)(float)a");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            return true;
        }

        public static Int32 Parse(List<Token> src, Int32 begin, out Expr node) {

            // 1. try to match '(' type_name ')'
            TypeName type_name;
            Int32 current = _unary_expression.ParseTypeName(src, begin, out type_name);
            if (current != -1) {
                // successful match '(' type_name ')'

                // match cast_expression recursively
                current = _cast_expression.Parse(src, current, out node);
                if (current == -1) {
                    node = null;
                    return -1;
                }

                // successful match
                node = new TypeCast(type_name, node);
                return current;

            }

            // 2. unary_expression
            return _unary_expression.Parse(src, begin, out node);

        }
    }


    /// <summary>
    /// multiplicative_expression
    ///   : cast_expression [ [ '*' | '/' | '%' ] cast_expression ]*
    /// </summary>
    [Obsolete]
    public class _multiplicative_expression : ParseRule {
        public static Boolean Test() {
            var src = Parser.GetTokensFromString("a * b");
            Expr expr;
            Int32 current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("a * b / c % d");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            return true;
        }

        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
            return Parser.ParseBinaryOperator(
                src, begin, out expr,
                _cast_expression.Parse,
                new List<Tuple<OperatorVal, Parser.BinaryExpressionConstructor>> {
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.MULT,
                        (_lhs, _rhs) => new Multiply(_lhs, _rhs)),
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.DIV,
                        (_lhs, _rhs) => new Divide(_lhs, _rhs)),
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.MOD,
                        (_lhs, _rhs) => new Modulo(_lhs, _rhs)),
                }
                );
        }
    }


    /// <summary>
    /// additive_expression
    ///   : multiplicative_expression [ [ '+' | '-' ] multiplicative_expression ]*
    /// </summary>
    [Obsolete]
    public class _additive_expression : ParseRule {
        public static Boolean Test() {
            var src = Parser.GetTokensFromString("a * b + c");
            Expr expr;
            Int32 current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("a + c + d");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            return true;
        }

        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
            return Parser.ParseBinaryOperator(
                src, begin, out expr,
                _multiplicative_expression.Parse,
                new List<Tuple<OperatorVal, Parser.BinaryExpressionConstructor>> {
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.ADD,
                        (_lhs, _rhs) => new Add(_lhs, _rhs)),
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.SUB,
                        (_lhs, _rhs) => new Sub(_lhs, _rhs)),
                }
                );
        }
    }


    /// <summary>
    /// shift_expression
    ///   : additive_expression [ [ '<<' | '>>' ] additive_expression ]*
    /// </summary>
    [Obsolete]
    public class _shift_expression : ParseRule {
        public static Boolean Test() {
            var src = Parser.GetTokensFromString("a * b + c << 3");
            Expr expr;
            Int32 current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("a << 3 >> 4");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            return true;
        }

        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
            return Parser.ParseBinaryOperator(
                src, begin, out expr,
                _additive_expression.Parse,
                new List<Tuple<OperatorVal, Parser.BinaryExpressionConstructor>> {
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.LSHIFT,
                        (_lhs, _rhs) => new LShift(_lhs, _rhs)),
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.RSHIFT,
                        (_lhs, _rhs) => new RShift(_lhs, _rhs)),
                }
                );
        }
    }


    /// <summary>
    /// relational_expression
    ///   : shift_expression [ [ '&lt;' | '>' | '&lt;=' | '>=' ] shift_expression ]*
    /// </summary>
    [Obsolete]
    public class _relational_expression : ParseRule {
        public static Boolean Test() {
            var src = Parser.GetTokensFromString("3 < 4");
            Expr expr;
            Int32 current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            src = Parser.GetTokensFromString("a < 3 > 4");
            current = Parse(src, 0, out expr);
            if (current == -1) {
                return false;
            }

            return true;
        }

        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
            return Parser.ParseBinaryOperator(
                src, begin, out expr,
                _shift_expression.Parse,
                new List<Tuple<OperatorVal, Parser.BinaryExpressionConstructor>> {
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.LT,
                        (_lhs, _rhs) => new Less(_lhs, _rhs)),
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.GT,
                        (_lhs, _rhs) => new Greater(_lhs, _rhs)),
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.LEQ,
                        (_lhs, _rhs) => new LEqual(_lhs, _rhs)),
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.GEQ,
                        (_lhs, _rhs) => new GEqual(_lhs, _rhs)),
                }
                );
        }
    }


    /// <summary>
    /// equality_expression
    ///   : relational_expression [ [ '==' | '!=' ] relational_expression ]*
    /// </summary>
    [Obsolete]
    public class _equality_expression : ParseRule {
        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
            return Parser.ParseBinaryOperator(
                src, begin, out expr,
                _relational_expression.Parse,
                new List<Tuple<OperatorVal, Parser.BinaryExpressionConstructor>> {
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.EQ,
                        (_lhs, _rhs) => new Equal(_lhs, _rhs)),
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.NEQ,
                        (_lhs, _rhs) => new NotEqual(_lhs, _rhs)),
                }
                );
        }
    }


    /// <summary>
    /// and_expression
    ///   : equality_expresion [ '&' equality_expression ]*
    /// </summary>
    [Obsolete]
    public class _and_expression : ParseRule {
        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
            return Parser.ParseBinaryOperator(
                src, begin, out expr,
                _equality_expression.Parse,
                new List<Tuple<OperatorVal, Parser.BinaryExpressionConstructor>> {
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.BITAND,
                        (_lhs, _rhs) => new BitwiseAnd(_lhs, _rhs)),
                }
                );
        }
    }


    /// <summary>
    /// exclusive_or_expression
    ///   : and_expression [ '^' and_expression ]*
    /// </summary>
    [Obsolete]
    public class _exclusive_or_expression : ParseRule {
        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
            return Parser.ParseBinaryOperator(
                src, begin, out expr,
                _and_expression.Parse,
                new List<Tuple<OperatorVal, Parser.BinaryExpressionConstructor>> {
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.XOR,
                        (_lhs, _rhs) => new Xor(_lhs, _rhs)),
                }
                );
        }
    }


    /// <summary>
    /// inclusive_or_expression
    ///   : exclulsive_or_expression [ '|' exclulsive_or_expression ]*
    /// </summary>
    [Obsolete]
    public class _inclusive_or_expression : ParseRule {
        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
            return Parser.ParseBinaryOperator(
                src, begin, out expr,
                _exclusive_or_expression.Parse,
                new List<Tuple<OperatorVal, Parser.BinaryExpressionConstructor>> {
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.BITOR,
                        (lhs, rhs) => new BitwiseOr(lhs, rhs)),
                }
                );
        }
    }


    /// <summary>
    /// logical_and_expression
    ///   : inclusive_or_expression [ '&&' inclusive_or_expression ]*
    /// 
    /// <remarks>
    /// A logical and expression is just a bunch of (bitwise) inclusive or expressions.
    /// </remarks>
    /// </summary>
    [Obsolete]
    public class _logical_and_expression : ParseRule {
        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
            return Parser.ParseBinaryOperator(
                src, begin, out expr,
                _inclusive_or_expression.Parse,
                new List<Tuple<OperatorVal, Parser.BinaryExpressionConstructor>> {
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.AND,
                        (lhs, rhs) => new LogicalAnd(lhs, rhs)),
                }
                );
        }
    }


    /// <summary>
    /// logical_or_expression
    ///   : logical_and_expression [ '||' logical_and_expression ]*
    /// 
    /// <remarks>
    /// A logical or expression is just a bunch of logical and expressions separated by '||'s.
    /// </remarks>
    /// </summary>
    [Obsolete]
    public class _logical_or_expression : ParseRule {
        public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
            return Parser.ParseBinaryOperator(
                src, begin, out expr,
                _logical_and_expression.Parse,
                new List<Tuple<OperatorVal, Parser.BinaryExpressionConstructor>> {
                    new Tuple<OperatorVal, Parser.BinaryExpressionConstructor>(OperatorVal.OR,
                        (lhs, rhs) => new LogicalOr(lhs, rhs)),
                }
                );
        }
    }
}