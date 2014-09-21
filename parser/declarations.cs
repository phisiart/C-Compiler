using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// declaration : declaration_specifiers [init_declarator_list]? ;
// [ return: Declaration ]
public class _declaration : PTNode {
    public static int Parse(List<Token> src, int begin, out Declaration declaration) {
        declaration = null;

        DeclarationSpecifiers declaration_specifiers;
        int current = _declaration_specifiers.Parse(src, begin, out declaration_specifiers);
        if (current == -1) {
            return -1;
        }

        int saved = current;
        List<InitDeclarator> init_declarators;
        current = _init_declarator_list.Parse(src, current, out init_declarators);
        if (current == -1) {
            current = saved;
        }

        if (src[current].type != TokenType.OPERATOR) {
            return -1;
        }
        if (((TokenOperator)(src[current])).val != OperatorVal.SEMICOLON) {
            return -1;
        }

        declaration = new Declaration();
        declaration.declaration_specifiers = declaration_specifiers;
        declaration.init_declarators = init_declarators;
        if (declaration_specifiers.IsTypedef()) {
            foreach (InitDeclarator init_declarator in init_declarators) {
                ScopeEnvironment.AddTypedefName(init_declarator.declarator.name);
            }
        }

        current++;
        return current;

    }
}

public class Declaration : ASTNode {
    public DeclarationSpecifiers declaration_specifiers;
    public List<InitDeclarator> init_declarators;
}


// declaration_specifiers : storage_class specifier [declaration_specifiers]?
//                        | type_specifier [declaration_specifiers]?
//                        | type_qualifier [declaration_specifiers]?
//
// RETURN: DeclarationSpecifiers
//
// FAIL: null
//
// NOTE:
// this is just a list, i'm turning it into:
//
// declaration_specifiers : [ storage_class_specifier | type_specifier | type_qualifier ]+
//
public class _declaration_specifiers : PTNode {
    public static bool Test() {
        DeclarationSpecifiers decl_specs;

        var src = Parser.GetTokensFromString("typedef int long const");
        int current = Parse(src, 0, out decl_specs);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("typedef typedef typedef const const");
        current = Parse(src, 0, out decl_specs);
        if (current == -1) {
            return false;
        }

        return true;
    }

    public static int Parse(List<Token> src, int begin, out DeclarationSpecifiers decl_specs) {
        List<StorageClassSpecifier> storage_class_specifiers = new List<StorageClassSpecifier>();
        List<TypeSpecifier> type_specifiers = new List<TypeSpecifier>();
        List<TypeQualifier> type_qualifiers = new List<TypeQualifier>();
        
        int current = begin;
        while (true) {
            int saved = current;

            // 1. match storage_class_specifier
            StorageClassSpecifier storage_class_specifier;
            current = _storage_class_specifier.Parse(src, current, out storage_class_specifier);
            if (current != -1) {
                storage_class_specifiers.Add(storage_class_specifier);
                continue;
            }

            // 2. if failed, match type_specifier
            current = saved;
            TypeSpecifier type_specifier;
            current = _type_specifier.Parse(src, current, out type_specifier);
            if (current != -1) {
                type_specifiers.Add(type_specifier);
                continue;
            }

            // 3. if failed, match type_qualifier
            current = saved;
            TypeQualifier type_qualifier;
            current = _type_qualifier.Parse(src, current, out type_qualifier);
            if (current != -1) {
                type_qualifiers.Add(type_qualifier);
                continue;
            }

            // 4. if all failed, break out of the loop
            current = saved;
            break;

        }

        if (storage_class_specifiers.Count == 0 && type_specifiers.Count == 0 && type_qualifiers.Count == 0) {
            decl_specs = null;
            return -1;
        }

        decl_specs = new DeclarationSpecifiers(storage_class_specifiers, type_specifiers, type_qualifiers);
        return current;

    }
    
    // the following code is deleted.

    //public static int Parse2(List<Token> src, int begin, out DeclarationSpecifiers node) {
    //    node = null;
    //    DeclarationSpecifiers.Type type;
    //    ASTNode content = null;
    //    int current;

    //    StorageClassSpecifier storage_class_specifier;
    //    if ((current = _storage_class_specifier.Parse(src, begin, out storage_class_specifier)) != -1) {
    //        // storage_class_specifier successful match
    //        content = storage_class_specifier;
    //        type = DeclarationSpecifiers.Type.STORAGE_CLASS_SPECIFIER;
    //    } else {
    //        TypeSpecifier type_specifier;
    //        if ((current = _type_specifier.Parse(src, begin, out type_specifier)) != -1) {
    //            // type_specifier successful match
    //            content = type_specifier;
    //            type = DeclarationSpecifiers.Type.TYPE_SPECIFIER;
    //        } else {
    //            TypeQualifier type_qualifier;
    //            if ((current = _type_qualifier.Parse(src, begin, out type_qualifier)) != -1) {
    //                // type_qualifier successful match
    //                content = type_qualifier;
    //                type = DeclarationSpecifiers.Type.TYPE_QUALIFIER;
    //            } else {
    //                // all fail, return
    //                return -1;
    //            }
    //        }
    //    }

    //    node = new DeclarationSpecifiers(type, content, null);
    //    // now check whether their is a next

    //    int saved = current;
    //    if ((current = _declaration_specifiers.Parse(src, current, out node.next)) != -1) {
    //        return current;
    //    } else {
    //        return saved;
    //    }

    //}
}

public class DeclarationSpecifiers : ASTNode {
    public DeclarationSpecifiers(List<StorageClassSpecifier> _storage_class_specifiers,
                                 List<TypeSpecifier> _type_specifiers,
                                 List<TypeQualifier> _type_qualifiers) {
        storage_class_specifiers = _storage_class_specifiers;
        type_qualifiers = _type_qualifiers;
        type_specifiers = _type_specifiers;
    }

    public bool IsTypedef() {
        return storage_class_specifiers.FindIndex(x => x == StorageClassSpecifier.TYPEDEF) != -1;
    }

    public List<StorageClassSpecifier> storage_class_specifiers;
    public List<TypeSpecifier> type_specifiers;
    public List<TypeQualifier> type_qualifiers;

    // The following code is deleted.

    //public DeclarationSpecifiers(Type _type, ASTNode _content, DeclarationSpecifiers _next) {
    //    type = _type;
    //    content = _content;
    //    next = _next;
    //}
    //public enum Type {
    //    STORAGE_CLASS_SPECIFIER,
    //    TYPE_SPECIFIER,
    //    TYPE_QUALIFIER
    //};
    //public bool IsTypedef2() {
    //    if (type == Type.STORAGE_CLASS_SPECIFIER) {
    //        if (((StorageClassSpecifier)content).content == KeywordVal.TYPEDEF) {
    //            return true;
    //        }
    //    }
    //    if (next == null) {
    //        return false;
    //    }
    //    return next.IsTypedef();
    //}


