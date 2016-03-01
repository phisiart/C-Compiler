using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SyntaxTree {

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
        public abstract AST.Expr GetExpr(AST.Env env);
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

        public override AST.Expr GetExpr(AST.Env env) {
            Option<AST.Env.Entry> entry_opt = env.Find(this.Name);

            if (entry_opt.IsNone) {
                throw new InvalidOperationException($"Cannot find variable '{this.Name}'");
            }

            AST.Env.Entry entry = entry_opt.Value;

            switch (entry.Kind) {
                case AST.Env.EntryKind.TYPEDEF:
                    throw new InvalidOperationException($"Expected a variable '{this.Name}', not a typedef.");
                case AST.Env.EntryKind.ENUM:
                    return new AST.ConstLong(entry.Offset, env);
                case AST.Env.EntryKind.FRAME:
                case AST.Env.EntryKind.GLOBAL:
                case AST.Env.EntryKind.STACK:
                    return new AST.Variable(entry.Type, this.Name, env);
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

        public override AST.Expr GetExpr(AST.Env env) {
            ImmutableList<AST.Expr> exprs = this.Exprs.ConvertAll(expr => expr.GetExpr(env));
            return new AST.AssignList(exprs);
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

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr cond = this.Cond.GetExpr(env);

            if (!cond.Type.IsScalar) {
                throw new InvalidOperationException("Expected a scalar condition in conditional expression.");
            }

            if (cond.Type.IsIntegral) {
                cond = AST.TypeCast.IntegralPromotion(cond).Item1;
            }

            AST.Expr true_expr = this.TrueExpr.GetExpr(env);
            AST.Expr false_expr = this.FalseExpr.GetExpr(env);

            // 1. if both true_expr and false_Expr have arithmetic types:
            //    perform usual arithmetic conversion
            if (true_expr.Type.IsArith && false_expr.Type.IsArith) {
                var r_cast = AST.TypeCast.UsualArithmeticConversion(true_expr, false_expr);
                true_expr = r_cast.Item1;
                false_expr = r_cast.Item2;
                return new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.Type);
            }

            if (true_expr.Type.Kind != false_expr.Type.Kind) {
                throw new InvalidOperationException("Operand types not match in conditional expression.");
            }

            switch (true_expr.Type.Kind) {
                // 2. if both true_expr and false_expr have struct or union Type
                //    make sure they are compatible
                case AST.ExprTypeKind.STRUCT_OR_UNION:
                    if (!true_expr.Type.EqualType(false_expr.Type)) {
                        throw new InvalidOperationException("Expected compatible types in conditional expression.");
                    }
                    return new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.Type);

                // 3. if both true_expr and false_expr have void Type
                //    return void
                case AST.ExprTypeKind.VOID:
                    return new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.Type);

                // 4. if both true_expr and false_expr have pointer Type
                case AST.ExprTypeKind.POINTER:

                    // if either points to void, convert to void *
                    if (((AST.PointerType)true_expr.Type).RefType.Kind == AST.ExprTypeKind.VOID
                        || ((AST.PointerType)false_expr.Type).RefType.Kind == AST.ExprTypeKind.VOID) {
                        return new AST.ConditionalExpr(cond, true_expr, false_expr, new AST.PointerType(new AST.VoidType()));
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

        public readonly Expr Func;
        public readonly ImmutableList<Expr> Args;

        public override AST.Expr GetExpr(AST.Env env) {

            // Step 1: get arguments passed into the function.
            // Note that currently the arguments are not casted based on the prototype.
            var args = this.Args.Select(_ => _.GetExpr(env)).ToList();

            // A special case:
            // If we cannot find the function prototype in the environment, make one up.
            // This function returns int.
            // Update the environment to add this function Type.
            if ((this.Func is Variable) && env.Find((this.Func as Variable).Name).IsNone) {
                // TODO: get this env used.
                env = env.PushEntry(AST.Env.EntryKind.TYPEDEF, (this.Func as Variable).Name, AST.FunctionType.Create(new AST.LongType(true), args.ConvertAll(_ => Tuple.Create("", _.Type)), false
                    )
                );
            }

            // Step 2: get function expression.
            AST.Expr func = this.Func.GetExpr(env);

            // Step 3: get the function Type.
            AST.FunctionType func_type;
            switch (func.Type.Kind) {
                case AST.ExprTypeKind.FUNCTION:
                    func_type = func.Type as AST.FunctionType;
                    break;

                case AST.ExprTypeKind.POINTER:
                    var ref_t = (func.Type as AST.PointerType).RefType;
                    if (!(ref_t is AST.FunctionType)) {
                        throw new InvalidOperationException("Expected a function pointer.");
                    }
                    func_type = ref_t as AST.FunctionType;
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
                (arg, entry) => AST.TypeCast.MakeCast(arg, entry.type)
            ).Concat(args.GetRange(num_args_prototype, num_args_actual - num_args_prototype)).ToList();

            return new AST.FuncCall(func, func_type, args);
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

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.Expr.GetExpr(env);
            String name = this.Member;

            if (expr.Type.Kind != AST.ExprTypeKind.STRUCT_OR_UNION) {
                throw new InvalidOperationException("Must get the attribute from a struct or union.");
            }

            AST.Utils.StoreEntry entry = (expr.Type as AST.StructOrUnionType).Attribs.First(_ => _.name == name);
            AST.ExprType type = entry.type;

            return new AST.Attribute(expr, name, type);
        }
    }

}