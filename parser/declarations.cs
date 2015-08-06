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
        Decln decl;
        Int32 current = Parse(src, 0, out decl);
        return current != -1;
    }
    
    public static Int32 Parse(List<Token> src, Int32 begin, out Decln declaration) {
        return Parser.ParseSequence(src, begin, out declaration,

            // declaration_specifiers
            _declaration_specifiers.Parse,

            // [init_declarator_list]?
            Parser.GetOptionalParser(new List<InitDeclr>(), _init_declarator_list.Parse),

            // ';'
            Parser.GetOperatorParser(OperatorVal.SEMICOLON),

			// # if this declaration is a typeof, then add the name into the environment
            (DeclnSpecs decln_specs, List<InitDeclr> init_declrs, Boolean _) => {
                if (decln_specs.IsTypedef()) {
                    foreach (InitDeclr init_declr in init_declrs) {
                        ParserEnvironment.AddTypedefName(init_declr.declr.name);
                    }
                }
                return new Decln(decln_specs, init_declrs);
            }
        );
    }
}


/// <summary>
/// declaration_specifiers
///   : [ storage_class_specifier | type_specifier | type_qualifier ]+
/// </summary>
/// <remarks>
/// 1. You can only have **one** storage class specifier.
/// 2. You can have duplicate type qualifiers, since it doesn't cause ambiguity.
/// </remarks>
public class _declaration_specifiers : ParseRule {
    public static Boolean Test() {
        DeclnSpecs decl_specs;

        var src = Parser.GetTokensFromString("typedef Int32 long double const");
        Int32 current = Parse(src, 0, out decl_specs);
        if (current == -1) {
            return false;
        }
        src = Parser.GetTokensFromString("typedef typedef typedef const const");
        current = Parse(src, 0, out decl_specs);
        return current != -1;
    }


    public static Int32 Parse(List<Token> src, Int32 begin, out DeclnSpecs decl_specs) {
        List<StorageClassSpec> storage_class_specifiers = new List<StorageClassSpec>();
        List<TypeSpec> type_specifiers = new List<TypeSpec>();
        List<TypeQual> type_qualifiers = new List<TypeQual>();
        
        Int32 current = begin;
        while (true) {
            Int32 saved = current;

            // 1. match storage_class_specifier
            StorageClassSpec storage_class_specifier;
			if ((current = _storage_class_specifier.Parse(src, saved, out storage_class_specifier)) != -1) {
                storage_class_specifiers.Add(storage_class_specifier);
                continue;
            }

            // 2. if failed, match type_specifier
            TypeSpec type_specifier;
			if ((current = _type_specifier.Parse(src, saved, out type_specifier)) != -1) {
                type_specifiers.Add(type_specifier);
                continue;
            }

            // 3. if failed, match type_qualifier
            TypeQual type_qualifier;
			if ((current = _type_qualifier.Parse(src, saved, out type_qualifier)) != -1) {
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

        decl_specs = new DeclnSpecs(storage_class_specifiers, type_specifiers, type_qualifiers);
        return current;

    }
    
}


/// <summary>
/// init_declarator_list
///   : init_declarator [ ',' init_declarator ]*
/// 
/// <remarks>
/// a non-empty list of init_declarators separated by ','
/// </remarks>
/// </summary>
public class _init_declarator_list : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out List<InitDeclr> init_declarators) {
        return Parser.ParseNonEmptyListWithSep(src, begin, out init_declarators, _init_declarator.Parse, OperatorVal.COMMA);
    }
}


/// <summary>
/// init_declarator
///   : declarator [ '=' initializer ]?
/// </summary>
public class _init_declarator : ParseRule {
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("a = 3 + 4");
        InitDeclr decl;
        Int32 current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static Int32 Parse(List<Token> src, Int32 begin, out InitDeclr init_declarator) {

		var InitializerParser = Parser.GetSequenceParser(

			// '='
			Parser.GetOperatorParser(OperatorVal.ASSIGN),

			// initializer
			_initializer.Parse,

			(Boolean _, Expr _init) => _init

		);

		return Parser.ParseSequence(src, begin, out init_declarator,

			// declarator
			_declarator.Parse,

			// [ '=' initializer ]?
			Parser.GetOptionalParser<Expr>(null, InitializerParser),

			(Declr declr, Expr init) => new InitDeclr(declr, init)
		);
    }
}


