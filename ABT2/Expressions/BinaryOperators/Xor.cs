using System;
using ABT2.TypeSystem;
using ABT2.Expressions.TypeCasts;

namespace ABT2.Expressions {
    public interface IXor : IBinaryOperator { }

    public interface IXor<out T> : IBinaryOperator<T>, IXor
        where T : class, IIntegralType { }

    public sealed class SIntXor : SIntBinaryOperator, IXor<TSInt> {
        public SIntXor(IRValueExpr<TSInt> left, IRValueExpr<TSInt> right)
            : base(left, right) { }
    }

    public sealed class UIntXor : UIntBinaryOperator, IXor<TUInt> {
        public UIntXor(IRValueExpr<TUInt> left, IRValueExpr<TUInt> right)
            : base(left, right) { }
    }

    public sealed class SLongXor : SLongBinaryOperator, IXor<TSLong> {
        public SLongXor(IRValueExpr<TSLong> left, IRValueExpr<TSLong> right)
            : base(left, right) { }
    }

    public sealed class ULongXor : ULongBinaryOperator, IXor<TULong> {
        public ULongXor(IRValueExpr<TULong> left, IRValueExpr<TULong> right)
            : base(left, right) { }
    }

    public static class Xor {
        public static IXor Create(IRValueExpr<IIntegralType> left,
                                  IRValueExpr<IIntegralType> right) {
            return UsualArithmeticConversion.Perform(left, right)
                                            .Visit(XorCreator.Get);
        }

        private sealed class XorCreator : IUACIntegralReturnVisitor<IXor> {
            private XorCreator() { }

            public static XorCreator Get { get; } = new XorCreator();

            public IXor VisitSInt(IRValueExpr<TSInt> left, IRValueExpr<TSInt> right) {
                return new SIntXor(left, right);
            }

            public IXor VisitSLong(IRValueExpr<TSLong> left, IRValueExpr<TSLong> right) {
                return new SLongXor(left, right);
            }

            public IXor VisitUInt(IRValueExpr<TUInt> left, IRValueExpr<TUInt> right) {
                return new UIntXor(left, right);
            }

            public IXor VisitULong(IRValueExpr<TULong> left, IRValueExpr<TULong> right) {
                return new ULongXor(left, right);
            }
        }
    }
}