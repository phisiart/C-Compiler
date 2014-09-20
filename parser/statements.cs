using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

// statement: labeled_statement
//          | compound_statement
//          | expression_statement
//          | selection_statement
//          | iteration_statement
//          | jump_statement
class _statement : PTNode {
    public static int Parse(List<Token> src, int begin, out Statement stmt) {
        stmt = null;
        int current = _labeled_statement.Parse(src, begin, out stmt);
        if (current != -1) {
            return current;
        }

        current = _compound_statement.Parse(src, begin, out stmt);
        if (current != -1) {
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

class Statement : ASTNode {
}


// jump_statement: goto identifier ;
//               | continue ;
//               | break ;
//               | return <expression>? ;
class _jump_statement : PTNode {
    public static int Parse(List<Token> src, int begin, out Statement stmt) {
        stmt = null;

        if (src[begin].type != TokenType.KEYWORD) {
            return -1;
        }

        KeywordVal val = ((TokenKeyword)src[begin]).val;

        int current = begin + 1;
        switch (val) {
        case KeywordVal.GOTO:
            if (src[current].type != TokenType.IDENTIFIER) {
                return -1;
            }
            stmt = new GotoStatement(((TokenIdentifier)src[current]).val);
            current++;
            break;
        case KeywordVal.CONTINUE:
            current++;
            break;
        case KeywordVal.BREAK:
            current++;
            break;
        case KeywordVal.RETURN:
            int saved = current;
            Expression expr;
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

class GotoStatement : Statement {
    public GotoStatement(String _label) {
        label = _label;
    }
    public String label;
}

class ContinueStatement : Statement {
}

class BreakStatement : Statement {
}

class ReturnStatement : Statement {
    public ReturnStatement(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}


// compound_statement : { <declaration_list>? <statement_list>? }
class _compound_statement : PTNode {
    public static int Parse(List<Token> src, int begin, out Statement stmt) {
        stmt = null;
        if (!Parser.IsLCURL(src[begin])) {
            return -1;
        }
        int current = begin + 1;

        List<Declaration> decl_list;
        int saved = current;
        current = _declaration_list.Parse(src, current, out decl_list);
        if (current == -1) {
            decl_list = new List<Declaration>();
            current = saved;
        }

        List<Statement> stmt_list;
        saved = current;
        current = _statement_list.Parse(src, current, out stmt_list);
        if (current == -1) {
            stmt_list = new List<Statement>();
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

class CompoundStatement : Statement {
    public CompoundStatement(List<Declaration> _decl_list, List<Statement> _stmt_list) {
        decl_list = _decl_list;
        stmt_list = _stmt_list;
    }
    List<Declaration> decl_list;
    List<Statement> stmt_list;
}


// declaration_list: declaration
//                 | declaration_list declaration
// [ note: my solution ]
// declaration_list: <declaration>+
class _declaration_list : PTNode {
    public static int Parse(List<Token> src, int begin, out List<Declaration> decl_list) {
        decl_list = new List<Declaration>();
        Declaration decl;
        int current = _declaration.Parse(src, begin, out decl);
        if (current == -1) {
            return -1;
        }
        decl_list.Add(decl);
        int saved;
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


// statement_list: statement
//               | statement_list statement
// [ note: my solution ]
// statement_list: <statement>+
class _statement_list : PTNode {
    public static int Parse(List<Token> src, int begin, out List<Statement> stmt_list) {
        stmt_list = new List<Statement>();
        Statement stmt;
        int current = _statement.Parse(src, begin, out stmt);
        if (current == -1) {
            return -1;
        }
        stmt_list.Add(stmt);
        int saved;
        while (true) {
            saved = current;
            current = _statement.Parse(src, current, out stmt);
            if (current == -1) {
                return saved;
            }
            stmt_list.Add(stmt);
        }
    }
}


// expression_statement: <expression>? ;
class _expression_statement : PTNode {
    public static int Parse(List<Token> src, int begin, out Statement stmt) {
        stmt = null;
        Expression expr;
        int current = _expression.Parse(src, begin, out expr);
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

class ExpressionStatement : Statement {
    public ExpressionStatement(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}


// iteration_statement: while ( expression ) statement
//                    | do statement while ( expression ) ;
//                    | for ( <expression>? ; <expression>? ; <expression>? ) statement
class _iteration_statement : PTNode {
    private static int ParseExpression(List<Token> src, int begin, out Expression expr) {
        expr = null;
        if (!Parser.IsLPAREN(src[begin])) {
            return -1;
        }
        int current = begin + 1;
        current = _expression.Parse(src, current, out expr);
        if (current == -1) {
            return -1;
        }
        if (!Parser.IsRPAREN(src[current])) {
            return -1;
        }
        current++;
        return current;
    }

    public static int Parse(List<Token> src, int begin, out Statement stmt) {
        stmt = null;
        int current;
        if (Parser.IsKeyword(src[begin], KeywordVal.WHILE)) {
            // while
            current = begin + 1;

            Expression cond;
            current = ParseExpression(src, current, out cond);
            if (current == -1) {
                return -1;
            }

            Statement body;
            current = _statement.Parse(src, current, out body);
            if (current == -1) {
                return -1;
            }

            stmt = new WhileStatement(cond, body);
            return current;

        } else if (Parser.IsKeyword(src[begin], KeywordVal.DO)) {
            // do
            current = begin + 1;

            Statement body;
            current = _statement.Parse(src, current, out body);
            if (current == -1) {
                return -1;
            }

            Expression cond;
            current = ParseExpression(src, current, out cond);
            if (current == -1) {
                return -1;
            }

            stmt = new DoWhileStatement(body, cond);
            return current;

        } else if (Parser.IsKeyword(src[begin], KeywordVal.FOR)) {
            // for
            current = begin + 1;

            // match '('
            if (!Parser.IsLPAREN(src[current])) {
                return -1;
            }
            current++;

            // match init
            Expression init;
            int saved = current;
            current = _expression.Parse(src, current, out init);
            if (current == -1) {
                init = null;
                current = saved;
            }

            // match ';'
            if (!Parser.IsSEMICOLON(src[current])) {
                return -1;
            }
            current++;

            // match cond
            Expression cond;
            saved = current;
            current = _expression.Parse(src, current, out cond);
            if (current == -1) {
                init = null;
                current = saved;
            }

            // match ';'
            if (!Parser.IsSEMICOLON(src[current])) {
                return -1;
            }
            current++;

            // match loop
            Expression loop;
            saved = current;
            current = _expression.Parse(src, current, out loop);
            if (current == -1) {
                init = null;
                current = saved;
            }

            // match ')'
            if (!Parser.IsRPAREN(src[current])) {
                return -1;
            }
            current++;

            Statement body;
            current = _statement.Parse(src, current, out body);
            if (current == -1) {
                return -1;
            }

            stmt = new ForStatement(init, cond, loop, body);
            return current;

        } else {
            return -1;
        }
    }
}

class WhileStatement : Statement {
    public WhileStatement(Expression _cond, Statement _body) {
        cond = _cond;
        body = _body;
    }
    public Expression cond;
    public Statement body;
}

class DoWhileStatement : Statement {
    public DoWhileStatement(Statement _body, Expression _cond) {
        body = _body;
        cond = _cond;
    }
    public Statement body;
    public Expression cond;
}

class ForStatement : Statement {
    public ForStatement(Expression _init, Expression _cond, Expression _loop, Statement _body) {
        init = _init;
        cond = _cond;
        loop = _loop;
        body = _body;
    }
    public Expression init;
    public Expression cond;
    public Expression loop;
    public Statement body;
}


// selection_statement: if ( expression ) statement
//                    | if ( expression ) statement else statement
//                    | switch ( expression ) statement
class _selection_statement : PTNode {
    private static int ParseExpression(List<Token> src, int begin, out Expression expr) {
        expr = null;
        if (!Parser.IsLPAREN(src[begin])) {
            return -1;
        }
        int current = begin + 1;
        current = _expression.Parse(src, current, out expr);
        if (current == -1) {
            return -1;
        }
        if (!Parser.IsRPAREN(src[current])) {
            return -1;
        }
        current++;
        return current;
    }

    public static int Parse(List<Token> src, int begin, out Statement stmt) {
        stmt = null;

        int current;
        Expression expr;
        if (Parser.IsKeyword(src[begin], KeywordVal.SWITCH)) {
            // switch
            current = begin + 1;
            current = ParseExpression(src, current, out expr);
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
            current = ParseExpression(src, current, out expr);
            if (current == -1) {
                return -1;
            }
            Statement true_stmt;
            current = _statement.Parse(src, current, out true_stmt);
            if (current == -1) {
                return -1;
            }
            if (!Parser.IsKeyword(src[current], KeywordVal.ELSE)) {
                stmt = new IfStatement(expr, true_stmt);
                return current;
            }
            current++;
            Statement false_stmt;
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

class SwitchStatement : Statement {
    public SwitchStatement(Expression _expr, Statement _stmt) {
        expr = _expr;
        stmt = _stmt;
    }
    public Expression expr;
    public Statement stmt;
}

class IfStatement : Statement {
    public IfStatement(Expression _cond, Statement _stmt) {
        cond = _cond;
        stmt = _stmt;
    }
    public Expression cond;
    public Statement stmt;
}

class IfElseStatement : Statement {
    public IfElseStatement(Expression _cond, Statement _true_stmt, Statement _false_stmt) {
        cond = _cond;
        true_stmt = _true_stmt;
        false_stmt = _false_stmt;
    }
    public Expression cond;
    public Statement true_stmt;
    public Statement false_stmt;
}


// labeled_statement : identifier : statement
//                   | case constant_expression : statement
//                   | default : statement
class _labeled_statement : PTNode {
    public static int Parse(List<Token> src, int begin, out Statement stmt) {
        stmt = null;

        int current;
        if (Parser.IsKeyword(src[begin], KeywordVal.DEFAULT)) {
            current = begin + 1;

            // match ':'
            if (!Parser.IsCOLON(src[current])) {
                return -1;
            }
            current++;

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
            Expression expr;
            current = _constant_expression.Parse(src, current, out expr);
            if (current == -1) {
                return -1;
            }

            // match ':'
            if (!Parser.IsCOLON(src[current])) {
                return -1;
            }
            current++;

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
            if (!Parser.IsCOLON(src[current])) {
                return -1;
            }
            current++;

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

class LabeledStatement : Statement {
    public LabeledStatement(String _label, Statement _stmt) {
        label = _label;
        stmt = _stmt;
    }
    public String label;
    public Statement stmt;
}

class CaseStatement : Statement {
    public CaseStatement(Expression _expr, Statement _stmt) {
        expr = _expr;
        stmt = _stmt;
    }
    // expr == null means 'default'
    public Expression expr;
    public Statement stmt;
}
