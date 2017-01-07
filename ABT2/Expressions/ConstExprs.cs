using System;
using ABT2.TypeSystem;
using ABT2.Environment;

namespace ABT2.Expressions {
    
    public interface IConstExpr : IRValueExpr { }

    public interface IConstExpr<out T> : IConstExpr, IRValueExpr<T> where T : IExprType { }

    public abstract class ConstExpr<T> : RValueExpr<T>, IConstExpr<T> where T : IExprType {
        protected ConstExpr(Env env) {
            this.Env = env;
        }

        public override sealed Env Env { get; }
    }

    public sealed class ConstSChar : ConstExpr<TSChar> {
        public ConstSChar(Int64 value, Env env): base(env) {
            this.Value = value;
        }

        public Int64 Value { get; }

        public override TSChar Type => TSChar.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitConstSChar(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitConstSChar(this);
        }
    }

    public sealed class ConstUChar : ConstExpr<TUChar> {
        public ConstUChar(UInt64 value, Env env): base(env) {
            this.Value = value;
        }

        public UInt64 Value { get; }

        public override TUChar Type => TUChar.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitConstUChar(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitConstUChar(this);
        }
    }

    public sealed class ConstSShort : ConstExpr<TSShort> {
        public ConstSShort(Int64 value, Env env): base(env) {
            this.Value = value;
        }

        public Int64 Value { get; }

        public override TSShort Type => TSShort.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitConstSShort(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitConstSShort(this);
        }
    }

    public sealed class ConstUShort : ConstExpr<TUShort> {
        public ConstUShort(UInt64 value, Env env): base(env) {
            this.Value = value;
        }

        public UInt64 Value { get; }

        public override TUShort Type => TUShort.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitConstUShort(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitConstUShort(this);
        }
    }

    public sealed class ConstSInt : ConstExpr<TSInt> {
        public ConstSInt(Int64 value, Env env): base(env) {
            this.Value = value;
        }

        public Int64 Value { get; }

        public override TSInt Type => TSInt.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitConstSInt(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitConstSInt(this);
        }
    }

    public sealed class ConstUInt : ConstExpr<TUInt> {
        public ConstUInt(UInt64 value, Env env): base(env) {
            this.Value = value;
        }

        public UInt64 Value { get; }

        public override TUInt Type => TUInt.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitConstUInt(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitConstUInt(this);
        }
    }

    public sealed class ConstSLong : ConstExpr<TSLong> {
        public ConstSLong(Int64 value, Env env): base(env) {
            this.Value = value;
        }

        public Int64 Value { get; }

        public override TSLong Type => TSLong.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitConstSLong(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitConstSLong(this);
        }
    }

    public sealed class ConstULong : ConstExpr<TULong> {
        public ConstULong(UInt64 value, Env env): base(env) {
            this.Value = value;
        }

        public UInt64 Value { get; }

        public override TULong Type => TULong.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitConstULong(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitConstULong(this);
        }
    }

    public sealed class ConstFloat : ConstExpr<TFloat> {
        public ConstFloat(Double value, Env env): base(env) {
            this.Value = value;
        }

        public Double Value { get; }

        public override TFloat Type => TFloat.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitConstFloat(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitConstFloat(this);
        }
    }

    public sealed class ConstDouble : ConstExpr<TDouble> {
        public ConstDouble(Double value, Env env): base(env) {
            this.Value = value;
        }

        public Double Value { get; }

        public override TDouble Type => TDouble.Get;

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitConstDouble(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitConstDouble(this);
        }
    }

    public sealed class ConstPointer : ConstExpr<TPointer> {
        public ConstPointer(UInt64 value, TPointer type, Env env): base(env) {
            this.Value = value;
            this.Type = type;
        }

        public UInt64 Value { get; }

        public override TPointer Type { get; }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            // TODO: should I add VisitConstPointer?
            visitor.VisitPointer(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            // TODO: should I add VisitConstPointer?
            return visitor.VisitPointer(this);
        }
    }
}
