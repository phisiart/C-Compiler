using System;
using ABT2.TypeSystem;

namespace ABT2.Expressions.TypeCasts {
    public interface IUACReturn {
        IRValueExpr<IArithmeticType> Left { get; }

        IRValueExpr<IArithmeticType> Right { get; }

        R Visit<R>(IUACReturnVisitor<R> visitor);

        //R Match<R>(
        //    Func<IRValueExpr<TDouble>, IRValueExpr<TDouble>, R> matchDouble,
        //    Func<IRValueExpr<TFloat>, IRValueExpr<TFloat>, R> matchFloat,
        //    Func<IRValueExpr<TULong>, IRValueExpr<TULong>, R> matchULong,
        //);
    }

    public interface IUACReturn<out T> : IUACReturn
        where T : class, IArithmeticType {

        new IRValueExpr<T> Left { get; }

        new IRValueExpr<T> Right { get; }
    }

    public interface IUACIntegralReturn : IUACReturn {
        new IRValueExpr<IIntegralPromotionReturnType> Left { get; }

        new IRValueExpr<IIntegralPromotionReturnType> Right { get; }

        R Visit<R>(IUACIntegralReturnVisitor<R> visitor);
    }

    public interface IUACIntegralReturn<out T>
        : IUACReturn<T>, IUACIntegralReturn
        where T : class, IIntegralPromotionReturnType {

        new IRValueExpr<IIntegralPromotionReturnType> Left { get; }

        new IRValueExpr<IIntegralPromotionReturnType> Right { get; }
    }

    public abstract class UACReturn<T> : IUACReturn<T>
        where T : class, IArithmeticType {

        protected UACReturn(IRValueExpr<T> left, IRValueExpr<T> right) {
            this.Left = left;
            this.Right = right;
        }

        IRValueExpr<IArithmeticType> IUACReturn.Left => this.Left;

        IRValueExpr<IArithmeticType> IUACReturn.Right => this.Right;

        public IRValueExpr<T> Left { get; }

        public IRValueExpr<T> Right { get; }

        public abstract R Visit<R>(IUACReturnVisitor<R> visitor);
    }

    public abstract class UACIntegralReturn<T>
        : UACReturn<T>, IUACIntegralReturn<T>
        where T : class, IIntegralPromotionReturnType {

        protected UACIntegralReturn(IRValueExpr<T> left, IRValueExpr<T> right)
            : base(left, right) { }

        IRValueExpr<IIntegralPromotionReturnType> IUACIntegralReturn.Left => this.Left;

        IRValueExpr<IIntegralPromotionReturnType> IUACIntegralReturn<T>.Left => this.Left;

        IRValueExpr<IIntegralPromotionReturnType> IUACIntegralReturn.Right => this.Right;

        IRValueExpr<IIntegralPromotionReturnType> IUACIntegralReturn<T>.Right => this.Right;

        public abstract R Visit<R>(IUACIntegralReturnVisitor<R> visitor);
    }

    public interface IUACIntegralReturnVisitor<out R>{
        R VisitULong(IRValueExpr<TULong> left, IRValueExpr<TULong> right);

        R VisitSLong(IRValueExpr<TSLong> left, IRValueExpr<TSLong> right);

        R VisitUInt(IRValueExpr<TUInt> left, IRValueExpr<TUInt> right);

        R VisitSInt(IRValueExpr<TSInt> left, IRValueExpr<TSInt> right);
    }

    public interface IUACReturnVisitor<out R> : IUACIntegralReturnVisitor<R> {
        R VisitDouble(IRValueExpr<TDouble> left, IRValueExpr<TDouble> right);

        R VisitFloat(IRValueExpr<TFloat> left, IRValueExpr<TFloat> right);
    }

    public sealed class UACDoubleReturn : UACReturn<TDouble> {
        public UACDoubleReturn(IRValueExpr<TDouble> left,
                               IRValueExpr<TDouble> right)
            : base(left, right) { }

        public override R Visit<R>(IUACReturnVisitor<R> visitor) {
            return visitor.VisitDouble(this.Left, this.Right);
        }
    }

    public sealed class UACFloatReturn : UACReturn<TFloat> {
        public UACFloatReturn(IRValueExpr<TFloat> left,
                              IRValueExpr<TFloat> right)
            : base(left, right) { }

        public override R Visit<R>(IUACReturnVisitor<R> visitor) {
            return visitor.VisitFloat(this.Left, this.Right);
        }
    }

    public sealed class UACULongReturn : UACIntegralReturn<TULong> {
        public UACULongReturn(IRValueExpr<TULong> left,
                              IRValueExpr<TULong> right)
            : base(left, right) { }

        public override R Visit<R>(IUACIntegralReturnVisitor<R> visitor) {
            return visitor.VisitULong(this.Left, this.Right);
        }

        public override R Visit<R>(IUACReturnVisitor<R> visitor) {
            return visitor.VisitULong(this.Left, this.Right);
        }
    }

