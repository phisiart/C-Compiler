using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using ABT2.Environment;

namespace ABT2.TypeSystem {
    public sealed class MemberEntry {
        public MemberEntry(Int64 offset, String name, IQualExprType type) {
            this.Offset = offset;
            this.Name = name;
            this.QualType = type;
        }

        public Int64 Offset { get; }

        public String Name { get; }

        public IQualExprType QualType { get; }
    }

    public enum StructOrUnionKind {
        Struct,
        Union
    }

    // Design for incomplete struct/union:
    // Each struct or union has a unique ID.
    public sealed class TStructOrUnion : IExprType {
        public TStructOrUnion(Int64 typeID, StructOrUnionKind kind) {
            this.TypeID = typeID;
            this.Kind = kind;
        }

        public void Visit(IExprTypeVisitor visitor) {
            visitor.VisitStructOrUnion(this);
        }

        public R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitStructOrUnion(this);
        }

        public StructOrUnionLayout GetLayout(Env env) {
            var layoutOpt = GetLayoutOpt(env);

            if (layoutOpt.IsNone) {
                throw new InvalidOperationException("Incomplete struct/union: cannot find layout");
            }

            return layoutOpt.Value;
        }

        public IOption<StructOrUnionLayout> GetLayoutOpt(Env env) {
            return env.GetStructOrUnionLayoutOpt(this);
        }

        public Boolean IsComplete(Env env) {
            var layoutOpt = GetLayoutOpt(env);
            return layoutOpt.IsSome;
        }

        public static Env<TStructOrUnion> CreateIncompleteStructType(Env env) {
            return env.NewIncompleteStructType();
        }

        public static Env<TStructOrUnion> CreateIncompleteUnionType(Env env) {
            return env.NewIncompleteUnionType();
        }

        public StructOrUnionKind Kind { get; }

        public Int64 TypeID { get; }

        public Int64 SizeOf(Env env) {
            var layout = env.GetStructOrUnionLayoutOpt(this);
            if (layout.IsSome) {
                return layout.Value.SizeOf;
            } else {
                throw new InvalidProgramException("Incomplete struct/union doesn't have sizeof.");
            }
        }

        public Int64 Alignment(Env env) {
            var layout = env.GetStructOrUnionLayoutOpt(this);
            if (layout.IsSome) {
                return layout.Value.Alignment;
            } else {
                throw new InvalidProgramException("Incomplete struct/union doesn't have alignment.");
            }
        }
    }

    /// <summary>
    /// A cv-qualified struct or union type.
    /// </summary>
    public sealed class QualStructOrUnion : QualExprType<TStructOrUnion> {
        public QualStructOrUnion(TypeQuals typeQuals, TStructOrUnion type)
            : base(typeQuals) {
            this.Type = type;
        }

        public override TStructOrUnion Type { get; }
    }

    public sealed class StructOrUnionLayout {
        public StructOrUnionLayout(StructOrUnionKind kind, Int64 sizeOf, Int64 alignment, ImmutableList<MemberEntry> members) {
            this.Kind = kind;
            this.SizeOf = sizeOf;
            this.Alignment = alignment;
            this.Members = members;
        }

        public StructOrUnionKind Kind { get; }

        public Int64 SizeOf { get; }

        public Int64 Alignment { get; }

        public ImmutableList<MemberEntry> Members { get; }

        public static StructOrUnionLayout CreateStructLayout(
            IEnumerable<ICovariantTuple<IOption<String>, IQualExprType>> members,
            Env env) {

            var memberBuilder = ImmutableList.CreateBuilder<MemberEntry>();

            Int64 offset = 0;
            Int64 alignment = 0;
            foreach (var member in members) {
                var nameOpt = member.Item1;
                var qualType = member.Item2;

                offset = Utils.RoundUp(offset, qualType.Alignment(env));
                alignment = Math.Max(alignment, qualType.Alignment(env));

                if (nameOpt.IsSome) {
                    String name = nameOpt.Value;
                    var entry = new MemberEntry(offset, name, qualType);
                    memberBuilder.Add(entry);
                }

                offset += qualType.SizeOf(env);
            }

            Int64 sizeOf = Utils.RoundUp(offset, alignment);

            return new StructOrUnionLayout(StructOrUnionKind.Struct, sizeOf, alignment, memberBuilder.ToImmutable());
        }

        public static StructOrUnionLayout CreateUnionLayout(
            IEnumerable<ICovariantTuple<IOption<String>, IQualExprType>> members,
            Env env) {

            var memberBuilder = ImmutableList.CreateBuilder<MemberEntry>();

            Int64 sizeOf = 0;
            Int64 alignment = 0;
            foreach (var member in members) {
                var nameOpt = member.Item1;
                var qualType = member.Item2;

                sizeOf = Math.Max(sizeOf, qualType.SizeOf(env));
                alignment = Math.Max(alignment, qualType.Alignment(env));

                if (nameOpt.IsSome) {
                    String name = nameOpt.Value;
                    Int64 offset = 0;
                    var entry = new MemberEntry(offset, name, qualType);
                    memberBuilder.Add(entry);
                }
            }

            return new StructOrUnionLayout(StructOrUnionKind.Union, sizeOf, alignment, memberBuilder.ToImmutable());
        }
    }
}
