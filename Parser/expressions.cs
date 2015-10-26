using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using SyntaxTree;

namespace Parsing {
    public partial class CParser {
        public static NamedParser<Expr>
            Expression { get; } = Parser.Create<Expr>("expression");

        public static NamedParser<Expr>
            PrimaryExpression { get; } = Parser.Create<Expr>("primary-expression");

        public static NamedParser<Expr>
            Variable { get; } = Parser.Create<Expr>("variable");

        public static NamedParser<Expr>
            Constant { get; } = Parser.Create<Expr>("constant");

        public static NamedParser<Expr>
            ConstantExpression { get; } = Parser.Create<Expr>("constant-expression");

        public static NamedParser<Expr>
            ConditionalExpression { get; } = Parser.Create<Expr>("conditional-expression");

        public static NamedParser<Expr>
            AssignmentExpression { get; } = Parser.Create<Expr>("assignment-expression");

        public static NamedParser<Expr>
            PostfixExpression { get; } = Parser.Create<Expr>("postfix-expression");

        public static NamedParser<IReadOnlyList<Expr>>
            ArgumentExpressionList { get; } = Parser.Create<IReadOnlyList<Expr>>("argument-expression-list");

        public static NamedParser<Expr>
            UnaryExpression { get; } = Parser.Create<Expr>("unary-expression");

        public static NamedParser<Expr>
            CastExpression { get; } = Parser.Create<Expr>("cast-expression");

        public static NamedParser<Expr>
            MultiplicativeExpression { get; } = Parser.Create<Expr>("multiplicative-expression");

        public static NamedParser<Expr>
            AdditiveExpression { get; } = Parser.Create<Expr>("additive-expression");

        public static NamedParser<Expr>
            ShiftExpression { get; } = Parser.Create<Expr>("shift-expression");

        public static NamedParser<Expr>
            RelationalExpression { get; } = Parser.Create<Expr>("relational-expression");

        public static NamedParser<Expr>
            EqualityExpression { get; } = Parser.Create<Expr>("equality-expression");

        public static NamedParser<Expr>
            AndExpression { get; } = Parser.Create<Expr>("and-expression");

        public static NamedParser<Expr>
            ExclusiveOrExpression { get; } = Parser.Create<Expr>("exclusive-or-expression");

        public static NamedParser<Expr>
            InclusiveOrExpression { get; } = Parser.Create<Expr>("inclusive-or-expression");

        public static NamedParser<Expr>
            LogicalAndExpression { get; } = Parser.Create<Expr>("logical-and-expression");

        public static NamedParser<Expr>
            LogicalOrExpression { get; } = Parser.Create<Expr>("logical-or-expression");

