using System;

namespace ABT2.TypeSystem {
    
    /// <summary>
    /// A type visitor without a result.
    /// </summary>
    public interface IExprTypeVisitor {
        void VisitSChar(TSChar type);

        void VisitUChar(TUChar type);

        void VisitSShort(TSShort type);

        void VisitUShort(TUShort type);

        void VisitSInt(TSInt type);

        void VisitUInt(TUInt type);

        void VisitSLong(TSLong type);

        void VisitULong(TULong type);

        void VisitFloat(TFloat type);

        void VisitDouble(TDouble type);

        void VisitPointer(TPointer type);

        void VisitStructOrUnion(TStructOrUnion type);

        void VisitFunction(TFunction type);

        void VisitArray(ArrayType type);

        void VisitIncompleteArray(IncompleteArrayType type);
    }

    /// <summary>
    /// A type visitor with a result.
    /// </summary>
    public interface IExprTypeVisitor<out R> {
        R VisitSChar(TSChar type);

        R VisitUChar(TUChar type);

        R VisitSShort(TSShort type);

        R VisitUShort(TUShort type);

        R VisitSInt(TSInt type);

        R VisitUInt(TUInt type);

        R VisitSLong(TSLong type);

        R VisitULong(TULong type);

        R VisitFloat(TFloat type);

        R VisitDouble(TDouble type);

        R VisitPointer(TPointer type);

        R VisitStructOrUnion(TStructOrUnion type);

        R VisitFunction(TFunction type);

        R VisitArray(ArrayType type);

        R VisitIncompleteArray(IncompleteArrayType type);
    }
}