    //public Type type;
    //public ASTNode content;
    //public DeclarationSpecifiers next;
}


// init_declarator_list : init_declarator
//                      | init_declarator_list , init_declarator
// [ note: my solution ]
// init_declarator_list : init_declarator [, init_declarator]*
//
// [ return: List<InitDeclarator> ]
// [ if fail, return empty List<InitDeclarator> ]
public class _init_declarator_list : PTNode {
    public static int Parse(List<Token> src, int begin, out List<InitDeclarator> init_declarators) {
        init_declarators = new List<InitDeclarator>();

        InitDeclarator init_declarator;
        int current = _init_declarator.Parse(src, begin, out init_declarator);
        if (current == -1) {
            return -1;
        }

        if (src[current].type != TokenType.OPERATOR) {
            init_declarators.Add(init_declarator);
            return current;
        }
        if (((TokenOperator)src[current]).val != OperatorVal.COMMA) {
            init_declarators.Add(init_declarator);
            return current;
        }

        current++;
        int saved = current;
        current = _init_declarator_list.Parse(src, current, out init_declarators);
        init_declarators.Insert(0, init_declarator);
        if (current != -1) {
            return current;
        } else {
            return saved;
        }

    }
}


// init_declarator : declarator [= initializer]?
//
// RETURN: InitDeclarator
//
// FAIL: null
//
public class _init_declarator : PTNode {
    public static bool Test() {
        var src = Parser.GetTokensFromString("a = 3 + 4");
        InitDeclarator decl;
        int current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static int ParseInitializer(List<Token> src, int begin, out Expression init) {
        if (!Parser.IsOperator(src[begin], OperatorVal.ASSIGN)) {
            init = null;
            return -1;
        }

        begin++;
        return _initializer.Parse(src, begin, out init);
    }
    
    public static int Parse(List<Token> src, int begin, out InitDeclarator init_declarator) {
        // step 1. match declarator
        Declarator declarator;
        int current = _declarator.Parse(src, begin, out declarator);
        if (current == -1) {
            init_declarator = null;
            return -1;
        }

        // step 2. match initializer
        int saved = current;
        Expression init;
        current = ParseInitializer(src, current, out init);
        if (current == -1) {
            current = saved;
            init = null;
        }

        init_declarator = new InitDeclarator(declarator, init);
        return current;
    }
}

public class InitDeclarator : ASTNode {
    public InitDeclarator(Declarator _decl, Expression _init) {
        declarator = _decl;
        init = _init;
    }
    public Declarator declarator;
    public Expression init;
}


// storage_class_specifier : auto | register | static | extern | typedef
//
// RETURN:
// enum StorageClassSpecifier
//
// FAIL:
// StorageClassSpecifier.NULL
//
// NOTE:
// there can be only one storage class in one declaration
//
public class _storage_class_specifier : PTNode {
    public static bool Test() {
        StorageClassSpecifier decl_specs;

        var src = Parser.GetTokensFromString("typedef");
        int current = Parse(src, 0, out decl_specs);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("typedef typedef typedef const const");
        current = Parse(src, 0, out decl_specs);
        if (current == -1) {
            return false;
        }

        return true;
    }

    public static int Parse(List<Token> src, int begin, out StorageClassSpecifier spec) {
        // make sure the token is a keyword
        if (src[begin].type != TokenType.KEYWORD) {
            spec = StorageClassSpecifier.NULL;
            return -1;
        }

        // check the value
        KeywordVal val = ((TokenKeyword)src[begin]).val;
        switch (val) {
        case KeywordVal.AUTO:
            spec = StorageClassSpecifier.AUTO;
            return begin + 1;

        case KeywordVal.REGISTER:
            spec = StorageClassSpecifier.REGISTER;
            return begin + 1;

        case KeywordVal.STATIC:
            spec = StorageClassSpecifier.STATIC;
            return begin + 1;

        case KeywordVal.EXTERN:
            spec = StorageClassSpecifier.EXTERN;
            return begin + 1;

        case KeywordVal.TYPEDEF:
            spec = StorageClassSpecifier.TYPEDEF;
            return begin + 1;

        default:
            spec = StorageClassSpecifier.NULL;
            return -1;
        }
    }
}

public enum StorageClassSpecifier {
    NULL,
    AUTO,
    REGISTER,
    STATIC,
    EXTERN,
    TYPEDEF
}

//public class StorageClassSpecifier : ASTNode {
//    public StorageClassSpecifier(KeywordVal _content) {
//        content = _content;
//    }
//    public KeywordVal content;
//}


// type_specifier : void                        /* VoidSpecifier : PrimitiveTypeSpecifier */
//                | char                        /* CharSpecifier : PrimitiveTypeSpecifier */
//                | short                       /* ShortSpecifier : PrimitiveTypeSpecifier */
//                | int                         /* IntSpecifier : PrimitiveTypeSpecifier */
//                | long                        /* LongSpecifier : PrimitiveTypeSpecifier */
//                | float                       /* FloatSpecifier : PrimitiveTypeSpecifier */
//                | double                      /* DoubleSpecifier : PrimitiveTypeSpecifier */
//                | signed                      /* SignedSpecifier : PrimitiveTypeSpecifier */
//                | unsigned                    /* UnsignedSpecifier : PrimitiveTypeSpecifier */
//                | struct_or_union_specifier   /* StructOrUnionSpecifier : PrimitiveTypeSpecifier */
//                | enum_specifier              /* EnumSpecifier : PrimitiveTypeSpecifier */
//                | typedef_name                /* TypedefName : TypeSpecifier */
//
// RETURN: TypeSpecifier
//
// FAIL: null
//
// NOTE: typedef_name needs environment
//
public class _type_specifier : PTNode {
    public static bool Test() {
        TypeSpecifier spec;

        List<String> codes = new List<string> {
            "union { int a; }", "void", "char", "short", "int", "long", "float", "double", "signed", "unsigned",
            "struct { int a; }"
        };

        ScopeEnvironment.InScope();
        ScopeEnvironment.AddTypedefName("Mytype");
        var src = Parser.GetTokensFromString("Mytype");
        int current = Parse(src, 0, out spec);
        if (current == -1) {
            return false;
        }
        ScopeEnvironment.OutScope();

        foreach (var code in codes) {
            src = Parser.GetTokensFromString(code);
            current = Parse(src, 0, out spec);
            if (current == -1) {
                return false;
            }
        }

        return true;
    }

