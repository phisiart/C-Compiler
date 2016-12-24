using System;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace ABT2.TypeSystem {
    using IQualExprType = IQualExprType<IExprType>;

    public sealed class ArgEntry {
        public ArgEntry(IQualExprType qualType, Int64 offset) {
            this.QualType = qualType;
            this.Offset = offset;
        }

        public IQualExprType QualType { get; }

        public Int64 Offset;
    }

    public sealed class FunctionType : IExprType {
        public FunctionType(IQualExprType returnQualType, ImmutableList<ArgEntry> args, Boolean hasVarArgs, Int64 argsSize) {
            this.ReturnQualType = returnQualType;
            this.Args = args;
            this.HasVarArgs = hasVarArgs;
            this.ArgsSize = argsSize;
        }

        public void Visit(IExprTypeVisitor visitor) {
            visitor.VisitFunction(this);
        }

        public R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitFunction(this);
        }

        public Int64 SizeOf => PlatformSpecificConstants.SizeOfLong;

        public Int64 Alignment => PlatformSpecificConstants.AlignmentOfLong;

        public IQualExprType ReturnQualType { get; }

        public ImmutableList<ArgEntry> Args { get; }

        public Boolean HasVarArgs { get; }

        public Int64 ArgsSize { get; }

        public static FunctionType Create(IQualExprType returnQualType,
                                          IEnumerable<IQualExprType> argQualTypes,
                                          Boolean hasVarArgs) {
            
            var argsBuilder = ImmutableList.CreateBuilder<ArgEntry>();

            Int64 offset = 0;
            Int64 alignment = PlatformSpecificConstants.AlignmentOfInt;
            foreach (var argQualType in argQualTypes) {
                offset = Utils.RoundUp(offset, argQualType.Alignment);
                alignment = Math.Max(alignment, argQualType.Alignment);

                ArgEntry argEntry = new ArgEntry(argQualType, offset);
                argsBuilder.Add(argEntry);

                offset += argQualType.SizeOf;
            }

            Int64 argsSize = Utils.RoundUp(offset, alignment);

            return new FunctionType(returnQualType, argsBuilder.ToImmutable(), hasVarArgs, argsSize);
        }
    }
}
