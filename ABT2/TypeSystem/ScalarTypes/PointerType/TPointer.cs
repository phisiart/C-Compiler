using System;
using ABT2.Environment;

namespace ABT2.TypeSystem {
    using IQualExprType = IQualExprType<IExprType>;

    public sealed class TPointer : IScalarType {
        public TPointer(IQualExprType elemQualType) {
            this.ElemQualType = elemQualType;
        }

        public void Visit(IExprTypeVisitor visitor) {
            visitor.VisitPointer(this);
        }

        public R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitPointer(this);
        }

        public IQualExprType ElemQualType { get; }

        public Int64 SizeOf(Env env) => PlatformSpecificConstants.SizeOfLong;

        public Int64 Alignment(Env env) => PlatformSpecificConstants.AlignmentOfLong;
    }
}
