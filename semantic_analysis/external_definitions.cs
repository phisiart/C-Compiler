using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TranslationUnit : PTNode {
    public TranslationUnit(List<PTNode> _list) {
        list = _list;
    }

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    foreach (ASTNode decl in list) {
    //        scope = decl.Semant(scope);
    //    }
    //    return scope;
    //}

    public List<PTNode> list;
}


public class FunctionDefinition : PTNode {
    public FunctionDefinition(DeclnSpecs _specs, Declr _decl, Statement _stmt) {
        specs = _specs;
        decl = _decl;
        stmt = _stmt;
    }
    public DeclnSpecs specs;
    public Declr decl;
    public Statement stmt;

    public StorageClassSpecifier __storage;

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;

    //    scope = specs.Semant(scope);
    //    decl.type = specs.type;
    //    scope = decl.Semant(scope);

    //    StorageClassSpecifier storage = specs.storage_class;
    //    switch (storage) {
    //    case StorageClassSpecifier.TYPEDEF:
    //        scope.AddSymbol(decl.name, new Symbol(Symbol.Kind.TYPEDEF, decl.type, 0));
    //        break;
    //    case StorageClassSpecifier.STATIC:
    //        scope.AddSymbol(decl.name, new Symbol(Symbol.Kind.STATIC, decl.type, 0));
    //        break;
    //    case StorageClassSpecifier.EXTERN:
    //        scope.AddSymbol(decl.name, new Symbol(Symbol.Kind.EXTERN, decl.type, 0));
    //        break;
    //    case StorageClassSpecifier.AUTO:
    //    case StorageClassSpecifier.NULL:
    //    case StorageClassSpecifier.REGISTER:
    //        scope.AddSymbol(decl.name, new Symbol(Symbol.Kind.AUTO, decl.type, 0));
    //        break;
    //    default:
    //        Log.SemantError("Error: Storage class error.");
    //        break;
    //    }
    //    if (decl.type.kind != TType.Kind.FUNCTION) {
    //        Log.SemantError("Error: not a function.");
    //    }
    //    scope.InScope();
    //    foreach (ScopeEntry entry in ((TFunction)decl.type).params_) {
    //        scope.AddSymbol(entry.name, new Symbol(Symbol.Kind.AUTO, entry.type, 0));
    //    }
    //    scope = stmt.Semant(scope);
    //    scope.OutScope();
    //    //TType type = specs.
    //    return scope;
    //}
}
