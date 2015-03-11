using System;
using System.Collections.Generic;
using SyntaxTree;
using System.Linq;


/// <summary>
/// declaration
///   : declaration_specifiers [init_declarator_list]? ';'
/// </summary>
public class _declaration : ParseRule {
    public static Boolean Test() {
        List<Token> src = Parser.GetTokensFromString("static int a = 3, *b = 0, **c = 3;");
        Declaration decl;
        Int32 current = Parse(src, 0, out decl);
        return current != -1;
    }
    
    public static Int32 Parse(List<Token> src, Int32 begin, out Declaration declaration) {
        return Parser.ParseSequence(src, begin, out declaration,

            // declaration_specifiers
            _declaration_specifiers.Parse,

            // [init_declarator_list]?
            Parser.GetOptionalParser(new List<InitializationDeclarator>(), _init_declarator_list.Parse),

            // ';'
            Parser.GetOperatorParser(OperatorVal.SEMICOLON),


            (DeclarationSpecifiers decln_specs, List<InitializationDeclarator> init_declrs, Boolean _) => {
                if (decln_specs.IsTypedef()) {
                    foreach (InitializationDeclarator init_declr in init_declrs) {
                        ParserEnvironment.AddTypedefName(init_declr.declr.declr_name);
                    }
                }
                return new Declaration(decln_specs, init_declrs);
            }
        );
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
//     unsigned short , or unsigned short Int32
//     int , signed , signed int , or no type specifiers
//     unsigned , or unsigned Int32
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
    public static Boolean Test() {
        DeclarationSpecifiers decl_specs;

        var src = Parser.GetTokensFromString("typedef Int32 long double const");
        Int32 current = Parse(src, 0, out decl_specs);
        if (current == -1) {
            return false;
        }
        src = Parser.GetTokensFromString("typedef typedef typedef const const");
        current = Parse(src, 0, out decl_specs);
        return current != -1;
    }


    public static Int32 Parse(List<Token> src, Int32 begin, out DeclarationSpecifiers decl_specs) {
        List<StorageClassSpecifier> storage_class_specifiers = new List<StorageClassSpecifier>();
        List<TypeSpecifier> type_specifiers = new List<TypeSpecifier>();
        List<TypeQualifier> type_qualifiers = new List<TypeQualifier>();
        
        Int32 current = begin;
        while (true) {
            Int32 saved = current;

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


/// <summary>
/// init_declarator_list
///   : init_declarator [ ',' init_declarator ]*
/// 
/// a non-empty list of init_declarators separated by ','
/// </summary>
public class _init_declarator_list : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out List<InitializationDeclarator> init_declarators) {
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
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("a = 3 + 4");
        InitializationDeclarator decl;
        Int32 current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static Int32 ParseInitializer(List<Token> src, Int32 begin, out Expression init) {
        if (!Parser.EatOperator(src, ref begin, OperatorVal.ASSIGN)) {
            init = null;
            return -1;
        }
        return _initializer.Parse(src, begin, out init);
    }
    
    public static Int32 Parse(List<Token> src, Int32 begin, out InitializationDeclarator init_declarator) {
        // step 1. match declarator
        Declarator declarator;
        Int32 current = _declarator.Parse(src, begin, out declarator);
        if (current == -1) {
            init_declarator = null;
            return -1;
        }

        // step 2. match initializer
        Int32 saved = current;
        Expression init;
        if ((current = ParseInitializer(src, current, out init)) == -1) {
            current = saved;
            init = null;
        }

        init_declarator = new InitializationDeclarator(declarator, init);
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
    public static Boolean Test() {
        StorageClassSpecifier decl_specs;

        var src = Parser.GetTokensFromString("typedef");
        Int32 current = Parse(src, 0, out decl_specs);
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

    public static Int32 Parse(List<Token> src, Int32 begin, out StorageClassSpecifier spec) {
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

// type_specifier : void                        /* VoidSpecifier : PrimitiveTypeSpecifier */
//                | char                        /* CharSpecifier : PrimitiveTypeSpecifier */
//                | short                       /* ShortSpecifier : PrimitiveTypeSpecifier */
//                | Int32                         /* IntSpecifier : PrimitiveTypeSpecifier */
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
    public static Boolean Test() {
        TypeSpecifier spec;

        List<String> codes = new List<String> {
            "union { int a; }", "void", "char", "short", "int", "long", "float", "double", "signed", "unsigned",
            "struct { int a; }"
        };

        ParserEnvironment.InScope();
        ParserEnvironment.AddTypedefName("Mytype");
        var src = Parser.GetTokensFromString("Mytype");
        Int32 current = Parse(src, 0, out spec);
        if (current == -1) {
            return false;
        }
        ParserEnvironment.OutScope();

        foreach (var code in codes) {
            src = Parser.GetTokensFromString(code);
            current = Parse(src, 0, out spec);
            if (current == -1) {
                return false;
            }
        }

        return true;
    }

    public static Int32 Parse(List<Token> src, Int32 begin, out TypeSpecifier spec) {

        // 1. match struct or union
        StructOrUnionSpecifier struct_or_union_specifier;
        Int32 current = _struct_or_union_specifier.Parse(src, begin, out struct_or_union_specifier);
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
            spec = new TypeSpecifier(BasicTypeSpecifier.VOID);
            return begin + 1;

        case KeywordVal.CHAR:
            spec = new TypeSpecifier(BasicTypeSpecifier.CHAR);
            return begin + 1;

        case KeywordVal.SHORT:
            spec = new TypeSpecifier(BasicTypeSpecifier.SHORT);
            return begin + 1;

        case KeywordVal.INT:
            spec = new TypeSpecifier(BasicTypeSpecifier.INT);
            return begin + 1;

        case KeywordVal.LONG:
            spec = new TypeSpecifier(BasicTypeSpecifier.LONG);
            return begin + 1;

        case KeywordVal.FLOAT:
            spec = new TypeSpecifier(BasicTypeSpecifier.FLOAT);
            return begin + 1;

        case KeywordVal.DOUBLE:
            spec = new TypeSpecifier(BasicTypeSpecifier.DOUBLE);
            return begin + 1;

        case KeywordVal.SIGNED:
            spec = new TypeSpecifier(BasicTypeSpecifier.SIGNED);
            return begin + 1;

        case KeywordVal.UNSIGNED:
            spec = new TypeSpecifier(BasicTypeSpecifier.UNSIGNED);
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
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("const volatile");
        TypeQualifier qualifier;
        Int32 current = Parse(src, 0, out qualifier);
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
    
    public static Int32 Parse(List<Token> src, Int32 begin, out TypeQualifier qualifier) {

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
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("* const * const a[3][4]");
        Declarator decl;
        Int32 current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static Int32 Parse(List<Token> src, Int32 begin, out Declarator declr) {
        // try to match pointer
        List<PointerModifier> pointer_infos;
        Int32 current = _pointer.Parse(src, begin, out pointer_infos);
        if (current == -1) {
            // if fail, just create an empty list
            pointer_infos = new List<PointerModifier>();
            current = begin;
        }

        // match direct_declarator
        Declarator direct_declr;
        if ((current = _direct_declarator.Parse(src, current, out direct_declr)) != -1) {
            String name = direct_declr.declr_name;
            List<TypeModifier> modifiers = new List<TypeModifier>(direct_declr.declr_modifiers);
            modifiers.AddRange(pointer_infos);
            declr = new Declarator(name, modifiers);
            return current;
        } else {
            declr = null;
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

/// <summary>
/// pointer
///   : [ '*' [type_qualifier_list]? ]+
/// </summary>
public class _pointer : ParseRule {
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("* const * volatile const *");
        List<PointerModifier> infos;
        Int32 current = Parse(src, 0, out infos);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static Int32 Parse(List<Token> src, Int32 begin, out List<PointerModifier> infos) {
        Int32 r = Parser.ParseNonEmptyList(src, begin, out infos,                                 // [
            Parser.GetSequenceParser(
                Parser.GetOperatorParser(OperatorVal.MULT),                                       //   '*'
                Parser.GetOptionalParser(new List<TypeQualifier>(), _type_qualifier_list.Parse),  //   [type_qualifier_list]?
                (Boolean _, List<TypeQualifier> type_quals) => new PointerModifier(type_quals)
            )
        );                                                                                        // ]+

        // reverse the pointer modifiers
        if (r == -1) {
            return -1;
        } else {
            infos.Reverse();
            return r;
        }
    }
}

/// <summary>
/// parameter_type_list
///   : parameter_list [ ',' '...' ]?
/// 
/// a parameter list and an optional vararg signature
/// used in function declarations
/// </summary>
public class _parameter_type_list : ParseRule {

    /// <summary>
    /// parse optional ', ...'
    /// </summary>
    public static Int32 ParseOptionalVarArgs(List<Token> src, Int32 begin, out Boolean is_varargs) {
        if (is_varargs = (
               Parser.IsOperator(src[begin], OperatorVal.COMMA)
            && Parser.IsOperator(src[begin + 1], OperatorVal.PERIOD)
            && Parser.IsOperator(src[begin + 2], OperatorVal.PERIOD)
            && Parser.IsOperator(src[begin + 3], OperatorVal.PERIOD)
        )) {
            return begin + 4;
        } else {
            return begin;
        }
    }

    public static Int32 Parse(List<Token> src, Int32 begin, out ParameterTypeList param_type_list) {
        return Parser.ParseSequence(src, begin, out param_type_list,
            _parameter_list.Parse,
            ParseOptionalVarArgs,
            (List<ParameterDeclaration> param_list, Boolean is_varargs) => new ParameterTypeList(param_list, is_varargs)
        );
    }
}


/// <summary>
/// parameter_list
///   : parameter_declaration [ ',' parameter_declaration ]*
/// 
/// a non-empty list of parameters separated by ','
/// used in a function signature
/// </summary>
public class _parameter_list : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out List<ParameterDeclaration> param_list) {
        return Parser.ParseNonEmptyListWithSep(src, begin, out param_list, _parameter_declaration.Parse, OperatorVal.COMMA);
    }
}


/// <summary>
/// type_qualifier_list
///   : [type_qualifier]+
/// 
/// a non-empty list of type qualifiers
/// </summary>
public class _type_qualifier_list : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out List<TypeQualifier> type_qualifiers) {
        return Parser.ParseNonEmptyList(src, begin, out type_qualifiers, _type_qualifier.Parse);
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
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("(*a)[3][5 + 7][]");
        Declarator decl;
        Int32 current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }

        return true;
    }

    // '(' declarator ')'
    // 
    public static Int32 ParseDeclarator(List<Token> src, Int32 begin, out Declarator declr) {
        if (!Parser.EatOperator(src, ref begin, OperatorVal.LPAREN)) {
            declr = null;
            return -1;
        }

        if ((begin = _declarator.Parse(src, begin, out declr)) == -1) {
            declr = null;
            return -1;
        }

        if (!Parser.EatOperator(src, ref begin, OperatorVal.RPAREN)) {
            declr = null;
            return -1;
        }

        return begin;
    }

    // '[' constant_expression ']'
    // 
    public static Int32 ParseArrayModifier(List<Token> src, Int32 begin, out ArrayModifier modifier) {
        // match '['
        if (!Parser.EatOperator(src, ref begin, OperatorVal.LBRACKET)) {
            modifier = null;
            return -1;
        }

        // match constant_expression, if fail, just put null
        Expression nelems;
        Int32 saved = begin;
        if ((begin = _constant_expression.Parse(src, begin, out nelems)) == -1) {
            nelems = new EmptyExpression();
            begin = saved;
        }

        // match ']'
        if (!Parser.EatOperator(src, ref begin, OperatorVal.RBRACKET)) {
            modifier = null;
            return -1;
        }

        modifier = new ArrayModifier(nelems);
        return begin;
    }

    // '(' parameter_type_list ')'
    // 
    public static Int32 ParseFunctionModifier(List<Token> src, Int32 begin, out FunctionModifier modifier) {
        // match '('
        if (!Parser.EatOperator(src, ref begin, OperatorVal.LPAREN)) {
            modifier = null;
            return -1;
        }

        // match constant_expression, if fail, just assume no parameter
        ParameterTypeList param_type_list;
        Int32 saved = begin;
        if ((begin = _parameter_type_list.Parse(src, begin, out param_type_list)) == -1) {
            param_type_list = new ParameterTypeList(new List<ParameterDeclaration>());
            begin = saved;
        }

        // match ')'
        if (!Parser.EatOperator(src, ref begin, OperatorVal.RPAREN)) {
            modifier = null;
            return -1;
        }

        modifier = new FunctionModifier(param_type_list);
        return begin;
    }

    // array modifier or function modifier
    // 
    public static Int32 ParseSuffixModifier(List<Token> src, Int32 begin, out TypeModifier modifier) {
        ArrayModifier array_modifier;
        Int32 current = ParseArrayModifier(src, begin, out array_modifier);
        if (current != -1) {
            modifier = array_modifier;
            return current;
        }

        FunctionModifier function_info;
        if ((current = ParseFunctionModifier(src, begin, out function_info)) != -1) {
            modifier = function_info;
            return current;
        }

        modifier = null;
        return -1;
    }
    
    // Parse direct declarator
    // 
    public static Int32 Parse(List<Token> src, Int32 begin, out Declarator declr) {
        String name;
        List<TypeModifier> modifiers = new List<TypeModifier>();

        // 1. match: id | '(' declarator ')'
        // 1.1. try: '(' declarator ')'
        Int32 current;
        if ((current = ParseDeclarator(src, begin, out declr)) != -1) {
            name = declr.declr_name;
            modifiers = new List<TypeModifier>(declr.declr_modifiers);
        } else {
            // if fail, 1.2. try id
            name = Parser.GetIdentifierValue(src[begin]);
            if (name == null) {
                declr = null;
                return -1;
            }
            current = begin + 1;
        }

        List<TypeModifier> more_modifiers;
        current = Parser.ParseList(src, current, out more_modifiers, ParseSuffixModifier);
        modifiers.AddRange(more_modifiers);

        declr = new Declarator(name, modifiers);
        return current;
    }

}


// enum_specifier : enum <identifier>? { enumerator_list }
//                | enum identifier
public class _enum_specifier : ParseRule {

    // this parses { enumerator_list }
    private static Int32 ParseEnumList(List<Token> src, Int32 begin, out List<Enumerator> enum_list) {
        enum_list = null;
        if (!Parser.IsOperator(src[begin], OperatorVal.LCURL)) {
            return -1;
        }
        Int32 current = begin + 1;
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

    public static Int32 Parse(List<Token> src, Int32 begin, out EnumSpecifier enum_spec) {
        

        if (src[begin].type != TokenType.KEYWORD) {
            enum_spec = null;
            return -1;
        }
        if (((TokenKeyword)src[begin]).val != KeywordVal.ENUM) {
            enum_spec = null;
            return -1;
        }

        Int32 current = begin + 1;
        List<Enumerator> enum_list;
        String name;
        if ((name = Parser.GetIdentifierValue(src[current])) != null) {
            current++;

            Int32 saved = current;
            if ((current = ParseEnumList(src, current, out enum_list)) == -1) {
                enum_spec = new EnumSpecifier(name, new List<Enumerator>());
                return saved;
            } else {
                enum_spec = new EnumSpecifier(name, enum_list);
                return current;
            }

        } else {
            current = ParseEnumList(src, current, out enum_list);
            if (current == -1) {
                enum_spec = null;
                return -1;
            }
            enum_spec = new EnumSpecifier("", enum_list);
            return current;

        }
    }
}


// enumerator_list : enumerator
//                 | enumerator_list, enumerator
// [ note: my solution ]
// enumerator_list : enumerator < , enumerator >*
public class _enumerator_list : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out List<Enumerator> enum_list) {
        return Parser.ParseNonEmptyListWithSep(src, begin, out enum_list, _enumerator.Parse, OperatorVal.COMMA);
    }
}


// enumerator : enumeration_constant
//            | enumeration_constant = constant_expression
// [ note: my solution ]
// enumerator : enumeration_constant < = constant_expression >?
public class _enumerator : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out Enumerator enumerator) {
        String name;
        Int32 current = _enumeration_constant.Parse(src, begin, out name);
        if (current == -1) {
            enumerator = null;
            return -1;
        }

        Expression init;
        if (Parser.EatOperator(src, ref current, OperatorVal.ASSIGN)) {
            if ((current = _constant_expression.Parse(src, current, out init)) == -1) {
                enumerator = null;
                return -1;
            }
        } else {
            init = null;
        }

        enumerator = new Enumerator(name, init);
        return current;
    }
}


/// <summary>
/// enumeration_constant
///   : identifier
/// </summary>
public class _enumeration_constant : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out String name) {
        return Parser.ParseIdentifier(src, begin, out name);
    }
}


// struct_or_union_specifier : struct_or_union <identifier>? { struct_declaration_list }
//                           | struct_or_union identifier
// [ note: need some treatment ]
public class _struct_or_union_specifier : ParseRule {
    public static Int32 ParseDeclarationList(List<Token> src, Int32 begin, out List<StructDeclaration> decl_list) {
        decl_list = null;

        if (!Parser.IsLCURL(src[begin])) {
            return -1;
        }
        Int32 current = begin + 1;
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

    public static Int32 Parse(List<Token> src, Int32 begin, out StructOrUnionSpecifier spec) {
        spec = null;

        StructOrUnion struct_or_union;
        List<StructDeclaration> decl_list;

        Int32 current = _struct_or_union.Parse(src, begin, out struct_or_union);
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
            Int32 saved = current;

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
                spec = new UnionSpecifier("", decl_list);
            } else {
                spec = new StructSpecifier("", decl_list);
            }

            return current;

        }
    }
}

