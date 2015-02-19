using System;
using System.Collections.Generic;
using System.Linq;

// basic rule: Semant(scope): return scope

public class ScopeEntry {
    public ScopeEntry(String _name, TType _type) {
        name = _name;
        type = _type;
    }
    public String name;
    public TType type;
}

public class Symbol {
    public enum Kind {
        FORMAL,
        EXTERN,
        STATIC,
        AUTO,
        TYPEDEF,
        ENUM
    }

    public Symbol(Kind _kind, TType _type, Int64 _value) {
        kind = _kind;
        type = _type;
        value = _value;
    }

    public Kind kind;
    public TType type;
    public Int64 value;

    public override String ToString() {
        switch (kind) {
            case Kind.ENUM:
                return value.ToString();
            default:
                return type.ToString();
        }
    }

}

public class Scope {
    public Scope() {
        vars = new List<String>();
        typedef_names = new List<string>();
        symbols = new Dictionary<string, Symbol>();
    }

    public bool HasVariable(String var) {
        return vars.FindIndex(x => x == var) != -1;
    }

    public bool HasTypedefName(String type) {
        return typedef_names.FindIndex(x => x == type) != -1;
    }

    public bool HasIdentifier(String id) {
        return HasVariable(id) || HasTypedefName(id);
    }

    public void AddTypedefName(String type) {
        typedef_names.Add(type);
    }

    //public void AddTypedef(ScopeEntry entry) {
    //    typedefs.Add(entry);
    //}

    public List<String> typedef_names;
    public List<String> vars;

    //public List<ScopeEntry> formals;
    //public List<ScopeEntry> externs;
    //public List<ScopeEntry> statics;
    //public List<ScopeEntry> autos;
    //public List<ScopeEntry> typedefs;
    //public Dictionary<String, Int64> enums;

    public Dictionary<String, Symbol> symbols;
}

public class ScopeSandbox {
    public ScopeSandbox() {
        scopes = new Stack<Scope>();
        scopes.Push(new Scope());
    }

    public void InScope() {
        scopes.Push(new Scope());
    }

    public void OutScope() {
        scopes.Pop();
    }

    public bool HasVariable(String var) {
        return scopes.Peek().HasVariable(var);
    }

    public bool HasTypedefName(String type) {
        return scopes.Peek().HasTypedefName(type);
    }

    public void AddTypedefName(String type) {
        scopes.Peek().AddTypedefName(type);
    }

    public bool HasIdentifier(String id) {
        return scopes.Peek().HasIdentifier(id);
    }

    public void AddSymbol(String name, Symbol symbol) {
        scopes.Peek().symbols.Add(name, symbol);
    }

    public Symbol FindSymbolInCurrentLevel(String name) {
        if (scopes.Peek().symbols.ContainsKey(name)) {
            return scopes.Peek().symbols[name];
        }
        return null;
    }

    public Symbol FindSymbol(String name) {
        foreach (Scope scope in scopes) {
            if (scope.symbols.ContainsKey(name)) {
                return scope.symbols[name];
            }
        }
        return null;
    }

    //public void AddTypedefs(List<ScopeEntry> entries) {
    //    scopes.Peek().typedefs.AddRange(entries);
    //}

    //public void AddStatics(List<ScopeEntry> entries) {
    //    scopes.Peek().statics.AddRange(entries);
    //}

    //public void AddAutos(List<ScopeEntry> entries) {
    //    scopes.Peek().autos.AddRange(entries);
    //}

    //public void AddExterns(List<ScopeEntry> entries) {
    //    scopes.Peek().externs.AddRange(entries);
    //}

    //public void AddExtern(ScopeEntry entry) {
    //    scopes.Peek().externs.Add(entry);
    //}

    //public void AddFormals(List<ScopeEntry> entries) {
    //    scopes.Peek().formals.AddRange(entries);
    //}

    //public void AddEnum(String name, Int64 value) {
    //    scopes.Peek().enums.Add(name, value);
    //}

    public bool IsGlobal() {
        return scopes.Count == 1;
    }

    public Stack<Scope> scopes;
}


public static class ScopeEnvironment {
    static ScopeEnvironment() {
        sandboxes = new Stack<ScopeSandbox>();
        sandboxes.Push(new ScopeSandbox());
    }

