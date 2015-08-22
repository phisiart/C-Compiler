using System;

namespace SyntaxTree {

    /// <summary>
    /// Postfix increment: x++
    /// </summary>
    public class PostIncrement : Expr {
        public PostIncrement(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

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
    public class PostDecrement : Expr {
        public PostDecrement(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

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
    public class SizeofType : Expr {
        public SizeofType(TypeName type_name) {
            this.type_name = type_name;
        }
        public readonly TypeName type_name;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.ExprType type = type_name.GetExprType(env);
            return new AST.ConstULong((UInt32)type.SizeOf);
        }
    }
    
    /// <summary>
    /// sizeof(expr)
    /// </summary>
    public class SizeofExpr : Expr {
        public SizeofExpr(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);
            return new AST.ConstULong((UInt32)expr.type.SizeOf);
        }
    }

    /// <summary>
    /// Prefix increment: ++x
    /// </summary>
    public class PrefixIncrement : Expr {
        public PrefixIncrement(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

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
    public class PreDecrement : Expr {
        public PreDecrement(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

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
    public class Reference : Expr {
        public Reference(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);
            return new AST.Reference(expr, new AST.TPointer(expr.type));
        }
    }

    /// <summary>
    /// Dereference: *expr
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
    public class Positive : Expr {
        public Positive(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

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
    public class Negative : Expr {
        public Negative(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsArith) {
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

            if (!expr.type.IsIntegral) {
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
    }

    /// <summary>
    /// Logical not
    /// </summary>
    public class LogicalNot : Expr {
        public LogicalNot(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.expr.GetExpr(env);

            if (!expr.type.IsArith) {
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
    }

}