using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

namespace SyntaxTree {

    // the declaration of an object
    public class Decln : ExternDecln {
        public Decln(DeclnSpecs decln_specs, IEnumerable<InitDeclr> init_declrs) {
            this.decln_specs = decln_specs;
            this.init_declrs = init_declrs;
        }

        public readonly DeclnSpecs decln_specs;
        public readonly IEnumerable<InitDeclr> init_declrs;

        [Checked]
        public Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> GetDeclns(AST.Env env) {

            // Get storage class, and base type.
            Tuple<AST.Env, AST.Decln.SCS, AST.ExprType> r_specs = decln_specs.GetSCSType(env);
            env = r_specs.Item1;
            AST.Decln.SCS scs = r_specs.Item2;
            AST.ExprType base_type = r_specs.Item3;

            List<Tuple<AST.Env, AST.Decln>> declns = new List<Tuple<AST.Env, AST.Decln>>();

            // For each init declarators, we'll generate a declaration.
            foreach (InitDeclr init_declr in init_declrs) {

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

    // InitDeclr
    // =========
    // initialization declarator: a normal declarator + an initialization expression
    // 
    public class InitDeclr : PTNode {

        public InitDeclr(Declr declr, Option<Initr> initr) {
            this.declr = declr;
            this.initr = initr;
        }

        public readonly Declr declr;
        public readonly Option<Initr> initr;

        public static Func<Declr, Option<Initr>, InitDeclr> Create { get; } =
            (Declr declr, Option<Initr> initr) => new InitDeclr(declr, initr);

        public Tuple<AST.Env, AST.ExprType, Option<AST.Initr>, String> GetInitDeclr(AST.Env env, AST.ExprType type) {
            String name;
            Option<AST.Initr> initr_opt;

            // Get the initializer list.
            Option<Tuple<AST.Env, AST.Initr>> r_initr = this.initr.Map(_ => _.GetInitr(env));
            if (r_initr.IsSome) {
                env = r_initr.Value.Item1;
                initr_opt = new Some<AST.Initr>(r_initr.Value.Item2);
            } else {
                initr_opt = new None<AST.Initr>();
            }

            // Get the declarator.
            Tuple<String, AST.ExprType> r_declr = declr.GetNameAndType(env, type);
            name = r_declr.Item1;
            type = r_declr.Item2;

            // Implicit cast the initializer.
            initr_opt = initr_opt.Map(_ => _.ConformType(type));

            // If the object is an incomplete list, we must determine the length based on the initializer.
            if (type.kind == AST.ExprType.Kind.INCOMPLETE_ARRAY) {
                if (initr_opt.IsNone) {
                    throw new InvalidOperationException("Cannot determine the length of the array.");
                }

                // Now we need to determine the length.
                // Find the last element in the init list.
                Int32 last_offset = -1;
                initr_opt.Value.Iterate(type, (offset, _) => { last_offset = offset; });

                if (last_offset == -1) {
                    throw new InvalidOperationException("Cannot determine the length of the array based on an empty initializer list.");
                }

                AST.ExprType elem_type = ((AST.TIncompleteArray)type).elem_type;

                Int32 num_elems = 1 + last_offset / ((AST.TIncompleteArray)type).elem_type.SizeOf;

                type = new AST.TArray(elem_type, num_elems, type.is_const, type.is_volatile);
            }

            return new Tuple<AST.Env, AST.ExprType, Option<AST.Initr>, String>(env, type, initr_opt, name);
        }

    }

    // Parameter Type List
    // ===================
    // 
    public class ParameterTypeList : PTNode {
        public ParameterTypeList(IReadOnlyList<ParamDecln> _param_list, Boolean _varargs) {
            params_varargs = _varargs;
            params_inner_declns = _param_list;
        }

        public ParameterTypeList(IReadOnlyList<ParamDecln> _param_list)
            : this(_param_list, false) { }

        public readonly Boolean params_varargs;
        public IReadOnlyList<ParamDecln> params_declns {
            get { return params_inner_declns; }
        }
        public readonly IReadOnlyList<ParamDecln> params_inner_declns;

        // Get Parameter Types
        // ===================
        // 
        public Tuple<Boolean, List<Tuple<AST.Env, String, AST.ExprType>>> GetParamTypesEnv(AST.Env env) {
            return Tuple.Create(
                params_varargs,
                params_inner_declns.Select(decln => {
                    Tuple<String, AST.ExprType> r_decln = decln.GetParamDecln(env);
                    // Tuple<AST.Env, String, AST.ExprType> r_decln = decln.GetParamDeclnEnv(env);
                    // env = r_decln.Item1;
                    return Tuple.Create(env, r_decln.Item1, r_decln.Item2);
                }).ToList()
            );
        }

    }

    public class Enumr : PTNode {
        public Enumr(String _name, Expr _init) {
            enum_name = _name;
            enum_init = _init;
        }
        public readonly String enum_name;
        public readonly Expr enum_init;

        public static Func<String, Option<Expr>, Enumr> Create { get; } =
            (String name, Option<Expr> init) =>
                new Enumr(name, init.IsSome ? init.Value : null);

        public Tuple<AST.Env, String, Int32> GetEnumerator(AST.Env env, Int32 idx) {
            AST.Expr init;

            if (enum_init == null) {
                return new Tuple<AST.Env, String, int>(env, enum_name, idx);
            }

            init = enum_init.GetExpr(env);

            init = AST.TypeCast.MakeCast(init, new AST.TLong());
            if (!init.IsConstExpr) {
                throw new InvalidOperationException("Error: expected constant integer");
            }
            Int32 init_idx = ((AST.ConstLong)init).value;

            return new Tuple<AST.Env, String, int>(env, enum_name, init_idx);
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

    public class StructDecln : PTNode {
        public StructDecln(DeclnSpecs _specs, List<Declr> _declrs) {
            specs = _specs;
            declrs = _declrs;
        }
        public readonly DeclnSpecs specs;
        public readonly List<Declr> declrs;

        // Get Declarations : env -> (env, (name, type)[])
        // ===============================================
        // 
        public Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> GetDeclns(AST.Env env) {
            Tuple<AST.Env, AST.ExprType> r_specs = specs.GetExprTypeEnv(env);
            env = r_specs.Item1;
            AST.ExprType base_type = r_specs.Item2;

            List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<String, AST.ExprType>>();
            foreach (Declr declr in declrs) {
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
        public ParamDecln(DeclnSpecs specs, Option<Declr> declr) {
            this.specs = specs;
            this.declr = declr;
        }

        public readonly DeclnSpecs specs;    // base type
        public readonly Option<Declr> declr; // type modifiers and name

        public Tuple<String, AST.ExprType> GetParamDecln(AST.Env env) {

            Tuple<AST.Env, AST.Decln.SCS, AST.ExprType> r_specs = specs.GetSCSType(env);
            // TODO: check environment
            AST.Decln.SCS scs = r_specs.Item2;
            AST.ExprType type = r_specs.Item3;

            String name = "";
            if (declr.IsSome) {
                Tuple<String, AST.ExprType> r_declr = declr.Value.GetNameAndType(env, type);
                name = r_declr.Item1;
                type = r_declr.Item2;
            }
            return Tuple.Create(name, type);
        }

    }

    public abstract class Initr : PTNode {
        public enum Kind {
            EXPR,
            INIT_LIST,
        }
        public Initr(Kind kind) {
            this.kind = kind;
        }
        public abstract Tuple<AST.Env, AST.Initr> GetInitr(AST.Env env);
        public readonly Kind kind;
    }

    // Initializer List
    // ================
    // used to initialize arrays and structs, etc
    // 
    // C language standard:
    // 1. scalar types
    //    
    // 2. aggregate types
    // 3. strings
    public class InitList : Initr {
        public InitList(List<Initr> initrs)
            : base(Kind.INIT_LIST) {
            this.initrs = initrs;
        }
        public readonly List<Initr> initrs;
        public override Tuple<AST.Env, AST.Initr> GetInitr(AST.Env env) {
            List<AST.Initr> initrs = this.initrs.ConvertAll(initr => {
                Tuple<AST.Env, AST.Initr> r_initr = initr.GetInitr(env);
                env = r_initr.Item1;
                return r_initr.Item2;
            });
            return new Tuple<AST.Env, AST.Initr>(env, new AST.InitList(initrs));
        }
    }

    public class InitExpr : Initr {
        public InitExpr(Expr expr)
            : base(Kind.EXPR) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public override Tuple<AST.Env, AST.Initr> GetInitr(AST.Env env) {
            // TODO: expr should change env
            return new Tuple<AST.Env, AST.Initr>(env, new AST.InitExpr(expr.GetExpr(env)));
        }
    }

    // Type Name
    // =========
    // describes a qualified type
    // 
    public class TypeName : PTNode {
        public TypeName(DeclnSpecs specs, Declr declr) {
            this.specs = specs;
            this.declr = declr;
        }

        public readonly DeclnSpecs specs;
        public readonly Declr declr;

        public Tuple<AST.Env, AST.ExprType> GetTypeEnv(AST.Env env) {
            Tuple<AST.Env, AST.ExprType> type_env = this.specs.GetExprTypeEnv(env);
            env = type_env.Item1;
            AST.ExprType base_type = type_env.Item2;

            return Tuple.Create(env, this.declr.GetNameAndType(env, base_type).Item2);
        }
    }

}