using System;
using System.Collections.Generic;
using System.Linq;

namespace SyntaxTree {

    // the declaration of an object
    public class Declaration : ExternalDeclaration {
        public Declaration(DeclarationSpecifiers decl_specs_, List<InitializationDeclarator> init_declrs_) {
            decl_specs = decl_specs_;
            inner_init_declrs = init_declrs_;
        }

        public readonly DeclarationSpecifiers decl_specs;
        public IReadOnlyList<InitializationDeclarator> init_declrs {
            get { return inner_init_declrs; }
        }

        private readonly List<InitializationDeclarator> inner_init_declrs;

        public Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> GetDeclns(AST.Env env) {
            List<Tuple<AST.Env, AST.Decln>> declns = new List<Tuple<AST.Env, AST.Decln>>();

            Tuple<AST.Env, AST.Decln.SCS, AST.ExprType> r_specs = decl_specs.GetSCSType(env);
            env = r_specs.Item1;
            AST.Decln.SCS scs = r_specs.Item2;
            AST.ExprType base_type = r_specs.Item3;

            foreach (InitializationDeclarator init_declr in init_declrs) {
                Tuple<AST.Env, AST.ExprType, AST.Expr, String> r_declr = init_declr.GetInitDeclr(env, base_type);

                env = r_declr.Item1;
                AST.ExprType type = r_declr.Item2;
                AST.Expr init = r_declr.Item3;
                String name = r_declr.Item4;

                // TODO : [finished] add the newly declared object into the environment
                AST.Env.EntryLoc loc;
                switch (scs) {
                case AST.Decln.SCS.AUTO:
                    if (env.IsGlobal()) {
                        loc = AST.Env.EntryLoc.GLOBAL;
                    } else {
                        loc = AST.Env.EntryLoc.STACK;
                    }
                    break;
                case AST.Decln.SCS.EXTERN:
                    loc = AST.Env.EntryLoc.GLOBAL;
                    break;
                case AST.Decln.SCS.STATIC:
                    loc = AST.Env.EntryLoc.GLOBAL;
                    break;
                case AST.Decln.SCS.TYPEDEF:
                    loc = AST.Env.EntryLoc.TYPEDEF;
                    break;
                default:
                    Log.SemantError("scs error");
                    return null;
                }
                env = env.PushEntry(loc, name, type);

                declns.Add(new Tuple<AST.Env, AST.Decln>(env, new AST.Decln(name, scs, type, init)));
            }

            return new Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>>(env, declns);
        }

        public override Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>> GetExternDecln(AST.Env env) {
            Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_declns = GetDeclns(env);
            env = r_declns.Item1;
            List<Tuple<AST.Env, AST.ExternDecln>> declns = new List<Tuple<AST.Env, AST.ExternDecln>>();
            foreach (Tuple<AST.Env, AST.Decln> decln in r_declns.Item2) {
                declns.Add(new Tuple<AST.Env, AST.ExternDecln>(decln.Item1, decln.Item2));
            }
            return new Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>>(env, declns);
        }

    }


    // Declaration specifiers
    // ======================
    // includes storage class specifiers
    //          type specifiers,
    //          type qualifiers
    //
    // in semant, use GetExprType(env) to get (type, env).
    // 
    public class DeclarationSpecifiers : PTNode {
        public DeclarationSpecifiers(List<StorageClassSpecifier> _scs,
                                     List<TypeSpecifier> _typespecs,
                                     List<TypeQualifier> _typequals) {
            specs_scs = _scs;
            specs_typequals = _typequals;
            specs_typespecs = _typespecs;
        }

        public readonly List<StorageClassSpecifier> specs_scs;
        public readonly List<TypeSpecifier> specs_typespecs;
        public readonly List<TypeQualifier> specs_typequals;

        // DeclnSpecs.SemantDeclnSpecs(env) -> (env, scs, type)
        public Tuple<AST.Env, AST.Decln.SCS, AST.ExprType> GetSCSType(AST.Env env) {
            Tuple<AST.Env, AST.ExprType> r_type = GetExprType(env);
            env = r_type.Item1;
            AST.ExprType type = r_type.Item2;

            AST.Decln.SCS scs;
            switch (GetStorageClass()) {
            case StorageClassSpecifier.AUTO:
            case StorageClassSpecifier.NULL:
            case StorageClassSpecifier.REGISTER:
                scs = AST.Decln.SCS.AUTO;
                break;
            case StorageClassSpecifier.EXTERN:
                scs = AST.Decln.SCS.EXTERN;
                break;
            case StorageClassSpecifier.STATIC:
                scs = AST.Decln.SCS.STATIC;
                break;
            case StorageClassSpecifier.TYPEDEF:
                scs = AST.Decln.SCS.TYPEDEF;
                break;
            default:
                throw new InvalidOperationException("Error: invalid storage class");
            }

            return new Tuple<AST.Env, AST.Decln.SCS, AST.ExprType>(env, scs, type);
        }