// struct_or_union : struct | union
public class _struct_or_union : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out StructOrUnion struct_or_union) {
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
    public static Int32 Parse(List<Token> src, Int32 begin, out List<StructDeclaration> decl_list) {
        return Parser.ParseNonEmptyList(src, begin, out decl_list, _struct_declaration.Parse);
    }
}


/// <summary>
/// struct_declaration
///   : specifier_qualifier_list struct_declarator_list ';'
/// 
/// <remarks>
/// Note that a struct declaration does not need a storage class specifier.
/// </remarks>
/// </summary>
public class _struct_declaration : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out StructDeclaration decln) {
        return Parser.ParseSequence(src, begin, out decln,
            _specifier_qualifier_list.Parse,
            _struct_declarator_list.Parse,
            Parser.GetOperatorParser(OperatorVal.SEMICOLON),
            (DeclarationSpecifiers specs, List<Declarator> declrs, Boolean _) => new StructDeclaration(specs, declrs)
        );
    }
}


/// <summary>
/// specifier_qualifier_list
///   : [ type_specifier | type_qualifier ]+
/// </summary>
public class _specifier_qualifier_list : ParseRule {
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("int long const");
        DeclarationSpecifiers specs;
        Int32 current = Parse(src, 0, out specs);
        if (current == -1) {
            return false;
        }

