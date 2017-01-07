using System;
using System.Text;
using ABT2.Environment;

namespace ABT2.TypeSystem {
    public sealed class TypeQuals {
        public TypeQuals(Boolean isConst, Boolean isVolatile) {
            this.IsConst = isConst;
            this.IsVolatile = isVolatile;
        }

        public override String ToString() {
            var builder = new StringBuilder();

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

    public interface IQualExprType {
        Boolean IsConst { get; }

        Boolean IsVolatile { get; }

        Int64 SizeOf(Env env);

        Int64 Alignment(Env env);

        IExprType Type { get; }

        TypeQuals TypeQuals { get; }
    }

    /// <summary>
    /// A qualified type.
    /// </summary>
    /// <remarks>
    /// In C, each type can have <c>const</c> or <c>volatile</c> qualifiers.
    /// 
    /// This interface is intended for covariance.
    /// 
    /// To create a qualified type, you should use
    /// <see cref="TypeSystemUtils.QualTypeCreator"/>.
    /// </remarks>
    public interface IQualExprType<out T> : IQualExprType where T : IExprType {
        new T Type { get; }
    }

    public abstract class QualExprType<T> : IQualExprType<T> where T : IExprType {
        protected QualExprType(Boolean isConst, Boolean isVolatile)
            : this(new TypeQuals(isConst, isVolatile)) { }

        protected QualExprType(TypeQuals typeQuals) {
            this.TypeQuals = typeQuals;
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

        public abstract T Type { get; }

        public TypeQuals TypeQuals { get; }

        IExprType IQualExprType.Type => this.Type;
    }

    public static class QualExprType {
        public static IQualExprType<T> Create<T>(TypeQuals typeQuals, T type) where T : IExprType {
            var creator = new TypeSystemUtils.QualTypeCreator(typeQuals);
            var qualType = type.Visit(creator);
            return (IQualExprType<T>)qualType;
        }
    }
}
