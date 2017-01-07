using System;
using ABT2.TypeSystem;

namespace ABT2.Expressions {
    public interface IMult : IBinaryOperator<IArithmeticType> { }

    public interface IMult<out T> : IBinaryOperator<T>, IAdd
        where T : IArithmeticType { }

    public sealed class SIntMult : SIntBinaryOperator, IMult<TSInt> {
        public SIntMult(IRValueExpr<TSInt> left, IRValueExpr<TSInt> right)
            : base(left, right) { }
    }

    public sealed class UIntMult : UIntBinaryOperator, IMult<TUInt> {
        public UIntMult(IRValueExpr<TUInt> left, IRValueExpr<TUInt> right)
            : base(left, right) { }
    }

    public sealed class SLongMult : SLongBinaryOperator, IMult<TSLong> {
        public SLongMult(IRValueExpr<TSLong> left, IRValueExpr<TSLong> right)
            : base(left, right) { }
    }

    public sealed class ULongMult : ULongBinaryOperator, IMult<TULong> {
        public ULongMult(IRValueExpr<TULong> left, IRValueExpr<TULong> right)
            : base(left, right) { }
    }

    public sealed class FloatMult : FloatBinaryOperator, IMult<TFloat> {
        public FloatMult(IRValueExpr<TFloat> left, IRValueExpr<TFloat> right)
            : base(left, right) { }
    }

    public sealed class DoubleMult : DoubleBinaryOperator, IMult<TDouble> {
        public DoubleMult(IRValueExpr<TDouble> left, IRValueExpr<TDouble> right)
            : base(left, right) { }
    }

    public static class Mult {
        public static IMult Create(IRValueExpr<IArithmeticType> left,
                                   IRValueExpr<IArithmeticType> right) {
            // TODO: implement this
            throw new NotImplementedException();
        }
    }
}