        // Get Expression Type : env -> (env, type)
        // ========================================
        // 
        public Tuple<AST.Env, AST.ExprType> GetExprType(AST.Env env) {

            Boolean is_const = specs_typequals.Exists(qual => qual == TypeQualifier.CONST);
            Boolean is_volatile = specs_typequals.Exists(qual => qual == TypeQualifier.VOLATILE);

            // 1. if no type specifier => Int32
            if (specs_typespecs.Count == 0) {
                return new Tuple<AST.Env, AST.ExprType>(env, new AST.TLong(is_const, is_volatile));
            }

            // 2. now let's analyse type specs
            if (specs_typespecs.All(spec => spec.basic != BasicTypeSpecifier.NULL)) {
                List<BasicTypeSpecifier> basic_specs = specs_typespecs.ConvertAll(spec => spec.basic);
                switch (GetBasicType(basic_specs)) {
                case AST.ExprType.EnumExprType.CHAR:
                    return new Tuple<AST.Env, AST.ExprType>(env, new AST.TChar(is_const, is_volatile));

                case AST.ExprType.EnumExprType.UCHAR:
                    return new Tuple<AST.Env, AST.ExprType>(env, new AST.TUChar(is_const, is_volatile));

                case AST.ExprType.EnumExprType.SHORT:
                    return new Tuple<AST.Env, AST.ExprType>(env, new AST.TShort(is_const, is_volatile));

                case AST.ExprType.EnumExprType.USHORT:
                    return new Tuple<AST.Env, AST.ExprType>(env, new AST.TUShort(is_const, is_volatile));

                case AST.ExprType.EnumExprType.LONG:
                    return new Tuple<AST.Env, AST.ExprType>(env, new AST.TLong(is_const, is_volatile));

                case AST.ExprType.EnumExprType.ULONG:
                    return new Tuple<AST.Env, AST.ExprType>(env, new AST.TULong(is_const, is_volatile));

                case AST.ExprType.EnumExprType.FLOAT:
                    return new Tuple<AST.Env, AST.ExprType>(env, new AST.TFloat(is_const, is_volatile));

                case AST.ExprType.EnumExprType.DOUBLE:
                    return new Tuple<AST.Env, AST.ExprType>(env, new AST.TDouble(is_const, is_volatile));

                default:
                    throw new Exception("Error: can't match type specifier");
                }

            } else if (specs_typespecs.Count == 1) {
                // now we can only match for struct, union, function...
                return specs_typespecs[0].GetExprType(env, is_const, is_volatile);

            } else {
                throw new InvalidOperationException("Error: can't match type specifier");

            }
        }

        // IsTypeOf
        // ========
        // Used by the parser
        // 
        public Boolean IsTypedef() {
            return specs_scs.Exists(scs => scs == StorageClassSpecifier.TYPEDEF);
        }

        // GetStorageClass
        // ===============
        // Infer the storage class
        // 
        public StorageClassSpecifier GetStorageClass() {
            if (specs_scs.Count == 0) {
                return StorageClassSpecifier.NULL;
            } else if (specs_scs.Count == 1) {
                return specs_scs[0];
            } else {
                throw new InvalidOperationException("Error: multiple storage class specifiers.");
            }
        }

        // GetBasicType
        // ============
        // input: specs
        // output: EnumExprType
        // returns a type from a list of type specifiers
        // 
        private static AST.ExprType.EnumExprType GetBasicType(List<BasicTypeSpecifier> specs) {
            foreach (KeyValuePair<List<BasicTypeSpecifier>, AST.ExprType.EnumExprType> pair in bspecs2enumtype) {
                if (MatchSpecs(specs, pair.Key)) {
                    return pair.Value;
                }
            }
            Log.SemantError("Error: can't match type specifiers");
            return AST.ExprType.EnumExprType.ERROR;
        }