/// <summary>
/// storage_class_specifier
///   : auto | register | static | extern | typedef
/// 
/// <remarks>
/// There can only be *one* storage class specifier in one declaration.
/// </remarks>
/// </summary>
public class _storage_class_specifier : ParseRule {
    public static Boolean Test() {
        StorageClassSpec decl_specs;

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

    public static Int32 Parse(List<Token> src, Int32 begin, out StorageClassSpec spec) {

        // make sure the token is a keyword
        if (src[begin].type != TokenType.KEYWORD) {
            spec = StorageClassSpec.NULL;
            return -1;
        }

        // check the value
        KeywordVal val = ((TokenKeyword)src[begin]).val;
        switch (val) {
        case KeywordVal.AUTO:
            spec = StorageClassSpec.AUTO;
            return begin + 1;

        case KeywordVal.REGISTER:
            spec = StorageClassSpec.REGISTER;
            return begin + 1;

        case KeywordVal.STATIC:
            spec = StorageClassSpec.STATIC;
            return begin + 1;

        case KeywordVal.EXTERN:
            spec = StorageClassSpec.EXTERN;
            return begin + 1;

        case KeywordVal.TYPEDEF:
            spec = StorageClassSpec.TYPEDEF;
            return begin + 1;

        default:
            spec = StorageClassSpec.NULL;
            return -1;
        }
    }
}


/// <summary>
/// type_specifier
///   : void
///   | char
///   | short
///   | int
///   | long
///   | float
///   | double
///   | signed
///   | unsigned
///   | struct_or_union_specifier
///   | enum_specifier
///   | typedef_name
/// 
/// <remarks>
/// 1. void, char, short, int, long, float, double, signed, unsigned are called "basic type specifiers".
/// 2. struct_or_union_specifier and enum_specifier need more complicated parsing.
/// 3. Parsing typedef_name actually requires the environment to participate. For example, consider this statement:
///      T *v;
///    Is T a type or an object? If T is a type, then this statement is a declaration: v is a pointer; if T is a object, then this statement is an expression.
///    So, we need to keep track of the typedefs in the environment even in the parsing stage!
/// </remarks>
/// </summary>
public class _type_specifier : ParseRule {
    public static Boolean Test() {
        TypeSpec spec;

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

    public static Int32 Parse(List<Token> src, Int32 begin, out TypeSpec spec) {
		Int32 current;

        // 1. match struct or union
        StructOrUnionSpec struct_or_union_specifier;
		if ((current = _struct_or_union_specifier.Parse(src, begin, out struct_or_union_specifier)) != -1) {
            spec = struct_or_union_specifier;
            return current;
        }

        // 2. match enum
        EnumSpecifier enum_specifier;
		if ((current = _enum_specifier.Parse(src, begin, out enum_specifier)) != -1) {
            spec = enum_specifier;
            return current;
        }

        // 3. match typedef name
        String typedef_name;
		if ((current = _typedef_name.Parse(src, begin, out typedef_name)) != -1) {
            spec = new TypedefName(typedef_name);
            return current;
        }

        // now we only need to take care of the basic type specifiers
		// first make sure the next token is a keyword
        if (src[begin].type != TokenType.KEYWORD) {
            spec = null;
            return -1;
        }

        // then check the value
        KeywordVal val = ((TokenKeyword)src[begin]).val;
        switch (val) {
        case KeywordVal.VOID:
            spec = new TypeSpec(TypeSpec.Kind.VOID);
            return begin + 1;

        case KeywordVal.CHAR:
            spec = new TypeSpec(TypeSpec.Kind.CHAR);
            return begin + 1;

        case KeywordVal.SHORT:
            spec = new TypeSpec(TypeSpec.Kind.SHORT);
            return begin + 1;

        case KeywordVal.INT:
            spec = new TypeSpec(TypeSpec.Kind.INT);
            return begin + 1;

        case KeywordVal.LONG:
            spec = new TypeSpec(TypeSpec.Kind.LONG);
            return begin + 1;

        case KeywordVal.FLOAT:
            spec = new TypeSpec(TypeSpec.Kind.FLOAT);
            return begin + 1;

        case KeywordVal.DOUBLE:
            spec = new TypeSpec(TypeSpec.Kind.DOUBLE);
            return begin + 1;

        case KeywordVal.SIGNED:
            spec = new TypeSpec(TypeSpec.Kind.SIGNED);
            return begin + 1;

        case KeywordVal.UNSIGNED:
            spec = new TypeSpec(TypeSpec.Kind.UNSIGNED);
            return begin + 1;

        default:
            spec = null;
            return -1;
        }

    }
}


/// <summary>
/// type_qualifier
///   : const
///   | volatile
/// 
/// <remarks>
/// Note that there can be multiple type qualifiers in one declarations.
/// </remarks>
/// </summary>
public class _type_qualifier : ParseRule {
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("const volatile");
        TypeQual qualifier;
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
    
