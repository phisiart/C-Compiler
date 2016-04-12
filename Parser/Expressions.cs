using System.Collections.Immutable;
using AST;
using static Parsing.ParserCombinator;

namespace Parsing {
    public partial class CParsers {
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

        public static NamedParser<ImmutableList<Expr>>
            ArgumentExpressionList { get; } = Parser.Create<ImmutableList<Expr>>("argument-expression-list");

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

            // expression
            //   : assignment-expression [ ',' assignment-expression ]*
            Expression.Is(
                AssignmentExpression
                .OneOrMore(Comma)
                .Then(exprs => {
                    if (exprs.Count == 1) {
                        return exprs[0];
                    }
                    return AssignmentList.Create(exprs);
                })
            );

            // primary-expression
            //   : identifier          # Cannot be a typedef name.
            //   | constant
            //   | string-literal      
            //   | '(' expression ')'
            PrimaryExpression.Is(
                Either(Variable)
                .Or(Constant)
                .Or(StringLiteral)
                .Or(
                    (LeftParen).Then(Expression).Then(RightParen)
                )
            );

            // An identifier for a variable must not be defined as a typedef name.
            Variable.Is(
                Identifier.Check(result => !result.Environment.IsTypedefName(result.Result)).Then(AST.Variable.Create)
            );

            // constant
            //   : const-char
            //   : const-int
            //   : const-float
            Constant.Is(
                Either(ConstChar)
                .Or(ConstInt)
                .Or(ConstFloat)
            );


            // constant-expression
            //   : conditional-expression
            // 
            // Note:
            // The size of an array should be a constant.
            // Note that the check is actually performed in semantic analysis.
            ConstantExpression.Is(
                ConditionalExpression
            );

            // conditional-expression
            //   : logical-or-expression [ '?' expression ':' conditional-expression ]?
            ConditionalExpression.Is(
                (LogicalOrExpression)
                .Then(
                    Given<Expr>()
                    .Then(Question)
                    .Then(Expression)
                    .Then(Colon)
                    .Then(ConditionalExpression)
                    .Then(AST.ConditionalExpression.Create)
                    .Optional()
                )
            );

            // assignment-expression
            //   : unary-expression assignment-operator assignment-expression   # first-set = first-set(unary-expression)
            //   | conditional-expression                                       # first-set = first-set(cast-expression) = first-set(unary-expression) ++ { '(' }
            // 
            // Note:
            //   Assignment operators are:
            //     '=', '*=', '/=', '%=', '+=', '-=', '<<=', '>>=', '&=', '^=', '|='
            AssignmentExpression.Is(
                Either(
                    AssignmentOperator(
                        UnaryExpression,
                        AssignmentExpression,
                        BinaryOperatorBuilder.Create(Assign, Assignment.Create),
                        BinaryOperatorBuilder.Create(MultAssign, AST.MultAssign.Create),
                        BinaryOperatorBuilder.Create(DivAssign, AST.DivAssign.Create),
                        BinaryOperatorBuilder.Create(ModAssign, AST.ModAssign.Create),
                        BinaryOperatorBuilder.Create(AddAssign, AST.AddAssign.Create),
                        BinaryOperatorBuilder.Create(SubAssign, AST.SubAssign.Create),
                        BinaryOperatorBuilder.Create(LeftShiftAssign, LShiftAssign.Create),
                        BinaryOperatorBuilder.Create(RightShiftAssign, RShiftAssign.Create),
                        BinaryOperatorBuilder.Create(BitwiseAndAssign, AST.BitwiseAndAssign.Create),
                        BinaryOperatorBuilder.Create(XorAssign, AST.XorAssign.Create),
                        BinaryOperatorBuilder.Create(BitwiseOrAssign, AST.BitwiseOrAssign.Create)
                    )
                ).Or(
                    ConditionalExpression
                )
            );

