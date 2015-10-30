using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

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

        [Checked]
        public Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> GetDeclns(AST.Env env) {

            // Get storage class, and base type.
            Tuple<AST.Env, AST.Decln.SCS, AST.ExprType> r_specs = DeclnSpecs.GetSCSType(env);
            env = r_specs.Item1;
            AST.Decln.SCS scs = r_specs.Item2;
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
                    case AST.Decln.SCS.AUTO:
                        if (env.IsGlobal()) {
                            kind = AST.Env.EntryKind.GLOBAL;
                        } else {
                            kind = AST.Env.EntryKind.STACK;
                        }
                        break;
                    case AST.Decln.SCS.EXTERN:
                        kind = AST.Env.EntryKind.GLOBAL;
                        break;
                    case AST.Decln.SCS.STATIC:
                        kind = AST.Env.EntryKind.GLOBAL;
                        break;
                    case AST.Decln.SCS.TYPEDEF:
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
        public override Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>> GetExternDecln(AST.Env env) {
            Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_declns = GetDeclns(env);
            env = r_declns.Item1;

            List<Tuple<AST.Env, AST.ExternDecln>> declns = r_declns
                .Item2
                .ConvertAll(_ => new Tuple<AST.Env, AST.ExternDecln>(_.Item1, _.Item2));

            return new Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>>(env, declns);
        }

    }

    public class Enumr : PTNode {

        // TODO: change this to optional.
        public Enumr(String name, Expr init) {
            this.Name = name;
            this.Init = init;
        }
        public String Name { get; }
        public Expr Init { get; }

        public static Enumr Create(String name, Option<Expr> init) =>
            new Enumr(name, init.IsSome ? init.Value : null);

        public Tuple<AST.Env, String, Int32> GetEnumerator(AST.Env env, Int32 idx) {
            AST.Expr init;

            if (this.Init == null) {
                return new Tuple<AST.Env, String, Int32>(env, this.Name, idx);
            }

            init = Init.GetExpr(env);

            init = AST.TypeCast.MakeCast(init, new AST.TLong());
            if (!init.IsConstExpr) {
                throw new InvalidOperationException("Error: expected constant integer");
            }
            Int32 init_idx = ((AST.ConstLong)init).value;

            return new Tuple<AST.Env, String, int>(env, Name, init_idx);
        }
    }

    /// <summary>
    /// Struct Specifier
    /// 
    /// Specifies a struct type.
    /// 
    /// if name == "", then
    ///     the parser ensures that declns != null,
    ///     and this specifier does not change the environment
    /// if name != "", then
    ///     if declns == null
    ///        this means that this specifier is just mentioning a struct, not defining one, so
    ///        if the current environment doesn't have this struct type, then add an **incomplete** struct
    ///     if declns != null
    ///        this means that this specifier is defining a struct, so we need to perform the following steps:
    ///        1. make sure that the current environment doesn't have a **complete** struct of this name
    ///        2. immediately add an **incomplete** struct into the environment
    ///        3. iterate over the declns
    ///        4. finish forming a complete struct and add it into the environment
    /// </summary>
    //public class StructSpec : StructOrUnionSpec {
    //    public StructSpec(String _name, IReadOnlyList<StructDecln> _declns)
    //        : base(_name, _declns) { }

    //    public override Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env, Boolean is_const, Boolean is_volatile) =>
    //        GetExprTypeEnv(true, env, is_const, is_volatile);
    //}

    /// <summary>
    /// Union Specifier
    /// 
    /// Specifies a union type.
    /// 
    /// if name == "", then
    ///     the parser ensures that declns != null,
    ///     and this specifier does not change the environment
    /// if name != "", then
    ///     if declns == null
    ///        this means that this specifier is just mentioning a struct, not defining one, so
    ///        if the current environment doesn't have this union type, then add an **incomplete** struct
    ///     if declns != null
    ///        this means that this specifier is defining a struct, so we need to perform the following steps:
    ///        1. make sure that the current environment doesn't have a **complete** union of this name
    ///        2. immediately add an **incomplete** union into the environment
    ///        3. iterate over the declns
    ///        4. finish forming a complete union and add it into the environment
    /// </summary>
    //public class UnionSpec : StructOrUnionSpec {
    //    public UnionSpec(String _name, IReadOnlyList<StructDecln> _declns)
    //        : base(_name, _declns) { }

    //    public override Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env, Boolean is_const, Boolean is_volatile) =>
    //        GetExprTypeEnv(false, env, is_const, is_volatile);

    //}

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
    public class StructDecln : PTNode {
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

        public ISemantReturn<ImmutableList<Tuple<Option<String>, AST.ExprType>>> GetMemberDeclns(AST.Env env) {
            // Semant specifier-qualifier-list.
            var baseType = SemanticAnalysis.Semant(this.SpecQualList.GetExprType, ref env);

            // Decorate types, based on struct declarators.
            var memberTypes =
                this.StructDeclrs
                .ConvertAll(
                    structDeclr =>
                        SemanticAnalysis.Semant(structDeclr.DecorateType, baseType, ref env)
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
            Tuple<AST.Env, AST.ExprType> r_specs = SpecQualList.GetExprTypeEnv(env);
            env = r_specs.Item1;
            AST.ExprType base_type = r_specs.Item2;

            List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<String, AST.ExprType>>();
            foreach (Declr declr in StructDeclrs) {
                Tuple<String, AST.ExprType> r_declr = declr.GetNameAndType(env, base_type);
                String name = r_declr.Item1;
                AST.ExprType type = r_declr.Item2;
                attribs.Add(new Tuple<String, AST.ExprType>(name, type));
            }
            return new Tuple<AST.Env, List<Tuple<String, AST.ExprType>>>(env, attribs);
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
    public class ParamDecln : PTNode {

        [Obsolete]
        public ParamDecln(DeclnSpecs declnSpecs, Option<Declr> declr)
            : this(declnSpecs, declr.IsSome ? declr.Value as IParamDeclr : AbstractDeclr.Empty) { }

        protected ParamDecln(DeclnSpecs declnSpecs, IParamDeclr declr) {
            this.DeclnSpecs = declnSpecs;
            this.Declr = declr;
        }

        public static ParamDecln Create(DeclnSpecs declnSpecs, IParamDeclr declr) =>
            new ParamDecln(declnSpecs, declr);

        public DeclnSpecs DeclnSpecs { get; }   // base type
        public IParamDeclr Declr { get; }     // type modifiers and name

        [Obsolete]
        public Tuple<String, AST.ExprType> GetParamDecln(AST.Env env) {

            Tuple<AST.Env, AST.Decln.SCS, AST.ExprType> r_specs = DeclnSpecs.GetSCSType(env);
            // TODO: check environment
            AST.Decln.SCS scs = r_specs.Item2;
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
    public class TypeName : PTNode {
        protected TypeName(SpecQualList specQualList, AbstractDeclr abstractDeclr) {
            this.SpecQualList = specQualList;
            this.AbstractDeclr = abstractDeclr;
        }

        [Obsolete]
        public TypeName(DeclnSpecs declnSpecs, Declr declr)
            : this(declnSpecs, AbstractDeclr.Create(declr.TypeModifiers)) { }

        public static TypeName Create(SpecQualList specQualList, AbstractDeclr abstractDeclr) =>
            new TypeName(specQualList, abstractDeclr);

        public SpecQualList SpecQualList { get; }
        public AbstractDeclr AbstractDeclr { get; }

        public Tuple<AST.Env, AST.ExprType> GetTypeEnv(AST.Env env) {
            Tuple<AST.Env, AST.ExprType> type_env = this.SpecQualList.GetExprTypeEnv(env);
            env = type_env.Item1;
            AST.ExprType base_type = type_env.Item2;

            return Tuple.Create(env, this.AbstractDeclr.GetNameAndType(env, base_type).Item2);
        }
    }
}