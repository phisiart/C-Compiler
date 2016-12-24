using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions.TypeCasts {

    public abstract class IntegralToUShortCastExpr<FromType>
        : TypeCastExpr<TUShort, FromType>
        where FromType : IIntegralType {

        protected IntegralToUShortCastExpr(IRValueExpr<FromType> fromExpr) {
            this.FromExpr = fromExpr;
        }

        public IRValueExpr<FromType> FromExpr { get; }

        public override Env Env => this.FromExpr.Env;

        public override TUShort Type => TUShort.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitUShort(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitUShort(this);
        }
    }

    public sealed class UCharToUShortCast
        : IntegralToUShortCastExpr<TUChar> {
        public UCharToUShortCast(IRValueExpr<TUChar> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class SShortToUShortCast
        : IntegralToUShortCastExpr<TSShort> {
        public SShortToUShortCast(IRValueExpr<TSShort> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class UIntToUShortCast
        : IntegralToUShortCastExpr<TUInt> {
        public UIntToUShortCast(IRValueExpr<TUInt> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ULongToUShortCast
        : IntegralToUShortCastExpr<TULong> {
        public ULongToUShortCast(IRValueExpr<TULong> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ToUnsignedShortCreator : ToIntegralCreator<TUShort> {
        private ToUnsignedShortCreator() { }

        public static ToUnsignedShortCreator Get = new ToUnsignedShortCreator();

        public override TUShort ToType => TUShort.Get;

        public override IRValueExpr<TUShort> VisitConstDouble(ConstDouble expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TUShort.Get);
        }

        public override IRValueExpr<TUShort> VisitConstFloat(ConstFloat expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TUShort.Get);
        }

        public override IRValueExpr<TUShort> VisitConstSChar(ConstSChar expr) {
            return expr.CastTo(TSShort.Get)
                       .CastTo(TUShort.Get);
        }

        public override IRValueExpr<TUShort> VisitConstSInt(ConstSInt expr) {
            return expr.CastTo(TSShort.Get)
                       .CastTo(TUShort.Get);
        }

        public override IRValueExpr<TUShort> VisitConstSLong(ConstSLong expr) {
            return expr.CastTo(TSShort.Get)
                       .CastTo(TUShort.Get);
        }

        public override IRValueExpr<TUShort> VisitConstSShort(ConstSShort expr) {
            return new ConstUShort((UInt64)expr.Value, expr.Env);
        }

        public override IRValueExpr<TUShort> VisitConstUChar(ConstUChar expr) {
            return new ConstUShort(expr.Value, expr.Env);
        }

        public override IRValueExpr<TUShort> VisitConstUInt(ConstUInt expr) {
            return new ConstUShort(expr.Value, expr.Env);
        }

        public override IRValueExpr<TUShort> VisitConstULong(ConstULong expr) {
            return new ConstUShort(expr.Value, expr.Env);
        }

        public override IRValueExpr<TUShort> VisitConstUShort(ConstUShort expr) {
            return expr;
        }

        public override IRValueExpr<TUShort> VisitDouble(IRValueExpr<TDouble> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TUShort.Get);
        }

        public override IRValueExpr<TUShort> VisitFloat(IRValueExpr<TFloat> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TUShort.Get);
        }

        public override IRValueExpr<TUShort> VisitSChar(IRValueExpr<TSChar> expr) {
            return expr.CastTo(TSShort.Get)
                       .CastTo(TUShort.Get);
        }

        public override IRValueExpr<TUShort> VisitSInt(IRValueExpr<TSInt> expr) {
            return expr.CastTo(TSShort.Get)
                       .CastTo(TUShort.Get);
        }

        public override IRValueExpr<TUShort> VisitSLong(IRValueExpr<TSLong> expr) {
            return expr.CastTo(TSShort.Get)
                       .CastTo(TUShort.Get);
        }

        public override IRValueExpr<TUShort> VisitSShort(IRValueExpr<TSShort> expr) {
            return new SShortToUShortCast(expr);
        }

        public override IRValueExpr<TUShort> VisitUChar(IRValueExpr<TUChar> expr) {
            return new UCharToUShortCast(expr);
        }

        public override IRValueExpr<TUShort> VisitUInt(IRValueExpr<TUInt> expr) {
            return new UIntToUShortCast(expr);
        }

        public override IRValueExpr<TUShort> VisitULong(IRValueExpr<TULong> expr) {
            return new ULongToUShortCast(expr);
        }

        public override IRValueExpr<TUShort> VisitUShort(IRValueExpr<TUShort> expr) {
            return expr;
        }
    }

}