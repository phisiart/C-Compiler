using ABT2.TypeSystem;
using ABT2.Expressions.TypeCasts;

namespace ABT2.Expressions {
    public interface IMult : IBinaryOperator { }

    public interface IMult<out T> : IBinaryOperator<T>, IMult
        where T : class, IArithmeticType { }

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

            return UsualArithmeticConversion.Perform(left, right)
                                            .Visit(MultCreator.Get);
        }

        private class MultCreator : IUACReturnVisitor<IMult> {
            public static MultCreator Get { get; } = new MultCreator();

            public IMult VisitDouble(IRValueExpr<TDouble> left,
                                     IRValueExpr<TDouble> right) {
                return new DoubleMult(left, right);
            }

            public IMult VisitFloat(IRValueExpr<TFloat> left,
                                    IRValueExpr<TFloat> right) {
                return new FloatMult(left, right);
            }

            public IMult VisitSInt(IRValueExpr<TSInt> left,
                                   IRValueExpr<TSInt> right) {
                return new SIntMult(left, right);
            }

            public IMult VisitSLong(IRValueExpr<TSLong> left,
                                    IRValueExpr<TSLong> right) {
                return new SLongMult(left, right);
            }

            public IMult VisitUInt(IRValueExpr<TUInt> left,
                                   IRValueExpr<TUInt> right) {
                return new UIntMult(left, right);
            }

            public IMult VisitULong(IRValueExpr<TULong> left,
                                    IRValueExpr<TULong> right) {
                return new ULongMult(left, right);
            }
        }
    }
}