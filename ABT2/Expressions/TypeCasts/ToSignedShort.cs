using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions.TypeCasts {

    public abstract class IntegralToSShortCastExpr<FromType>
        : TypeCastExpr<TSShort, FromType>
        where FromType : IIntegralType {

        protected IntegralToSShortCastExpr(IRValueExpr<FromType> fromExpr) {
            this.FromExpr = fromExpr;
        }

        public IRValueExpr<FromType> FromExpr { get; }

        public override Env Env => this.FromExpr.Env;

        public override TSShort Type => TSShort.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSShort(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSShort(this);
        }
    }

    public sealed class SCharToSShortCast
        : IntegralToSShortCastExpr<TSChar> {
        public SCharToSShortCast(IRValueExpr<TSChar> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class UShortToSShortCast
        : IntegralToSShortCastExpr<TUShort> {
        public UShortToSShortCast(IRValueExpr<TUShort> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class SIntToSShortCast
        : IntegralToSShortCastExpr<TSInt> {
        public SIntToSShortCast(IRValueExpr<TSInt> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class SLongToSShortCast
        : IntegralToSShortCastExpr<TSLong> {
        public SLongToSShortCast(IRValueExpr<TSLong> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ToSignedShortCreator : ToIntegralCreator<TSShort> {
        private ToSignedShortCreator() { }

        public static ToSignedShortCreator Get = new ToSignedShortCreator();

        public override TSShort ToType => TSShort.Get;

        public override IRValueExpr<TSShort> VisitConstDouble(ConstDouble expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSShort> VisitConstFloat(ConstFloat expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSShort> VisitConstSChar(ConstSChar expr) {
            return new ConstSShort(expr.Value, expr.Env);
        }

        public override IRValueExpr<TSShort> VisitConstSInt(ConstSInt expr) {
            return new ConstSShort(expr.Value, expr.Env);
        }

        public override IRValueExpr<TSShort> VisitConstSLong(ConstSLong expr) {
            return new ConstSShort(expr.Value, expr.Env);
        }

        public override IRValueExpr<TSShort> VisitConstSShort(ConstSShort expr) {
            return expr;
        }

        public override IRValueExpr<TSShort> VisitConstUChar(ConstUChar expr) {
            return expr.CastTo(TUShort.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSShort> VisitConstUInt(ConstUInt expr) {
            return expr.CastTo(TUShort.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSShort> VisitConstULong(ConstULong expr) {
            return expr.CastTo(TUShort.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSShort> VisitConstUShort(ConstUShort expr) {
            return new ConstSShort((Int64)expr.Value, expr.Env);
        }

        public override IRValueExpr<TSShort> VisitDouble(IRValueExpr<TDouble> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSShort> VisitFloat(IRValueExpr<TFloat> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSShort> VisitSChar(IRValueExpr<TSChar> expr) {
            return new SCharToSShortCast(expr);
        }

        public override IRValueExpr<TSShort> VisitSInt(IRValueExpr<TSInt> expr) {
            return new SIntToSShortCast(expr);
        }

        public override IRValueExpr<TSShort> VisitSLong(IRValueExpr<TSLong> expr) {
            return new SLongToSShortCast(expr);
        }

        public override IRValueExpr<TSShort> VisitSShort(IRValueExpr<TSShort> expr) {
            return expr;
        }

        public override IRValueExpr<TSShort> VisitUChar(IRValueExpr<TUChar> expr) {
            return expr.CastTo(TUShort.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSShort> VisitUInt(IRValueExpr<TUInt> expr) {
            return expr.CastTo(TUShort.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSShort> VisitULong(IRValueExpr<TULong> expr) {
            return expr.CastTo(TUShort.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSShort> VisitUShort(IRValueExpr<TUShort> expr) {
            return expr.CastTo(TUShort.Get)
                       .CastTo(this.ToType);
        }
    }
}