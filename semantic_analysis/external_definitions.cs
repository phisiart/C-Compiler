using System;
using System.Collections.Generic;


public class TranslationUnit : PTNode {
    public TranslationUnit(List<ExternalDeclaration> _list) {
        list = _list;
    }

    //public override ScopeSandbox Semant(ScopeSandbox _scope) {
    //    scope = _scope;
    //    foreach (ASTNode decl in list) {
    //        scope = decl.Semant(scope);
    //    }
    //    return scope;
    //}

    public Tuple<AST.Env, AST.TranslationUnit> GetTranslationUnit() {
        List<Tuple<AST.Env, AST.ExternDecln>> declns = new List<Tuple<AST.Env, AST.ExternDecln>>();
        AST.Env env = new AST.Env();

        foreach (ExternalDeclaration decln in list) {
            Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>> r_decln = decln.GetExternDecln(env);
            env = r_decln.Item1;
            declns.AddRange(r_decln.Item2);
        }

        return new Tuple<AST.Env, AST.TranslationUnit>(env, new AST.TranslationUnit(declns));
    }

    public List<ExternalDeclaration> list;
}


public abstract class ExternalDeclaration : PTNode {
    public abstract Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>> GetExternDecln(AST.Env env);
}

public class FunctionDefinition : ExternalDeclaration {
    public FunctionDefinition(DeclnSpecs _specs, Declr _decl, Statement _stmt) {
        func_specs = _specs;
        func_declr = _decl;
        func_stmt = _stmt;
    }
    public DeclnSpecs func_specs;
    public Declr func_declr;
    public Statement func_stmt;


    public StorageClassSpecifier __storage;

    public Tuple<AST.Env, AST.FuncDef> GetDef(AST.Env env) {
        Tuple<AST.Env, AST.Decln.EnumSCS, AST.ExprType> r_specs = func_specs.SemantDeclnSpecs(env);
        env = r_specs.Item1;
        AST.Decln.EnumSCS scs = r_specs.Item2;
        AST.ExprType base_type = r_specs.Item3;

        Tuple<AST.Env, AST.ExprType, String> r_declr = func_declr.WrapExprType(env, base_type);
        env = r_declr.Item1;
        AST.ExprType type = r_declr.Item2;

        AST.TFunction func_type;
        if (type.expr_type == AST.ExprType.EnumExprType.FUNCTION) {
            func_type = (AST.TFunction)type;
        } else {
            Log.SemantError("Error: not a function");
            return null;
        }

        String name = r_declr.Item3;

        switch (scs) {
        case AST.Decln.EnumSCS.AUTO:
        case AST.Decln.EnumSCS.EXTERN:
        case AST.Decln.EnumSCS.STATIC:
            env = env.PushEntry(AST.Env.EntryLoc.GLOBAL, name, type);
            break;
        default:
            Log.SemantError("Error: invalid storage class specifier for function definition.");
            return null;
        }

        env = env.SetCurrentFunction(func_type);

        Tuple<AST.Env, AST.Stmt> r_stmt = func_stmt.GetStmt(env);
        env = r_stmt.Item1;
        AST.Stmt stmt = r_stmt.Item2;

        env = env.SetCurrentFunction(new AST.TEmptyFunction());

        return new Tuple<AST.Env, AST.FuncDef>(env, new AST.FuncDef(name, func_type, stmt));

    }

    public override Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>> GetExternDecln(AST.Env env) {
        Tuple<AST.Env, AST.FuncDef> r_def = GetDef(env);
        return new Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>>(
            r_def.Item1,
            new List<Tuple<AST.Env, AST.ExternDecln>>() {
                new Tuple<AST.Env, AST.ExternDecln>(r_def.Item1, r_def.Item2)
            }
        );
    }

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
