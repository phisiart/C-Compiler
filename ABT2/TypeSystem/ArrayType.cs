using System;
using ABT2.Environment;

namespace ABT2.TypeSystem {
    /// <summary>
    /// An array type.
    /// </summary>
    public sealed class ArrayType : IExprType {
        public ArrayType(IQualExprType elemQualType, Int64 numElems) {
            this.ElemQualType = elemQualType;
            this.NumElems = numElems;
        }

        public void Visit(IExprTypeVisitor visitor) {
            visitor.VisitArray(this);
        }

        public R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitArray(this);
        }

        public IQualExprType ElemQualType { get; }

        public Int64 NumElems { get; }

        public Int64 SizeOf(Env env) => this.ElemQualType.SizeOf(env) * this.NumElems;

        public Int64 Alignment(Env env) => this.ElemQualType.Alignment(env);
    }

    /// <summary>
    /// A cv-qualified array type.
    /// According to C89 standard - 3.5.3 Type qualifiers:
    ///  the qualifiers are applied to the element type.
    /// </summary>
    public sealed class QualArray : QualExprType<ArrayType> {
        public QualArray(ArrayType type)
            : base(type.ElemQualType.TypeQuals) {
            this.Type = type;
        }

        public override ArrayType Type { get; }
    }

    public sealed class IncompleteArrayType : IExprType {
        public IncompleteArrayType(IQualExprType elemQualType) {
            this.ElemQualType = elemQualType;
        }

        public void Visit(IExprTypeVisitor visitor) {
            visitor.VisitIncompleteArray(this);
        }

        public R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitIncompleteArray(this);
        }

        public IQualExprType ElemQualType { get; }

        public Int64 SizeOf(Env env) {
            throw new Exception("Can't get size of incomplete array");
        }

        public Int64 Alignment(Env env) => this.ElemQualType.Alignment(env);
    }
}

