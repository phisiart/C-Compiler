using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

namespace SyntaxTree {
    using static SemanticAnalysis;

    /// <summary>
    /// Modify a type into a function, array, or pointer
    /// </summary>
    public abstract class TypeModifier : SyntaxTreeNode {
        [Obsolete]
        public abstract AST.ExprType GetDecoratedType(AST.Env env, AST.ExprType type);

        public abstract ISemantReturn<AST.ExprType> DecorateType(AST.Env env, AST.ExprType baseType);
    }

    public sealed class FunctionModifier : TypeModifier {
        [Obsolete]
        public FunctionModifier(List<ParamDecln> param_declns, Boolean has_varargs)
            : this(Option.Some(SyntaxTree.ParamTypeList.Create(param_declns.ToImmutableList(), has_varargs))) { }

        private FunctionModifier(Option<ParamTypeList> paramTypeList) {
            this.ParamTypeList = paramTypeList;
        }

        public static FunctionModifier Create(Option<ParamTypeList> paramTypeList) =>
            new FunctionModifier(paramTypeList);

        public Option<ParamTypeList> ParamTypeList { get; }
        
        [Obsolete]
        public override AST.ExprType GetDecoratedType(AST.Env env, AST.ExprType ret_t) {
            var args = ParamTypeList.Value.ParamDeclns.ConvertAll(decln => decln.GetParamDecln(env));
            return AST.TFunction.Create(ret_t, args.ToList(), ParamTypeList.Value.HasVarArgs);
        }

        [SemantMethod]
        public override ISemantReturn<AST.ExprType> DecorateType(AST.Env env, AST.ExprType returnType) {
            if (this.ParamTypeList.IsNone) {
                return SemantReturn.Create(env, AST.TFunction.Create(returnType));
            }

            var paramTypeList = this.ParamTypeList.Value;

            var namesAndTypes = Semant(paramTypeList.GetNamesAndTypes, ref env);
            var hasVarArgs = paramTypeList.HasVarArgs;

            return SemantReturn.Create(env, AST.TFunction.Create(returnType, namesAndTypes, hasVarArgs));
        }

    }

    /// <summary>
    /// parameter-type-list
    ///   : parameter-list [ ',' '...' ]?
    /// 
    /// parameter-list
    ///   : parameter-declaration [ ',' parameter-declaration ]*
    /// </summary>
    public sealed class ParamTypeList : SyntaxTreeNode {
        [Obsolete]
        public ParamTypeList(IEnumerable<ParamDecln> paramDeclns, Boolean hasVarArgs)
            : this(paramDeclns.ToImmutableList(), hasVarArgs) { }

        private ParamTypeList(ImmutableList<ParamDecln> paramDeclns, Boolean hasVarArgs) {
            this.ParamDeclns = paramDeclns;
            this.HasVarArgs = hasVarArgs;
        }

        public static ParamTypeList Create(ImmutableList<ParamDecln> paramDeclns, Boolean hasVarArgs) =>
            new ParamTypeList(paramDeclns, hasVarArgs);

        public static ParamTypeList Create() =>
            Create(ImmutableList<ParamDecln>.Empty, true);

        public ImmutableList<ParamDecln> ParamDeclns { get; }
        public Boolean HasVarArgs { get; }

        [SemantMethod]
        public ISemantReturn<ImmutableList<Tuple<Option<String>, AST.ExprType>>> GetNamesAndTypes(AST.Env env) {
            var namesAndTypes = this.ParamDeclns.ConvertAll(
                paramDecln =>
                    Tuple.Create(
                        paramDecln.GetParamName(),
                        Semant(paramDecln.GetParamType, ref env)
                    )
            );
            return SemantReturn.Create(env, namesAndTypes);
        }

        [Obsolete]
        public Tuple<Boolean, List<Tuple<AST.Env, String, AST.ExprType>>> GetParamTypesEnv(AST.Env env) {
            return Tuple.Create(
                HasVarArgs,
                ParamDeclns.Select(decln => {
                    Tuple<String, AST.ExprType> r_decln = decln.GetParamDecln(env);
                    // Tuple<AST.Env, String, AST.ExprType> r_decln = decln.GetParamDeclnEnv(env);
                    // env = r_decln.Item1;
                    return Tuple.Create(env, r_decln.Item1, r_decln.Item2);
                }).ToList()
            );
        }

    }

