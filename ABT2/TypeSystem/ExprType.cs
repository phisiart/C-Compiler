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

        Int64 SizeOf(Env env);

        Int64 Alignment(Env env);
    }
}