    public static int Parse(List<Token> src, int begin, out TypeSpecifier spec) {

        // 1. match struct or union
        StructOrUnionSpecifier struct_or_union_specifier;
        int current = _struct_or_union_specifier.Parse(src, begin, out struct_or_union_specifier);
        if (current != -1) {
            spec = struct_or_union_specifier;
            return current;
        }

        // 2. match enum
        EnumSpecifier enum_specifier;
        current = _enum_specifier.Parse(src, begin, out enum_specifier);
        if (current != -1) {
            spec = enum_specifier;
            return current;
        }

        // 3. match typedef name
        String typedef_name;
        current = _typedef_name.Parse(src, begin, out typedef_name);
        if (current != -1) {
            spec = new TypedefName(typedef_name);
            return current;
        }

        // now we only have keywords left
        // make sure the token is a keyword
        if (src[begin].type != TokenType.KEYWORD) {
            spec = null;
            return -1;
        }

        // check the value
        KeywordVal val = ((TokenKeyword)src[begin]).val;
        switch (val) {
        case KeywordVal.VOID:
            spec = new VoidSpecifier();
            return begin + 1;

        case KeywordVal.CHAR:
            spec = new CharSpecifier();
            return begin + 1;

        case KeywordVal.SHORT:
            spec = new ShortSpecifier();
            return begin + 1;

        case KeywordVal.INT:
            spec = new IntSpecifier();
            return begin + 1;

        case KeywordVal.LONG:
            spec = new LongSpecifier();
            return begin + 1;

        case KeywordVal.FLOAT:
            spec = new FloatSpecifier();
            return begin + 1;

        case KeywordVal.DOUBLE:
            spec = new DoubleSpecifier();
            return begin + 1;

        case KeywordVal.SIGNED:
            spec = new SignedSpecifier();
            return begin + 1;

        case KeywordVal.UNSIGNED:
            spec = new UnsignedSpecifier();
            return begin + 1;

        default:
            spec = null;
            return -1;
        }

    }
}

public class TypeSpecifier : ASTNode {}

public class PrimitiveTypeSpecifier : TypeSpecifier {}

public class IntSpecifier : PrimitiveTypeSpecifier {}

public class ShortSpecifier : PrimitiveTypeSpecifier {}

public class LongSpecifier : PrimitiveTypeSpecifier {}

public class FloatSpecifier : PrimitiveTypeSpecifier {}

public class DoubleSpecifier : PrimitiveTypeSpecifier {}

public class CharSpecifier : PrimitiveTypeSpecifier {}

// this is just temporary
public class SignedSpecifier : PrimitiveTypeSpecifier {}

// this is just temporary
public class UnsignedSpecifier : PrimitiveTypeSpecifier {}

public class VoidSpecifier : PrimitiveTypeSpecifier {}

// this is just temporary
public class TypedefName : TypeSpecifier {
    public TypedefName(String _name) {
        name = _name;
    }
    public String name;
}

// the follwing code is deleted
//public class TypeSpecifier : ASTNode {
//    public TypeSpecifier(KeywordVal _content) {
//        content = _content;
//    }
//    public KeywordVal content;
//}


// type_qualifier : const | volatile
//
// RETURN: enum TypeQualifier
//
// FAIL: TypeQUalifier.NULL
//
// NOTE: there can be multiple type_qualifiers in one declaration
//
public class _type_qualifier : PTNode {
    public static bool Test() {
        var src = Parser.GetTokensFromString("const volatile");
        TypeQualifier qualifier;
        int current = Parse(src, 0, out qualifier);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("volatile const");
        current = Parse(src, 0, out qualifier);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("haha volatile const");
        current = Parse(src, 0, out qualifier);
        if (current != -1) {
            return false;
        }

        return true;
    }
    
    public static int Parse(List<Token> src, int begin, out TypeQualifier qualifier) {

        // make sure te token is a keyword
        if (src[begin].type != TokenType.KEYWORD) {
            qualifier = TypeQualifier.NULL;
            return -1;
        }

        // check the value
        KeywordVal val = ((TokenKeyword)src[begin]).val;
        switch (val) {
        case KeywordVal.CONST:
            qualifier = TypeQualifier.CONST;
            return begin + 1;

        case KeywordVal.VOLATILE:
            qualifier = TypeQualifier.VOLATILE;
            return begin + 1;

        default:
            qualifier = TypeQualifier.NULL;
            return -1;
        }

    }
}

public enum TypeQualifier {
    NULL,
    CONST,
    VOLATILE
}


//public class TypeQualifier : ASTNode {
//    public TypeQualifier(KeywordVal _content) {
//        content = _content;
//    }
//    public KeywordVal content;
//}



// declarator : [pointer]? direct_declarator
//
// RETURN: Declarator
//
// FAIL: null
//
public class _declarator : PTNode {
    public static bool Test() {
        var src = Parser.GetTokensFromString("* const * const a[3][4]");
        Declarator decl;
        int current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static int Parse(List<Token> src, int begin, out Declarator decl) {
        
        // try to match pointer
        List<PointerInfo> pointer_infos;
        int current = _pointer.Parse(src, begin, out pointer_infos);
        if (current == -1) {
            // if fail, just create an empty list
            pointer_infos = new List<PointerInfo>();
            current = begin;
        }

        // match direct_declarator
        current = _direct_declarator.Parse(src, current, out decl);
        if (current != -1) {
            decl.type_infos.AddRange(pointer_infos);
            return current;
        } else {
            decl = null;
            return -1;
        }
    }
}

public interface TypeInfo {}

public class FunctionInfo : TypeInfo {
    public FunctionInfo(ParameterTypeList _param_type_list) {
        param_type_list = _param_type_list;
    }
    public ParameterTypeList param_type_list;
}

public class ArrayInfo : TypeInfo {
    public ArrayInfo(Expression _nelems) {
        nelems = _nelems;
    }
    public Expression nelems;
}

public class PointerInfo : TypeInfo {
    public PointerInfo(List<TypeQualifier> _type_qualifiers) {
        type_qualifiers = _type_qualifiers;
    }
    public List<TypeQualifier> type_qualifiers;
}

public class Declarator : ASTNode {
    public Declarator() {
        type_infos = new List<TypeInfo>();
    }
    public List<TypeInfo> type_infos;
    public String name;
}


// pointer : '*' [type_qualifier_list]? [pointer]?
//
// RETURN: List<PointerInfo>
//
// FAIL: null
//
public class _pointer : PTNode {
    public static bool Test() {
        var src = Parser.GetTokensFromString("* const * volatile const *");
        List<PointerInfo> infos;
        int current = Parse(src, 0, out infos);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static int Parse(List<Token> src, int begin, out List<PointerInfo> infos) {
        // match '*'
        if (!Parser.IsOperator(src[begin], OperatorVal.MULT)) {
            infos = null;
            return -1;
        }
        int current = begin + 1;

        // try to match type_qualifier_list, if fail, just create an empty list
        List<TypeQualifier> type_qualifiers;
        int saved = current;
        current = _type_qualifier_list.Parse(src, current, out type_qualifiers);
        if (current == -1) {
            current = saved;
            type_qualifiers = new List<TypeQualifier>();
        }
        PointerInfo info = new PointerInfo(type_qualifiers);

        saved = current;
        current = _pointer.Parse(src, current, out infos);
        if (current == -1) {
            infos = new List<PointerInfo>();
            infos.Add(info);
            return saved;
        } else {
            infos.Add(info);
            return current;
        }
    }
}


// parameter_type_list : parameter_list
//                     | parameter_list , ...
// [ note: my solution ]
// parameter_type_list : parameter_list < , ... >?
public class _parameter_type_list : PTNode {
    public static int Parse(List<Token> src, int begin, out ParameterTypeList param_type_list) {
        param_type_list = null;

        List<ParameterDeclaration> param_list;
        int current = _parameter_list.Parse(src, begin, out param_list);
        if (current == -1) {
            return -1;
        }

        param_type_list = new ParameterTypeList(param_list);

        if (Parser.IsCOMMA(src[current])) {
            int saved = current;
            current++;
            if (Parser.IsEllipsis(src, current)) {
                current += 3;
                param_type_list.IsVarArgs = true;
                return current;
            } else {
                current = saved;
            }
        }

        return current;
    }
}

public class ParameterTypeList : ASTNode {
    public ParameterTypeList(List<ParameterDeclaration> _param_list) {
        IsVarArgs = false;
        param_list = _param_list;
    }

