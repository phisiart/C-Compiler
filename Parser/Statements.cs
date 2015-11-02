using System.Collections.Immutable;

using SyntaxTree;
using static Parsing.ParserCombinator;

namespace Parsing {
    public partial class CParsers {

        /// <summary>
        /// statement
        ///   : labeled-statement
        ///   | compound-statement
        ///   | expression-statement
        ///   | selection-statement
        ///   | iteration-statement
        ///   | jump-statement
        /// </summary>
        public static NamedParser<Stmt>
            Statement = new NamedParser<Stmt>("statement");

        /// <summary>
        /// jump-statement
        ///   : 'goto' identifier ';'
        ///   | 'continue' ';'
        ///   | 'break' ';'
        ///   | 'return' [expression]? ';'
        /// </summary>
        public static NamedParser<Stmt>
            JumpStatement = new NamedParser<Stmt>("jump-statement");

        /// <summary>
        /// compound-statement
        ///   : '{' [declaration-list]? [statement-list]? '}'
        /// </summary>
        public static NamedParser<Stmt>
            CompoundStatement = new NamedParser<Stmt>("compound-statement");

        /// <summary>
        /// declaration-list
        ///   : [declaration]+
        /// </summary>
        public static NamedParser<ImmutableList<Decln>>
            DeclarationList = new NamedParser<ImmutableList<Decln>>("declaration-list");

        /// <summary>
        /// statement-list
        ///   : [statement]+
        /// </summary>
        public static NamedParser<ImmutableList<Stmt>>
            StatementList = new NamedParser<ImmutableList<Stmt>>("statement-list");

        /// <summary>
        /// expression-statement
        ///   : [expression]? ';'
        /// </summary>
        public static NamedParser<Stmt>
            ExpressionStatement = new NamedParser<Stmt>("expression-statement");

        /// <summary>
        /// iteration-statement
        ///   : 'while' '(' expression ')' statement
        ///   | 'do' statement 'while' '(' expression ')' ';'
        ///   | 'for' '(' [expression]? ';' [expression]? ';' [expression]? ')' statement
        /// </summary>
        public static NamedParser<Stmt>
            IterationStatement = new NamedParser<Stmt>("iteration-statement");

        /// <summary>
        /// selection-statement
        ///   : 'if' '(' expression ')' statement 'else' statement
        ///   | 'if' '(' expression ')' statement
        ///   | 'switch' '(' expression ')' statement
        /// </summary>
        public static NamedParser<Stmt>
            SelectionStatement = new NamedParser<Stmt>("selection-statement");

        /// <summary>
        /// labeled-statement
        ///   : identifier ':' statement
        ///   | 'case' constant-expression ':' statement
        ///   | 'default' ':' statement
        /// </summary>
        public static NamedParser<Stmt>
            LabeledStatement = new NamedParser<Stmt>("labeled-statement");

        public static void SetStatementRules() {

            // statement
            //   : labeled-statement
            //   | compound-statement
            //   | expression-statement
            //   | selection-statement
            //   | iteration-statement
            //   | jump-statement
            Statement.Is(
                (LabeledStatement)
                .Or(CompoundStatement)
                .Or(ExpressionStatement)
                .Or(SelectionStatement)
                .Or(IterationStatement)
                .Or(JumpStatement)
            );

            // jump-statement
            //   : 'goto' identifier ';'
            //   | 'continue' ';'
            //   | 'break' ';'
            //   | 'return' [expression]? ';'
            JumpStatement.Is(
                (
                    ((Goto).Then(Identifier).Then(GotoStmt.Create))
                    .Or(Continue)
                    .Or(Break)
                    .Or((Return).Then(Expression.Optional()).Then(ReturnStmt.Create))
                )
                .Then(Semicolon)
            );

            // compound-statement
            //   : '{' [declaration-list]? [statement-list]? '}'
            CompoundStatement.Is(
                (LeftCurlyBrace)
                .TransformEnvironment(env => env.InScope())
                .Then(DeclarationList.Optional(ImmutableList<Decln>.Empty))
                .Then(StatementList.Optional(ImmutableList<Stmt>.Empty))
                .Then(RightCurlyBrace)
                .TransformEnvironment(env => env.OutScope())
                .Then(CompoundStmt.Create)
            );

            // declaration-list
            //   : [declaration]+
            DeclarationList.Is(
                Declaration.OneOrMore()
            );

            // statement-list
            //   : [statement]+
            StatementList.Is(
                Statement.OneOrMore()
            );

            // expression-statement
            //   : [expression]? ';'
            ExpressionStatement.Is(
                Expression.Optional()
                .Then(Semicolon)
                .Then(ExprStmt.Create)
            );

            // iteration-statement
            //   : 'while' '(' expression ')' statement
            //   | 'do' statement 'while' '(' expression ')' ';'
            //   | 'for' '(' [expression]? ';' [expression]? ';' [expression]? ')' statement
            IterationStatement.Is(
                (
                    (While)
                    .Then(LeftParen)
                    .Then(Expression)
                    .Then(RightParen)
                    .Then(Statement)
                    .Then(WhileStmt.Create)
                ).Or(
                    (Do)
                    .Then(Statement)
                    .Then(While)
                    .Then(LeftParen)
                    .Then(Expression)
                    .Then(RightParen)
                    .Then(Semicolon)
                    .Then(DoWhileStmt.Create)
                ).Or(
                    (For)
                    .Then(LeftParen)
                    .Then(Expression.Optional())
                    .Then(Semicolon)
                    .Then(Expression.Optional())
                    .Then(Semicolon)
                    .Then(Expression.Optional())
                    .Then(RightParen)
                    .Then(Statement)
                    .Then(ForStmt.Create)
                )
            );

            // selection-statement
            //   : 'if' '(' expression ')' statement 'else' statement
            //   | 'if' '(' expression ')' statement
            //   | 'switch' '(' expression ')' statement
            SelectionStatement.Is(
                (
                    (If)
                    .Then(LeftParen)
                    .Then(Expression)
                    .Then(RightParen)
                    .Then(Statement)
                    .Then(
                        (
                            Given<Expr, Stmt>()
                            .Then(Else)
                            .Then(Statement)
                            .Then(IfElseStmt.Create)
                        ).Or(
                            Given<Expr, Stmt>()
                            .Then(IfStmt.Create)
                        )
                    )
                ).Or(
                    (Switch)
                    .Then(LeftParen)
                    .Then(Expression)
                    .Then(RightParen)
                    .Then(Statement)
                    .Then(SwitchStmt.Create)
                )
            );

            // labeled-statement
            //   : identifier ':' statement
            //   | 'case' constant-expression ':' statement
            //   | 'default' ':' statement
            LabeledStatement.Is(
                (
                    (Identifier)
                    .Then(Colon)
                    .Then(Statement)
                    .Then(LabeledStmt.Create)
                )
                .Or(
                    (Case)
                    .Then(ConstantExpression)
                    .Then(Colon)
                    .Then(Statement)
                    .Then(CaseStmt.Create)
                ).Or(
                    (Default)
                    .Then(Colon)
                    .Then(Statement)
                    .Then(DefaultStmt.Create)
                )
            );
        }
    }
}
