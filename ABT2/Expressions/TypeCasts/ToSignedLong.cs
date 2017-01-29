using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions.TypeCasts {

    public abstract class IntegralToSLongCastExpr<FromType>
        : TypeCastExpr<TSLong, FromType>
        where FromType : class, IIntegralType {

        protected IntegralToSLongCastExpr(IRValueExpr<FromType> fromExpr) {
            this.FromExpr = fromExpr;
        }

        public IRValueExpr<FromType> FromExpr { get; }

        public override Env Env => this.FromExpr.Env;

        public override TSLong Type => TSLong.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSLong(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSLong(this);
        }
    }

    public sealed class FloatToSLongCast : TypeCastExpr<TSLong, TFloat> {
        public FloatToSLongCast(IRValueExpr<TFloat> fromExpr) {
            this.FromExpr = fromExpr;
        }

        public IRValueExpr<TFloat> FromExpr { get; }

        public override Env Env => this.FromExpr.Env;

        public override TSLong Type => TSLong.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSLong(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSLong(this);
        }
    }

    public sealed class DoubleToSLongCast : TypeCastExpr<TSLong, TDouble> {
        public DoubleToSLongCast(IRValueExpr<TDouble> fromExpr) {
            this.FromExpr = fromExpr;
        }

        public IRValueExpr<TDouble> FromExpr { get; }

        public override Env Env => this.FromExpr.Env;

        public override TSLong Type => TSLong.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSLong(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSLong(this);
        }
    }

    public sealed class SCharToSLongCast
        : IntegralToSLongCastExpr<TSChar> {
        public SCharToSLongCast(IRValueExpr<TSChar> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ULongToSLongCast
        : IntegralToSLongCastExpr<TULong> {
        public ULongToSLongCast(IRValueExpr<TULong> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class SShortToSLongCast
        : IntegralToSLongCastExpr<TSShort> {
        public SShortToSLongCast(IRValueExpr<TSShort> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class SIntToSLongCast
        : IntegralToSLongCastExpr<TSInt> {
        public SIntToSLongCast(IRValueExpr<TSInt> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ToSignedLongCreator : ToIntegralCreator<TSLong> {
        private ToSignedLongCreator() { }

        public static ToSignedLongCreator Get = new ToSignedLongCreator();

        public override TSLong ToType => TSLong.Get;

        public override IRValueExpr<TSLong> VisitConstDouble(ConstDouble expr) {
            return new ConstSLong((Int64)expr.Value, expr.Env);
        }

        public override IRValueExpr<TSLong> VisitConstFloat(ConstFloat expr) {
            return new ConstSLong((Int64)expr.Value, expr.Env);
        }

        public override IRValueExpr<TSLong> VisitConstSChar(ConstSChar expr) {
            return new ConstSLong(expr.Value, expr.Env);
        }

        public override IRValueExpr<TSLong> VisitConstSInt(ConstSInt expr) {
            return new ConstSLong(expr.Value, expr.Env);
        }

        public override IRValueExpr<TSLong> VisitConstSLong(ConstSLong expr) {
            return expr;
        }

        public override IRValueExpr<TSLong> VisitConstSShort(ConstSShort expr) {
            return new ConstSLong(expr.Value, expr.Env);
        }

        public override IRValueExpr<TSLong> VisitConstUChar(ConstUChar expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(TSLong.Get);
        }

        public override IRValueExpr<TSLong> VisitConstUInt(ConstUInt expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(TSLong.Get);
        }

        public override IRValueExpr<TSLong> VisitConstULong(ConstULong expr) {
            return new ConstSLong((Int64)expr.Value, expr.Env);
        }

        public override IRValueExpr<TSLong> VisitConstUShort(ConstUShort expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(TSLong.Get);
        }

        public override IRValueExpr<TSLong> VisitDouble(IRValueExpr<TDouble> expr) {
            return new DoubleToSLongCast(expr);
        }

        public override IRValueExpr<TSLong> VisitFloat(IRValueExpr<TFloat> expr) {
            return new FloatToSLongCast(expr);
        }

        public override IRValueExpr<TSLong> VisitSChar(IRValueExpr<TSChar> expr) {
            return new SCharToSLongCast(expr);
        }

        public override IRValueExpr<TSLong> VisitSInt(IRValueExpr<TSInt> expr) {
            return new SIntToSLongCast(expr);
        }

        public override IRValueExpr<TSLong> VisitSLong(IRValueExpr<TSLong> expr) {
            return expr;
        }

        public override IRValueExpr<TSLong> VisitSShort(IRValueExpr<TSShort> expr) {
            return new SShortToSLongCast(expr);
        }

        public override IRValueExpr<TSLong> VisitUChar(IRValueExpr<TUChar> expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(TSLong.Get);
        }

        public override IRValueExpr<TSLong> VisitUInt(IRValueExpr<TUInt> expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(TSLong.Get);
        }

        public override IRValueExpr<TSLong> VisitULong(IRValueExpr<TULong> expr) {
            return new ULongToSLongCast(expr);
        }

        public override IRValueExpr<TSLong> VisitUShort(IRValueExpr<TUShort> expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(TSLong.Get);
        }
    }
}