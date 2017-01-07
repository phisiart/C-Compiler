using ABT2.TypeSystem;

namespace ABT2.Expressions.TypeCasts {
    public interface IUACReturn {
        IRValueExpr<IExprType> Left { get; }

        IRValueExpr<IExprType> Right { get; }
    }

    public interface IUACReturn<out T> : IUACReturn where T : class, IExprType {
        new IRValueExpr<T> Left { get; }

        new IRValueExpr<T> Right { get; }
    }

    public abstract class UACReturn<T> : IUACReturn<T> where T : class, IExprType {
        protected UACReturn(IRValueExpr<T> left, IRValueExpr<T> right) {
            this.Left = left;
            this.Right = right;
        }

        IRValueExpr<IExprType> IUACReturn.Left => this.Left;

        IRValueExpr<IExprType> IUACReturn.Right => this.Right;

        public IRValueExpr<T> Left { get; }

        public IRValueExpr<T> Right { get; }
    }

    public sealed class UACDoubleReturn : UACReturn<TDouble> {
        public UACDoubleReturn(IRValueExpr<TDouble> left, IRValueExpr<TDouble> right)
            : base(left, right) { }
    }

    public sealed class UACFloatReturn : UACReturn<TFloat> {
        public UACFloatReturn(IRValueExpr<TFloat> left, IRValueExpr<TFloat> right)
            : base(left, right) { }
    }

    public sealed class UACULongReturn : UACReturn<TULong> {
        public UACULongReturn(IRValueExpr<TULong> left, IRValueExpr<TULong> right)
            : base(left, right) { }
    }

    public sealed class UACSLongReturn : UACReturn<TLong> {
        public UACSLongReturn(IRValueExpr<TLong> left, IRValueExpr<TLong> right)
            : base(left, right) { }
    }

    public sealed class UACUIntRetrun : UACReturn<TUInt> {
        public UACUIntRetrun(IRValueExpr<TUInt> left, IRValueExpr<TUInt> right)
            : base(left, right) { }
    }

    public sealed class UACSIntReturn : UACReturn<TInt> {
        public UACSIntReturn(IRValueExpr<TInt> left, IRValueExpr<TInt> right)
            : base(left, right) { }
    }

    public static class UsualArithmeticConversion {
        public static IUACReturn Perform(
            IRValueExpr<IArithmeticType> left,
            IRValueExpr<IArithmeticType> right) {

            var leftType = left.Type;
            var rightType = right.Type;

            if (leftType is TDouble || rightType is TDouble) {
                return new UACDoubleReturn(
                    left.CastTo(TDouble.Get),
                    right.CastTo(TDouble.Get)
                );
            }

            if (leftType is TFloat || rightType is TFloat) {
                return new UACFloatReturn(
                    left.CastTo(TFloat.Get),
                    right.CastTo(TFloat.Get)
                );
            }

            if (leftType is TULong || rightType is TULong) {
                return new UACULongReturn(
                    left.CastTo(TULong.Get),
                    right.CastTo(TULong.Get)
                );
            }

            if (leftType is TSLong || rightType is TSLong) {
                return new UACSLongReturn(
                    left.CastTo(TSLong.Get),
                    right.CastTo(TSLong.Get)
                );
            }

            if (leftType is TUInt || rightType is TUInt) {
                return new UACUIntRetrun(
                    left.CastTo(TUInt.Get),
                    right.CastTo(TUInt.Get)
                );
            }

            return new UACSIntReturn(
                left.CastTo(TSInt.Get),
                right.CastTo(TSInt.Get)
            );
        }
    }
}
