using System;
using System.Collections.Generic;
using SyntaxTree;

// statement: labeled_statement
//          | compound_statement
//          | expression_statement
//          | selection_statement
//          | iteration_statement
//          | jump_statement
public class _statement : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out Stmt stmt) {
        stmt = null;
        Int32 current = _labeled_statement.Parse(src, begin, out stmt);
        if (current != -1) {
            return current;
        }

        CompoundStatement compound_stmt;
        current = _compound_statement.Parse(src, begin, out compound_stmt);
        if (current != -1) {
            stmt = compound_stmt;
            return current;
        }

        current = _expression_statement.Parse(src, begin, out stmt);
        if (current != -1) {
            return current;
        }

        current = _selection_statement.Parse(src, begin, out stmt);
        if (current != -1) {
            return current;
        }

        current = _iteration_statement.Parse(src, begin, out stmt);
        if (current != -1) {
            return current;
        }

        current = _jump_statement.Parse(src, begin, out stmt);
        if (current != -1) {
            return current;
        }

        return -1;
    }
}


// jump_statement: goto identifier ;
//               | continue ;
//               | break ;
//               | return <expression>? ;
public class _jump_statement : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out Stmt stmt) {
        stmt = null;

        if (src[begin].type != TokenType.KEYWORD) {
            return -1;
        }

        KeywordVal val = ((TokenKeyword)src[begin]).val;

        Int32 current = begin + 1;
        switch (val) {
        case KeywordVal.GOTO:
            if (src[current].type != TokenType.IDENTIFIER) {
                return -1;
            }
            stmt = new GotoStatement(((TokenIdentifier)src[current]).val);
            current++;
            break;
        case KeywordVal.CONTINUE:
            // current++;
            stmt = new ContinueStatement();
            break;
        case KeywordVal.BREAK:
            // current++;
            stmt = new BreakStatement();
            break;
        case KeywordVal.RETURN:
            Int32 saved = current;
            Expr expr;
            current = _expression.Parse(src, current, out expr);
            if (current == -1) {
                current = saved;
                stmt = new ReturnStatement(null);
            } else {
                stmt = new ReturnStatement(expr);
            }
            break;
        default:
            return -1;
        }

        if (!Parser.IsSEMICOLON(src[current])) {
            stmt = null;
            return -1;
        }
        current++;
        return current;

    }
}


// compound_statement : { <declaration_list>? <statement_list>? }
public class _compound_statement : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out CompoundStatement stmt) {
        stmt = null;
        if (!Parser.IsLCURL(src[begin])) {
            return -1;
        }
        Int32 current = begin + 1;

        List<Decln> decl_list;
        Int32 saved = current;
        current = _declaration_list.Parse(src, current, out decl_list);
        if (current == -1) {
            decl_list = new List<Decln>();
            current = saved;
        }

        List<Stmt> stmt_list;
        saved = current;
        current = _statement_list.Parse(src, current, out stmt_list);
        if (current == -1) {
            stmt_list = new List<Stmt>();
            current = saved;
        }

        if (!Parser.IsRCURL(src[current])) {
            return -1;
        }
        current++;

        stmt = new CompoundStatement(decl_list, stmt_list);
        return current;
    }
}


// declaration_list: declaration
//                 | declaration_list declaration
// [ note: my solution ]
// declaration_list: <declaration>+
public class _declaration_list : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out List<Decln> decl_list) {
        decl_list = new List<Decln>();
        Decln decl;
        Int32 current = _declaration.Parse(src, begin, out decl);
        if (current == -1) {
            return -1;
        }
        decl_list.Add(decl);
        Int32 saved;
        while (true) {
            saved = current;
            current = _declaration.Parse(src, current, out decl);
            if (current == -1) {
                return saved;
            }
            decl_list.Add(decl);
        }
    }
}


/// <summary>
/// statement_list
///   : [statement]+
/// </summary>
public class _statement_list : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out List<Stmt> stmts) {
        return Parser.ParseNonEmptyList(src, begin, out stmts, _statement.Parse);
    }
}


// expression_statement: <expression>? ;
public class _expression_statement : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out Stmt stmt) {
        stmt = null;
        Expr expr;
        Int32 current = _expression.Parse(src, begin, out expr);
        if (current == -1) {
            expr = null;
            current = begin;
        }

        if (!Parser.IsSEMICOLON(src[current])) {
            return -1;
        }
        current++;

        stmt = new ExpressionStatement(expr);
        return current;
    }
}