    public static void PushSandbox() {
        if (sandboxes.Count == 0) {
            return;
        }
        sandboxes.Push(sandboxes.Peek());
    }

    public static void PopSandbox() {
        if (sandboxes.Count < 2) {
            return;
        }
        ScopeSandbox top = sandboxes.Pop();
        sandboxes.Pop();
        sandboxes.Push(top);
    }

    public static void InScope() {
        sandboxes.Peek().InScope();
    }

    public static void OutScope() {
        sandboxes.Peek().OutScope();
    }

    public static bool HasVariable(String var) {
        return sandboxes.Peek().HasVariable(var);
    }

    public static bool HasTypedefName(String type) {
        return sandboxes.Peek().HasTypedefName(type);
    }

    public static void AddTypedefName(String type) {
        sandboxes.Peek().AddTypedefName(type);
    }

    public static bool HasIdentifier(String id) {
        return sandboxes.Peek().HasIdentifier(id);
    }

    public static Stack<ScopeSandbox> sandboxes;

}


public interface ParseRule {
}
public class PTNode {
    //// Semant: take in the scope before this node, return the scope after this node.
    //public virtual ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    return scope;
    //}

    public ScopeSandbox scope;

}


// the declaration of an object
public class Decln : ExternalDeclaration {
    public Decln(DeclnSpecs decl_specs_, List<InitDeclr> init_declrs_) {
        decl_specs = decl_specs_;
        init_declrs = init_declrs_;
        __entries = new List<ScopeEntry>();
    }
    public DeclnSpecs decl_specs;
    public List<InitDeclr> init_declrs;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;

    //    // semant declaration specifiers
    //    // to get 1) storage class 2) type
    //    scope = decl_specs.Semant(scope);

    //    foreach (InitDeclr init_declr in init_declrs) {
    //        init_declr.type = decl_specs.type;
    //        scope = init_declr.Semant(scope);

    //        switch (decl_specs.storage_class) {
    //            case StorageClassSpecifier.TYPEDEF:
    //                scope.AddSymbol(init_declr.declarator.name, new Symbol(Symbol.Kind.TYPEDEF, init_declr.type, 0));
    //                break;
    //            case StorageClassSpecifier.STATIC:
    //                scope.AddSymbol(init_declr.declarator.name, new Symbol(Symbol.Kind.STATIC, init_declr.type, 0));
    //                break;
    //            case StorageClassSpecifier.EXTERN:
    //                scope.AddSymbol(init_declr.declarator.name, new Symbol(Symbol.Kind.EXTERN, init_declr.type, 0));
    //                break;
    //            case StorageClassSpecifier.AUTO:
    //            case StorageClassSpecifier.NULL:
    //            case StorageClassSpecifier.REGISTER:
    //                scope.AddSymbol(init_declr.declarator.name, new Symbol(Symbol.Kind.AUTO, init_declr.type, 0));
    //                break;
    //            default:
    //                Log.SemantError("Error: Storage class error.");
    //                break;
    //        }

    //    }

    //    //GetEntries();
    //    //StorageClassSpecifier scs = decl_specs.GetStorageClass();
    //    //switch (scs) {
    //    //case StorageClassSpecifier.TYPEDEF:
    //    //    //scope.AddTypedefs(__entries);
    //    //    break;
    //    //case StorageClassSpecifier.STATIC:
    //    //    //scope.AddStatics(__entries);
    //    //    break;
    //    //case StorageClassSpecifier.EXTERN:
    //    //    // a declaration of an object, no storage
    //    //    //scope.AddExterns(__entries);
    //    //    break;
    //    //case StorageClassSpecifier.AUTO:
    //    //case StorageClassSpecifier.NULL:
    //    //case StorageClassSpecifier.REGISTER:
    //    //    // a normal object
    //    //    if (scope.IsGlobal()) {
    //    //        //scope.AddExterns(__entries);
    //    //    } else {
    //    //        //scope.AddAutos(__entries);
    //    //    }
    //    //    break;
    //    //default:
    //    //    Console.Error.WriteLine("Error: can't match type specifier");
    //    //    Environment.Exit(1);
    //    //    break;
    //    //}
    //    return scope;
    //}

