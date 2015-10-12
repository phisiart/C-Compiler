using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Parsing {
    public partial class Parser2 {

        /// <summary>
        /// expression
        ///   : assignment-expression [ ',' assignment-expression ]*
        /// </summary>
        public static ParsingFunction<SyntaxTree.Expr>
            Expression =>
                AssignmentExpression.aggregate(
                    ParseMore: Operator(OperatorVal.COMMA).then(AssignmentExpression),
                    Combine: (first, next) => new SyntaxTree.AssignmentList(new List<SyntaxTree.Expr> { first, next })
                );

        /// <summary>
        /// primary-expression
        ///   : identifier          <see cref="Variable"/> # Notice that the identifier cannot be a typedef name.
        ///   | constant            <see cref="Constant"/>   # Can either be const-char, const-float, or const-int
        ///   | string-literal      <see cref="StringLiteral"/>
        ///   | '(' expression ')'
        /// </summary>
        public static ParsingFunction<SyntaxTree.Expr>
            PrimaryExpression =>
                (Variable)
                .or(Constant)
                .or(StringLiteral)
                .or(
                    Operator(OperatorVal.LPAREN).then(Expression).then(Operator(OperatorVal.RPAREN))
                )
                ;

        public static ParsingFunction<SyntaxTree.Expr> Variable =>
            ParseTokenWhen((TokenIdentifier token) => true, token => new SyntaxTree.Variable(token.val));

        public static ParsingFunction<SyntaxTree.Expr>
            Constant =>
                (ConstChar)
                .or(ConstInt)
                .or(ConstFloat)
                ;

        public static ParsingFunction<SyntaxTree.Expr> ConstChar =>
            ParseToken((TokenCharConst token) => new SyntaxTree.ConstInt(token.value, TokenInt.Suffix.NONE));

        public static ParsingFunction<SyntaxTree.Expr> ConstFloat =>
            ParseToken((TokenFloat token) => new SyntaxTree.ConstFloat(token.value, token.suffix));

        public static ParsingFunction<SyntaxTree.Expr> ConstInt =>
            ParseToken((TokenInt token) => new SyntaxTree.ConstInt(token.val, token.suffix));

        public static ParsingFunction<SyntaxTree.Expr> StringLiteral =>
            ParseToken((TokenString token) => new SyntaxTree.StringLiteral(token.raw));

        /// <summary>
        /// constant-expression
        ///   : conditional-expression
        /// </summary>
        /// <remarks>
        /// The size of an array should be a constant.
        /// Note that the check is actually performed in semantic analysis.
        /// </remarks>
        public static ParsingFunction<SyntaxTree.Expr>
            ConstantExpression =>
                ConditionalExpression;

        /// <summary>
        /// conditional-expression
        ///   : logical-or-expression [ '?' expression ':' conditional-expression ]?
        /// </summary>
        public static ParsingFunction<SyntaxTree.Expr> ConditionalExpression =>
            null;

        /// <summary>
        /// assignment-expression
        ///   : conditional-expression
        ///   | unary-expression assignment-operator assignment-expression
        /// </summary>
        /// <remarks>
        /// Conflict?
        /// </remarks>
        public static ParsingFunction<SyntaxTree.Expr> AssignmentExpression =>
            null;

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
        public static ParsingFunction<SyntaxTree.Expr> PostfixExpression =>
            null;

        /// <summary>
        /// argument-expression-list
        ///   : assignment-expression [ ',' assignment-expression ]*
        /// </summary>
        public static ParsingFunction<IReadOnlyList<SyntaxTree.Expr>>
            ArgumentExpressionList =>
                AssignmentExpression.transform(expr => new List<SyntaxTree.Expr> { expr })
                .aggregate(Operator(OperatorVal.COMMA).then(AssignmentExpression), // TODO);

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
        public static ParsingFunction<SyntaxTree.Expr>
            UnaryExpression =>
                (
                    PostfixExpression
                ).or(
                    Operator(OperatorVal.INC).then(UnaryExpression)
                ).or(
                    Operator(OperatorVal.DEC).then(UnaryExpression)
                ).or(
                    Operator(OperatorVal.BITAND).then(CastExpression).transform(expr => new SyntaxTree.Reference(expr))
                ).or(
                    Operator(OperatorVal.MULT).then(CastExpression).transform(expr => new SyntaxTree.Dereference(expr))
                ).or(
                    Operator(OperatorVal.ADD).then(CastExpression).transform(expr => new SyntaxTree.Positive(expr))
                ).or(
                    Operator(OperatorVal.SUB).then(CastExpression).transform(expr => new SyntaxTree.Negative(expr))
                ).or(
                    Operator(OperatorVal.TILDE).then(CastExpression).transform(expr => new SyntaxTree.BitwiseNot(expr))
                ).or(
                    Operator(OperatorVal.NOT).then(CastExpression).transform(expr => new SyntaxTree.LogicalNot(expr))
                ).or(
                    Keyword(KeywordVal.SIZEOF).then(UnaryExpression).transform(expr => new SyntaxTree.SizeofExpr(expr))
                ).or(
                    Keyword(KeywordVal.SIZEOF)
                    .then(Operator(OperatorVal.LPAREN))
                    .then(TypeName)
                    .then(Operator(OperatorVal.RPAREN))
                    .transform(typeName => new SyntaxTree.SizeofType(typeName))
                )
                ;

        /// <summary>
        /// cast-expression
        ///   : unary-expression
        ///   | '(' type_name ')' cast-expression
        /// </summary>
        public static ParsingFunction<SyntaxTree.Expr>
            CastExpression =>
                (UnaryExpression)
                .or(
                    Operator(OperatorVal.LPAREN)
                    .then(TypeName)
                    .then(Operator(OperatorVal.RPAREN))
                    .then(CastExpression)
                    .transform(typeNameAndExpression => new SyntaxTree.TypeCast(typeNameAndExpression.Item2, typeNameAndExpression.Item1))
                );

        /// <summary>
        /// multiplicative-expression
        ///   : cast-expression [ [ '*' | '/' | '%' ] cast-expression ]*
        /// </summary>
        public static ParsingFunction<SyntaxTree.Expr> MultiplicativeExpression =>
            ParseBinaryOperator(
                CastExpression,
                CreateDealer(OperatorVal.MULT, SyntaxTree.Multiply.Create),
                CreateDealer(OperatorVal.DIV, SyntaxTree.Divide.Create),
                CreateDealer(OperatorVal.MOD, SyntaxTree.Modulo.Create)
            );

        /// <summary>
        /// additive-expression
        ///   : multiplicative-expression [ [ '+' | '-' ] multiplicative-expression ]*
        /// </summary>
        public static ParsingFunction<SyntaxTree.Expr> AdditiveExpression =>
            ParseBinaryOperator(
                MultiplicativeExpression,
                CreateDealer(OperatorVal.ADD, SyntaxTree.Add.Create),
                CreateDealer(OperatorVal.SUB, SyntaxTree.Sub.Create)
            );

        /// <summary>
        /// shift-expression
        ///   : additive-expression [ [ '&lt;&lt;' | '>>' ] additive-expression ]*
        /// </summary>
        public static ParsingFunction<SyntaxTree.Expr> ShiftExpression =>
            ParseBinaryOperator(
                AdditiveExpression,
                CreateDealer(OperatorVal.LSHIFT, SyntaxTree.LShift.Create),
                CreateDealer(OperatorVal.RSHIFT, SyntaxTree.RShift.Create)
            );

        /// <summary>
        /// relational-expression
        ///   : shift-expression [ [ '&lt;' | '>' | '&lt=' | '>=' ] shift-expression ]*
        /// </summary>
        public static ParsingFunction<SyntaxTree.Expr> RelationalExpression =>
            ParseBinaryOperator(
                ShiftExpression,
                CreateDealer(OperatorVal.LT, SyntaxTree.Less.Create),
                CreateDealer(OperatorVal.GT, SyntaxTree.Greater.Create),
                CreateDealer(OperatorVal.LEQ, SyntaxTree.LEqual.Create),
                CreateDealer(OperatorVal.GEQ, SyntaxTree.GEqual.Create)
            );

        /// <summary>
        /// equality-expression
        ///   : relational-expression [ [ '=' | '!=' ] relational-expression ]*
        /// </summary>
        public static ParsingFunction<SyntaxTree.Expr> EqualityExpression =>
            ParseBinaryOperator(
                RelationalExpression,
                CreateDealer(OperatorVal.EQ, SyntaxTree.Equal.Create),
                CreateDealer(OperatorVal.NEQ, SyntaxTree.NotEqual.Create)
            );

        /// <summary>
        /// and-expression
        ///   : equality-expression [ '&' equality-expression ]*
        /// </summary>
        public static ParsingFunction<SyntaxTree.Expr> AndExpression =>
            ParseBinaryOperator(
                EqualityExpression,
                CreateDealer(OperatorVal.BITAND, SyntaxTree.BitwiseAnd.Create)
            );

        /// <summary>
        /// exclusive-or-expression
        ///   : and-expression [ '^' and-expression ]*
        /// </summary>
        public static ParsingFunction<SyntaxTree.Expr> ExclusiveOrExpression =>
            ParseBinaryOperator(
                AndExpression,
                CreateDealer(OperatorVal.XOR, SyntaxTree.Xor.Create)
            );

        /// <summary>
        /// inclusive-or-expression
        ///   : exclusive-or-expression [ '|' exclusive-or-expression ]*
        /// </summary>
        public static ParsingFunction<SyntaxTree.Expr> InclusiveOrExpression =>
            ParseBinaryOperator(
                ExclusiveOrExpression,
                CreateDealer(OperatorVal.BITOR, SyntaxTree.BitwiseOr.Create)
            );

        /// <summary>
        /// logical-and-expression
        ///   : inclusive-or-expression [ '&&' inclusive-or-expression ]*
        /// </summary>
        public static ParsingFunction<SyntaxTree.Expr> LogicalAndExpression =>
            ParseBinaryOperator(
                InclusiveOrExpression,
                CreateDealer(OperatorVal.AND, SyntaxTree.LogicalAnd.Create)
            );

        /// <summary>
        /// logical-or-expression
        ///   :logical-and-expression [ '||' logical-and-expression ]*
        /// </summary>
        public static ParsingFunction<SyntaxTree.Expr> LogicalOrExpression =>
            ParseBinaryOperator(
                LogicalAndExpression,
                CreateDealer(OperatorVal.OR, SyntaxTree.LogicalOr.Create)
            );

    }
}