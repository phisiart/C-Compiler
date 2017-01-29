using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions.TypeCasts {
    
    public abstract class IntegralToSCharCastExpr<FromType>
        : TypeCastExpr<TSChar, FromType>
        where FromType : class, IIntegralType {

        protected IntegralToSCharCastExpr(IRValueExpr<FromType> fromExpr) {
            this.FromExpr = fromExpr;
        }

        public IRValueExpr<FromType> FromExpr { get; }

        public override Env Env => this.FromExpr.Env;

        public override TSChar Type => TSChar.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSChar(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSChar(this);
        }
    }

    public sealed class UCharToSCharCast
        : IntegralToSCharCastExpr<TUChar> {
        public UCharToSCharCast(IRValueExpr<TUChar> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class SShortToSCharCast
        : IntegralToSCharCastExpr<TSShort> {
        public SShortToSCharCast(IRValueExpr<TSShort> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class SIntToSCharCast
        : IntegralToSCharCastExpr<TSInt> {
        public SIntToSCharCast(IRValueExpr<TSInt> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class SLongToSCharCast
        : IntegralToSCharCastExpr<TSLong> {
        public SLongToSCharCast(IRValueExpr<TSLong> fromExpr)
            : base(fromExpr) { }
    }

    public sealed class ToSignedCharCreator : ToIntegralCreator<TSChar> {
        private ToSignedCharCreator() { }

        public static ToSignedCharCreator Get = new ToSignedCharCreator();

        public override TSChar ToType => TSChar.Get;

        public override IRValueExpr<TSChar> VisitConstDouble(ConstDouble expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSChar> VisitConstFloat(ConstFloat expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSChar> VisitConstSChar(ConstSChar expr) {
            return expr;
        }

        public override IRValueExpr<TSChar> VisitConstSInt(ConstSInt expr) {
            return new ConstSChar(expr.Value, expr.Env);
        }

        public override IRValueExpr<TSChar> VisitConstSLong(ConstSLong expr) {
            return new ConstSChar(expr.Value, expr.Env);
        }

        public override IRValueExpr<TSChar> VisitConstSShort(ConstSShort expr) {
            return new ConstSChar(expr.Value, expr.Env);
        }

        public override IRValueExpr<TSChar> VisitConstUChar(ConstUChar expr) {
            return new ConstSChar((Int64)expr.Value, expr.Env);
        }

        public override IRValueExpr<TSChar> VisitConstUInt(ConstUInt expr) {
            return expr.CastTo(TUChar.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSChar> VisitConstULong(ConstULong expr) {
            return expr.CastTo(TUChar.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSChar> VisitConstUShort(ConstUShort expr) {
            return expr.CastTo(TUChar.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSChar> VisitDouble(IRValueExpr<TDouble> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSChar> VisitFloat(IRValueExpr<TFloat> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSChar> VisitSChar(IRValueExpr<TSChar> expr) {
            return expr;
        }

        public override IRValueExpr<TSChar> VisitSInt(IRValueExpr<TSInt> expr) {
            return new SIntToSCharCast(expr);
        }

        public override IRValueExpr<TSChar> VisitSLong(IRValueExpr<TSLong> expr) {
            return new SLongToSCharCast(expr);
        }

        public override IRValueExpr<TSChar> VisitSShort(IRValueExpr<TSShort> expr) {
            return new SShortToSCharCast(expr);
        }

        public override IRValueExpr<TSChar> VisitUChar(IRValueExpr<TUChar> expr) {
            return new UCharToSCharCast(expr);
        }

        public override IRValueExpr<TSChar> VisitUInt(IRValueExpr<TUInt> expr) {
            return expr.CastTo(TUChar.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSChar> VisitULong(IRValueExpr<TULong> expr) {
            return expr.CastTo(TUChar.Get)
                       .CastTo(this.ToType);
        }

        public override IRValueExpr<TSChar> VisitUShort(IRValueExpr<TUShort> expr) {
            return expr.CastTo(TUChar.Get)
                       .CastTo(this.ToType);
        }
    }
}
