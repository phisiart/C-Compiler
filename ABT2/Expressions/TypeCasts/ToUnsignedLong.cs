using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions.TypeCasts {

    public abstract class IntegralToULongCastExpr<FromType>
        : TypeCastExpr<TULong, FromType>
        where FromType : IIntegralType {

        protected IntegralToULongCastExpr(IRValueExpr<FromType> fromExpr) {
            this.FromExpr = fromExpr;
        }

        public IRValueExpr<FromType> FromExpr { get; }

        public override Env Env => this.FromExpr.Env;

        public override TULong Type => TULong.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitULong(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitULong(this);
        }
    }

    public sealed class PointToULongCast : TypeCastExpr<TULong, TPointer> {
        public PointToULongCast(IRValueExpr<TPointer> fromExpr) {
            this.FromExpr = fromExpr;
        }

        public IRValueExpr<TPointer> FromExpr { get; }

        public override Env Env => this.FromExpr.Env;

        public override TULong Type => TULong.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitULong(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitULong(this);
        }
    }

    public sealed class UCharToULongCast
        : IntegralToULongCastExpr<TUChar> {
        public UCharToULongCast(IRValueExpr<TUChar> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class UShortToULongCast
        : IntegralToULongCastExpr<TUShort> {
        public UShortToULongCast(IRValueExpr<TUShort> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class UIntToULongCast
        : IntegralToULongCastExpr<TUInt> {
        public UIntToULongCast(IRValueExpr<TUInt> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class SLongToULongCast
        : IntegralToULongCastExpr<TSLong> {
        public SLongToULongCast(IRValueExpr<TSLong> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ToUnsignedLongCreator : ToIntegralCreator<TULong> {
        private ToUnsignedLongCreator() { }

        public static ToUnsignedLongCreator Get = new ToUnsignedLongCreator();

        public override TULong ToType => TULong.Get;

        public override IRValueExpr<TULong> VisitConstDouble(ConstDouble expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TULong.Get);
        }

        public override IRValueExpr<TULong> VisitConstFloat(ConstFloat expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TULong.Get);
        }

        public override IRValueExpr<TULong> VisitConstSChar(ConstSChar expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TULong.Get);
        }

        public override IRValueExpr<TULong> VisitConstSInt(ConstSInt expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TULong.Get);
        }

        public override IRValueExpr<TULong> VisitConstSLong(ConstSLong expr) {
            throw new NotImplementedException();
        }

        public override IRValueExpr<TULong> VisitConstSShort(ConstSShort expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TULong.Get);
        }

        public override IRValueExpr<TULong> VisitConstUChar(ConstUChar expr) {
            return new ConstULong(expr.Value, expr.Env);
        }

        public override IRValueExpr<TULong> VisitConstUInt(ConstUInt expr) {
            return new ConstULong(expr.Value, expr.Env);
        }

        public override IRValueExpr<TULong> VisitConstULong(ConstULong expr) {
            return expr;
        }

        public override IRValueExpr<TULong> VisitConstUShort(ConstUShort expr) {
            return new ConstULong(expr.Value, expr.Env);
        }

        public override IRValueExpr<TULong> VisitDouble(IRValueExpr<TDouble> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TULong.Get);
        }

        public override IRValueExpr<TULong> VisitFloat(IRValueExpr<TFloat> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TULong.Get);
        }

        public override IRValueExpr<TULong> VisitSChar(IRValueExpr<TSChar> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TULong.Get);
        }

        public override IRValueExpr<TULong> VisitSInt(IRValueExpr<TSInt> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TULong.Get);
        }

        public override IRValueExpr<TULong> VisitSLong(IRValueExpr<TSLong> expr) {
            return new SLongToULongCast(expr);
        }

        public override IRValueExpr<TULong> VisitSShort(IRValueExpr<TSShort> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TULong.Get);
        }

        public override IRValueExpr<TULong> VisitUChar(IRValueExpr<TUChar> expr) {
            return new UCharToULongCast(expr);
        }

        public override IRValueExpr<TULong> VisitUInt(IRValueExpr<TUInt> expr) {
            return new UIntToULongCast(expr);
        }

        public override IRValueExpr<TULong> VisitULong(IRValueExpr<TULong> expr) {
            return expr;
        }

        public override IRValueExpr<TULong> VisitUShort(IRValueExpr<TUShort> expr) {
            return new UShortToULongCast(expr);
        }
    }
}