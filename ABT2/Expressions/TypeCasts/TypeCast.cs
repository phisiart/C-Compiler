using ABT2.TypeSystem;
using ABT2.Environment;
using System;

namespace ABT2.Expressions.TypeCasts {

    public enum TypeCastKind {
        
    }

    public abstract class TypeCastExpr<ToType, FromType> : IRValueExpr<ToType> where ToType : IExprType where FromType : IExprType {
        public abstract ToType Type { get; }

        public abstract Env Env { get; }

        IExprType Expressions.IRValueExpr.Type => this.Type;

        public abstract void Visit(IRValueExprByTypeVisitor visitor);

        public abstract R Visit<R>(IRValueExprByTypeVisitor<R> visitor);
    }

    /// <summary>
    /// The interface to create type cast expressions.
    /// </summary>
    public static class TypeCastExpr {
        
        /// <summary>
        /// This doesn't necessarily create a TypeCastExpr.
        /// If the expression is a constant, will calculate cast result and return a new constant.
        /// So the return type can only be IExpr.
        /// </summary>
        public static IRValueExpr<T> Create<T>(T type, IRValueExpr expr) where T : IExprType {
            TypeCastCreator creator = new TypeCastCreator(expr);
            IRValueExpr ret = type.Visit(creator);
            return (IRValueExpr<T>)ret;
        }

        public static IRValueExpr<T> CastTo<T>(this IRValueExpr expr, T type) where T : IExprType {
            return Create<T>(type, expr);
        }
    }

    public enum TypeCastType {
        INT8_TO_INT16,
        INT8_TO_INT32,

        INT16_TO_INT32,

        INT32_TO_FLOAT,
        INT32_TO_DOUBLE,

        PRESERVE_INT8,
        PRESERVE_INT16,

        UINT8_TO_UINT16,
        UINT8_TO_UINT32,

        UINT16_TO_UINT32,

        FLOAT_TO_INT32,
        FLOAT_TO_DOUBLE,

        DOUBLE_TO_INT32,
        DOUBLE_TO_FLOAT
    }
}