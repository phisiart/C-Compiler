using System;
using System.Collections.Generic;
using System.Linq;


public interface ParseRule {
}

public class PTNode {
}


// the declaration of an object
public class Declaration : ExternalDeclaration {
    public Declaration(DeclarationSpecifiers decl_specs_, List<InitDeclr> init_declrs_) {
        decl_specs = decl_specs_;
        init_declrs = init_declrs_;
    }
    public DeclarationSpecifiers decl_specs;
    public List<InitDeclr> init_declrs;

    public Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> GetDeclns(AST.Env env) {
        List<Tuple<AST.Env, AST.Decln>> declns = new List<Tuple<AST.Env, AST.Decln>>();

        Tuple<AST.Env, AST.Decln.SCS, AST.ExprType> r_specs = decl_specs.GetSCSType(env);
        env = r_specs.Item1;
        AST.Decln.SCS scs = r_specs.Item2;
        AST.ExprType base_type = r_specs.Item3;

        foreach (InitDeclr init_declr in init_declrs) {
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


// DeclnSpecs
// ==========
// Declaration specifiers
// includes storage class specifiers
//          type specifiers,
//          type qualifiers
//
// in semant, use GetExprType(env) to get (type, env).
// 
public class DeclarationSpecifiers : PTNode {
    public DeclarationSpecifiers(List<StorageClassSpecifier> _storage_class_specifiers,
                                 List<TypeSpecifier> _type_specifiers,
                                 List<TypeQualifier> _type_qualifiers) {
        storage_class_specifiers = _storage_class_specifiers;
        type_qualifiers = _type_qualifiers;
        type_specifiers = _type_specifiers;
    }

    // after parsing
    // -------------
    public List<StorageClassSpecifier> storage_class_specifiers;
    public List<TypeSpecifier> type_specifiers;
    public List<TypeQualifier> type_qualifiers;

    // TODO : [finished] DeclnSpecs.SemantDeclnSpecs(env) -> (env, scs, type)
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
            Log.SemantError("scs error");
            return null;
        }

        return new Tuple<AST.Env, AST.Decln.SCS, AST.ExprType>(env, scs, type);
    }

    // GetExprType : env -> (type, env)
    // 
    public Tuple<AST.Env, AST.ExprType> GetExprType(AST.Env env) {

        bool is_const = type_qualifiers.Exists(qual => qual == TypeQualifier.CONST);
        bool is_volatile = type_qualifiers.Exists(qual => qual == TypeQualifier.VOLATILE);

        // 1. if no type specifier => int
        if (type_specifiers.Count == 0) {
            return new Tuple<AST.Env, AST.ExprType>(env, new AST.TLong());
        }

        // 2. now let's analyse type specs
        int nbasics = type_specifiers.Count(spec => spec.basic != BTypeSpec.NULL);
        if (nbasics == type_specifiers.Count) {
            List<BTypeSpec> basic_specs = new List<BTypeSpec>();
            foreach (TypeSpecifier spec in type_specifiers) {
                basic_specs.Add(spec.basic);
            }

            // this might fail and cause the program to exit.
            AST.ExprType.EnumExprType enum_type = GetBasicType(basic_specs);
            AST.ExprType type = null;
            switch (enum_type) {
            case AST.ExprType.EnumExprType.CHAR:
                type = new AST.TChar(is_const, is_volatile);
                break;
            case AST.ExprType.EnumExprType.UCHAR:
                type = new AST.TUChar(is_const, is_volatile);
                break;
            case AST.ExprType.EnumExprType.SHORT:
                type = new AST.TShort(is_const, is_volatile);
                break;
            case AST.ExprType.EnumExprType.USHORT:
                type = new AST.TUShort(is_const, is_volatile);
                break;
            case AST.ExprType.EnumExprType.LONG:
                type = new AST.TLong(is_const, is_volatile);
                break;
            case AST.ExprType.EnumExprType.ULONG:
                type = new AST.TULong(is_const, is_volatile);
                break;
            case AST.ExprType.EnumExprType.FLOAT:
                type = new AST.TFloat(is_const, is_volatile);
                break;
            case AST.ExprType.EnumExprType.DOUBLE:
                type = new AST.TDouble(is_const, is_volatile);
                break;
            default:
                throw new Exception("Error: can't match type specifier");
            }
            return new Tuple<AST.Env, AST.ExprType>(env, type);

        } else if (nbasics > 0) {
            // partly basic specs, partly not
            throw new Exception("Error: can't match type specifier");

        } else if (type_specifiers.Count != 1) {
            // now we can only match for struct, union, function...
            throw new Exception("Error: can't match type specifier");

        } else {
            // now semant the only type spec
            return type_specifiers[0].GetExprType(env, is_const, is_volatile);

        }
    }

    // IsTypeOf
    // ========
    // Used by the parser
    // 
    public bool IsTypedef() {
        return storage_class_specifiers.FindIndex(x => x == StorageClassSpecifier.TYPEDEF) != -1;
    }

    // GetStorageClass
    // ===============
    // Infer the storage class
    // 
    public StorageClassSpecifier GetStorageClass() {
        if (storage_class_specifiers.Count == 0) {
            return StorageClassSpecifier.NULL;
        } else if (storage_class_specifiers.Count == 1) {
            return storage_class_specifiers[0];
        } else {
            Log.SemantError("Error: multiple storage class specifiers.");
            return StorageClassSpecifier.ERROR;
        }
    }

    // GetBasicType
    // ============
    // input: specs
    // output: EnumExprType
    // returns a type from a list of type specifiers
    // 
    private static AST.ExprType.EnumExprType GetBasicType(List<BTypeSpec> specs) {
        foreach (KeyValuePair<List<BTypeSpec>, AST.ExprType.EnumExprType> pair in bspecs2enumtype) {
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
    private static bool MatchSpecs(List<BTypeSpec> specs, List<BTypeSpec> key) {
        if (specs.Count != key.Count) {
            return false;
        }
        foreach (BTypeSpec spec in key) {
            if (specs.FindIndex(x => x == spec) == -1) {
                return false;
            }
        }
        return true;
    }

    // bspecs2enumtype
    // ===============
    // 
    private static Dictionary<List<BTypeSpec>, AST.ExprType.EnumExprType> bspecs2enumtype = new Dictionary<List<BTypeSpec>, AST.ExprType.EnumExprType> {

        // void : { void }
        { new List<BTypeSpec> { BTypeSpec.VOID }, AST.ExprType.EnumExprType.VOID },

        // char : { char }
        //      | { signed char }
        { new List<BTypeSpec> { BTypeSpec.CHAR }, AST.ExprType.EnumExprType.CHAR },
        { new List<BTypeSpec> { BTypeSpec.SIGNED, BTypeSpec.CHAR}, AST.ExprType.EnumExprType.CHAR },

        // uchar : { unsigned char }
        { new List<BTypeSpec> { BTypeSpec.UNSIGNED, BTypeSpec.CHAR}, AST.ExprType.EnumExprType.UCHAR },

        // short : { short }
        //       | { signed short }
        //       | { short int }
        //       | { signed short int }
        { new List<BTypeSpec> { BTypeSpec.SHORT }, AST.ExprType.EnumExprType.SHORT },
        { new List<BTypeSpec> { BTypeSpec.SIGNED, BTypeSpec.SHORT }, AST.ExprType.EnumExprType.SHORT },
        { new List<BTypeSpec> { BTypeSpec.SHORT, BTypeSpec.INT }, AST.ExprType.EnumExprType.SHORT },
        { new List<BTypeSpec> { BTypeSpec.SIGNED, BTypeSpec.SHORT, BTypeSpec.INT }, AST.ExprType.EnumExprType.SHORT },

        // ushort : { unsigned short }
        //        | { unsigned short int }
        { new List<BTypeSpec> { BTypeSpec.UNSIGNED, BTypeSpec.SHORT }, AST.ExprType.EnumExprType.USHORT },
        { new List<BTypeSpec> { BTypeSpec.UNSIGNED, BTypeSpec.SHORT, BTypeSpec.INT }, AST.ExprType.EnumExprType.USHORT },

        // long : { int }
        //      | { signed }
        //      | { signed int }
        //      | { long }
        //      | { signed long }
        //      | { long int }
        //      | { signed long int }
        { new List<BTypeSpec> { BTypeSpec.INT }, AST.ExprType.EnumExprType.LONG },
        { new List<BTypeSpec> { BTypeSpec.SIGNED }, AST.ExprType.EnumExprType.LONG },
        { new List<BTypeSpec> { BTypeSpec.SIGNED, BTypeSpec.INT }, AST.ExprType.EnumExprType.LONG },
        { new List<BTypeSpec> { BTypeSpec.LONG }, AST.ExprType.EnumExprType.LONG },
        { new List<BTypeSpec> { BTypeSpec.SIGNED, BTypeSpec.LONG }, AST.ExprType.EnumExprType.LONG },
        { new List<BTypeSpec> { BTypeSpec.LONG, BTypeSpec.INT }, AST.ExprType.EnumExprType.LONG },
        { new List<BTypeSpec> { BTypeSpec.SIGNED, BTypeSpec.LONG, BTypeSpec.INT }, AST.ExprType.EnumExprType.LONG },

        // ulong : { unsigned }
        //       | { unsigned int }
        //       | { unsigned long }
        //       | { unsigned long int }
        { new List<BTypeSpec> { BTypeSpec.UNSIGNED }, AST.ExprType.EnumExprType.ULONG },
        { new List<BTypeSpec> { BTypeSpec.UNSIGNED, BTypeSpec.INT }, AST.ExprType.EnumExprType.ULONG },
        { new List<BTypeSpec> { BTypeSpec.UNSIGNED, BTypeSpec.LONG }, AST.ExprType.EnumExprType.ULONG },
        { new List<BTypeSpec> { BTypeSpec.UNSIGNED, BTypeSpec.LONG, BTypeSpec.INT }, AST.ExprType.EnumExprType.ULONG },

        // float : { float }
        { new List<BTypeSpec> { BTypeSpec.FLOAT }, AST.ExprType.EnumExprType.FLOAT },

        // double : { double }
        //        | { long double }
        { new List<BTypeSpec> { BTypeSpec.DOUBLE }, AST.ExprType.EnumExprType.DOUBLE },
        { new List<BTypeSpec> { BTypeSpec.LONG, BTypeSpec.DOUBLE }, AST.ExprType.EnumExprType.DOUBLE },

    };

}


// InitDeclr
// =========
// initialization declarator: a normal declarator + an initialization expression
// 
public class InitDeclr : PTNode {

    public InitDeclr(Declarator _decl, Expression _init) {
        if (_decl != null) {
            declarator = _decl;
        } else {
            declarator = new NullDeclr();
        }

        if (_init != null) {
            init = _init;
        } else {
            init = new NullExpression();
        }
    }

    public Declarator declarator;
    public Expression init;


    // TODO : InitDeclr.GetInitDeclr(env, type) -> (env, type, expr) : change the type corresponding to init expression
    public Tuple<AST.Env, AST.ExprType, AST.Expr, String> GetInitDeclr(AST.Env env, AST.ExprType type) {

        Tuple<AST.Env, AST.Expr> r_init = init.GetExpr(env);
        env = r_init.Item1;
        AST.Expr ast_init = r_init.Item2;

        Tuple<AST.Env, AST.ExprType, String> r_declr = declarator.WrapExprType(env, type);
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


public enum BTypeSpec {
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
        basic = BTypeSpec.NULL;
    }
    public TypeSpecifier(BTypeSpec spec) {
        basic = spec;
    }

    // GetExprType
    // ===========
    // input: env
    // output: tuple<ExprType, Environment>
    // 
    public virtual Tuple<AST.Env, AST.ExprType> GetExprType(AST.Env env, bool is_const, bool is_volatile) {
        throw new NotImplementedException();
    }

    public BTypeSpec basic;
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
    public override Tuple<AST.Env, AST.ExprType> GetExprType(AST.Env env, bool is_const, bool is_volatile) {
        throw new NotImplementedException();
    }


    public String name;
}


public enum TypeQualifier {
    NULL,
    CONST,
    VOLATILE
}


// TypeInfo
// ========
// a base class of FunctionInfo, ArrayInfo, and PointerInfo
// 
public class TypeInfo : PTNode {
    public enum TypeInfoType {
        FUNCTION,
        ARRAY,
        POINTER
    }

    public virtual Tuple<AST.Env, AST.ExprType> WrapType(AST.Env env, AST.ExprType type) {
        throw new NotImplementedException();
    }
    public TypeInfoType type;
}

public class FunctionInfo : TypeInfo {
    public FunctionInfo(ParamTypeList _param_type_list) {
        param_type_list = _param_type_list;
        type = TypeInfoType.FUNCTION;
    }
    public ParamTypeList param_type_list;

    // TODO : [finished] FunctionInfo.Wrap(env, type) -> (env, type)
    public override Tuple<AST.Env, AST.ExprType> WrapType(AST.Env env, AST.ExprType type) {
        List<Tuple<AST.Env, String, AST.ExprType>> r_params = param_type_list.GetParamTypes(env);
        List<Tuple<String, AST.ExprType>> args = new List<Tuple<String, AST.ExprType>>();
        foreach (Tuple<AST.Env, String, AST.ExprType> r_param in r_params) {
            env = r_param.Item1;
            args.Add(new Tuple<string, AST.ExprType>(r_param.Item2, r_param.Item3));
        }
        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TFunction(type, args));
    }
}

public class ArrayInfo : TypeInfo {
    public ArrayInfo(Expression _nelems) {
        nelems = _nelems;
        type = TypeInfoType.ARRAY;
    }

    // TODO : ArrayInfo.WrapType(env, type) -> (env, type)
    public override Tuple<AST.Env, AST.ExprType> WrapType(AST.Env env, AST.ExprType type) {
        return new Tuple<AST.Env, AST.ExprType>(env, type);
    }
    public Expression nelems;
    public int __nelems;
}

public class PointerInfo : TypeInfo {
    public PointerInfo(List<TypeQualifier> _type_qualifiers) {
        type_qualifiers = _type_qualifiers;
        type = TypeInfoType.POINTER;
    }

    // TODO : [finished] PointerInfo.WrapType(env, type) -> (env, type)
    public override Tuple<AST.Env, AST.ExprType> WrapType(AST.Env env, AST.ExprType type) {
        bool is_const = type_qualifiers.Any(x => x == TypeQualifier.CONST);
        bool is_volatile = type_qualifiers.Any(x => x == TypeQualifier.VOLATILE);
        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TPointer(type, is_const, is_volatile));
    }
    public List<TypeQualifier> type_qualifiers;
}

public class Declarator : PTNode {
    public Declarator(String _name) {
        type_infos = new List<TypeInfo>();
        name = _name;
    }

    public List<TypeInfo> type_infos;
    public String name;

    // TODO : [finished] Declr.WrapExprType(env, type) -> (env, type, name) : wrap up the type
    public virtual Tuple<AST.Env, AST.ExprType, String> WrapExprType(AST.Env env, AST.ExprType type) {
        type_infos.ForEach(info => {
            Tuple<AST.Env, AST.ExprType> r = info.WrapType(env, type);
            env = r.Item1;
            type = r.Item2;
        });
        return new Tuple<AST.Env, AST.ExprType, string>(env, type, name);
    }
}

public class NullDeclr : Declarator {
    public NullDeclr() : base("") { }

    public override Tuple<AST.Env, AST.ExprType, String> WrapExprType(AST.Env env, AST.ExprType type) {
        return new Tuple<AST.Env, AST.ExprType, String>(env, type, "");
    }
}

// Finished
public class ParamTypeList : PTNode {
    public ParamTypeList(List<ParamDecln> _param_list) {
        IsVarArgs = false;
        param_list = _param_list;
    }

    public bool IsVarArgs;
    public List<ParamDecln> param_list;

    // TODO : [finished] ParamTypeList.GetParamTypes(env) -> (env, type)[]
    public List<Tuple<AST.Env, String, AST.ExprType>> GetParamTypes(AST.Env env) {
        List<Tuple<AST.Env, String, AST.ExprType>> param_types = new List<Tuple<AST.Env, String, AST.ExprType>>();
        foreach (ParamDecln decln in param_list) {
            Tuple<AST.Env, String, AST.ExprType> r_decln = decln.GetParamDecln(env);
            param_types.Add(r_decln);
        }
        return param_types;
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
public class EnumSpec : TypeSpecifier {
    public EnumSpec(String _name, List<Enumerator> _enum_list) {
        name = _name;
        enum_list = _enum_list;
    }

    public override Tuple<AST.Env, AST.ExprType> GetExprType(AST.Env env, bool is_const, bool is_volatile) {
        if (enum_list == null) {
            // if there is no content in this enum type, we must find it's definition in the environment
            AST.Env.Entry entry = env.Find("enum " + name);
            if (entry == null || entry.entry_loc != AST.Env.EntryLoc.TYPEDEF) {
                Log.SemantError("Error: type 'enum " + name + " ' has not been defined.");
                return null;
            }
        } else {
            // so there are something in this enum type, we need to put this type into the environment
            int idx = 0;
            foreach (Enumerator elem in enum_list) {
                env = env.PushEnum(elem.name, new AST.TLong(), idx);
                idx++;
            }
            env = env.PushEntry(AST.Env.EntryLoc.TYPEDEF, "enum " + name, new AST.TLong());
        }

        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TLong(is_const, is_volatile));
    }

    public readonly String name;
    public readonly List<Enumerator> enum_list;

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
public class StructOrUnionSpec : TypeSpecifier {
    public String name;
    public List<StructDecln> declns;
}


// StructSpec
// ==========
// 
public class StructSpec : StructOrUnionSpec {
    public StructSpec(String _name, List<StructDecln> _declns) {
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
    public override Tuple<AST.Env, AST.ExprType> GetExprType(AST.Env env, bool is_const, bool is_volatile) {

        // TODO : non-complete type
        if (name != "") {
            // add a non-complete type
            // env = env.PushEntry(AST.Env.EntryLoc.TYPEDEF, "struct " + name, null);
        }

        List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<string, AST.ExprType>>();
        foreach (StructDecln decln in declns) {
            Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> r_decln = decln.GetDeclns(env);
            env = r_decln.Item1;
            attribs.AddRange(r_decln.Item2);
        }

        AST.TStruct type = new AST.TStruct(attribs, is_const, is_volatile);

        if (name != "") {
            env = env.PushEntry(AST.Env.EntryLoc.TYPEDEF, "struct " + name, type);
        }

        return new Tuple<AST.Env, AST.ExprType>(env, type);
    }

}


// UnionSpec
// =========
// 
public class UnionSpec : StructOrUnionSpec {
    public UnionSpec(String _name, List<StructDecln> _decl_list) {
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
    public override Tuple<AST.Env, AST.ExprType> GetExprType(AST.Env env, bool is_const, bool is_volatile) {
        List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<string, AST.ExprType>>();
        foreach (StructDecln decln in declns) {
            Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> r_decln = decln.GetDeclns(env);
            env = r_decln.Item1;
            attribs.AddRange(r_decln.Item2);
        }

        AST.TUnion type = new AST.TUnion(attribs, is_const, is_volatile);

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
    public StructOrUnion(bool _is_union) {
        is_union = _is_union;
    }
    public bool is_union;
}


public class StructDecln : PTNode {
    public StructDecln(DeclarationSpecifiers _specs, List<Declarator> _declrs) {
        specs = _specs;
        declrs = _declrs;
    }
    public DeclarationSpecifiers specs;
    public List<Declarator> declrs;

    public Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> GetDeclns(AST.Env env) {
        Tuple<AST.Env, AST.ExprType> r_specs = specs.GetExprType(env);
        env = r_specs.Item1;
        AST.ExprType base_type = r_specs.Item2;

        List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<string, AST.ExprType>>();
        foreach (Declarator declr in declrs) {
            Tuple<AST.Env, AST.ExprType, String> r_declr = declr.WrapExprType(env, base_type);
            env = r_declr.Item1;
            AST.ExprType type = r_declr.Item2;
            String name = r_declr.Item3;
            attribs.Add(new Tuple<string, AST.ExprType>(name, type));
        }
        return new Tuple<AST.Env, List<Tuple<string, AST.ExprType>>>(env, attribs);
    }

}

// Finished.
public class ParamDecln : PTNode {
    public ParamDecln(DeclarationSpecifiers _specs, Declarator _decl) {
        specs = _specs;

        if (_decl != null) {
            decl = _decl;
        } else {
            decl = new NullDeclr();
        }
    }

    public DeclarationSpecifiers specs;
    public Declarator decl;

    // TODO : [finished] ParamDecln.GetParamDecln(env) -> (env, name, type)
    public Tuple<AST.Env, String, AST.ExprType> GetParamDecln(AST.Env env) {
        Tuple<AST.Env, AST.Decln.SCS, AST.ExprType> r_specs = specs.GetSCSType(env);
        env = r_specs.Item1;
        AST.Decln.SCS scs = r_specs.Item2;
        AST.ExprType type = r_specs.Item3;

        Tuple<AST.Env, AST.ExprType, String> r_declr = decl.WrapExprType(env, type);
        env = r_declr.Item1;
        type = r_declr.Item2;
        String name = r_declr.Item3;

        return new Tuple<AST.Env, string, AST.ExprType>(env, name, type);
    }

}


public class InitrList : Expression {
    public InitrList(List<Expression> _exprs) {
        exprs = _exprs;
    }
    public List<Expression> exprs;
}


public class TypeName : PTNode {
    public TypeName(DeclarationSpecifiers _specs, Declarator _decl) {
        specs = _specs;
        decl = _decl;
    }

    public DeclarationSpecifiers specs;
    public Declarator decl;

}

