using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SyntaxTree {

    /// <summary>
    /// specifier-qualifier-list
    ///   : [ type-specifier | type-qualifier ]+
    /// </summary>
    public class SpecQualList : PTNode {
        protected SpecQualList(ImmutableList<TypeSpec> typeSpecs, ImmutableList<TypeQual> typeQuals) {
            this.TypeSpecs = typeSpecs;
            this.TypeQuals = typeQuals;
        }

        public static SpecQualList Create(ImmutableList<TypeSpec> typeSpecs, ImmutableList<TypeQual> typeQuals) =>
            new SpecQualList(typeSpecs, typeQuals);

        public static SpecQualList Create() =>
            Create(ImmutableList<TypeSpec>.Empty, ImmutableList<TypeQual>.Empty);

        public static SpecQualList AddTypeSpec(SpecQualList list, TypeSpec typeSpec) =>
            Create(list.TypeSpecs.Add(typeSpec), list.TypeQuals);

        public static SpecQualList AddTypeQual(SpecQualList list, TypeQual typeQual) =>
            Create(list.TypeSpecs, list.TypeQuals.Add(typeQual));

        public ImmutableList<TypeSpec> TypeSpecs { get; }
        public ImmutableList<TypeQual> TypeQuals { get; }

    }

    /// <summary>
    /// declaration-specifiers
    ///   : [ storage-class-specifier | type-specifier | type-qualifier ]+
    /// </summary>
    public class DeclnSpecs : SpecQualList {
        protected DeclnSpecs(ImmutableList<StorageClsSpec> storageClsSpecs, ImmutableList<TypeSpec> typeSpecs, ImmutableList<TypeQual> typeQuals)
            : base(typeSpecs, typeQuals) {
            this.StorageClsSpecs = storageClsSpecs;
        }

        public static DeclnSpecs Create(ImmutableList<StorageClsSpec> storageClsSpecs, ImmutableList<TypeSpec> typeSpecs, ImmutableList<TypeQual> typeQuals) =>
            new DeclnSpecs(storageClsSpecs, typeSpecs, typeQuals);

        public static new DeclnSpecs Create() =>
            Create(ImmutableList<StorageClsSpec>.Empty, ImmutableList<TypeSpec>.Empty, ImmutableList<TypeQual>.Empty);

        public static DeclnSpecs AddStorageClsSpe(DeclnSpecs declnSpecs, TypeSpec typeSpec) =>
            Create(declnSpecs.StorageClsSpecs, declnSpecs.TypeSpecs.Add(typeSpec), declnSpecs.TypeQuals);


        public static DeclnSpecs AddTypeSpec(DeclnSpecs declnSpecs, TypeSpec typeSpec) =>
            Create(declnSpecs.StorageClsSpecs, declnSpecs.TypeSpecs.Add(typeSpec), declnSpecs.TypeQuals);

        public ImmutableList<StorageClsSpec> StorageClsSpecs { get; }

        /// <summary>
        /// Get storage class specifier and type.
        /// </summary>
        public Tuple<AST.Env, AST.Decln.SCS, AST.ExprType> GetSCSType(AST.Env env) {
            Tuple<AST.Env, AST.ExprType> r_type = GetExprTypeEnv(env);
            env = r_type.Item1;
            AST.ExprType type = r_type.Item2;
            AST.Decln.SCS scs = GetSCS();
            return Tuple.Create(env, scs, type);
        }

        /// <summary>
        /// Get the type and the modified environment.
        /// </summary>
        public Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env) {

            Boolean is_const = TypeQuals.Contains(TypeQual.CONST);
            Boolean is_volatile = TypeQuals.Contains(TypeQual.VOLATILE);

            // 1. if no type specifier => long
            if (TypeSpecs.Count == 0) {
                return new Tuple<AST.Env, AST.ExprType>(env, new AST.TLong(is_const, is_volatile));
            }

            // 2. now let's analyse type specs
            if (TypeSpecs.All(spec => spec.kind != TypeSpec.Kind.NON_BASIC)) {

                var basic_specs = TypeSpecs.Select(spec => spec.kind);

                var basic_type = GetBasicType(basic_specs);

                switch (basic_type) {
                    case AST.ExprType.Kind.VOID:
                        return Tuple.Create(env, (AST.ExprType)new AST.TVoid(is_const, is_volatile));

                    case AST.ExprType.Kind.CHAR:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TChar(is_const, is_volatile));

                    case AST.ExprType.Kind.UCHAR:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TUChar(is_const, is_volatile));

                    case AST.ExprType.Kind.SHORT:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TShort(is_const, is_volatile));

                    case AST.ExprType.Kind.USHORT:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TUShort(is_const, is_volatile));

                    case AST.ExprType.Kind.LONG:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TLong(is_const, is_volatile));

                    case AST.ExprType.Kind.ULONG:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TULong(is_const, is_volatile));

                    case AST.ExprType.Kind.FLOAT:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TFloat(is_const, is_volatile));

                    case AST.ExprType.Kind.DOUBLE:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TDouble(is_const, is_volatile));

                    default:
                        throw new Exception("Can't match type specifier.");
                }

            } else if (TypeSpecs.Count == 1) {
                // now we can only match for struct, union, function...
                return TypeSpecs[0].GetExprTypeEnv(env, is_const, is_volatile);

            } else {
                throw new InvalidOperationException("Can't match type specifier.");
            }
        }

        /// <summary>
        /// Only used by the parser.
        /// </summary>
        /// <returns></returns>
        public bool IsTypedef() => StorageClsSpecs.Contains(StorageClsSpec.TYPEDEF);

        private AST.Decln.SCS GetSCS() {
            if (StorageClsSpecs.Count == 0) {
                return AST.Decln.SCS.AUTO;
            }
            if (StorageClsSpecs.Count == 1) {
                switch (StorageClsSpecs[0]) {
                    case StorageClsSpec.AUTO:
                    case StorageClsSpec.NULL:
                    case StorageClsSpec.REGISTER:
                        return AST.Decln.SCS.AUTO;
                    case StorageClsSpec.EXTERN:
                        return AST.Decln.SCS.EXTERN;
                    case StorageClsSpec.STATIC:
                        return AST.Decln.SCS.STATIC;
                    case StorageClsSpec.TYPEDEF:
                        return AST.Decln.SCS.TYPEDEF;
                    default:
                        throw new InvalidOperationException();
                }
            }
            throw new InvalidOperationException("Multiple storage class specifiers.");
        }

        /// <summary>
        /// Match basic type specifier: int / char / ...
        /// </summary>
        private static AST.ExprType.Kind GetBasicType(IEnumerable<TypeSpec.Kind> specs) {
            foreach (KeyValuePair<TypeSpec.Kind[], AST.ExprType.Kind> pair in type_spec_dict) {
                if (!Enumerable.Except(pair.Key, specs).Any()) {
                    return pair.Value;
                }
            }
            throw new InvalidOperationException("Cannot match basic type specifier.");
        }

        private static IReadOnlyDictionary<TypeSpec.Kind[], AST.ExprType.Kind> type_spec_dict = new Dictionary<TypeSpec.Kind[], AST.ExprType.Kind> {

            // void
            [new[] { TypeSpec.Kind.VOID }] = AST.ExprType.Kind.VOID,

            // char
            [new[] { TypeSpec.Kind.CHAR }] = AST.ExprType.Kind.CHAR,
            [new[] { TypeSpec.Kind.SIGNED, TypeSpec.Kind.CHAR }] = AST.ExprType.Kind.CHAR,

            // uchar
            [new[] { TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.CHAR }] = AST.ExprType.Kind.UCHAR,

            // short
            [new[] { TypeSpec.Kind.SHORT }] = AST.ExprType.Kind.SHORT,
            [new[] { TypeSpec.Kind.SIGNED, TypeSpec.Kind.SHORT }] = AST.ExprType.Kind.SHORT,
            [new[] { TypeSpec.Kind.SHORT, TypeSpec.Kind.INT }] = AST.ExprType.Kind.SHORT,
            [new[] { TypeSpec.Kind.SIGNED, TypeSpec.Kind.SHORT, TypeSpec.Kind.INT }] = AST.ExprType.Kind.SHORT,

            // ushort
            [new[] { TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.SHORT }] = AST.ExprType.Kind.USHORT,
            [new[] { TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.SHORT, TypeSpec.Kind.INT }] = AST.ExprType.Kind.USHORT,

            // long
            [new[] { TypeSpec.Kind.INT }] = AST.ExprType.Kind.LONG,
            [new[] { TypeSpec.Kind.SIGNED }] = AST.ExprType.Kind.LONG,
            [new[] { TypeSpec.Kind.SIGNED, TypeSpec.Kind.INT }] = AST.ExprType.Kind.LONG,
            [new[] { TypeSpec.Kind.LONG }] = AST.ExprType.Kind.LONG,
            [new[] { TypeSpec.Kind.SIGNED, TypeSpec.Kind.LONG }] = AST.ExprType.Kind.LONG,
            [new[] { TypeSpec.Kind.LONG, TypeSpec.Kind.INT }] = AST.ExprType.Kind.LONG,
            [new[] { TypeSpec.Kind.SIGNED, TypeSpec.Kind.LONG, TypeSpec.Kind.INT }] = AST.ExprType.Kind.LONG,

            // ulong
            [new[] { TypeSpec.Kind.UNSIGNED }] = AST.ExprType.Kind.ULONG,
            [new[] { TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.INT }] = AST.ExprType.Kind.ULONG,
            [new[] { TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.LONG }] = AST.ExprType.Kind.ULONG,
            [new[] { TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.LONG, TypeSpec.Kind.INT }] = AST.ExprType.Kind.ULONG,

            // float
            [new[] { TypeSpec.Kind.FLOAT }] = AST.ExprType.Kind.FLOAT,

            // double
            [new[] { TypeSpec.Kind.DOUBLE }] = AST.ExprType.Kind.DOUBLE,
            [new[] { TypeSpec.Kind.LONG, TypeSpec.Kind.DOUBLE }] = AST.ExprType.Kind.DOUBLE,

        };

    }

}