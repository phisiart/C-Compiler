using System;
namespace ABT2.TypeSystem {
    using IQualExprType = IQualExprType<IExprType>;

    public static partial class TypeSystemUtils {

        /// <summary>
        /// The purpose of this class is as follows:
        /// Suppose you have a IExprType, and the actual object type is SignedCharType.
        /// You want to create a IQualExprType<IExprType>.
        /// Notice that IQualExprType is covariant, so IQualExprType<IExprType> could be IQualExprType<SignecCharType>.
        /// However, QualExprType is a class, which cannot be covariant. So you must explicitly create a QualExprType<SignedCharType>.
        /// </summary>
        public class QualTypeCreator : IExprTypeVisitor<IQualExprType> {
            public QualTypeCreator(Boolean isConst, Boolean isVolatile) {
                this.TypeQuals = new TypeQuals(isConst, isVolatile);
            }

            public IQualExprType VisitSChar(TSChar type) {
                return new QualSChar(this.TypeQuals, type);
            }

            public IQualExprType VisitUChar(TUChar type) {
                return new QualUChar(this.TypeQuals, type);
            }

            public IQualExprType VisitSShort(TSShort type) {
                return new QualSShort(this.TypeQuals, type);
            }

            public IQualExprType VisitUShort(TUShort type) {
                return new QualUShort(this.TypeQuals, type);
            }

            public IQualExprType VisitSInt(TSInt type) {
                return new QualSInt(this.TypeQuals, type);
            }

            public IQualExprType VisitUInt(TUInt type) {
                return new QualUInt(this.TypeQuals, type);
            }

            public IQualExprType VisitSLong(TSLong type) {
                return new QualSLong(this.TypeQuals, type);
            }

            public IQualExprType VisitULong(TULong type) {
                return new QualULong(this.TypeQuals, type);
            }

            public IQualExprType VisitFloat(TFloat type) {
                return new QualExprType<TFloat>(this.IsConst, this.IsVolatile, type);
            }

            public IQualExprType VisitDouble(TDouble type) {
                return new QualExprType<TDouble>(this.IsConst, this.IsVolatile, type);
            }

            public IQualExprType VisitPointer(TPointer type) {
                return new QualExprType<TPointer>(this.IsConst, this.IsVolatile, type);
            }

            public IQualExprType VisitStructOrUnion(StructOrUnionType type) {
                return new QualExprType<StructOrUnionType>(this.IsConst, this.IsVolatile, type);
            }

            public IQualExprType VisitFunction(FunctionType type) {
                return new QualExprType<FunctionType>(this.IsConst, this.IsVolatile, type);
            }

            public IQualExprType VisitArray(ArrayType type) {
                return new QualExprType<ArrayType>(this.IsConst, this.IsVolatile, type);
            }

            public IQualExprType VisitIncompleteArray(IncompleteArrayType type) {
                return new QualExprType<IncompleteArrayType>(this.IsConst, this.IsVolatile, type);
            }

            public TypeQuals TypeQuals { get; }
        }
    }
}