    public sealed class ArrayModifier : TypeModifier {
        private ArrayModifier(Option<Expr> numElements) {
            this.NumElems = numElements;
        }

        public static ArrayModifier Create(Option<Expr> numElements) =>
            new ArrayModifier(numElements);

        [Obsolete]
        public override AST.ExprType GetDecoratedType(AST.Env env, AST.ExprType type) {

            if (NumElems.IsNone) {
                return new AST.TIncompleteArray(type);
            }

            AST.Expr num_elems = AST.TypeCast.MakeCast(NumElems.Value.GetExpr(env), new AST.TLong(true, true));

            if (!num_elems.IsConstExpr) {
                throw new InvalidOperationException("Expected constant length.");
            }

            return new AST.TArray(type, ((AST.ConstLong)num_elems).value);
        }

        public override ISemantReturn<AST.ExprType> DecorateType(AST.Env env, AST.ExprType elemType) {
            if (this.NumElems.IsNone) {
                return SemantReturn.Create(env, new AST.TIncompleteArray(elemType));
            }

            // Get number of elements.
            // Be careful: the environment might change.
            var numElems = SemantExpr(this.NumElems.Value, ref env);

            // Try to cast number of elements to a integer.
            // TODO: allow float???
            numElems = AST.TypeCast.MakeCast(numElems, new AST.TLong(is_const: true, is_volatile: false));

            if (!numElems.IsConstExpr) {
                throw new InvalidOperationException("Number of elements of an array must be constant.");
            }

            return SemantReturn.Create(env, new AST.TArray(elemType, (numElems as AST.ConstLong).value));
        }

        public Option<Expr> NumElems { get; }
    }

    public sealed class PointerModifier : TypeModifier {
        private PointerModifier(ImmutableList<TypeQual> typeQuals) {
            this.TypeQuals = typeQuals;
        }

        [Obsolete]
        public PointerModifier(IReadOnlyList<TypeQual> type_quals)
            : this(type_quals.ToImmutableList()) { }

        public static PointerModifier Create(ImmutableList<TypeQual> typeQuals) =>
            new PointerModifier(typeQuals);

        [Obsolete]
        public override AST.ExprType GetDecoratedType(AST.Env env, AST.ExprType type) {
            Boolean is_const = TypeQuals.Contains(TypeQual.CONST);
            Boolean is_volatile = TypeQuals.Contains(TypeQual.VOLATILE);

            // This is commented out, for incomplete struct declaration.
            //if (!type.IsComplete) {
            //    throw new InvalidOperationException("The type a pointer points to must be complete.");
            //}

            return new AST.TPointer(type, is_const, is_volatile);
        }

        public override ISemantReturn<AST.ExprType> DecorateType(AST.Env env, AST.ExprType targetType) {
            var isConst = this.TypeQuals.Contains(TypeQual.CONST);
            var isVolatile = this.TypeQuals.Contains(TypeQual.VOLATILE);
            return SemantReturn.Create(env, new AST.TPointer(targetType, isConst, isVolatile));
        }

        public ImmutableList<TypeQual> TypeQuals { get; }
    }

    /// <summary>
    /// There are a bunch of declarators in C.
    /// They are all derived from <see cref="BaseDeclr"/>.
    /// </summary>
    public abstract class BaseDeclr : SyntaxTreeNode, IBaseDeclr {
        protected BaseDeclr(ImmutableList<TypeModifier> typeModifiers) {
            this.TypeModifiers = typeModifiers;
        }

        public ISemantReturn<AST.ExprType> DecorateType(AST.Env env, AST.ExprType baseType) {
            var type = this.TypeModifiers
                .Reverse()  // The first type modifier is nearest to the symbol name, which indicates the outmost type.
                .Aggregate( // Wrap up the type based on the type modifiers.
                    seed: baseType,
                    func: (currentType, typeModifier) => Semant(typeModifier.DecorateType, currentType, ref env)
                );

            return SemantReturn.Create(env, type);
        }

        public ImmutableList<TypeModifier> TypeModifiers { get; }
    }

