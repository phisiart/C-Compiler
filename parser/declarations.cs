using System;
using System.Collections.Generic;

// declaration : declaration_specifiers [init_declarator_list]? ;
// [ return: Declaration ]
// 
public class _declaration : ParseRule {
    public static bool Test() {
        List<Token> src = Parser.GetTokensFromString("static int a = 3, *b = 0, **c = 3;");
        Declaration decl;
        int current = Parse(src, 0, out decl);
        return current != -1;
    }
    
    public static int Parse(List<Token> src, int pos, out Declaration declaration) {

        DeclarationSpecifiers decl_specs;
        int current = _declaration_specifiers.Parse(src, pos, out decl_specs);
        if (current == -1) {
            declaration = null;
            return -1;
        }

        int saved = current;
        List<InitDeclr> init_declrs;
        current = _init_declarator_list.Parse(src, current, out init_declrs);
        if (current == -1) {
            init_declrs = new List<InitDeclr>();
            current = saved;
        }

        if (!Parser.EatOperator(src, ref current, OperatorVal.SEMICOLON)) {
            declaration = null;
            return -1;
        }

        declaration = new Declaration(decl_specs, init_declrs);

        // add parser scope.
        if (decl_specs.IsTypedef()) {
            foreach (InitDeclr init_declarator in init_declrs) {
                ScopeEnvironment.AddTypedefName(init_declarator.declarator.name);
            }
        }

        return current;

    }
}




