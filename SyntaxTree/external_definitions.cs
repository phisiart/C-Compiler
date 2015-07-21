using System;
using System.Collections.Generic;

namespace SyntaxTree {

    public class TranslationUnit : PTNode {
        public TranslationUnit(List<ExternalDeclaration> _declns) {
            unit_declns = _declns;
        }

        public Tuple<AST.Env, AST.TranslnUnit> GetTranslationUnit() {
            List<Tuple<AST.Env, AST.ExternDecln>> declns = new List<Tuple<AST.Env, AST.ExternDecln>>();
            AST.Env env = new AST.Env();

            foreach (ExternalDeclaration decln in unit_declns) {
                Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>> r_decln = decln.GetExternDecln(env);
                env = r_decln.Item1;
                declns.AddRange(r_decln.Item2);
            }

            return new Tuple<AST.Env, AST.TranslnUnit>(env, new AST.TranslnUnit(declns));
        }

        public List<ExternalDeclaration> unit_declns;
    }


    public abstract class ExternalDeclaration : PTNode {
        public abstract Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>> GetExternDecln(AST.Env env);
    }

    // Function Definition
    // ===================
    // 
    public class FunctionDefinition : ExternalDeclaration {
        public FunctionDefinition(DeclnSpecs _specs, Declr _decl, CompoundStatement _stmt) {
            func_specs = _specs;
            func_declr = _decl;
            func_stmt = _stmt;
        }

        public readonly DeclnSpecs func_specs;
        public readonly Declr func_declr;
        public readonly CompoundStatement func_stmt;

        // Get Function Definition
        // =======================
        // 
        public Tuple<AST.Env, AST.FuncDef> GetFuncDef(AST.Env env) {
            Tuple<AST.Env, AST.Decln.SCS, AST.ExprType> r_specs = func_specs.GetSCSType(env);
            env = r_specs.Item1;
            AST.Decln.SCS scs = r_specs.Item2;
            AST.ExprType base_type = r_specs.Item3;

            Tuple<AST.Env, AST.ExprType, string> r_declr = func_declr.WrapExprType(env, base_type);
            env = r_declr.Item1;
            AST.ExprType type = r_declr.Item2;

            AST.TFunction func_type;
            if (type.kind == AST.ExprType.Kind.FUNCTION) {
                func_type = (AST.TFunction)type;
            } else {
                Log.SemantError("Error: not a function");
                return null;
            }

            string name = r_declr.Item3;

            switch (scs) {
            case AST.Decln.SCS.AUTO:
            case AST.Decln.SCS.EXTERN:
            case AST.Decln.SCS.STATIC:
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

            return new Tuple<AST.Env, AST.FuncDef>(env, new AST.FuncDef(name, scs, func_type, stmt));

        }

        public override Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>> GetExternDecln(AST.Env env) {
            Tuple<AST.Env, AST.FuncDef> r_def = GetFuncDef(env);
            return new Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>>(
                r_def.Item1,
                new List<Tuple<AST.Env, AST.ExternDecln>>() {
                new Tuple<AST.Env, AST.ExternDecln>(r_def.Item1, r_def.Item2)
                }
            );
        }

    }

}