        // MatchSpecs
        // ============================
        // input: specs, key
        // private
        // Test whether the basic type specs matches the key
        // 
        private static Boolean MatchSpecs(List<BasicTypeSpecifier> lhs, List<BasicTypeSpecifier> rhs) {
            return lhs.Count == rhs.Count && rhs.All(item => lhs.Contains(item));
        }

        // bspecs2enumtype
        // ===============
        // 
        private static Dictionary<List<BasicTypeSpecifier>, AST.ExprType.EnumExprType> bspecs2enumtype = new Dictionary<List<BasicTypeSpecifier>, AST.ExprType.EnumExprType> {

            // void : { void }
            { new List<BasicTypeSpecifier> { BasicTypeSpecifier.VOID }, AST.ExprType.EnumExprType.VOID },

            // char : { char }
            //      | { signed char }
            { new List<BasicTypeSpecifier> { BasicTypeSpecifier.CHAR }, AST.ExprType.EnumExprType.CHAR },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.SIGNED, BasicTypeSpecifier.CHAR}, AST.ExprType.EnumExprType.CHAR },

            // uchar : { unsigned char }
            { new List<BasicTypeSpecifier> { BasicTypeSpecifier.UNSIGNED, BasicTypeSpecifier.CHAR}, AST.ExprType.EnumExprType.UCHAR },

