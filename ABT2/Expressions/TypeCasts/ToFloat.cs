using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions.TypeCasts {

    public abstract class ArithmeticToFloatCastExpr<FromType>
        : TypeCastExpr<TFloat, FromType>
        where FromType : class, IArithmeticType {

        protected ArithmeticToFloatCastExpr(IRValueExpr<FromType> fromExpr) {
            this.FromExpr = fromExpr;
        }

        public IRValueExpr<FromType> FromExpr { get; }

        public override Env Env => this.FromExpr.Env;

        public override TFloat Type => TFloat.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitFloat(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitFloat(this);
        }
    }

    public sealed class SLongToFloatCast
        : ArithmeticToFloatCastExpr<TSLong> {
        public SLongToFloatCast(IRValueExpr<TSLong> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ULongToFloatCast
        : ArithmeticToFloatCastExpr<TULong> {
        public ULongToFloatCast(IRValueExpr<TULong> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class DoubleToFloatCast
        : ArithmeticToFloatCastExpr<TDouble> {
        public DoubleToFloatCast(IRValueExpr<TDouble> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ToFloatCreator : ToFloatingPointCreator<TFloat> {
        private ToFloatCreator() { }

        public static ToFloatCreator Get = new ToFloatCreator();

        public override TFloat ToType => TFloat.Get;

        public override IRValueExpr<TFloat> VisitConstDouble(ConstDouble expr) {
            return new ConstFloat(expr.Value, expr.Env);
        }

        public override IRValueExpr<TFloat> VisitConstFloat(ConstFloat expr) {
            return expr;
        }

        public override IRValueExpr<TFloat> VisitConstSLong(ConstSLong expr) {
            return new ConstFloat(expr.Value, expr.Env);
        }

        public override IRValueExpr<TFloat> VisitConstULong(ConstULong expr) {
            return new ConstFloat(expr.Value, expr.Env);
        }

        public override IRValueExpr<TFloat> VisitDouble(IRValueExpr<TDouble> expr) {
            return new DoubleToFloatCast(expr);
        }

        public override IRValueExpr<TFloat> VisitFloat(IRValueExpr<TFloat> expr) {
            return expr;
        }

        public override IRValueExpr<TFloat> VisitSLong(IRValueExpr<TSLong> expr) {
            return new SLongToFloatCast(expr);
        }

        public override IRValueExpr<TFloat> VisitULong(IRValueExpr<TULong> expr) {
            return new ULongToFloatCast(expr);
        }
    }
}