    public bool IsVarArgs;
    public List<ParameterDeclaration> param_list;
}


// parameter_list : parameter_declaration
//                | parameter_list, parameter_declaration
// [ note: my solution ]
// parameter_list : parameter_declaration < , parameter_declaration >*
// [ note: it's okay to have a lonely ',', just leave it alone ]
public class _parameter_list : PTNode {
    public static int Parse(List<Token> src, int begin, out List<ParameterDeclaration> param_list) {
        ParameterDeclaration decl;
        int current = _parameter_declaration.Parse(src, begin, out decl);
        if (current == -1) {
            param_list = null;
            return -1;
        }

        param_list = new List<ParameterDeclaration>();
        param_list.Add(decl);

        int saved;
        while (true) {
            if (Parser.IsCOMMA(src[current])) {
                saved = current;
                current++;
                current = _parameter_declaration.Parse(src, current, out decl);
                if (current == -1) {
                    return saved;
                }
                param_list.Add(decl);
            } else {
                return current;
            }
        }
    }
}


// type_qualifier_list : [type_qualifier]+
// [ return: List<TypeQualifier> ]
// [ if fail, return empty List<TypeQualifier> ]
public class _type_qualifier_list : PTNode {
    public static int Parse(List<Token> src, int begin, out List<TypeQualifier> type_qualifiers) {
        type_qualifiers = new List<TypeQualifier>();

        TypeQualifier type_qualifier;
        int current = _type_qualifier.Parse(src, begin, out type_qualifier);
        if (current == -1) {
            return -1;
        }

        int saved = current;
        current = _type_qualifier_list.Parse(src, current, out type_qualifiers);
        type_qualifiers.Insert(0, type_qualifier);
        if (current != -1) {
            return current;
        } else {
            return saved;
        }

    }
}


// direct_declarator : identifier
//                   | '(' declarator ')'
//                   | direct_declarator '[' [constant_expression]? ']'
//                   | direct_declarator '(' [parameter_type_list]? ')'
//                   | direct_declarator '(' identifier_list ')'            /* old style, i'm deleting this */
//
// NOTE: the grammar [ direct_declarator '(' identifier_list ')' ] is for the **old-style** function prototype like this:
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
// so, i'm deleting this particular production and changing the grammar to:
// direct_declarator : identifier                                           /* Declarator */
//                   | '(' declarator ')'                                   /* Declarator */
//                   | direct_declarator '[' [constant_expression]? ']'     /* Declarator */
//                   | direct_declarator '(' [parameter_type_list]? ')'     /* Declarator */
//
// RETURN: Declarator
//
// FAIL: null
//
// NOTE: this grammar is left-recursive, so i'm changing it to:
// direct_declarator : [ identifier | '(' declarator ')' ] [ '[' [constant_expression]? ']' | '(' [parameter_type_list]? ')' ]*
public class _direct_declarator : PTNode {
    public static bool Test() {
        var src = Parser.GetTokensFromString("(*a)[3][5 + 7][]");
        Declarator decl;
        int current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }

        return true;
    }
    
    public static int ParseDeclarator(List<Token> src, int begin, out Declarator decl) {
        if (!Parser.IsOperator(src[begin], OperatorVal.LPAREN)) {
            decl = null;
            return -1;
        }
        begin++;

        begin = _declarator.Parse(src, begin, out decl);
        if (begin == -1) {
            decl = null;
            return -1;
        }

        if (!Parser.IsOperator(src[begin], OperatorVal.RPAREN)) {
            decl = null;
            return -1;
        }
        begin++;

        return begin;
    }

    public static int ParseArrayInfo(List<Token> src, int begin, out ArrayInfo info) {
        // match '['
        if (!Parser.IsOperator(src[begin], OperatorVal.LBRACKET)) {
            info = null;
            return -1;
        }
        begin++;
        
        // match constant_expression, if fail, just put null
        Expression nelems;
        int saved = begin;
        begin = _constant_expression.Parse(src, begin, out nelems);
        if (begin == -1) {
            nelems = null;
            begin = saved;
        }

        // match ']'
        if (!Parser.IsOperator(src[begin], OperatorVal.RBRACKET)) {
            info = null;
            return -1;
        }
        begin++;

        info = new ArrayInfo(nelems);
        return begin;
    }

    public static int ParseFunctionInfo(List<Token> src, int begin, out FunctionInfo info) {
        // match '('
        if (!Parser.IsOperator(src[begin], OperatorVal.LPAREN)) {
            info = null;
            return -1;
        }
        begin++;

        // match constant_expression, if fail, just put null
        ParameterTypeList param_type_list;
        int saved = begin;
        begin = _parameter_type_list.Parse(src, begin, out param_type_list);
        if (begin == -1) {
            param_type_list = null;
            begin = saved;
        }

        // match ')'
        if (!Parser.IsOperator(src[begin], OperatorVal.RPAREN)) {
            info = null;
            return -1;
        }
        begin++;

        info = new FunctionInfo(param_type_list);
        return begin;
    }

