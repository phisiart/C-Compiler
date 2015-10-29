using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SyntaxTree;
using static Parsing.ParserCombinator;

namespace Parsing {
    public partial class CParser {
        public static NamedParser<TranslnUnit>
            TranslationUnit { get; } = new NamedParser<TranslnUnit>("translation-unit");

        public static NamedParser<ExternDecln>
            ExternalDeclaration { get; } = new NamedParser<ExternDecln>("external-declaration");

        public static NamedParser<FuncDef>
            FunctionDefinition { get; } = new NamedParser<FuncDef>("function-definition");

        public static void SetExternalDefinitionRules() {

            /// <summary>
            /// translation-unit
            ///   : [external-declaration]+
            /// </summary>
            TranslationUnit.Is(
                ExternalDeclaration
                .OneOrMore()
                .Then(TranslnUnit.Create)
            );

            /// <summary>
            /// external-declaration
            ///   : function-definition | declaration
            /// </summary>
            ExternalDeclaration.Is(
                (FunctionDefinition as IParser<ExternDecln>)
                .Or(Declaration)
            );

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
            FunctionDefinition.Is(
                DeclarationSpecifiers.Optional(DeclnSpecs.Create())
                .Then(Declarator)
                .Then(CompoundStatement)
                .Then(FuncDef.Create)
            );

        }
    }
}
