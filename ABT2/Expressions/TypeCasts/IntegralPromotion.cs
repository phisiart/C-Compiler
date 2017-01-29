using ABT2.TypeSystem;

namespace ABT2.Expressions.TypeCasts {
    public interface IIPReturn {
        IRValueExpr<IIntegralPromotionReturnType> Expr { get; }

        R Visit<R>(IIPReturnVisitor<R> visitor);
    }

    public interface IIPReturn<out T> : IIPReturn
        where T : class, IIntegralPromotionReturnType {

        new IRValueExpr<T> Expr { get; }
    }

    public abstract class IPReturn<T> : IIPReturn<T>
        where T : class, IIntegralPromotionReturnType {

        protected IPReturn(IRValueExpr<T> expr) {
            this.Expr = expr;
        }

        public IRValueExpr<T> Expr { get; }

        IRValueExpr<IIntegralPromotionReturnType> IIPReturn.Expr => this.Expr;

        public abstract R Visit<R>(IIPReturnVisitor<R> visitor);
    }

    public interface IIPReturnVisitor<out R> {
        R VisitULong(IRValueExpr<TULong> expr);

        R VisitSLong(IRValueExpr<TSLong> expr);

        R VisitUInt(IRValueExpr<TUInt> expr);

        R VisitSInt(IRValueExpr<TSInt> expr);
    }

    public sealed class IPULongReturn : IPReturn<TULong> {
        public IPULongReturn(IRValueExpr<TULong> expr) : base(expr) { }
        
        public override R Visit<R>(IIPReturnVisitor<R> visitor) {
            return visitor.VisitULong(this.Expr);
        }
    }

    public sealed class IPSLongReturn : IPReturn<TSLong> {
        public IPSLongReturn(IRValueExpr<TSLong> expr) : base(expr) { }

        public override R Visit<R>(IIPReturnVisitor<R> visitor) {
            return visitor.VisitSLong(this.Expr);
        }
    }

    public sealed class IPSIntReturn : IPReturn<TSInt> {
        public IPSIntReturn(IRValueExpr<TSInt> expr) : base(expr) { }

        public override R Visit<R>(IIPReturnVisitor<R> visitor) {
            return visitor.VisitSInt(this.Expr);
        }
    }

    public sealed class IPUIntReturn : IPReturn<TUInt> {
        public IPUIntReturn(IRValueExpr<TUInt> expr) : base(expr) { }

        public override R Visit<R>(IIPReturnVisitor<R> visitor) {
            return visitor.VisitUInt(this.Expr);
        }
    }

    /// <summary>
    /// Integegral promotion.
    /// </summary>
    /// <remarks>
    /// 1. { unsigned char, signed char, unsigned short, signed short }
    ///      -> signed int
    ///
    /// 2. signed int -> signed int
    ///
    /// 3. unsigned int -> unsigned int
    /// 
    /// 4. signed long -> signed long
    /// 
    /// 5. unsigned long -> unsigned long
    /// </remarks>
    public static class IntegralPromotion {
        public static IIPReturn Perform(IRValueExpr<IIntegralType> expr) {
            var type = expr.Type;

            if (type is TULong) {
                return new IPULongReturn((IRValueExpr<TULong>)expr);
            }

            if (type is TSLong) {
                return new IPSLongReturn((IRValueExpr<TSLong>)expr);
            }

            if (type is TUInt) {
                return new IPUIntReturn((IRValueExpr<TUInt>)expr);
            }

            return new IPSIntReturn((IRValueExpr<TSInt>)expr);
        }
    }
}