    public static Int32 Parse(List<Token> src, Int32 begin, out TypeQual qualifier) {

        // make sure the token is a keyword
        if (src[begin].type != TokenType.KEYWORD) {
            qualifier = TypeQual.NULL;
            return -1;
        }

        // check the value
        KeywordVal val = ((TokenKeyword)src[begin]).val;
        switch (val) {
        case KeywordVal.CONST:
            qualifier = TypeQual.CONST;
            return begin + 1;

        case KeywordVal.VOLATILE:
            qualifier = TypeQual.VOLATILE;
            return begin + 1;

        default:
            qualifier = TypeQual.NULL;
            return -1;
        }

    }
}


/// <summary>
/// declarator
///   : [pointer]? direct_declarator
/// 
/// <remarks>
/// A declarator gives a name to the object and also modifies the type.
/// </remarks>
/// </summary>
public class _declarator : ParseRule {
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("* const * const a[3][4]");
        Declr decl;
        Int32 current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static Int32 Parse(List<Token> src, Int32 begin, out Declr declr) {
		return Parser.ParseSequence(
			src, begin, out declr,

			// [pointer]?
			Parser.GetOptionalParser(new List<PointerModifier>(), _pointer.Parse),

			// direct_declarator
			_direct_declarator.Parse,

			(List<PointerModifier> pointer_modifiers, Declr direct_declr) => {
				String name = direct_declr.name;
				List<TypeModifier> modifiers = new List<TypeModifier>(direct_declr.declr_modifiers);
				modifiers.AddRange(pointer_modifiers);
				return new Declr(name, modifiers);
			}
		);
    }
}


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

