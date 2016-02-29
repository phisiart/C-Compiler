using System;

namespace AST2 {
    public interface ITypeCast<out T> : IExpr<T> where T : IType {
    }

    public class CharExtend : ITypeCast<SignedLongType> {
        public IQualifiedType<SignedLongType> Type => _type;

        private static readonly QualifiedType<SignedLongType> _type =
            QualifiedType.Const(SignedLongType.Instance);
    }

}