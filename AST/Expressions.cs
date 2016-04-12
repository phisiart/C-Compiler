using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AST {

    // 3.2.1.5
    /* First, if either operand has Type long double, the other operand is converted to long double.
     * Otherwise, if either operand has Type double, the other operand is converted to double.
     * Otherwise, if either operand has Type float, the other operand is converted to float.
     * Otherwise, the integral promotions are performed on both operands.
     * Then the following rules are applied:
     * If either operand has Type unsigned long Int32, the other operand is converted to unsigned long Int32.
     * Otherwise, if one operand has Type long Int32 and the other has Type unsigned Int32, if a long Int32 can represent all values of an unsigned Int32, the operand of Type unsigned Int32 is converted to long Int32;
     * if a long Int32 cannot represent all the values of an unsigned Int32, both operands are converted to unsigned long Int32. Otherwise, if either operand has Type long Int32, the other operand is converted to long Int32.
     * Otherwise, if either operand has Type unsigned Int32, the other operand is converted to unsigned Int32.
     * Otherwise, both operands have Type Int32.*/

    // My simplification:
    // I let long = int, long double = double

    public abstract class Expr : ISyntaxTreeNode {
        public abstract ABT.Expr GetExpr(ABT.Env env);
    }

    /// <summary>
    /// Only a name
    /// </summary>
    public class Variable : Expr {
        public Variable(String name) {
            this.Name = name;
        }

        public static Expr Create(String name) =>
            new Variable(name);

        public String Name { get; }

        public override ABT.Expr GetExpr(ABT.Env env) {
            Option<ABT.Env.Entry> entry_opt = env.Find(this.Name);

            if (entry_opt.IsNone) {
                throw new InvalidOperationException($"Cannot find variable '{this.Name}'");
            }

            ABT.Env.Entry entry = entry_opt.Value;

            switch (entry.Kind) {
                case ABT.Env.EntryKind.TYPEDEF:
                    throw new InvalidOperationException($"Expected a variable '{this.Name}', not a typedef.");
                case ABT.Env.EntryKind.ENUM:
                    return new ABT.ConstLong(entry.Offset, env);
                case ABT.Env.EntryKind.FRAME:
                case ABT.Env.EntryKind.GLOBAL:
                case ABT.Env.EntryKind.STACK:
                    return new ABT.Variable(entry.Type, this.Name, env);
                default:
                    throw new InvalidOperationException($"Cannot find variable '{this.Name}'");
            }
        }
    }

    /// <summary>
    /// A list of assignment expressions.
    /// e.g.
    ///   a = 3, b = 4;
    /// </summary>
	public class AssignmentList : Expr {
        protected AssignmentList(ImmutableList<Expr> exprs) {
            this.Exprs = exprs;
        }

        public ImmutableList<Expr> Exprs { get; }

        public static Expr Create(ImmutableList<Expr> exprs) =>
            new AssignmentList(exprs);

        public override ABT.Expr GetExpr(ABT.Env env) {
            ImmutableList<ABT.Expr> exprs = this.Exprs.ConvertAll(expr => expr.GetExpr(env));
            return new ABT.AssignList(exprs);
        }
    }

    /// <summary>
    /// Conditional Expression
    /// 
    /// Cond ? true_expr : false_expr
    /// 
    /// Cond must be of scalar Type
    /// 
    /// 1. if both true_expr and false_expr have arithmetic types
    ///    perform usual arithmetic conversion
    /// 2. 
    /// </summary>
    // TODO : What if const???
    public class ConditionalExpression : Expr {
        public ConditionalExpression(Expr cond, Expr trueExpr, Expr falseExpr) {
            this.Cond = cond;
            this.TrueExpr = trueExpr;
            this.FalseExpr = falseExpr;
        }

        public Expr Cond { get; }
        public Expr TrueExpr { get; }
        public Expr FalseExpr { get; }

        public static Expr Create(Expr cond, Expr trueExpr, Expr falseExpr) =>
            new ConditionalExpression(cond, trueExpr, falseExpr);

        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.Expr cond = this.Cond.GetExpr(env);

            if (!cond.Type.IsScalar) {
                throw new InvalidOperationException("Expected a scalar condition in conditional expression.");
            }

            if (cond.Type.IsIntegral) {
                cond = ABT.TypeCast.IntegralPromotion(cond).Item1;
            }

            ABT.Expr true_expr = this.TrueExpr.GetExpr(env);
            ABT.Expr false_expr = this.FalseExpr.GetExpr(env);

            // 1. if both true_expr and false_Expr have arithmetic types:
            //    perform usual arithmetic conversion
            if (true_expr.Type.IsArith && false_expr.Type.IsArith) {
                var r_cast = ABT.TypeCast.UsualArithmeticConversion(true_expr, false_expr);
                true_expr = r_cast.Item1;
                false_expr = r_cast.Item2;
                return new ABT.ConditionalExpr(cond, true_expr, false_expr, true_expr.Type);
            }

            if (true_expr.Type.Kind != false_expr.Type.Kind) {
                throw new InvalidOperationException("Operand types not match in conditional expression.");
            }

            switch (true_expr.Type.Kind) {
                // 2. if both true_expr and false_expr have struct or union Type
                //    make sure they are compatible
                case ABT.ExprTypeKind.STRUCT_OR_UNION:
                    if (!true_expr.Type.EqualType(false_expr.Type)) {
                        throw new InvalidOperationException("Expected compatible types in conditional expression.");
                    }
                    return new ABT.ConditionalExpr(cond, true_expr, false_expr, true_expr.Type);

                // 3. if both true_expr and false_expr have void Type
                //    return void
                case ABT.ExprTypeKind.VOID:
                    return new ABT.ConditionalExpr(cond, true_expr, false_expr, true_expr.Type);

                // 4. if both true_expr and false_expr have pointer Type
                case ABT.ExprTypeKind.POINTER:

                    // if either points to void, convert to void *
                    if (((ABT.PointerType)true_expr.Type).RefType.Kind == ABT.ExprTypeKind.VOID
                        || ((ABT.PointerType)false_expr.Type).RefType.Kind == ABT.ExprTypeKind.VOID) {
                        return new ABT.ConditionalExpr(cond, true_expr, false_expr, new ABT.PointerType(new ABT.VoidType()));
                    }

                    throw new NotImplementedException("More comparisons here.");

                default:
                    throw new InvalidOperationException("Expected compatible types in conditional expression.");
            }
        }
    }

    /// <summary>
    /// Function call: func(args)
    /// </summary>
    public class FuncCall : Expr {
        protected FuncCall(Expr func, ImmutableList<Expr> args) {
            this.Func = func;
            this.Args = args;
        }
        
        public static Expr Create(Expr func, ImmutableList<Expr> args) =>
            new FuncCall(func, args);

        public Expr Func { get; }

        public ImmutableList<Expr> Args { get; }

        public override ABT.Expr GetExpr(ABT.Env env) {

            // Step 1: get arguments passed into the function.
            // Note that currently the arguments are not casted based on the prototype.
            var args = this.Args.Select(_ => _.GetExpr(env)).ToList();

            // A special case:
            // If we cannot find the function prototype in the environment, make one up.
            // This function returns int.
            // Update the environment to add this function Type.
            if ((this.Func is Variable) && env.Find((this.Func as Variable).Name).IsNone) {
                // TODO: get this env used.
                env = env.PushEntry(ABT.Env.EntryKind.TYPEDEF, (this.Func as Variable).Name, ABT.FunctionType.Create(new ABT.LongType(true), args.ConvertAll(_ => Tuple.Create("", _.Type)), false
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
                    var ref_t = (func.Type as ABT.PointerType).RefType;
                    if (!(ref_t is ABT.FunctionType)) {
                        throw new InvalidOperationException("Expected a function pointer.");
                    }
                    func_type = ref_t as ABT.FunctionType;
                    break;

                default:
                    throw new InvalidOperationException("Expected a function in function call.");
            }


            Int32 num_args_prototype = func_type.Args.Count;
            Int32 num_args_actual = args.Count;

            // If this function doesn't take varargs, make sure the number of arguments match that in the prototype.
            if (!func_type.HasVarArgs && num_args_actual != num_args_prototype) {
                throw new InvalidOperationException("Number of arguments mismatch.");
            }

            // Anyway, you can't call a function with fewer arguments than the prototype.
            if (num_args_actual < num_args_prototype) {
                throw new InvalidOperationException("Too few arguments.");
            }

            // Make implicit cast.
            args = args.GetRange(0, num_args_prototype).Zip(func_type.Args,
                (arg, entry) => ABT.TypeCast.MakeCast(arg, entry.type)
            ).Concat(args.GetRange(num_args_prototype, num_args_actual - num_args_prototype)).ToList();

            return new ABT.FuncCall(func, func_type, args);
        }
    }

    /// <summary>
    /// Expr.attrib: get an attribute from a struct or union
    /// </summary>
    public class Attribute : Expr {
        protected Attribute(Expr expr, String member) {
            this.Expr = expr;
            this.Member = member;
        }

        public Expr Expr { get; }
        public String Member { get; }

        public static Expr Create(Expr expr, String member) =>
            new Attribute(expr, member);

        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.Expr expr = this.Expr.GetExpr(env);
            String name = this.Member;

            if (expr.Type.Kind != ABT.ExprTypeKind.STRUCT_OR_UNION) {
                throw new InvalidOperationException("Must get the attribute from a struct or union.");
            }

            ABT.Utils.StoreEntry entry = (expr.Type as ABT.StructOrUnionType).Attribs.First(_ => _.name == name);
            ABT.ExprType type = entry.type;

            return new ABT.Attribute(expr, name, type);
        }
    }

}