    public static int ParseTypeInfo(List<Token> src, int begin, out TypeInfo info) {
        ArrayInfo array_info;
        int current = ParseArrayInfo(src, begin, out array_info);
        if (current != -1) {
            info = array_info;
            return current;
        }

        FunctionInfo function_info;
        current = ParseFunctionInfo(src, begin, out function_info);
        if (current != -1) {
            info = function_info;
            return current;
        }

        info = null;
        return -1;
    }
    
    public static int Parse(List<Token> src, int begin, out Declarator decl) {

        // 1. match id | '(' declarator ')'
        // 1.1. try '(' declarator ')'
        int current = ParseDeclarator(src, begin, out decl);
        if (current == -1) {
            // if fail, 1.2. try id
            if (src[begin].type != TokenType.IDENTIFIER) {
                decl = null;
                return -1;
            }
            String name = ((TokenIdentifier)src[begin]).val;
            current = begin + 1;
            
            decl = new Declarator();
            decl.name = name;
        }

        // now match infos
        int saved;
        while (true) {
            TypeInfo info;
            saved = current;
            current = ParseTypeInfo(src, current, out info);
            if (current != -1) {
                decl.type_infos.Add(info);
                continue;
            }

            current = saved;
            return current;
        }
    }

    // the following code is deleted
    //public static int Parse2(List<Token> src, int begin, out Declarator node) {
    //    node = null;
    //    int current;

    //    // match id | ( declarator )
    //    if (src[begin].type == TokenType.IDENTIFIER) {
    //        // match id
    //        String name = ((TokenIdentifier)src[begin]).val;
    //        //if (ScopeEnvironment.IsNewId(name)) {
    //        // ScopeEnvironment.AddVar(name);
    //        if (true) {
    //            node = new Declarator();
    //            node.name = name;
    //            current = begin + 1;
    //        } else {
    //            // have seen this identifier, not good
    //            return -1;
    //        }
    //    } else if (src[begin].type == TokenType.OPERATOR) {
    //        // match ( declarator )
    //        if (((TokenOperator)src[begin]).val != OperatorVal.LPAREN) {
    //            return -1;
    //        }
    //        current = begin + 1;
    //        current = _direct_declarator.Parse(src, current, out node);
    //        if (current == -1) {
    //            return -1;
    //        }
    //        if (src[current].type != TokenType.OPERATOR) {
    //            return -1;
    //        }
    //        if (((TokenOperator)src[current]).val != OperatorVal.RPAREN) {
    //            return -1;
    //        }
    //        current++;
    //    } else {
    //        return -1;
    //    }

    //    if (src[current].type != TokenType.OPERATOR) {
    //        return current;
    //    }
    //    while (src[current].type == TokenType.OPERATOR) {
    //        OperatorVal op = ((TokenOperator)src[current]).val;
    //        if (op == OperatorVal.LPAREN) {
    //            current++;
    //            op = ((TokenOperator)src[current]).val;
    //            if (op == OperatorVal.RPAREN) {
    //                current++;
    //                node.type_infos.Add(new FunctionInfo());
    //            } else {
    //                return -1;
    //            }
    //        } else if (op == OperatorVal.LBRACKET) {
    //            current++;
    //            op = ((TokenOperator)src[current]).val;
    //            if (op == OperatorVal.RBRACKET) {
    //                current++;
    //                node.type_infos.Add(new ArrayInfo());
    //            } else {
    //                return -1;
    //            }
    //        } else {
    //            return current;
    //        }
    //    }
    //    return current;
    //}
}


// enum_specifier : enum <identifier>? { enumerator_list }
//                | enum identifier
public class _enum_specifier : PTNode {

    // this parses { enumerator_list }
    private static int ParseEnumList(List<Token> src, int begin, out List<Enumerator> enum_list) {
        enum_list = null;
        if (!Parser.IsLCURL(src[begin])) {
            return -1;
        }
        int current = begin + 1;
        current = _enumerator_list.Parse(src, current, out enum_list);
        if (current == -1) {
            return -1;
        }
        if (!Parser.IsRCURL(src[current])) {
            return -1;
        }
        current++;
        return current;
    }

    public static int Parse(List<Token> src, int begin, out EnumSpecifier enum_spec) {
        enum_spec = null;
        if (src[begin].type != TokenType.KEYWORD) {
            return -1;
        }
        if (((TokenKeyword)src[begin]).val != KeywordVal.ENUM) {
            return -1;
        }

        int current = begin + 1;
        List<Enumerator> enum_list;
        if (src[current].type == TokenType.IDENTIFIER) {
            enum_spec = new EnumSpecifier(((TokenIdentifier)src[current]).val, null);
            current++;
            int saved = current;
            current = ParseEnumList(src, current, out enum_list);
            if (current == -1) {
                return saved;
            }
            enum_spec.enum_list = enum_list;
            return current;

        } else {
            current = ParseEnumList(src, current, out enum_list);
            if (current == -1) {
                return -1;
            }
            enum_spec = new EnumSpecifier("", enum_list);
            return current;

        }
    }
}

public class EnumSpecifier : TypeSpecifier {
    public EnumSpecifier(String _name, List<Enumerator> _enum_list) {
        name = _name;
        enum_list = _enum_list;
    }
    public String name;
    public List<Enumerator> enum_list;
}


// enumerator_list : enumerator
//                 | enumerator_list, enumerator
// [ note: my solution ]
// enumerator_list : enumerator < , enumerator >*
public class _enumerator_list : PTNode {
    public static int Parse(List<Token> src, int begin, out List<Enumerator> enum_list) {
        Enumerator enumerator;
        enum_list = new List<Enumerator>();
        int current = _enumerator.Parse(src, begin, out enumerator);
        if (current == -1) {
            return -1;
        }
        enum_list.Add(enumerator);
        int saved;

        while (true) {
            if (Parser.IsCOMMA(src[current])) {
                saved = current;
                current++;
                current = _enumerator.Parse(src, current, out enumerator);
                if (current == -1) {
                    return saved;
                }
                enum_list.Add(enumerator);
            } else {
                return current;
            }
        }
    }
}


// enumerator : enumeration_constant
//            | enumeration_constant = constant_expression
// [ note: my solution ]
// enumerator : enumeration_constant < = constant_expression >?
public class _enumerator : PTNode {
    public static int Parse(List<Token> src, int begin, out Enumerator enumerator) {
        int current = _enumeration_constant.Parse(src, begin, out enumerator);
        if (current == -1) {
            return -1;
        }

        if (Parser.IsAssignment(src[current])) {
            current++;
            Expression init;
            current = _constant_expression.Parse(src, current, out init);
            if (current == -1) {
                return -1;
            }

            enumerator.init = init;
            return current;
        }

        return current;
    }
}

public class Enumerator : ASTNode {
    public Enumerator(String _name, Expression _init) {
        name = _name;
        init = _init;
    }
    public Expression init;
    public String name;
}


// enumeration_constant : identifier
public class _enumeration_constant : PTNode {
    public static int Parse(List<Token> src, int begin, out Enumerator enumerator) {
        if (src[begin].type == TokenType.IDENTIFIER) {
            enumerator = new Enumerator(((TokenIdentifier)src[begin]).val, null);
            return begin + 1;
        }
        enumerator = null;
        return -1;
    }
}


// struct_or_union_specifier : struct_or_union <identifier>? { struct_declaration_list }
//                           | struct_or_union identifier
// [ note: need some treatment ]
public class _struct_or_union_specifier : PTNode {
    public static int ParseDeclarationList(List<Token> src, int begin, out List<StructDecleration> decl_list) {
        decl_list = null;

        if (!Parser.IsLCURL(src[begin])) {
            return -1;
        }
        int current = begin + 1;
        current = _struct_declaration_list.Parse(src, current, out decl_list);
        if (current == -1) {
            return -1;
        }

        if (!Parser.IsRCURL(src[current])) {
            return -1;
        }
        current++;
        return current;

    }

