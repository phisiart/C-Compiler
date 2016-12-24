using System;
using System.Linq;
using System.Collections.Immutable;

namespace AST {
    using static SemanticAnalysis;

    public abstract partial class Expr {
        public abstract ABT.Expr GetExpr(ABT.Env env);
    }

    public sealed partial class Variable {
        public override ABT.Expr GetExpr(ABT.Env env) {
            Option<ABT.Env.Entry> entryOpt = env.Find(this.Name);

            if (entryOpt.IsNone) {
                throw new InvalidOperationException(
                    $"Cannot find variable '{this.Name}'"
                );
            }

            ABT.Env.Entry entry = entryOpt.Value;

            switch (entry.Kind) {
                case ABT.Env.EntryKind.TYPEDEF:
                    throw new InvalidOperationException(
                        $"Expected a variable '{this.Name}', not a typedef."
                    );

                case ABT.Env.EntryKind.ENUM:
                    return new ABT.ConstLong(entry.Offset, env);
                    
                case ABT.Env.EntryKind.FRAME:
                case ABT.Env.EntryKind.GLOBAL:
                case ABT.Env.EntryKind.STACK:
                    return new ABT.Variable(entry.Type, this.Name, env);
                    
                default:
                    throw new InvalidOperationException(
                        $"Cannot find variable '{this.Name}'"
                    );
            }
        }
    }

    public sealed partial class AssignList {
        public override ABT.Expr GetExpr(ABT.Env env) {
            var exprs = this.Exprs.ConvertAll(
                // TODO: is this 'ref' okay?
                expr => SemantExpr(expr, ref env)
            );
            return new ABT.AssignList(exprs);
        }
    }

    public sealed partial class ConditionalExpr {
        // TODO : What if const???
        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.Expr cond = SemantExpr(this.Cond, ref env);

            if (!cond.Type.IsScalar) {
                throw new InvalidOperationException(
                    "Expected a scalar condition in conditional expression."
                );
            }

            if (cond.Type.IsIntegral) {
                cond = ABT.TypeCast.IntegralPromotion(cond).Item1;
            }

            ABT.Expr trueExpr = SemantExpr(this.TrueExpr, ref env);
            ABT.Expr falseExpr = SemantExpr(this.FalseExpr, ref env);

            // 1. if both trueExpr and falseExpr have arithmetic types:
            //    perform usual arithmetic conversion
            if (trueExpr.Type.IsArith && falseExpr.Type.IsArith) {
                var r_cast = ABT.TypeCast.UsualArithmeticConversion(
                    trueExpr,
                    falseExpr
                );

                trueExpr = r_cast.Item1;
                falseExpr = r_cast.Item2;

                return new ABT.ConditionalExpr(
                    cond,
                    trueExpr,
                    falseExpr,
                    trueExpr.Type
                );
            }

            if (trueExpr.Type.Kind != falseExpr.Type.Kind) {
                throw new InvalidOperationException(
                    "Operand types not match in conditional expression."
                );
            }

