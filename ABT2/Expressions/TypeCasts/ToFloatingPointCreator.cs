using System;
using System.Diagnostics.Contracts;
using ABT2.TypeSystem;

namespace ABT2.Expressions.TypeCasts {
    public abstract class ToFloatingPointCreator<T>
        : IRValueExprByTypeVisitor<IRValueExpr<T>>
        where T : IFloatingPointType {

        protected ToFloatingPointCreator() { }

        public abstract T ToType { get; }

        /// <summary>
        /// Cast from array type to floating point type T.
        /// </summary>
        public IRValueExpr<T> VisitArray(IRValueExpr<ArrayType> expr) {
            // TODO: implement this
            throw new TypeCastException("Not Implemented");
        }

        /// <summary>
        /// Cast from function type to floating point type T.
        /// </summary>
        public IRValueExpr<T> VisitFunction(IRValueExpr<TFunction> expr) {
            // TODO: implement this
            throw new TypeCastException("Not Implemented");
        }

        /// <summary>
        /// Cast from pointer to floating point type T.
        /// </summary>
        public IRValueExpr<T> VisitPointer(IRValueExpr<TPointer> expr) {
            // TODO: implement this
            throw new TypeCastException("Not Implemented");
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
        public abstract IRValueExpr<T> VisitConstSLong(ConstSLong expr);
        public abstract IRValueExpr<T> VisitConstULong(ConstULong expr);

        public abstract IRValueExpr<T> VisitDouble(IRValueExpr<TDouble> expr);
        public abstract IRValueExpr<T> VisitFloat(IRValueExpr<TFloat> expr);
        public abstract IRValueExpr<T> VisitSLong(IRValueExpr<TSLong> expr);
        public abstract IRValueExpr<T> VisitULong(IRValueExpr<TULong> expr);

        public IRValueExpr<T> VisitConstSChar(ConstSChar expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<T> VisitConstSShort(ConstSShort expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<T> VisitConstSInt(ConstSInt expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<T> VisitConstUChar(ConstUChar expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<T> VisitConstUShort(ConstUShort expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<T> VisitConstUInt(ConstUInt expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<T> VisitSChar(IRValueExpr<TSChar> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<T> VisitSInt(IRValueExpr<TSInt> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<T> VisitSShort(IRValueExpr<TSShort> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<T> VisitUChar(IRValueExpr<TUChar> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<T> VisitUInt(IRValueExpr<TUInt> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<T> VisitUShort(IRValueExpr<TUShort> expr) {
            return expr.CastTo(TSLong.Get)
                       .CastTo(this.ToType);
        }
    }
}