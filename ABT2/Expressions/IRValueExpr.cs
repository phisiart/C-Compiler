using ABT2.TypeSystem;
using ABT2.Environment;

namespace ABT2.Expressions {

    // IRValue
    //    |
    //    +-----------+
    //    |           |
    //IRValue<T> ILValue
    //    |           |
    //    +-----------+
    //    |           |
    //RValue<T> ILValue<T>
    //    |           |
    //    +-----------+
    //    |           |
    //  .....     LValue<T>
    //                |
    //              .....

    public interface IRValueExpr {
        IExprType Type { get; }

        Env Env { get; }

        void Visit(IRValueExprByTypeVisitor visitor);

        R Visit<R>(IRValueExprByTypeVisitor<R> visitor);
    }

    public interface IRValueExpr<out T> : IRValueExpr where T : IExprType {
        new T Type { get; }
    }

    public abstract class RValueExpr<T> : IRValueExpr<T> where T : IExprType {
        public abstract Env Env { get; }

        public abstract T Type { get; }

        IExprType IRValueExpr.Type => this.Type;

        public abstract void Visit(IRValueExprByTypeVisitor visitor);

        public abstract R Visit<R>(IRValueExprByTypeVisitor<R> visitor);
    }

    public interface IRValueExprByTypeVisitor {
        void VisitSChar(IRValueExpr<TSChar> expr);
        void VisitConstSChar(ConstSChar expr);

        void VisitUChar(IRValueExpr<TUChar> expr);
        void VisitConstUChar(ConstUChar expr);

        void VisitSShort(IRValueExpr<TSShort> expr);
        void VisitConstSShort(ConstSShort expr);

        void VisitUShort(IRValueExpr<TUShort> expr);
        void VisitConstUShort(ConstUShort expr);

        void VisitSInt(IRValueExpr<TSInt> expr);
        void VisitConstSInt(ConstSInt expr);

        void VisitUInt(IRValueExpr<TUInt> expr);
        void VisitConstUInt(ConstUInt expr);

        void VisitSLong(IRValueExpr<TSLong> expr);
        void VisitConstSLong(ConstSLong expr);

        void VisitULong(IRValueExpr<TULong> expr);
        void VisitConstULong(ConstULong expr);

        void VisitFloat(IRValueExpr<TFloat> expr);
        void VisitConstFloat(ConstFloat expr);

        void VisitDouble(IRValueExpr<TDouble> expr);
        void VisitConstDouble(ConstDouble expr);

        void VisitPointer(IRValueExpr<TPointer> expr);

        void VisitStructOrUnion(IRValueExpr<TStructOrUnion> expr);

        void VisitFunction(IRValueExpr<TFunction> expr);

        void VisitArray(IRValueExpr<ArrayType> expr);
    }

    public interface IRValueExprByTypeVisitor<out R> {
        R VisitSChar(IRValueExpr<TSChar> expr);
        R VisitConstSChar(ConstSChar expr);

        R VisitUChar(IRValueExpr<TUChar> expr);
        R VisitConstUChar(ConstUChar expr);

        R VisitSShort(IRValueExpr<TSShort> expr);
        R VisitConstSShort(ConstSShort expr);

        R VisitUShort(IRValueExpr<TUShort> expr);
        R VisitConstUShort(ConstUShort expr);

        R VisitSInt(IRValueExpr<TSInt> expr);
        R VisitConstSInt(ConstSInt expr);

        R VisitUInt(IRValueExpr<TUInt> expr);
        R VisitConstUInt(ConstUInt expr);

        R VisitSLong(IRValueExpr<TSLong> expr);
        R VisitConstSLong(ConstSLong expr);

        R VisitULong(IRValueExpr<TULong> expr);
        R VisitConstULong(ConstULong expr);

        R VisitFloat(IRValueExpr<TFloat> expr);
        R VisitConstFloat(ConstFloat expr);

        R VisitDouble(IRValueExpr<TDouble> expr);
        R VisitConstDouble(ConstDouble expr);

        R VisitPointer(IRValueExpr<TPointer> expr);

        R VisitStructOrUnion(IRValueExpr<TStructOrUnion> expr);

        R VisitFunction(IRValueExpr<TFunction> expr);

        R VisitArray(IRValueExpr<ArrayType> expr);
    }

    public interface ILValueExpr : IRValueExpr {
        IQualExprType<IExprType> QualType { get; }
    }

    public interface ILValueExpr<out T> : IRValueExpr<T>, ILValueExpr
        where T : IExprType {

        new IQualExprType<T> QualType { get; }
    }

    public abstract class LValueExpr<T> : RValueExpr<T>, ILValueExpr<T>
        where T : class, IExprType {

        public abstract IQualExprType<T> QualType { get; }

        IQualExprType<IExprType> ILValueExpr.QualType => this.QualType;
    }
}