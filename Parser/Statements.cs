using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

using SyntaxTree;
using static Parsing.ParserCombinator;

namespace Parsing {
    public partial class CParser {
        public static NamedParser<Stmt>
            Statement = new NamedParser<Stmt>("statement");

        public static NamedParser<Stmt>
            JumpStatement = new NamedParser<Stmt>("jump-statement");

        public static NamedParser<Stmt>
            CompoundStatement = new NamedParser<Stmt>("compound-statement");

        public static NamedParser<ImmutableList<Decln>>
            DeclarationList = new NamedParser<ImmutableList<Decln>>("declaration-list");

        public static NamedParser<ImmutableList<Stmt>>
            StatementList = new NamedParser<ImmutableList<Stmt>>("statement-list");

        public static NamedParser<Stmt>
            ExpressionStatement = new NamedParser<Stmt>("expression-statement");

        public static NamedParser<Stmt>
            IterationStatement = new NamedParser<Stmt>("iteration-statement");

        public static NamedParser<Stmt>
            SelectionStatement = new NamedParser<Stmt>("selection-statement");

        public static NamedParser<Stmt>
            LabeledStatement = new NamedParser<Stmt>("labeled-statement");

        public static void SetStatementRules() {

            /// <summary>
            /// statement
            ///   : labeled-statement
            ///   | compound-statement
            ///   | expression-statement
            ///   | selection-statement
            ///   | iteration-statement
            ///   | jump-statement
            /// </summary>
            Statement.Is(
                (LabeledStatement)
                .Or(CompoundStatement)
                .Or(ExpressionStatement)
                .Or(SelectionStatement)
                .Or(IterationStatement)
                .Or(JumpStatement)
            );

            /// <summary>
            /// jump-statement
            ///   : 'goto' identifier ';'
            ///   | 'continue' ';'
            ///   | 'break' ';'
            ///   | 'return' [expression]? ';'
            /// </summary>
            JumpStatement.Is(
                (
                    ((GOTO).Then(IDENTIFIER).Then(GotoStmt.Create))
                    .Or(CONTINUE)
                    .Or(BREAK)
                    .Or((RETURN).Then(Expression.Optional()).Then(ReturnStmt.Create))
                )
                .Then(SEMICOLON)
            );

            /// <summary>
            /// compound-statement
            ///   : '{' [declaration-list]? [statement-list]? '}'
            /// </summary>
            CompoundStatement.Is(
                (LEFT_CURLY_BRACE)
                .Then(DeclarationList.Optional(ImmutableList<Decln>.Empty))
                .Then(StatementList.Optional(ImmutableList<Stmt>.Empty))
                .Then(RIGHT_CURLY_BRACE)
                .Then(CompoundStmt.Create)
            );

            /// <summary>
            /// declaration-list
            ///   : [declaration]+
            /// </summary>
            DeclarationList.Is(
                Declaration.OneOrMore()
            );

            /// <summary>
            /// statement-list
            ///   : [statement]+
            /// </summary>
            StatementList.Is(
                Statement.OneOrMore()
            );

            /// <summary>
            /// expression-statement
            ///   : [expression]? ';'
            /// </summary>
            ExpressionStatement.Is(
                Expression.Optional()
                .Then(SEMICOLON)
                .Then(ExprStmt.Create)
            );

            /// <summary>
            /// iteration-statement
            ///   : 'while' '(' expression ')' statement
            ///   | 'do' statement 'while' '(' expression ')' ';'
            ///   | 'for' '(' [expression]? ';' [expression]? ';' [expression]? ')' statement
            /// </summary>
            IterationStatement.Is(
                (
                    (WHILE)
                    .Then(LEFT_PAREN)
                    .Then(Expression)
                    .Then(RIGHT_PAREN)
                    .Then(Statement)
                    .Then(WhileStmt.Create)
                ).Or(
                    (DO)
                    .Then(Statement)
                    .Then(WHILE)
                    .Then(LEFT_PAREN)
                    .Then(Expression)
                    .Then(RIGHT_PAREN)
                    .Then(SEMICOLON)
                    .Then(DoWhileStmt.Create)
                ).Or(
                    (FOR)
                    .Then(LEFT_PAREN)
                    .Then(Expression.Optional())
                    .Then(SEMICOLON)
                    .Then(Expression.Optional())
                    .Then(SEMICOLON)
                    .Then(Expression.Optional())
                    .Then(RIGHT_PAREN)
                    .Then(Statement)
                    .Then(ForStmt.Create)
                )
            );

            /// <summary>
            /// selection-statement
            ///   : 'if' '(' expression ')' statement 'else' statement
            ///   | 'if' '(' expression ')' statement
            ///   | 'switch' '(' expression ')' statement
            /// </summary>
            SelectionStatement.Is(
                (
                    (IF)
                    .Then(LEFT_PAREN)
                    .Then(Expression)
                    .Then(RIGHT_PAREN)
                    .Then(Statement)
                    .Then(
                        (
                            Given<Expr, Stmt>()
                            .Then(ELSE)
                            .Then(Statement)
                            .Then(IfElseStmt.Create)
                        ).Or(
                            Given<Expr, Stmt>()
                            .Then(IfStmt.Create)
                        )
                    )
                ).Or(
                    (SWITCH)
                    .Then(LEFT_PAREN)
                    .Then(Expression)
                    .Then(RIGHT_PAREN)
                    .Then(Statement)
                    .Then(SwitchStmt.Create)
                )
            );

            /// <summary>
            /// labeled-statement
            ///   : identifier ':' statement
            ///   | 'case' constant-expression ':' statement
            ///   | 'default' ':' statement
            /// </summary>
            // TODO: case and default should be different
            LabeledStatement.Is(
                (
                    (IDENTIFIER)
                    .Then(COLON)
                    .Then(Statement)
                    .Then(LabeledStmt.Create)
                )
                .Or(
                    (CASE)
                    .Then(ConstantExpression.Then(_ => new Some<Expr>(_)))
                    .Then(COLON)
                    .Then(Statement)
                    .Then(CaseStmt.Create)
                ).Or(
                    (DEFAULT)
                    .Then(COLON)
                    .Then(Statement)
                    .Then(stmt => CaseStmt.Create(new None<Expr>(), stmt))
                )
            );
        }
    }
}