    public interface IBaseDeclr {
        ImmutableList<TypeModifier> TypeModifiers { get; }
        ISemantReturn<AST.ExprType> DecorateType(AST.Env env, AST.ExprType baseType);
    }

    /// <summary>
    /// Has a list of modifiers. Has an optional name.
    /// </summary>
    public interface IParamDeclr : IBaseDeclr {
        Option<String> OptionalName { get; }
    }

    /// <summary>
    /// struct-declarator
    ///   : declarator
    ///   | [declarator]? ':' constant-expression
    /// </summary>
    public interface IStructDeclr : IBaseDeclr {
        Boolean IsBitField { get; }
        Declr GetDeclr { get; }
        BitFieldDeclr GetBitFieldDeclr { get; }
        Option<String> OptionalName { get; }
    }

    public sealed class BitFieldDeclr : BaseDeclr, IStructDeclr {
        private BitFieldDeclr(Option<String> name, ImmutableList<TypeModifier> typeModifiers, Expr numBits)
            : base(typeModifiers) {
            this.Name = name;
            this.NumBits = numBits;
        }

        public static BitFieldDeclr Create(Option<Declr> declr, Expr numBits) {
            if (declr.IsSome) {
                return new BitFieldDeclr(Option.Some(declr.Value.Name), declr.Value.TypeModifiers, numBits);
            } else {
                return new BitFieldDeclr(Option<String>.None, ImmutableList<TypeModifier>.Empty, numBits);
            }
        }

        public BitFieldDeclr GetBitFieldDeclr => this;

        public Declr GetDeclr {
            get {
                throw new InvalidOperationException($"This is a {nameof(BitFieldDeclr)}, not a {nameof(Declr)}");
            }
        }

        public Option<String> Name { get; }

        public Boolean IsBitField => true;

        public Expr NumBits { get; }

        public Option<String> OptionalName => this.Name;
    }

    /// <summary>
    /// abstract-declarator
    ///   : [pointer]? direct-abstract-declarator
    ///   | pointer
    /// 
    /// direct-abstract-declarator
    ///   : [
    ///         '(' abstract-declarator ')'
    ///       | '[' [constant-expression]? ']'  // array modifier
    ///       | '(' [parameter-type_list]? ')'  // function modifier
    ///     ] [
    ///         '[' [constant-expression]? ']'  // array modifier
    ///       | '(' [parameter-type-list]? ')'  // function modifier
    ///     ]*
    /// 
    /// An abstract declarator is a list of (pointer, function, or array) type modifiers
    /// </summary>
    public sealed class AbstractDeclr : BaseDeclr, IParamDeclr {
        private AbstractDeclr(ImmutableList<TypeModifier> typeModifiers)
            : base(typeModifiers) { }
        
        public static AbstractDeclr Create<Modifier>(ImmutableList<Modifier> typeModifiers) where Modifier : TypeModifier =>
            new AbstractDeclr(typeModifiers.ToImmutableList<TypeModifier>());
        
        public static AbstractDeclr Empty { get; } =
            Create(ImmutableList<TypeModifier>.Empty);

        public static AbstractDeclr Add(AbstractDeclr abstractDeclr, TypeModifier typeModifier) =>
            Create(abstractDeclr.TypeModifiers.Add(typeModifier));

        public static AbstractDeclr Add(ImmutableList<PointerModifier> pointerModifiers, AbstractDeclr abstractDeclr) =>
            Create(abstractDeclr.TypeModifiers.AddRange(pointerModifiers));

        public Option<String> OptionalName => Option<String>.None;
    }

    /// <summary>
    /// Has a name and a list of modifiers.
    /// </summary>
    public sealed class Declr : BaseDeclr, IParamDeclr, IStructDeclr {

        [Obsolete]
        public Declr(String name, IReadOnlyList<TypeModifier> modifiers)
            : this(name, modifiers.ToImmutableList()) { }

        private Declr(String name, ImmutableList<TypeModifier> typeModifiers)
            : base(typeModifiers) {
            this.Name = name;
            this.OptionalName = Option.Some(name);
        }

        public static Declr Create(String name, ImmutableList<TypeModifier> typeModifiers) =>
            new Declr(name, typeModifiers);

