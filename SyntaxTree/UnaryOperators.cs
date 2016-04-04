using System;
using static SyntaxTree.SemanticAnalysis;

namespace SyntaxTree {

    public abstract class UnaryExprOperator : Expr {
        protected UnaryExprOperator(Expr expr) {
            this.Expr = expr;
        }
        public Expr Expr { get; }
    }

    /// <summary>
    /// Postfix increment: x++
    /// </summary>
    // TODO: Check lvalue
    public sealed class PostIncrement : UnaryExprOperator {
        public PostIncrement(Expr expr)
            : base(expr) { }
        public static Expr Create(Expr expr) => new PostIncrement(expr);

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.Expr.GetExpr(env);

            if (!expr.Type.IsScalar) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new AST.PostIncrement(expr);
        }
    }

    /// <summary>
    /// Postfix decrement: x--
    /// </summary>
    // TODO: Check lvalue
    public sealed class PostDecrement : UnaryExprOperator {
        public PostDecrement(Expr expr)
            : base(expr) { }
        public static Func<Expr, Expr> Create { get; } = expr => new PostDecrement(expr);

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.Expr.GetExpr(env);

            if (!expr.Type.IsScalar) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new AST.PostDecrement(expr);
        }
    }

    /// <summary>
    /// sizeof(Type)
    /// </summary>
    [Checked]
    public sealed class SizeofType : Expr {
        public SizeofType(TypeName typeName) {
            this.TypeName = typeName;
        }
        public TypeName TypeName { get; }
        public static Expr Create(TypeName typeName) =>
            new SizeofType(typeName);

        public override AST.Expr GetExpr(AST.Env env) {
            //Tuple<AST.Env, AST.ExprType> type_env = this.TypeName.GetTypeEnv(env);
            //env = type_env.Item1;
            //AST.ExprType Type = type_env.Item2;

            var type = Semant(this.TypeName.GetExprType, ref env);

            return new AST.ConstULong((UInt32)type.SizeOf, env);
        }
    }

    /// <summary>
    /// sizeof(Expr)
    /// </summary>
    [Checked]
    public sealed class SizeofExpr : UnaryExprOperator {
        public SizeofExpr(Expr expr)
            : base(expr) { }
        public static Expr Create(Expr expr) =>
            new SizeofExpr(expr);

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.Expr.GetExpr(env);
            return new AST.ConstULong((UInt32)expr.Type.SizeOf, env);
        }
    }

    /// <summary>
    /// Prefix increment: ++x
    /// </summary>
    [Checked]
    public sealed class PreIncrement : UnaryExprOperator {
        public PreIncrement(Expr expr)
            : base(expr) { }
        public static Expr Create(Expr expr) =>
            new PreIncrement(expr);

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.Expr.GetExpr(env);

            if (!expr.Type.IsScalar) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new AST.PreIncrement(expr);
        }
    }

    /// <summary>
    /// Prefix decrement: --x
    /// </summary>
    [Checked]
    public sealed class PreDecrement : UnaryExprOperator {
        public PreDecrement(Expr expr)
            : base(expr) { }
        public static Expr Create(Expr expr) =>
            new PreDecrement(expr);

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.Expr.GetExpr(env);

            if (!expr.Type.IsScalar) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new AST.PreDecrement(expr);
        }
    }

    /// <summary>
    /// Reference: &expr
    /// </summary>
    [Checked]
    public sealed class Reference : UnaryExprOperator {
        public Reference(Expr expr)
            : base(expr) { }
        public static Expr Create(Expr expr) =>
            new Reference(expr);

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.Expr.GetExpr(env);
            return new AST.Reference(expr);
        }
    }

    /// <summary>
    /// Dereference: *Expr
    /// 
    /// Note that Expr might have an **incomplete** Type.
    /// We need to search the environment
    /// </summary>
    [Checked]
    public sealed class Dereference : UnaryExprOperator {
        public Dereference(Expr expr)
            : base(expr) { }
        public static Expr Create(Expr expr) => new Dereference(expr);

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.Expr.GetExpr(env);

            if (expr.Type.Kind != AST.ExprTypeKind.POINTER) {
                throw new InvalidOperationException("Expected a pointer.");
            }

            AST.ExprType type = ((AST.PointerType)expr.Type).RefType;
            if (type.Kind == AST.ExprTypeKind.STRUCT_OR_UNION && !((AST.StructOrUnionType)type).IsComplete) {
                throw new InvalidOperationException("Cannot dereference incomplete Type.");
            }

            return new AST.Dereference(expr, type);
        }
    }

    /// <summary>
    /// Merely a check on arithmetic Type.
    /// </summary>
    [Checked]
    public sealed class Positive : UnaryExprOperator {
        public Positive(Expr expr)
            : base(expr) { }
        public static Expr Create(Expr expr) =>
            new Positive(expr);
        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.Expr.GetExpr(env);

            if (!expr.Type.IsArith) {
                throw new InvalidOperationException("Expected arithmetic Type.");
            }

            return expr;
        }
    }

    /// <summary>
    /// Negative: requires arithmetic Type.
    /// </summary>
    [Checked]
    public sealed class Negative : UnaryExprOperator {
        public Negative(Expr expr)
            : base(expr) { }
        public static Expr Create(Expr expr) =>
            new Negative(expr);
        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.Expr.GetExpr(env);

            if (!expr.Type.IsArith) {
                throw new InvalidOperationException("Expected arithmetic Type.");
            }

            if (expr.Type.IsIntegral) {
                expr = AST.TypeCast.IntegralPromotion(expr).Item1;
            }

            if (expr.IsConstExpr) {
                switch (expr.Type.Kind) {
                    case AST.ExprTypeKind.LONG:
                        return new AST.ConstLong(-((AST.ConstLong)expr).Value, env);

                    case AST.ExprTypeKind.ULONG:
                        return new AST.ConstLong(-(Int32)((AST.ConstULong)expr).Value, env);

                    case AST.ExprTypeKind.FLOAT:
                        return new AST.ConstFloat(-((AST.ConstFloat)expr).Value, env);

                    case AST.ExprTypeKind.DOUBLE:
                        return new AST.ConstDouble(-((AST.ConstDouble)expr).Value, env);

                    default:
                        throw new InvalidOperationException();
                }
            }

            return new AST.Negative(expr);
        }
    }

    /// <summary>
    /// Bitwise not: requires integral.
    /// </summary>
    [Checked]
    public sealed class BitwiseNot : UnaryExprOperator {
        public BitwiseNot(Expr expr)
            : base(expr) { }
        public static Expr Create(Expr expr) =>
            new BitwiseNot(expr);
        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr expr = this.Expr.GetExpr(env);

            if (!expr.Type.IsIntegral) {
                throw new InvalidOperationException("Expected integral Type.");
            }

            expr = AST.TypeCast.IntegralPromotion(expr).Item1;

            if (expr.IsConstExpr) {
                switch (expr.Type.Kind) {
                    case AST.ExprTypeKind.LONG:
                        return new AST.ConstLong(~((AST.ConstLong)expr).Value, env);
                    case AST.ExprTypeKind.ULONG:
                        return new AST.ConstULong(~((AST.ConstULong)expr).Value, env);
                    default:
                        throw new InvalidOperationException();
                }
            }

            return new AST.BitwiseNot(expr);
        }
    }

    /// <summary>
    /// Logical not
    /// </summary>
    [Checked]
    public sealed class LogicalNot : UnaryExprOperator {
        public LogicalNot(Expr expr)
            : base(expr) { }
        public static Expr Create(Expr expr) =>
            new LogicalNot(expr);

        public override AST.Expr GetExpr(AST.Env env) {
            var expr = this.Expr.GetExpr(env);

            if (!expr.Type.IsArith) {
                throw new InvalidOperationException("Expected arithmetic Type.");
            }

            if (expr.Type.IsIntegral) {
                expr = AST.TypeCast.IntegralPromotion(expr).Item1;
            }

            if (expr.IsConstExpr) {
                Boolean isZero;
                switch (expr.Type.Kind) {
                    case AST.ExprTypeKind.LONG:
                        isZero = ((AST.ConstLong)expr).Value == 0;
                        break;
                    case AST.ExprTypeKind.ULONG:
                        isZero = ((AST.ConstULong)expr).Value == 0;
                        break;
                    case AST.ExprTypeKind.FLOAT:
                        isZero = ((AST.ConstFloat)expr).Value == 0;
                        break;
                    case AST.ExprTypeKind.DOUBLE:
                        isZero = ((AST.ConstDouble)expr).Value == 0;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                return new AST.ConstLong(Convert.ToInt32(isZero), env);
            }

            return new AST.LogicalNot(expr);
        }
    }

    /// <summary>
    /// User-specified explicit Type cast
    /// </summary>
    [Checked]
    public sealed class TypeCast : Expr {
        public TypeCast(TypeName typeName, Expr expr) {
            this.TypeName = typeName;
            this.Expr = expr;
        }

        public TypeName TypeName { get; }
        public Expr Expr { get; }

        public static Expr Create(TypeName typeName, Expr expr) =>
            new TypeCast(typeName, expr);

        public override AST.Expr GetExpr(AST.Env env) {
            AST.ExprType type = Semant(this.TypeName.GetExprType, ref env);
            AST.Expr expr = this.Expr.GetExpr(env);
            return AST.TypeCast.MakeCast(expr, type);
        }
    }

}