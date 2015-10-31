using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using static SyntaxTree.SemanticAnalysis;

namespace SyntaxTree {

    /// <summary>
    /// declaration
    ///   : declaration-specifiers [init-declarator-list]? ';'
    /// </summary>
    public class Decln : ExternDecln {
        protected Decln(DeclnSpecs declnSpecs, ImmutableList<InitDeclr> initDeclrs) {
            this.DeclnSpecs = declnSpecs;
            this.InitDeclrs = initDeclrs;
        }

        [Obsolete]
        public Decln(DeclnSpecs declnSpecs, IEnumerable<InitDeclr> initDeclrs)
            : this(declnSpecs, initDeclrs.ToImmutableList()) { }

        public static Decln Create(DeclnSpecs declnSpecs, ImmutableList<InitDeclr> initDeclrs) =>
            new Decln(declnSpecs, initDeclrs);

        public DeclnSpecs DeclnSpecs { get; }
        public ImmutableList<InitDeclr> InitDeclrs { get; }

        [SemantMethod]
        public ISemantReturn<ImmutableList<Tuple<AST.Env, AST.Decln>>> GetDeclns(AST.Env env) {
            var storageClass = this.DeclnSpecs.GetStorageClass();
            var baseType = Semant(this.DeclnSpecs.GetExprType, ref env);

            var declns = this.InitDeclrs.ConvertAll(
                initDeclr => {
                    var typeAndInitr = Semant(initDeclr.GetDecoratedTypeAndInitr, baseType, ref env);
                    var type = typeAndInitr.Item1;
                    var initr = typeAndInitr.Item2;
                    var name = initDeclr.GetName();

                    // Add the new symbol into the environment.
                    AST.Env.EntryKind kind;
                    switch (storageClass) {
                        case AST.Decln.StorageClass.AUTO:
                            if (env.IsGlobal()) {
                                kind = AST.Env.EntryKind.GLOBAL;
                            } else {
                                kind = AST.Env.EntryKind.STACK;
                            }
                            break;
                        case AST.Decln.StorageClass.EXTERN:
                            kind = AST.Env.EntryKind.GLOBAL;
                            break;
                        case AST.Decln.StorageClass.STATIC:
                            kind = AST.Env.EntryKind.GLOBAL;
                            break;
                        case AST.Decln.StorageClass.TYPEDEF:
                            kind = AST.Env.EntryKind.TYPEDEF;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    env = env.PushEntry(kind, name, type);

                    return Tuple.Create(env, new AST.Decln(name, storageClass, type, initr));
                }
            );

            return SemantReturn.Create(env, declns);
        }

        [Checked]
        [Obsolete]
        public Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> GetDeclns_(AST.Env env) {

            // Get storage class, and base type.
            Tuple<AST.Env, AST.Decln.StorageClass, AST.ExprType> r_specs = DeclnSpecs.GetSCSType(env);
            env = r_specs.Item1;
            AST.Decln.StorageClass scs = r_specs.Item2;
            AST.ExprType base_type = r_specs.Item3;

            List<Tuple<AST.Env, AST.Decln>> declns = new List<Tuple<AST.Env, AST.Decln>>();

            // For each init declarators, we'll generate a declaration.
            foreach (InitDeclr init_declr in InitDeclrs) {

                // Get the final type, name, and initializer.
                Tuple<AST.Env, AST.ExprType, Option<AST.Initr>, String> r_declr = init_declr.GetInitDeclr(env, base_type);
                env = r_declr.Item1;
                AST.ExprType type = r_declr.Item2;
                Option<AST.Initr> initr = r_declr.Item3;
                String name = r_declr.Item4;

                // Insert the new symbol into the environment.
                AST.Env.EntryKind kind;
                switch (scs) {
                    case AST.Decln.StorageClass.AUTO:
                        if (env.IsGlobal()) {
                            kind = AST.Env.EntryKind.GLOBAL;
                        } else {
                            kind = AST.Env.EntryKind.STACK;
                        }
                        break;
                    case AST.Decln.StorageClass.EXTERN:
                        kind = AST.Env.EntryKind.GLOBAL;
                        break;
                    case AST.Decln.StorageClass.STATIC:
                        kind = AST.Env.EntryKind.GLOBAL;
                        break;
                    case AST.Decln.StorageClass.TYPEDEF:
                        kind = AST.Env.EntryKind.TYPEDEF;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                env = env.PushEntry(kind, name, type);

                // Generate the declaration.
                declns.Add(Tuple.Create(env, new AST.Decln(name, scs, type, initr)));

            }

            return new Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>>(env, declns);
        }

        // Simply change the Decln's to ExternDecln's.
        [Checked]
        [Obsolete]
        public Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>> GetExternDecln_(AST.Env env) {
            Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_declns = GetDeclns_(env);
            env = r_declns.Item1;

            List<Tuple<AST.Env, AST.ExternDecln>> declns = r_declns
                .Item2
                .ConvertAll(_ => new Tuple<AST.Env, AST.ExternDecln>(_.Item1, _.Item2));

            return new Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>>(env, declns);
        }

        [SemantMethod]
        public ISemantReturn<ImmutableList<Tuple<AST.Env, AST.ExternDecln>>> GetExternDecln(AST.Env env) {
            var declns = Semant(GetDeclns, ref env);
            var externDeclns = declns.ConvertAll(_ => Tuple.Create(_.Item1, _.Item2 as AST.ExternDecln));
            return SemantReturn.Create(env, externDeclns);
        }
    }

    /// <summary>
    /// struct-declaration
    ///   : specifier-qualifier-list struct-declarator-list ';'
    /// 
    /// struct-declarator-list
    ///   : struct-declarator [ ',' struct-declarator ]*
    /// 
    /// struct-declarator
    ///   : declarator
    ///   | [declarator]? ':' constant-expression
    /// </summary>
    public class StructDecln : SyntaxTreeNode {
        protected StructDecln(SpecQualList specQualList, ImmutableList<IStructDeclr> structDeclrs) {
            this.SpecQualList = specQualList;
            this.StructDeclrs = structDeclrs;
        }

        [Obsolete]
        public StructDecln(DeclnSpecs declnSpecs, List<Declr> structDeclrs)
            : this(declnSpecs, structDeclrs.ToImmutableList<IStructDeclr>()) { }

        public static StructDecln Create(SpecQualList specQualList, ImmutableList<IStructDeclr> structDeclrs) =>
            new StructDecln(specQualList, structDeclrs);

        public SpecQualList SpecQualList { get; }
        public ImmutableList<IStructDeclr> StructDeclrs { get; }

        [SemantMethod]
        public ISemantReturn<ImmutableList<Tuple<Option<String>, AST.ExprType>>> GetMemberDeclns(AST.Env env) {
            // Semant specifier-qualifier-list.
            var baseType = Semant(this.SpecQualList.GetExprType, ref env);

            // Decorate types, based on struct declarators.
            var memberTypes =
                this.StructDeclrs
                .ConvertAll(
                    structDeclr =>
                        Semant(structDeclr.DecorateType, baseType, ref env)
                );

            // Get (optional) member names.
            var memberNames =
                this.StructDeclrs
                .ConvertAll(
                    structDeclr => structDeclr.OptionalName
                );

            return SemantReturn.Create(env, memberNames.Zip(memberTypes, Tuple.Create).ToImmutableList());
        }

        // Get Declarations : env -> (env, (name, type)[])
        // ===============================================
        // 
        [Obsolete]
        public Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> GetDeclns(AST.Env env) {
            var structDeclns = Semant(this.GetMemberDeclns, ref env);
            return Tuple.Create(
                env,
                structDeclns.ConvertAll(_ => Tuple.Create(_.Item1.Value, _.Item2)).ToList()
            );
            //Tuple<AST.Env, AST.ExprType> r_specs = SpecQualList.GetExprTypeEnv(env);
            //env = r_specs.Item1;
            //AST.ExprType base_type = r_specs.Item2;

            //List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<String, AST.ExprType>>();
            //foreach (Declr declr in StructDeclrs) {
            //    Tuple<String, AST.ExprType> r_declr = declr.GetNameAndType(env, base_type);
            //    String name = r_declr.Item1;
            //    AST.ExprType type = r_declr.Item2;
            //    attribs.Add(new Tuple<String, AST.ExprType>(name, type));
            //}
            //return new Tuple<AST.Env, List<Tuple<String, AST.ExprType>>>(env, attribs);
        }

    }

    /// <summary>
    /// Parameter Declaration.
    /// 
    /// int foo(int arg0, int arg1);
    ///         ~~~~~~~~
    /// 
    /// int foo(int, int);
    ///         ~~~
    /// 
    /// The declarator can be completely omitted.
    /// </summary>
    public class ParamDecln : SyntaxTreeNode {
        protected ParamDecln(DeclnSpecs declnSpecs, IParamDeclr declr) {
            this.DeclnSpecs = declnSpecs;
            this.Declr = declr;
        }

        public static ParamDecln Create(DeclnSpecs declnSpecs, Option<IParamDeclr> declr) =>
            new ParamDecln(declnSpecs, declr.IsNone ? AbstractDeclr.Empty : declr.Value);

        public static ParamDecln Create(DeclnSpecs declnSpecs, IParamDeclr declr) =>
            new ParamDecln(declnSpecs, declr);

        public DeclnSpecs DeclnSpecs { get; }
        public IParamDeclr Declr { get; }

        [SemantMethod]
        // TODO: support register storage in function parameters.
        public AST.Decln.StorageClass GetStorageClsSpec() {
            var storageClsSpecs = this.DeclnSpecs
                .StorageClsSpecs
                .DefaultIfEmpty(StorageClsSpec.NULL);

            if (storageClsSpecs.Count() > 1) {
                throw new InvalidOperationException("Only one storage class specifier is allowed.");
            }

            switch (storageClsSpecs.First()) {
                case StorageClsSpec.NULL:
                case StorageClsSpec.REGISTER:
                    return AST.Decln.StorageClass.AUTO;
                default:
                    throw new InvalidOperationException("Only register storage is allowed in function parameters.");
            }
        }

        [SemantMethod]
        public Option<String> GetParamName() =>
            this.Declr.OptionalName;

        [SemantMethod]
        public ISemantReturn<AST.ExprType> GetParamType(AST.Env env) {
            var baseType = Semant(this.DeclnSpecs.GetExprType, ref env);
            var type = Semant(this.Declr.DecorateType, baseType, ref env);
            return SemantReturn.Create(env, type);
        }

        [Obsolete]
        public Tuple<String, AST.ExprType> GetParamDecln(AST.Env env) {

            Tuple<AST.Env, AST.Decln.StorageClass, AST.ExprType> r_specs = DeclnSpecs.GetSCSType(env);
            // TODO: check environment
            AST.Decln.StorageClass scs = r_specs.Item2;
            AST.ExprType type = r_specs.Item3;

            String name = "";
            if (Declr.OptionalName.IsSome) {
                Tuple<String, AST.ExprType> r_declr = (Declr as Declr).GetNameAndType(env, type);
                name = r_declr.Item1;
                type = r_declr.Item2;
            }
            return Tuple.Create(name, type);
        }
    }

    /// <summary>
    /// type-name
    ///   : specifier-qualifier-list [abstract-declarator]?
    /// </summary>
    public class TypeName : SyntaxTreeNode {
        protected TypeName(SpecQualList specQualList, AbstractDeclr abstractDeclr) {
            this.SpecQualList = specQualList;
            this.AbstractDeclr = abstractDeclr;
        }

        [Obsolete]
        public TypeName(DeclnSpecs declnSpecs, Declr declr)
            : this(declnSpecs, AbstractDeclr.Create(declr.TypeModifiers)) { }

        public static TypeName Create(SpecQualList specQualList, AbstractDeclr abstractDeclr) =>
            new TypeName(specQualList, abstractDeclr);

        public static TypeName Create(SpecQualList specQualList, Option<AbstractDeclr> abstractDeclr) =>
            Create(specQualList, abstractDeclr.IsSome ? AbstractDeclr.Empty : abstractDeclr.Value);

        public SpecQualList SpecQualList { get; }
        public AbstractDeclr AbstractDeclr { get; }

        [SemantMethod]
        public ISemantReturn<AST.ExprType> GetExprType(AST.Env env) {
            var baseType = Semant(this.SpecQualList.GetExprType, ref env);
            var type = Semant(this.AbstractDeclr.DecorateType, baseType, ref env);
            return SemantReturn.Create(env, type);
        }

        [Obsolete]
        public Tuple<AST.Env, AST.ExprType> GetTypeEnv(AST.Env env) {
            var type = Semant(this.GetExprType, ref env);
            return Tuple.Create(env, type);
        }
    }
}