        public static Declr Create(String name) =>
            new Declr(name, ImmutableList<TypeModifier>.Empty);

        public static Declr Create(Option<ImmutableList<PointerModifier>> pointerModifiers, Declr declr) =>
            Add(pointerModifiers.IsSome ? pointerModifiers.Value : ImmutableList<PointerModifier>.Empty, declr);
        
        public static Declr Add(Declr declr, TypeModifier typeModifier) =>
            Create(declr.Name, declr.TypeModifiers.Add(typeModifier));

        public static Declr Add(ImmutableList<PointerModifier> pointerModifiers, Declr declr) =>
            Create(declr.Name, declr.TypeModifiers.AddRange(pointerModifiers));

        public String Name { get; }

        public Option<String> OptionalName { get; }

        public Boolean IsBitField => false;

        public Declr GetDeclr => this;

        public BitFieldDeclr GetBitFieldDeclr {
            get {
                throw new InvalidOperationException($"This is a {nameof(Declr)}, not a {nameof(BitFieldDeclr)}");
            }
        }

        /// <summary>
        /// A declarator consists of 1) a name, and 2) a list of decorators.
        /// This method returns the name, and the modified type.
        /// </summary>
        [Obsolete]
        public Tuple<String, AST.ExprType> GetNameAndType(AST.Env env, AST.ExprType base_type) =>
            Tuple.Create(
                Name,
                TypeModifiers
                    .Reverse()
                    .Aggregate(base_type, (type, modifier) => modifier.GetDecoratedType(env, type))
            );

    }

    /// <summary>
    /// init-declarator
    ///   : declarator [ '=' initializer ]?
    /// </summary>
    public sealed class InitDeclr : SyntaxTreeNode {
        private InitDeclr(Declr declr, Option<Initr> initr) {
            this.Declr = declr;
            this.Initr = initr;
        }

        public Declr Declr { get; }
        public Option<Initr> Initr { get; }

        public static InitDeclr Create(Declr declr, Option<Initr> initr)
            => new InitDeclr(declr, initr);

        [SemantMethod]
        public String GetName() => this.Declr.Name;

        [SemantMethod]
        public ISemantReturn<Tuple<AST.ExprType, Option<AST.Initr>>> GetDecoratedTypeAndInitr(AST.Env env, AST.ExprType baseType) {
            var initrOpt = this.Initr.Map(_ => Semant(_.GetInitr, ref env));
            var type = Semant(this.Declr.DecorateType, baseType, ref env);

            // Check that the initializer conforms to the type.
            initrOpt = initrOpt.Map(_ => _.ConformType(type));

            // If the object is an incomplete array, we must determine the length based on the initializer.
            if (type.kind == AST.ExprType.Kind.INCOMPLETE_ARRAY) {
                if (initrOpt.IsNone) {
                    throw new InvalidOperationException("Cannot determine the length of the array without an initializer.");
                }

                // Now we need to determine the length.
                // Find the last element in the init list.
                Int32 lastOffset = -1;
                initrOpt.Value.Iterate(type, (offset, _) => { lastOffset = offset; });

                if (lastOffset == -1) {
                    throw new InvalidOperationException("Cannot determine the length of the array based on an empty initializer list.");
                }

                AST.ExprType elemType = ((AST.TIncompleteArray)type).elem_type;

                Int32 numElems = 1 + lastOffset / ((AST.TIncompleteArray)type).elem_type.SizeOf;

                type = new AST.TArray(elemType, numElems, type.is_const, type.is_volatile);
            }

            return SemantReturn.Create(env, Tuple.Create(type, initrOpt));
        }

