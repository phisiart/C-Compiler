using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyntaxTree;

namespace Parsing {

    public partial class CParser {
        
        public static IParser<String> IDENTIFIER { get; } = new IdentifierParser();
        public static IParser<Expr> CONST_CHAR { get; } = new ConstCharParser();
        public static IParser<Expr> CONST_INT { get; } = new ConstIntParser();
        public static IParser<Expr> CONST_FLOAT { get; } = new ConstFloatParser();
        public static IParser<Expr> STRING_LITERAL { get; } = new StringLiteralParser();

        public static IConsumer LEFT_PAREN { get; } = OperatorConsumer.Create(OperatorVal.LPAREN);
        public static IConsumer RIGHT_PAREN { get; } = OperatorConsumer.Create(OperatorVal.RPAREN);
        public static IConsumer QUESTION { get; } = OperatorConsumer.Create(OperatorVal.QUESTION);
        public static IConsumer COMMA { get; } = OperatorConsumer.Create(OperatorVal.COMMA);
        public static IConsumer COLON { get; } = OperatorConsumer.Create(OperatorVal.COLON);

        public static IConsumer ASSIGN { get; } = OperatorConsumer.Create(OperatorVal.ASSIGN);
        public static IConsumer MULT_ASSIGN { get; } = OperatorConsumer.Create(OperatorVal.MULTASSIGN);
        public static IConsumer DIV_ASSIGN { get; } = OperatorConsumer.Create(OperatorVal.DIVASSIGN);
        public static IConsumer MOD_ASSIGN { get; } = OperatorConsumer.Create(OperatorVal.MODASSIGN);
        public static IConsumer ADD_ASSIGN { get; } = OperatorConsumer.Create(OperatorVal.ADDASSIGN);
        public static IConsumer SUB_ASSIGN { get; } = OperatorConsumer.Create(OperatorVal.SUBASSIGN);
        public static IConsumer LEFT_SHIFT_ASSIGN { get; } = OperatorConsumer.Create(OperatorVal.LSHIFTASSIGN);
        public static IConsumer RIGHT_SHIFT_ASSIGN { get; } = OperatorConsumer.Create(OperatorVal.RSHIFTASSIGN);
        public static IConsumer BITWISE_AND_ASSIGN { get; } = OperatorConsumer.Create(OperatorVal.ANDASSIGN);
        public static IConsumer XOR_ASSIGN { get; } = OperatorConsumer.Create(OperatorVal.XORASSIGN);
        public static IConsumer BITWISE_OR_ASSIGN { get; } = OperatorConsumer.Create(OperatorVal.ORASSIGN);
        public static IConsumer LEFT_BRACKET { get; } = OperatorConsumer.Create(OperatorVal.LBRACKET);
        public static IConsumer RIGHT_BRACKET { get; } = OperatorConsumer.Create(OperatorVal.RBRACKET);
        public static IConsumer LEFT_CURLY_BRACE { get; } = OperatorConsumer.Create(OperatorVal.LCURL);
        public static IConsumer RIGHT_CURLY_BRACE { get; } = OperatorConsumer.Create(OperatorVal.RCURL);
        public static IConsumer PERIOD { get; } = OperatorConsumer.Create(OperatorVal.PERIOD);
        public static IConsumer RIGHT_ARROW { get; } = OperatorConsumer.Create(OperatorVal.RARROW);
        public static IConsumer INCREMENT { get; } = OperatorConsumer.Create(OperatorVal.INC);
        public static IConsumer DECREMENT { get; } = OperatorConsumer.Create(OperatorVal.DEC);
        public static IConsumer MULT { get; } = OperatorConsumer.Create(OperatorVal.MULT);
        public static IConsumer ADD { get; } = OperatorConsumer.Create(OperatorVal.ADD);
        public static IConsumer SUB { get; } = OperatorConsumer.Create(OperatorVal.SUB);
        public static IConsumer BITWISE_NOT { get; } = OperatorConsumer.Create(OperatorVal.TILDE);
        public static IConsumer LOGICAL_NOT { get; } = OperatorConsumer.Create(OperatorVal.NOT);
        public static IConsumer DIV { get; } = OperatorConsumer.Create(OperatorVal.DIV);
        public static IConsumer MOD { get; } = OperatorConsumer.Create(OperatorVal.MOD);
        public static IConsumer LEFT_SHIFT { get; } = OperatorConsumer.Create(OperatorVal.LSHIFT);
        public static IConsumer RIGHT_SHIFT { get; } = OperatorConsumer.Create(OperatorVal.RSHIFT);
        public static IConsumer LESS { get; } = OperatorConsumer.Create(OperatorVal.LT);
        public static IConsumer GREATER { get; } = OperatorConsumer.Create(OperatorVal.GT);
        public static IConsumer LESS_EQUAL { get; } = OperatorConsumer.Create(OperatorVal.LEQ);
        public static IConsumer GREATER_EQUAL { get; } = OperatorConsumer.Create(OperatorVal.GEQ);
        public static IConsumer EQUAL { get; } = OperatorConsumer.Create(OperatorVal.EQ);
        public static IConsumer NOT_EQUAL { get; } = OperatorConsumer.Create(OperatorVal.NEQ);
        public static IConsumer XOR { get; } = OperatorConsumer.Create(OperatorVal.XOR);
        public static IConsumer BITWISE_AND { get; } = OperatorConsumer.Create(OperatorVal.BITAND);
        public static IConsumer BITWISE_OR { get; } = OperatorConsumer.Create(OperatorVal.BITOR);
        public static IConsumer LOGICAL_AND { get; } = OperatorConsumer.Create(OperatorVal.AND);
        public static IConsumer LOGICAL_OR { get; } = OperatorConsumer.Create(OperatorVal.OR);
        public static IConsumer SEMICOLON { get; } = OperatorConsumer.Create(OperatorVal.SEMICOLON);

