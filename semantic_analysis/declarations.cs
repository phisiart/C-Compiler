using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


public interface PTNode {
}
public class ASTNode {
    // Semant: take in the scope before this node, return the scope after this node.
    public virtual ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        return scope;
    }
    
    public ScopeSandbox scope;

}


// the declaration of an object
public class Decln : ASTNode {
    public Decln(DeclnSpecs decl_specs_, List<InitDeclr> init_declarators_) {
        decl_specs = decl_specs_;
        init_declarators = init_declarators_;
        __entries = new List<ScopeEntry>();
    }
    public DeclnSpecs decl_specs;
    public List<InitDeclr> init_declarators;

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;

        // semant declaration specifiers
        // to get 1) storage class 2) type
        scope = decl_specs.Semant(scope);
        
        foreach (InitDeclr init_declr in init_declarators) {
            init_declr.type = decl_specs.type;
            scope = init_declr.Semant(scope);

            switch (decl_specs.storage_class) {
            case StorageClassSpecifier.TYPEDEF:
                scope.AddSymbol(init_declr.declarator.name, new Symbol(Symbol.Kind.TYPEDEF, init_declr.type, 0));
                break;
            case StorageClassSpecifier.STATIC:
                scope.AddSymbol(init_declr.declarator.name, new Symbol(Symbol.Kind.STATIC, init_declr.type, 0));
                break;
            case StorageClassSpecifier.EXTERN:
                scope.AddSymbol(init_declr.declarator.name, new Symbol(Symbol.Kind.EXTERN, init_declr.type, 0));
                break;
            case StorageClassSpecifier.AUTO:
            case StorageClassSpecifier.NULL:
            case StorageClassSpecifier.REGISTER:
                scope.AddSymbol(init_declr.declarator.name, new Symbol(Symbol.Kind.AUTO, init_declr.type, 0));
                break;
            default:
                Log.SemantError("Error: Storage class error.");
                break;
            }

        }

        //GetEntries();
        //StorageClassSpecifier scs = decl_specs.GetStorageClass();
        //switch (scs) {
        //case StorageClassSpecifier.TYPEDEF:
        //    //scope.AddTypedefs(__entries);
        //    break;
        //case StorageClassSpecifier.STATIC:
        //    //scope.AddStatics(__entries);
        //    break;
        //case StorageClassSpecifier.EXTERN:
        //    // a declaration of an object, no storage
        //    //scope.AddExterns(__entries);
        //    break;
        //case StorageClassSpecifier.AUTO:
        //case StorageClassSpecifier.NULL:
        //case StorageClassSpecifier.REGISTER:
        //    // a normal object
        //    if (scope.IsGlobal()) {
        //        //scope.AddExterns(__entries);
        //    } else {
        //        //scope.AddAutos(__entries);
        //    }
        //    break;
        //default:
        //    Console.Error.WriteLine("Error: can't match type specifier");
        //    Environment.Exit(1);
        //    break;
        //}
        return scope;
    }

    // calculate all the types
    public void GetEntries() {
        __entries.Clear();

        //TType type = decl_specs.GetNonQualifiedType();
        foreach (InitDeclr init_declr in init_declarators) {
            //__entries.Add(init_declr.GetEntry(type));
        }

    }
    public List<ScopeEntry> __entries;
}


public class DeclnSpecs : ASTNode {
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


    // Semant
    // ------
    // 1. Get storage class
    // 2. semant type specs and get TType
    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;

        // 1. Get storage class
        storage_class = GetStorageClass();

        // 2. semant type specs
        type = SemantTypeSpecs();

        // 3. get is_const, is_volatile
        type.is_const = type_qualifiers.Exists(x => x == TypeQualifier.CONST);
        type.is_volatile = type_qualifiers.Exists(x => x == TypeQualifier.VOLATILE);

        return scope;
    }


    // SemantTypeSpecs : used inside Semant
    // ---------------
    // 1. Semant (update scope)
    // 2. Get TType
    public TType SemantTypeSpecs() {

        // 1. if no type specifier, return INT
        if (type_specifiers.Count == 0) {
            return new TInt();
        }

        // 2. semant type specs.
        int nbasics = type_specifiers.Count(x => x.basic != BTypeSpec.NULL);

        // 2.1. try to match basic type
        if (nbasics == type_specifiers.Count) {
            List<BTypeSpec> basic_specs = new List<BTypeSpec>();
            foreach (TypeSpec spec in type_specifiers) {
                basic_specs.Add(spec.basic);
            }
            return MatchBasicType(basic_specs);
        }
        
        // 2.2. you cannot have a part of basic specs.
        if (nbasics != 0) {
            Console.Error.WriteLine("Error: can't match type specifier");
            Environment.Exit(1);
            return null;
        }
        
        // 2.3. try to match struct, union, enum ...
        if (type_specifiers.Count != 1) {
            Console.Error.WriteLine("Error: can't match type specifier");
            Environment.Exit(1);
            return null;
        }

        scope = type_specifiers[0].Semant(scope);
        return type_specifiers[0].type;

    }
    
    
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

        } else if (storage_class_specifiers.Count == 1) {
            return storage_class_specifiers[0];

        } else {
            Console.Error.WriteLine("Error: storage class specifier");
            Environment.Exit(1);
            return StorageClassSpecifier.ERROR;
        }
    }

    
    // MatchBasicType(list of specs)
    // -----------------------------
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
    

    // MatchSpecs(specs, key specs)
    // ----------------------------
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


public class InitDeclr : ASTNode {

