using System;

namespace ABT2.TypeSystem {
    using IQualExprType = IQualExprType<IExprType>;

    public static partial class TypeSystemUtils {

        public static Boolean TypesAreEqual(IExprType type1, IExprType type2) {
            var comparer = new TypeEqualityComparer(type1);
            return type2.Visit(comparer);
        }

        public static Boolean TypesAreEqual(IQualExprType type1, IQualExprType type2) {
            return TypeQuals.Equals(type1.TypeQuals, type2.TypeQuals)
                && TypesAreEqual(type1.Type, type2.Type);
        }

        public static IQualExprType<T> EmptyQual<T>(T type) where T : IExprType {
            var creator = new QualTypeCreator(isConst: false, isVolatile: false);
            return (IQualExprType<T>)type.Visit(creator);
        }

        public static IQualExprType<T> Const<T>(T type) where T : IExprType {
            var creator = new QualTypeCreator(isConst: true, isVolatile: false);
            return (IQualExprType<T>)type.Visit(creator);
        }

        public static IQualExprType<T> Volatile<T>(T type) where T : IExprType {
            var creator = new QualTypeCreator(isConst: false, isVolatile: true);
            return (IQualExprType<T>)type.Visit(creator);
        }

        public static IQualExprType<T> ConstVolatile<T>(T type) where T : IExprType {
            var creator = new QualTypeCreator(isConst: true, isVolatile: true);
            return (IQualExprType<T>)type.Visit(creator);
        }
    }
}
