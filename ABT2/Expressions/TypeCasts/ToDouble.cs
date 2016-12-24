using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions.TypeCasts {

    public abstract class ArithmeticToDoubleCastExpr<FromType>
        : TypeCastExpr<TDouble, FromType>
        where FromType : IArithmeticType {

        protected ArithmeticToDoubleCastExpr(IRValueExpr<FromType> fromExpr) {
            this.FromExpr = fromExpr;
        }

        public IRValueExpr<FromType> FromExpr { get; }

        public override Env Env => this.FromExpr.Env;

        public override TDouble Type => TDouble.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitDouble(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitDouble(this);
        }
    }

    public sealed class SLongToDoubleCast
        : ArithmeticToDoubleCastExpr<TSLong> {
        public SLongToDoubleCast(IRValueExpr<TSLong> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ULongToDoubleCast
        : ArithmeticToDoubleCastExpr<TULong> {
        public ULongToDoubleCast(IRValueExpr<TULong> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class FloatToDoubleCast
        : ArithmeticToDoubleCastExpr<TFloat> {
        public FloatToDoubleCast(IRValueExpr<TFloat> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ToDoubleCreator : ToFloatingPointCreator<TDouble> {
        private ToDoubleCreator() { }

        public static ToDoubleCreator Get = new ToDoubleCreator();

        public override TDouble ToType => TDouble.Get;

        public override IRValueExpr<TDouble> VisitConstDouble(ConstDouble expr) {
            return expr;
        }

        public override IRValueExpr<TDouble> VisitConstFloat(ConstFloat expr) {
            return new ConstDouble(expr.Value, expr.Env);
        }

        public override IRValueExpr<TDouble> VisitConstSLong(ConstSLong expr) {
            return new ConstDouble(expr.Value, expr.Env);
        }

        public override IRValueExpr<TDouble> VisitConstULong(ConstULong expr) {
            return new ConstDouble(expr.Value, expr.Env);
        }

        public override IRValueExpr<TDouble> VisitDouble(IRValueExpr<TDouble> expr) {
            return expr;
        }

        public override IRValueExpr<TDouble> VisitFloat(IRValueExpr<TFloat> expr) {
            return new FloatToDoubleCast(expr);
        }

        public override IRValueExpr<TDouble> VisitSLong(IRValueExpr<TSLong> expr) {
            return new SLongToDoubleCast(expr);
        }

        public override IRValueExpr<TDouble> VisitULong(IRValueExpr<TULong> expr) {
            return new ULongToDoubleCast(expr);
        }
    }
}