        return true;
    }
    
    public static Int32 Parse(List<Token> src, Int32 begin, out DeclarationSpecifiers decl_specs) {
        List<TypeSpecifier> type_specifiers = new List<TypeSpecifier>();
        List<TypeQualifier> type_qualifiers = new List<TypeQualifier>();

        while (true) {
            Int32 saved = begin;

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


/// <summary>
/// struct_declarator_list
///   : struct_declarator [ ',' struct_declarator ]*
/// </summary>
public class _struct_declarator_list : ParseRule {
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("*a, *b[3]");
        List<Declarator> decl_list;
        Int32 current = Parse(src, 0, out decl_list);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static Int32 Parse(List<Token> src, Int32 begin, out List<Declarator> declrs) {
        return Parser.ParseNonEmptyListWithSep(src, begin, out declrs, _struct_declarator.Parse, OperatorVal.COMMA);
    }
}


/// <summary>
/// struct_declarator
///   : declarator
///   | type_specifier [declarator]? : constant_expression
/// 
/// <remarks>
/// Note that the second one represents a 'bit-field', which I'm not going to support.
/// </remarks>
/// </summary>
public class _struct_declarator : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out Declarator declr) {
        return _declarator.Parse(src, begin, out declr);
    }
}


/// <summary>
/// parameter_declaration
///   : declaration_specifiers [ declarator | abstract_declarator ]?
/// </summary>
public class _parameter_declaration : ParseRule {
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("int *a[]");
        ParameterDeclaration decl;
        Int32 current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }
        return true;
    }

    public static Int32 Parse(List<Token> src, Int32 begin, out ParameterDeclaration decln) {
        return Parser.ParseSequence(src, begin, out decln,

            // declaration_specifiers
            _declaration_specifiers.Parse,

            // [ 
            Parser.GetOptionalParser(
                new Declarator("", new List<TypeModifier>()),

                // declarator | abstract_declarator
                Parser.GetChoicesParser(new List<Parser.FParse<Declarator>> { _declarator.Parse, _abstract_declarator.Parse })
            ),
            // ]?

            (DeclarationSpecifiers specs, Declarator declr) => new ParameterDeclaration(specs, declr)
        );
    }
}

