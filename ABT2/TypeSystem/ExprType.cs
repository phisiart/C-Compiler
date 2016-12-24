using System;
using System.Text;
using ABT2.Environment;

namespace ABT2.TypeSystem {
    public static class PlatformSpecificConstants {
        public const Int64 SizeOfChar = 1;
        public const Int64 AlignmentOfChar = 1;

        public const Int64 SizeOfShort = 2;
        public const Int64 AlignmentOfShort = 2;

        public const Int64 SizeOfInt = 4;
        public const Int64 AlignmentOfInt = 4;

        public const Int64 SizeOfLong = 8;
        public const Int64 AlignmentOfLong = 8;

        public const Int64 SizeOfFloat = 4;
        public const Int64 AlignmentOfFloat = 4;

        public const Int64 SizeOfDouble = 8;
        public const Int64 AlignmentOfDouble = 4;
    }

    public interface IExprType {
        void Visit(IExprTypeVisitor visitor);

        R Visit<R>(IExprTypeVisitor<R> visitor);

        Int64 SizeOf { get; }

        Int64 Alignment { get; }
    }

    /// <summary>
    /// A qualified type.
    /// </summary>
    /// <remarks>
    /// In C, each type can have `const` or `volatile` qualifiers.
    /// This interface is intended for covariance.
    /// To create a qualified type, it is recommended to use <see cref="TypeSystemUtils."/>
    /// </remarks>
    public interface IQualExprType<out T> where T : IExprType {
        Boolean IsConst { get; }

        Boolean IsVolatile { get; }

        Int64 SizeOf { get; }

        Int64 Alignment { get; }

        T Type { get; }

        TypeQuals TypeQuals { get; }
    }

    public sealed class TypeQuals {
        public TypeQuals(Boolean isConst, Boolean isVolatile) {
            this.IsConst = isConst;
            this.IsVolatile = isVolatile;
        }

        public override String ToString() {
            StringBuilder builder = new StringBuilder();

            if (this.IsConst) {
                builder.Append("const");
            }

            if (this.IsVolatile) {
                if (builder.Length == 0) {
                    builder.Append("volatile");
                } else {
                    builder.Append(" volatile");
                }
            }

            return builder.ToString();
        }

        public static Boolean Equals(TypeQuals quals1, TypeQuals quals2) {
            return (quals1.IsConst == quals2.IsConst)
                && (quals1.IsVolatile == quals2.IsVolatile);
        }

        public Boolean IsConst { get; }

        public Boolean IsVolatile { get; }
    }

    public class QualExprType<T> : IQualExprType<T> where T : IExprType {
        public QualExprType(Boolean isConst, Boolean isVolatile, T type)
            : this(new TypeQuals(isConst, isVolatile), type) { }

        public QualExprType(TypeQuals typeQuals, T type) {
            this.TypeQuals = typeQuals;
            this.Type = type;
        }

        public override string ToString() {
            var printer = new TypeSystemUtils.TypePrinter(this.TypeQuals, Env.Empty);
            this.Type.Visit(printer);
            return printer.Name;
        }

        public Boolean IsConst => this.TypeQuals.IsConst;

        public Boolean IsVolatile => this.TypeQuals.IsVolatile;

        public Int64 SizeOf => this.Type.SizeOf;

        public Int64 Alignment => this.Type.Alignment;

        public T Type { get; }

        public TypeQuals TypeQuals { get; }
    }
}
