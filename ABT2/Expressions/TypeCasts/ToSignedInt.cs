using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions.TypeCasts {

    public abstract class IntegralToSIntCastExpr<FromType>
        : TypeCastExpr<TSInt, FromType>
        where FromType : class, IIntegralType {

        protected IntegralToSIntCastExpr(IRValueExpr<FromType> fromExpr) {
            this.FromExpr = fromExpr;
        }

        public IRValueExpr<FromType> FromExpr { get; }

        public override Env Env => this.FromExpr.Env;

        public override TSInt Type => TSInt.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSInt(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSInt(this);
        }
    }

    public sealed class SCharToSIntCast
        : IntegralToSIntCastExpr<TSChar> {
        public SCharToSIntCast(IRValueExpr<TSChar> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class UIntToSIntCast
        : IntegralToSIntCastExpr<TUInt> {
        public UIntToSIntCast(IRValueExpr<TUInt> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class SShortToSIntCast
        : IntegralToSIntCastExpr<TSShort> {
        public SShortToSIntCast(IRValueExpr<TSShort> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class SLongToSIntCast
        : IntegralToSIntCastExpr<TSLong> {
        public SLongToSIntCast(IRValueExpr<TSLong> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ToSignedIntCreator : ToIntegralCreator<TSInt> {
        private ToSignedIntCreator() { }

        public static ToSignedIntCreator Get = new ToSignedIntCreator();

        public override TSInt ToType => TSInt.Get;

        public override IRValueExpr<TSInt> VisitConstDouble(ConstDouble expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TSInt.Get);
        }

        public override IRValueExpr<TSInt> VisitConstFloat(ConstFloat expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TSInt.Get);
        }

        public override IRValueExpr<TSInt> VisitConstSChar(ConstSChar expr) {
            return new ConstSInt(expr.Value, expr.Env);
        }

        public override IRValueExpr<TSInt> VisitConstSInt(ConstSInt expr) {
            return expr;
        }

        public override IRValueExpr<TSInt> VisitConstSLong(ConstSLong expr) {
            return new ConstSInt(expr.Value, expr.Env);
        }

        public override IRValueExpr<TSInt> VisitConstSShort(ConstSShort expr) {
            return new ConstSInt(expr.Value, expr.Env);
        }

        public override IRValueExpr<TSInt> VisitConstUChar(ConstUChar expr) {
            return expr.CastTo(TUInt.Get)
                       .CastTo(TSInt.Get);
        }

        public override IRValueExpr<TSInt> VisitConstUInt(ConstUInt expr) {
            return new ConstSInt((Int64)expr.Value, expr.Env);
        }

        public override IRValueExpr<TSInt> VisitConstULong(ConstULong expr) {
            return expr.CastTo(TUInt.Get)
                       .CastTo(TSInt.Get);
        }

        public override IRValueExpr<TSInt> VisitConstUShort(ConstUShort expr) {
            return expr.CastTo(TUInt.Get)
                       .CastTo(TSInt.Get);
        }

        public override IRValueExpr<TSInt> VisitDouble(IRValueExpr<TDouble> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TSInt.Get);
        }

        public override IRValueExpr<TSInt> VisitFloat(IRValueExpr<TFloat> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(TSInt.Get);
        }

        public override IRValueExpr<TSInt> VisitSChar(IRValueExpr<TSChar> expr) {
            return new SCharToSIntCast(expr);
        }

        public override IRValueExpr<TSInt> VisitSInt(IRValueExpr<TSInt> expr) {
            return expr;
        }

        public override IRValueExpr<TSInt> VisitSLong(IRValueExpr<TSLong> expr) {
            return new SLongToSIntCast(expr);
        }

        public override IRValueExpr<TSInt> VisitSShort(IRValueExpr<TSShort> expr) {
            return new SShortToSIntCast(expr);
        }

        public override IRValueExpr<TSInt> VisitUChar(IRValueExpr<TUChar> expr) {
            return expr.CastTo(TUInt.Get)
                       .CastTo(TSInt.Get);
        }

        public override IRValueExpr<TSInt> VisitUInt(IRValueExpr<TUInt> expr) {
            return new UIntToSIntCast(expr);
        }

        public override IRValueExpr<TSInt> VisitULong(IRValueExpr<TULong> expr) {
            return expr.CastTo(TUInt.Get)
                       .CastTo(TSInt.Get);
        }

        public override IRValueExpr<TSInt> VisitUShort(IRValueExpr<TUShort> expr) {
            return expr.CastTo(TUInt.Get)
                       .CastTo(TSInt.Get);
        }
    }
}