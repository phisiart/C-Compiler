using System;

namespace ABT2.TypeSystem {
    public interface IFloatingPointType : IArithmeticType { }

    public sealed class TFloat : IFloatingPointType {
        private TFloat() { } // singleton

        public void Visit(IExprTypeVisitor visitor) {
            visitor.VisitFloat(this);
        }

        public R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitFloat(this);
        }

        public Int64 SizeOf => PlatformSpecificConstants.SizeOfFloat;

        public Int64 Alignment => PlatformSpecificConstants.AlignmentOfFloat;

        public static TFloat Get { get; } = new TFloat();
    }

    public sealed class TDouble : IFloatingPointType {
        private TDouble() { } // singleton

        public void Visit(IExprTypeVisitor visitor) {
            visitor.VisitDouble(this);
        }

        public R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitDouble(this);
        }

        public Int64 SizeOf => PlatformSpecificConstants.SizeOfDouble;

        public Int64 Alignment => PlatformSpecificConstants.AlignmentOfDouble;

        public static TDouble Get { get; } = new TDouble();
    }
}