    public static int Parse(List<Token> src, int begin, out StructOrUnionSpecifier spec) {
        spec = null;

        StructOrUnion struct_or_union;
        List<StructDecleration> decl_list;

        int current = _struct_or_union.Parse(src, begin, out struct_or_union);
        if (current == -1) {
            return -1;
        }
        //current++;

        if (src[current].type == TokenType.IDENTIFIER) {
            // named struct or union

            String name = ((TokenIdentifier)src[current]).val;
            if (struct_or_union.is_union) {
                spec = new UnionSpecifier(name, null);
            } else {
                spec = new StructSpecifier(name, null);
            }
            current++;
            int saved = current;

            current = ParseDeclarationList(src, current, out decl_list);
            if (current != -1) {
                spec.decl_list = decl_list;
                return current;
            }

            return current;

        } else {
            // anonymous struct or union

            current = ParseDeclarationList(src, current, out decl_list);
            if (current == -1) {
                return -1;
            }

            if (struct_or_union.is_union) {
                spec = new UnionSpecifier("", decl_list);
            } else {
                spec = new StructSpecifier("", decl_list);
            }

            return current;

        }
    }
}

public class StructOrUnionSpecifier : TypeSpecifier {
    public String name;
    public List<StructDecleration> decl_list;
}

public class StructSpecifier : StructOrUnionSpecifier {
    public StructSpecifier(String _name, List<StructDecleration> _decl_list) {
        name = _name;
        decl_list = _decl_list;
    }
}

public class UnionSpecifier : StructOrUnionSpecifier {
    public UnionSpecifier(String _name, List<StructDecleration> _decl_list) {
        name = _name;
        decl_list = _decl_list;
    }
}


// struct_or_union : struct | union
public class _struct_or_union : PTNode {
    public static int Parse(List<Token> src, int begin, out StructOrUnion struct_or_union) {
        struct_or_union = null;
        if (src[begin].type != TokenType.KEYWORD) {
            return -1;
        }
        switch (((TokenKeyword)src[begin]).val) {
        case KeywordVal.STRUCT:
            struct_or_union = new StructOrUnion(false);
            return begin + 1;
        case KeywordVal.UNION:
            struct_or_union = new StructOrUnion(true);
            return begin + 1;
        default:
            return -1;
        }
    }
}

public class StructOrUnion : ASTNode {
    public StructOrUnion(bool _is_union) {
        is_union = _is_union;
    }
    public bool is_union;
}


// struct_declaration_list : struct_declaration
//                         | struct_declaration_list struct_declaration
// [ note: my solution ]
// struct_declaration_list : <struct_declaration>+
public class _struct_declaration_list : PTNode {
    public static int Parse(List<Token> src, int begin, out List<StructDecleration> decl_list) {
        decl_list = new List<StructDecleration>();

        StructDecleration decl;
        int current = _struct_declaration.Parse(src, begin, out decl);
        if (current == -1) {
            return -1;
        }
        decl_list.Add(decl);

        int saved;
        while (true) {
            saved = current;
            current = _struct_declaration.Parse(src, current, out decl);
            if (current == -1) {
                return saved;
            }
            decl_list.Add(decl);
        }
    }
}


// struct_declaration : specifier_qualifier_list struct_declarator_list ;
public class _struct_declaration : PTNode {
    public static int Parse(List<Token> src, int begin, out StructDecleration decl) {
        decl = null;

        DeclarationSpecifiers specs;
        List<Declarator> decl_list;
        int current = _specifier_qualifier_list.Parse(src, begin, out specs);
        if (current == -1) {
            return -1;
        }
        current = _struct_declarator_list.Parse(src, current, out decl_list);
        if (current == -1) {
            return -1;
        }
        if (!Parser.IsSEMICOLON(src[current])) {
            return -1;
        }

        current++;
        decl = new StructDecleration(specs, decl_list);
        return current;
    }
}

public class StructDecleration : ASTNode {
    public StructDecleration(DeclarationSpecifiers _specs, List<Declarator> _decl_list) {
        specs = _specs;
        decl_list = _decl_list;
    }
    public DeclarationSpecifiers specs;
    public List<Declarator> decl_list;
}

// specifier_qualifier_list : type_specifier [specifier_qualifier_list]?
//                          | type_qualifier [specifier_qualifier_list]?
//
// RETURN: DeclarationSpecifiers
//
// FAIL: null
//
// NOTE: this is simply a list
// specifier_qualifier_list : [ type_specifier | type_qualifier ]+
public class _specifier_qualifier_list : PTNode {
    public static bool Test() {
        var src = Parser.GetTokensFromString("int long const");
        DeclarationSpecifiers specs;
        int current = Parse(src, 0, out specs);
        if (current == -1) {
            return false;
        }

        return true;
    }
    
    public static int Parse(List<Token> src, int begin, out DeclarationSpecifiers decl_specs) {
        List<TypeSpecifier> type_specifiers = new List<TypeSpecifier>();
        List<TypeQualifier> type_qualifiers = new List<TypeQualifier>();

        int current = begin;
        while (true) {
            int saved = current;

            // 1. match type_specifier
            current = saved;
            TypeSpecifier type_specifier;
            current = _type_specifier.Parse(src, current, out type_specifier);
            if (current != -1) {
                type_specifiers.Add(type_specifier);
                continue;
            }

            // 2. match type_qualifier
            current = saved;
            TypeQualifier type_qualifier;
            current = _type_qualifier.Parse(src, current, out type_qualifier);
            if (current != -1) {
                type_qualifiers.Add(type_qualifier);
                continue;
            }

            // 3. if all failed, break out of the loop
            current = saved;
            break;

        }

        if (type_specifiers.Count == 0 && type_qualifiers.Count == 0) {
            decl_specs = null;
            return -1;
        }

        decl_specs = new DeclarationSpecifiers(null, type_specifiers, type_qualifiers);
        return current;

    }
    