    public InitDeclr(Declr _decl, Expression _init) {
        declarator = _decl;
        init = _init;
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


    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;

        if (init != null) {
            scope = init.Semant(scope);
        }

        // type is already the primitive type.
        declarator.type = type;
        scope = declarator.Semant(scope);
        type = declarator.type;

        return scope;
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
public class TypeSpec : ASTNode {
    public TypeSpec() {
        basic = BTypeSpec.NULL;
    }
    public TypeSpec(BTypeSpec spec) {
        basic = spec;
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

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;

        Symbol symbol = scope.FindSymbol(name);
        if (symbol == null) {
            Log.SemantError("Error: cannot find typedef name.");
        }

        if (symbol.kind != Symbol.Kind.TYPEDEF) {
            Log.SemantError("Error: expected typedef name.");
        }

        type = symbol.type;

        return scope;
    }

    public String name;
}


public enum TypeQualifier {
    NULL,
    CONST,
    VOLATILE
}


public class TypeInfo : ASTNode {
    public enum TypeInfoType {
        FUNCTION,
        ARRAY,
        POINTER
    }
    public virtual TType WrapTType(TType type) {
        return type;
    }
    public TypeInfoType type;
}

public class FunctionInfo : TypeInfo {
    public FunctionInfo(ParamTypeList _param_type_list) {
        param_type_list = _param_type_list;
        type = TypeInfoType.FUNCTION;
    }
    public ParamTypeList param_type_list;
}

public class ArrayInfo : TypeInfo {
    public ArrayInfo(Expression _nelems) {
        nelems = _nelems;
        type = TypeInfoType.ARRAY;
    }
    public override TType WrapTType(TType type) {
        return new TArray(type, __nelems);
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

    public List<TypeQualifier> type_qualifiers;
}

public class Declr : ASTNode {
    public Declr(String _name) {
        type_infos = new List<TypeInfo>();
        name = _name;
    }
    
    // after parsing
    public List<TypeInfo> type_infos;
    public String name;
    
    // after semant
    public TType type;

    // Wrap the type
    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        type = WrapTType(type);
        return scope;
    }

    public TType WrapTType(TType type) {
        type_infos.ForEach(info => type = info.WrapTType(type));
        return type;
    }
    
}


public class ParamTypeList : ASTNode {
    public ParamTypeList(List<ParamDecln> _param_list) {
        IsVarArgs = false;
        param_list = _param_list;
    }

    public bool IsVarArgs;
    public List<ParamDecln> param_list;

    public List<ScopeEntry> __params;
}


public class EnumSpec : TypeSpec {
    public EnumSpec(String _name, List<Enumerator> _enum_list) {
        name = _name;
        enum_list = _enum_list;
    }

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;

        if (scope.FindSymbol("enum " + name) != null) {
            // if the enum type is already in scope, that means there is a definition.
            if (enum_list != null && enum_list.Count != 0) {
                Log.SemantError("Error: type 'enum " + name + " ' redifinition.");
            }
        } else {
            // if the enum type is not found in scope, we must make sure that this is a definition.
            if (enum_list == null) {
                Log.SemantError("Error: type 'enum " + name + " ' has not been defined.");
            }
            scope.AddSymbol("enum " + name, new Symbol(Symbol.Kind.TYPEDEF, new TInt(), 0));
        }

        int idx = 0;
        foreach (Enumerator elem in enum_list) {
            scope = elem.Semant(scope);
            if (scope.FindSymbolInCurrentLevel(elem.name) != null) {
                Log.SemantError("Error: " + elem.name + " already defined.");
            }
            scope.AddSymbol(elem.name, new Symbol(Symbol.Kind.ENUM, new TInt(), idx));
            idx++;
        }

        type = new TInt();
        return scope;
    }

    public String name;
    public List<Enumerator> enum_list;

}


public class Enumerator : ASTNode {
    public Enumerator(String _name, Expression _init) {
        name = _name;
        init = _init;
    }

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        if (init != null) {
            scope = init.Semant(scope);
        }
        return scope;
    }
    public Expression init;
    public String name;
}


public class StructOrUnionSpec : TypeSpec {
    public String name;
    public List<StructDecln> decl_list;
}


public class StructSpec : StructOrUnionSpec {
    public StructSpec(String _name, List<StructDecln> _decl_list) {
        name = _name;
        decl_list = _decl_list;
    }

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;

        return scope;
    }
}


public class UnionSpec : StructOrUnionSpec {
    public UnionSpec(String _name, List<StructDecln> _decl_list) {
        name = _name;
        decl_list = _decl_list;
    }
}


public class StructOrUnion : ASTNode {
    public StructOrUnion(bool _is_union) {
        is_union = _is_union;
    }
    public bool is_union;
}


public class StructDecln : ASTNode {
    public StructDecln(DeclnSpecs _specs, List<Declr> _decl_list) {
        specs = _specs;
        decl_list = _decl_list;
    }
    public DeclnSpecs specs;
    public List<Declr> decl_list;
}


public class ParamDecln : ASTNode {
    public ParamDecln(DeclnSpecs _specs, Declr _decl) {
        specs = _specs;
        decl = _decl;
    }

    public DeclnSpecs specs;
    public Declr decl;

    public ScopeEntry __entry;
}


public class InitrList : Expression {
    public InitrList(List<Expression> _exprs) {
        exprs = _exprs;
    }
    public List<Expression> exprs;
}


public class TypeName : ASTNode {
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

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;

        scope = specs.Semant(scope);

        decl.type = specs.type;
        scope = decl.Semant(scope);
        type = decl.type;

        return scope;
    }
   
}

