using System;
using System.Collections.Immutable;
using System.Linq;
using static Parsing.ParserCombinator;
using SyntaxTree;

namespace Parsing {
    public partial class CParser {

        /// <summary>
        /// declaration
        ///   : declaration-specifiers [init-declarator-list]? ';'
        /// </summary>
        public static NamedParser<Decln>
            Declaration { get; } = new NamedParser<Decln>("declaration");

        /// <summary>
        /// declaration-specifiers
        ///   : [ storage-class-specifier | type-specifier | type-qualifier ]+
        /// </summary>
        /// <remarks>
        /// 1. You can only have **one** storage class specifier.
        /// 2. You can have duplicate type qualifiers, since it doesn't cause ambiguity.
        /// </remarks>
        public static NamedParser<DeclnSpecs>
            DeclarationSpecifiers { get; } = new NamedParser<DeclnSpecs>("declaration-specifiers");

        /// <summary>
        /// init-declarator-list
        ///   : init-declarator [ ',' init-declarator ]*
        /// </summary>
        /// <remarks>
        /// a non-empty list of init_declarators separated by ','
        /// </remarks>
        public static NamedParser<ImmutableList<InitDeclr>>
            InitDeclaratorList { get; } = new NamedParser<ImmutableList<InitDeclr>>("init-declarator-list");

        /// <summary>
        /// init-declarator
        ///   : declarator [ '=' initializer ]?
        /// </summary>
        public static NamedParser<InitDeclr>
            InitDeclarator { get; } = new NamedParser<InitDeclr>("init-declarator");

        /// <summary>
        /// storage-class-specifier
        ///   : auto | register | static | extern | typedef
        /// </summary>
        /// <remarks>
        /// There can only be *one* storage class specifier in one declaration.
        /// </remarks>
        public static NamedParser<StorageClsSpec>
            StorageClassSpecifier { get; } = new NamedParser<StorageClsSpec>("storage-class-specifier");

        /// <summary>
        /// type-specifier
        ///   : void
        ///   | char
        ///   | short
        ///   | int
        ///   | long
        ///   | float
        ///   | double
        ///   | signed
        ///   | unsigned
        ///   | struct-or-union-specifier
        ///   | enum-specifier
        ///   | typedef-name
        /// </summary>
        /// <remarks>
        /// 1. void, char, short, int, long, float, double, signed, unsigned are called "basic type specifiers".
        /// 2. struct-or-union_specifier and enum-specifier need more complicated parsing.
        /// 3. Parsing typedef-name actually requires the environment to participate. For example, consider this statement:
        ///      T *v;
        ///    Is T a type or an object? If T is a type, then this statement is a declaration: v is a pointer; if T is a object, then this statement is an expression.
        ///    So, we need to keep track of the typedefs in the environment even in the parsing stage!
        /// </remarks>
        public static NamedParser<TypeSpec>
            TypeSpecifier { get; } = new NamedParser<TypeSpec>("type-specifier");

        /// <summary>
        /// type-qualifier
        ///   : const
        ///   | volatile
        /// </summary>
        /// <remarks>
        /// Note that there can be multiple type qualifiers in one declarations.
        /// </remarks>
        public static NamedParser<TypeQual>
            TypeQualifier { get; } = new NamedParser<TypeQual>("type-qualifier");

        /// <summary>
        /// declarator
        ///   : [pointer]? direct-declarator
        /// </summary>
        /// <remarks>
        /// A declarator gives a name to the object and also modifies the type.
        /// </remarks>
        public static NamedParser<Declr>
            Declarator { get; } = new NamedParser<Declr>("declarator");

        /// <summary>
        /// pointer
        ///   : [ '*' [type-qualifier-list]? ]+
        /// </summary>
        public static NamedParser<ImmutableList<PointerModifier>>
            Pointer { get; } = new NamedParser<ImmutableList<PointerModifier>>("pointer");

