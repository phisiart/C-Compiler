using System;

namespace ABT2.TypeSystem {
    public static partial class TypeSystemUtils {

        /// <summary>
        /// We will explain the purpose of this class with an example.
        /// 
        /// Suppose you have a <see cref="IExprType"/>, and the actual object
        /// has type <see cref="TSChar"/> (but you don't know this).
        /// 
        /// You want to create a <see cref="IQualExprType"/>, and ensure that
        /// the actual object created is <see cref="IQualExprType{TSChar}"/>.
        /// 
        /// Then you must create a <see cref="QualSChar"/>.
        /// </summary>
        public sealed class QualTypeCreator : IExprTypeVisitor<IQualExprType> {
            public QualTypeCreator(Boolean isConst, Boolean isVolatile)
                : this(new TypeQuals(isConst, isVolatile)) { }

            public QualTypeCreator(TypeQuals typeQuals) {
                this.TypeQuals = typeQuals;
            }

            public TypeQuals TypeQuals { get; }

            public IQualExprType VisitSChar(TSChar type) =>
                new QualSChar(this.TypeQuals);

            public IQualExprType VisitUChar(TUChar type) =>
                new QualUChar(this.TypeQuals);

            public IQualExprType VisitSShort(TSShort type) =>
                new QualSShort(this.TypeQuals);

            public IQualExprType VisitUShort(TUShort type) =>
                new QualUShort(this.TypeQuals);

            public IQualExprType VisitSInt(TSInt type) =>
                new QualSInt(this.TypeQuals);

            public IQualExprType VisitUInt(TUInt type) =>
                new QualUInt(this.TypeQuals);

            public IQualExprType VisitSLong(TSLong type) =>
                new QualSLong(this.TypeQuals);

            public IQualExprType VisitULong(TULong type) =>
                new QualULong(this.TypeQuals);

            public IQualExprType VisitFloat(TFloat type) =>
                new QualFloat(this.TypeQuals);

            public IQualExprType VisitDouble(TDouble type) =>
                new QualDouble(this.TypeQuals);

            public IQualExprType VisitPointer(TPointer type) =>
                new QualPointer(this.TypeQuals, type);

            public IQualExprType VisitStructOrUnion(TStructOrUnion type) =>
                new QualStructOrUnion(this.TypeQuals, type);

            public IQualExprType VisitFunction(TFunction type) {
                throw new InvalidProgramException("There is no cv-qualified function type.");
            }

            public IQualExprType VisitArray(ArrayType type) {
                var typeQuals = new TypeQuals(
                    this.TypeQuals.IsConst || type.ElemQualType.IsConst,
                    this.TypeQuals.IsVolatile || type.ElemQualType.IsVolatile
                );

                return new QualArray(
                    new ArrayType(
                        QualExprType.Create(typeQuals, type.ElemQualType.Type),
                        type.NumElems
                    )
                );
            }

            public IQualExprType VisitIncompleteArray(IncompleteArrayType type) {
                throw new InvalidProgramException("There is no cv-qualified imcomplete array type.");
            }
        }
    }
}
