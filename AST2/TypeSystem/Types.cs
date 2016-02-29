using System;

namespace AST2 {
    /// <summary>
    /// Unqualified type
    /// </summary>
    public interface IType {
        UInt32 SizeOf { get; }

        UInt32 Alignment { get; }

        /// <summary>
        /// Can be assigned to without (explicit or implicit) type cast.
        /// </summary>
        Boolean CanBeAssignedTo(IType type);
    }

    public interface IQualifiedType<out T> where T : IType {
        T UnqualifiedType { get; }

        Boolean IsConst { get; }

        Boolean IsVolatile { get; }

        Boolean IsRestricted { get; }

        UInt32 SizeOf { get; }

        UInt32 Alignment { get; }

        Boolean CanBeAssignedTo(IQualifiedType<IType> valueType);
    }

    public class QualifiedType<T> : IQualifiedType<T> where T : IType {
        public QualifiedType(T unqualifiedType, Boolean isConst, Boolean isVolatile, Boolean isRestricted) {
            this.UnqualifiedType = unqualifiedType;
            this.IsConst = isConst;
            this.IsVolatile = isVolatile;
            this.IsRestricted = isRestricted;
        } 

        public T UnqualifiedType { get; }

        public Boolean IsConst { get; }

        public Boolean IsVolatile { get; }

        public Boolean IsRestricted { get; }

        public UInt32 SizeOf => this.UnqualifiedType.SizeOf;

        public UInt32 Alignment => this.UnqualifiedType.Alignment;

        public Boolean CanBeAssignedTo(IQualifiedType<IType> valueType) {
            if (this.IsConst) {
                throw new InvalidOperationException("Cannot assign to const.");
            }
            return this.UnqualifiedType.CanBeAssignedTo(valueType.UnqualifiedType);
        }
    }

    public class QualifiedType {
        public static QualifiedType<T> Const<T>(T type) where T : IType =>
            new QualifiedType<T>(type, isConst: true, isVolatile: false, isRestricted: false);
    }

    public interface IPointerType<out T> : IScalarType where T : IType {
        IQualifiedType<T> PointeeType { get; }
    }

    public class PointerType<T> : IPointerType<T> where T : IType {
        public PointerType(IQualifiedType<T> pointeeType) {
            this.PointeeType = pointeeType;
        }

        public IQualifiedType<T> PointeeType { get; }

        public UInt32 SizeOf => 4;

        public UInt32 Alignment => 4;

        public Boolean CanBeAssignedTo(IType type) {
            var pointerType = type as IPointerType<IType>;
            return pointerType != null
                   && this.PointeeType.UnqualifiedType.CanBeAssignedTo(pointerType.PointeeType.UnqualifiedType);
        }
    }

    public interface IScalarType : IType { }

    public interface IArithmeticType : IScalarType { }

    public abstract class ArithmeticType : IArithmeticType {
        public abstract UInt32 SizeOf { get; }

        public abstract UInt32 Alignment { get; }

        public Boolean CanBeAssignedTo(IType type) => this == type;
    }
    
    public class DoubleType : ArithmeticType {
        private DoubleType() { }

        public static DoubleType Instance = new DoubleType();

        public override UInt32 SizeOf => 8;

        public override UInt32 Alignment => 4;
    }

    public class FloatType : ArithmeticType {
        private FloatType() { }

        public static FloatType Instance = new FloatType();

        public override UInt32 SizeOf => 4;

        public override UInt32 Alignment => 4;
    }
}