    public sealed class UACSLongReturn : UACIntegralReturn<TSLong> {
        public UACSLongReturn(IRValueExpr<TSLong> left,
                              IRValueExpr<TSLong> right)
            : base(left, right) { }

        public override R Visit<R>(IUACIntegralReturnVisitor<R> visitor) {
            return visitor.VisitSLong(this.Left, this.Right);
        }

        public override R Visit<R>(IUACReturnVisitor<R> visitor) {
            return visitor.VisitSLong(this.Left, this.Right);
        }
    }

    public sealed class UACUIntRetrun : UACIntegralReturn<TUInt> {
        public UACUIntRetrun(IRValueExpr<TUInt> left, IRValueExpr<TUInt> right)
            : base(left, right) { }

        public override R Visit<R>(IUACIntegralReturnVisitor<R> visitor) {
            return visitor.VisitUInt(this.Left, this.Right);
        }

        public override R Visit<R>(IUACReturnVisitor<R> visitor) {
            return visitor.VisitUInt(this.Left, this.Right);
        }
    }

    public sealed class UACSIntReturn : UACIntegralReturn<TSInt> {
        public UACSIntReturn(IRValueExpr<TSInt> left, IRValueExpr<TSInt> right)
            : base(left, right) { }

        public override R Visit<R>(IUACIntegralReturnVisitor<R> visitor) {
            return visitor.VisitSInt(this.Left, this.Right);
        }

        public override R Visit<R>(IUACReturnVisitor<R> visitor) {
            return visitor.VisitSInt(this.Left, this.Right);
        }
    }

    /// <summary>
    /// Usual arithmetic conversion.
    /// </summary>
    /// <remarks>
    /// 1. If either operand is <c>long double</c>, the other operand is
    /// converted to <c>long double</c>. (We currently doesn't implement
    /// <c>long double</c>.)
    /// 
    /// 2. If either operand is <c>double</c>, the other operand is converted to
    /// <c>double</c>.
    /// 
    /// 3. If either operand is <c>float</c>, the other operand is converted to
    /// <c>float</c>.
    /// 
    /// 4. Perform Integer Promotion to both operands, then:
    /// 
    /// 4.1. If either operand is <c>unsigned long</c>, the other operand is
    ///      converted to <c>unsigned long</c>.
    /// 
    /// 4.2. If either operand is <c>signed long</c>, the other operand is
    ///      converted to <c>signed long</c>.
    /// 
    /// 4.3. If either operand is <c>unsigned int</c>, the other operand is
    ///      converted to <c>unsigned int</c>.
    /// 
    /// 4.4. If either operand is <c>signed int</c>, the other operand is
    ///      converted to <c>signed int</c>.
    /// </remarks>
    public static class UsualArithmeticConversion {
        public static IUACIntegralReturn Perform(IRValueExpr<IIntegralType> left,
                                                 IRValueExpr<IIntegralType> right) {

            var leftIPReturn = IntegralPromotion.Perform(left);
            var rightIPReturn = IntegralPromotion.Perform(right);

            if (leftIPReturn is IPULongReturn || rightIPReturn is IPULongReturn) {
                return new UACULongReturn(
                    leftIPReturn.Expr.CastTo(TULong.Get),
                    rightIPReturn.Expr.CastTo(TULong.Get)
                );
            }

            if (leftIPReturn is IPSLongReturn || rightIPReturn is IPSLongReturn) {
                return new UACSLongReturn(
                    leftIPReturn.Expr.CastTo(TSLong.Get),
                    rightIPReturn.Expr.CastTo(TSLong.Get)
                );
            }

            if (leftIPReturn is IPUIntReturn || rightIPReturn is IPUIntReturn) {
                return new UACUIntRetrun(
                    leftIPReturn.Expr.CastTo(TUInt.Get),
                    rightIPReturn.Expr.CastTo(TUInt.Get)
                );
            }

            if (leftIPReturn is IPSIntReturn || rightIPReturn is IPSIntReturn) {
                return new UACSIntReturn(
                    leftIPReturn.Expr.CastTo(TSInt.Get),
                    rightIPReturn.Expr.CastTo(TSInt.Get)
                );
            }

            throw new InvalidProgramException("Shouldn't reach here.");
        }

        public static IUACReturn Perform(IRValueExpr<IArithmeticType> left,
                                         IRValueExpr<IArithmeticType> right) {

            var leftType = left.Type;
            var rightType = right.Type;

            if (leftType is TDouble || rightType is TDouble) {
                return new UACDoubleReturn(
                    left.CastTo(TDouble.Get),
                    right.CastTo(TDouble.Get)
                );
            }

            if (leftType is TFloat || rightType is TFloat) {
                return new UACFloatReturn(
                    left.CastTo(TFloat.Get),
                    right.CastTo(TFloat.Get)
                );
            }

            return Perform(
                (IRValueExpr<IIntegralType>)left,
                (IRValueExpr<IIntegralType>)right
            );
        }
    }
}
