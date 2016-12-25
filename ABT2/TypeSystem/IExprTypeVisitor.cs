using System;

namespace ABT2.TypeSystem {
    
    /// <summary>
    /// A type visitor without a result.
    /// </summary>
    public interface IExprTypeVisitor {
        void VisitSignedChar(TSChar type);

        void VisitUnsignedChar(TUChar type);

        void VisitSignedShort(TSShort type);

        void VisitUnsignedShort(TUShort type);

        void VisitSignedInt(TSInt type);

        void VisitUnsignedInt(TUInt type);

        void VisitSignedLong(TSLong type);

        void VisitUnsignedLong(TULong type);

        void VisitFloat(TFloat type);

        void VisitDouble(TDouble type);

        void VisitPointer(TPointer type);

        void VisitStructOrUnion(StructOrUnionType type);

        void VisitFunction(FunctionType type);

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

        R VisitStructOrUnion(StructOrUnionType type);

        R VisitFunction(FunctionType type);

        R VisitArray(ArrayType type);

        R VisitIncompleteArray(IncompleteArrayType type);
    }
}