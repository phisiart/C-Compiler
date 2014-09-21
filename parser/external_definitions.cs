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


// function_definition : [declaration_specifiers]? declarator [declaration_list]? compound_statement
//
// NOTE: the optional declaration_list is for the **old-style** function prototype like this:
// +-------------------------------+
// |    int foo(param1, param2)    |
// |    int param1;                |
// |    char param2;               |
// |    {                          |
// |        ....                   |
// |    }                          |
// +-------------------------------+
//
// i'm **not** going to support this style. function prototypes should always be like this:
// +------------------------------------------+
// |    int foo(int param1, char param2) {    |
// |        ....                              |
// |    }                                     |
// +------------------------------------------+
//
// so the grammar becomes:
// function_definition : [declaration_specifiers]? declarator compound_statement
//
// RETURN: FunctionDefinition
//
// FAIL: null
//
public class _function_definition : PTNode {
    public static bool Test() {
        var src = Parser.GetTokensFromString("int add(int a, int b) { return a + b; }");
        FunctionDefinition def;
        int current = Parse(src, 0, out def);
        if (current == -1) {
            return false;
        }

        return true;
    }
    
    public static int Parse(List<Token> src, int begin, out FunctionDefinition def) {
        // try to match declaration_specifiers, if not found, create an empty one.
        DeclarationSpecifiers specs;
        int current = _declaration_specifiers.Parse(src, begin, out specs);
        if (current == -1) {
            specs = new DeclarationSpecifiers(new List<StorageClassSpecifier>(), new List<TypeSpecifier>(), new List<TypeQualifier>());
            current = begin;
        }

        // match declarator
        Declarator decl;
        current = _declarator.Parse(src, current, out decl);
        if (current == -1) {
            def = null;
            return -1;
        }

        // match compound_statement
        Statement stmt;
        current = _compound_statement.Parse(src, current, out stmt);
        if (current == -1) {
            def = null;
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
