using System;
using LexicalAnalysis;
using SyntaxTree;

namespace Parsing {

    public partial class CParsers {
        
        public static IParser<String> Identifier { get; } = new IdentifierParser();
        public static IParser<Expr> ConstChar { get; } = new ConstCharParser();
        public static IParser<Expr> ConstInt { get; } = new ConstIntParser();
        public static IParser<Expr> ConstFloat { get; } = new ConstFloatParser();
        public static IParser<Expr> StringLiteral { get; } = new StringLiteralParser();

        public static IConsumer LeftParen { get; } = OperatorConsumer.Create(OperatorVal.LPAREN);
        public static IConsumer RightParen { get; } = OperatorConsumer.Create(OperatorVal.RPAREN);
        public static IConsumer Question { get; } = OperatorConsumer.Create(OperatorVal.QUESTION);
        public static IConsumer Comma { get; } = OperatorConsumer.Create(OperatorVal.COMMA);
        public static IConsumer Colon { get; } = OperatorConsumer.Create(OperatorVal.COLON);

        public static IConsumer Assign { get; } = OperatorConsumer.Create(OperatorVal.ASSIGN);
        public static IConsumer MultAssign { get; } = OperatorConsumer.Create(OperatorVal.MULTASSIGN);
        public static IConsumer DivAssign { get; } = OperatorConsumer.Create(OperatorVal.DIVASSIGN);
        public static IConsumer ModAssign { get; } = OperatorConsumer.Create(OperatorVal.MODASSIGN);
        public static IConsumer AddAssign { get; } = OperatorConsumer.Create(OperatorVal.ADDASSIGN);
        public static IConsumer SubAssign { get; } = OperatorConsumer.Create(OperatorVal.SUBASSIGN);
        public static IConsumer LeftShiftAssign { get; } = OperatorConsumer.Create(OperatorVal.LSHIFTASSIGN);
        public static IConsumer RightShiftAssign { get; } = OperatorConsumer.Create(OperatorVal.RSHIFTASSIGN);
        public static IConsumer BitwiseAndAssign { get; } = OperatorConsumer.Create(OperatorVal.ANDASSIGN);
        public static IConsumer XorAssign { get; } = OperatorConsumer.Create(OperatorVal.XORASSIGN);
        public static IConsumer BitwiseOrAssign { get; } = OperatorConsumer.Create(OperatorVal.ORASSIGN);
        public static IConsumer LeftBracket { get; } = OperatorConsumer.Create(OperatorVal.LBRACKET);
        public static IConsumer RightBracket { get; } = OperatorConsumer.Create(OperatorVal.RBRACKET);
        public static IConsumer LeftCurlyBrace { get; } = OperatorConsumer.Create(OperatorVal.LCURL);
        public static IConsumer RightCurlyBrace { get; } = OperatorConsumer.Create(OperatorVal.RCURL);
        public static IConsumer Period { get; } = OperatorConsumer.Create(OperatorVal.PERIOD);
        public static IConsumer RightArrow { get; } = OperatorConsumer.Create(OperatorVal.RARROW);
        public static IConsumer Increment { get; } = OperatorConsumer.Create(OperatorVal.INC);
        public static IConsumer Decrement { get; } = OperatorConsumer.Create(OperatorVal.DEC);
        public static IConsumer Mult { get; } = OperatorConsumer.Create(OperatorVal.MULT);
        public static IConsumer Add { get; } = OperatorConsumer.Create(OperatorVal.ADD);
        public static IConsumer Sub { get; } = OperatorConsumer.Create(OperatorVal.SUB);
        public static IConsumer BitwiseNot { get; } = OperatorConsumer.Create(OperatorVal.TILDE);
        public static IConsumer LogicalNot { get; } = OperatorConsumer.Create(OperatorVal.NOT);
        public static IConsumer Div { get; } = OperatorConsumer.Create(OperatorVal.DIV);
        public static IConsumer Mod { get; } = OperatorConsumer.Create(OperatorVal.MOD);
        public static IConsumer LeftShift { get; } = OperatorConsumer.Create(OperatorVal.LSHIFT);
        public static IConsumer RightShift { get; } = OperatorConsumer.Create(OperatorVal.RSHIFT);
        public static IConsumer Less { get; } = OperatorConsumer.Create(OperatorVal.LT);
        public static IConsumer Greater { get; } = OperatorConsumer.Create(OperatorVal.GT);
        public static IConsumer LessEqual { get; } = OperatorConsumer.Create(OperatorVal.LEQ);
        public static IConsumer GreaterEqual { get; } = OperatorConsumer.Create(OperatorVal.GEQ);
        public static IConsumer Equal { get; } = OperatorConsumer.Create(OperatorVal.EQ);
        public static IConsumer NotEqual { get; } = OperatorConsumer.Create(OperatorVal.NEQ);
        public static IConsumer Xor { get; } = OperatorConsumer.Create(OperatorVal.XOR);
        public static IConsumer BitwiseAnd { get; } = OperatorConsumer.Create(OperatorVal.BITAND);
        public static IConsumer BitwiseOr { get; } = OperatorConsumer.Create(OperatorVal.BITOR);
        public static IConsumer LogicalAnd { get; } = OperatorConsumer.Create(OperatorVal.AND);
        public static IConsumer LogicalOr { get; } = OperatorConsumer.Create(OperatorVal.OR);
        public static IConsumer Semicolon { get; } = OperatorConsumer.Create(OperatorVal.SEMICOLON);