// identifier_list : /* old style, i'm deleting this */


/// <summary>
/// abstract_declarator
///   : pointer
///   | [pointer]? direct_abstract_declarator
/// 
/// an abstract declarator is a non-empty list of (pointer, function, or array) type modifiers
/// </summary>
public class _abstract_declarator : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out Declarator declr) {
        List<TypeModifier> modifiers;
        begin = Parser.ParseSequence(src, begin, out modifiers,
            Parser.GetOptionalParser(new List<PointerModifier>(), _pointer.Parse),                                                      // [pointer]?
            Parser.GetOptionalParser(new Declarator("", new List<TypeModifier>()), _direct_abstract_declarator.Parse),                  // [direct_abstract_declarator]?
            (List<PointerModifier> ptr_modifiers, Declarator abst_declr) => abst_declr.declr_modifiers.Concat(ptr_modifiers).ToList()
        );

        // make sure the list is non-empty
        if (begin != -1 && modifiers.Any()) {
            declr = new Declarator("", modifiers);
            return begin;
        } else {
            declr = null;
            return -1;
        }
    }
}


/// <summary>
/// direct_abstract_declarator
///   : [
///         '(' abstract_declarator ')'
///       | '[' [constant_expression]? ']'  // array modifier
///       | '(' [parameter_type_list]? ')'  // function modifier
///     ] [
///         '[' [constant_expression]? ']'  // array modifier
///       | '(' [parameter_type_list]? ')'  // function modifier
///     ]*
/// </summary>
public class _direct_abstract_declarator : ParseRule {
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("(*)[3][5 + 7][]");
        Declarator decl;
        Int32 current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }

        return true;
    }

    // '(' abstract_declarator ')'
    // 
    public static Int32 ParseAbstractDeclarator(List<Token> src, Int32 begin, out Declarator decl) {
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

    public static Int32 Parse(List<Token> src, Int32 begin, out Declarator declr) {
        List<TypeModifier> modifiers;

        // 1. match modifier | '(' abstract_declarator ')'
        // 1.1 try '(' abstract_declarator ')'
        Int32 current = ParseAbstractDeclarator(src, begin, out declr);
        if (current != -1) {
            modifiers = new List<TypeModifier>(declr.declr_modifiers);
        } else {
            // if fail, 1.2. try modifier
            TypeModifier modifier;
            if ((current = _direct_declarator.ParseSuffixModifier(src, begin, out modifier)) == -1) {
                declr = null;
                return -1;
            }
            modifiers = new List<TypeModifier> { modifier };
        }

        // now match modifiers
        List<TypeModifier> more_modifiers;
        current = Parser.ParseList(src, current, out more_modifiers, _direct_declarator.ParseSuffixModifier);
        modifiers.AddRange(more_modifiers);

        declr = new Declarator("", modifiers);
        return current;
    }

}


