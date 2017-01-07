using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions.TypeCasts {
    public class ToPointerCreator
        : IRValueExprByTypeVisitor<IRValueExpr<TPointer>> {

        public ToPointerCreator(TPointer toType) {
            this.ToType = toType;
        }

        public TPointer ToType { get; }

        public IRValueExpr<TPointer> VisitArray(IRValueExpr<ArrayType> expr) {
            // TODO: implement this
            throw new NotImplementedException();
        }

        public IRValueExpr<TPointer> VisitConstDouble(ConstDouble expr) {
            throw new TypeCastException("Cannot cast double to pointer");
        }

        public IRValueExpr<TPointer> VisitConstFloat(ConstFloat expr) {
            throw new TypeCastException("Cannot cast float to pointer");
        }

        public IRValueExpr<TPointer> VisitConstSChar(ConstSChar expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<TPointer> VisitConstSInt(ConstSInt expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<TPointer> VisitConstSLong(ConstSLong expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<TPointer> VisitConstSShort(ConstSShort expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<TPointer> VisitConstUChar(ConstUChar expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<TPointer> VisitConstUInt(ConstUInt expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<TPointer> VisitConstULong(ConstULong expr) {
            // TODO: implement this
            throw new NotImplementedException();
        }

        public IRValueExpr<TPointer> VisitConstUShort(ConstUShort expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<TPointer> VisitDouble(IRValueExpr<TDouble> expr) {
            throw new TypeCastException("Cannot cast double to pointer");
        }

        public IRValueExpr<TPointer> VisitFloat(IRValueExpr<TFloat> expr) {
            throw new TypeCastException("Cannot cast float to pointer");
        }

        public IRValueExpr<TPointer> VisitFunction(IRValueExpr<TFunction> expr) {
            // TODO: implement this
            throw new NotImplementedException();
        }

        public IRValueExpr<TPointer> VisitPointer(IRValueExpr<TPointer> expr) {
            // TODO: implement this
            throw new NotImplementedException();
        }

        public IRValueExpr<TPointer> VisitSChar(IRValueExpr<TSChar> expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<TPointer> VisitSInt(IRValueExpr<TSInt> expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<TPointer> VisitSLong(IRValueExpr<TSLong> expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<TPointer> VisitSShort(IRValueExpr<TSShort> expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<TPointer> VisitStructOrUnion(IRValueExpr<TStructOrUnion> expr) {
            throw new TypeCastException("Cannot cast struct or union to pointer");
        }

        public IRValueExpr<TPointer> VisitUChar(IRValueExpr<TUChar> expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<TPointer> VisitUInt(IRValueExpr<TUInt> expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }

        public IRValueExpr<TPointer> VisitULong(IRValueExpr<TULong> expr) {
            // TODO: implement this
            throw new NotImplementedException();
        }

        public IRValueExpr<TPointer> VisitUShort(IRValueExpr<TUShort> expr) {
            return expr.CastTo(TULong.Get)
                       .CastTo(this.ToType);
        }
    }
}
