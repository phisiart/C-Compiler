using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// translation_unit : [external_declaration]+
public class _translation_unit : PTNode {
    public static int Parse(List<Token> src, int begin, out List<ASTNode> unit) {
        unit = null;

        ASTNode node;
        int current = _external_declaration.Parse(src, begin, out node);
        if (current == -1) {
            return -1;
        }

        unit.Add(node);

        int saved;
        while (true) {
            saved = current;
            current = _external_declaration.Parse(src, current, out node);
            if (current == -1) {
                return saved;
            }
            unit.Add(node);
        }

    }
}


// external_declaration: function_definition | declaration
public class _external_declaration : PTNode {
    public static int Parse(List<Token> src, int begin, out ASTNode node) {
        node = null;

        FunctionDefinition func_def;
        int current = _function_definition.Parse(src, begin, out func_def);
        if (current != -1) {
            node = func_def;
            return current;
        }

        Declaration decl;
        current = _declaration.Parse(src, begin, out decl);
        if (current != -1) {
            node = decl;
            return current;
        }

        return -1;
    }
}


// function_definition : <declaration_specifiers>? declarator <declaration_list>? compound_statement
// [ note: declaration_list is for old-style prototype, i'm not supporting this ]
// [ note: my solution ]
// function_definition : <declaration_specifiers>? declarator compound_statement
public class _function_definition : PTNode {
    public static int Parse(List<Token> src, int begin, out FunctionDefinition def) {
        def = null;

        DeclarationSpecifiers specs;
        int current = _declaration_specifiers.Parse(src, begin, out specs);
        if (current == -1) {
            specs = null;
            current = begin;
        }

        Declarator decl;
        current = _declarator.Parse(src, current, out decl);
        if (current == -1) {
            return -1;
        }

        Statement stmt;
        current = _compound_statement.Parse(src, current, out stmt);
        if (current == -1) {
            return -1;
        }

        def = new FunctionDefinition(specs, decl, stmt);
        return current;
    }
}

public class FunctionDefinition : ASTNode {
    public FunctionDefinition(DeclarationSpecifiers _specs, Declarator _decl, Statement _stmt) {
        specs = _specs;
        decl = _decl;
        stmt = _stmt;
    }
    public DeclarationSpecifiers specs;
    public Declarator decl;
    public Statement stmt;
}