        public static IConsumer SIZEOF { get; } = KeywordConsumer.Create(KeywordVal.SIZEOF);
        public static IParser<StorageClsSpec> AUTO { get; } = KeywordParser.Create(KeywordVal.AUTO, StorageClsSpec.AUTO);
        public static IParser<StorageClsSpec> REGISTER { get; } = KeywordParser.Create(KeywordVal.REGISTER, StorageClsSpec.REGISTER);
        public static IParser<StorageClsSpec> STATIC { get; } = KeywordParser.Create(KeywordVal.STATIC, StorageClsSpec.STATIC);
        public static IParser<StorageClsSpec> EXTERN { get; } = KeywordParser.Create(KeywordVal.EXTERN, StorageClsSpec.EXTERN);
        public static IParser<StorageClsSpec> TYPEDEF { get; } = KeywordParser.Create(KeywordVal.TYPEDEF, StorageClsSpec.TYPEDEF);

        public static IParser<TypeSpec.Kind> VOID { get; } = KeywordParser.Create(KeywordVal.VOID, TypeSpec.Kind.VOID);
        public static IParser<TypeSpec.Kind> CHAR { get; } = KeywordParser.Create(KeywordVal.CHAR, TypeSpec.Kind.CHAR);
        public static IParser<TypeSpec.Kind> SHORT { get; } = KeywordParser.Create(KeywordVal.SHORT, TypeSpec.Kind.SHORT);
        public static IParser<TypeSpec.Kind> INT { get; } = KeywordParser.Create(KeywordVal.INT, TypeSpec.Kind.INT);
        public static IParser<TypeSpec.Kind> LONG { get; } = KeywordParser.Create(KeywordVal.LONG, TypeSpec.Kind.LONG);
        public static IParser<TypeSpec.Kind> FLOAT { get; } = KeywordParser.Create(KeywordVal.FLOAT, TypeSpec.Kind.FLOAT);
        public static IParser<TypeSpec.Kind> DOUBLE { get; } = KeywordParser.Create(KeywordVal.DOUBLE, TypeSpec.Kind.DOUBLE);
        public static IParser<TypeSpec.Kind> SIGNED { get; } = KeywordParser.Create(KeywordVal.SIGNED, TypeSpec.Kind.SIGNED);
        public static IParser<TypeSpec.Kind> UNSIGNED { get; } = KeywordParser.Create(KeywordVal.UNSIGNED, TypeSpec.Kind.UNSIGNED);

        public static IParser<TypeQual> CONST { get; } = KeywordParser.Create(KeywordVal.CONST, TypeQual.CONST);
        public static IParser<TypeQual> VOLATILE { get; } = KeywordParser.Create(KeywordVal.VOLATILE, TypeQual.VOLATILE);

        public static IConsumer ENUM { get; } = KeywordConsumer.Create(KeywordVal.ENUM);
        public static IParser<StructOrUnion> STRUCT { get; } = KeywordParser.Create(KeywordVal.STRUCT, SyntaxTree.StructOrUnion.STRUCT);
        public static IParser<StructOrUnion> UNION { get; } = KeywordParser.Create(KeywordVal.UNION, SyntaxTree.StructOrUnion.UNION);

        public static IConsumer GOTO { get; } = KeywordConsumer.Create(KeywordVal.GOTO);
        public static IParser<Stmt> CONTINUE { get; } = KeywordParser.Create(KeywordVal.CONTINUE, new ContStmt());
        public static IParser<Stmt> BREAK { get; } = KeywordParser.Create(KeywordVal.BREAK, new BreakStmt());
        public static IConsumer RETURN { get; } = KeywordConsumer.Create(KeywordVal.RETURN);

        public static IConsumer WHILE { get; } = KeywordConsumer.Create(KeywordVal.WHILE);
        public static IConsumer DO { get; } = KeywordConsumer.Create(KeywordVal.DO);
        public static IConsumer FOR { get; } = KeywordConsumer.Create(KeywordVal.FOR);

        public static IConsumer IF { get; } = KeywordConsumer.Create(KeywordVal.IF);
        public static IConsumer ELSE { get; } = KeywordConsumer.Create(KeywordVal.ELSE);
        public static IConsumer SWITCH { get; } = KeywordConsumer.Create(KeywordVal.SWITCH);

        public static IConsumer CASE { get; } = KeywordConsumer.Create(KeywordVal.CASE);
        public static IConsumer DEFAULT { get; } = KeywordConsumer.Create(KeywordVal.DEFAULT);
    }
}
