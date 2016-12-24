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

            public Boolean VisitSignedChar(TSChar type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitUnsignedChar(TUChar type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitSignedShort(TSShort type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitUnsignedShort(TUShort type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitSignedInt(TSInt type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitUnsignedInt(TUInt type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitSignedLong(TSLong type) {
                return VisitArithmeticType(type);
            }

            public Boolean VisitUnsignedLong(TULong type) {
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

            public Boolean VisitStructOrUnion(StructOrUnionType type) {
                if (!(this.OtherType is StructOrUnionType)) {
                    return false;
                }

                var otherType = (StructOrUnionType)this.OtherType;
                return otherType.TypeID == type.TypeID;
            }

            public Boolean VisitFunction(FunctionType type) {
                if (!(this.OtherType is FunctionType)) {
                    return false;
                }

                var otherType = (FunctionType)this.OtherType;

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