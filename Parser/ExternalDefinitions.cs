using AST;
using static Parsing.ParserCombinator;

namespace Parsing {
    public partial class CParsers {
        
        /// <summary>
        /// translation-unit
        ///   : [external-declaration]+
        /// </summary>
        public static NamedParser<TranslnUnit>
            TranslationUnit { get; } = new NamedParser<TranslnUnit>("translation-unit");

        /// <summary>
        /// external-declaration
        ///   : function-definition | declaration
        /// </summary>
        public static NamedParser<IExternDecln>
            ExternalDeclaration { get; } = new NamedParser<IExternDecln>("external-declaration");

        /// <summary>
        /// function-definition
        ///   : [declaration-specifiers]? declarator [declaration-list]? compound-statement
        ///
        /// NOTE: the optional declaration_list is for the **old-style** function prototype like this:
        /// +-------------------------------+
        /// |    int foo(param1, param2)    |
        /// |    int param1;                |
        /// |    char param2;               |
        /// |    {                          |
        /// |        ....                   |
        /// |    }                          |
        /// +-------------------------------+
        ///
        /// i'm **not** going to support this style. function prototypes should always be like this:
        /// +------------------------------------------+
        /// |    int foo(int param1, char param2) {    |
        /// |        ....                              |
        /// |    }                                     |
        /// +------------------------------------------+
        ///
        /// so the grammar becomes:
        /// function-definition
        ///   : [declaration-specifiers]? declarator compound-statement
        /// </summary>
        public static NamedParser<FuncDef>
            FunctionDefinition { get; } = new NamedParser<FuncDef>("function-definition");

        public static void SetExternalDefinitionRules() {

            // translation-unit
            //   : [external-declaration]+
            TranslationUnit.Is(
                (ExternalDeclaration)
                .OneOrMore()
                .Then(TranslnUnit.Create)
            );

            // external-declaration
            //   : function-definition | declaration
            ExternalDeclaration.Is(
                Either<IExternDecln>(
                    FunctionDefinition
                ).Or(
                    Declaration
                )
            );

            // function-definition
            //   : [declaration-specifiers]? declarator compound-statement
            FunctionDefinition.Is(
                DeclarationSpecifiers.Optional()
                .Then(Declarator)
                .Then(CompoundStatement)
                .Then(FuncDef.Create)
            );

        }
    }
}