        [Obsolete]
        public Tuple<AST.Env, AST.ExprType, Option<AST.Initr>, String> GetInitDeclr(AST.Env env, AST.ExprType type) {
            String name;
            Option<AST.Initr> initr_opt;

            // Get the initializer list.
            Option<Tuple<AST.Env, AST.Initr>> r_initr = this.Initr.Map(_ => _.GetInitr_(env));
            if (r_initr.IsSome) {
                env = r_initr.Value.Item1;
                initr_opt = new Some<AST.Initr>(r_initr.Value.Item2);
            } else {
                initr_opt = new None<AST.Initr>();
            }

            // Get the declarator.
            Tuple<String, AST.ExprType> r_declr = Declr.GetNameAndType(env, type);
            name = r_declr.Item1;
            type = r_declr.Item2;

            // Implicit cast the initializer.
            initr_opt = initr_opt.Map(_ => _.ConformType(type));

            // If the object is an incomplete list, we must determine the length based on the initializer.
            if (type.kind == AST.ExprType.Kind.INCOMPLETE_ARRAY) {
                if (initr_opt.IsNone) {
                    throw new InvalidOperationException("Cannot determine the length of the array.");
                }

                // Now we need to determine the length.
                // Find the last element in the init list.
                Int32 last_offset = -1;
                initr_opt.Value.Iterate(type, (offset, _) => { last_offset = offset; });

                if (last_offset == -1) {
                    throw new InvalidOperationException("Cannot determine the length of the array based on an empty initializer list.");
                }

                AST.ExprType elem_type = ((AST.TIncompleteArray)type).elem_type;

                Int32 num_elems = 1 + last_offset / ((AST.TIncompleteArray)type).elem_type.SizeOf;

                type = new AST.TArray(elem_type, num_elems, type.is_const, type.is_volatile);
            }

            return new Tuple<AST.Env, AST.ExprType, Option<AST.Initr>, String>(env, type, initr_opt, name);
        }

    }

    /// <summary>
    /// initializer
    ///   : assignment-expression
    ///   | '{' initializer-list '}'
    ///   | '{' initializer-list ',' '}'
    /// 
    /// initializer-list
    ///   : initializer [ ',' initializer ]*
    /// </summary>
    public abstract class Initr : SyntaxTreeNode {
        public enum Kind {
            EXPR,
            INIT_LIST,
        }

        [Obsolete]
        public abstract Tuple<AST.Env, AST.Initr> GetInitr_(AST.Env env);

        [SemantMethod]
        public abstract ISemantReturn<AST.Initr> GetInitr(AST.Env env);

        public abstract Kind kind { get; }
    }

    public sealed class InitExpr : Initr {
        private InitExpr(Expr expr) {
            this.Expr = expr;
        }

        public static InitExpr Create(Expr expr) =>
            new InitExpr(expr);

        public override Kind kind => Kind.EXPR;
        public Expr Expr { get; }

        [SemantMethod]
        public override ISemantReturn<AST.Initr> GetInitr(AST.Env env) {
            var expr = SemantExpr(this.Expr, ref env);
            return SemantReturn.Create(env, new AST.InitExpr(expr));
        }

        [Obsolete]
        public override Tuple<AST.Env, AST.Initr> GetInitr_(AST.Env env) {
            // TODO: Expr should change env
            return new Tuple<AST.Env, AST.Initr>(env, new AST.InitExpr(Expr.GetExpr(env)));
        }
    }

    public sealed class InitList : Initr {
        private InitList(ImmutableList<Initr> initrs) {
            this.Initrs = initrs;
        }

        [Obsolete]
        public InitList(List<Initr> initrs)
            : this(initrs.ToImmutableList()) { }

        public static InitList Create(ImmutableList<Initr> initrs) =>
            new InitList(initrs);

        public override Kind kind => Kind.INIT_LIST;
        public ImmutableList<Initr> Initrs { get; }

        [SemantMethod]
        public override ISemantReturn<AST.Initr> GetInitr(AST.Env env) {
            var initrs = this.Initrs.ConvertAll(
                initr => Semant(initr.GetInitr, ref env)
            );
            return SemantReturn.Create(env, new AST.InitList(initrs.ToList()));
        }

        [Obsolete]
        public override Tuple<AST.Env, AST.Initr> GetInitr_(AST.Env env) {
            ImmutableList<AST.Initr> initrs = this.Initrs.ConvertAll(initr => {
                Tuple<AST.Env, AST.Initr> r_initr = initr.GetInitr_(env);
                env = r_initr.Item1;
                return r_initr.Item2;
            });
            return new Tuple<AST.Env, AST.Initr>(env, new AST.InitList(initrs.ToList()));
        }
    }
}