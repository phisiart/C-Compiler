using System;
using System.Collections.Immutable;
using System.Linq;
using AST;
using static SyntaxTree.SemanticAnalysis;

namespace SyntaxTree {

    /// <summary>
    /// declaration
    ///   : declaration-specifiers [Init-declarator-list]? ';'
    /// </summary>
    public sealed class Decln : IExternDecln {
        private Decln(DeclnSpecs declnSpecs, ImmutableList<InitDeclr> initDeclrs) {
            this.DeclnSpecs = declnSpecs;
            this.InitDeclrs = initDeclrs;
        }

        public static Decln Create(DeclnSpecs declnSpecs, ImmutableList<InitDeclr> initDeclrs) =>
            new Decln(declnSpecs, initDeclrs);

        public DeclnSpecs DeclnSpecs { get; }
        public ImmutableList<InitDeclr> InitDeclrs { get; }

        [SemantMethod]
        public ISemantReturn<ImmutableList<Tuple<Env, AST.Decln>>> GetDeclns(Env env) {
            var storageClass = this.DeclnSpecs.GetStorageClass();
            var baseType = Semant(this.DeclnSpecs.GetExprType, ref env);

            var declns = this.InitDeclrs.ConvertAll(
                initDeclr => {
                    var typeAndInitr = Semant(initDeclr.GetDecoratedTypeAndInitr, baseType, ref env);
                    var type = typeAndInitr.Item1;
                    var initr = typeAndInitr.Item2;
                    var name = initDeclr.GetName();

                    // Add the new symbol into the environment.
                    Env.EntryKind kind;
                    switch (storageClass) {
                        case StorageClass.AUTO:
                            if (env.IsGlobal()) {
                                kind = Env.EntryKind.GLOBAL;
                            } else {
                                kind = Env.EntryKind.STACK;
                            }
                            break;
                        case StorageClass.EXTERN:
                            kind = Env.EntryKind.GLOBAL;
                            break;
                        case StorageClass.STATIC:
                            kind = Env.EntryKind.GLOBAL;
                            break;
                        case StorageClass.TYPEDEF:
                            kind = Env.EntryKind.TYPEDEF;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    env = env.PushEntry(kind, name, type);

                    return Tuple.Create(env, new AST.Decln(name, storageClass, type, initr));
                }
            );

            return SemantReturn.Create(env, declns);
        }

        [SemantMethod]
        public ISemantReturn<ImmutableList<Tuple<Env, ExternDecln>>> GetExternDecln(Env env) {
            var declns = Semant(GetDeclns, ref env);
            var externDeclns = declns.ConvertAll(_ => Tuple.Create(_.Item1, _.Item2 as ExternDecln));
            return SemantReturn.Create(env, externDeclns);
        }
    }

    /// <summary>
    /// struct-declaration
    ///   : specifier-qualifier-list struct-declarator-list ';'
    /// 
    /// struct-declarator-list
    ///   : struct-declarator [ ',' struct-declarator ]*
    /// 
    /// struct-declarator
    ///   : declarator
    ///   | [declarator]? ':' constant-expression
    /// </summary>
    public class StructDecln : ISyntaxTreeNode {
        protected StructDecln(SpecQualList specQualList, ImmutableList<StructDeclr> structDeclrs) {
            this.SpecQualList = specQualList;
            this.StructDeclrs = structDeclrs;
        }

        public static StructDecln Create(SpecQualList specQualList, ImmutableList<StructDeclr> structDeclrs) =>
            new StructDecln(specQualList, structDeclrs);

        public SpecQualList SpecQualList { get; }
        public ImmutableList<StructDeclr> StructDeclrs { get; }

        [SemantMethod]
        public ISemantReturn<ImmutableList<Tuple<Option<String>, ExprType>>> GetMemberDeclns(Env env) {
            // Semant specifier-qualifier-list.
            var baseType = Semant(this.SpecQualList.GetExprType, ref env);

            // Decorate types, based on struct declarators.
            var memberTypes =
                this.StructDeclrs
                .ConvertAll(
                    structDeclr =>
                        Semant(structDeclr.DecorateType, baseType, ref env)
                );

            // Get (optional) member names.
            var memberNames =
                this.StructDeclrs
                .ConvertAll(
                    structDeclr => structDeclr.Name
                );

            return SemantReturn.Create(env, memberNames.Zip(memberTypes, Tuple.Create).ToImmutableList());
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
    public class ParamDecln : ISyntaxTreeNode {
        protected ParamDecln(DeclnSpecs declnSpecs, ParamDeclr paramDeclr) {
            this.DeclnSpecs = declnSpecs;
            this.ParamDeclr = paramDeclr;
        }

        public static ParamDecln Create(DeclnSpecs declnSpecs, Option<ParamDeclr> paramDeclr) =>
            new ParamDecln(declnSpecs, paramDeclr.IsSome ? paramDeclr.Value : ParamDeclr.Empty);

        public DeclnSpecs DeclnSpecs { get; }
        public ParamDeclr ParamDeclr { get; }

        [SemantMethod]
        public ISemantReturn<ExprType> GetParamType(Env env) {
            var baseType = Semant(this.DeclnSpecs.GetExprType, ref env);
            var type = Semant(this.ParamDeclr.DecorateType, baseType, ref env);
            return SemantReturn.Create(env, type);
        }
    }

    /// <summary>
    /// Type-name
    ///   : specifier-qualifier-list [abstract-declarator]?
    /// </summary>
    public class TypeName : ISyntaxTreeNode {
        protected TypeName(SpecQualList specQualList, AbstractDeclr abstractDeclr) {
            this.SpecQualList = specQualList;
            this.AbstractDeclr = abstractDeclr;
        }

        public static TypeName Create(SpecQualList specQualList, Option<AbstractDeclr> abstractDeclr) =>
            new TypeName(specQualList, abstractDeclr.IsSome ? abstractDeclr.Value : AbstractDeclr.Empty);

        public SpecQualList SpecQualList { get; }
        public AbstractDeclr AbstractDeclr { get; }

        [SemantMethod]
        public ISemantReturn<ExprType> GetExprType(Env env) {
            var baseType = Semant(this.SpecQualList.GetExprType, ref env);
            var type = Semant(this.AbstractDeclr.DecorateType, baseType, ref env);
            return SemantReturn.Create(env, type);
        }

    }
}