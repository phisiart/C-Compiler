using System;
using ABT2.TypeSystem;
using ABT2.Environment;

namespace ABT2.Expressions {
    public abstract class Assign<T> : IRValueExpr<T> where T : IExprType {
        public Assign(ILValueExpr<T> dest, IRValueExpr<T> src) {
            this.Dest = dest;
            this.Src = src;
        }

        public ILValueExpr<T> Dest { get; }

        public IRValueExpr<T> Src { get; }

        public Env Env => Src.Env;

        public T Type => Src.Type;

        public abstract void Visit(IRValueExprByTypeVisitor visitor);

        public abstract R Visit<R>(IRValueExprByTypeVisitor<R> visitor);
    }

    public static class Assign {
        public static IRValueExpr<IExprType> Create(ILValueExpr<IExprType> dst, IRValueExpr<IExprType> src) {
            // TODO: implement this
            throw new NotImplementedException();
        }
    }

    public sealed class SIntAssign : Assign<TSInt> {
        public SIntAssign(ILValueExpr<TSInt> dest, IRValueExpr<TSInt> src)
            : base(dest, src) { }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSInt(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSInt(this);
        }
    }

    public sealed class UIntAssign : Assign<TUInt> {
        public UIntAssign(ILValueExpr<TUInt> dest, IRValueExpr<TUInt> src)
            : base(dest, src) { }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitUInt(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitUInt(this);
        }
    }

    public sealed class SLongAssign : Assign<TSLong> {
        public SLongAssign(ILValueExpr<TSLong> dest, IRValueExpr<TSLong> src)
            : base(dest, src) { }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSLong(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSLong(this);
        }
    }

    public sealed class ULongAssign : Assign<TULong> {
        public ULongAssign(ILValueExpr<TULong> dest, IRValueExpr<TULong> src)
            : base(dest, src) { }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitULong(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitULong(this);
        }
    }

    public sealed class SShortAssign : Assign<TSShort> {
        public SShortAssign(ILValueExpr<TSShort> dest, IRValueExpr<TSShort> src)
            : base(dest, src) { }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSShort(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSShort(this);
        }
    }

    public sealed class UShortAssign : Assign<TUShort> {
        public UShortAssign(ILValueExpr<TUShort> dest, IRValueExpr<TUShort> src)
            : base(dest, src) { }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitUShort(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitUShort(this);
        }
    }

    public sealed class SCharAssign : Assign<TSChar> {
        public SCharAssign(ILValueExpr<TSChar> dest, IRValueExpr<TSChar> src)
            : base(dest, src) { }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSChar(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSChar(this);
        }
    }

    public sealed class UCharAssign : Assign<TUChar> {
        public UCharAssign(ILValueExpr<TUChar> dest, IRValueExpr<TUChar> src)
            : base(dest, src) { }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitUChar(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitUChar(this);
        }
    }

    public sealed class FloatAssign : Assign<TFloat> {
        public FloatAssign(ILValueExpr<TFloat> dest, IRValueExpr<TFloat> src)
            : base(dest, src) { }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitFloat(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitFloat(this);
        }
    }

    public sealed class DoubleAssign : Assign<TDouble> {
        public DoubleAssign(ILValueExpr<TDouble> dest, IRValueExpr<TDouble> src)
            : base(dest, src) { }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitDouble(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitDouble(this);
        }
    }

    public sealed class PointerAssign : Assign<TPointer> {
        public PointerAssign(ILValueExpr<TPointer> dest, IRValueExpr<TPointer> src)
            : base(dest, src) { }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitPointer(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitPointer(this);
        }
    }

    public sealed class StructOrUnionAssign : Assign<StructOrUnionType> {
        public StructOrUnionAssign(ILValueExpr<StructOrUnionType> dest, IRValueExpr<StructOrUnionType> src)
            : base(dest, src) { }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitStructOrUnion(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitStructOrUnion(this);
        }
    }
}
