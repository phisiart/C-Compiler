using System;

namespace ABT2.TypeSystem {
    using IQualExprType = IQualExprType<IExprType>;

    public static partial class TypeSystemUtils {
        /// <summary>
        /// Type equality comparer.
        /// Compares whether two types are equal.
        /// </summary>
        public sealed class TypeEqualityComparer : IExprTypeVisitor<Boolean> {
            public TypeEqualityComparer(IExprType type) {
                this.OtherType = type;
            }

            public Boolean VisitArithmeticType(IArithmeticType type) {
                // All arithmetic types are singletons
                return this.OtherType == type;
            }

            public Boolean VisitSChar(TSChar type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitUChar(TUChar type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitSShort(TSShort type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitUShort(TUShort type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitSInt(TSInt type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitUInt(TUInt type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitSLong(TSLong type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitULong(TULong type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitFloat(TFloat type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitDouble(TDouble type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitPointer(TPointer type) {
                if (!(this.OtherType is TPointer)) {
                    return false;
                }

                var otherType = (TPointer)this.OtherType;
                return TypesAreEqual(type.ElemQualType, otherType.ElemQualType);
            }

            public Boolean VisitStructOrUnion(TStructOrUnion type) {
                if (!(this.OtherType is TStructOrUnion)) {
                    return false;
                }

                var otherType = (TStructOrUnion)this.OtherType;
                return otherType.TypeID == type.TypeID;
            }

            public Boolean VisitFunction(TFunction type) {
                if (!(this.OtherType is TFunction)) {
                    return false;
                }

                var otherType = (TFunction)this.OtherType;

                if (!TypesAreEqual(otherType.ReturnQualType, type.ReturnQualType)) {
                    return false;
                }

                if (type.Args.Count != otherType.Args.Count) {
                    return false;
                }

                for (int i = 0; i < type.Args.Count; ++i) {
                    if (!TypesAreEqual(type.Args[i].QualType, otherType.Args[i].QualType)) {
                        return false;
                    }

                    if (type.Args[i].Offset != otherType.Args[i].Offset) {
                        return false;
                    }
                }

                if (type.HasVarArgs != otherType.HasVarArgs) {
                    return false;
                }

                return true;
            }

            public Boolean VisitArray(ArrayType type) {
                if (!(this.OtherType is ArrayType)) {
                    return false;
                }

                var otherType = (ArrayType)this.OtherType;

                if (!TypesAreEqual(type.ElemQualType, otherType.ElemQualType)) {
                    return false;
                }

                if (type.NumElems != otherType.NumElems) {
                    return false;
                }

                return true;
            }

            public Boolean VisitIncompleteArray(IncompleteArrayType type) {
                if (!(this.OtherType is IncompleteArrayType)) {
                    return false;
                }

                var otherType = (IncompleteArrayType)this.OtherType;

                if (!TypesAreEqual(type.ElemQualType, otherType.ElemQualType)) {
                    return false;
                }

                return true;
            }

            public IExprType OtherType { get; }
        }
    }
}