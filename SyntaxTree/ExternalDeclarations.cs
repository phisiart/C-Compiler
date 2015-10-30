using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SyntaxTree {

    /// <summary>
    /// A translation unit consists of a list of external declarations - functions and objects.
    /// </summary>
    public class TranslnUnit : PTNode {

        [Obsolete]
        public TranslnUnit(IEnumerable<ExternDecln> declns)
            : this(declns.ToImmutableList()) { }

        protected TranslnUnit(ImmutableList<ExternDecln> declns) {
            this.Declns = declns;
        }

        public static TranslnUnit Create(ImmutableList<ExternDecln> externDeclns) =>
            new TranslnUnit(externDeclns);

        public Tuple<AST.Env, AST.TranslnUnit> GetTranslationUnit() {
            List<Tuple<AST.Env, AST.ExternDecln>> declns = new List<Tuple<AST.Env, AST.ExternDecln>>();
            AST.Env env = new AST.Env();

            foreach (ExternDecln decln in this.Declns) {
                Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>> r_decln = decln.GetExternDecln(env);
                env = r_decln.Item1;
                declns.AddRange(r_decln.Item2);
            }

            return new Tuple<AST.Env, AST.TranslnUnit>(env, new AST.TranslnUnit(declns));
        }

        public ImmutableList<ExternDecln> Declns { get; }
    }


    public abstract class ExternDecln : PTNode {
        public abstract Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>> GetExternDecln(AST.Env env);
    }

    /// <summary>
    /// A function definition gives the implementation.
    /// </summary>
    public class FuncDef : ExternDecln {
        public FuncDef(DeclnSpecs specs, Declr declr, CompoundStmt stmt) {
            this.Specs = specs;
            this.Declr = declr;
            this.Stmt = stmt;
        }

        public static FuncDef Create(DeclnSpecs declnSpecs, Declr declr, Stmt body) =>
            new FuncDef(declnSpecs, declr, body as CompoundStmt);

        public DeclnSpecs Specs { get; }
        public Declr Declr { get; }
        public CompoundStmt Stmt { get; }

        public Tuple<AST.Env, AST.FuncDef> GetFuncDef(AST.Env env) {

            // Get storage class specifier and base type from declaration specifiers.
            Tuple<AST.Env, AST.Decln.StorageClass, AST.ExprType> r_specs = this.Specs.GetSCSType(env);
            env = r_specs.Item1;
            AST.Decln.StorageClass scs = r_specs.Item2;
            AST.ExprType base_type = r_specs.Item3;

            // Get function name and function type from declarator.
            Tuple<String, AST.ExprType> r_declr = this.Declr.GetNameAndType(env, base_type);
            String name = r_declr.Item1;
            AST.ExprType type = r_declr.Item2;

            AST.TFunction func_type;
            if (type.kind == AST.ExprType.Kind.FUNCTION) {
                func_type = (AST.TFunction)type;
            } else {
                throw new InvalidOperationException($"{name} is not a function.");
            }

            switch (scs) {
                case AST.Decln.StorageClass.AUTO:
                case AST.Decln.StorageClass.EXTERN:
                case AST.Decln.StorageClass.STATIC:
                    env = env.PushEntry(AST.Env.EntryKind.GLOBAL, name, type);
                    break;
                case AST.Decln.StorageClass.TYPEDEF:
                default:
                    throw new InvalidOperationException("Invalid storage class specifier for function definition.");
            }

            env = env.SetCurrentFunction(func_type);

            Tuple<AST.Env, AST.Stmt> r_stmt = this.Stmt.GetStmt(env);
            env = r_stmt.Item1;
            AST.Stmt stmt = r_stmt.Item2;

            env = env.SetCurrentFunction(new AST.TEmptyFunction());

            return Tuple.Create(env, new AST.FuncDef(name, scs, func_type, stmt));
        }

        public override Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>> GetExternDecln(AST.Env env) {
            Tuple<AST.Env, AST.FuncDef> r_def = GetFuncDef(env);
            return new Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>>(
                r_def.Item1,
                new List<Tuple<AST.Env, AST.ExternDecln>>() { Tuple.Create(r_def.Item1, (AST.ExternDecln)r_def.Item2) }
            );
        }

    }

}