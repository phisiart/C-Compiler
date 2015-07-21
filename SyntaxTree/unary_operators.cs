using System;

namespace SyntaxTree {

    /// <summary>
    /// Increment
    /// 
    /// x++
    /// </summary>
    public class Increment : Expr {
        public Increment(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsScalar()) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new AST.PostIncrement(expr, expr.type);
        }

        [Obsolete]
        public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
            AST.Expr expr;

            Tuple<AST.Env, AST.Expr> r_expr = this.expr.GetExprEnv(env);
            env = r_expr.Item1;
            expr = r_expr.Item2;

            if (!expr.type.IsScalar()) {
                throw new InvalidOperationException("Error: expected a scalar");
            }

            return new Tuple<AST.Env, AST.Expr>(env, new AST.PostIncrement(expr, expr.type));
        }
    }

    /// <summary>
    /// Decrement
    /// 
    /// x--
    /// </summary>
    public class Decrement : Expr {
        public Decrement(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsScalar()) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new AST.PostDecrement(expr, expr.type);
        }

        [Obsolete]
        public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
            AST.Expr expr;

            Tuple<AST.Env, AST.Expr> r_expr = this.expr.GetExprEnv(env);
            env = r_expr.Item1;
            expr = r_expr.Item2;

            if (!expr.type.IsScalar()) {
                throw new InvalidOperationException("Error: expected a scalar");
            }

            return new Tuple<AST.Env, AST.Expr>(env, new AST.PostDecrement(expr, expr.type));
        }
    }

    public class SizeofType : Expr {
        public SizeofType(TypeName type_name) {
            this.type_name = type_name;
        }
        public readonly TypeName type_name;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.ExprType type = type_name.GetExprType(env);
            return new AST.ConstULong((UInt32)type.SizeOf);
        }

        [Obsolete]
        public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
            AST.ExprType type;

            Tuple<AST.Env, AST.ExprType> r_typename = type_name.GetExprTypeEnv(env);
            env = r_typename.Item1;
            type = r_typename.Item2;

            return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstULong((UInt32)type.SizeOf));
        }
    }

    public class SizeofExpression : Expr {
        public SizeofExpression(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);
            return new AST.ConstULong((UInt32)expr.type.SizeOf);
        }

        [Obsolete]
        public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
            AST.Expr expr;

            Tuple<AST.Env, AST.Expr> r_expr = this.expr.GetExprEnv(env);
            env = r_expr.Item1;
            expr = r_expr.Item2;

            return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstULong((UInt32)expr.type.SizeOf));
        }
    }

    /// <summary>
    /// Prefix Increment
    /// 
    /// ++x
    /// </summary>
    public class PrefixIncrement : Expr {
        public PrefixIncrement(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsScalar()) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new AST.PreIncrement(expr, expr.type);
        }

        [Obsolete]
        public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
            AST.Expr expr;

            Tuple<AST.Env, AST.Expr> r_expr = this.expr.GetExprEnv(env);
            env = r_expr.Item1;
            expr = r_expr.Item2;

            if (!expr.type.IsScalar()) {
                throw new InvalidOperationException("Error: expected a scalar");
            }

            return new Tuple<AST.Env, AST.Expr>(env, new AST.PreIncrement(expr, expr.type));
        }
    }

    /// <summary>
    /// Prefix Decrement
    /// 
    /// --x
    /// </summary>
    public class PrefixDecrement : Expr {
        public PrefixDecrement(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsScalar()) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new AST.PreDecrement(expr, expr.type);
        }

        [Obsolete]
        public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
            AST.Expr expr;

            Tuple<AST.Env, AST.Expr> r_expr = this.expr.GetExprEnv(env);
            env = r_expr.Item1;
            expr = r_expr.Item2;

            if (!expr.type.IsScalar()) {
                throw new InvalidOperationException("Error: expected a scalar");
            }

            return new Tuple<AST.Env, AST.Expr>(env, new AST.PreDecrement(expr, expr.type));
        }
    }

    /// <summary>
    /// Reference
	/// 
	/// &expr
    /// </summary>
    public class Reference : Expr {
        public Reference(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);
            return new AST.Reference(expr, new AST.TPointer(expr.type));
        }

        [Obsolete]
        public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = this.expr.GetExprEnv(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            return new Tuple<AST.Env, AST.Expr>(env, new AST.Reference(expr, new AST.TPointer(expr.type)));
        }
    }

    /// <summary>
    /// Dereference
    /// 
    /// *expr
    /// 
    /// Note that expr might have an **incomplete** type.
    /// We need to search the environment
    /// </summary>
    public class Dereference : Expr {
        public Dereference(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (expr.type.kind != AST.ExprType.Kind.POINTER) {
                throw new InvalidOperationException("Expected a pointer.");
            }

            AST.ExprType ref_type = ((AST.TPointer)expr.type).referenced_type;
            if (ref_type.kind == AST.ExprType.Kind.INCOMPLETE_STRUCT) {
                AST.Env.Entry r_find = env.Find("struct " + ((AST.TIncompleteStruct)ref_type).struct_name);
                if (r_find.entry_loc != AST.Env.EntryLoc.TYPEDEF) {
                    throw new InvalidOperationException("Cannot find struct.");
                }
                ref_type = r_find.entry_type;
            }

            return new AST.Dereference(expr, ref_type);
        }

        [Obsolete]
        public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = this.expr.GetExprEnv(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            if (expr.type.kind != AST.ExprType.Kind.POINTER) {
                throw new Exception("Error: dereferencing a non-pointer");
            }

            AST.ExprType ref_type = ((AST.TPointer)expr.type).referenced_type;
            if (ref_type.kind == AST.ExprType.Kind.INCOMPLETE_STRUCT) {
                AST.Env.Entry r_find = env.Find("struct " + ((AST.TIncompleteStruct)ref_type).struct_name);
                if (r_find.entry_loc != AST.Env.EntryLoc.TYPEDEF) {
                    throw new InvalidOperationException("Error: cannot find struct");
                }
                ref_type = r_find.entry_type;
            }

            // no matter constant or not
            return new Tuple<AST.Env, AST.Expr>(env, new AST.Dereference(expr, ref_type));
        }
    }

    /// <summary>
    /// Merely a check on arithmetic type.
    /// </summary>
    public class Positive : Expr {
        public Positive(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsArith()) {
                throw new InvalidOperationException("Expected arithmetic type.");
            }

            return expr;
        }

        [Obsolete]
        // TODO : [finished] Positive.GetExpr(env) -> (env, expr)
        public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = this.expr.GetExprEnv(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            if (!expr.type.IsArith()) {
                Log.SemantError("Error: negation expectes arithmetic type.");
                return null;
            }

            return new Tuple<AST.Env, AST.Expr>(env, expr);
        }

    }

    /// <summary>
    /// Negative: requires arithmetic type.
    /// </summary>
    public class Negative : Expr {
        public Negative(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsArith()) {
                throw new InvalidOperationException("Expected arithmetic type.");
            }

            if (expr.IsConstExpr()) {
                switch (expr.type.kind) {
                    case AST.ExprType.Kind.LONG:
                        return new AST.ConstLong(-((AST.ConstLong)expr).value);
                    case AST.ExprType.Kind.ULONG:
                        return new AST.ConstLong(-(Int32)((AST.ConstULong)expr).value);
                    case AST.ExprType.Kind.FLOAT:
                        return new AST.ConstFloat(-((AST.ConstFloat)expr).value);
                    case AST.ExprType.Kind.DOUBLE:
                        return new AST.ConstDouble(-((AST.ConstDouble)expr).value);
                    default:
                        throw new InvalidOperationException();
                }
            }

            return new AST.Negative(expr, expr.type);
        }

        //// TODO : [finished] Negative.GetExpr(env) -> (env, expr)
        //public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
        //    Tuple<AST.Env, AST.Expr> r_expr = this.expr.GetExprEnv(env);
        //    env = r_expr.Item1;
        //    AST.Expr expr = r_expr.Item2;

        //    if (!expr.type.IsArith()) {
        //        Log.SemantError("Error: negation expectes arithmetic type.");
        //        return null;
        //    }

        //    if (expr.IsConstExpr()) {
        //        switch (expr.type.kind) {
        //        case AST.ExprType.Kind.LONG:
        //            AST.ConstLong long_expr = (AST.ConstLong)expr;
        //            expr = new AST.ConstLong(-long_expr.value);
        //            break;
        //        case AST.ExprType.Kind.ULONG:
        //            AST.ConstULong ulong_expr = (AST.ConstULong)expr;
        //            expr = new AST.ConstLong(-(Int32)ulong_expr.value);
        //            break;
        //        case AST.ExprType.Kind.FLOAT:
        //            AST.ConstFloat float_expr = (AST.ConstFloat)expr;
        //            expr = new AST.ConstFloat(-float_expr.value);
        //            break;
        //        case AST.ExprType.Kind.DOUBLE:
        //            AST.ConstDouble double_expr = (AST.ConstDouble)expr;
        //            expr = new AST.ConstDouble(-double_expr.value);
        //            break;
        //        default:
        //            Log.SemantError("Error: wrong constant type?");
        //            break;
        //        }
        //    } else {
        //        expr = new AST.Negative(expr, expr.type);
        //    }

        //    return new Tuple<AST.Env, AST.Expr>(env, expr);
        //}

    }

    /// <summary>
    /// Bitwise not: requires integral.
    /// </summary>
    public class BitwiseNot : Expr {
        public BitwiseNot(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsIntegral()) {
                throw new InvalidOperationException("Expected integral type.");
            }

            if (expr.IsConstExpr()) {
                switch (expr.type.kind) {
                    case AST.ExprType.Kind.LONG:
                        return new AST.ConstLong(~((AST.ConstLong)expr).value);
                    case AST.ExprType.Kind.ULONG:
                        return new AST.ConstULong(~((AST.ConstULong)expr).value);
                    default:
                        throw new InvalidOperationException();
                }
            }

            return new AST.BitwiseNot(expr, expr.type);
        }

        //// TODO : [finished] BitwiseNot.GetExpr(env) -> (env, expr)
        //public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
        //    Tuple<AST.Env, AST.Expr> r_expr = this.expr.GetExprEnv(env);
        //    env = r_expr.Item1;
        //    AST.Expr expr = r_expr.Item2;

        //    if (!expr.type.IsIntegral()) {
        //        Log.SemantError("Error: operator '~' expectes integral type.");
        //        return null;
        //    }

        //    if (expr.IsConstExpr()) {
        //        switch (expr.type.kind) {
        //        case AST.ExprType.Kind.LONG:
        //            AST.ConstLong long_expr = (AST.ConstLong)expr;
        //            expr = new AST.ConstLong(~long_expr.value);
        //            break;
        //        case AST.ExprType.Kind.ULONG:
        //            AST.ConstULong ulong_expr = (AST.ConstULong)expr;
        //            expr = new AST.ConstULong(~ulong_expr.value);
        //            break;
        //        default:
        //            Log.SemantError("Error: wrong constant type?");
        //            break;
        //        }
        //    } else {
        //        expr = new AST.BitwiseNot(expr, expr.type);
        //    }

        //    return new Tuple<AST.Env, AST.Expr>(env, expr);
        //}

    }

    /// <summary>
    /// Logical not
    /// </summary>
    public class Not : Expr {
        public Not(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsArith()) {
                throw new InvalidOperationException("Expected arithmetic type.");
            }

            if (expr.IsConstExpr()) {
                Boolean is_zero;
                switch (expr.type.kind) {
                    case AST.ExprType.Kind.LONG:
                        is_zero = ((AST.ConstLong)expr).value == 0;
                        break;
                    case AST.ExprType.Kind.ULONG:
                        is_zero = ((AST.ConstULong)expr).value == 0;
                        break;
                    case AST.ExprType.Kind.FLOAT:
                        is_zero = ((AST.ConstFloat)expr).value == 0;
                        break;
                    case AST.ExprType.Kind.DOUBLE:
                        is_zero = ((AST.ConstDouble)expr).value == 0;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                return new AST.ConstLong(Convert.ToInt32(is_zero));
            }

            return new AST.LogicalNot(expr, new AST.TLong(expr.type.is_const, expr.type.is_volatile));
        }

        //// TODO : [finished] Not.GetExpr(env) -> (env, expr(type=long))
        //public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
        //    Tuple<AST.Env, AST.Expr> r_expr = this.expr.GetExprEnv(env);
        //    env = r_expr.Item1;
        //    AST.Expr expr = r_expr.Item2;

        //    if (!expr.type.IsArith()) {
        //        Log.SemantError("Error: operator '!' expectes arithmetic type.");
        //        return null;
        //    }

        //    if (expr.IsConstExpr()) {
        //        Boolean value = false;
        //        switch (expr.type.type_kind) {
        //        case AST.ExprType.Kind.LONG:
        //            AST.ConstLong long_expr = (AST.ConstLong)expr;
        //            value = long_expr.value != 0;
        //            break;
        //        case AST.ExprType.Kind.ULONG:
        //            AST.ConstULong ulong_expr = (AST.ConstULong)expr;
        //            value = ulong_expr.value != 0;
        //            break;
        //        case AST.ExprType.Kind.FLOAT:
        //            AST.ConstFloat float_expr = (AST.ConstFloat)expr;
        //            value = float_expr.value != 0;
        //            break;
        //        case AST.ExprType.Kind.DOUBLE:
        //            AST.ConstDouble double_expr = (AST.ConstDouble)expr;
        //            value = double_expr.value != 0;
        //            break;
        //        default:
        //            Log.SemantError("Error: wrong constant type?");
        //            break;
        //        }
        //        if (value) {
        //            expr = new AST.ConstLong(1);
        //        } else {
        //            expr = new AST.ConstLong(0);
        //        }
        //    } else {
        //        expr = new AST.LogicalNot(expr, new AST.TLong(expr.type.is_const, expr.type.is_volatile));
        //    }

        //    return new Tuple<AST.Env, AST.Expr>(env, expr);
        //}

    }

    /// <summary>
    /// User-specified explicit type cast
    /// </summary>
    public class TypeCast : Expr {
        public TypeCast(TypeName type_name, Expr expr) {
            this.type_name = type_name;
            this.expr = expr;
        }
        public readonly TypeName type_name;
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            // TODO: does this change environment?
            AST.ExprType type = type_name.GetExprType(env);

            AST.Expr expr = this.expr.GetExpr(env);

            return AST.TypeCast.MakeCast(expr, type);
        }

        //public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
        //	AST.ExprType type;
        //	AST.Expr expr;

        //	Tuple<AST.Env, AST.ExprType> r_typename = type_name.GetExprType(env);
        //	env = r_typename.Item1;
        //	type = r_typename.Item2;

        //          Tuple<AST.Env, AST.Expr> r_expr = this.expr.GetExprEnv(env);
        //	env = r_expr.Item1;
        //	expr = r_expr.Item2;

        //	return new Tuple<AST.Env, AST.Expr>(env, AST.TypeCast.MakeCast(expr, type));
        //}
    }

}