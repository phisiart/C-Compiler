using System;

namespace SyntaxTree {

    /// <summary>
    /// Postfix increment: x++
    /// </summary>
    // TODO: Check lvalue
    public class PostIncrement : Expr {
        public PostIncrement(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public static Func<Expr, Expr> Create { get; } = expr => new PostIncrement(expr);

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsScalar) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new AST.PostIncrement(expr);
        }
    }

    /// <summary>
    /// Postfix decrement: x--
    /// </summary>
    // TODO: Check lvalue
    public class PostDecrement : Expr {
        public PostDecrement(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public static Func<Expr, Expr> Create { get; } = expr => new PostDecrement(expr);

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsScalar) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new AST.PostDecrement(expr);
        }
    }

    /// <summary>
    /// sizeof(type)
    /// </summary>
    [Checked]
    public class SizeofType : Expr {
        public SizeofType(TypeName type_name) {
            this.type_name = type_name;
        }
        public readonly TypeName type_name;
        public static Expr Create(TypeName typeName) =>
            new SizeofType(typeName);
        public override AST.Expr GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.ExprType> type_env = this.type_name.GetTypeEnv(env);
            env = type_env.Item1;
            AST.ExprType type = type_env.Item2;
            return new AST.ConstULong((UInt32)type.SizeOf, env);
        }
    }
    
    /// <summary>
    /// sizeof(expr)
    /// </summary>
    [Checked]
    public class SizeofExpr : Expr {
        public SizeofExpr(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public static Expr Create(Expr expr) =>
            new SizeofExpr(expr);
        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);
            return new AST.ConstULong((UInt32)expr.type.SizeOf, env);
        }
    }

    /// <summary>
    /// Prefix increment: ++x
    /// </summary>
    [Checked]
    public class PreIncrement : Expr {
        public PreIncrement(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public static Expr Create(Expr expr) =>
            new PreIncrement(expr);

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsScalar) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new AST.PreIncrement(expr);
        }
    }

    /// <summary>
    /// Prefix decrement: --x
    /// </summary>
    [Checked]
    public class PreDecrement : Expr {
        public PreDecrement(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public static Expr Create(Expr expr) =>
            new PreDecrement(expr);

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsScalar) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new AST.PreDecrement(expr);
        }
    }

    /// <summary>
    /// Reference: &expr
    /// </summary>
    [Checked]
    public class Reference : Expr {
        public Reference(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public static Expr Create(Expr expr) =>
            new Reference(expr);

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);
            return new AST.Reference(expr);
        }
    }

    /// <summary>
    /// Dereference: *expr
    /// 
    /// Note that expr might have an **incomplete** type.
    /// We need to search the environment
    /// </summary>
    [Checked]
    public class Dereference : Expr {
        public Dereference(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public static Expr Create(Expr expr) => new Dereference(expr);

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (expr.type.kind != AST.ExprType.Kind.POINTER) {
                throw new InvalidOperationException("Expected a pointer.");
            }

            AST.ExprType type = ((AST.TPointer)expr.type).ref_t;
            if (type.kind == AST.ExprType.Kind.STRUCT_OR_UNION && !((AST.TStructOrUnion)type).IsComplete) {
                throw new InvalidOperationException("Cannot dereference incomplete type.");
            }

            return new AST.Dereference(expr, type);
        }
    }

    /// <summary>
    /// Merely a check on arithmetic type.
    /// </summary>
    [Checked]
    public class Positive : Expr {
        public Positive(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public static Expr Create(Expr expr) =>
            new Positive(expr);
        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsArith) {
                throw new InvalidOperationException("Expected arithmetic type.");
            }

            return expr;
        }
    }

    /// <summary>
    /// Negative: requires arithmetic type.
    /// </summary>
    [Checked]
    public class Negative : Expr {
        public Negative(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public static Expr Create(Expr expr) =>
            new Negative(expr);
        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsArith) {
                throw new InvalidOperationException("Expected arithmetic type.");
            }

            if (expr.type.IsIntegral) {
                expr = AST.TypeCast.IntegralPromotion(expr).Item1;
            }

            if (expr.IsConstExpr) {
                switch (expr.type.kind) {
                    case AST.ExprType.Kind.LONG:
                        return new AST.ConstLong(-((AST.ConstLong)expr).value, env);

                    case AST.ExprType.Kind.ULONG:
                        return new AST.ConstLong(-(Int32)((AST.ConstULong)expr).value, env);

                    case AST.ExprType.Kind.FLOAT:
                        return new AST.ConstFloat(-((AST.ConstFloat)expr).value, env);

                    case AST.ExprType.Kind.DOUBLE:
                        return new AST.ConstDouble(-((AST.ConstDouble)expr).value, env);

                    default:
                        throw new InvalidOperationException();
                }
            }

            return new AST.Negative(expr, expr.type);
        }
    }

    /// <summary>
    /// Bitwise not: requires integral.
    /// </summary>
    [Checked]
    public class BitwiseNot : Expr {
        public BitwiseNot(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public static Expr Create(Expr expr) =>
            new BitwiseNot(expr);
        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsIntegral) {
                throw new InvalidOperationException("Expected integral type.");
            }

            expr = AST.TypeCast.IntegralPromotion(expr).Item1;

            if (expr.IsConstExpr) {
                switch (expr.type.kind) {
                    case AST.ExprType.Kind.LONG:
                        return new AST.ConstLong(~((AST.ConstLong)expr).value, env);
                    case AST.ExprType.Kind.ULONG:
                        return new AST.ConstULong(~((AST.ConstULong)expr).value, env);
                    default:
                        throw new InvalidOperationException();
                }
            }

            return new AST.BitwiseNot(expr, expr.type);
        }
    }

    /// <summary>
    /// Logical not
    /// </summary>
    [Checked]
    public class LogicalNot : Expr {
        public LogicalNot(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public static Expr Create(Expr expr) =>
            new LogicalNot(expr);
        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsArith) {
                throw new InvalidOperationException("Expected arithmetic type.");
            }

            if (expr.type.IsIntegral) {
                expr = AST.TypeCast.IntegralPromotion(expr).Item1;
            }

            if (expr.IsConstExpr) {
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
                return new AST.ConstLong(Convert.ToInt32(is_zero), env);
            }

            return new AST.LogicalNot(expr, new AST.TLong(expr.type.is_const, expr.type.is_volatile));
        }
    }

    /// <summary>
    /// User-specified explicit type cast
    /// </summary>
    [Checked]
    public class TypeCast : Expr {
        public TypeCast(TypeName type_name, Expr expr) {
            this.type_name = type_name;
            this.expr = expr;
        }
        public readonly TypeName type_name;
        public readonly Expr expr;
        public static Expr Create(TypeName typeName, Expr expr) =>
            new TypeCast(typeName, expr);
        public override AST.Expr GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.ExprType> type_env = this.type_name.GetTypeEnv(env);
            env = type_env.Item1;
            AST.ExprType type = type_env.Item2;

            AST.Expr expr = this.expr.GetExpr(env);

            return AST.TypeCast.MakeCast(expr, type);
        }
    }

}