        /// <summary>
        /// parameter-type-list
        ///   : parameter-list [ ',' '...' ]?
        /// </summary>
        /// <remarks>
        /// A parameter list and an optional vararg signature.
        /// Used in function declarations.
        /// </remarks>
        public static NamedParser<ParamTypeList>
            ParameterTypeList { get; } = new NamedParser<ParamTypeList>("parameter-type-list");

        /// <summary>
        /// parameter-list
        ///   : parameter-declaration [ ',' parameter-declaration ]*
        /// </summary>
        /// <remarks>
        /// A non-empty list of parameters separated by ','.
        /// Used in a function signature.
        /// </remarks>
        public static NamedParser<ImmutableList<ParamDecln>>
            ParameterList { get; } = new NamedParser<ImmutableList<ParamDecln>>("parameter-list");

        /// <summary>
        /// type-qualifier-list
        ///   : [type-qualifier]+
        /// </summary>
        /// <remarks>
        /// A non-empty list of type qualifiers.
        /// </remarks>
        public static NamedParser<ImmutableList<TypeQual>>
            TypeQualifierList { get; } = new NamedParser<ImmutableList<TypeQual>>("type-qualifier-list");

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
        public static NamedParser<Declr>
            DirectDeclarator { get; } = new NamedParser<Declr>("direct-declarator");

        /// <summary>
        /// enum-specifier
        ///   : enum [identifier]? '{' enumerator-list '}'
        ///   | enum identifier
        /// </summary>
        public static NamedParser<EnumSpec>
            EnumSpecifier { get; } = new NamedParser<EnumSpec>("enum-specifier");

        /// <summary>
        /// enumerator-list
        ///   : enumerator [ ',' enumerator ]*
        /// </summary>
        public static NamedParser<ImmutableList<Enumr>>
            EnumeratorList { get; } = new NamedParser<ImmutableList<Enumr>>("enumerator-list");

        /// <summary>
        /// enumerator
        ///   : enumeration-constant [ '=' constant-expression ]?
        /// </summary>
        public static NamedParser<Enumr>
            Enumerator { get; } = new NamedParser<Enumr>("enumerator");

        /// <summary>
        /// enumeration-constant
        ///   : identifier
        /// </summary>
        public static NamedParser<String>
            EnumerationConstant { get; } = new NamedParser<string>("enumeration-constant");

        /// <summary>
        /// struct-or-union-specifier
        ///   : struct-or-union [identifier]? { struct-declaration-list }
        ///   | struct-or-union identifier
        /// </summary>
        /// <remarks>
        /// Note: if no struct-declaration-list given, the type is considered incomplete.
        /// </remarks>
        public static NamedParser<StructOrUnionSpec>
            StructOrUnionSpecifier { get; } = new NamedParser<StructOrUnionSpec>("struct-or-union-specifier");

        /// <summary>
        /// struct-or-union
        ///   : struct | union
        /// </summary>
        public static NamedParser<StructOrUnion>
            StructOrUnion { get; } = new NamedParser<StructOrUnion>("struct-or-union");

        /// <summary>
        /// struct-declaration-list
        ///   : [struct-declaration]+
        /// </summary>
        public static NamedParser<ImmutableList<StructDecln>>
            StructDeclarationList { get; } = new NamedParser<ImmutableList<StructDecln>>("struct-declaration-list");

        /// <summary>
        /// struct-declaration
        ///   : specifier-qualifier-list struct-declarator-list ';'
        /// </summary>
        /// <remarks>
        /// Note that a struct declaration does not need a storage class specifier.
        /// </remarks>
        public static NamedParser<StructDecln>
            StructDeclaration { get; } = new NamedParser<StructDecln>("struct-declaration");

        /// <summary>
        /// specifier-qualifier-list
        ///   : [ type-specifier | type-qualifier ]+
        /// </summary>
        public static NamedParser<SpecQualList>
            SpecifierQualifierList { get; } = new NamedParser<SpecQualList>("specifier-qualifier-list");