// declaration_specifiers : storage_class_specifier [declaration_specifiers]?
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
// SEMANT NOTE:
// 1. after parsing, we need to check that the type specifiers are one of the following sets:
//     void
//     char
//     signed char
//     unsigned char
//     short , signed short , short int , or signed short int
//     unsigned short , or unsigned short int
//     int , signed , signed int , or no type specifiers
//     unsigned , or unsigned int
//     long , signed long , long int , or signed long int
//     unsigned long , or unsigned long int
//     float
//     double
//     long double
//     struct-or-union specifier
//     enum-specifier
//     typedef-name
//   note that typing 'int' twice isn't allowed
// 2. you can only have **one** storage-class specifier
// 3. you can have many type qualifiers though, because it doesn't cause ambiguity
//
public class _declaration_specifiers : ParseRule {
    public static bool Test() {
        DeclarationSpecifiers decl_specs;

        var src = Parser.GetTokensFromString("typedef int long double const");
        int current = Parse(src, 0, out decl_specs);
        if (current == -1) {
            return false;
        }

        //TType type = DeclnSpecs.GetNonQualifiedType(decl_specs.type_specifiers);
        //StorageClassSpecifier storage = DeclnSpecs.GetStorageClass(decl_specs.storage_class_specifiers);

        src = Parser.GetTokensFromString("typedef typedef typedef const const");
        current = Parse(src, 0, out decl_specs);
        return current != -1;
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
    
}

// init_declarator_list : init_declarator
//                      | init_declarator_list , init_declarator
// [ note: my solution ]
// init_declarator_list : init_declarator [, init_declarator]*
//
// [ return: List<InitDeclarator> ]
// [ if fail, return empty List<InitDeclarator> ]
public class _init_declarator_list : ParseRule {
    public static int Parse(List<Token> src, int begin, out List<InitDeclr> init_declarators) {
        return Parser.ParseNonEmptyListWithSep(src, begin, out init_declarators, _init_declarator.Parse, OperatorVal.COMMA);
    }
}


// init_declarator : declarator [= initializer]?
//
// RETURN: InitDeclarator
//
// FAIL: null
//
public class _init_declarator : ParseRule {
    public static bool Test() {
        var src = Parser.GetTokensFromString("a = 3 + 4");
        InitDeclr decl;
        int current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static int ParseInitializer(List<Token> src, int begin, out Expression init) {
        if (!Parser.EatOperator(src, ref begin, OperatorVal.ASSIGN)) {
            init = null;
            return -1;
        }
        return _initializer.Parse(src, begin, out init);
    }
    
    public static int Parse(List<Token> src, int begin, out InitDeclr init_declarator) {
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
        if ((current = ParseInitializer(src, current, out init)) == -1) {
            current = saved;
            init = null;
        }

        init_declarator = new InitDeclr(declarator, init);
        return current;
    }
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
public class _storage_class_specifier : ParseRule {
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
/*
public class StorageClassSpecifier : ASTNode {
    public StorageClassSpecifier(KeywordVal _content) {
        content = _content;
    }
    public KeywordVal content;
}
*/

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
public class _type_specifier : ParseRule {
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
        StructOrUnionSpec struct_or_union_specifier;
        int current = _struct_or_union_specifier.Parse(src, begin, out struct_or_union_specifier);
        if (current != -1) {
            spec = struct_or_union_specifier;
            return current;
        }

        // 2. match enum
        EnumSpec enum_specifier;
        current = _enum_specifier.Parse(src, begin, out enum_specifier);
        if (current != -1) {
            spec = enum_specifier;
            return current;
        }

        // 3. match typedef name
        String typedef_name;
        current = begin;
        if (_typedef_name.Parse(src, ref current, out typedef_name)) {
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
            spec = new TypeSpecifier(BTypeSpec.VOID);
            return begin + 1;

        case KeywordVal.CHAR:
            spec = new TypeSpecifier(BTypeSpec.CHAR);
            return begin + 1;

        case KeywordVal.SHORT:
            spec = new TypeSpecifier(BTypeSpec.SHORT);
            return begin + 1;

        case KeywordVal.INT:
            spec = new TypeSpecifier(BTypeSpec.INT);
            return begin + 1;

        case KeywordVal.LONG:
            spec = new TypeSpecifier(BTypeSpec.LONG);
            return begin + 1;

        case KeywordVal.FLOAT:
            spec = new TypeSpecifier(BTypeSpec.FLOAT);
            return begin + 1;

        case KeywordVal.DOUBLE:
            spec = new TypeSpecifier(BTypeSpec.DOUBLE);
            return begin + 1;

        case KeywordVal.SIGNED:
            spec = new TypeSpecifier(BTypeSpec.SIGNED);
            return begin + 1;

        case KeywordVal.UNSIGNED:
            spec = new TypeSpecifier(BTypeSpec.UNSIGNED);
            return begin + 1;

        default:
            spec = null;
            return -1;
        }

    }
}

// type_qualifier : const | volatile
//
// RETURN: enum TypeQualifier
//
// FAIL: TypeQUalifier.NULL
//
// NOTE: there can be multiple type_qualifiers in one declaration
//
public class _type_qualifier : ParseRule {
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


// declarator : [pointer]? direct_declarator
//
// RETURN: Declarator
//
// FAIL: null
//
public class _declarator : ParseRule {
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

// pointer : '*' [type_qualifier_list]? [pointer]?
//
// RETURN: List<PointerInfo>
//
// FAIL: null
//
public class _pointer : ParseRule {
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
public class _parameter_type_list : ParseRule {
    public static int Parse(List<Token> src, int begin, out ParamTypeList param_type_list) {
        param_type_list = null;

        List<ParamDecln> param_list;
        int current = _parameter_list.Parse(src, begin, out param_list);
        if (current == -1) {
            return -1;
        }

        param_type_list = new ParamTypeList(param_list);

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


// parameter_list : parameter_declaration
//                | parameter_list, parameter_declaration
// [ note: my solution ]
// parameter_list : parameter_declaration < , parameter_declaration >*
// [ note: it's okay to have a lonely ',', just leave it alone ]
public class _parameter_list : ParseRule {
    public static int Parse(List<Token> src, int begin, out List<ParamDecln> param_list) {
        ParamDecln decl;
        int current = _parameter_declaration.Parse(src, begin, out decl);
        if (current == -1) {
            param_list = null;
            return -1;
        }

        param_list = new List<ParamDecln>();
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
public class _type_qualifier_list : ParseRule {
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
public class _direct_declarator : ParseRule {
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
        ParamTypeList param_type_list;
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
            
            decl = new Declarator(name);
            //decl.name = name;
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

}


// enum_specifier : enum <identifier>? { enumerator_list }
//                | enum identifier
public class _enum_specifier : ParseRule {

    // this parses { enumerator_list }
    private static int ParseEnumList(List<Token> src, int begin, out List<Enumerator> enum_list) {
        enum_list = null;
        if (!Parser.IsOperator(src[begin], OperatorVal.LCURL)) {
            return -1;
        }
        int current = begin + 1;
        current = _enumerator_list.Parse(src, current, out enum_list);
        if (current == -1) {
            return -1;
        }
        if (!Parser.IsOperator(src[begin], OperatorVal.RCURL)) {
            return -1;
        }
        current++;
        return current;
    }

    public static int Parse(List<Token> src, int begin, out EnumSpec enum_spec) {
        

        if (src[begin].type != TokenType.KEYWORD) {
            enum_spec = null;
            return -1;
        }
        if (((TokenKeyword)src[begin]).val != KeywordVal.ENUM) {
            enum_spec = null;
            return -1;
        }

        int current = begin + 1;
        List<Enumerator> enum_list;
        String name;
        if ((name = Parser.GetIdentifierValue(src[current])) != null) {
            current++;

            int saved = current;
            if ((current = ParseEnumList(src, current, out enum_list)) == -1) {
                enum_spec = new EnumSpec(name, new List<Enumerator>());
                return saved;
            } else {
                enum_spec = new EnumSpec(name, enum_list);
                return current;
            }

        } else {
            current = ParseEnumList(src, current, out enum_list);
            if (current == -1) {
                enum_spec = null;
                return -1;
            }
            enum_spec = new EnumSpec("", enum_list);
            return current;

        }
    }
}


// enumerator_list : enumerator
//                 | enumerator_list, enumerator
// [ note: my solution ]
// enumerator_list : enumerator < , enumerator >*
public class _enumerator_list : ParseRule {
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
public class _enumerator : ParseRule {
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

// enumeration_constant : identifier
public class _enumeration_constant : ParseRule {
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
public class _struct_or_union_specifier : ParseRule {
    public static int ParseDeclarationList(List<Token> src, int begin, out List<StructDecln> decl_list) {
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

    public static int Parse(List<Token> src, int begin, out StructOrUnionSpec spec) {
        spec = null;

        StructOrUnion struct_or_union;
        List<StructDecln> decl_list;

        int current = _struct_or_union.Parse(src, begin, out struct_or_union);
        if (current == -1) {
            return -1;
        }
        //current++;

        if (src[current].type == TokenType.IDENTIFIER) {
            // named struct or union

            String name = ((TokenIdentifier)src[current]).val;
            if (struct_or_union.is_union) {
                spec = new UnionSpec(name, null);
            } else {
                spec = new StructSpec(name, null);
            }
            current++;
            int saved = current;

            current = ParseDeclarationList(src, current, out decl_list);
            if (current != -1) {
                spec.declns = decl_list;
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
                spec = new UnionSpec("", decl_list);
            } else {
                spec = new StructSpec("", decl_list);
            }

            return current;

        }
    }
}

// struct_or_union : struct | union
public class _struct_or_union : ParseRule {
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

// struct_declaration_list : struct_declaration
//                         | struct_declaration_list struct_declaration
// [ note: my solution ]
// struct_declaration_list : <struct_declaration>+
public class _struct_declaration_list : ParseRule {
    public static int Parse(List<Token> src, int begin, out List<StructDecln> decl_list) {
        return Parser.ParseNonEmptyList(src, begin, out decl_list, _struct_declaration.Parse);
    }
}


// struct_declaration : specifier_qualifier_list struct_declarator_list ;
public class _struct_declaration : ParseRule {
    public static int Parse(List<Token> src, int begin, out StructDecln decl) {
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
        decl = new StructDecln(specs, decl_list);
        return current;
    }
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
//
public class _specifier_qualifier_list : ParseRule {
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

        //int current = begin;
        while (true) {
            int saved = begin;

            // 1. match type_specifier
            begin = saved;
            TypeSpecifier type_specifier;
            if ((begin = _type_specifier.Parse(src, begin, out type_specifier)) != -1) {
                type_specifiers.Add(type_specifier);
                continue;
            }

            // 2. match type_qualifier
            begin = saved;
            TypeQualifier type_qualifier;
            if ((begin = _type_qualifier.Parse(src, begin, out type_qualifier)) != -1) {
                type_qualifiers.Add(type_qualifier);
                continue;
            }

            // 3. if all failed, break out of the loop
            begin = saved;
            break;

        }

        if (type_specifiers.Count == 0 && type_qualifiers.Count == 0) {
            decl_specs = null;
            return -1;
        }

        decl_specs = new DeclarationSpecifiers(null, type_specifiers, type_qualifiers);
        return begin;

    }
    
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
public class _struct_declarator_list : ParseRule {
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
        return Parser.ParseNonEmptyListWithSep(src, begin, out decl_list, _struct_declarator.Parse, OperatorVal.COMMA);
    }
}


// struct_declarator : declarator
//                   | type_specifier <declarator>? : constant_expression
// [ note: the second is for bit-field ]
// TODO : [ note: i'm not supporting bit-field ]
public class _struct_declarator : ParseRule {
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
public class _parameter_declaration : ParseRule {
    public static bool Test() {
        var src = Parser.GetTokensFromString("int *a[]");
        ParamDecln decl;
        int current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static int Parse(List<Token> src, int begin, out ParamDecln decl) {
        // step 1. match declaration_specifiers
        DeclarationSpecifiers specs;
        if ((begin = _declaration_specifiers.Parse(src, begin, out specs)) == -1) {
            decl = null;
            return -1;
        }

        // step 2. try to match declarator
        int saved = begin;
        Declarator declarator;
        if ((begin = _declarator.Parse(src, begin, out declarator)) != -1) {
            decl = new ParamDecln(specs, declarator);
            return begin;
        }

        // if fail, step 3. try to match abstract_declarator
        begin = saved;
        //AbstractDeclarator abstract_declarator;
        if ((begin = _abstract_declarator.Parse(src, begin, out declarator)) != -1) {
            decl = new ParamDecln(specs, declarator);
            return begin;
        }

        // if fail, never mind, just return specifiers
        decl = new ParamDecln(specs, null);
        return saved;

    }
}

// identifier_list : /* old style, i'm deleting this */


// abstract_declarator : pointer
//                     | <pointer>? direct_abstract_declarator
// [ note: this is for anonymous declarator ]
// [ note: there couldn't be any typename in an abstract_declarator ]
public class _abstract_declarator : ParseRule {
    public static int Parse(List<Token> src, int begin, out Declarator decl) {
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

        decl = new Declarator("");
        decl.type_infos.AddRange(infos);
        return saved;

    }
}

/*
//public class AbstractDeclarator : ASTNode {
//    public AbstractDeclarator() {
//        type_infos = new List<TypeInfo>();
//    }
//    public List<TypeInfo> type_infos;
//}
*/

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
public class _direct_abstract_declarator : ParseRule {
    public static bool Test() {
        var src = Parser.GetTokensFromString("(*)[3][5 + 7][]");
        Declarator decl;
        int current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }

        return true;
    }

    // '(' abstract_declarator ')'
    public static int ParseAbstractDeclarator(List<Token> src, int begin, out Declarator decl) {
        if (!Parser.EatOperator(src, ref begin, OperatorVal.LPAREN)) {
            decl = null;
            return -1;
        }

        if ((begin = _abstract_declarator.Parse(src, begin, out decl)) == -1) {
            decl = null;
            return -1;
        }

        if (!Parser.EatOperator(src, ref begin, OperatorVal.RPAREN)) {
            decl = null;
            return -1;
        }

        return begin;
    }

    public static int Parse(List<Token> src, int begin, out Declarator decl) {
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

            decl = new Declarator("");
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

}


// initializer : assignment_expression
//             | '{' initializer_list '}'
//             | '{' initializer_list ',' '}'
public class _initializer : ParseRule {
    public static bool Test() {
        var src = Parser.GetTokensFromString("a = 3");
        Expression expr;
        int current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("{ a = 3, b = 4, c = 5 }");
        current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        src = Parser.GetTokensFromString("{ a = 3, b = 4, c = 5, }");
        current = Parse(src, 0, out expr);
        if (current == -1) {
            return false;
        }

        return true;
    }

    public static int Parse(List<Token> src, int begin, out Expression expr) {
        // 1. if not start with '{', we have to match assignment_expression
        if (!Parser.EatOperator(src, ref begin, OperatorVal.LCURL))
            return _assignment_expression.Parse(src, begin, out expr);

        // 2. if start with '{', match initializer_list
        if ((begin = _initializer_list.Parse(src, begin, out expr)) == -1)
            return -1;

        // 3. try to match '}'
        if (Parser.EatOperator(src, ref begin, OperatorVal.RCURL))
            return begin;
        
        // 4. if fail, try to match ',' '}'
        if (!Parser.EatOperator(src, ref begin, OperatorVal.COMMA))
            return -1;
        if (!Parser.EatOperator(src, ref begin, OperatorVal.RCURL))
            return -1;

        return begin;
    }
}


// initializer_list : initializer
//                  | initializer_list ',' initializer
//
// NOTE: this is a list
//       leave single ',' alone
//
// initializer_list : initializer [ ',' initializer ]*
//
public class _initializer_list : ParseRule {
    public static bool Test() {
        var src = Parser.GetTokensFromString("{1, 2}, {2, 3}");
        Expression init;
        int current = Parse(src, 0, out init);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static int Parse(List<Token> src, int begin, out Expression expr) {
        List<Expression> exprs;
        if ((begin = Parser.ParseNonEmptyListWithSep(src, begin, out exprs, _initializer.Parse, OperatorVal.COMMA)) == -1) {
            expr = null;
            return -1;
        }

        expr = new InitrList(exprs);
        return begin;

    }
}


// type_name : specifier_qualifier_list <abstract_declarator>?
public class _type_name : ParseRule {
    public static int Parse(List<Token> src, int begin, out TypeName type_name) {
        type_name = null;
        DeclarationSpecifiers specs;
        int current = _specifier_qualifier_list.Parse(src, begin, out specs);
        if (current == -1) {
            return -1;
        }

        int saved = current;
        Declarator decl;
        current = _abstract_declarator.Parse(src, current, out decl);
        if (current == -1) {
            type_name = new TypeName(specs, null);
            return saved;
        }
        type_name = new TypeName(specs, decl);
        return current;
    }
}

// typedef_name : identifier
//
// RETURN: String
//
// FAIL: null
//
// NOTE: must be something already defined, so this needs environment
//
public class _typedef_name : ParseRule {
    public static bool Parse(List<Token> src, ref int pos, out String name) {
        if (src[pos].type != TokenType.IDENTIFIER) {
            name = null;
            return false;
        }

        if (!ScopeEnvironment.HasTypedefName(((TokenIdentifier)src[pos]).val)) {
            name = null;
            return false;
        }

        name = ((TokenIdentifier)src[pos]).val;
        pos++;
        return true;
    }
}