        public static IConsumer SizeOf { get; } = KeywordConsumer.Create(KeywordVal.SIZEOF);
        public static IParser<StorageClsSpec> Auto { get; } = KeywordParser.Create(KeywordVal.AUTO, StorageClsSpec.AUTO);
        public static IParser<StorageClsSpec> Register { get; } = KeywordParser.Create(KeywordVal.REGISTER, StorageClsSpec.REGISTER);
        public static IParser<StorageClsSpec> Static { get; } = KeywordParser.Create(KeywordVal.STATIC, StorageClsSpec.STATIC);
        public static IParser<StorageClsSpec> Extern { get; } = KeywordParser.Create(KeywordVal.EXTERN, StorageClsSpec.EXTERN);
        public static IParser<StorageClsSpec> Typedef { get; } = KeywordParser.Create(KeywordVal.TYPEDEF, StorageClsSpec.TYPEDEF);

        public static IParser<TypeSpecKind> Void { get; } = KeywordParser.Create(KeywordVal.VOID, TypeSpecKind.VOID);
        public static IParser<TypeSpecKind> Char { get; } = KeywordParser.Create(KeywordVal.CHAR, TypeSpecKind.CHAR);
        public static IParser<TypeSpecKind> Short { get; } = KeywordParser.Create(KeywordVal.SHORT, TypeSpecKind.SHORT);
        public static IParser<TypeSpecKind> Int { get; } = KeywordParser.Create(KeywordVal.INT, TypeSpecKind.INT);
        public static IParser<TypeSpecKind> Long { get; } = KeywordParser.Create(KeywordVal.LONG, TypeSpecKind.LONG);
        public static IParser<TypeSpecKind> Float { get; } = KeywordParser.Create(KeywordVal.FLOAT, TypeSpecKind.FLOAT);
        public static IParser<TypeSpecKind> Double { get; } = KeywordParser.Create(KeywordVal.DOUBLE, TypeSpecKind.DOUBLE);
        public static IParser<TypeSpecKind> Signed { get; } = KeywordParser.Create(KeywordVal.SIGNED, TypeSpecKind.SIGNED);
        public static IParser<TypeSpecKind> Unsigned { get; } = KeywordParser.Create(KeywordVal.UNSIGNED, TypeSpecKind.UNSIGNED);

        public static IParser<TypeQual> Const { get; } = KeywordParser.Create(KeywordVal.CONST, TypeQual.CONST);
        public static IParser<TypeQual> Volatile { get; } = KeywordParser.Create(KeywordVal.VOLATILE, TypeQual.VOLATILE);

        public static IConsumer Enum { get; } = KeywordConsumer.Create(KeywordVal.ENUM);
        public static IParser<StructOrUnion> Struct { get; } = KeywordParser.Create(KeywordVal.STRUCT, SyntaxTree.StructOrUnion.STRUCT);
        public static IParser<StructOrUnion> Union { get; } = KeywordParser.Create(KeywordVal.UNION, SyntaxTree.StructOrUnion.UNION);

        public static IConsumer Goto { get; } = KeywordConsumer.Create(KeywordVal.GOTO);
        public static IParser<Stmt> Continue { get; } = KeywordParser.Create(KeywordVal.CONTINUE, new ContStmt());
        public static IParser<Stmt> Break { get; } = KeywordParser.Create(KeywordVal.BREAK, new BreakStmt());
        public static IConsumer Return { get; } = KeywordConsumer.Create(KeywordVal.RETURN);

        public static IConsumer While { get; } = KeywordConsumer.Create(KeywordVal.WHILE);
        public static IConsumer Do { get; } = KeywordConsumer.Create(KeywordVal.DO);
        public static IConsumer For { get; } = KeywordConsumer.Create(KeywordVal.FOR);

        public static IConsumer If { get; } = KeywordConsumer.Create(KeywordVal.IF);
        public static IConsumer Else { get; } = KeywordConsumer.Create(KeywordVal.ELSE);
        public static IConsumer Switch { get; } = KeywordConsumer.Create(KeywordVal.SWITCH);

        public static IConsumer Case { get; } = KeywordConsumer.Create(KeywordVal.CASE);
        public static IConsumer Default { get; } = KeywordConsumer.Create(KeywordVal.DEFAULT);
    }
}