        /// <summary>
        /// struct-declarator-list
        ///   : struct-declarator [ ',' struct-declarator ]*
        /// </summary>
        public static NamedParser<ImmutableList<IStructDeclr>>
            StructDeclaratorList { get; } = new NamedParser<ImmutableList<IStructDeclr>>("struct-declarator-list");

        /// <summary>
        /// struct-declarator
        ///   : [declarator]? ':' constant-expression
        ///   | declarator
        /// </summary>
        /// <remarks>
        /// Note that the second one represents a 'bit-field', which I'm not going to support.
        /// </remarks>
        public static NamedParser<IStructDeclr>
            StructDeclarator { get; } = new NamedParser<IStructDeclr>("struct-declarator");

        /// <summary>
        /// parameter-declaration
        ///   : declaration-specifiers [ declarator | abstract-declarator ]?
        /// </summary>
        /// <remarks>
        /// int foo(int arg1, int arg2);
        ///         ~~~~~~~~
        /// 
        /// int foo(int, int);
        ///         ~~~
        /// 
        /// The declarator can be completely omitted.
        /// </remarks>
        public static NamedParser<ParamDecln>
            ParameterDeclaration { get; } = new NamedParser<ParamDecln>("parameter-declaration");

        // identifier_list
        //   : /* old style, i'm deleting this */

        /// <summary>
        /// abstract-declarator
        ///   : [pointer]? direct-abstract-declarator
        ///   | pointer
        /// </summary>
        /// <remarks>
        /// An abstract declarator is a non-empty list of (pointer, function, or array) type modifiers
        /// </remarks>
        public static NamedParser<AbstractDeclr>
            AbstractDeclarator { get; } = new NamedParser<AbstractDeclr>("abstract-declarator");

        /// <summary>
        /// direct-abstract-declarator
        ///   : [
        ///         '(' abstract-declarator ')'
        ///       | '[' [constant-expression]? ']'  // array modifier
        ///       | '(' [parameter-type_list]? ')'  // function modifier
        ///     ] [
        ///         '[' [constant-expression]? ']'  // array modifier
        ///       | '(' [parameter-type-list]? ')'  // function modifier
        ///     ]*
        /// </summary>
        public static NamedParser<AbstractDeclr>
            DirectAbstractDeclarator { get; } = new NamedParser<AbstractDeclr>("direct-abstract-declarator");

        /// <summary>
        /// initializer
        ///   : assignment-expression
        ///   | '{' initializer-list '}'
        ///   | '{' initializer-list ',' '}'
        /// </summary>
        public static NamedParser<Initr>
            Initializer { get; } = new NamedParser<Initr>("initializer");

        /// <summary>
        /// initializer-list
        ///   : initializer [ ',' initializer ]*
        /// 
        /// A non-empty list of initializers.
        /// </summary>
        public static NamedParser<Initr>
            InitializerList { get; } = new NamedParser<Initr>("initializer-list");

        /// <summary>
        /// type-name
        ///   : specifier-qualifier-list [abstract-declarator]?
        /// </summary>
        public static NamedParser<TypeName>
            TypeName { get; } = new NamedParser<TypeName>("type-name");

        /// <summary>
        /// typedef-name
        ///   : identifier
        /// </summary>
        /// <remarks>
        /// It must be something already defined.
        /// We need to look it up in the parser environment.
        /// </remarks>
        public static NamedParser<String>
            TypeDefName { get; } = new NamedParser<String>("typedef-name");

