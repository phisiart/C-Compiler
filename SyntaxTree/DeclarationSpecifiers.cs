using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
    public abstract class TypeSpec : SyntaxTreeNode {
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

        [Obsolete]
        public abstract Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env, Boolean isConst, Boolean isVolatile);

        [SemantMethod]
        public abstract ISemantReturn<AST.ExprType> GetExprType(AST.Env env);

        public abstract Kind kind { get; }
    }

    public sealed class BasicTypeSpec : TypeSpec {
        public BasicTypeSpec(Kind kind) {
            this.kind = kind;
        }

        public override Kind kind { get; }

        [Obsolete]
        public override Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env, Boolean isConst, Boolean isVolatile) {
            throw new InvalidProgramException();
        }

        [SemantMethod]
        public override ISemantReturn<AST.ExprType> GetExprType(AST.Env env) {
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

        [Obsolete]
        public override Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env, Boolean isConst, Boolean isVolatile) {

            var entryOpt = env.Find(this.Name);

            if (entryOpt.IsNone) {
                throw new InvalidOperationException($"Cannot find name \"{this.Name}\".");
            }

            var entry = entryOpt.Value;

            if (entry.kind != AST.Env.EntryKind.TYPEDEF) {
                throw new InvalidOperationException($"\"{this.Name}\" is not a typedef.");
            }

            return Tuple.Create(env, entry.type.GetQualifiedType(isConst, isVolatile));
        }

        [SemantMethod]
        public override ISemantReturn<AST.ExprType> GetExprType(AST.Env env) {
            var entryOpt = env.Find(this.Name);
            if (entryOpt.IsNone) {
                throw new InvalidProgramException("This should not pass the parser.");
            }
            var entry = entryOpt.Value;
            if (entry.kind != AST.Env.EntryKind.TYPEDEF) {
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
    public class SpecQualList : SyntaxTreeNode {
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

        private static ImmutableDictionary<ImmutableSortedSet<TypeSpec.Kind>, AST.ExprType> BasicTypeSpecLookupTable { get; }

        static SpecQualList() {

            BasicTypeSpecLookupTable = ImmutableDictionary<ImmutableSortedSet<TypeSpec.Kind>, AST.ExprType>.Empty
                
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.VOID), new AST.TVoid())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.CHAR), new AST.TChar())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.CHAR, TypeSpec.Kind.SIGNED), new AST.TChar())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.CHAR, TypeSpec.Kind.UNSIGNED), new AST.TUChar())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SHORT), new AST.TShort())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SHORT, TypeSpec.Kind.SIGNED), new AST.TShort())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SHORT, TypeSpec.Kind.INT), new AST.TShort())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SHORT, TypeSpec.Kind.INT, TypeSpec.Kind.SIGNED), new AST.TShort())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SHORT, TypeSpec.Kind.UNSIGNED), new AST.TUShort())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SHORT, TypeSpec.Kind.INT, TypeSpec.Kind.UNSIGNED), new AST.TUShort())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.INT), new AST.TLong())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.INT, TypeSpec.Kind.SIGNED), new AST.TLong())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.INT, TypeSpec.Kind.LONG), new AST.TLong())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.INT, TypeSpec.Kind.SIGNED, TypeSpec.Kind.LONG), new AST.TLong())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SIGNED), new AST.TLong())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.SIGNED, TypeSpec.Kind.LONG), new AST.TLong())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.LONG), new AST.TLong())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.UNSIGNED), new AST.TULong())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.INT), new AST.TULong())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.LONG), new AST.TULong())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.INT, TypeSpec.Kind.LONG), new AST.TULong())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.FLOAT), new AST.TFloat())

            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.DOUBLE), new AST.TDouble())
            .Add(ImmutableSortedSet.Create(TypeSpec.Kind.DOUBLE, TypeSpec.Kind.LONG), new AST.TDouble())
            ;
        }

        /// <summary>
        /// Get qualified type, based on type specifiers & type qualifiers.
        /// </summary>
        [SemantMethod]
        public ISemantReturn<AST.ExprType> GetExprType(AST.Env env) {
            Boolean isConst = this.TypeQuals.Contains(TypeQual.CONST);
            Boolean isVolatile = this.TypeQuals.Contains(TypeQual.VOLATILE);

            // If no type specifier is given, assume long type.
            if (this.TypeSpecs.IsEmpty) {
                return SemantReturn.Create(env, new AST.TLong(isConst, isVolatile));
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

        public static DeclnSpecs Create() =>
            Create(ImmutableList<StorageClsSpec>.Empty, ImmutableList<TypeSpec>.Empty, ImmutableList<TypeQual>.Empty);

        public static DeclnSpecs Add(DeclnSpecs declnSpecs, StorageClsSpec storageClsSpec) =>
            Create(declnSpecs.StorageClsSpecs.Add(storageClsSpec), declnSpecs.TypeSpecs, declnSpecs.TypeQuals);

        public static DeclnSpecs Add(DeclnSpecs declnSpecs, TypeSpec typeSpec) =>
            Create(declnSpecs.StorageClsSpecs, declnSpecs.TypeSpecs.Add(typeSpec), declnSpecs.TypeQuals);

        public static DeclnSpecs Add(DeclnSpecs declnSpecs, TypeQual typeQual) =>
            Create(declnSpecs.StorageClsSpecs, declnSpecs.TypeSpecs, declnSpecs.TypeQuals.Add(typeQual));

        public ImmutableList<StorageClsSpec> StorageClsSpecs { get; }

        /// <summary>
        /// Get storage class specifier and type.
        /// </summary>
        [Obsolete]
        public Tuple<AST.Env, AST.Decln.StorageClass, AST.ExprType> GetSCSType(AST.Env env) {
            Tuple<AST.Env, AST.ExprType> r_type = GetExprTypeEnv(env);
            env = r_type.Item1;
            AST.ExprType type = r_type.Item2;
            AST.Decln.StorageClass scs = GetStorageClass();
            return Tuple.Create(env, scs, type);
        }

        /// <summary>
        /// Get the type and the modified environment.
        /// </summary>
        [Obsolete]
        public Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env) {
            var _ = this.GetExprType(env);
            return Tuple.Create(_.Env, _.Value);
            //Boolean isConst = TypeQuals.Contains(TypeQual.CONST);
            //Boolean is_volatile = TypeQuals.Contains(TypeQual.VOLATILE);

            //// 1. if no type specifier => long
            //if (TypeSpecs.Count == 0) {
            //    return new Tuple<AST.Env, AST.ExprType>(env, new AST.TLong(isConst, is_volatile));
            //}

            //// 2. now let's analyse type specs
            //if (TypeSpecs.All(spec => spec.kind != TypeSpec.Kind.NON_BASIC)) {

            //    var basic_specs = TypeSpecs.Select(spec => spec.kind);

            //    var basic_type = GetBasicType(basic_specs);

            //    switch (basic_type) {
            //        case AST.ExprType.Kind.VOID:
            //            return Tuple.Create(env, (AST.ExprType)new AST.TVoid(isConst, is_volatile));

            //        case AST.ExprType.Kind.CHAR:
            //            return new Tuple<AST.Env, AST.ExprType>(env, new AST.TChar(isConst, is_volatile));

            //        case AST.ExprType.Kind.UCHAR:
            //            return new Tuple<AST.Env, AST.ExprType>(env, new AST.TUChar(isConst, is_volatile));

            //        case AST.ExprType.Kind.SHORT:
            //            return new Tuple<AST.Env, AST.ExprType>(env, new AST.TShort(isConst, is_volatile));

            //        case AST.ExprType.Kind.USHORT:
            //            return new Tuple<AST.Env, AST.ExprType>(env, new AST.TUShort(isConst, is_volatile));

            //        case AST.ExprType.Kind.LONG:
            //            return new Tuple<AST.Env, AST.ExprType>(env, new AST.TLong(isConst, is_volatile));

            //        case AST.ExprType.Kind.ULONG:
            //            return new Tuple<AST.Env, AST.ExprType>(env, new AST.TULong(isConst, is_volatile));

            //        case AST.ExprType.Kind.FLOAT:
            //            return new Tuple<AST.Env, AST.ExprType>(env, new AST.TFloat(isConst, is_volatile));

            //        case AST.ExprType.Kind.DOUBLE:
            //            return new Tuple<AST.Env, AST.ExprType>(env, new AST.TDouble(isConst, is_volatile));

            //        default:
            //            throw new Exception("Can't match type specifier.");
            //    }

            //} else if (TypeSpecs.Count == 1) {
            //    // now we can only match for struct, union, function...
            //    return TypeSpecs[0].GetExprTypeEnv(env, isConst, is_volatile);

            //} else {
            //    throw new InvalidOperationException("Can't match type specifier.");
            //}
        }

        /// <summary>
        /// Only used by the parser.
        /// </summary>
        [Obsolete]
        public bool IsTypedef() => StorageClsSpecs.Contains(StorageClsSpec.TYPEDEF);

        [SemantMethod]
        public AST.Decln.StorageClass GetStorageClass() {
            if (StorageClsSpecs.Count == 0) {
                return AST.Decln.StorageClass.AUTO;
            }

            if (StorageClsSpecs.Count == 1) {
                switch (StorageClsSpecs[0]) {
                    case StorageClsSpec.AUTO:
                    case StorageClsSpec.NULL:
                    case StorageClsSpec.REGISTER:
                        return AST.Decln.StorageClass.AUTO;

                    case StorageClsSpec.EXTERN:
                        return AST.Decln.StorageClass.EXTERN;

                    case StorageClsSpec.STATIC:
                        return AST.Decln.StorageClass.STATIC;

                    case StorageClsSpec.TYPEDEF:
                        return AST.Decln.StorageClass.TYPEDEF;

                    default:
                        throw new InvalidOperationException();
                }
            }
            throw new InvalidOperationException("Multiple storage class specifiers.");
        }
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

        [Obsolete]
        public Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> GetAttribs(AST.Env env) {
            var attribs = new List<Tuple<String, AST.ExprType>>();
            foreach (var decln in this.MemberDeclns.Value) {
                Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> r_decln = decln.GetDeclns(env);
                env = r_decln.Item1;
                attribs.AddRange(r_decln.Item2);
            }
            return Tuple.Create(env, attribs);
        }

        [Obsolete]
        public override Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env, Boolean isConst, Boolean isVolatile) {

            // If no name is supplied, this must be a new type.
            // Members must be supplied.
            if (this.Name.IsNone) {
                if (MemberDeclns.IsNone) {
                    throw new InvalidProgramException();
                }

                Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> r_attribs = GetAttribs(env);
                env = r_attribs.Item1;

                if (this.StructOrUnion == StructOrUnion.STRUCT) {
                    return new Tuple<AST.Env, AST.ExprType>(env, AST.TStructOrUnion.CreateStruct("<anonymous>", r_attribs.Item2, isConst, isVolatile));
                } else {
                    return new Tuple<AST.Env, AST.ExprType>(env, AST.TStructOrUnion.CreateUnion("<anonymous>", r_attribs.Item2, isConst, isVolatile));
                }

            } else {
                // If a name is supplied, split into 2 cases.

                String typeName = (this.StructOrUnion == StructOrUnion.STRUCT) ? $"struct {this.Name.Value}" : $"union {this.Name.Value}";

                if (MemberDeclns.IsNone) {
                    // Case 1: If no attribute list supplied, then we are either
                    //       1) mentioning an already-existed struct/union
                    //    or 2) creating an incomplete struct/union

                    Option<AST.Env.Entry> entry_opt = env.Find(typeName);

                    if (entry_opt.IsNone) {
                        // If the struct/union is not in the current environment,
                        // then add an incomplete struct/union into the environment
                        AST.ExprType type =
                            (this.StructOrUnion == StructOrUnion.STRUCT)
                            ? AST.TStructOrUnion.CreateIncompleteStruct(this.Name.Value, isConst, isVolatile)
                            : AST.TStructOrUnion.CreateIncompleteUnion(this.Name.Value, isConst, isVolatile);

                        env = env.PushEntry(AST.Env.EntryKind.TYPEDEF, typeName, type);
                        return Tuple.Create(env, type);
                    }

                    if (entry_opt.Value.kind != AST.Env.EntryKind.TYPEDEF) {
                        throw new InvalidProgramException(typeName + " is not a type? This should be my fault.");
                    }

                    // If the struct/union is found, return it.
                    return Tuple.Create(env, entry_opt.Value.type);

                } else {
                    // Case 2: If an attribute list is supplied.

                    // 1) Make sure there is no complete struct/union in the current environment.
                    Option<AST.Env.Entry> entry_opt = env.Find(typeName);
                    if (entry_opt.IsSome && entry_opt.Value.type.kind == AST.ExprType.Kind.STRUCT_OR_UNION && ((AST.TStructOrUnion)entry_opt.Value.type).IsComplete) {
                        throw new InvalidOperationException($"Redefining {typeName}");
                    }

                    // 2) Add an incomplete struct/union into the environment.
                    AST.TStructOrUnion type =
                        (this.StructOrUnion == StructOrUnion.STRUCT)
                        ? AST.TStructOrUnion.CreateIncompleteStruct(this.Name.Value, isConst, isVolatile)
                        : AST.TStructOrUnion.CreateIncompleteUnion(this.Name.Value, isConst, isVolatile);
                    env = env.PushEntry(AST.Env.EntryKind.TYPEDEF, typeName, type);

                    // 3) Iterate over the attributes.
                    Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> r_attribs = GetAttribs(env);
                    env = r_attribs.Item1;

                    // 4) Make the type complete. This would also change the entry inside env.
                    if (this.StructOrUnion == StructOrUnion.STRUCT) {
                        type.DefineStruct(r_attribs.Item2);
                    } else {
                        type.DefineUnion(r_attribs.Item2);
                    }

                    return new Tuple<AST.Env, AST.ExprType>(env, type);
                }
            }
        }

        [SemantMethod]
        public ISemantReturn<ImmutableList<Tuple<Option<String>, AST.ExprType>>> GetMembers(AST.Env env, ImmutableList<StructDecln> memberDeclns) {
            var result = memberDeclns.Aggregate(
                seed: ImmutableList<Tuple<Option<String>, AST.ExprType>>.Empty,
                func: (acc, decln) => acc.AddRange(Semant(decln.GetMemberDeclns, ref env))
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
        public override ISemantReturn<AST.ExprType> GetExprType(AST.Env env) {

            AST.TStructOrUnion type;

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
                    type = AST.TStructOrUnion.CreateIncompleteType(this.StructOrUnion, name);
                    env = env.PushEntry(AST.Env.EntryKind.TYPEDEF, typeName, type);
                    return SemantReturn.Create(env, type);
                }

                // If name found: fetch it.
                if (entryOpt.Value.kind != AST.Env.EntryKind.TYPEDEF) {
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
                    type = AST.TStructOrUnion.CreateIncompleteType(this.StructOrUnion, name);
                    env = env.PushEntry(AST.Env.EntryKind.TYPEDEF, typeName, type);
                } else {
                    if (entryOpt.Value.kind != AST.Env.EntryKind.TYPEDEF) {
                        throw new InvalidProgramException(
                            "A struct or union in env that is not typedef? This should not appear.");
                    }

                    type = entryOpt.Value.type as AST.TStructOrUnion;
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
                type = AST.TStructOrUnion.CreateIncompleteType(this.StructOrUnion, typeName);
            }

            var members = Semant(this.GetMembers, this.MemberDeclns.Value, ref env);
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

        [Obsolete]
        public override Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env, Boolean isConst, Boolean isVolatile) {
            if (this.Enumrs.IsNone) {
                // if there is no content in this enum type, we must find it's definition in the environment
                Option<AST.Env.Entry> entryOpt = env.Find($"enum {this.Name.Value}");
                if (entryOpt.IsNone || entryOpt.Value.kind != AST.Env.EntryKind.TYPEDEF) {
                    throw new InvalidOperationException($"Type \"enum {this.Name.Value}\" has not been defined.");
                }
            } else {
                // so there are something in this enum type, we need to put this type into the environment
                Int32 idx = 0;
                foreach (Enumr elem in Enumrs.Value) {
                    Tuple<AST.Env, String, Int32> r_enum = elem.GetEnumerator(env, idx);
                    env = r_enum.Item1;
                    String name = r_enum.Item2;
                    idx = r_enum.Item3;
                    env = env.PushEnum(name, new AST.TLong(), idx);
                    idx++;
                }
                env = env.PushEntry(AST.Env.EntryKind.TYPEDEF, $"enum {this.Name.Value}", new AST.TLong());
            }

            return new Tuple<AST.Env, AST.ExprType>(env, new AST.TLong(isConst, isVolatile));
        }

        [SemantMethod]
        public override ISemantReturn<AST.ExprType> GetExprType(AST.Env env) {
            if (this.Enumrs.IsNone) {
                // If no enumerators provided: must find enum type in the current environment.

                if (this.Name.IsNone) {
                    throw new InvalidProgramException("This should not pass the parser.");
                }

                var name = this.Name.Value;
                var entryOpt = env.Find($"enum {name}");

                if (entryOpt.IsNone || entryOpt.Value.kind != AST.Env.EntryKind.TYPEDEF) {
                    throw new InvalidOperationException($"enum {name} has not been defined.");
                }

                return SemantReturn.Create(env, new AST.TLong());
            }

            // If enumerators are provided: add names to environment
            Int32 offset = 0;
            foreach (var enumr in this.Enumrs.Value) {

                if (enumr.Init.IsSome) {
                    // If the user provides an initialization value, use it.
                    var init = SemantExpr(enumr.Init.Value, ref env);
                    init = AST.TypeCast.MakeCast(init, new AST.TLong());
                    if (!init.IsConstExpr) {
                        throw new InvalidOperationException("Enumerator initialization must have a constant value.");
                    }
                    offset = ((AST.ConstLong)init).value;
                }

                env = env.PushEnum(enumr.Name, new AST.TLong(), offset);

                offset++;
            }

            // If the user provides a name to the enum, add it to the environment.
            if (this.Name.IsSome) {
                var typeName = $"enum {this.Name.Value}";

                if (env.FindInCurrentScope(typeName).IsSome) {
                    throw new InvalidOperationException($"{typeName} is already defined.");
                }
                env = env.PushEntry(AST.Env.EntryKind.TYPEDEF, typeName, new AST.TLong());
            }

            return SemantReturn.Create(env, new AST.TLong());
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
    public sealed class Enumr : SyntaxTreeNode {
        private Enumr(String name, Option<Expr> init) {
            this.Name = name;
            this.Init = init;
        }

        public String Name { get; }
        public Option<Expr> Init { get; }

        public static Enumr Create(String name, Option<Expr> init) =>
            new Enumr(name, init);

        [Obsolete]
        public Tuple<AST.Env, String, Int32> GetEnumerator(AST.Env env, Int32 idx) {
            if (this.Init.IsNone) {
                return new Tuple<AST.Env, String, Int32>(env, this.Name, idx);
            }

            AST.Expr init = this.Init.Value.GetExpr(env);

            init = AST.TypeCast.MakeCast(init, new AST.TLong());
            if (!init.IsConstExpr) {
                throw new InvalidOperationException("Error: expected constant integer");
            }
            Int32 initIdx = ((AST.ConstLong)init).value;

            return new Tuple<AST.Env, String, int>(env, this.Name, initIdx);
        }
    }

}