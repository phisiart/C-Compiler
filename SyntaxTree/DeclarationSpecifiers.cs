using System;
using System.Collections.Immutable;
using System.Linq;
using AST;

namespace SyntaxTree {
    using static SemanticAnalysis;

    /// <summary>
    /// storage-class-specifier
    ///   : auto | register | static | extern | typedef
    /// </summary>
    public enum StorageClsSpec {
        NULL,
        AUTO,
        REGISTER,
        STATIC,
        EXTERN,
        TYPEDEF
    }

    /// <summary>
    /// type-specifier
    ///   : void      --+
    ///   | char        |
    ///   | short       |
    ///   | int         |
    ///   | long        +--> Basic type specifier
    ///   | float       |
    ///   | double      |
    ///   | signed      |
    ///   | unsigned  --+
    ///   | struct-or-union-specifier
    ///   | enum-specifier
    ///   | typedef-name
    /// </summary>
    public abstract class TypeSpec : ISyntaxTreeNode {
        public enum Kind {
            NON_BASIC,
            VOID,
            CHAR,
            SHORT,
            INT,
            LONG,
            FLOAT,
            DOUBLE,
            SIGNED,
            UNSIGNED
        }
        
        [SemantMethod]
        public abstract ISemantReturn<ExprType> GetExprType(Env env);

        public abstract Kind kind { get; }
    }

    public sealed class BasicTypeSpec : TypeSpec {
        public BasicTypeSpec(Kind kind) {
            this.kind = kind;
        }

        public override Kind kind { get; }
        
        [SemantMethod]
        public override ISemantReturn<ExprType> GetExprType(Env env) {
            throw new InvalidProgramException();
        }
    }

    public abstract class NonBasicTypeSpec : TypeSpec {
        public override Kind kind => Kind.NON_BASIC;
    }

    /// <summary>
    /// typedef-name
    ///   : identifier
    /// </summary>
    public sealed class TypedefName : NonBasicTypeSpec {
        private TypedefName(String name) {
            this.Name = name;
        }

        public static TypedefName Create(String name) =>
            new TypedefName(name);
        
        [SemantMethod]
        public override ISemantReturn<ExprType> GetExprType(Env env) {
            var entryOpt = env.Find(this.Name);
            if (entryOpt.IsNone) {
                throw new InvalidProgramException("This should not pass the parser.");
            }
            var entry = entryOpt.Value;
            if (entry.kind != Env.EntryKind.TYPEDEF) {
                throw new InvalidProgramException("This should not pass the parser.");
            }
            return SemantReturn.Create(env, entry.type);
        }

        public String Name { get; }
    }

    /// <summary>
    /// type-qualifier
    ///   : const | volatile
    /// </summary>
    public enum TypeQual {
        NULL,
        CONST,
        VOLATILE
    }

    /// <summary>
    /// specifier-qualifier-list
    ///   : [ type-specifier | type-qualifier ]+
    /// </summary>
    public class SpecQualList : ISyntaxTreeNode {
        protected SpecQualList(ImmutableList<TypeSpec> typeSpecs, ImmutableList<TypeQual> typeQuals) {
            this.TypeSpecs = typeSpecs;
            this.TypeQuals = typeQuals;
        }

        public static SpecQualList Create(ImmutableList<TypeSpec> typeSpecs, ImmutableList<TypeQual> typeQuals) =>
            new SpecQualList(typeSpecs, typeQuals);

        public static SpecQualList Empty { get; } =
            Create(ImmutableList<TypeSpec>.Empty, ImmutableList<TypeQual>.Empty);

        public static SpecQualList Add(SpecQualList list, TypeSpec typeSpec) =>
            Create(list.TypeSpecs.Add(typeSpec), list.TypeQuals);

        public static SpecQualList Add(SpecQualList list, TypeQual typeQual) =>
            Create(list.TypeSpecs, list.TypeQuals.Add(typeQual));