    // the following code is deleted
    //public static int Parse2(List<Token> src, int begin, out DeclarationSpecifiers node) {
    //    node = null;
    //    DeclarationSpecifiers.Type type;
    //    ASTNode content = null;
    //    int current;


    //    TypeSpecifier type_specifier;
    //    if ((current = _type_specifier.Parse(src, begin, out type_specifier)) != -1) {
    //        // type_specifier successful match
    //        content = type_specifier;
    //        type = DeclarationSpecifiers.Type.TYPE_SPECIFIER;
    //    } else {
    //        TypeQualifier type_qualifier;
    //        if ((current = _type_qualifier.Parse(src, begin, out type_qualifier)) != -1) {
    //            // type_qualifier successful match
    //            content = type_qualifier;
    //            type = DeclarationSpecifiers.Type.TYPE_QUALIFIER;
    //        } else {
    //            // all fail, return
    //            return -1;
    //        }
    //    }


    //    node = new DeclarationSpecifiers(type, content, null);
    //    // now check whether their is a next

    //    int saved = current;
    //    if ((current = _specifier_qualifier_list.Parse(src, current, out node.next)) != -1) {
    //        return current;
    //    } else {
    //        return saved;
    //    }

    //}
}


// struct_declarator_list : struct_declarator
//                        | struct_declarator_list ',' struct_declarator
//
// NOTE:
// this grammar is left recursive, and i'm turning it into a list
// struct_declarator_list : struct_declarator [ ',' struct_declarator ]*
//
// RETURN: List<Declarator>
//
// FAIL: null
//
public class _struct_declarator_list : PTNode {
    public static bool Test() {
        var src = Parser.GetTokensFromString("*a, *b[3]");
        List<Declarator> decl_list;
        int current = Parse(src, 0, out decl_list);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static int Parse(List<Token> src, int begin, out List<Declarator> decl_list) {
        Declarator decl;
        
        // match the first declarator
        int current = _struct_declarator.Parse(src, begin, out decl);
        if (current == -1) {
            decl_list = null;
            return -1;
        }
        decl_list = new List<Declarator>();
        decl_list.Add(decl);

        // try to match more
        int saved;
        while (true) {
            if (Parser.IsOperator(src[current], OperatorVal.COMMA)) {
                saved = current;
                current++;
                current = _struct_declarator.Parse(src, current, out decl);
                if (current == -1) {
                    return saved;
                }
                decl_list.Add(decl);
            } else {
                return current;
            }
        }
    }
}


// struct_declarator : declarator
//                   | type_specifier <declarator>? : constant_expression
// [ note: the second is for bit-field ]
// [ note: i'm not supporting bit-field ]
public class _struct_declarator : PTNode {
    public static int Parse(List<Token> src, int begin, out Declarator decl) {
        return _declarator.Parse(src, begin, out decl);
    }
}


// parameter_declaration : declaration_specifiers declarator
//                       | declaration_specifiers [abstract_declarator]?
//
// RETURN: ParameterDeclaration
//
// FAIL: null
//
public class _parameter_declaration : PTNode {
    public static bool Test() {
        var src = Parser.GetTokensFromString("int *a[]");
        ParameterDeclaration decl;
        int current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static int Parse(List<Token> src, int begin, out ParameterDeclaration decl) {

        // step 1. match declaration_specifiers
        DeclarationSpecifiers specs;
        int current = _declaration_specifiers.Parse(src, begin, out specs);
        if (current == -1) {
            decl = null;
            return -1;
        }

        // step 2. try to match declarator
        int saved = current;
        Declarator declarator;
        current = _declarator.Parse(src, current, out declarator);
        if (current != -1) {
            decl = new ParameterDeclaration(specs, declarator);
            return current;
        }

        // if fail, step 3. try to match abstract_declarator
        current = saved;
        AbstractDeclarator abstract_declarator;
        current = _abstract_declarator.Parse(src, current, out abstract_declarator);
        if (current != -1) {
            decl = new ParameterDeclaration(specs, abstract_declarator);
            return current;
        }

        // if fail, never mind, just return specifiers
        decl = new ParameterDeclaration(specs);
        return saved;

    }
}

public class ParameterDeclaration : ASTNode {
    public ParameterDeclaration(DeclarationSpecifiers _specs) {
        specs = _specs;
        decl = null;
        abstract_decl = null;
    }

    public ParameterDeclaration(DeclarationSpecifiers _specs, Declarator _decl) {
        specs = _specs;
        decl = _decl;
        abstract_decl = null;
    }

    public ParameterDeclaration(DeclarationSpecifiers _specs, AbstractDeclarator _decl) {
        specs = _specs;
        decl = null;
        abstract_decl = _decl;
    }

    public DeclarationSpecifiers specs;
    public Declarator decl;
    public AbstractDeclarator abstract_decl;
}


// identifier_list : /* old style, i'm deleting this */


// abstract_declarator : pointer
//                     | <pointer>? direct_abstract_declarator
// [ note: this is for anonymous declarator ]
// [ note: there couldn't be any typename in an abstract_declarator ]
public class _abstract_declarator : PTNode {
    public static int Parse(List<Token> src, int begin, out AbstractDeclarator decl) {
        List<PointerInfo> infos;
        int current = _pointer.Parse(src, begin, out infos);
        if (current == -1) {
            return _direct_abstract_declarator.Parse(src, begin, out decl);
        }

        int saved = current;
        current = _direct_abstract_declarator.Parse(src, current, out decl);
        if (current != -1) {
            decl.type_infos.AddRange(infos);
            return current;
        }

        decl = new AbstractDeclarator();
        decl.type_infos.AddRange(infos);
        return saved;

    }
}

public class AbstractDeclarator : ASTNode {
    public AbstractDeclarator() {
        type_infos = new List<TypeInfo>();
    }
    public List<TypeInfo> type_infos;
}

// direct_abstract_declarator : '(' abstract_declarator ')'
//                            | [direct_abstract_declarator]? '[' [constant_expression]? ']'
//                            | [direct_abstract_declarator]? '(' [parameter_type_list]? ')'
//
// NOTE: this grammar is left-recursive, so i'm turning it to:
// direct_abstract_declarator : [ '(' abstract_declarator ')' | '[' [constant_expression]? ']' | '(' [parameter_type_list]? ')' ] [ '[' [constant_expression]? ']' | '(' [parameter_type_list]? ')' ]*
//
// RETURN: AbstratDeclarator
//
// FAIL: null
//
public class _direct_abstract_declarator : PTNode {
    public static bool Test() {
        var src = Parser.GetTokensFromString("(*)[3][5 + 7][]");
        AbstractDeclarator decl;
        int current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }

        return true;
    }
    
    private static int ParseInfo(List<Token> src, int begin, out TypeInfo info) {
        info = null;
        int current;
        if (Parser.IsLPAREN(src[begin])) {
            current = begin + 1;
            if (Parser.IsRPAREN(src[current])) {
                info = new FunctionInfo(null);
                current++;
                return current;
            }
        }
        if (Parser.IsLBRACKET(src[begin])) {
            current = begin + 1;
            if (Parser.IsRBRACKET(src[current])) {
                info = new ArrayInfo(null);
                current++;
                return current;
            }
        }
        return -1;
    }

    public static int ParseAbstractDeclarator(List<Token> src, int begin, out AbstractDeclarator decl) {
        if (!Parser.IsOperator(src[begin], OperatorVal.LPAREN)) {
            decl = null;
            return -1;
        }
        begin++;

        begin = _abstract_declarator.Parse(src, begin, out decl);
        if (begin == -1) {
            decl = null;
            return -1;
        }

        if (!Parser.IsOperator(src[begin], OperatorVal.RPAREN)) {
            decl = null;
            return -1;
        }
        begin++;

        return begin;
    }
    
    public static int Parse(List<Token> src, int begin, out AbstractDeclarator decl) {
        // 1. match typeinfo | '(' abstract_declarator ')'
        // 1.1 try '(' abstract_declarator ')'
        int current = ParseAbstractDeclarator(src, begin, out decl);
        if (current == -1) {
            // if fail, 1.2. try typeinfo
            TypeInfo info;
            current = _direct_declarator.ParseTypeInfo(src, begin, out info);
            if (current == -1) {
                decl = null;
                return -1;
            }

            decl = new AbstractDeclarator();
            decl.type_infos.Add(info);
        }

        // now match infos
        int saved;
        while (true) {
            TypeInfo info;
            saved = current;
            current = _direct_declarator.ParseTypeInfo(src, current, out info);
            if (current != -1) {
                decl.type_infos.Add(info);
                continue;
            }

            current = saved;
            return current;
        }
        
    }

    // the following code is deleted
    //public static int Parse2(List<Token> src, int begin, out AbstractDeclarator decl) {
    //    TypeInfo info;
    //    List<TypeInfo> type_infos;
    //    int current;
    //    int saved;

    //    if (Parser.IsLPAREN(src[begin])) {
    //        current = begin + 1;
    //        current = _abstract_declarator.Parse(src, current, out decl);
    //        if (current != -1) {
    //            type_infos = decl.type_infos;
    //            if (!Parser.IsRPAREN(src[current])) {
    //                decl = null;
    //                return -1;
    //            }
    //            current++;

    //            while (true) {
    //                saved = current;
    //                current = ParseInfo(src, current, out info);
    //                if (current == -1) {
    //                    decl = new AbstractDeclarator();
    //                    decl.type_infos = type_infos;
    //                    return saved;
    //                }
    //                type_infos.Add(info);
    //            }
    //        }
    //    }

    //    // if not start with abstract declarator
    //    type_infos = new List<TypeInfo>();
    //    current = ParseInfo(src, begin, out info);
    //    if (current == -1) {
    //        decl = null;
    //        return -1;
    //    }
    //    type_infos.Add(info);
    //    while (true) {
    //        saved = current;
    //        current = ParseInfo(src, current, out info);
    //        if (current == -1) {
    //            decl = new AbstractDeclarator();
    //            decl.type_infos = type_infos;
    //            return saved;
    //        }
    //        type_infos.Add(info);
    //    }

    //}
}


// initializer : assignment_expression
//             | { initializer_list }
//             | { initializer_list , }
public class _initializer : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        if (!Parser.IsLCURL(src[begin])) {
            return _assignment_expression.Parse(src, begin, out node);
        }

        int current = begin + 1;
        current = _initializer_list.Parse(src, current, out node);
        if (current == -1) {
            return -1;
        }

        if (Parser.IsRCURL(src[current])) {
            current++;
            return current;
        }

        if (!Parser.IsCOMMA(src[current])) {
            return -1;
        }

        current++;
        if (!Parser.IsRCURL(src[current])) {
            return -1;
        }

        current++;
        return current;
    }
}


// initializer_list : initializer
//                  | initializer_list , initializer
// [ note: my solution ]
// initializer_list : initializer < , initializer >*
// [ leave single ',' alone ]
public class _initializer_list : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        node = null;
        Expression expr;
        List<Expression> exprs = new List<Expression>();
        int current = _initializer.Parse(src, begin, out expr);
        if (current == -1) {
            return -1;
        }
        exprs.Add(expr);
        int saved;

        while (true) {
            if (Parser.IsCOMMA(src[current])) {
                saved = current;
                current++;
                current = _initializer.Parse(src, current, out expr);
                if (current == -1) {
                    node = new InitializerList(exprs);
                    return saved;
                }
                exprs.Add(expr);
            } else {
                node = new InitializerList(exprs);
                return current;
            }
        }
    }
}

public class InitializerList : Expression {
    public InitializerList(List<Expression> _exprs) {
        exprs = _exprs;
    }
    public List<Expression> exprs;
}


// type_name : specifier_qualifier_list <abstract_declarator>?
public class _type_name : PTNode {
    public static int Parse(List<Token> src, int begin, out TypeName type_name) {
        type_name = null;
        DeclarationSpecifiers specs;
        int current = _specifier_qualifier_list.Parse(src, begin, out specs);
        if (current == -1) {
            return -1;
        }

        int saved = current;
        AbstractDeclarator decl;
        current = _abstract_declarator.Parse(src, current, out decl);
        if (current == -1) {
            type_name = new TypeName(specs, null);
            return saved;
        }
        type_name = new TypeName(specs, decl);
        return current;
    }
}

public class TypeName : ASTNode {
    public TypeName(DeclarationSpecifiers _specs, AbstractDeclarator _decl) {
        specs = _specs;
        decl = _decl;
    }
    public DeclarationSpecifiers specs;
    public AbstractDeclarator decl;
}


// typedef_name : identifier
//
// RETURN: String
//
// FAIL: null
//
// NOTE: must be something already defined, so this needs environment
//
public class _typedef_name : PTNode {
    public static int Parse(List<Token> src, int begin, out String name) {
        if (src[begin].type != TokenType.IDENTIFIER) {
            name = null;
            return -1;
        }
        if (!ScopeEnvironment.HasTypedefName(((TokenIdentifier)src[begin]).val)) {
            name = null;
            return -1;
        }

        name = ((TokenIdentifier)src[begin]).val;
        return begin + 1;
    }
}

