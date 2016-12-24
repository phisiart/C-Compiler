using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions.TypeCasts {

    public abstract class IntegralToUIntCastExpr<FromType>
        : TypeCastExpr<TUInt, FromType>
        where FromType : IIntegralType {

        protected IntegralToUIntCastExpr(IRValueExpr<FromType> fromExpr) {
            this.FromExpr = fromExpr;
        }

        public IRValueExpr<FromType> FromExpr { get; }

        public override Env Env => this.FromExpr.Env;

        public override TUInt Type => TUInt.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitUInt(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitUInt(this);
        }
    }

    public sealed class UCharToUIntCast
        : IntegralToUIntCastExpr<TUChar> {
        public UCharToUIntCast(IRValueExpr<TUChar> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class UShortToUIntCast
        : IntegralToUIntCastExpr<TUShort> {
        public UShortToUIntCast(IRValueExpr<TUShort> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class SIntToUIntCast
        : IntegralToUIntCastExpr<TSInt> {
        public SIntToUIntCast(IRValueExpr<TSInt> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ULongToUIntCast
        : IntegralToUIntCastExpr<TULong> {
        public ULongToUIntCast(IRValueExpr<TULong> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ToUnsignedIntCreator : ToIntegralCreator<TUInt> {
        private ToUnsignedIntCreator() { }

        public static ToUnsignedIntCreator Get = new ToUnsignedIntCreator();

        public override TUInt ToType => TUInt.Get;

        public override IRValueExpr<TUInt> VisitConstDouble(ConstDouble expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TUInt.Get);
        }

        public override IRValueExpr<TUInt> VisitConstFloat(ConstFloat expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TUInt.Get);
        }

        public override IRValueExpr<TUInt> VisitConstSChar(ConstSChar expr) {
            return expr.CastTo(TSInt.Get)
                       .CastTo(TUInt.Get);
        }

        public override IRValueExpr<TUInt> VisitConstSInt(ConstSInt expr) {
            return new ConstUInt((UInt64)expr.Value, expr.Env);
        }

        public override IRValueExpr<TUInt> VisitConstSLong(ConstSLong expr) {
            return expr.CastTo(TSInt.Get)
                       .CastTo(TUInt.Get);
        }

        public override IRValueExpr<TUInt> VisitConstSShort(ConstSShort expr) {
            return expr.CastTo(TSInt.Get)
                       .CastTo(TUInt.Get);
        }

        public override IRValueExpr<TUInt> VisitConstUChar(ConstUChar expr) {
            return new ConstUInt(expr.Value, expr.Env);
        }

        public override IRValueExpr<TUInt> VisitConstUInt(ConstUInt expr) {
            return expr;
        }

        public override IRValueExpr<TUInt> VisitConstULong(ConstULong expr) {
            return new ConstUInt(expr.Value, expr.Env);
        }

        public override IRValueExpr<TUInt> VisitConstUShort(ConstUShort expr) {
            return new ConstUInt(expr.Value, expr.Env);
        }

        public override IRValueExpr<TUInt> VisitDouble(IRValueExpr<TDouble> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TUInt.Get);
        }

        public override IRValueExpr<TUInt> VisitFloat(IRValueExpr<TFloat> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TUInt.Get);
        }

        public override IRValueExpr<TUInt> VisitSChar(IRValueExpr<TSChar> expr) {
            return expr.CastTo(TSInt.Get)
                       .CastTo(TUInt.Get);
        }

        public override IRValueExpr<TUInt> VisitSInt(IRValueExpr<TSInt> expr) {
            return new SIntToUIntCast(expr);
        }

        public override IRValueExpr<TUInt> VisitSLong(IRValueExpr<TSLong> expr) {
            return expr.CastTo(TSInt.Get)
                       .CastTo(TUInt.Get);
        }

        public override IRValueExpr<TUInt> VisitSShort(IRValueExpr<TSShort> expr) {
            return expr.CastTo(TSInt.Get)
                       .CastTo(TUInt.Get);
        }

        public override IRValueExpr<TUInt> VisitUChar(IRValueExpr<TUChar> expr) {
            return new UCharToUIntCast(expr);
        }

        public override IRValueExpr<TUInt> VisitUInt(IRValueExpr<TUInt> expr) {
            return expr;
        }

        public override IRValueExpr<TUInt> VisitULong(IRValueExpr<TULong> expr) {
            return new ULongToUIntCast(expr);
        }

        public override IRValueExpr<TUInt> VisitUShort(IRValueExpr<TUShort> expr) {
            return new UShortToUIntCast(expr);
        }
    }

}