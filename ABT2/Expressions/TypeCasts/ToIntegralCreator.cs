using System;
using System.Diagnostics.Contracts;
using ABT2.TypeSystem;

namespace ABT2.Expressions.TypeCasts {
    public abstract class ToIntegralCreator<T> : IRValueExprByTypeVisitor<IRValueExpr<T>> where T : IIntegralType {
        protected ToIntegralCreator() { }

        public abstract T ToType { get; }

        /// <summary>
        /// Cast from array type to integral type T.
        /// </summary>
        public IRValueExpr<T> VisitArray(IRValueExpr<ArrayType> expr) {
            throw new TypeCastException("Not Implemented");
        }

        /// <summary>
        /// Cast from function type to integral type T.
        /// </summary>
        public IRValueExpr<T> VisitFunction(IRValueExpr<TFunction> expr) {
            throw new TypeCastException("Not Implemented");
        }

        /// <summary>
        /// Cast from pointer to integral.
        /// Always first cast to unsigned long.
        /// </summary>
        public IRValueExpr<T> VisitPointer(IRValueExpr<TPointer> expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }

        /// <summary>
        /// Cast from struct or union to integral.
        /// This is impossible.
        /// </summary>
        public IRValueExpr<T> VisitStructOrUnion(IRValueExpr<TStructOrUnion> expr) {
            throw new TypeCastException("Cannot cast struct or union to integral");
        }

        public abstract IRValueExpr<T> VisitConstDouble(ConstDouble expr);
        public abstract IRValueExpr<T> VisitConstFloat(ConstFloat expr);
        public abstract IRValueExpr<T> VisitConstSChar(ConstSChar expr);
        public abstract IRValueExpr<T> VisitConstSInt(ConstSInt expr);
        public abstract IRValueExpr<T> VisitConstSLong(ConstSLong expr);
        public abstract IRValueExpr<T> VisitConstSShort(ConstSShort expr);
        public abstract IRValueExpr<T> VisitConstUChar(ConstUChar expr);
        public abstract IRValueExpr<T> VisitConstUInt(ConstUInt expr);
        public abstract IRValueExpr<T> VisitConstULong(ConstULong expr);
        public abstract IRValueExpr<T> VisitConstUShort(ConstUShort expr);

        public abstract IRValueExpr<T> VisitDouble(IRValueExpr<TDouble> expr);
        public abstract IRValueExpr<T> VisitFloat(IRValueExpr<TFloat> expr);
        public abstract IRValueExpr<T> VisitSChar(IRValueExpr<TSChar> expr);
        public abstract IRValueExpr<T> VisitSInt(IRValueExpr<TSInt> expr);
        public abstract IRValueExpr<T> VisitSLong(IRValueExpr<TSLong> expr);
        public abstract IRValueExpr<T> VisitSShort(IRValueExpr<TSShort> expr);

        public abstract IRValueExpr<T> VisitUChar(IRValueExpr<TUChar> expr);
        public abstract IRValueExpr<T> VisitUInt(IRValueExpr<TUInt> expr);
        public abstract IRValueExpr<T> VisitULong(IRValueExpr<TULong> expr);
        public abstract IRValueExpr<T> VisitUShort(IRValueExpr<TUShort> expr);
    }
}