// initializer : assignment_expression
//             | '{' initializer_list '}'
//             | '{' initializer_list ',' '}'
public class _initializer : ParseRule {
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("a = 3");
        Expression expr;
        Int32 current = Parse(src, 0, out expr);
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

    public static Int32 Parse(List<Token> src, Int32 begin, out Expression expr) {
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

/// <summary>
/// initializer_list
///   : initializer [ ',' initializer ]*
/// 
/// A non-empty list of initializers.
/// </summary>
public class _initializer_list : ParseRule {
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("{1, 2}, {2, 3}");
        Expression init;
        Int32 current = Parse(src, 0, out init);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static Int32 Parse(List<Token> src, Int32 begin, out Expression expr) {
        List<Expression> exprs;
        if ((begin = Parser.ParseNonEmptyListWithSep(src, begin, out exprs, _initializer.Parse, OperatorVal.COMMA)) == -1) {
            expr = null;
            return -1;
        }

        expr = new InitializerList(exprs);
        return begin;

    }
}


/// <summary>
/// type_name
///   : specifier_qualifier_list [abstract_declarator]?
/// 
/// It's just a declaration with the name optional.
/// </summary>
public class _type_name : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out TypeName type_name) {
        return Parser.ParseSequence(src, begin, out type_name,

            // specifier_qualifier_list
            _specifier_qualifier_list.Parse,

            // [abstract_declarator]?
            Parser.GetOptionalParser(new Declarator("", new List<TypeModifier>()), _abstract_declarator.Parse),


            (DeclarationSpecifiers specs, Declarator declr) => new TypeName(specs, declr)
        );
    }
}

/// <summary>
/// typedef_name
///   : identifier
/// 
/// It must be something already defined.
/// We need to look it up in the parser environment.
/// </summary>
public class _typedef_name : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 pos, out String name) {
        if ((pos = (Parser.ParseIdentifier(src, pos, out name))) == -1) {
            return -1;
        }
        if (!ParserEnvironment.HasTypedefName(name)) {
            return -1;
        }

        return pos;
    }
}

