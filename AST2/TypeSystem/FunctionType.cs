using System;
using System.Collections.Immutable;

namespace AST2 {
    public interface IFunctionType<out R> : IType where R : IType {
        IQualifiedType<R> ReturnType { get; }

        IImmutableList<IQualifiedType<IType>> ArgumentTypes { get; }

        Boolean HasVarArgs { get; }
    }

    public class FunctionType<R> : IFunctionType<R> where R : IType {
        public FunctionType(IQualifiedType<R> returnType, IImmutableList<IQualifiedType<IType>> argumentTypes, Boolean hasVarArgs) {
            this.ReturnType = returnType;
            this.ArgumentTypes = argumentTypes;
            this.HasVarArgs = hasVarArgs;
        }

        public UInt32 SizeOf {
            get { throw new InvalidOperationException("A function type is not allowed here."); }
        }

        public UInt32 Alignment {
            get { throw new InvalidOperationException("A function type is not allowed here."); }
        }

        public Boolean CanBeAssignedTo(IType type) => false;

        public IQualifiedType<R> ReturnType { get; }

        public IImmutableList<IQualifiedType<IType>> ArgumentTypes { get; }

        public Boolean HasVarArgs { get; }
    }
}