using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parsing.ParserCombinator;
using SyntaxTree;

namespace Parsing {
    public partial class CParser {
        public static NamedParser<Decln>
            Declaration { get; } = new NamedParser<Decln>("declaration");

        public static NamedParser<DeclnSpecs>
            DeclarationSpecifiers { get; } = new NamedParser<DeclnSpecs>("declaration-specifiers");

        public static NamedParser<ImmutableList<InitDeclr>>
            InitDeclaratorList { get; } = new NamedParser<ImmutableList<InitDeclr>>("init-declarator-list");

        public static NamedParser<InitDeclr>
            InitDeclarator { get; } = new NamedParser<InitDeclr>("init-declarator");

        public static NamedParser<StorageClsSpec>
            StorageClassSpecifier { get; } = new NamedParser<StorageClsSpec>("storage-class-specifier");

        public static NamedParser<TypeSpec>
            TypeSpecifier { get; } = new NamedParser<TypeSpec>("type-specifier");

        public static NamedParser<TypeQual>
            TypeQualifier { get; } = new NamedParser<TypeQual>("type-qualifier");

        public static NamedParser<Declr>
            Declarator { get; } = new NamedParser<Declr>("declarator");

        public static NamedParser<ImmutableList<PointerModifier>>
            Pointer { get; } = new NamedParser<ImmutableList<PointerModifier>>("pointer");

        public static NamedParser<ParameterTypeList>
            ParameterTypeList { get; } = new NamedParser<ParameterTypeList>("parameter-type-list");

        public static NamedParser<ImmutableList<ParamDecln>>
            ParameterList { get; } = new NamedParser<ImmutableList<ParamDecln>>("parameter-list");

        public static NamedParser<ImmutableList<TypeQual>>
            TypeQualifierList { get; } = new NamedParser<ImmutableList<TypeQual>>("type-qualifier-list");

        public static NamedParser<Declr>
            DirectDeclarator { get; } = new NamedParser<Declr>("direct-declarator");

        public static NamedParser<EnumSpec>
            EnumSpecifier { get; } = new NamedParser<EnumSpec>("enum-specifier");

        public static NamedParser<ImmutableList<Enumr>>
            EnumeratorList { get; } = new NamedParser<ImmutableList<Enumr>>("enumerator-list");

        public static NamedParser<Enumr>
            Enumerator { get; } = new NamedParser<Enumr>("enumerator");

        public static NamedParser<String>
            EnumerationConstant { get; } = new NamedParser<string>("enumeration-constant");

        public static NamedParser<StructOrUnionSpec>
            StructOrUnionSpecifier { get; } = new NamedParser<StructOrUnionSpec>("struct-or-union-specifier");

        public static NamedParser<StructOrUnion>
            StructOrUnion { get; } = new NamedParser<StructOrUnion>("struct-or-union");

        public static NamedParser<ImmutableList<StructDecln>>
            StructDeclarationList { get; } = new NamedParser<ImmutableList<StructDecln>>("struct-declaration-list");

        public static NamedParser<StructDecln>
            StructDeclaration { get; } = new NamedParser<StructDecln>("struct-declaration");

        public static NamedParser<DeclnSpecs>
            SpecifierQualifierList { get; } = new NamedParser<DeclnSpecs>("specifier-qualifier-list");

        public static NamedParser<ImmutableList<Declr>>
            StructDeclaratorList { get; } = new NamedParser<ImmutableList<Declr>>("struct-declarator-list");

        public static NamedParser<ParamDecln>
            ParameterDeclaration { get; } = new NamedParser<ParamDecln>("parameter-declaration");

        public static NamedParser<Declr>
            AbstractDeclarator { get; } = new NamedParser<Declr>("abstract-declarator");

        public static NamedParser<Declr>
            DirectAbstractDeclarator { get; } = new NamedParser<Declr>("direct-abstract-declarator");

        public static NamedParser<Initr>
            Initializer { get; } = new NamedParser<Initr>("initializer");

        public static NamedParser<InitList>
            InitializerList { get; } = new NamedParser<InitList>("initializer-list");

        public static NamedParser<TypeName>
            TypeName { get; } = new NamedParser<TypeName>("type-name");

        public static NamedParser<String>
            TypeDefName { get; } = new NamedParser<string>("typedef-name");

