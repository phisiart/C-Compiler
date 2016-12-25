using System;
using System.Text;
using ABT2.Environment;

namespace ABT2.TypeSystem {
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

        Int64 SizeOf(Env env);

        Int64 Alignment(Env env);

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

        public override String ToString() {
            var printer = new TypeSystemUtils.TypePrinter(this.TypeQuals, Env.Empty);
            this.Type.Visit(printer);
            return printer.Name;
        }

        public Boolean IsConst => this.TypeQuals.IsConst;

        public Boolean IsVolatile => this.TypeQuals.IsVolatile;

        public Int64 SizeOf(Env env) => this.Type.SizeOf(env);

        public Int64 Alignment(Env env) => this.Type.Alignment(env);

        public T Type { get; }

        public TypeQuals TypeQuals { get; }
    }
}
