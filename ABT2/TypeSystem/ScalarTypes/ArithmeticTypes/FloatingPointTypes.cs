using ABT2.Environment;
using System;

namespace ABT2.TypeSystem {
    public interface IFloatingPointType : IArithmeticType { }

    /// <summary>
    /// The float type.
    /// </summary>
    public sealed class TFloat : IFloatingPointType {
        private TFloat() { } // singleton

        public void Visit(IExprTypeVisitor visitor) {
            visitor.VisitFloat(this);
        }

        public R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitFloat(this);
        }

        public Int64 SizeOf(Env env) => PlatformSpecificConstants.SizeOfFloat;

        public Int64 Alignment(Env env) => PlatformSpecificConstants.AlignmentOfFloat;

        public static TFloat Get { get; } = new TFloat();
    }

    /// <summary>
    /// A cv-qualified float type.
    /// </summary>
    public sealed class QualFloat : QualExprType<TFloat> {
        public QualFloat(TypeQuals typeQuals)
            : base(typeQuals) { }

        public override TFloat Type => TFloat.Get;
    }

    /// <summary>
    /// The double type.
    /// </summary>
    public sealed class TDouble : IFloatingPointType {
        private TDouble() { } // singleton

        public void Visit(IExprTypeVisitor visitor) {
            visitor.VisitDouble(this);
        }

        public R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitDouble(this);
        }

        public Int64 SizeOf(Env env) => PlatformSpecificConstants.SizeOfDouble;

        public Int64 Alignment(Env env) => PlatformSpecificConstants.AlignmentOfDouble;

        public static TDouble Get { get; } = new TDouble();
    }

    /// <summary>
    /// A cv-qualified double type.
    /// </summary>
    public sealed class QualDouble : QualExprType<TDouble> {
        public QualDouble(TypeQuals typeQuals)
            : base(typeQuals) { }

        public override TDouble Type => TDouble.Get;
    }
}