        public static void SetDeclarationRules() {

            // declaration
            //   : declaration-specifiers [init-declarator-list]? ';'
            Declaration.Is(
                (DeclarationSpecifiers)
                .Then(InitDeclaratorList.Optional(ImmutableList<InitDeclr>.Empty))
                .Then(SEMICOLON)
                .Then(Decln.Create)
                .TransformResult(
                    _ => {
                        var result = _.Result;
                        var env = _.Environment;
                        env = result.InitDeclrs.Aggregate(
                            seed: env,
                            func: (currentEnv, initDeclr) =>
                                      currentEnv.AddSymbol(
                                          initDeclr.Declr.Name,
                                          result.DeclnSpecs.StorageClsSpecs.DefaultIfEmpty(StorageClsSpec.AUTO).First()
                                      )
                        );
                        return ParserSucceeded.Create(result, env, _.Source);
                    }
                )
            );

            // declaration-specifiers
            //   : [ storage-class-specifier | type-specifier | type-qualifier ]+
            DeclarationSpecifiers.Is(
                Parser.Seed(DeclnSpecs.Create())
                .Then(
                    (
                        Given<DeclnSpecs>()
                        .Then(StorageClassSpecifier)
                        .Then(DeclnSpecs.Add)
                    ).Or(
                        Given<DeclnSpecs>()
                        .Then(TypeSpecifier)
                        .Then(DeclnSpecs.Add)
                    ).Or(
                        Given<DeclnSpecs>()
                        .Then(TypeQualifier)
                        .Then(DeclnSpecs.Add)
                    ).OneOrMore()
                )
            );

            // init-declarator-list
            //   : init-declarator [ ',' init-declarator ]*
            InitDeclaratorList.Is(
                InitDeclarator.OneOrMore(COMMA)
            );

            // init-declarator
            //   : declarator [ '=' initializer ]?
            InitDeclarator.Is(
                (Declarator)
                .Then(
                    (ASSIGN).Then(Initializer).Optional()
                ).Then(InitDeclr.Create)
            );

            // storage-class-specifier
            //   : auto | register | static | extern | typedef
            StorageClassSpecifier.Is(
                (AUTO)
                .Or(REGISTER)
                .Or(STATIC)
                .Or(EXTERN)
                .Or(TYPEDEF)
            );

            // type-specifier
            //   : void
            //   | char
            //   | short
            //   | int
            //   | long
            //   | float
            //   | double
            //   | signed
            //   | unsigned
            //   | struct-or-union-specifier
            //   | enum-specifier
            //   | typedef-name
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
                    .Then(kind => new BasicTypeSpec(kind) as TypeSpec)
                )
                .Or(StructOrUnionSpecifier)
                .Or(EnumSpecifier)
                .Or(TypeDefName.Then(TypedefName.Create))
            );

            // type_qualifier
            //   : const
            //   | volatile
            TypeQualifier.Is(
                (CONST).Or(VOLATILE)
            );

            // declarator
            //   : [pointer]? direct-declarator
            Declarator.Is(
                (Pointer.Optional())
                .Then(DirectDeclarator)
                .Then(Declr.Create)
            );

            // pointer
            //   : [ '*' [type-qualifier-list]? ]+
            Pointer.Is(
                (
                    MULT.
                    Then(TypeQualifierList.Optional(ImmutableList<TypeQual>.Empty))
                    .Then(PointerModifier.Create)
                ).OneOrMore()
                .Then(pointerModifiers => pointerModifiers.Reverse())
            );

            // parameter-type-list
            //   : parameter-list [ ',' '...' ]?
            ParameterTypeList.Is(
                ParameterList
                .Then(
                    (COMMA)
                    .Then(PERIOD).Then(PERIOD).Then(PERIOD)
                    .Optional()
                ).Then(ParamTypeList.Create)
            );

            // parameter-list
            //   : parameter-declaration [ ',' parameter-declaration ]*
            ParameterList.Is(
                ParameterDeclaration.OneOrMore(COMMA)
            );

            // type-qualifier-list
            //   : [type-qualifier]+
            TypeQualifierList.Is(
                TypeQualifier.OneOrMore()
            );

