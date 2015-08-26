using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyntaxTree;

// translation_unit : [external_declaration]+
public class _translation_unit : ParseRule {
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("int a; int b() { return 1; }");
        TranslnUnit unit;
        Int32 current = Parse(src, 0, out unit);
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
    
    public static Int32 Parse(List<Token> src, Int32 pos, out TranslnUnit unit) {
        List<ExternDecln> list;
        Int32 current;
        if ((current = Parser.ParseNonEmptyList(src, pos, out list, _external_declaration.Parse)) != -1) {
            unit = new TranslnUnit(list);
            return current;
        } else {
            unit = null;
            return -1;
        }
    }
    
}
// external_declaration: function_definition | declaration
public class _external_declaration : ParseRule {
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("int a;");
        ExternDecln node;
        Int32 current = Parse(src, 0, out node);
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

    public static Int32 Parse(List<Token> src, Int32 pos, out ExternDecln node) {
        return Parser.Parse2Choices<ExternDecln, FuncDef, Decln>(src, pos, out node, _function_definition.Parse, _declaration.Parse);
    }
}


// function_definition : [declaration_specifiers]? declarator [declaration_list]? compound_statement
//
// NOTE: the optional declaration_list is for the **old-style** function prototype like this:
// +-------------------------------+
// |    Int32 foo(param1, param2)    |
// |    Int32 param1;                |
// |    char param2;               |
// |    {                          |
// |        ....                   |
// |    }                          |
// +-------------------------------+
//
// i'm **not** going to support this style. function prototypes should always be like this:
// +------------------------------------------+
// |    Int32 foo(Int32 param1, char param2) {    |
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
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("int add(int a, int b) { return a + b; }");
        FuncDef def;
        Int32 current = Parse(src, 0, out def);
        if (current == -1) {
            return false;
        }

        return true;
    }
    
    public static Int32 Parse(List<Token> src, Int32 begin, out FuncDef def) {
        // try to match declaration_specifiers, if not found, create an empty one.
        DeclnSpecs specs;
        Int32 current = _declaration_specifiers.Parse(src, begin, out specs);
        if (current == -1) {
            specs = new DeclnSpecs(new List<StorageClassSpec>(), new List<TypeSpec>(), new List<TypeQual>());
            current = begin;
        }

        // match declarator
        Declr decl;
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

        def = new FuncDef(specs, decl, stmt);
        return current;
    }
}