            switch (trueExpr.Type.Kind) {
                // 2. if both trueExpr and falseExpr have struct or union Type
                //    make sure they are compatible
                case ABT.ExprTypeKind.STRUCT_OR_UNION:
                    if (!trueExpr.Type.EqualType(falseExpr.Type)) {
                        throw new InvalidOperationException(
                            "Expected compatible types."
                        );
                    }
                    return new ABT.ConditionalExpr(
                        cond,
                        trueExpr,
                        falseExpr,
                        trueExpr.Type
                    );

                // 3. if both true_expr and false_expr have void Type
                //    return void
                case ABT.ExprTypeKind.VOID:
                    return new ABT.ConditionalExpr(
                        cond,
                        trueExpr,
                        falseExpr,
                        trueExpr.Type
                    );

                // 4. if both true_expr and false_expr have pointer Type
                case ABT.ExprTypeKind.POINTER:
                    var trueExprType = (ABT.PointerType)trueExpr.Type;
                    var falseExprType = (ABT.PointerType)falseExpr.Type;
                    // if either points to void, convert to void *
                    if (trueExprType.RefType is ABT.VoidType ||
                        falseExprType.RefType is ABT.VoidType) {
                        return new ABT.ConditionalExpr(
                            cond,
                            trueExpr,
                            falseExpr,
                            new ABT.PointerType(new ABT.VoidType())
                        );
                    }

                    // TODO: more comparisons.
                    throw new NotImplementedException("More comparisons here.");

                default:
                    throw new InvalidOperationException(
                        "Expected compatible types in conditional expression."
                    );
            }
        }
    }

    public sealed partial class FuncCall {
        public override ABT.Expr GetExpr(ABT.Env env) {

            // Step 1: get arguments passed into the function.
            // Currently the arguments are not casted based on the prototype.
            var args = this.Args.ConvertAll(arg => SemantExpr(arg, ref env));

            // A special case:
            // If we cannot find the prototype in the environment, make one up.
            // This function returns int.
            // Update the environment to add this function Type.
            if ((this.Func is Variable) &&
                env.Find(((Variable)this.Func).Name).IsNone) {

                // TODO: get this env used.
                env = env.PushEntry(
                    ABT.Env.EntryKind.TYPEDEF,
                    (this.Func as Variable).Name,
                    ABT.FunctionType.Create(
                        new ABT.LongType(true),
                        // TODO: function argument automatic promotion.
                        args.ConvertAll(_ => Tuple.Create("", _.Type)).ToList(),
                        false
                    )
                );
            }

            // Step 2: get function expression.
            ABT.Expr func = this.Func.GetExpr(env);

            // Step 3: get the function Type.
            ABT.FunctionType func_type;
            switch (func.Type.Kind) {
                case ABT.ExprTypeKind.FUNCTION:
                    func_type = func.Type as ABT.FunctionType;
                    break;

                case ABT.ExprTypeKind.POINTER:
                    var refType = (func.Type as ABT.PointerType).RefType;
                    if (!(refType is ABT.FunctionType)) {
                        throw new InvalidOperationException(
                            "Expected a function pointer."
                        );
                    }
                    func_type = refType as ABT.FunctionType;
                    break;

                default:
                    throw new InvalidOperationException(
                        "Expected a function in function call."
                    );
            }


            Int32 numArgsPrototype = func_type.Args.Count;
            Int32 numArgsActual = args.Count;

            // If this function doesn't take varargs, make sure the number of 
            //  arguments match that in the prototype.
            if (!func_type.HasVarArgs && numArgsActual != numArgsPrototype) {
                throw new InvalidOperationException(
                    "Number of arguments mismatch."
                );
            }

            // You can't call a function with fewer args than the prototype.
            if (numArgsActual < numArgsPrototype) {
                throw new InvalidOperationException("Too few arguments.");
            }

            // Make implicit cast.
            args = args
                .GetRange(0, numArgsPrototype)
                .Zip(
                    func_type.Args,
                    (arg, entry) => ABT.TypeCast.MakeCast(arg, entry.type)
                )
                .Concat(
                    args.GetRange(
                        numArgsPrototype,
                        numArgsActual - numArgsPrototype
                    )
                )
                .ToImmutableList();

            return new ABT.FuncCall(func, func_type, args.ToList());
        }
    }

    public sealed partial class Attribute {
        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.Expr expr = SemantExpr(this.Expr, ref env);
            String name = this.Member;

            if (expr.Type.Kind != ABT.ExprTypeKind.STRUCT_OR_UNION) {
                throw new InvalidOperationException(
                    "Must get the attribute from a struct or union."
                );
            }

            ABT.Utils.StoreEntry entry = (expr.Type as ABT.StructOrUnionType)
                .Attribs.First(_ => _.name == name);
            
            ABT.ExprType type = entry.type;

            return new ABT.Attribute(expr, name, type);
        }
    }
}