		// [
        Int32 r = Parser.ParseNonEmptyList(src, begin, out infos,
            Parser.GetSequenceParser(

				// '*'
                Parser.GetOperatorParser(OperatorVal.MULT),

				// [type_qualifier_list]?
                Parser.GetOptionalParser(new List<TypeQual>(), _type_qualifier_list.Parse),

                (Boolean _, List<TypeQual> type_quals) => new PointerModifier(type_quals)
            )
        );
		// ]+

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
        return Parser.ParseSequence(
			src, begin, out param_type_list,
            
			// parameter_list
			_parameter_list.Parse,

			// [ ',' '...' ]?
            ParseOptionalVarArgs,

            (List<ParamDecln> param_list, Boolean is_varargs) => new ParameterTypeList(param_list, is_varargs)
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
    public static Int32 Parse(List<Token> src, Int32 begin, out List<ParamDecln> param_list) {
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
    public static Int32 Parse(List<Token> src, Int32 begin, out List<TypeQual> type_qualifiers) {
        return Parser.ParseNonEmptyList(src, begin, out type_qualifiers, _type_qualifier.Parse);
    }
}


/// <summary>
/// direct_declarator
///   : [ identifier | '(' declarator ')' ] [ '[' [constant_expression]? ']' | '(' [parameter_type_list]? ')' ]*
/// </summary>
/// <remarks>
/// There is an old style of function definition:
/// +-------------------------------+
/// |    int foo(param1, param2)    |
/// |    int  param1;               |
/// |    char param2;               |
/// |    {                          |
/// |        ....                   |
/// |    }                          |
/// +-------------------------------+
/// 
/// I'm not gonna support this style, and function definitions should always be like this:
/// +------------------------------------------+
/// |    int foo(int param1, char param2) {    |
/// |        ....                              |
/// |    }                                     |
/// +------------------------------------------+
/// </remarks>
public class _direct_declarator : ParseRule {
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("(*a)[3][5 + 7][]");
        Declr decl;
        Int32 current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }

        return true;
    }

    // '(' declarator ')'
    // 
    public static Int32 ParseDeclarator(List<Token> src, Int32 begin, out Declr declr) {
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

	/// <summary>
	/// array_modifier
	///   : '[' [constant_expression]? ']'
	/// </summary>
    public static Int32 ParseArrayModifier(List<Token> src, Int32 begin, out ArrayModifier modifier) {
        // match '['
        if (!Parser.EatOperator(src, ref begin, OperatorVal.LBRACKET)) {
            modifier = null;
            return -1;
        }

        // match constant_expression, if fail, just put null
        Expr nelems;
        Int32 saved = begin;
        if ((begin = _constant_expression.Parse(src, begin, out nelems)) == -1) {
            nelems = new EmptyExpr();
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

	/// <summary>
    /// function_modifier
	///   : '(' [parameter_type_list] ')'
	/// </summary>
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
            param_type_list = new ParameterTypeList(new List<ParamDecln>());
            begin = saved;
        }

        // match ')'
        if (!Parser.EatOperator(src, ref begin, OperatorVal.RPAREN)) {
            modifier = null;
            return -1;
        }

        modifier = new FunctionModifier(param_type_list.params_inner_declns, param_type_list.params_varargs);
        return begin;
    }

	// suffix_modifier
	//   : '[' [constant_expression]? ']'
	//   | '(' [parameter_type_list]? ')'
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
    public static Int32 Parse(List<Token> src, Int32 begin, out Declr declr) {
        String name;
        List<TypeModifier> modifiers = new List<TypeModifier>();

        // 1. match: id | '(' declarator ')'
        // 1.1. try: '(' declarator ')'
        Int32 current;
        if ((current = ParseDeclarator(src, begin, out declr)) != -1) {
            name = declr.name;
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

        declr = new Declr(name, modifiers);
        return current;
    }

}


/// <summary>
/// enum_specifier
///   : enum [identifier]? '{' enumerator_list '}'
///   | enum identifier
/// </summary>
public class _enum_specifier : ParseRule {

    /// <summary>
	/// '{' enumerator_list '}'
    /// </summary>
    private static Int32 ParseEnumList(List<Token> src, Int32 begin, out List<Enumerator> enum_list) {
        if (!Parser.IsOperator(src[begin], OperatorVal.LCURL)) {
			enum_list = null;
            return -1;
        }
		++begin;
        
		if ((begin = _enumerator_list.Parse(src, begin, out enum_list)) == -1) {
			enum_list = null;
			return -1;
		}
        
        if (!Parser.IsOperator(src[begin], OperatorVal.RCURL)) {
			enum_list = null;
            return -1;
        }
		return begin + 1;
    }

    public static Int32 Parse(List<Token> src, Int32 begin, out EnumSpecifier enum_spec) {
        
		// enum
		if (!Parser.IsKeyword(src[begin], KeywordVal.ENUM)) {
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
			if ((current = ParseEnumList(src, current, out enum_list)) == -1) {
                enum_spec = null;
                return -1;
            }
            enum_spec = new EnumSpecifier("", enum_list);
            return current;
        }
    }
}


/// <summary>
/// enumerator_list
///   : enumerator [ ',' enumerator ]*
/// </summary>
public class _enumerator_list : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out List<Enumerator> enum_list) {
        return Parser.ParseNonEmptyListWithSep(src, begin, out enum_list, _enumerator.Parse, OperatorVal.COMMA);
    }
}


/// <summary>
/// enumerator
///   : enumeration [ '=' constant_expression ]?
/// </summary>
public class _enumerator : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out Enumerator enumerator) {
		return Parser.ParseSequence(src, begin, out enumerator,

			// enumeration
			_enumeration_constant.Parse,

			// [
			Parser.GetOptionalParser(
				null,
				Parser.GetSequenceParser(
					// '='
					Parser.GetOperatorParser(OperatorVal.EQ),

					// constant_expression
					_constant_expression.Parse,
					(Boolean _, Expr expr) => expr
				)
			),
			// ]?

			(String name, Expr expr) => new Enumerator(name, expr)

		);
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


/// <summary>
/// struct_or_union_specifier
///   : struct_or_union [identifier]? { struct_declaration_list }
///   | struct_or_union identifier
/// 
/// <remarks>
/// Note: if no struct_declaration_list given, the type is considered incomplete.
/// </remarks>
/// </summary>
public class _struct_or_union_specifier : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out StructOrUnionSpec spec) {
		return Parser.ParseSequence(src, begin, out spec,

			// struct_or_union
			_struct_or_union.Parse,

			// [
			Parser.GetChoicesParser (new List<Parser.FParse<Tuple<String, List<StructDeclaration>>>> {

				// [
				Parser.GetSequenceParser(

					// [identifier]?
					Parser.GetOptionalParser("", Parser.ParseIdentifier),

					// { struct_declaration_list }
					Parser.GetBraceSurroundedParser<List<StructDeclaration>>(_struct_declaration_list.Parse),

					(String id, List<StructDeclaration> declns) => Tuple.Create(id, declns)
				),
				// ]

				// |

				// identifier
				Parser.GetModifiedParser(
					Parser.ParseIdentifier,
					(String id) => new Tuple<String, List<StructDeclaration>>(id, null)
				)
			}),
			// ]

			(Boolean is_union, Tuple<String, List<StructDeclaration>> declns) => {
				if (is_union) {
					return new UnionSpec(declns.Item1, declns.Item2);
				} else {
					return new StructSpecifier(declns.Item1, declns.Item2);
				}
			}

		);

    }
}


