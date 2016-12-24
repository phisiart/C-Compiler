using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using ABT2.Environment;

namespace ABT2.TypeSystem {
    using IQualExprType = IQualExprType<IExprType>;

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
    public sealed class StructOrUnionType : IExprType {
        public StructOrUnionType(Int64 typeID, StructOrUnionKind kind) {
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

        public static Env<StructOrUnionType> CreateIncompleteStructType(Env env) {
            return env.NewIncompleteStructType();
        }

        public static Env<StructOrUnionType> CreateIncompleteUnionType(Env env) {
            return env.NewIncompleteUnionType();
        }

        public StructOrUnionKind Kind { get; }

        public Int64 TypeID { get; }

        public Int64 SizeOf {
            get {
                throw new InvalidOperationException("Must first get layout");
            }
        }

        public Int64 Alignment {
            get {
                throw new InvalidOperationException("Must first get layout");
            }
        }
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

        public static StructOrUnionLayout CreateStructLayout(IEnumerable<ICovariantTuple<IOption<String>, IQualExprType>> members) {
            var memberBuilder = ImmutableList.CreateBuilder<MemberEntry>();

            Int64 offset = 0;
            Int64 alignment = 0;
            foreach (var member in members) {
                var nameOpt = member.Item1;
                var qualType = member.Item2;

                offset = Utils.RoundUp(offset, qualType.Alignment);
                alignment = Math.Max(alignment, qualType.Alignment);

                if (nameOpt.IsSome) {
                    String name = nameOpt.Value;
                    var entry = new MemberEntry(offset, name, qualType);
                    memberBuilder.Add(entry);
                }

                offset += qualType.SizeOf;
            }

            Int64 sizeOf = Utils.RoundUp(offset, alignment);

            return new StructOrUnionLayout(StructOrUnionKind.Struct, sizeOf, alignment, memberBuilder.ToImmutable());
        }

        public static StructOrUnionLayout CreateUnionLayout(IEnumerable<ICovariantTuple<IOption<String>, IQualExprType>> members) {
            var memberBuilder = ImmutableList.CreateBuilder<MemberEntry>();

            Int64 sizeOf = 0;
            Int64 alignment = 0;
            foreach (var member in members) {
                var nameOpt = member.Item1;
                var qualType = member.Item2;

                sizeOf = Math.Max(sizeOf, qualType.SizeOf);
                alignment = Math.Max(alignment, qualType.Alignment);

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

    //public sealed class CompleteStructOrUnionType : IExprType {
    //    public CompleteStructOrUnionType(StructOrUnionKind kind, Int64 sizeOf, Int64 alignment, ImmutableList<MemberEntry> members) {
    //        this.Kind = kind;
    //        this.SizeOf = sizeOf;
    //        this.Alignment = alignment;
    //        this.Members = members;
    //    }

    //    public void Visit(IExprTypeVisitor visitor) {
    //        visitor.VisitStructOrUnion(this);
    //    }

    //    public R Visit<R>(IExprTypeVisitor<R> visitor) {
    //        return visitor.VisitStructOrUnion(this);
    //    }

    //    public StructOrUnionKind Kind { get; }

    //    public Int64 SizeOf { get; }

    //    public Int64 Alignment { get; }

    //    public ImmutableList<MemberEntry> Members { get; }

    //    public static CompleteStructOrUnionType CreateStructType(IEnumerable<ICovariantTuple<IOption<String>, IQualExprType>> members) {
    //        var memberBuilder = ImmutableList.CreateBuilder<MemberEntry>();

    //        Int64 offset = 0;
    //        Int64 alignment = 0;
    //        foreach (var member in members) {
    //            var nameOpt = member.Item1;
    //            var qualType = member.Item2;

    //            offset = Utils.RoundUp(offset, qualType.Alignment);
    //            alignment = Math.Max(alignment, qualType.Alignment);

    //            if (nameOpt.IsSome) {
    //                String name = nameOpt.Value;
    //                var entry = new MemberEntry(offset, name, qualType);
    //                memberBuilder.Add(entry);
    //            }

    //            offset += qualType.SizeOf;
    //        }

    //        Int64 sizeOf = Utils.RoundUp(offset, alignment);

    //        return new CompleteStructOrUnionType(StructOrUnionKind.Struct, sizeOf, alignment, memberBuilder.ToImmutable());
    //    }

    //    public static CompleteStructOrUnionType CreateUnionType(IEnumerable<ICovariantTuple<IOption<String>, IQualExprType>> members) {
    //        var memberBuilder = ImmutableList.CreateBuilder<MemberEntry>();

    //        Int64 sizeOf = 0;
    //        Int64 alignment = 0;
    //        foreach (var member in members) {
    //            var nameOpt = member.Item1;
    //            var qualType = member.Item2;

    //            sizeOf = Math.Max(sizeOf, qualType.SizeOf);
    //            alignment = Math.Max(alignment, qualType.Alignment);

    //            if (nameOpt.IsSome) {
    //                String name = nameOpt.Value;
    //                Int64 offset = 0;
    //                var entry = new MemberEntry(offset, name, qualType);
    //                memberBuilder.Add(entry);
    //            }
    //        }

    //        return new CompleteStructOrUnionType(StructOrUnionKind.Union, sizeOf, alignment, memberBuilder.ToImmutable());
    //    }
    //}
}
