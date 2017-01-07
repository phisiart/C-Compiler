using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using ABT2.Environment;

namespace ABT2.TypeSystem {
    public sealed class ArgEntry {
        public ArgEntry(IQualExprType qualType, Int64 offset) {
            this.QualType = qualType;
            this.Offset = offset;
        }

        public IQualExprType QualType { get; }

        public Int64 Offset;
    }

    /// <summary>
    /// A function type.
    /// There are no cv-qualified function types.
    /// </summary>
    public sealed class TFunction : IExprType {
        public TFunction(IQualExprType returnQualType, ImmutableList<ArgEntry> args, Boolean hasVarArgs, Int64 argsSize) {
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

        public Int64 SizeOf(Env env) => PlatformSpecificConstants.SizeOfLong;

        public Int64 Alignment(Env env) => PlatformSpecificConstants.AlignmentOfLong;

        public IQualExprType ReturnQualType { get; }

        public ImmutableList<ArgEntry> Args { get; }

        public Boolean HasVarArgs { get; }

        public Int64 ArgsSize { get; }

        public static TFunction Create(IQualExprType returnQualType,
                                       IEnumerable<IQualExprType> argQualTypes,
                                       Boolean hasVarArgs,
                                       Env env) {
            
            var argsBuilder = ImmutableList.CreateBuilder<ArgEntry>();

            Int64 offset = 0;
            Int64 alignment = PlatformSpecificConstants.AlignmentOfInt;
            foreach (var argQualType in argQualTypes) {
                offset = Utils.RoundUp(offset, argQualType.Alignment(env));
                alignment = Math.Max(alignment, argQualType.Alignment(env));

                var argEntry = new ArgEntry(argQualType, offset);
                argsBuilder.Add(argEntry);

                offset += argQualType.SizeOf(env);
            }

            Int64 argsSize = Utils.RoundUp(offset, alignment);

            return new TFunction(returnQualType, argsBuilder.ToImmutable(), hasVarArgs, argsSize);
        }
    }

    public sealed class QualFunction : QualExprType<TFunction> {
        public QualFunction(TFunction type)
            : base(type.ReturnQualType.TypeQuals) {
            this.Type = type;
        }

        public override TFunction Type { get; }
    }
}
