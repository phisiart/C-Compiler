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
                this.IsConst = isConst;
                this.IsVolatile = isVolatile;
            }

            public IQualExprType VisitSignedChar(TSChar type) {
                return new QualExprType<TSChar>(this.IsConst, this.IsVolatile, type);
            }

            public IQualExprType VisitUnsignedChar(TUChar type) {
                return new QualExprType<TUChar>(this.IsConst, this.IsVolatile, type);
            }

            public IQualExprType VisitSignedShort(TSShort type) {
                return new QualExprType<TSShort>(this.IsConst, this.IsVolatile, type);
            }

            public IQualExprType VisitUnsignedShort(TUShort type) {
                return new QualExprType<TUShort>(this.IsConst, this.IsVolatile, type);
            }

            public IQualExprType VisitSignedInt(TSInt type) {
                return new QualExprType<TSInt>(this.IsConst, this.IsVolatile, type);
            }

            public IQualExprType VisitUnsignedInt(TUInt type) {
                return new QualExprType<TUInt>(this.IsConst, this.IsVolatile, type);
            }

            public IQualExprType VisitSignedLong(TSLong type) {
                return new QualExprType<TSLong>(this.IsConst, this.IsVolatile, type);
            }

            public IQualExprType VisitUnsignedLong(TULong type) {
                return new QualExprType<TULong>(this.IsConst, this.IsVolatile, type);
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

            public Boolean IsConst { get; }

            public Boolean IsVolatile { get; }
        }
    }
}