            // short : { short }
            //       | { signed short }
            //       | { short Int32 }
            //       | { signed short Int32 }
            { new List<BasicTypeSpecifier> { BasicTypeSpecifier.SHORT }, AST.ExprType.EnumExprType.SHORT },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.SIGNED, BasicTypeSpecifier.SHORT }, AST.ExprType.EnumExprType.SHORT },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.SHORT, BasicTypeSpecifier.INT }, AST.ExprType.EnumExprType.SHORT },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.SIGNED, BasicTypeSpecifier.SHORT, BasicTypeSpecifier.INT }, AST.ExprType.EnumExprType.SHORT },

            // ushort : { unsigned short }
            //        | { unsigned short Int32 }
            { new List<BasicTypeSpecifier> { BasicTypeSpecifier.UNSIGNED, BasicTypeSpecifier.SHORT }, AST.ExprType.EnumExprType.USHORT },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.UNSIGNED, BasicTypeSpecifier.SHORT, BasicTypeSpecifier.INT }, AST.ExprType.EnumExprType.USHORT },

            // long : { Int32 }
            //      | { signed }
            //      | { signed Int32 }
            //      | { long }
            //      | { signed long }
            //      | { long Int32 }
            //      | { signed long Int32 }
            { new List<BasicTypeSpecifier> { BasicTypeSpecifier.INT }, AST.ExprType.EnumExprType.LONG },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.SIGNED }, AST.ExprType.EnumExprType.LONG },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.SIGNED, BasicTypeSpecifier.INT }, AST.ExprType.EnumExprType.LONG },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.LONG }, AST.ExprType.EnumExprType.LONG },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.SIGNED, BasicTypeSpecifier.LONG }, AST.ExprType.EnumExprType.LONG },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.LONG, BasicTypeSpecifier.INT }, AST.ExprType.EnumExprType.LONG },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.SIGNED, BasicTypeSpecifier.LONG, BasicTypeSpecifier.INT }, AST.ExprType.EnumExprType.LONG },

            // ulong : { unsigned }
            //       | { unsigned Int32 }
            //       | { unsigned long }
            //       | { unsigned long Int32 }
            { new List<BasicTypeSpecifier> { BasicTypeSpecifier.UNSIGNED }, AST.ExprType.EnumExprType.ULONG },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.UNSIGNED, BasicTypeSpecifier.INT }, AST.ExprType.EnumExprType.ULONG },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.UNSIGNED, BasicTypeSpecifier.LONG }, AST.ExprType.EnumExprType.ULONG },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.UNSIGNED, BasicTypeSpecifier.LONG, BasicTypeSpecifier.INT }, AST.ExprType.EnumExprType.ULONG },

            // float : { float }
            { new List<BasicTypeSpecifier> { BasicTypeSpecifier.FLOAT }, AST.ExprType.EnumExprType.FLOAT },

            // double : { double }
            //        | { long double }
            { new List<BasicTypeSpecifier> { BasicTypeSpecifier.DOUBLE }, AST.ExprType.EnumExprType.DOUBLE },
        { new List<BasicTypeSpecifier> { BasicTypeSpecifier.LONG, BasicTypeSpecifier.DOUBLE }, AST.ExprType.EnumExprType.DOUBLE },

    };

    }


    // InitDeclr
    // =========
    // initialization declarator: a normal declarator + an initialization expression
    // 
    public class InitializationDeclarator : PTNode {

        public InitializationDeclarator(Declarator _declr, Expression _init) {
            if (_declr != null) {
                declr = _declr;
            } else {
                declr = new NullDeclarator();
            }

            if (_init != null) {
                init = _init;
            } else {
                init = new EmptyExpression();
            }
        }

        public Declarator declr;
        public Expression init;


        // TODO : InitDeclr.GetInitDeclr(env, type) -> (env, type, expr) : change the type corresponding to init expression
        public Tuple<AST.Env, AST.ExprType, AST.Expr, String> GetInitDeclr(AST.Env env, AST.ExprType type) {

            Tuple<AST.Env, AST.Expr> r_init = init.GetExpr(env);
            env = r_init.Item1;
            AST.Expr ast_init = r_init.Item2;

            Tuple<AST.Env, AST.ExprType, String> r_declr = declr.WrapExprType(env, type);
            env = r_declr.Item1;
            type = r_declr.Item2;
            String name = r_declr.Item3;

            return new Tuple<AST.Env, AST.ExprType, AST.Expr, String>(env, type, ast_init, name);
        }

    }


    public enum StorageClassSpecifier {
        NULL,
        ERROR,
        AUTO,
        REGISTER,
        STATIC,
        EXTERN,
        TYPEDEF
    }


    public enum BasicTypeSpecifier {
        NULL,
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

    // TypeSpec
    // ========
    // TypeSpec
    //    |
    //    +--- TypedefName
    //    |
    //    +--- EnumSpec
    //    |
    //    +--- StructOrUnionSpec
    //                 |
    //                 +--- StructSpec
    //                 |
    //                 +--- UnionSpec
    //
    public class TypeSpecifier : PTNode {
        public TypeSpecifier() {
            basic = BasicTypeSpecifier.NULL;
        }
        public TypeSpecifier(BasicTypeSpecifier spec) {
            basic = spec;
        }

        // GetExprType
        // ===========
        // input: env
        // output: tuple<ExprType, Environment>
        // 
        public virtual Tuple<AST.Env, AST.ExprType> GetExprType(AST.Env env, Boolean is_const, Boolean is_volatile) {
            throw new NotImplementedException();
        }

        public readonly BasicTypeSpecifier basic;
    }


    // this is just temporary
    public class TypedefName : TypeSpecifier {
        public TypedefName(String _name) {
            name = _name;
        }

        // GetExprType
        // ===========
        // input: env, is_const, is_volatile
        // output: tuple<ExprType, Environment>
        // 
        // TODO : ** NOT FINISHED **
        // 
        public override Tuple<AST.Env, AST.ExprType> GetExprType(AST.Env env, Boolean is_const, Boolean is_volatile) {
            throw new NotImplementedException();
        }


        public readonly String name;
    }


    public enum TypeQualifier {
        NULL,
        CONST,
        VOLATILE
    }



    // Type Modifier
    // =============
    // Modify a type into a function, array, or pointer
    // 
    public abstract class TypeModifier : PTNode {
        public enum TypeModifierKind {
            FUNCTION,
            ARRAY,
            POINTER
        }

        public TypeModifier(TypeModifierKind _kind) {
            modifier_kind = _kind;
        }

        // Modify Type : (env, type) -> (env, type)
        // ========================================
        // 
        public abstract Tuple<AST.Env, AST.ExprType> ModifyType(AST.Env env, AST.ExprType type);

        public readonly TypeModifierKind modifier_kind;
    }

    public class FunctionModifier : TypeModifier {
        public FunctionModifier(ParameterTypeList _param_type_list)
            : base(TypeModifierKind.FUNCTION) {
            param_type_list = _param_type_list;
        }
        public ParameterTypeList param_type_list;

        // Modify Type : (env, type) -> (env, type)
        // ========================================
        // 
        public override Tuple<AST.Env, AST.ExprType> ModifyType(AST.Env env, AST.ExprType ret_type) {
            Tuple<Boolean, List<Tuple<AST.Env, String, AST.ExprType>>> r_params = param_type_list.GetParamTypes(env);
            Boolean varargs = r_params.Item1;
            List<Tuple<AST.Env, String, AST.ExprType>> param_types = r_params.Item2;

            List<Tuple<String, AST.ExprType>> args = param_types.ConvertAll(arg => {
                env = arg.Item1;
                return Tuple.Create(arg.Item2, arg.Item3);
            });

            return new Tuple<AST.Env, AST.ExprType>(env, AST.TFunction.Create(ret_type, args, varargs));
        }
    }

    public class ArrayModifier : TypeModifier {
        public ArrayModifier(Expression _nelems)
            : base(TypeModifierKind.ARRAY) {
            array_nelems = _nelems;
        }

        // Modify Type : (env, type) => (env, type)
        // ========================================
        // 
        public override Tuple<AST.Env, AST.ExprType> ModifyType(AST.Env env, AST.ExprType type) {
            Tuple<AST.Env, AST.Expr> r_nelems = array_nelems.GetExpr(env);
            env = r_nelems.Item1;

            // Try to cast the 'nelems' expression to a long int.
            AST.Expr expr_nelems = AST.TypeCast.MakeCast(r_nelems.Item2, new AST.TLong());

            if (!expr_nelems.IsConstExpr()) {
                throw new InvalidOperationException("Error: size of the array is not a constant.");
            }

            Int32 nelems = ((AST.ConstLong)expr_nelems).value;
            return new Tuple<AST.Env, AST.ExprType>(env, new AST.TArray(type, nelems));
        }

        public readonly Expression array_nelems;
    }

    public class PointerModifier : TypeModifier {
        public PointerModifier(List<TypeQualifier> _type_qualifiers)
            : base(TypeModifierKind.POINTER) {
            type_qualifiers = _type_qualifiers;
        }

        // Modify Type : (env, type) => (env, type)
        // ========================================
        // 
        public override Tuple<AST.Env, AST.ExprType> ModifyType(AST.Env env, AST.ExprType type) {
            Boolean is_const = type_qualifiers.Any(x => x == TypeQualifier.CONST);
            Boolean is_volatile = type_qualifiers.Any(x => x == TypeQualifier.VOLATILE);
            return new Tuple<AST.Env, AST.ExprType>(env, new AST.TPointer(type, is_const, is_volatile));
        }
        public readonly List<TypeQualifier> type_qualifiers;
    }

    public class Declarator : PTNode {
        public Declarator(String _name, List<TypeModifier> _declr_modifiers) {
            inner_declr_modifiers = _declr_modifiers;
            declr_name = _name;
        }

        public Declarator()
            : this("", new List<TypeModifier>()) { }

        public IReadOnlyList<TypeModifier> declr_modifiers {
            get { return inner_declr_modifiers; }
        }
        private readonly List<TypeModifier> inner_declr_modifiers;
        public readonly String declr_name;

        // TODO : [finished] Declr.WrapExprType(env, type) -> (env, type, name) : wrap up the type
        public virtual Tuple<AST.Env, AST.ExprType, String> WrapExprType(AST.Env env, AST.ExprType type) {
            for (int i = inner_declr_modifiers.Count; i --> 0;) {
                TypeModifier modifier = inner_declr_modifiers[i];

                Tuple<AST.Env, AST.ExprType> r = modifier.ModifyType(env, type);
                env = r.Item1;
                type = r.Item2;
            }
            return new Tuple<AST.Env, AST.ExprType, String>(env, type, declr_name);
        }
    }

    public class NullDeclarator : Declarator {
        public NullDeclarator() : base("", new List<TypeModifier>()) { }

        public override Tuple<AST.Env, AST.ExprType, String> WrapExprType(AST.Env env, AST.ExprType type) {
            return new Tuple<AST.Env, AST.ExprType, String>(env, type, "");
        }
    }

    // Parameter Type List
    // ===================
    // 
    public class ParameterTypeList : PTNode {
        public ParameterTypeList(List<ParameterDeclaration> _param_list, Boolean _varargs) {
            params_varargs = _varargs;
            params_inner_declns = _param_list;
        }

        public ParameterTypeList(List<ParameterDeclaration> _param_list)
            : this(_param_list, false) { }

        public readonly Boolean params_varargs;
        public IReadOnlyList<ParameterDeclaration> params_declns {
            get { return params_inner_declns; }
        }
        public readonly List<ParameterDeclaration> params_inner_declns;

        // Get Parameter Types
        // ===================
        // 
        public Tuple<Boolean, List<Tuple<AST.Env, String, AST.ExprType>>> GetParamTypes(AST.Env env) {
            return Tuple.Create(
                params_varargs,
                params_inner_declns.ConvertAll(decln => {
                    Tuple<AST.Env, String, AST.ExprType> r_decln = decln.GetParamDecln(env);
                    env = r_decln.Item1;
                    return r_decln;
                })
            );
        }

    }


    // EnumSpec : TypeSpec
    // ===================
    // enum <name> {
    //     ENUM0,
    //     ENUM1,
    //     ...
    // }
    // 
    // members:
    //   name      : String
    //   enum_list : List<Enumerator>
    // 
    public class EnumSpecifier : TypeSpecifier {
        public EnumSpecifier(String _name, List<Enumerator> _enum_list) {
            spec_name = _name;
            spec_enums = _enum_list;
        }

        public override Tuple<AST.Env, AST.ExprType> GetExprType(AST.Env env, Boolean is_const, Boolean is_volatile) {
            if (spec_enums == null) {
                // if there is no content in this enum type, we must find it's definition in the environment
                AST.Env.Entry entry = env.Find("enum " + spec_name);
                if (entry == null || entry.entry_loc != AST.Env.EntryLoc.TYPEDEF) {
                    Log.SemantError("Error: type 'enum " + spec_name + " ' has not been defined.");
                    return null;
                }
            } else {
                // so there are something in this enum type, we need to put this type into the environment
                Int32 idx = 0;
                foreach (Enumerator elem in spec_enums) {
                    env = env.PushEnum(elem.name, new AST.TLong(), idx);
                    idx++;
                }
                env = env.PushEntry(AST.Env.EntryLoc.TYPEDEF, "enum " + spec_name, new AST.TLong());
            }

            return new Tuple<AST.Env, AST.ExprType>(env, new AST.TLong(is_const, is_volatile));
        }

        public readonly String spec_name;
        public readonly List<Enumerator> spec_enums;

    }


    public class Enumerator : PTNode {
        public Enumerator(String _name, Expression _init) {
            name = _name;
            init = _init;
        }

        public Expression init;
        public String name;
    }


    // StructOrUnionSpec
    // =================
    // a base class of StructSpec and UnionSpec
    // not present in the semant phase
    // 
    public class StructOrUnionSpecifier : TypeSpecifier {
        public String name;
        public List<StructDeclaration> declns;
    }


    // StructSpec
    // ==========
    // 
    public class StructSpecifier : StructOrUnionSpecifier {
        public StructSpecifier(String _name, List<StructDeclaration> _declns) {
            name = _name;
            declns = _declns;
        }

        // GetExprType
        // ===========
        // input: env, is_const, is_volatile
        // output: tuple<ExprType, Environment>
        // 
        // TODO : StructSpec.GetExprType(env, is_const, is_volatile) -> (type, env)
        // 
        public override Tuple<AST.Env, AST.ExprType> GetExprType(AST.Env env, Boolean is_const, Boolean is_volatile) {

            // TODO : non-complete type
            if (name != "") {
                // add a non-complete type
                // env = env.PushEntry(AST.Env.EntryLoc.TYPEDEF, "struct " + name, null);
            }

            List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<String, AST.ExprType>>();
            foreach (StructDeclaration decln in declns) {
                Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> r_decln = decln.GetDeclns(env);
                env = r_decln.Item1;
                attribs.AddRange(r_decln.Item2);
            }

            AST.TStruct type = AST.TStruct.Create(attribs, is_const, is_volatile);

            if (name != "") {
                env = env.PushEntry(AST.Env.EntryLoc.TYPEDEF, "struct " + name, type);
            }

            return new Tuple<AST.Env, AST.ExprType>(env, type);
        }

    }


    // UnionSpec
    // =========
    // 
    public class UnionSpecifier : StructOrUnionSpecifier {
        public UnionSpecifier(String _name, List<StructDeclaration> _decl_list) {
            name = _name;
            declns = _decl_list;
        }

        // GetExprType
        // ===========
        // input: env, is_const, is_volatile
        // output: tuple<ExprType, Environment>
        // 
        // TODO : UnionSpec.GetExprType(env, is_const, is_volatile) -> (type, env)
        // 
        public override Tuple<AST.Env, AST.ExprType> GetExprType(AST.Env env, Boolean is_const, Boolean is_volatile) {
            List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<String, AST.ExprType>>();
            foreach (StructDeclaration decln in declns) {
                Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> r_decln = decln.GetDeclns(env);
                env = r_decln.Item1;
                attribs.AddRange(r_decln.Item2);
            }

            AST.TUnion type = AST.TUnion.Create(attribs, is_const, is_volatile);

            if (name != "") {
                env = env.PushEntry(AST.Env.EntryLoc.TYPEDEF, "union " + name, type);
            }

            return new Tuple<AST.Env, AST.ExprType>(env, type);
        }

    }


    // StructOrUnion
    // =============
    // only used in parsing phase
    // 
    public class StructOrUnion : PTNode {
        public StructOrUnion(Boolean _is_union) {
            is_union = _is_union;
        }
        public Boolean is_union;
    }


    public class StructDeclaration : PTNode {
        public StructDeclaration(DeclarationSpecifiers _specs, List<Declarator> _declrs) {
            specs = _specs;
            declrs = _declrs;
        }
        public DeclarationSpecifiers specs;
        public List<Declarator> declrs;

        // Get Declarations : env -> (env, (name, type)[])
        // ===============================================
        // 
        public Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> GetDeclns(AST.Env env) {
            Tuple<AST.Env, AST.ExprType> r_specs = specs.GetExprType(env);
            env = r_specs.Item1;
            AST.ExprType base_type = r_specs.Item2;

            List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<String, AST.ExprType>>();
            foreach (Declarator declr in declrs) {
                Tuple<AST.Env, AST.ExprType, String> r_declr = declr.WrapExprType(env, base_type);
                env = r_declr.Item1;
                AST.ExprType type = r_declr.Item2;
                String name = r_declr.Item3;
                attribs.Add(new Tuple<String, AST.ExprType>(name, type));
            }
            return new Tuple<AST.Env, List<Tuple<String, AST.ExprType>>>(env, attribs);
        }

    }

    // Parameter Declaration
    // =====================
    // 
    public class ParameterDeclaration : PTNode {
        public ParameterDeclaration(DeclarationSpecifiers _specs, Declarator _decl) {
            specs = _specs;

            if (_decl != null) {
                decl = _decl;
            } else {
                decl = new NullDeclarator();
            }
        }

        public readonly DeclarationSpecifiers specs;
        public readonly Declarator decl;

        // Get Parameter Declaration : env -> (env, name, type)
        // ====================================================
        // 
        public Tuple<AST.Env, String, AST.ExprType> GetParamDecln(AST.Env env) {
            Tuple<AST.Env, AST.Decln.SCS, AST.ExprType> r_specs = specs.GetSCSType(env);
            env = r_specs.Item1;
            AST.Decln.SCS scs = r_specs.Item2;
            AST.ExprType type = r_specs.Item3;

            Tuple<AST.Env, AST.ExprType, String> r_declr = decl.WrapExprType(env, type);
            env = r_declr.Item1;
            type = r_declr.Item2;
            String name = r_declr.Item3;

            return new Tuple<AST.Env, String, AST.ExprType>(env, name, type);
        }

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
    public class InitializerList : Expression {
        public InitializerList(List<Expression> _exprs) {
            initlist_exprs = _exprs;
        }
        public List<Expression> initlist_exprs;

        public Tuple<AST.Env, AST.InitList> GetInitList(AST.Env env) {
            List<AST.Expr> exprs = initlist_exprs.ConvertAll(expr => {
                Tuple<AST.Env, AST.Expr> r_expr = expr.GetExpr(env);
                env = r_expr.Item1;
                return r_expr.Item2;
            });
            return Tuple.Create(env, new AST.InitList(exprs));
        }

    }


    // Type Name
    // =========
    // describes a qualified type
    // 
    public class TypeName : PTNode {
        public TypeName(DeclarationSpecifiers _specs, Declarator _decl) {
            typename_specs = _specs;
            typename_declr = _decl;
        }

        public readonly DeclarationSpecifiers typename_specs;
        public readonly Declarator typename_declr;

        public Tuple<AST.Env, AST.ExprType> GetExprType(AST.Env env) {
            Tuple<AST.Env, AST.ExprType> r_specs = typename_specs.GetExprType(env);
            Tuple<AST.Env, AST.ExprType, String> r_declr = typename_declr.WrapExprType(r_specs.Item1, r_specs.Item2);
            return Tuple.Create(r_declr.Item1, r_declr.Item2);
        }
    }

}