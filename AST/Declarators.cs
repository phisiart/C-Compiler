using System;
using System.Collections.Immutable;
using System.Linq;

namespace AST {
    using static SemanticAnalysis;

    /// <summary>
    /// Modify a Type into a function, array, or pointer
    /// </summary>
    public abstract class TypeModifier : ISyntaxTreeNode {
        [SemantMethod]
        public abstract ISemantReturn<ABT.ExprType> DecorateType(ABT.Env env, ABT.ExprType baseType);
    }

    public sealed class FunctionModifier : TypeModifier {
        private FunctionModifier(Option<ParamTypeList> paramTypeList) {
            this.ParamTypeList = paramTypeList;
        }

        public static FunctionModifier Create(Option<ParamTypeList> paramTypeList) =>
            new FunctionModifier(paramTypeList);

        public Option<ParamTypeList> ParamTypeList { get; }
        
        [SemantMethod]
        public override ISemantReturn<ABT.ExprType> DecorateType(ABT.Env env, ABT.ExprType returnType) {
            if (this.ParamTypeList.IsNone) {
                return SemantReturn.Create(env, ABT.FunctionType.Create(returnType));
            }

            var paramTypeList = this.ParamTypeList.Value;

            var namesAndTypes = Semant(paramTypeList.GetNamesAndTypes, ref env);
            var hasVarArgs = paramTypeList.HasVarArgs;

            return SemantReturn.Create(env, ABT.FunctionType.Create(returnType, namesAndTypes, hasVarArgs));
        }

    }

    /// <summary>
    /// parameter-Type-list
    ///   : parameter-list [ ',' '...' ]?
    /// 
    /// parameter-list
    ///   : parameter-declaration [ ',' parameter-declaration ]*
    /// </summary>
    public sealed class ParamTypeList : ISyntaxTreeNode {
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
        public ISemantReturn<ImmutableList<Tuple<Option<String>, ABT.ExprType>>> GetNamesAndTypes(ABT.Env env) {
            var namesAndTypes = this.ParamDeclns.ConvertAll(
                paramDecln =>
                    Tuple.Create(
                        paramDecln.ParamDeclr.Name,
                        Semant(paramDecln.GetParamType, ref env)
                    )
            );
            return SemantReturn.Create(env, namesAndTypes);
        }
    }

    public sealed class ArrayModifier : TypeModifier {
        private ArrayModifier(Option<Expr> numElements) {
            this.NumElems = numElements;
        }

        public static ArrayModifier Create(Option<Expr> numElements) =>
            new ArrayModifier(numElements);

        [SemantMethod]
        public override ISemantReturn<ABT.ExprType> DecorateType(ABT.Env env, ABT.ExprType elemType) {
            if (this.NumElems.IsNone) {
                return SemantReturn.Create(env, new ABT.IncompleteArrayType(elemType));
            }

            // Get number of elements.
            // Be careful: the environment might change.
            var numElems = SemantExpr(this.NumElems.Value, ref env);

            // Try to cast number of elements to a integer.
            // TODO: allow float???
            numElems = ABT.TypeCast.MakeCast(numElems, new ABT.LongType(true, false));

            if (!numElems.IsConstExpr) {
                throw new InvalidOperationException("Number of elements of an array must be constant.");
            }

            return SemantReturn.Create(env, new ABT.ArrayType(elemType, ((ABT.ConstLong) numElems).Value));
        }

        public Option<Expr> NumElems { get; }
    }

    public sealed class PointerModifier : TypeModifier {
        private PointerModifier(ImmutableList<TypeQual> typeQuals) {
            this.TypeQuals = typeQuals;
        }
        
        public static PointerModifier Create(ImmutableList<TypeQual> typeQuals) =>
            new PointerModifier(typeQuals);

        [SemantMethod]
        public override ISemantReturn<ABT.ExprType> DecorateType(ABT.Env env, ABT.ExprType targetType) {
            var isConst = this.TypeQuals.Contains(TypeQual.CONST);
            var isVolatile = this.TypeQuals.Contains(TypeQual.VOLATILE);
            return SemantReturn.Create(env, new ABT.PointerType(targetType, isConst, isVolatile));
        }

        public ImmutableList<TypeQual> TypeQuals { get; }
    }
    
    /// <summary>
    /// Has a list of modifiers. Has an optional name.
    /// </summary>
    public class ParamDeclr : AbstractDeclr {
        protected ParamDeclr(Option<String> name, ImmutableList<TypeModifier> typeModifiers)
            : base(typeModifiers) {
            this.Name = name;
        }

        public static ParamDeclr Create(Declr declr) =>
            new ParamDeclr(Option.Some(declr.Name), declr.TypeModifiers);

        public static ParamDeclr Create(AbstractDeclr abstractDeclr) =>
            new ParamDeclr(Option<String>.None, abstractDeclr.TypeModifiers);

        public new static ParamDeclr Empty { get; }
            = new ParamDeclr(Option<String>.None, ImmutableList<TypeModifier>.Empty);

        public Option<String> Name { get; }
    }

    /// <summary>
    /// struct-declarator
    ///   : [declarator]? ':' constant-expression
    ///   | declarator
    /// </summary>
    public sealed class StructDeclr : ParamDeclr {
        private StructDeclr(Option<String> name, ImmutableList<TypeModifier> typeModifiers, Option<Expr> numBits)
            : base(name, typeModifiers) {
            this.NumBits = numBits;
        }