    // TODO : Decln(env) -> (env, (env, decln)[]) : semant declarations

    public Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> GetDeclns(AST.Env env) {
        List<Tuple<AST.Env, AST.Decln>> declns = new List<Tuple<AST.Env, AST.Decln>>();

        Tuple<AST.Env, AST.Decln.EnumSCS, AST.ExprType> r_specs = decl_specs.SemantDeclnSpecs(env);
        env = r_specs.Item1;
        AST.Decln.EnumSCS scs = r_specs.Item2;
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
                case AST.Decln.EnumSCS.AUTO:
                    if (env.IsGlobal()) {
                        loc = AST.Env.EntryLoc.GLOBAL;
                    }
                    else {
                        loc = AST.Env.EntryLoc.STACK;
                    }
                    break;
                case AST.Decln.EnumSCS.EXTERN:
                    loc = AST.Env.EntryLoc.GLOBAL;
                    break;
                case AST.Decln.EnumSCS.STATIC:
                    loc = AST.Env.EntryLoc.GLOBAL;
                    break;
                case AST.Decln.EnumSCS.TYPEDEF:
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

    // calculate all the types
    public void GetEntries() {
        __entries.Clear();

        //TType type = decl_specs.GetNonQualifiedType();
        foreach (InitDeclr init_declr in init_declrs) {
            //__entries.Add(init_declr.GetEntry(type));
        }

    }
    public List<ScopeEntry> __entries;
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
public class DeclnSpecs : PTNode {
    public DeclnSpecs(List<StorageClassSpecifier> _storage_class_specifiers,
                                 List<TypeSpec> _type_specifiers,
                                 List<TypeQualifier> _type_qualifiers) {
        storage_class_specifiers = _storage_class_specifiers;
        type_qualifiers = _type_qualifiers;
        type_specifiers = _type_specifiers;
    }

    // after parsing
    // -------------
    public List<StorageClassSpecifier> storage_class_specifiers;
    public List<TypeSpec> type_specifiers;
    public List<TypeQualifier> type_qualifiers;


    // after semantic analysis
    // -----------------------
    public StorageClassSpecifier storage_class;
    public TType type;


    // TODO : [finished] DeclnSpecs.SemantDeclnSpecs(env) -> (env, scs, type)
    public Tuple<AST.Env, AST.Decln.EnumSCS, AST.ExprType> SemantDeclnSpecs(AST.Env env) {
        Tuple<AST.ExprType, AST.Env> r_type = GetExprType(env);
        AST.ExprType type = r_type.Item1;
        env = r_type.Item2;

        AST.Decln.EnumSCS scs;
        switch (GetStorageClass()) {
            case StorageClassSpecifier.AUTO:
            case StorageClassSpecifier.NULL:
            case StorageClassSpecifier.REGISTER:
                scs = AST.Decln.EnumSCS.AUTO;
                break;
            case StorageClassSpecifier.EXTERN:
                scs = AST.Decln.EnumSCS.EXTERN;
                break;
            case StorageClassSpecifier.STATIC:
                scs = AST.Decln.EnumSCS.STATIC;
                break;
            case StorageClassSpecifier.TYPEDEF:
                scs = AST.Decln.EnumSCS.TYPEDEF;
                break;
            default:
                Log.SemantError("scs error");
                return null;
        }

        return new Tuple<AST.Env, AST.Decln.EnumSCS, AST.ExprType>(env, scs, type);
    }

    //// Semant
    //// ------
    //// 1. Get storage class
    //// 2. semant type specs and get TType
    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;

    //    // 1. Get storage class
    //    storage_class = GetStorageClass();

    //    // 2. semant type specs
    //    type = SemantTypeSpecs();

    //    // 3. get is_const, is_volatile
    //    type.is_const = type_qualifiers.Exists(x => x == TypeQualifier.CONST);
    //    type.is_volatile = type_qualifiers.Exists(x => x == TypeQualifier.VOLATILE);

    //    return scope;
    //}

    // DeclnSpecs.GetExprType
    // ======================
    // input: env
    // output: ExprType, Environment
    // get the type from the specifiers
    // 
    public Tuple<AST.ExprType, AST.Env> GetExprType(AST.Env env) {

        bool is_const = type_qualifiers.Exists(qual => qual == TypeQualifier.CONST);
        bool is_volatile = type_qualifiers.Exists(qual => qual == TypeQualifier.VOLATILE);

        // 1. if no type specifier => int
        if (type_specifiers.Count == 0) {
            return new Tuple<AST.ExprType, AST.Env>(new AST.TLong(), env);
        }

        // 2. now let's analyse type specs
        int nbasics = type_specifiers.Count(spec => spec.basic != BTypeSpec.NULL);
        if (nbasics == type_specifiers.Count) {
            List<BTypeSpec> basic_specs = new List<BTypeSpec>();
            foreach (TypeSpec spec in type_specifiers) {
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
                    Log.SemantError("Error: can't match type specifier");
                    return null;
            }
            return new Tuple<AST.ExprType, AST.Env>(type, env);

        }
        else if (nbasics > 0) {
            // partly basic specs, partly not
            Log.SemantError("Error: can't match type specifier");
            return null;

        }
        else if (type_specifiers.Count != 1) {
            // now we can only match for struct, union, function...
            Log.SemantError("Error: can't match type specifier");
            return null;

        }
        else {
            // now semant the only type spec
            return type_specifiers[0].GetExprType(env, is_const, is_volatile);

        }
    }

    //// SemantTypeSpecs : used inside Semant
    //// ---------------
    //// 1. Semant (update scope)
    //// 2. Get TType
    //public TType SemantTypeSpecs() {

    //    // 1. if no type specifier, return INT
    //    if (type_specifiers.Count == 0) {
    //        return new TInt();
    //    }

    //    // 2. semant type specs.
    //    int nbasics = type_specifiers.Count(x => x.basic != BTypeSpec.NULL);

    //    // 2.1. try to match basic type
    //    if (nbasics == type_specifiers.Count) {
    //        List<BTypeSpec> basic_specs = new List<BTypeSpec>();
    //        foreach (TypeSpec spec in type_specifiers) {
    //            basic_specs.Add(spec.basic);
    //        }
    //        return MatchBasicType(basic_specs);
    //    }

    //    // 2.2. you cannot have a part of basic specs.
    //    if (nbasics != 0) {
    //        Console.Error.WriteLine("Error: can't match type specifier");
    //        Environment.Exit(1);
    //        return null;
    //    }

    //    // 2.3. try to match struct, union, enum ...
    //    if (type_specifiers.Count != 1) {
    //        Console.Error.WriteLine("Error: can't match type specifier");
    //        Environment.Exit(1);
    //        return null;
    //    }

    //    scope = type_specifiers[0].Semant(scope);
    //    return type_specifiers[0].type;

    //}


    public bool IsTypedef() {
        return storage_class_specifiers.FindIndex(x => x == StorageClassSpecifier.TYPEDEF) != -1;
    }


    // GetStorageClass
    // ---------------
    // Infer the storage class
    public StorageClassSpecifier GetStorageClass() {
        return GetStorageClass(storage_class_specifiers);
    }


    // GetStorageClass(list of storage class specs)
    // --------------------------------------------
    // Infer the storage class
    public static StorageClassSpecifier GetStorageClass(List<StorageClassSpecifier> storage_class_specifiers) {
        if (storage_class_specifiers.Count == 0) {
            return StorageClassSpecifier.NULL;

        }
        else if (storage_class_specifiers.Count == 1) {
            return storage_class_specifiers[0];

        }
        else {
            Console.Error.WriteLine("Error: storage class specifier");
            Environment.Exit(1);
            return StorageClassSpecifier.ERROR;
        }
    }


    // MatchBasicType
    // ==============
    // 
    // private
    // On fail: Exit
    private static TType MatchBasicType(List<BTypeSpec> specs) {
        foreach (var pair in type_map) {
            if (MatchSpecs(specs, pair.Key)) {
                return pair.Value;
            }
        }
        Console.Error.WriteLine("Error: can't match type specifier");
        Environment.Exit(1);
        return null;
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
    #region bspecs2enumtype
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

    #endregion

    #region type_map : { basic type specs } -> basic type
    // --------------------------------------------------
    private static Dictionary<List<BTypeSpec>, TType> type_map = new Dictionary<List<BTypeSpec>, TType> {
        { new List<BTypeSpec> { BTypeSpec.VOID }, new TVoid() },

        { new List<BTypeSpec> { BTypeSpec.CHAR }, new TChar() },
        { new List<BTypeSpec> { BTypeSpec.SIGNED, BTypeSpec.CHAR}, new TChar() },

        { new List<BTypeSpec> { BTypeSpec.UNSIGNED, BTypeSpec.CHAR}, new TUChar() },

        { new List<BTypeSpec> { BTypeSpec.SHORT }, new TShort() },
        { new List<BTypeSpec> { BTypeSpec.SIGNED, BTypeSpec.SHORT }, new TShort() },
        { new List<BTypeSpec> { BTypeSpec.SHORT, BTypeSpec.INT }, new TShort() },
        { new List<BTypeSpec> { BTypeSpec.SIGNED, BTypeSpec.SHORT, BTypeSpec.INT }, new TShort() },

        { new List<BTypeSpec> { BTypeSpec.UNSIGNED, BTypeSpec.SHORT }, new TUShort() },
        { new List<BTypeSpec> { BTypeSpec.UNSIGNED, BTypeSpec.SHORT, BTypeSpec.INT }, new TUShort() },

        { new List<BTypeSpec> { BTypeSpec.INT }, new TInt() },
        { new List<BTypeSpec> { BTypeSpec.SIGNED }, new TInt() },
        { new List<BTypeSpec> { BTypeSpec.SIGNED, BTypeSpec.INT }, new TInt() },

        { new List<BTypeSpec> { BTypeSpec.UNSIGNED }, new TUInt() },
        { new List<BTypeSpec> { BTypeSpec.UNSIGNED, BTypeSpec.INT }, new TUInt() },

        { new List<BTypeSpec> { BTypeSpec.LONG }, new TLong() },
        { new List<BTypeSpec> { BTypeSpec.SIGNED, BTypeSpec.LONG }, new TLong() },
        { new List<BTypeSpec> { BTypeSpec.LONG, BTypeSpec.INT }, new TLong() },
        { new List<BTypeSpec> { BTypeSpec.SIGNED, BTypeSpec.LONG, BTypeSpec.INT }, new TLong() },

        { new List<BTypeSpec> { BTypeSpec.UNSIGNED, BTypeSpec.LONG }, new TULong() },
        { new List<BTypeSpec> { BTypeSpec.UNSIGNED, BTypeSpec.LONG, BTypeSpec.INT }, new TULong() },

        { new List<BTypeSpec> { BTypeSpec.FLOAT }, new TFloat() },

        { new List<BTypeSpec> { BTypeSpec.DOUBLE }, new TDouble() },

        { new List<BTypeSpec> { BTypeSpec.LONG, BTypeSpec.DOUBLE }, new TLongDouble() }

    };
    #endregion


    //// use the type_specs to figure out the type
    //public TType GetNonQualifiedType() {
    //    return GetNonQualifiedType(type_specifiers);
    //}


    //// GetTType(list of type specs)
    //// ----------------------------
    //// inter the type
    //// could be a basic type or struct or union or enum
    //public static TType GetNonQualifiedType(List<TypeSpec> type_specifiers) {

    //    // if no type specifier, return INT
    //    if (type_specifiers.Count == 0) {
    //        return new TInt();
    //    }

    //    int basic_count = type_specifiers.Count(x => x.basic != BTypeSpec.NULL);

    //    if (basic_count == type_specifiers.Count) {

    //        // try to match basic type
    //        List<BTypeSpec> basic_specs = new List<BTypeSpec>();
    //        foreach (TypeSpec spec in type_specifiers) {
    //            basic_specs.Add(spec.basic);
    //        }
    //        return MatchBasicType(basic_specs);

    //    } else if (basic_count == 0) {

    //        // try to match a type specifier
    //        if (type_specifiers.Count != 1) {
    //            Console.Error.WriteLine("Error: can't match type specifier");
    //            Environment.Exit(1);
    //            return null;
    //        }
    //        return type_specifiers[0].GetTType();

    //    } else {

    //        // error
    //        Console.Error.WriteLine("Error: can't match type specifier");
    //        Environment.Exit(1);
    //        return null;

    //    }


    //}

}


// InitDeclr
// =========
// initialization declarator: a normal declarator + an initialization expression
// 
public class InitDeclr : PTNode {

    public InitDeclr(Declr _decl, Expression _init) {
        if (_decl != null) {
            declarator = _decl;
        }
        else {
            declarator = new NullDeclr();
        }

        if (_init != null) {
            init = _init;
        }
        else {
            init = new NullExpression();
        }
    }


    // after parsing
    public Declr declarator;
    public Expression init;


    // after semant
    public TType type;


    // wrap the type
    public ScopeEntry GetEntry(TType _type) {
        type = declarator.WrapTType(_type);
        return new ScopeEntry(declarator.name, type);
    }


    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;

    //    if (init != null) {
    //        scope = init.Semant(scope);
    //    }

    //    // type is already the primitive type.
    //    declarator.type = type;
    //    scope = declarator.Semant(scope);
    //    type = declarator.type;

    //    return scope;
    //}

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
public class TypeSpec : PTNode {
    public TypeSpec() {
        basic = BTypeSpec.NULL;
    }
    public TypeSpec(BTypeSpec spec) {
        basic = spec;
    }

    // GetExprType
    // ===========
    // input: env
    // output: tuple<ExprType, Environment>
    // 
    public virtual Tuple<AST.ExprType, AST.Env> GetExprType(AST.Env env, bool is_const, bool is_volatile) {
        return null;
    }

    public TType type;
    public TType GetTType() {
        return null;
    }
    public BTypeSpec basic;
}


// this is just temporary
public class TypedefName : TypeSpec {
    public TypedefName(String _name) {
        name = _name;
    }

    // GetExprType
    // ===========
    // input: env, is_const, is_volatile
    // output: tuple<ExprType, Environment>
    // 
    // ** NOT FINISHED **
    // 
    public override Tuple<AST.ExprType, AST.Env> GetExprType(AST.Env env, bool is_const, bool is_volatile) {
        return null;
    }

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;

    //    Symbol symbol = scope.FindSymbol(name);
    //    if (symbol == null) {
    //        Log.SemantError("Error: cannot find typedef name.");
    //    }

    //    if (symbol.kind != Symbol.Kind.TYPEDEF) {
    //        Log.SemantError("Error: expected typedef name.");
    //    }

    //    type = symbol.type;

    //    return scope;
    //}

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
    public virtual TType WrapTType(TType type) {
        return type;
    }
    public virtual Tuple<AST.Env, AST.ExprType> WrapType(AST.Env env, AST.ExprType type) {
        return null;
    }
    public TypeInfoType type;
}

public class FunctionInfo : TypeInfo {
    public FunctionInfo(ParamTypeList _param_type_list) {
        param_type_list = _param_type_list;
        type = TypeInfoType.FUNCTION;
    }
    public ParamTypeList param_type_list;
    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = param_type_list.Semant(scope);
    //    return scope;
    //}
    //public override TType WrapTType(TType type) {
    //    return new TFunction(param_type_list.__params, param_type_list.IsVarArgs);
    //}

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
    public override TType WrapTType(TType type) {
        return new TArray(type, __nelems);
    }
    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = nelems.Semant(scope);
    //    __nelems = 0;
    //    return scope;
    //}

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

    public override TType WrapTType(TType type) {
        TPointer ptr = new TPointer(type);
        ptr.is_const = type_qualifiers.Any(x => x == TypeQualifier.CONST);
        ptr.is_volatile = type_qualifiers.Any(x => x == TypeQualifier.VOLATILE);
        return ptr;
    }

    // TODO : [finished] PointerInfo.WrapType(env, type) -> (env, type)
    public override Tuple<AST.Env, AST.ExprType> WrapType(AST.Env env, AST.ExprType type) {
        bool is_const = type_qualifiers.Any(x => x == TypeQualifier.CONST);
        bool is_volatile = type_qualifiers.Any(x => x == TypeQualifier.VOLATILE);
        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TPointer(type, is_const, is_volatile));
    }
    public List<TypeQualifier> type_qualifiers;
}

public class Declr : PTNode {
    public Declr(String _name) {
        type_infos = new List<TypeInfo>();
        name = _name;
    }

    // after parsing
    public List<TypeInfo> type_infos;
    public String name;

    // after semant
    public TType type;

    //// Wrap the type
    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    foreach (TypeInfo info in type_infos) {
    //        scope = info.Semant(scope);
    //    }
    //    type = WrapTType(type);
    //    return scope;
    //}

    public TType WrapTType(TType type) {
        type_infos.ForEach(info => type = info.WrapTType(type));
        return type;
    }

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

public class NullDeclr : Declr {
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

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    foreach (ParamDecln param in param_list) {
    //        scope = param.Semant(scope);
    //    }
    //    __params = new List<ScopeEntry>();
    //    foreach (ParamDecln param in param_list) {
    //        __params.Add(param.__entry);
    //    }
    //    return scope;
    //}

    // TODO : [finished] ParamTypeList.GetParamTypes(env) -> (env, type)[]
    public List<Tuple<AST.Env, String, AST.ExprType>> GetParamTypes(AST.Env env) {
        List<Tuple<AST.Env, String, AST.ExprType>> param_types = new List<Tuple<AST.Env, String, AST.ExprType>>();
        foreach (ParamDecln decln in param_list) {
            Tuple<AST.Env, String, AST.ExprType> r_decln = decln.GetParamDecln(env);
            param_types.Add(r_decln);
        }
        return param_types;
    }

    public List<ScopeEntry> __params;
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
public class EnumSpec : TypeSpec {
    public EnumSpec(String _name, List<Enumerator> _enum_list) {
        name = _name;
        enum_list = _enum_list;
    }

    public override Tuple<AST.ExprType, AST.Env> GetExprType(AST.Env env, bool is_const, bool is_volatile) {
        if (enum_list == null) {
            // if there is no content in this enum type, we must find it's definition in the environment
            AST.Env.Entry entry = env.Find("enum " + name);
            if (entry == null || entry.entry_loc != AST.Env.EntryLoc.TYPEDEF) {
                Log.SemantError("Error: type 'enum " + name + " ' has not been defined.");
                return null;
            }
        }
        else {
            // so there are something in this enum type, we need to put this type into the environment
            int idx = 0;
            foreach (Enumerator elem in enum_list) {
                env = env.PushEnum(elem.name, new AST.TLong(), idx);
                idx++;
            }
            env = env.PushEntry(AST.Env.EntryLoc.TYPEDEF, "enum " + name, new AST.TLong());
        }
        return new Tuple<AST.ExprType, AST.Env>(new AST.TLong(is_const, is_volatile), env);
    }

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;

    //    if (scope.FindSymbol("enum " + name) != null) {
    //        // if the enum type is already in scope, that means there is a definition.
    //        if (enum_list != null && enum_list.Count != 0) {
    //            Log.SemantError("Error: type 'enum " + name + " ' redifinition.");
    //        }
    //    }
    //    else {
    //        // if the enum type is not found in scope, we must make sure that this is a definition.
    //        if (enum_list == null) {
    //            Log.SemantError("Error: type 'enum " + name + " ' has not been defined.");
    //        }
    //        scope.AddSymbol("enum " + name, new Symbol(Symbol.Kind.TYPEDEF, new TInt(), 0));
    //    }

    //    int idx = 0;
    //    foreach (Enumerator elem in enum_list) {
    //        scope = elem.Semant(scope);
    //        if (scope.FindSymbolInCurrentLevel(elem.name) != null) {
    //            Log.SemantError("Error: " + elem.name + " already defined.");
    //        }
    //        scope.AddSymbol(elem.name, new Symbol(Symbol.Kind.ENUM, new TInt(), idx));
    //        idx++;
    //    }

    //    type = new TInt();
    //    return scope;
    //}

    public String name;
    public List<Enumerator> enum_list;

}


public class Enumerator : PTNode {
    public Enumerator(String _name, Expression _init) {
        name = _name;
        init = _init;
    }

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    if (init != null) {
    //        scope = init.Semant(scope);
    //    }
    //    return scope;
    //}
    public Expression init;
    public String name;
}


// StructOrUnionSpec
// =================
// a base class of StructSpec and UnionSpec
// not present in the semant phase
// 
public class StructOrUnionSpec : TypeSpec {
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
    public override Tuple<AST.ExprType, AST.Env> GetExprType(AST.Env env, bool is_const, bool is_volatile) {

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

        return new Tuple<AST.ExprType, AST.Env>(type, env);
    }

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    foreach (StructDecln decl in declns) {
    //        scope = decl.Semant(scope);
    //    }
    //    TStruct _type = new TStruct();
    //    foreach (StructDecln decl in declns) {
    //        foreach (Declr d in decl.declrs) {
    //            _type.attribs.Add(new ScopeEntry(d.name, d.type));
    //        }
    //    }
    //    type = _type;
    //    return scope;
    //}
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
    public override Tuple<AST.ExprType, AST.Env> GetExprType(AST.Env env, bool is_const, bool is_volatile) {
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

        return new Tuple<AST.ExprType, AST.Env>(type, env);
    }

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    foreach (StructDecln decl in declns) {
    //        scope = decl.Semant(scope);
    //    }
    //    TUnion _type = new TUnion();
    //    foreach (StructDecln decl in declns) {
    //        foreach (Declr d in decl.declrs) {
    //            _type.attribs.Add(new ScopeEntry(d.name, d.type));
    //        }
    //    }
    //    type = _type;
    //    return scope;
    //}
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
    public StructDecln(DeclnSpecs _specs, List<Declr> _declrs) {
        specs = _specs;
        declrs = _declrs;
    }
    public DeclnSpecs specs;
    public List<Declr> declrs;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = specs.Semant(scope);
    //    foreach (Declr decl in declrs) {
    //        decl.type = specs.type;
    //        scope = decl.Semant(scope);
    //    }
    //    return scope;
    //}

    public Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> GetDeclns(AST.Env env) {
        Tuple<AST.ExprType, AST.Env> r_specs = specs.GetExprType(env);
        AST.ExprType base_type = r_specs.Item1;
        env = r_specs.Item2;

        List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<string, AST.ExprType>>();
        foreach (Declr declr in declrs) {
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
    public ParamDecln(DeclnSpecs _specs, Declr _decl) {
        specs = _specs;

        if (_decl != null) {
            decl = _decl;
        }
        else {
            decl = new NullDeclr();
        }
    }

    public DeclnSpecs specs;
    public Declr decl;

    //// Create __entry
    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    scope = specs.Semant(scope);
    //    decl.type = specs.type;
    //    scope = decl.Semant(scope);
    //    __entry = new ScopeEntry(decl.name, decl.type);
    //    return scope;
    //}

    // TODO : [finished] ParamDecln.GetParamDecln(env) -> (env, name, type)
    public Tuple<AST.Env, String, AST.ExprType> GetParamDecln(AST.Env env) {
        Tuple<AST.Env, AST.Decln.EnumSCS, AST.ExprType> r_specs = specs.SemantDeclnSpecs(env);
        env = r_specs.Item1;
        AST.Decln.EnumSCS scs = r_specs.Item2;
        AST.ExprType type = r_specs.Item3;

        Tuple<AST.Env, AST.ExprType, String> r_declr = decl.WrapExprType(env, type);
        env = r_declr.Item1;
        type = r_declr.Item2;
        String name = r_declr.Item3;

        return new Tuple<AST.Env, string, AST.ExprType>(env, name, type);
    }

    // After semant
    public ScopeEntry __entry;
}


public class InitrList : Expression {
    public InitrList(List<Expression> _exprs) {
        exprs = _exprs;
    }
    public List<Expression> exprs;
}


public class TypeName : PTNode {
    public TypeName(DeclnSpecs _specs, Declr _decl) {
        specs = _specs;
        decl = _decl;
    }

    public TypeName(TType _type) {
        type = _type;
    }

    // after parsing
    public DeclnSpecs specs;
    public Declr decl;

    // after semant
    public TType type;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;

    //    scope = specs.Semant(scope);

    //    decl.type = specs.type;
    //    scope = decl.Semant(scope);
    //    type = decl.type;

    //    return scope;
    //}

}