            // direct-declarator
            //   : [
            //         identifier | '(' declarator ')'
            //     ] [
            //         '[' [constant-expression]? ']'
            //       | '(' [parameter-type-list]? ')'
            //     ]*
            DirectDeclarator.Is(
                (
                    (IDENTIFIER).Then(Declr.Create)
                    .Or((LEFT_PAREN).Then(Declarator).Then(RIGHT_PAREN))
                ).Then(
                    (
                        Given<Declr>()
                        .Then(LEFT_BRACKET)
                        .Then(ConstantExpression.Optional().Then(ArrayModifier.Create))
                        .Then(RIGHT_BRACKET)
                        .Then(Declr.Add)
                    ).Or(
                        Given<Declr>()
                        .Then(LEFT_PAREN)
                        .Then(
                            ParameterTypeList
                            .Optional()
                            .Then(FunctionModifier.Create)
                        ).Then(RIGHT_PAREN)
                        .Then(Declr.Add)
                    )
                    .ZeroOrMore()
                )
            );

            // enum-specifier
            //   : enum [identifier]? '{' enumerator-list '}'
            //   | enum identifier
            EnumSpecifier.Is(
                (ENUM)
                .Then(
                    (
                        IDENTIFIER.Optional()
                        .Then(LEFT_CURLY_BRACE)
                        .Then(EnumeratorList)
                        .Then(RIGHT_CURLY_BRACE)
                        .Then(EnumSpec.Create)
                    ).Or(
                        (IDENTIFIER)
                        .Then(EnumSpec.Create)
                    )
                )
            );

            // enumerator-list
            //   : enumerator [ ',' enumerator ]*
            EnumeratorList.Is(
                Enumerator.OneOrMore(COMMA)
            );

            // enumerator
            //   : enumeration-constant [ '=' constant-expression ]?
            Enumerator.Is(
                EnumerationConstant
                .Then(
                    (ASSIGN)
                    .Then(ConstantExpression)
                    .Optional()
                ).Then(Enumr.Create)
            );

            // enumeration-constant
            //   : identifier
            EnumerationConstant.Is(
                IDENTIFIER
            );

            // struct-or-union-specifier
            //   : struct-or-union [identifier]? { struct-declaration-list }
            //   | struct-or-union identifier
            StructOrUnionSpecifier.Is(
                (StructOrUnion)
                .Then(
                    (
                        Given<StructOrUnion>()
                        .Then(IDENTIFIER.Optional())
                        .Then(LEFT_CURLY_BRACE)
                        .Then(StructDeclarationList)
                        .Then(RIGHT_CURLY_BRACE)
                        .Then(StructOrUnionSpec.Create)
                    ).Or(
                        Given<StructOrUnion>()
                        .Then(IDENTIFIER)
                        .Then(StructOrUnionSpec.Create)
                    )
                )
            );

            // struct-or-union
            //   : struct | union
            StructOrUnion.Is(
                (STRUCT).Or(UNION)
            );

            // struct-declaration-list
            //   : [struct-declaration]+
            StructDeclarationList.Is(
                StructDeclaration.OneOrMore()
            );

            // struct-declaration
            //   : specifier-qualifier-list struct-declarator-list ';'
            StructDeclaration.Is(
                (SpecifierQualifierList)
                .Then(StructDeclaratorList)
                .Then(SEMICOLON)
                .Then(StructDecln.Create)
            );

            // specifier-qualifier-list
            //   : [ type-specifier | type-qualifier ]+
            SpecifierQualifierList.Is(
                Parser.Seed(SpecQualList.Empty)
                .Then(
                    (
                        Given<SpecQualList>()
                        .Then(TypeSpecifier)
                        .Then(SpecQualList.Add)
                    ).Or(
                        Given<SpecQualList>()
                        .Then(TypeQualifier)
                        .Then(SpecQualList.Add)
                    )
                    .OneOrMore()
                )
            );

            // struct-declarator-list
            //   : struct-declarator [ ',' struct-declarator ]*
            StructDeclaratorList.Is(
                StructDeclarator.OneOrMore(COMMA)
            );

            // struct-declarator
            //   : [declarator]? ':' constant-expression
            //   | declarator
            StructDeclarator.Is(
                (
                    Declarator.Optional()
                    .Then(COLON)
                    .Then(ConstantExpression)
                    .Then(BitFieldDeclr.Create)
                    as IParser<IStructDeclr>
                ).Or(
                    Declarator
                )
            );
            
