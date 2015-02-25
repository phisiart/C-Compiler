using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// translation_unit : [external_declaration]+
public class _translation_unit : ParseRule {
    public static bool Test() {
        var src = Parser.GetTokensFromString("int a; int b() { return 1; }");
        TranslationUnit unit;
        int current = Parse(src, 0, out unit);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("int a() { return 1; } int b;");
        current = Parse(src, 0, out unit);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("int a");
        current = Parse(src, 0, out unit);
        if (current != -1) {
            return false;
        }

        return true;
    }
    
    public static int Parse(List<Token> src, int pos, out TranslationUnit unit) {
        List<ExternalDeclaration> list;
        int current;
        if ((current = Parser.ParseNonEmptyList(src, pos, out list, _external_declaration.Parse)) != -1) {
            unit = new TranslationUnit(list);
            return current;
        } else {
            unit = null;
            return -1;
        }
    }
    
}
// external_declaration: function_definition | declaration
public class _external_declaration : ParseRule {
    public static bool Test() {
        var src = Parser.GetTokensFromString("int a;");
        ExternalDeclaration node;
        int current = Parse(src, 0, out node);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("int a() { return 1; }");
        current = Parse(src, 0, out node);
        if (current == -1) {
            return false;
        }

        return true;
    }

    public static int Parse(List<Token> src, int pos, out ExternalDeclaration node) {
        return Parser.Parse2Choices<ExternalDeclaration, FunctionDefinition, Decln>(src, pos, out node, _function_definition.Parse, _declaration.Parse);
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
public class _function_definition : ParseRule {
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
        CompoundStatement stmt;
        current = _compound_statement.Parse(src, current, out stmt);
        if (current == -1) {
            def = null;
            return -1;
        }

        def = new FunctionDefinition(specs, decl, stmt);
        return current;
    }
}