/// <summary>
/// struct_or_union
///   : struct | union
/// </summary>
public class _struct_or_union : ParseRule {
    public static Int32 Parse(List<Token> src, Int32 begin, out Boolean is_union) {
		if (Parser.IsKeyword(src[begin], KeywordVal.STRUCT)) {
			is_union = false;
			return begin + 1;
		} else if (Parser.IsKeyword(src[begin], KeywordVal.UNION)) {
			is_union = true;
			return begin + 1;
		} else {
			is_union = false;
			return -1;
		}
    }
}


/// <summary>
/// struct_declaration_list
///   : [struct_declaration]+
/// </summary>
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
        return Parser.ParseSequence(
			src, begin, out decln,
            
			// specifier_qualifier_list
			_specifier_qualifier_list.Parse,
            
			// struct_declarator_list
			_struct_declarator_list.Parse,

			// ';'
            Parser.GetOperatorParser(OperatorVal.SEMICOLON),

            (DeclnSpecs specs, List<Declr> declrs, Boolean _) => new StructDeclaration(specs, declrs)
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
        DeclnSpecs specs;
        Int32 current = Parse(src, 0, out specs);
        if (current == -1) {
            return false;
        }

        return true;
    }
    
    public static Int32 Parse(List<Token> src, Int32 begin, out DeclnSpecs decl_specs) {
        List<TypeSpec> type_specifiers = new List<TypeSpec>();
        List<TypeQual> type_qualifiers = new List<TypeQual>();

        while (true) {
            Int32 saved = begin;

            // 1. match type_specifier
            begin = saved;
            TypeSpec type_specifier;
            if ((begin = _type_specifier.Parse(src, begin, out type_specifier)) != -1) {
                type_specifiers.Add(type_specifier);
                continue;
            }

            // 2. match type_qualifier
            begin = saved;
            TypeQual type_qualifier;
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

        decl_specs = new DeclnSpecs(null, type_specifiers, type_qualifiers);
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
        List<Declr> decl_list;
        Int32 current = Parse(src, 0, out decl_list);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static Int32 Parse(List<Token> src, Int32 begin, out List<Declr> declrs) {
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
    public static Int32 Parse(List<Token> src, Int32 begin, out Declr declr) {
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
        ParamDecln decl;
        Int32 current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }
        return true;
    }

    public static Int32 Parse(List<Token> src, Int32 begin, out ParamDecln decln) {
        return Parser.ParseSequence(src, begin, out decln,

            // declaration_specifiers
            _declaration_specifiers.Parse,

            // [ 
            Parser.GetOptionalParser(
                new Declr("", new List<TypeModifier>()),

                // declarator | abstract_declarator
                Parser.GetChoicesParser(new List<Parser.FParse<Declr>> { _declarator.Parse, _abstract_declarator.Parse })
            ),
            // ]?

            (DeclnSpecs specs, Declr declr) => new ParamDecln(specs, declr)
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
    public static Int32 Parse(List<Token> src, Int32 begin, out Declr declr) {
        List<TypeModifier> modifiers;
        begin = Parser.ParseSequence(src, begin, out modifiers,

			// [pointer]?
            Parser.GetOptionalParser(new List<PointerModifier>(), _pointer.Parse),

			// [direct_abstract_declarator]?
            Parser.GetOptionalParser(new Declr("", new List<TypeModifier>()), _direct_abstract_declarator.Parse),

            (List<PointerModifier> ptr_modifiers, Declr abst_declr) => abst_declr.declr_modifiers.Concat(ptr_modifiers).ToList()
        );

        // make sure the list is non-empty
        if (begin != -1 && modifiers.Any()) {
            declr = new Declr("", modifiers);
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
        Declr decl;
        Int32 current = Parse(src, 0, out decl);
        if (current == -1) {
            return false;
        }

        return true;
    }

    // '(' abstract_declarator ')'
    // 
    public static Int32 ParseAbstractDeclarator(List<Token> src, Int32 begin, out Declr decl) {
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

    public static Int32 Parse(List<Token> src, Int32 begin, out Declr declr) {
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

        declr = new Declr("", modifiers);
        return current;
    }

}


// initializer : assignment_expression
//             | '{' initializer_list '}'
//             | '{' initializer_list ',' '}'
public class _initializer : ParseRule {
    public static Boolean Test() {
        var src = Parser.GetTokensFromString("a = 3");
        Expr expr;
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

    public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
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
        Expr init;
        Int32 current = Parse(src, 0, out init);
        if (current == -1) {
            return false;
        }
        return true;
    }
    
    public static Int32 Parse(List<Token> src, Int32 begin, out Expr expr) {
        List<Expr> exprs;
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
            Parser.GetOptionalParser(new Declr("", new List<TypeModifier>()), _abstract_declarator.Parse),

            (DeclnSpecs specs, Declr declr) => new TypeName(specs, declr)
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
    public static Int32 Parse(List<Token> src, Int32 begin, out String name) {
        if ((begin = (Parser.ParseIdentifier(src, begin, out name))) == -1) {
            return -1;
        }
        if (!ParserEnvironment.HasTypedefName(name)) {
            return -1;
        }

        return begin;
    }
}