            // parameter-declaration
            //   : declaration-specifiers [ declarator | abstract-declarator ]?
            ParameterDeclaration.Is(
                (DeclarationSpecifiers)
                .Then(
                    ((IParser<IParamDeclr>)Declarator)
                    .Or(AbstractDeclarator)
                    .Optional(AbstractDeclr.Empty)
                ).Then(ParamDecln.Create)
            );

            // abstract-declarator
            //   : [pointer]? direct-abstract-declarator
            //   | pointer
            AbstractDeclarator.Is(
                (
                    (Pointer.Optional(ImmutableList<PointerModifier>.Empty))
                    .Then(DirectAbstractDeclarator)
                    .Then(AbstractDeclr.Add)
                ).Or(
                    (Pointer)
                    .Then(AbstractDeclr.Create)
                )
            );

            // direct-abstract-declarator
            //   : [
            //         '(' abstract-declarator ')'
            //       | '[' [constant-expression]? ']'  // array modifier
            //       | '(' [parameter-type_list]? ')'  // function modifier
            //     ] [
            //         '[' [constant-expression]? ']'  // array modifier
            //       | '(' [parameter-type-list]? ')'  // function modifier
            //     ]*
            DirectAbstractDeclarator.Is(
                (
                    (
                        (LEFT_PAREN)
                        .Then(AbstractDeclarator)
                        .Then(RIGHT_PAREN)
                    ).Or(
                        (LEFT_BRACKET)
                        .Then(ConstantExpression.Optional())
                        .Then(RIGHT_BRACKET)
                        .Then(ArrayModifier.Create)
                        .Then(ImmutableList.Create)
                        .Then(AbstractDeclr.Create)
                    ).Or(
                        (LEFT_PAREN)
                        .Then(ParameterTypeList.Optional())
                        .Then(RIGHT_PAREN)
                        .Then(FunctionModifier.Create)
                        .Then(ImmutableList.Create)
                        .Then(AbstractDeclr.Create)
                    )
                ).Then(
                    (
                        Given<AbstractDeclr>()
                        .Then(
                            LEFT_BRACKET
                            .Then(ConstantExpression.Optional())
                            .Then(RIGHT_BRACKET)
                            .Then(ArrayModifier.Create)
                        ).Then(
                            AbstractDeclr.Add
                        )
                    ).Or(
                        Given<AbstractDeclr>()
                        .Then(
                            (LEFT_PAREN)
                            .Then(ParameterTypeList.Optional())
                            .Then(RIGHT_PAREN)
                            .Then(FunctionModifier.Create)
                        ).Then(
                            AbstractDeclr.Add
                        )
                    )
                    .ZeroOrMore()
                )
            );

            // initializer
            //   : assignment-expression
            //   | '{' initializer-list '}'
            //   | '{' initializer-list ',' '}'
            Initializer.Is(
                AssignmentExpression.Then(InitExpr.Create)
                .Or(
                    (LEFT_CURLY_BRACE)
                    .Then(InitializerList)
                    .Then(RIGHT_CURLY_BRACE)
                ).Or(
                    (LEFT_CURLY_BRACE)
                    .Then(InitializerList)
                    .Then(COMMA)
                    .Then(RIGHT_CURLY_BRACE)
                )
            );

            // initializer-list
            //   : initializer [ ',' initializer ]*
            InitializerList.Is(
                Initializer.OneOrMore(COMMA)
                .Then(InitList.Create)
            );

            // type-name
            //   : specifier-qualifier-list [abstract-declarator]?
            TypeName.Is(
                (SpecifierQualifierList)
                .Then(AbstractDeclarator.Optional(AbstractDeclr.Empty))
                .Then(SyntaxTree.TypeName.Create)
            );

            // typedef-name
            //   : identifier
            TypeDefName.Is(
                (IDENTIFIER)
                .Check(
                    result => result.Environment.IsTypedefName(result.Result)
                )
            );
        }
    }
}
