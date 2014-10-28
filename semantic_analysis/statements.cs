using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

public class Statement : ASTNode {}


public class GotoStatement : Statement {
    public GotoStatement(String _label) {
        label = _label;
    }
    public String label;
}


public class ContinueStatement : Statement {}


public class BreakStatement : Statement {}


public class ReturnStatement : Statement {
    public ReturnStatement(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}


public class CompoundStatement : Statement {
    public CompoundStatement(List<Decln> _decl_list, List<Statement> _stmt_list) {
        decl_list = _decl_list;
        stmt_list = _stmt_list;
    }
    List<Decln> decl_list;
    List<Statement> stmt_list;
}


public class ExpressionStatement : Statement {
    public ExpressionStatement(Expression _expr) {
        expr = _expr;
    }
    public Expression expr;
}


// loops:
public class WhileStatement : Statement {
    public WhileStatement(Expression _cond, Statement _body) {
        cond = _cond;
        body = _body;
    }
    public Expression cond;
    public Statement body;
}


public class DoWhileStatement : Statement {
    public DoWhileStatement(Statement _body, Expression _cond) {
        body = _body;
        cond = _cond;
    }
    public Statement body;
    public Expression cond;
}


public class ForStatement : Statement {
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


public class SwitchStatement : Statement {
    public SwitchStatement(Expression _expr, Statement _stmt) {
        expr = _expr;
        stmt = _stmt;
    }
    public Expression expr;
    public Statement stmt;
}


public class IfStatement : Statement {
    public IfStatement(Expression _cond, Statement _stmt) {
        cond = _cond;
        stmt = _stmt;
    }
    public Expression cond;
    public Statement stmt;
}


public class IfElseStatement : Statement {
    public IfElseStatement(Expression _cond, Statement _true_stmt, Statement _false_stmt) {
        cond = _cond;
        true_stmt = _true_stmt;
        false_stmt = _false_stmt;
    }
    public Expression cond;
    public Statement true_stmt;
    public Statement false_stmt;
}


public class LabeledStatement : Statement {
    public LabeledStatement(String _label, Statement _stmt) {
        label = _label;
        stmt = _stmt;
    }
    public String label;
    public Statement stmt;
}


public class CaseStatement : Statement {
    public CaseStatement(Expression _expr, Statement _stmt) {
        expr = _expr;
        stmt = _stmt;
    }
    // expr == null means 'default'
    public Expression expr;
    public Statement stmt;
}
