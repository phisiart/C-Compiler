using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ABT2.TypeSystem;
using ABT2.Environment;

namespace ABT2.Expressions {
    public interface IRValueExpr<out T> where T : IExprType {

        // Type qualifiers for rvalues are unnecessary
        T Type { get; }

        Env Env { get; }

        void Visit(IRValueExprByTypeVisitor visitor);

        R Visit<R>(IRValueExprByTypeVisitor<R> visitor);
    }

    public interface IRValueExprByTypeVisitor {
        void VisitSChar(IRValueExpr<TSChar> expr);
        void VisitConstSChar(ConstSChar expr);

        void VisitUChar(IRValueExpr<TUChar> expr);
        void VisitConstUChar(ConstUChar expr);

        void VisitSShort(IRValueExpr<TSShort> expr);
        void VisitConstSShort(ConstSShort expr);

        void VisitUShort(IRValueExpr<TUShort> expr);
        void VisitConstUShort(ConstUShort expr);

        void VisitSInt(IRValueExpr<TSInt> expr);
        void VisitConstSInt(ConstSInt expr);

        void VisitUInt(IRValueExpr<TUInt> expr);
        void VisitConstUInt(ConstUInt expr);

        void VisitSLong(IRValueExpr<TSLong> expr);
        void VisitConstSLong(ConstSLong expr);

        void VisitULong(IRValueExpr<TULong> expr);
        void VisitConstULong(ConstULong expr);

        void VisitFloat(IRValueExpr<TFloat> expr);
        void VisitConstFloat(ConstFloat expr);

        void VisitDouble(IRValueExpr<TDouble> expr);
        void VisitConstDouble(ConstDouble expr);

        void VisitPointer(IRValueExpr<TPointer> expr);

        void VisitStructOrUnion(IRValueExpr<StructOrUnionType> expr);

        void VisitFunction(IRValueExpr<FunctionType> expr);

        void VisitArray(IRValueExpr<ArrayType> expr);
    }

    public interface IRValueExprByTypeVisitor<out R> {
        R VisitSChar(IRValueExpr<TSChar> expr);
        R VisitConstSChar(ConstSChar expr);

        R VisitUChar(IRValueExpr<TUChar> expr);
        R VisitConstUChar(ConstUChar expr);

        R VisitSShort(IRValueExpr<TSShort> expr);
        R VisitConstSShort(ConstSShort expr);

        R VisitUShort(IRValueExpr<TUShort> expr);
        R VisitConstUShort(ConstUShort expr);

        R VisitSInt(IRValueExpr<TSInt> expr);
        R VisitConstSInt(ConstSInt expr);

        R VisitUInt(IRValueExpr<TUInt> expr);
        R VisitConstUInt(ConstUInt expr);

        R VisitSLong(IRValueExpr<TSLong> expr);
        R VisitConstSLong(ConstSLong expr);

        R VisitULong(IRValueExpr<TULong> expr);
        R VisitConstULong(ConstULong expr);

        R VisitFloat(IRValueExpr<TFloat> expr);
        R VisitConstFloat(ConstFloat expr);

        R VisitDouble(IRValueExpr<TDouble> expr);
        R VisitConstDouble(ConstDouble expr);

        R VisitPointer(IRValueExpr<TPointer> expr);

        R VisitStructOrUnion(IRValueExpr<StructOrUnionType> expr);

        R VisitFunction(IRValueExpr<FunctionType> expr);

        R VisitArray(IRValueExpr<ArrayType> expr);
    }

    public interface ILValueExpr<out T> : IRValueExpr<T> where T : IExprType {
        IQualExprType<T> QualType { get; }
    }
}