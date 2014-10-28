using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TranslationUnit : ASTNode {
    public TranslationUnit(List<ASTNode> _list) {
        list = _list;
    }

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;
        foreach (ASTNode decl in list) {
            scope = decl.Semant(scope);
        }
        return scope;
    }

    public List<ASTNode> list;
}


public class FunctionDefinition : ASTNode {
    public FunctionDefinition(DeclnSpecs _specs, Declr _decl, Statement _stmt) {
        specs = _specs;
        decl = _decl;
        stmt = _stmt;
    }
    public DeclnSpecs specs;
    public Declr decl;
    public Statement stmt;

    public StorageClassSpecifier __storage;

    public override ScopeSandbox Semant(ScopeSandbox _scope) {
        scope = _scope;

        StorageClassSpecifier storage = specs.GetStorageClass();
        //TType type = DeclnSpecs.GetNonQualifiedType(specs.type_specifiers);
        decl.Semant(scope);

        //TType type = specs.
        return scope;
    }
}