        /// <summary>
        /// The following are rules for C expressions.
        /// </summary>
        public static void SetExpressionRules() {

            /// <summary>
            /// expression
            ///   : assignment-expression [ ',' assignment-expression ]*
            /// </summary>
            Expression.Is(
                AssignmentExpression
                .OneOrMore(COMMA)
                .Then(exprs => {
                    if (exprs.Count == 1) {
                        return exprs[0];
                    } else {
                        return new AssignmentList(exprs);
                    }
                })
            );

            /// <summary>
            /// primary-expression
            ///   : identifier          <see cref="Variable"/> # Notice that the identifier cannot be a typedef name.
            ///   | constant            <see cref="Constant"/>   # Can either be const-char, const-float, or const-int
            ///   | string-literal      <see cref="STRING_LITERAL"/>
            ///   | '(' expression ')'
            /// </summary>
            PrimaryExpression.Is(
                (Variable)
                .Or(Constant)
                .Or(STRING_LITERAL)
                .Or((LEFT_PAREN).Then(Expression).Then(RIGHT_PAREN))
            );

            /// <summary>
            /// An identifier for a variable must not be defined as a typedef name.
            /// </summary>
            // TODO: name ambiguity?
            Variable.Is(
                IDENTIFIER.Check(result => true).Then(SyntaxTree.Variable.Create)
            );

            /// <summary>
            /// constant
            ///   : const-char
            ///   : const-int
            ///   : const-float
            /// </summary>
            Constant.Is(
                (CONST_CHAR)
                .Or(CONST_INT)
                .Or(CONST_FLOAT)
            );


            /// <summary>
            /// constant-expression
            ///   : conditional-expression
            /// </summary>
            /// <remarks>
            /// The size of an array should be a constant.
            /// Note that the check is actually performed in semantic analysis.
            /// </remarks>
            ConstantExpression.Is(
                ConditionalExpression
            );

            /// <summary>
            /// conditional-expression
            ///   : logical-or-expression [ '?' expression ':' conditional-expression ]?
            /// </summary>
            ConditionalExpression.Is(
                (LogicalOrExpression)
                .Then(
                    (QUESTION)
                    .Then(Expression)
                    .Then(COMMA)
                    .Then(ConditionalExpression)
                    .Then(
                        (Expr cond, Tuple<Expr, Expr> results) =>
                            SyntaxTree.ConditionalExpression.Create(cond, results.Item2, results.Item1)
                    )
                    .Optional()
                )
            );

            /// <summary>
            /// assignment-expression
            ///   : conditional-expression
            ///   : unary-expression assignment-operator assignment-expression
            /// </summary>
            // Assignment operators are:
            //   = *= /= %= += -= <<= >>= &= ^= |=
            AssignmentExpression.Is(
                (ConditionalExpression)
                .Or(
                    AssignmentOperatorParser(
                        UnaryExpression,
                        AssignmentExpression,
                        Tuple.Create(ASSIGN, Assignment.Create),
                        Tuple.Create(MULT_ASSIGN, MultAssign.Create),
                        Tuple.Create(DIV_ASSIGN, DivAssign.Create),
                        Tuple.Create(MOD_ASSIGN, ModAssign.Create),
                        Tuple.Create(ADD_ASSIGN, AddAssign.Create),
                        Tuple.Create(SUB_ASSIGN, SubAssign.Create),
                        Tuple.Create(LEFT_SHIFT_ASSIGN, LShiftAssign.Create),
                        Tuple.Create(RIGHT_SHIFT_ASSIGN, RShiftAssign.Create),
                        Tuple.Create(BITWISE_AND_ASSIGN, BitwiseAndAssign.Create),
                        Tuple.Create(XOR_ASSIGN, XorAssign.Create),
                        Tuple.Create(BITWISE_OR_ASSIGN, BitwiseOrAssign.Create)
                    )
                )
            );

            /// <summary>
            /// postfix-expression
            ///   : primary-expression [
            ///         '[' expression ']'                      # Get element from array
            ///       | '(' [argument-expression-list]? ')'     # Function call
            ///       | '.' identifier                          # Get member from struct/union
            ///       | '->' identifier                         # Get member from struct/union
            ///       | '++'                                    # Increment
            ///       | '--'                                    # Decrement
            ///     ]*
            /// </summary>
            PostfixExpression.Is(
                PrimaryExpression
                .Then(
                    (
                        (LEFT_BRACKET).Then(Expression).Then(RIGHT_BRACKET)
                        .Then(
                            (Expr array, Expr index) => Dereference.Create(Add.Create(array, index))
                        )
                    ).Or(
                        (LEFT_PAREN)
                        .Then(
                            (ArgumentExpressionList)
                            .Optional()
                            .Then(
                                optionalList => optionalList.IsSome ? optionalList.Value : new List<Expr>()
                            )
                        ).Then(RIGHT_PAREN)
                        .Then(FuncCall.Create)
                    ).Or(
                        (PERIOD).Then(IDENTIFIER).Then(SyntaxTree.Attribute.Create)
                    ).Or(
                        (RIGHT_ARROW).Then(IDENTIFIER)
                        .Then(
                            (Expr expr, String member) => SyntaxTree.Attribute.Create(Dereference.Create(expr), member)
                        )
                    ).Or(
                        (INCREMENT).Then(PostIncrement.Create)
                    ).Or(
                        (DECREMENT)
                        .Then(PostDecrement.Create)
                    ).ZeroOrMore()
                )
            );

            /// <summary>
            /// argument-expression-list
            ///   : assignment-expression [ ',' assignment-expression ]*
            /// </summary>
            ArgumentExpressionList.Is(
                AssignmentExpression.Then(ImmutableList.Create)
                .Then(
                    (COMMA)
                    .Then(AssignmentExpression)
                    .Then(
                        (ImmutableList<Expr> list, Expr expr) => list.Add(expr)
                    ).ZeroOrMore()
                )
            );

            /// <summary>
            /// unary-expression
            ///   : postfix-expression
            ///   | '++' unary-expression
            ///   | '--' unary-expression
            ///   | unary-operator cast-expression
            ///   | sizeof unary-expression
            ///   | sizeof '(' type-name ')'
            /// </summary>
            /// <remarks>
            /// 1. unary-operator can be '&', '*', '+', '-', '~', '!'.
            /// 2. The last two rules are ambiguous: you can't figure out whether the x in sizeof(x) is a typedef of a variable.
            ///    I have a parser hack for this: add a parser environment to track all the typedefs.
            /// 3. first_set = first_set(postfix-expression) + { '++', '--', '&', '*', '+', '-', '~', '!', sizeof }
            ///              = first_set(primary-expression) + { '++', '--', '&', '*', '+', '-', '~', '!', sizeof }
            ///              = { id, const, string, '++', '--', '&', '*', '+', '-', '~', '!', sizeof }
            /// </remarks>
            UnaryExpression.Is(
                (PostfixExpression)
                .Or(
                    (INCREMENT).Then(UnaryExpression).Then(PreIncrement.Create)
                ).Or(
                    (DECREMENT).Then(UnaryExpression).Then(PreDecrement.Create)
                ).Or(
                    (BITWISE_AND).Then(CastExpression).Then(Reference.Create)
                ).Or(
                    (MULT).Then(CastExpression).Then(Dereference.Create)
                ).Or(
                    (ADD).Then(CastExpression).Then(Positive.Create)
                ).Or(
                    (SUB).Then(CastExpression).Then(Negative.Create)
                ).Or(
                    (BITWISE_NOT).Then(CastExpression).Then(BitwiseNot.Create)
                ).Or(
                    (LOGICAL_NOT).Then(CastExpression).Then(LogicalNot.Create)
                ).Or(
                    (SIZEOF).Then(UnaryExpression).Then(SizeofExpr.Create)
                ).Or(
                    (SIZEOF).Then(LEFT_PAREN).Then(TypeName).Then(SizeofType.Create)
                )
            );

            /// <summary>
            /// cast-expression
            ///   : unary-expression
            ///   | '(' type_name ')' cast-expression
            /// </summary>
            CastExpression.Is(
                (UnaryExpression)
                .Or(
                    (LEFT_PAREN).Then(TypeName).Then(RIGHT_PAREN).Then(CastExpression)
                    .Then((Tuple<Expr, TypeName> _) => TypeCast.Create(_.Item2, _.Item1))
                )
            );

            /// <summary>
            /// multiplicative-expression
            ///   : cast-expression [ [ '*' | '/' | '%' ] cast-expression ]*
            /// </summary>
            MultiplicativeExpression.Is(
                BinaryOperatorParser(
                    CastExpression,
                    Tuple.Create(MULT, Multiply.Create),
                    Tuple.Create(DIV, Divide.Create),
                    Tuple.Create(MOD, Modulo.Create)
                )
            );

            /// <summary>
            /// additive-expression
            ///   : multiplicative-expression [ [ '+' | '-' ] multiplicative-expression ]*
            /// </summary>
            AdditiveExpression.Is(
                BinaryOperatorParser(
                    MultiplicativeExpression,
                    Tuple.Create(ADD, Add.Create),
                    Tuple.Create(SUB, Sub.Create)
                )
            );

            /// <summary>
            /// shift-expression
            ///   : additive-expression [ [ '&lt;&lt;' | '>>' ] additive-expression ]*
            /// </summary>
            ShiftExpression.Is(
                BinaryOperatorParser(
                    AdditiveExpression,
                    Tuple.Create(LEFT_SHIFT, LShift.Create),
                    Tuple.Create(RIGHT_SHIFT, RShift.Create)
                )
            );

            /// <summary>
            /// relational-expression
            ///   : shift-expression [ [ '&lt;' | '>' | '&lt=' | '>=' ] shift-expression ]*
            /// </summary>
            RelationalExpression.Is(
                BinaryOperatorParser(
                    ShiftExpression,
                    Tuple.Create(LESS, Less.Create),
                    Tuple.Create(GREATER, Greater.Create),
                    Tuple.Create(LESS_EQUAL, LEqual.Create),
                    Tuple.Create(GREATER_EQUAL, GEqual.Create)
                )
            );

            /// <summary>
            /// equality-expression
            ///   : relational-expression [ [ '==' | '!=' ] relational-expression ]*
            /// </summary>
            EqualityExpression.Is(
                BinaryOperatorParser(
                    RelationalExpression,
                    Tuple.Create(EQUAL, Equal.Create),
                    Tuple.Create(NOT_EQUAL, NotEqual.Create)
                )
            );

            /// <summary>
            /// and-expression
            ///   : equality-expression [ '&' equality-expression ]*
            /// </summary>
            AndExpression.Is(
                BinaryOperatorParser(
                    EqualityExpression,
                    Tuple.Create(BITWISE_AND, BitwiseAnd.Create)
                )
            );

            /// <summary>
            /// exclusive-or-expression
            ///   : and-expression [ '^' and-expression ]*
            /// </summary>
            ExclusiveOrExpression.Is(
                BinaryOperatorParser(
                    AndExpression,
                    Tuple.Create(XOR, Xor.Create)
                )
            );

            /// <summary>
            /// inclusive-or-expression
            ///   : exclusive-or-expression [ '|' exclusive-or-expression ]*
            /// </summary>
            InclusiveOrExpression.Is(
                BinaryOperatorParser(
                    ExclusiveOrExpression,
                    Tuple.Create(BITWISE_OR, BitwiseOr.Create)
                )
            );

            /// <summary>
            /// logical-and-expression
            ///   : inclusive-or-expression [ '&&' inclusive-or-expression ]*
            /// </summary>
            LogicalAndExpression.Is(
                BinaryOperatorParser(
                    InclusiveOrExpression,
                    Tuple.Create(LOGICAL_AND, LogicalAnd.Create)
                )
            );

            /// <summary>
            /// logical-or-expression
            ///   :logical-and-expression [ '||' logical-and-expression ]*
            /// </summary>
            LogicalOrExpression.Is(
                BinaryOperatorParser(
                    LogicalAndExpression,
                    Tuple.Create(LOGICAL_OR, LogicalOr.Create)
                )
            );
        }
    }
}
