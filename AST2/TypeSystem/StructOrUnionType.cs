using System;
using System.Collections.Immutable;

namespace AST2 {
    public class StructOrUnionType : IType {
        public static StructOrUnionType Create(Option<String> name) =>
            new StructOrUnionType(name);

        public static StructOrUnionType Create() =>
            Create(Option<String>.None);

        public static StructOrUnionType Create(String name) =>
            Create(Option.Some(name));

        private StructOrUnionType(Option<String> name) {
            this.Name = name;
            this.Layout = new SetOnce<StructOrUnionLayout>();
        }

        public Option<String> Name { get; }

        /// <summary>
        /// Two-step initialization of the content of a struct or union.
        /// </summary>
        public SetOnce<StructOrUnionLayout> Layout { get; }

        /// <summary>
        /// The size of a struct or union is determined by its content.
        /// </summary>
        public UInt32 SizeOf {
            get {
                if (!this.Layout.IsSet) {
                    throw new InvalidOperationException("This type is incomplete.");
                }
                return this.Layout.Value.SizeOf;
            }
        }

        /// <summary>
        /// The alignment of a struct or union is determined by its content.
        /// </summary>
        public UInt32 Alignment {
            get {
                if (!this.Layout.IsSet) {
                    throw new InvalidOperationException("This type is incomplete.");
                }
                return this.Layout.Value.Alignment;
            }
        }

        public Boolean CanBeAssignedTo(IType type) {
            var structOrUnionType = type as StructOrUnionType;
            if (structOrUnionType == null) {
                return false;
            }
            return structOrUnionType.Layout == this.Layout;
        }
    }

    public class StructOrUnionLayout {
        public static StructOrUnionLayout CreateStructLayout(ImmutableList<Tuple<Option<String>, IQualifiedType<IType>>> members) {
            UInt32 offset = 0;
            UInt32 alignment = 0;

            var memberBuilder = ImmutableList.CreateBuilder<IStructOrUnionMember<IType>>();
            foreach (var member in members) {
                Option<String> name = member.Item1;
                IQualifiedType<IType> type = member.Item2;

                offset = RoundUp(offset, type.Alignment);
                alignment = Math.Max(alignment, type.Alignment);

                memberBuilder.Add(StructOrUnionMember.Create(name, type, offset));

                offset += type.SizeOf;
            }

            return new StructOrUnionLayout(
                members : memberBuilder.ToImmutable(),
                sizeOf : RoundUp(offset, alignment),
                alignment : alignment
            );
        }

        public static StructOrUnionLayout CreateUnionLayout(ImmutableList<Tuple<Option<String>, IQualifiedType<IType>>> members) {
            UInt32 alignment = 0;
            UInt32 sizeOf = 0;

            var memberBuilder = ImmutableList.CreateBuilder<IStructOrUnionMember<IType>>();
            foreach (var member in members) {
                Option<String> name = member.Item1;
                IQualifiedType<IType> type = member.Item2;

                alignment = Math.Max(alignment, type.Alignment);
                sizeOf = Math.Max(sizeOf, type.SizeOf);

                memberBuilder.Add(StructOrUnionMember.Create(name, type, offset : 0));
            }

            return new StructOrUnionLayout(
                members : memberBuilder.ToImmutable(),
                sizeOf : RoundUp(sizeOf, alignment),
                alignment : alignment
            );
        }

        private StructOrUnionLayout(ImmutableList<IStructOrUnionMember<IType>> members, UInt32 sizeOf, UInt32 alignment) {
            this.Members = members;
            this.SizeOf = sizeOf;
            this.Alignment = alignment;
        }

        private static UInt32 RoundUp(UInt32 position, UInt32 alignment) =>
            (position + alignment - 1) & ~(alignment- 1);

        public ImmutableList<IStructOrUnionMember<IType>> Members { get; }

        public UInt32 SizeOf { get; }
        
        public UInt32 Alignment { get; }
    }

    public interface IStructOrUnionMember<out T> where T : IType {
        Option<String> Name { get; }
        IQualifiedType<T> Type { get; }
        UInt32 Offset { get; }
    }

    public class StructOrUnionMember<T> : IStructOrUnionMember<T> where T : IType {
        /// <summary>
        /// Create a member with optional name.
        /// </summary>
        public StructOrUnionMember(Option<String> name, IQualifiedType<T> type, UInt32 offset) {
            this.Name = name;
            this.Type = type;
            this.Offset = offset;
        }

        public Option<String> Name { get; }

        public IQualifiedType<T> Type { get; }

        public UInt32 Offset { get; }
    }

    public static class StructOrUnionMember {
        /// <summary>
        /// Create a member with optional name.
        /// </summary>
        public static StructOrUnionMember<T> Create<T>(Option<String> name, IQualifiedType<T> type, UInt32 offset) where T : IType =>
            new StructOrUnionMember<T>(name, type, offset);

        /// <summary>
        /// Create a member with name.
        /// </summary>
        public static StructOrUnionMember<T> Create<T>(String name, IQualifiedType<T> type, UInt32 offset) where T : IType =>
            Create(Option.Some(name), type, offset);

        /// <summary>
        /// Create a member without name.
        /// </summary>
        public static StructOrUnionMember<T> Create<T>(IQualifiedType<T> type, UInt32 offset) where T : IType =>
            Create(Option<String>.None, type, offset);
    }

    public interface IBitField<out T> : IStructOrUnionMember<T> where T : LongType { }

    public class SignedBitField : StructOrUnionMember<SignedLongType>, IBitField<SignedLongType> {
        public SignedBitField(Option<String> name, IQualifiedType<SignedLongType> type, UInt32 offset, UInt32 numBits)
            : base(name, type, offset) {
            this.NumBits = numBits;
        }

        public UInt32 NumBits { get; }
    }

    public class UnsignedBitField : StructOrUnionMember<UnsignedLongType>, IBitField<UnsignedLongType> {
        public UnsignedBitField(Option<String> name, IQualifiedType<UnsignedLongType> type, UInt32 offset, UInt32 numBits)
            : base(name, type, offset) {
            this.NumBits = numBits;
        }

        public UInt32 NumBits { get; }
    }
    
}