        public ImmutableList<TypeSpec> TypeSpecs { get; }
        public ImmutableList<TypeQual> TypeQuals { get; }

        private static ImmutableDictionary<ImmutableSortedSet<TypeSpec.Kind>, ExprType> BasicTypeSpecLookupTable { get; }

        static SpecQualList() {

            BasicTypeSpecLookupTable = ImmutableDictionary<ImmutableSortedSet<TypeSpec.Kind>, ExprType>.Empty
                
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.VOID), new VoidType())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.CHAR), new CharType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.CHAR, TypeSpec.Kind.SIGNED), new CharType())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.CHAR, TypeSpec.Kind.UNSIGNED), new UCharType())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SHORT), new ShortType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SHORT, TypeSpec.Kind.SIGNED), new ShortType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SHORT, TypeSpec.Kind.INT), new ShortType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SHORT, TypeSpec.Kind.INT, TypeSpec.Kind.SIGNED), new ShortType())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SHORT, TypeSpec.Kind.UNSIGNED), new UShortType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SHORT, TypeSpec.Kind.INT, TypeSpec.Kind.UNSIGNED), new UShortType())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.INT), new LongType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.INT, TypeSpec.Kind.SIGNED), new LongType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.INT, TypeSpec.Kind.LONG), new LongType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.INT, TypeSpec.Kind.SIGNED, TypeSpec.Kind.LONG), new LongType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SIGNED), new LongType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SIGNED, TypeSpec.Kind.LONG), new LongType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.LONG), new LongType())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.UNSIGNED), new ULongType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.INT), new ULongType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.LONG), new ULongType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.INT, TypeSpec.Kind.LONG), new ULongType())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.FLOAT), new FloatType())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.DOUBLE), new DoubleType())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.DOUBLE, TypeSpec.Kind.LONG), new DoubleType())
            ;
        }

        /// <summary>
        /// Get qualified type, based on type specifiers & type qualifiers.
        /// </summary>
        [SemantMethod]
        public ISemantReturn<ExprType> GetExprType(Env env) {
            Boolean isConst = this.TypeQuals.Contains(TypeQual.CONST);
            Boolean isVolatile = this.TypeQuals.Contains(TypeQual.VOLATILE);

            // If no type specifier is given, assume long type.
            if (this.TypeSpecs.IsEmpty) {
                return SemantReturn.Create(env, new LongType(isConst, isVolatile));
            }

            // If every type specifier is basic, go to the lookup table.
            if (this.TypeSpecs.All(typeSpec => typeSpec.kind != TypeSpec.Kind.NON_BASIC)) {
                var basicTypeSpecKinds =
                    this.TypeSpecs
                    .ConvertAll(typeSpec => typeSpec.kind)
                    .Distinct()
                    .ToImmutableSortedSet();

                foreach (var pair in BasicTypeSpecLookupTable) {
                    if (pair.Key.SetEquals(basicTypeSpecKinds)) {
                        return SemantReturn.Create(env, pair.Value);
                    }
                }

                throw new InvalidOperationException("Invalid type specifier set.");
            }

            // If there is a non-basic type specifier, semant it.
            if (this.TypeSpecs.Count != 1) {
                throw new InvalidOperationException("Invalid type specifier set.");
            }

            var type = Semant(this.TypeSpecs[0].GetExprType, ref env);
            return SemantReturn.Create(env, type.GetQualifiedType(isConst, isVolatile));
        }
    }

    /// <summary>
    /// declaration-specifiers
    ///   : [ storage-class-specifier | type-specifier | type-qualifier ]+
    /// </summary>
    public sealed class DeclnSpecs : SpecQualList {
        private DeclnSpecs(ImmutableList<StorageClsSpec> storageClsSpecs, ImmutableList<TypeSpec> typeSpecs, ImmutableList<TypeQual> typeQuals)
            : base(typeSpecs, typeQuals) {
            this.StorageClsSpecs = storageClsSpecs;
        }

        public static DeclnSpecs Create(ImmutableList<StorageClsSpec> storageClsSpecs, ImmutableList<TypeSpec> typeSpecs, ImmutableList<TypeQual> typeQuals) =>
            new DeclnSpecs(storageClsSpecs, typeSpecs, typeQuals);

        public new static DeclnSpecs Empty { get; } = Create(ImmutableList<StorageClsSpec>.Empty, ImmutableList<TypeSpec>.Empty, ImmutableList<TypeQual>.Empty);

        public static DeclnSpecs Add(DeclnSpecs declnSpecs, StorageClsSpec storageClsSpec) =>
            Create(declnSpecs.StorageClsSpecs.Add(storageClsSpec), declnSpecs.TypeSpecs, declnSpecs.TypeQuals);

        public static DeclnSpecs Add(DeclnSpecs declnSpecs, TypeSpec typeSpec) =>
            Create(declnSpecs.StorageClsSpecs, declnSpecs.TypeSpecs.Add(typeSpec), declnSpecs.TypeQuals);

        public static DeclnSpecs Add(DeclnSpecs declnSpecs, TypeQual typeQual) =>
            Create(declnSpecs.StorageClsSpecs, declnSpecs.TypeSpecs, declnSpecs.TypeQuals.Add(typeQual));

        [SemantMethod]
        public StorageClass GetStorageClass() {
            if (this.StorageClsSpecs.Count == 0) {
                return StorageClass.AUTO;
            }

            if (this.StorageClsSpecs.Count == 1) {
                switch (this.StorageClsSpecs[0]) {
                    case StorageClsSpec.AUTO:
                    case StorageClsSpec.NULL:
                    case StorageClsSpec.REGISTER:
                        return StorageClass.AUTO;

                    case StorageClsSpec.EXTERN:
                        return StorageClass.EXTERN;

                    case StorageClsSpec.STATIC:
                        return StorageClass.STATIC;

                    case StorageClsSpec.TYPEDEF:
                        return StorageClass.TYPEDEF;

                    default:
                        throw new InvalidOperationException();
                }
            }

            throw new InvalidOperationException("Multiple storage class specifiers.");
        }

        public ImmutableList<StorageClsSpec> StorageClsSpecs { get; }

        /// <summary>
        /// Only used by the parser.
        /// </summary>
        [Obsolete]
        public bool IsTypedef() => this.StorageClsSpecs.Contains(StorageClsSpec.TYPEDEF);
    }

    /// <summary>
    /// struct-or-union
    ///   : struct | union
    /// </summary>
    public enum StructOrUnion {
        STRUCT,
        UNION
    }

    /// <summary>
    /// struct-or-union-specifier
    /// </summary>
    public sealed class StructOrUnionSpec : NonBasicTypeSpec {
        private StructOrUnionSpec(StructOrUnion structOrUnion, Option<String> name, Option<ImmutableList<StructDecln>> memberDeclns) {
            this.StructOrUnion = structOrUnion;
            this.Name = name;
            this.MemberDeclns = memberDeclns;
        }

        [Obsolete]
        public static StructOrUnionSpec Create(StructOrUnion structOrUnion, Option<String> name, Option<ImmutableList<StructDecln>> memberDeclns) =>
            new StructOrUnionSpec(structOrUnion, name, memberDeclns);

        public static StructOrUnionSpec Create(StructOrUnion structOrUnion, Option<String> name, ImmutableList<StructDecln> memberDeclns) =>
            new StructOrUnionSpec(structOrUnion, name, Option.Some(memberDeclns));

        public static StructOrUnionSpec Create(StructOrUnion structOrUnion, String name) =>
            new StructOrUnionSpec(structOrUnion, Option.Some(name), Option<ImmutableList<StructDecln>>.None);

        public StructOrUnion StructOrUnion { get; }
        public Option<String> Name { get; }
        public Option<ImmutableList<StructDecln>> MemberDeclns { get; }

        [SemantMethod]
        public ISemantReturn<ImmutableList<Tuple<Option<String>, ExprType>>> GetMembers(Env env, ImmutableList<StructDecln> memberDeclns) {
            var result = memberDeclns.Aggregate(ImmutableList<Tuple<Option<String>, ExprType>>.Empty, (acc, decln) => acc.AddRange(Semant(decln.GetMemberDeclns, ref env))
            );

            return SemantReturn.Create(env, result);
        }

        //            +----------------------------------------------+-------------------------------------------+
        //            |                   members                    |                 members X                 |
        // +----------+----------------------------------------------+-------------------------------------------+
        // |          | May have incomplete type in current scope.   |                                           |
        // |   name   | 1. Get/New incomplete type in current scope; | Name must appear in previous environment. |
        // |          | 2. Fill up with members.                     |                                           |
        // +----------+----------------------------------------------+-------------------------------------------+
        // |  name X  | Fill up with members.                        |                     X                     |
        // +----------+----------------------------------------------+-------------------------------------------+
        [SemantMethod]
        public override ISemantReturn<ExprType> GetExprType(Env env) {

            StructOrUnionType type;

            // If no members provided, then we need to find the type in the current environment.
            if (this.MemberDeclns.IsNone) {

                if (this.Name.IsNone) {
                    throw new InvalidProgramException("This should not pass the parser");
                }

                var name = this.Name.Value;
                var typeName = (this.StructOrUnion == StructOrUnion.STRUCT ? "struct" : "union") + $" {name}";

                // Try to find type name in the current environment.
                var entryOpt = env.Find(typeName);

                // If name not found: create an incomplete type and add it into the environment.
                if (entryOpt.IsNone) {
                    type = StructOrUnionType.CreateIncompleteType(this.StructOrUnion, name);
                    env = env.PushEntry(Env.EntryKind.TYPEDEF, typeName, type);
                    return SemantReturn.Create(env, type);
                }

                // If name found: fetch it.
                if (entryOpt.Value.kind != Env.EntryKind.TYPEDEF) {
                    throw new InvalidProgramException("A struct or union in env that is not typedef? This should not appear.");
                }

                return SemantReturn.Create(env, entryOpt.Value.type);

            }

            // If members are provided, the user is trying to define a new struct/union.

            if (this.Name.IsSome) {

                var name = this.Name.Value;
                var typeName = (this.StructOrUnion == StructOrUnion.STRUCT ? "struct" : "union") + $" {name}";

                // Try to find type name in the current environment.
                // Notice we need to search the current **scope** only.
                var entryOpt = env.FindInCurrentScope(typeName);

                // If name not found: create an incomplete type and add it into the environment.
                if (entryOpt.IsNone) {
                    type = StructOrUnionType.CreateIncompleteType(this.StructOrUnion, name);
                    env = env.PushEntry(Env.EntryKind.TYPEDEF, typeName, type);
                } else {
                    if (entryOpt.Value.kind != Env.EntryKind.TYPEDEF) {
                        throw new InvalidProgramException(
                            "A struct or union in env that is not typedef? This should not appear.");
                    }

                    type = entryOpt.Value.type as StructOrUnionType;
                    if (type == null) {
                        throw new InvalidProgramException(
                            $"{typeName} is not a struct or union? This should not appear.");
                    }
                }

                // Current type mustn't be already complete.
                if (type.IsComplete) {
                    throw new InvalidOperationException($"Redifinition of {typeName}");
                }

            } else {
                var typeName = (this.StructOrUnion == StructOrUnion.STRUCT ? "struct" : "union") + " <unnamed>";
                type = StructOrUnionType.CreateIncompleteType(this.StructOrUnion, typeName);
            }

            var members = Semant(GetMembers, this.MemberDeclns.Value, ref env);
            type.Define(this.StructOrUnion, members);

            return SemantReturn.Create(env, type);
        }
    }

    /// <summary>
    /// enum-specifier
    ///   : enum [identifier]? '{' enumerator-list '}'
    ///   | enum identifier
    /// 
    /// enumerator-list
    ///   : enumerator [ ',' enumerator ]*
    /// </summary>
    public sealed class EnumSpec : NonBasicTypeSpec {
        private EnumSpec(Option<String> name, Option<ImmutableList<Enumr>> enumrs) {
            this.Name = name;
            this.Enumrs = enumrs;
        }

        private static EnumSpec Create(Option<String> name, Option<ImmutableList<Enumr>> enumrs) =>
            new EnumSpec(name, enumrs);

        public static EnumSpec Create(Option<String> name, ImmutableList<Enumr> enumrs) =>
            Create(name, Option.Some(enumrs));

        public static EnumSpec Create(String name) =>
            Create(Option.Some(name), Option<ImmutableList<Enumr>>.None);

        [SemantMethod]
        public override ISemantReturn<ExprType> GetExprType(Env env) {
            if (this.Enumrs.IsNone) {
                // If no enumerators provided: must find enum type in the current environment.

                if (this.Name.IsNone) {
                    throw new InvalidProgramException("This should not pass the parser.");
                }

                var name = this.Name.Value;
                var entryOpt = env.Find($"enum {name}");

                if (entryOpt.IsNone || entryOpt.Value.kind != Env.EntryKind.TYPEDEF) {
                    throw new InvalidOperationException($"enum {name} has not been defined.");
                }

                return SemantReturn.Create(env, new LongType());
            }

            // If enumerators are provided: add names to environment
            Int32 offset = 0;
            foreach (var enumr in this.Enumrs.Value) {

                if (enumr.Init.IsSome) {
                    // If the user provides an initialization value, use it.
                    var init = SemantExpr(enumr.Init.Value, ref env);
                    init = AST.TypeCast.MakeCast(init, new LongType());
                    if (!init.IsConstExpr) {
                        throw new InvalidOperationException("Enumerator initialization must have a constant value.");
                    }
                    offset = ((ConstLong)init).value;
                }

                env = env.PushEnum(enumr.Name, new LongType(), offset);

                offset++;
            }

            // If the user provides a name to the enum, add it to the environment.
            if (this.Name.IsSome) {
                var typeName = $"enum {this.Name.Value}";

                if (env.FindInCurrentScope(typeName).IsSome) {
                    throw new InvalidOperationException($"{typeName} is already defined.");
                }
                env = env.PushEntry(Env.EntryKind.TYPEDEF, typeName, new LongType());
            }

            return SemantReturn.Create(env, new LongType());
        }

        public Option<String> Name { get; }
        public Option<ImmutableList<Enumr>> Enumrs { get; }
    }

    /// <summary>
    /// enumerator
    ///   : enumeration-constant [ '=' constant-expression ]?
    /// 
    /// enumeration-constant
    ///   : identifier
    /// </summary>
    public sealed class Enumr : ISyntaxTreeNode {
        private Enumr(String name, Option<Expr> init) {
            this.Name = name;
            this.Init = init;
        }

        public String Name { get; }
        public Option<Expr> Init { get; }

        public static Enumr Create(String name, Option<Expr> init) =>
            new Enumr(name, init);

        [Obsolete]
        public Tuple<Env, String, Int32> GetEnumerator(Env env, Int32 idx) {
            if (this.Init.IsNone) {
                return new Tuple<Env, String, Int32>(env, this.Name, idx);
            }

            AST.Expr init = this.Init.Value.GetExpr(env);

            init = AST.TypeCast.MakeCast(init, new LongType());
            if (!init.IsConstExpr) {
                throw new InvalidOperationException("Error: expected constant integer");
            }
            Int32 initIdx = ((ConstLong)init).value;

            return new Tuple<Env, String, int>(env, this.Name, initIdx);
        }
    }

}