        public static void SetDeclarationRules() {

            /// <summary>
            /// declaration
            ///   : declaration-specifiers [init-declarator-list]? ';'
            /// </summary>
            // TODO: Update environment
            Declaration.Is(
                (DeclarationSpecifiers)
                .Then(InitDeclaratorList.Optional(ImmutableList<InitDeclr>.Empty))
                .Then(SEMICOLON)
                .Then((Tuple<ImmutableList<InitDeclr>, DeclnSpecs> _) => new Decln(_.Item2, _.Item1))
            );

            /// <summary>
            /// declaration-specifiers
            ///   : [ storage-class-specifier | type-specifier | type-qualifier ]+
            /// </summary>
            /// <remarks>
            /// 1. You can only have **one** storage class specifier.
            /// 2. You can have duplicate type qualifiers, since it doesn't cause ambiguity.
            /// </remarks>
            // TODO: decl specs is different with specqual
            DeclarationSpecifiers.Is(
                Parser.Seed(new DeclnSpecs())
                .Then(
                    (
                        StorageClassSpecifier
                        .Then(
                            (DeclnSpecs declnSpecs, StorageClsSpec storageClsSpec) => new DeclnSpecs(declnSpecs.StorageClsSpecs.Add(storageClsSpec), declnSpecs.TypeSpecs, declnSpecs.TypeQuals)
                        )
                    ).Or(
                        TypeSpecifier
                        .Then(
                            (DeclnSpecs declnSpecs, TypeSpec typeSpec) => new DeclnSpecs(declnSpecs.StorageClsSpecs, declnSpecs.TypeSpecs.Add(typeSpec), declnSpecs.TypeQuals)
                        )
                    ).Or(
                        TypeQualifier
                        .Then(
                            (DeclnSpecs declnSpecs, TypeQual typeQual) => new DeclnSpecs(declnSpecs.StorageClsSpecs, declnSpecs.TypeSpecs, declnSpecs.TypeQuals.Add(typeQual))
                        )
                    ).OneOrMore()
                )
            );

            /// <summary>
            /// init-declarator-list
            ///   : init-declarator [ ',' init-declarator ]*
            /// 
            /// <remarks>
            /// a non-empty list of init_declarators separated by ','
            /// </remarks>
            /// </summary>
            InitDeclaratorList.Is(
                InitDeclarator.OneOrMore(COMMA)
            );

            /// <summary>
            /// init-declarator
            ///   : declarator [ '=' initializer ]?
            /// </summary>
            InitDeclarator.Is(
                (Declarator)
                .Then(
                    EQUAL
                    .Then(Initializer)
                    .Optional()
                    .Then(InitDeclr.Create)
                )
            );

            /// <summary>
            /// storage_class_specifier
            ///   : auto | register | static | extern | typedef
            /// 
            /// <remarks>
            /// There can only be *one* storage class specifier in one declaration.
            /// </remarks>
            /// </summary>
            StorageClassSpecifier.Is(
                (AUTO)
                .Or(REGISTER)
                .Or(STATIC)
                .Or(EXTERN)
                .Or(TYPEDEF)
            );

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
            TypeSpecifier.Is(
                (
                    (VOID)
                    .Or(CHAR)
                    .Or(SHORT)
                    .Or(INT)
                    .Or(LONG)
                    .Or(FLOAT)
                    .Or(DOUBLE)
                    .Or(SIGNED)
                    .Or(UNSIGNED)
                ).Then(kind => new TypeSpec(kind))
                .Or(StructOrUnionSpecifier)
                .Or(EnumSpecifier)
                .Or(TypeDefName.Then(name => new TypedefName(name)))
            );

            /// <summary>
            /// type_qualifier
            ///   : const
            ///   | volatile
            /// 
            /// <remarks>
            /// Note that there can be multiple type qualifiers in one declarations.
            /// </remarks>
            /// </summary>
            TypeQualifier.Is(
                (CONST).Or(VOLATILE)
            );

            /// <summary>
            /// pointer
            ///   : [ '*' [type_qualifier_list]? ]+
            /// </summary>
            Pointer.Is(
                (
                    MULT.
                    Then(TypeQualifierList.Optional(ImmutableList<TypeQual>.Empty))
                    .Then((ImmutableList<TypeQual> typeQuals) => new PointerModifier(typeQuals))
                ).OneOrMore()
                .Then((ImmutableList<PointerModifier> pointerModifiers) => pointerModifiers.Reverse())
            );

            /// <summary>
            /// parameter_type_list
            ///   : parameter_list [ ',' '...' ]?
            /// 
            /// a parameter list and an optional vararg signature
            /// used in function declarations
            /// </summary>
            ParameterTypeList.Is(
                ParameterList
                .Then(
                    COMMA.Then(PERIOD).Then(PERIOD).Then(PERIOD)
                    .Optional()
                    .Then((ImmutableList<ParamDecln> paramDecln, Boolean hasVarArgs) => new ParameterTypeList(paramDecln, hasVarArgs))
                )
            );

            /// <summary>
            /// parameter-list
            ///   : parameter-declaration [ ',' parameter-declaration ]*
            /// 
            /// a non-empty list of parameters separated by ','
            /// used in a function signature
            /// </summary>
            ParameterList.Is(
                ParameterDeclaration.OneOrMore(COMMA)
            );

            /// <summary>
            /// type-qualifier-list
            ///   : [type-qualifier]+
            /// 
            /// a non-empty list of type qualifiers
            /// </summary>
            TypeQualifierList.Is(
                TypeQualifier.OneOrMore()
            );

            /// <summary>
            /// direct-declarator
            ///   : [
            ///         identifier | '(' declarator ')'
            ///     ] [
            ///         '[' [constant-expression]? ']'
            ///       | '(' [parameter-type-list]? ')'
            ///     ]*
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
            // TODO: direct declarator should be different with declarator
            DirectDeclarator.Is(
                (
                    IDENTIFIER.Then(name => new Declr(name, ImmutableList<TypeModifier>.Empty))
                    .Or((LEFT_PAREN).Then(Declarator).Then(RIGHT_PAREN))
                ).Then(
                    (
                        (LEFT_BRACKET)
                        .Then(ConstantExpression.Optional())
                        .Then(RIGHT_BRACKET)
                        .Then((Option<Expr> expr) => new ArrayModifier(expr))
                        .Then(
                            (Declr declr, ArrayModifier modifier) => new Declr(declr.name, declr.declr_modifiers.ToImmutableList().Add(modifier))
                        )
                    ).Or(
                        (LEFT_PAREN)
                        .Then(
                            ParameterTypeList
                            .Optional(new ParameterTypeList(ImmutableList<ParamDecln>.Empty))
                        )
                        .Then(RIGHT_PAREN)
                        .Then((ParameterTypeList paramTypeList) => new FunctionModifier(paramTypeList))
                        .Then(
                            (Declr declr, FunctionModifier modifier) => new Declr(declr.name, declr.declr_modifiers.ToImmutableList().Add(modifier))
                        )
                    )
                    .ZeroOrMore()
                )
            );

            /// <summary>
            /// enum-specifier
            ///   : enum [identifier]? '{' enumerator-list '}'
            ///   | enum identifier
            /// </summary>
            // TODO: make name optional
            EnumSpecifier.Is(
                ENUM.Then(
                    (
                        IDENTIFIER.Optional("")
                        .Then(
                            (LEFT_CURLY_BRACE).Then(EnumeratorList).Then(RIGHT_CURLY_BRACE)
                            .Then(
                                (String name, IReadOnlyList<Enumr> enumrs) => new EnumSpec(name, enumrs)
                            )
                        )
                    ).Or(
                        IDENTIFIER.Then(name => new EnumSpec(name, ImmutableList<Enumr>.Empty))
                    )
                )
            );

            /// <summary>
            /// enumerator-list
            ///   : enumerator [ ',' enumerator ]*
            /// </summary>
            EnumeratorList.Is(
                Enumerator.OneOrMore(COMMA)
            );

            /// <summary>
            /// enumerator
            ///   : enumeration [ '=' constant_expression ]?
            /// </summary>
            // TODO: null -> option
            Enumerator.Is(
                EnumerationConstant
                .Then(
                    (EQUAL).Then(ConstantExpression).Optional()
                    .Then(Enumr.Create)
                )
            );

            /// <summary>
            /// enumeration-constant
            ///   : identifier
            /// </summary>
            EnumerationConstant.Is(
                IDENTIFIER
            );

            /// <summary>
            /// struct-or-union-specifier
            ///   : struct-or-union [identifier]? { struct-declaration-list }
            ///   | struct-or-union identifier
            /// 
            /// <remarks>
            /// Note: if no struct-declaration-list given, the type is considered incomplete.
            /// </remarks>
            /// </summary>
            // TODO: modify StructOrUnionSpec
            StructOrUnionSpecifier.Is(
                (StructOrUnion)
                .Then(IDENTIFIER.Optional(""))
                .Then(LEFT_CURLY_BRACE)
                .Then(StructDeclarationList)
                .Then(RIGHT_CURLY_BRACE)
                .Then((structOrUnion, name, structDeclns) => {
                    if (structOrUnion == SyntaxTree.StructOrUnion.STRUCT) {
                        return new StructSpec(name, structDeclns) as StructOrUnionSpec;
                    } else {
                        return new UnionSpec(name, structDeclns);
                    }
                })
            );

            /// <summary>
            /// struct-or-union
            ///   : struct | union
            /// </summary>
            StructOrUnion.Is(
                (STRUCT).Or(UNION)
            );

            /// <summary>
            /// struct-declaration-list
            ///   : [struct-declaration]+
            /// </summary>
            StructDeclarationList.Is(
                StructDeclaration.OneOrMore()
            );

            /// <summary>
            /// struct-declaration
            ///   : specifier-qualifier-list struct-declarator-list ';'
            /// 
            /// <remarks>
            /// Note that a struct declaration does not need a storage class specifier.
            /// </remarks>
            /// </summary>
            // TODO: structDecln isn't Decln. SpecQual isn't DeclnSpecs.
            StructDeclaration.Is(
                (SpecifierQualifierList)
                .Then(StructDeclaratorList)
                .Then(SEMICOLON)
                .Then((Tuple<ImmutableList<Declr>, DeclnSpecs> _) => new StructDecln(_.Item2, _.Item1.ToList()))
            );

            /// <summary>
            /// specifier-qualifier-list
            ///   : [ type-specifier | type-qualifier ]+
            /// </summary>
            SpecifierQualifierList.Is(

            );
        }
    }
}
