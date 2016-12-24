using System;

namespace AST {
    using static SemanticAnalysis;

    public sealed partial class PostIncrement {
        // TODO: Check lvalue
        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.Expr expr = SemantExpr(this.Expr, ref env);

            if (!expr.Type.IsScalar) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new ABT.PostIncrement(expr);
        }
    }

    public sealed partial class PostDecrement {
        // TODO: Check lvalue
        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.Expr expr = SemantExpr(this.Expr, ref env);

            if (!expr.Type.IsScalar) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new ABT.PostDecrement(expr);
        }
    }

    public sealed partial class SizeofType {
        public override ABT.Expr GetExpr(ABT.Env env) {
            var type = Semant(this.TypeName.GetExprType, ref env);
            return new ABT.ConstULong((UInt32)type.SizeOf, env);
        }
    }

    public sealed partial class SizeofExpr {
        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.Expr expr = SemantExpr(this.Expr, ref env);
            return new ABT.ConstULong((UInt32)expr.Type.SizeOf, env);
        }
    }

    public sealed partial class PreIncrement {
        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.Expr expr = SemantExpr(this.Expr, ref env);

            if (!expr.Type.IsScalar) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new ABT.PreIncrement(expr);
        }
    }

    public sealed partial class PreDecrement {
        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.Expr expr = SemantExpr(this.Expr, ref env);

            if (!expr.Type.IsScalar) {
                throw new InvalidOperationException("Expected a scalar.");
            }

            return new ABT.PreDecrement(expr);
        }
    }

    public sealed partial class Reference {
        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.Expr expr = SemantExpr(this.Expr, ref env);
            return new ABT.Reference(expr);
        }
    }

    public sealed partial class Dereference {
        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.Expr expr = SemantExpr(this.Expr, ref env);

            var pointerType = expr.Type as ABT.PointerType;
            if (pointerType == null) {
                throw new InvalidOperationException("Expected a pointer.");
            }

            if (!pointerType.RefType.IsComplete) {
                throw new InvalidOperationException(
                    "Cannot dereference incomplete Type."
                );
            }

            return new ABT.Dereference(expr, pointerType.RefType);
        }
    }

    public sealed partial class Positive {
        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.Expr expr = SemantExpr(this.Expr, ref env);

            if (!expr.Type.IsArith) {
                throw new InvalidOperationException(
                    "Expected arithmetic Type."
                );
            }

            return expr;
        }
    }

    public sealed partial class Negative {
        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.Expr expr = SemantExpr(this.Expr, ref env);

            if (!expr.Type.IsArith) {
                throw new InvalidOperationException(
                    "Expected arithmetic Type."
                );
            }

            if (expr.Type.IsIntegral) {
                expr = ABT.TypeCast.IntegralPromotion(expr).Item1;
            }

            if (expr.IsConstExpr) {
                switch (expr.Type.Kind) {
                    case ABT.ExprTypeKind.LONG:
                        return new ABT.ConstLong(
                            -((ABT.ConstLong)expr).Value,
                            env
                        );

                    case ABT.ExprTypeKind.ULONG:
                        return new ABT.ConstLong(
                            -(Int32)((ABT.ConstULong)expr).Value,
                            env
                        );

                    case ABT.ExprTypeKind.FLOAT:
                        return new ABT.ConstFloat(
                            -((ABT.ConstFloat)expr).Value,
                            env
                        );

                    case ABT.ExprTypeKind.DOUBLE:
                        return new ABT.ConstDouble(
                            -((ABT.ConstDouble)expr).Value,
                            env
                        );

                    default:
                        throw new InvalidOperationException();
                }
            }

            return new ABT.Negative(expr);
        }
    }

    public sealed partial class BitwiseNot {
        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.Expr expr = SemantExpr(this.Expr, ref env);

            if (!expr.Type.IsIntegral) {
                throw new InvalidOperationException("Expected integral Type.");
            }

            expr = ABT.TypeCast.IntegralPromotion(expr).Item1;

            if (expr.IsConstExpr) {
                switch (expr.Type.Kind) {
                    case ABT.ExprTypeKind.LONG:
                        return new ABT.ConstLong(
                            ~((ABT.ConstLong)expr).Value,
                            env
                        );
                        
                    case ABT.ExprTypeKind.ULONG:
                        return new ABT.ConstULong(
                            ~((ABT.ConstULong)expr).Value,
                            env
                        );
                        
                    default:
                        throw new InvalidOperationException();
                }
            }

            return new ABT.BitwiseNot(expr);
        }
    }

    public sealed partial class LogicalNot {
        public override ABT.Expr GetExpr(ABT.Env env) {
            var expr = SemantExpr(this.Expr, ref env);

            if (!expr.Type.IsArith) {
                throw new InvalidOperationException(
                    "Expected arithmetic type."
                );
            }

            if (expr.Type.IsIntegral) {
                expr = ABT.TypeCast.IntegralPromotion(expr).Item1;
            }

            if (expr.IsConstExpr) {
                Boolean isZero;
                switch (expr.Type.Kind) {
                    case ABT.ExprTypeKind.LONG:
                        isZero = ((ABT.ConstLong)expr).Value == 0;
                        break;
                        
                    case ABT.ExprTypeKind.ULONG:
                        isZero = ((ABT.ConstULong)expr).Value == 0;
                        break;
                        
                    case ABT.ExprTypeKind.FLOAT:
#pragma warning disable RECS0018 // float comparison
                        isZero = ((ABT.ConstFloat)expr).Value == 0;
#pragma warning restore RECS0018 // float comparison
                        break;
                        
                    case ABT.ExprTypeKind.DOUBLE:
#pragma warning disable RECS0018 // double comparison
                        isZero = ((ABT.ConstDouble)expr).Value == 0;
#pragma warning restore RECS0018 // double comparison
                        break;
                        
                    default:
                        throw new InvalidOperationException();
                }

                return new ABT.ConstLong(Convert.ToInt32(isZero), env);
            }

            return new ABT.LogicalNot(expr);
        }
    }

    public sealed partial class TypeCast {
        public override ABT.Expr GetExpr(ABT.Env env) {
            ABT.ExprType type = Semant(this.TypeName.GetExprType, ref env);
            ABT.Expr expr = SemantExpr(this.Expr, ref env);
            return ABT.TypeCast.MakeCast(expr, type);
        }
    }
}