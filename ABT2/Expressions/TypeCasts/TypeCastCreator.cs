using System;
using System.Diagnostics.Contracts;
using ABT2.TypeSystem;
using ABT2.Environment;

namespace ABT2.Expressions.TypeCasts {
    using IRValueExpr = IRValueExpr<IExprType>;

    public sealed class TypeCastCreator : IExprTypeVisitor<IRValueExpr> {

        public IRValueExpr Expr { get; }

        /// <summary>
        /// Construct a <see cref="TypeCastCreator"/>
        /// </summary>
        public TypeCastCreator(IRValueExpr expr) {
            this.Expr = expr;
        }

        public IRValueExpr VisitSignedChar(TSChar type) {
            Contract.Ensures(type != null);

            return this.Expr.Visit(ToSignedCharCreator.Get);
        }

        public IRValueExpr VisitUnsignedChar(TUChar type) {
            Contract.Ensures(type != null);

            return this.Expr.Visit(ToUnsignedCharCreator.Get);
        }

        public IRValueExpr VisitSignedShort(TSShort type) {
            Contract.Ensures(type != null);

            return this.Expr.Visit(ToSignedShortCreator.Get);
        }

        public IRValueExpr VisitUnsignedShort(TUShort type) {
            Contract.Ensures(type != null);

            return this.Expr.Visit(ToUnsignedShortCreator.Get);
        }

        public IRValueExpr VisitSignedInt(TSInt type) {
            Contract.Ensures(type != null);

            return this.Expr.Visit(ToSignedIntCreator.Get);
        }

        public IRValueExpr VisitUnsignedInt(TUInt type) {
            Contract.Ensures(type != null);

            return this.Expr.Visit(ToUnsignedIntCreator.Get);
        }

        public IRValueExpr VisitSignedLong(TSLong type) {
            Contract.Ensures(type != null);

            return this.Expr.Visit(ToSignedLongCreator.Get);
        }

        public IRValueExpr VisitUnsignedLong(TULong type) {
            Contract.Ensures(type != null);

            return this.Expr.Visit(ToUnsignedLongCreator.Get);
        }

        public IRValueExpr VisitFloat(TFloat type) {
            Contract.Ensures(type != null);

            return this.Expr.Visit(ToFloatCreator.Get);
        }

        public IRValueExpr VisitDouble(TDouble type) {
            Contract.Ensures(type != null);

            return this.Expr.Visit(ToDoubleCreator.Get);
        }

        public IRValueExpr VisitPointer(TPointer type) {
            Contract.Ensures(type != null);

            // TODO: implement this
            throw new NotImplementedException();
        }

        public IRValueExpr VisitStructOrUnion(StructOrUnionType type) {
            Contract.Ensures(type != null);

            if (TypeSystemUtils.TypesAreEqual(this.Expr.Type, type)) {
                return this.Expr;
            } else {
                throw new TypeCastException("Cannot cast to struct/union");
            }
        }

        public IRValueExpr VisitFunction(FunctionType type) {
            Contract.Ensures(type != null);

            // TODO: implement this
            throw new NotImplementedException();
        }

        public IRValueExpr VisitArray(ArrayType type) {
            Contract.Ensures(type != null);

            // TODO: implement this
            throw new NotImplementedException();
        }

        public IRValueExpr VisitIncompleteArray(IncompleteArrayType type) {
            Contract.Ensures(type != null);

            // TODO: implement this
            throw new NotImplementedException();
        }
    }
}