            // postfix-expression
            //   : primary-expression [
            //         '[' expression ']'                      # Get element from array
            //       | '(' [argument-expression-list]? ')'     # Function call
            //       | '.' identifier                          # Get member from struct/union
            //       | '->' identifier                         # Get member from struct/union
            //       | '++'                                    # Increment
            //       | '--'                                    # Decrement
            //     ]*
            PostfixExpression.Is(
                PrimaryExpression
                .Then(
                    Either(
                        Given<Expr>()
                        .Then(LeftBracket)
                        .Then(Expression)
                        .Then(RightBracket)
                        .Then((array, index) => Dereference.Create(AST.Add.Create(array, index)))
                    ).Or(
                        Given<Expr>()
                        .Then(LeftParen)
                        .Then(ArgumentExpressionList.Optional(ImmutableList<Expr>.Empty))
                        .Then(RightParen)
                        .Then(FuncCall.Create)
                    ).Or(
                        Given<Expr>()
                        .Then(Period)
                        .Then(Identifier)
                        .Then(Attribute.Create)
                    ).Or(
                        Given<Expr>()
                        .Then(RightArrow)
                        .Then(Identifier)
                        .Then((expr, member) => Attribute.Create(Dereference.Create(expr), member))
                    ).Or(
                        Given<Expr>()
                        .Then(Increment)
                        .Then(PostIncrement.Create)
                    ).Or(
                        Given<Expr>()
                        .Then(Decrement)
                        .Then(PostDecrement.Create)
                    ).ZeroOrMore()
                )
            );

            // argument-expression-list
            //   : assignment-expression [ ',' assignment-expression ]*
            ArgumentExpressionList.Is(
                AssignmentExpression.OneOrMore(Comma)
            );

            // unary-expression
            //   : postfix-expression               # first-set = { id, const, string }
            //   | '++' unary-expression            # first-set = { '++' }
            //   | '--' unary-expression            # first-set = { '--' }
            //   | unary-operator cast-expression   # first-set = { '&', '*', '+', '-', '~', '!' }
            //   | 'sizeof' unary-expression        # first-set = { 'sizeof' }
            //   | 'sizeof' '(' Type-name ')'       # first-set = { 'sizeof' }
            // 
            // Notes:
            // 1. unary-operator can be '&', '*', '+', '-', '~', '!'.
            // 2. The last two rules are ambiguous: you can't figure out whether the x in sizeof(x) is a typedef of a variable.
            //    I have a parser hack for this: add a parser environment to track all the typedefs.
            // 3. first_set = first_set(postfix-expression) + { '++', '--', '&', '*', '+', '-', '~', '!', 'sizeof' }
            //              = first_set(primary-expression) + { '++', '--', '&', '*', '+', '-', '~', '!', 'sizeof' }
            //              = { id, const, string, '++', '--', '&', '*', '+', '-', '~', '!', 'sizeof' }
            UnaryExpression.Is(
                Either(
                    PostfixExpression
                ).Or(
                    (Increment).Then(UnaryExpression).Then(PreIncrement.Create)
                ).Or(
                    (Decrement).Then(UnaryExpression).Then(PreDecrement.Create)
                ).Or(
                    (BitwiseAnd).Then(CastExpression).Then(Reference.Create)
                ).Or(
                    (Mult).Then(CastExpression).Then(Dereference.Create)
                ).Or(
                    (Add).Then(CastExpression).Then(Positive.Create)
                ).Or(
                    (Sub).Then(CastExpression).Then(Negative.Create)
                ).Or(
                    (BitwiseNot).Then(CastExpression).Then(AST.BitwiseNot.Create)
                ).Or(
                    (LogicalNot).Then(CastExpression).Then(AST.LogicalNot.Create)
                ).Or(
                    (SizeOf).Then(UnaryExpression).Then(SizeofExpr.Create)
                ).Or(
                    (SizeOf).Then(LeftParen).Then(TypeName).Then(RightParen).Then(SizeofType.Create)
                )
            );

