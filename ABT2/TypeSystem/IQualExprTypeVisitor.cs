using System;

namespace ABT2.TypeSystem {
    public interface IQualExprTypeVisitor<out R> {
        R VisitSChar(QualSChar qualType);

        R VisitUChar(QualUChar qualType);

        R VisitSShort(QualSShort qualType);

        R VisitUShort(QualUShort qualType);

        R VisitSInt(QualSInt qualType);

        R VisitUInt(QualUInt qualType);

        R VisitSLong(QualSLong qualType);

        R VisitULong(QualULong qualType);

        R VisitFloat(QualFloat qualType);

        R VisitDouble(QualDouble qualType);

        R VisitPointer(QualPointer qualType);

        R VisitStructOrUnion(QualStructOrUnion qualType);

        R VisitFunction(QualFunction qualType);

        R VisitArray(QualArray qualType);
    }
}