        public static StructDeclr Create(Option<Declr> declr, Expr numBits) {
            return declr.IsNone
                ? new StructDeclr(Option<String>.None, ImmutableList<TypeModifier>.Empty, Option.Some(numBits))
                : new StructDeclr(Option.Some(declr.Value.Name), declr.Value.TypeModifiers, Option<Expr>.None);
        }

        public new static StructDeclr Create(Declr declr) =>
            new StructDeclr(Option.Some(declr.Name), declr.TypeModifiers, Option<Expr>.None);

        public Option<Expr> NumBits { get; }
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
    ///       | '(' [parameter-Type-list]? ')'  // function modifier
    ///     ]*
    /// 
    /// An abstract declarator is a list of (pointer, function, or array) Type modifiers
    /// </summary>
    public class AbstractDeclr {
        protected AbstractDeclr(ImmutableList<TypeModifier> typeModifiers) {
            this.TypeModifiers = typeModifiers;
        }
        
        public static AbstractDeclr Create<Modifier>(ImmutableList<Modifier> typeModifiers) where Modifier : TypeModifier =>
            new AbstractDeclr(typeModifiers.ToImmutableList<TypeModifier>());
        
        public static AbstractDeclr Empty { get; } =
            Create(ImmutableList<TypeModifier>.Empty);

        public static AbstractDeclr Add(AbstractDeclr abstractDeclr, TypeModifier typeModifier) =>
            Create(abstractDeclr.TypeModifiers.Add(typeModifier));

        public static AbstractDeclr Add(ImmutableList<PointerModifier> pointerModifiers, AbstractDeclr abstractDeclr) =>
            Create(abstractDeclr.TypeModifiers.AddRange(pointerModifiers));

        [SemantMethod]
        public ISemantReturn<ABT.ExprType> DecorateType(ABT.Env env, ABT.ExprType baseType) {
            var type = this.TypeModifiers
                .Reverse()  // The first Type modifier is nearest to the symbol name, which indicates the outmost Type.
                .Aggregate( // Wrap up the Type based on the Type modifiers.
                    baseType, (currentType, typeModifier) => Semant(typeModifier.DecorateType, currentType, ref env)
                );

            return SemantReturn.Create(env, type);
        }

        public ImmutableList<TypeModifier> TypeModifiers { get; }
    }

    /// <summary>
    /// Has a name and a list of modifiers.
    /// </summary>
    public sealed class Declr : AbstractDeclr {
        private Declr(String name, ImmutableList<TypeModifier> typeModifiers)
            : base(typeModifiers) {
            this.Name = name;
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
    }

    /// <summary>
    /// Init-declarator
    ///   : declarator [ '=' initializer ]?
    /// </summary>
    public sealed class InitDeclr : ISyntaxTreeNode {
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
        public ISemantReturn<Tuple<ABT.ExprType, Option<ABT.Initr>>> GetDecoratedTypeAndInitr(ABT.Env env, ABT.ExprType baseType) {

            // Get the Type based on the declarator. Note that this might be an incomplete array.
            var type = Semant(this.Declr.DecorateType, baseType, ref env);

            Option<ABT.Initr> initrOption;

            if (this.Initr.IsNone) {
                initrOption = Option<ABT.Initr>.None;

            } else {
                // If an initializer is present:

                var initr = Semant(this.Initr.Value.GetInitr, ref env);

                // Check that the initializer conforms to the Type.
                initr = initr.ConformType(type);

                // If the object is an incomplete array, we must determine the length based on the initializer.
                if (type.Kind == ABT.ExprTypeKind.INCOMPLETE_ARRAY) {
                    // Now we need to determine the length.
                    // Find the last element in the Init list.
                    var lastOffset = -1;
                    initr.Iterate(type, (offset, _) => { lastOffset = offset; });

                    if (lastOffset == -1) {
                        throw new InvalidOperationException("Cannot determine the length of the array based on an empty initializer list.");
                    }

                    var elemType = ((ABT.IncompleteArrayType)type).ElemType;

                    var numElems = 1 + lastOffset / ((ABT.IncompleteArrayType)type).ElemType.SizeOf;

                    type = new ABT.ArrayType(elemType, numElems);
                }

                initrOption = Option.Some(initr);
            }
            
            // Now everything is created. Check that the Type is complete.
            if (!type.IsComplete) {
                throw new InvalidOperationException("Cannot create an object with an incomplete Type.");
            }

            return SemantReturn.Create(env, Tuple.Create(type, initrOption));
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
    public abstract class Initr : ISyntaxTreeNode {
        [SemantMethod]
        public abstract ISemantReturn<ABT.Initr> GetInitr(ABT.Env env);
    }

    public sealed class InitExpr : Initr {
        private InitExpr(Expr expr) {
            this.Expr = expr;
        }

        public static InitExpr Create(Expr expr) =>
            new InitExpr(expr);

        public Expr Expr { get; }

        [SemantMethod]
        public override ISemantReturn<ABT.Initr> GetInitr(ABT.Env env) {
            var expr = SemantExpr(this.Expr, ref env);
            return SemantReturn.Create(env, new ABT.InitExpr(expr));
        }
    }

    public sealed class InitList : Initr {
        private InitList(ImmutableList<Initr> initrs) {
            this.Initrs = initrs;
        }

        public static InitList Create(ImmutableList<Initr> initrs) =>
            new InitList(initrs);

        public ImmutableList<Initr> Initrs { get; }

        [SemantMethod]
        public override ISemantReturn<ABT.Initr> GetInitr(ABT.Env env) {
            var initrs = this.Initrs.ConvertAll(
                initr => Semant(initr.GetInitr, ref env)
            );
            return SemantReturn.Create(env, new ABT.InitList(initrs.ToList()));
        }
    }
}