            // cast-expression
            //   : unary-expression                     # first-set = { id, const, string, '++', '--', '&', '*', '+', '-', '~', '!', 'sizeof' }
            //   | '(' type_name ')' cast-expression    # first-set = '('
            CastExpression.Is(
                Either(
                    UnaryExpression
                ).Or(
                    (LeftParen).Then(TypeName).Then(RightParen).Then(CastExpression)
                    .Then(TypeCast.Create)
                )
            );

            // multiplicative-expression
            //   : cast-expression [ [ '*' | '/' | '%' ] cast-expression ]*
            MultiplicativeExpression.Is(
                BinaryOperator(
                    CastExpression,
                    BinaryOperatorBuilder.Create(Mult, Multiply.Create),
                    BinaryOperatorBuilder.Create(Div, Divide.Create),
                    BinaryOperatorBuilder.Create(Mod, Modulo.Create)
                )
            );

            // additive-expression
            //   : multiplicative-expression [ [ '+' | '-' ] multiplicative-expression ]*
            AdditiveExpression.Is(
                BinaryOperator(
                    MultiplicativeExpression,
                    BinaryOperatorBuilder.Create(Add, AST.Add.Create),
                    BinaryOperatorBuilder.Create(Sub, AST.Sub.Create)
                )
            );

            // shift-expression
            //   : additive-expression [ [ '<<' | '>>' ] additive-expression ]*
            ShiftExpression.Is(
                BinaryOperator(
                    AdditiveExpression,
                    BinaryOperatorBuilder.Create(LeftShift, LShift.Create),
                    BinaryOperatorBuilder.Create(RightShift, RShift.Create)
                )
            );

            // relational-expression
            //   : shift-expression [ [ '<' | '>' | '<=' | '>=' ] shift-expression ]*
            RelationalExpression.Is(
                BinaryOperator(
                    ShiftExpression,
                    BinaryOperatorBuilder.Create(Less, AST.Less.Create),
                    BinaryOperatorBuilder.Create(Greater, AST.Greater.Create),
                    BinaryOperatorBuilder.Create(LessEqual, LEqual.Create),
                    BinaryOperatorBuilder.Create(GreaterEqual, GEqual.Create)
                )
            );

            // equality-expression
            //   : relational-expression [ [ '==' | '!=' ] relational-expression ]*
            EqualityExpression.Is(
                BinaryOperator(
                    RelationalExpression,
                    BinaryOperatorBuilder.Create(Equal, AST.Equal.Create),
                    BinaryOperatorBuilder.Create(NotEqual, AST.NotEqual.Create)
                )
            );

            // and-expression
            //   : equality-expression [ '&' equality-expression ]*
            AndExpression.Is(
                BinaryOperator(
                    EqualityExpression,
                    BinaryOperatorBuilder.Create(BitwiseAnd, AST.BitwiseAnd.Create)
                )
            );

            // exclusive-or-expression
            //   : and-expression [ '^' and-expression ]*
            ExclusiveOrExpression.Is(
                BinaryOperator(
                    AndExpression,
                    BinaryOperatorBuilder.Create(Xor, AST.Xor.Create)
                )
            );

            // inclusive-or-expression
            //   : exclusive-or-expression [ '|' exclusive-or-expression ]*
            InclusiveOrExpression.Is(
                BinaryOperator(
                    ExclusiveOrExpression,
                    BinaryOperatorBuilder.Create(BitwiseOr, AST.BitwiseOr.Create)
                )
            );

            // logical-and-expression
            //   : inclusive-or-expression [ '&&' inclusive-or-expression ]*
            LogicalAndExpression.Is(
                BinaryOperator(
                    InclusiveOrExpression,
                    BinaryOperatorBuilder.Create(LogicalAnd, AST.LogicalAnd.Create)
                )
            );

            // logical-or-expression
            //   :logical-and-expression [ '||' logical-and-expression ]*
            LogicalOrExpression.Is(
                BinaryOperator(
                    LogicalAndExpression,
                    BinaryOperatorBuilder.Create(LogicalOr, AST.LogicalOr.Create)
                )
            );
        }
    }
}