// iteration_statement: while ( expression ) statement
//                    | do statement while ( expression ) ;
//                    | for ( <expression>? ; <expression>? ; <expression>? ) statement
public class _iteration_statement : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out Stmt stmt) {
        stmt = null;
        Int32 current;
        if (Parser.IsKeyword(src[begin], KeywordVal.WHILE)) {
            // while
            current = begin + 1;

            Expr cond;
            current = Parser.ParseParenExpr(src, current, out cond);
            if (current == -1) {
                return -1;
            }

            Stmt body;
            current = _statement.Parse(src, current, out body);
            if (current == -1) {
                return -1;
            }

            stmt = new WhileStatement(cond, body);
            return current;

        } else if (Parser.IsKeyword(src[begin], KeywordVal.DO)) {
            // do
            current = begin + 1;

            Stmt body;
            current = _statement.Parse(src, current, out body);
            if (current == -1) {
                return -1;
            }

            Expr cond;
            current = Parser.ParseParenExpr(src, current, out cond);
            if (current == -1) {
                return -1;
            }

            stmt = new DoWhileStatement(body, cond);
            return current;

        } else if (Parser.IsKeyword(src[begin], KeywordVal.FOR)) {
            // for
            current = begin + 1;

            // match '('
            if (!Parser.EatOperator(src, ref current, OperatorVal.LPAREN)) {
                return -1;
            }

            // match init
            Option<Expr> init_opt;
            // Expr init;
            Int32 saved = current;
            current = Parser.ParseOption(src, current, out init_opt, _expression.Parse);
            //current = _expression.Parse(src, current, out init);
            //if (current == -1) {
            //    init = null;
            //    init_opt = new None<Expr>();
            //    current = saved;
            //} else {
            //    init_opt = new Some<Expr>(init);
            //}

            // match ';'
            if (!Parser.EatOperator(src, ref current, OperatorVal.SEMICOLON)) {
                return -1;
            }

            // match cond
            Option<Expr> cond_opt;
            current = Parser.ParseOption(src, current, out cond_opt, _expression.Parse);
            //Expr cond;
            //saved = current;
            //current = _expression.Parse(src, current, out cond);
            //if (current == -1) {
            //    init = null;
            //    current = saved;
            //}

            // match ';'
            if (!Parser.EatOperator(src, ref current, OperatorVal.SEMICOLON)) {
                return -1;
            }

            // match loop
            Option<Expr> loop_opt;
            current = Parser.ParseOption(src, current, out loop_opt, _expression.Parse);
            //Expr loop;
            //saved = current;
            //current = _expression.Parse(src, current, out loop);
            //if (current == -1) {
            //    init = null;
            //    current = saved;
            //}

            // match ')'
            if (!Parser.EatOperator(src, ref current, OperatorVal.RPAREN)) {
                return -1;
            }

            Stmt body;
            current = _statement.Parse(src, current, out body);
            if (current == -1) {
                return -1;
            }

            stmt = new ForStmt(init_opt, cond_opt, loop_opt, body);
            return current;

        } else {
            return -1;
        }
    }
}


// selection_statement: if ( expression ) statement
//                    | if ( expression ) statement else statement
//                    | switch ( expression ) statement
public class _selection_statement : ParseRule {

    public static Int32 Parse(List<Token> src, Int32 begin, out Stmt stmt) {
        stmt = null;

        Int32 current;
        Expr expr;
        if (Parser.IsKeyword(src[begin], KeywordVal.SWITCH)) {
            // switch

            current = begin + 1;
            current = Parser.ParseParenExpr(src, current, out expr);
            if (current == -1) {
                return -1;
            }

            current = _statement.Parse(src, current, out stmt);
            if (current == -1) {
                return -1;
            }

            stmt = new SwitchStatement(expr, stmt);
            return current;

        } else if (Parser.IsKeyword(src[begin], KeywordVal.IF)) {
            // if
            current = begin + 1;
            current = Parser.ParseParenExpr(src, current, out expr);
            if (current == -1) {
                return -1;
            }
            Stmt true_stmt;
            current = _statement.Parse(src, current, out true_stmt);
            if (current == -1) {
                return -1;
            }
            if (!Parser.IsKeyword(src[current], KeywordVal.ELSE)) {
                stmt = new IfStatement(expr, true_stmt);
                return current;
            }
            current++;
            Stmt false_stmt;
            current = _statement.Parse(src, current, out false_stmt);
            if (current == -1) {
                return -1;
            }
            stmt = new IfElseStatement(expr, true_stmt, false_stmt);
            return current;

        } else {
            return -1;
        }
    }
}


// labeled_statement : identifier : statement
//                   | case constant_expression : statement
//                   | default : statement
public class _labeled_statement : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out Stmt stmt) {
        stmt = null;

        Int32 current;
        if (Parser.IsKeyword(src[begin], KeywordVal.DEFAULT)) {
            current = begin + 1;

            // match ':'
            if (!Parser.EatOperator(src, ref current, OperatorVal.COLON)) {
                return -1;
            }

            // match statement
            current = _statement.Parse(src, current, out stmt);
            if (current == -1) {
                return -1;
            }

            stmt = new CaseStatement(null, stmt);
            return current;

        } else if (Parser.IsKeyword(src[begin], KeywordVal.CASE)) {
            current = begin + 1;

            // match expr
            Expr expr;
            current = _constant_expression.Parse(src, current, out expr);
            if (current == -1) {
                return -1;
            }

            // match ':'
            if (!Parser.EatOperator(src, ref current, OperatorVal.COLON)) {
                return -1;
            }

            // match statement
            current = _statement.Parse(src, current, out stmt);
            if (current == -1) {
                return -1;
            }

            stmt = new CaseStatement(expr, stmt);
            return current;

        } else if (src[begin].type == TokenType.IDENTIFIER) {
            String label = ((TokenIdentifier)src[begin]).val;
            current = begin + 1;

            // match ':'
            if (!Parser.EatOperator(src, ref current, OperatorVal.COLON)) {
                return -1;
            }

            // match statement
            current = _statement.Parse(src, current, out stmt);
            if (current == -1) {
                return -1;
            }

            stmt = new LabeledStatement(label, stmt);
            return current;

        } else {
            return -1;
        }


    }
}
