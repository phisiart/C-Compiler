using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions.TypeCasts {

    public abstract class IntegralToUCharCast<FromType>
        : TypeCastExpr<TUChar, FromType>
        where FromType : IIntegralType {

        protected IntegralToUCharCast(IRValueExpr<FromType> fromExpr) {
            this.FromExpr = fromExpr;
        }

        public IRValueExpr<FromType> FromExpr { get; }

        public override Env Env => this.FromExpr.Env;

        public override TUChar Type => TUChar.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitUChar(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitUChar(this);
        }
    }

    public sealed class SCharToUCharCast : IntegralToUCharCast<TSChar> {
        public SCharToUCharCast(IRValueExpr<TSChar> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class UShortToUCharCast : IntegralToUCharCast<TUShort> {
        public UShortToUCharCast(IRValueExpr<TUShort> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class UIntToUCharCast : IntegralToUCharCast<TUInt> {
        public UIntToUCharCast(IRValueExpr<TUInt> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ULongToUCharCast : IntegralToUCharCast<TULong> {
        public ULongToUCharCast(IRValueExpr<TULong> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ToUnsignedCharCreator : ToIntegralCreator<TUChar> {
        private ToUnsignedCharCreator() { }

        public static ToUnsignedCharCreator Get = new ToUnsignedCharCreator();

        public override TUChar ToType => TUChar.Get;

        public override IRValueExpr<TUChar> VisitConstDouble(ConstDouble expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TUChar> VisitConstFloat(ConstFloat expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TUChar> VisitConstSChar(ConstSChar expr) {
            return new ConstUChar((UInt64)expr.Value, expr.Env);
        }

        public override IRValueExpr<TUChar> VisitConstSInt(ConstSInt expr) {
            return new ConstUChar((UInt64)expr.Value, expr.Env);
        }

        public override IRValueExpr<TUChar> VisitConstSLong(ConstSLong expr) {
            return new ConstUChar((UInt64)expr.Value, expr.Env);
        }

        public override IRValueExpr<TUChar> VisitConstSShort(ConstSShort expr) {
            return new ConstUChar((UInt64)expr.Value, expr.Env);
        }

        public override IRValueExpr<TUChar> VisitConstUChar(ConstUChar expr) {
            return expr;
        }

        public override IRValueExpr<TUChar> VisitConstUInt(ConstUInt expr) {
            return expr.CastTo(TUChar.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TUChar> VisitConstULong(ConstULong expr) {
            return expr.CastTo(TUChar.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TUChar> VisitConstUShort(ConstUShort expr) {
            return expr.CastTo(TUChar.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TUChar> VisitDouble(IRValueExpr<TDouble> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TUChar> VisitFloat(IRValueExpr<TFloat> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TUChar> VisitSChar(IRValueExpr<TSChar> expr) {
            return new SCharToUCharCast(expr);
        }

        public override IRValueExpr<TUChar> VisitSInt(IRValueExpr<TSInt> expr) {
            return expr.CastTo(TSChar.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TUChar> VisitSLong(IRValueExpr<TSLong> expr) {
            return expr.CastTo(TSChar.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TUChar> VisitSShort(IRValueExpr<TSShort> expr) {
            return expr.CastTo(TSChar.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TUChar> VisitUChar(IRValueExpr<TUChar> expr) {
            return expr;
        }

        public override IRValueExpr<TUChar> VisitUInt(IRValueExpr<TUInt> expr) {
            return new UIntToUCharCast(expr);
        }

        public override IRValueExpr<TUChar> VisitULong(IRValueExpr<TULong> expr) {
            return new ULongToUCharCast(expr);
        }

        public override IRValueExpr<TUChar> VisitUShort(IRValueExpr<TUShort> expr) {
            return new UShortToUCharCast(expr);
        }
    }

}