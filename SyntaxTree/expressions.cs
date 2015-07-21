using System;
using System.Collections.Generic;
using System.Linq;

namespace SyntaxTree {

    // 3.2.1.5
    /* First, if either operand has type long double, the other operand is converted to long double.
     * Otherwise, if either operand has type double, the other operand is converted to double.
     * Otherwise, if either operand has type float, the other operand is converted to float.
     * Otherwise, the integral promotions are performed on both operands.
     * Then the following rules are applied:
     * If either operand has type unsigned long Int32, the other operand is converted to unsigned long Int32.
     * Otherwise, if one operand has type long Int32 and the other has type unsigned Int32, if a long Int32 can represent all values of an unsigned Int32, the operand of type unsigned Int32 is converted to long Int32;
     * if a long Int32 cannot represent all the values of an unsigned Int32, both operands are converted to unsigned long Int32. Otherwise, if either operand has type long Int32, the other operand is converted to long Int32.
     * Otherwise, if either operand has type unsigned Int32, the other operand is converted to unsigned Int32.
     * Otherwise, both operands have type Int32.*/

    // My simplification:
    // I let long = int, long double = double

    public abstract class Expr : PTNode {

        public virtual AST.Expr GetExpr(AST.Env env) {
            throw new NotImplementedException();
        }

        [Obsolete]
        // TODO : [finished] Expression.GetExpression(env) -> (env, expr)
        public virtual Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
            return Tuple.Create(env, GetExpr(env));
        }

        public delegate TValue ConstOperation<TValue>(TValue lhs, TValue rhs);

        public delegate Int32 ConstLogialOperation<TValue>(TValue lhs, TValue rhs);

        public delegate TRet BinExprConstructor<TRet>(AST.Expr lhs, AST.Expr rhs, AST.ExprType type);

        public delegate AST.Expr UnaryExprConstructor(AST.Expr expr);

		public static Tuple<AST.Env, AST.Expr> GetIntegralBinOpExprEnv<TRet>(
			AST.Env env,
			AST.Expr lhs,
			AST.Expr rhs,
			ConstOperation<UInt32> uint32_op,
			ConstOperation<Int32> int32_op,
			BinExprConstructor<TRet> construct
		) where TRet : AST.Expr {

			Tuple<AST.Expr, AST.Expr, AST.ExprType.Kind> r_cast = AST.TypeCast.UsualArithmeticConversion(lhs, rhs);
			lhs = r_cast.Item1;
			rhs = r_cast.Item2;

			Boolean c1 = lhs.type.is_const;
			Boolean c2 = rhs.type.is_const;
			Boolean v1 = lhs.type.is_volatile;
			Boolean v2 = rhs.type.is_volatile;
			Boolean is_const = c1 || c2;
			Boolean is_volatile = v1 || v2;

			AST.ExprType.Kind enum_type = r_cast.Item3;

			AST.Expr expr;
			if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
				switch (enum_type) {
				case AST.ExprType.Kind.ULONG:
					expr = new AST.ConstULong(uint32_op(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value));
					break;
				case AST.ExprType.Kind.LONG:
					expr = new AST.ConstLong(int32_op(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value));
					break;
				default:
					Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
					return null;
				}

			} else {
				switch (enum_type) {
				case AST.ExprType.Kind.ULONG:
					expr = construct(lhs, rhs, new AST.TULong(is_const, is_volatile));
					break;
				case AST.ExprType.Kind.LONG:
					expr = construct(lhs, rhs, new AST.TULong(is_const, is_volatile));
					break;
				default:
					Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
					return null;
				}
			}

			return new Tuple<AST.Env, AST.Expr>(env, expr);
		}

        [Obsolete]
        public static Tuple<AST.Env, AST.Expr> GetIntegralBinOpExprEnv<TRet>(
            AST.Env env,
            Expr expr_lhs,
            Expr expr_rhs,
            ConstOperation<UInt32> uint32_op,
            ConstOperation<Int32> int32_op,
            BinExprConstructor<TRet> construct
        ) where TRet : AST.Expr {

			AST.Expr lhs;
			AST.Expr rhs;

            Tuple<AST.Env, AST.Expr> r_lhs = expr_lhs.GetExprEnv(env);
            env = r_lhs.Item1;
            lhs = r_lhs.Item2;

            Tuple<AST.Env, AST.Expr> r_rhs = expr_rhs.GetExprEnv(env);
            env = r_rhs.Item1;
            rhs = r_rhs.Item2;

			return GetIntegralBinOpExprEnv(
				env,
				lhs,
				rhs,
				uint32_op,
				int32_op,
				construct
			);

        }

        public static AST.Expr GetIntegralBinaryOperator<TRet>(
            AST.Env env,
            AST.Expr lhs,
            AST.Expr rhs,
            ConstOperation<UInt32> OperateUInt32,
            ConstOperation<Int32> OperateInt32,
            BinExprConstructor<TRet> ConstructExpr
        ) where TRet : AST.Expr {
            Tuple<AST.Expr, AST.Expr, AST.ExprType.Kind> r_cast = AST.TypeCast.UsualArithmeticConversion(lhs, rhs);
            lhs = r_cast.Item1;
            rhs = r_cast.Item2;
            AST.ExprType.Kind kind = r_cast.Item3;

            Boolean is_const = lhs.type.is_const || rhs.type.is_const;
            Boolean is_volatile = lhs.type.is_volatile || rhs.type.is_volatile;

            if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
                switch (kind) {
                    case AST.ExprType.Kind.ULONG:
                        return new AST.ConstULong(OperateUInt32(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value));
                    case AST.ExprType.Kind.LONG:
                        return new AST.ConstLong(OperateInt32(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value));
                    default:
                        throw new InvalidOperationException("Expected long or unsigned long.");
                }
            }

            switch (kind) {
                case AST.ExprType.Kind.ULONG:
                    return ConstructExpr(lhs, rhs, new AST.TULong(is_const, is_volatile));
                case AST.ExprType.Kind.LONG:
                    return ConstructExpr(lhs, rhs, new AST.TULong(is_const, is_volatile));
                default:
                    throw new InvalidOperationException("Expected long or unsigned long.");
            }
        }

        /// <summary>
        /// Gets an arithmetic binary operation expression
        /// from two **semanted** expressions.
        /// </summary>
        public static Tuple<AST.Env, AST.Expr> GetArithmeticBinOpExpr<TRet>(
			AST.Env env,
			AST.Expr lhs,
			AST.Expr rhs,
			ConstOperation<Double> double_op,
			ConstOperation<Single> float_op,
			ConstOperation<UInt32> uint32_op,
			ConstOperation<Int32>  int32_op,
			BinExprConstructor<TRet> construct
		) where TRet : AST.Expr {

			Tuple<AST.Expr, AST.Expr, AST.ExprType.Kind> r_cast = AST.TypeCast.UsualArithmeticConversion(lhs, rhs);
			lhs = r_cast.Item1;
			rhs = r_cast.Item2;

			Boolean c1 = lhs.type.is_const;
			Boolean c2 = rhs.type.is_const;
			Boolean v1 = lhs.type.is_volatile;
			Boolean v2 = rhs.type.is_volatile;
			Boolean is_const = c1 || c2;
			Boolean is_volatile = v1 || v2;

			AST.ExprType.Kind enum_type = r_cast.Item3;

			AST.Expr expr;
			if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
				switch (enum_type) {
				case AST.ExprType.Kind.DOUBLE:
					expr = new AST.ConstDouble(double_op(((AST.ConstDouble)lhs).value, ((AST.ConstDouble)rhs).value));
					break;
				case AST.ExprType.Kind.FLOAT:
					expr = new AST.ConstFloat(float_op(((AST.ConstFloat)lhs).value, ((AST.ConstFloat)rhs).value));
					break;
				case AST.ExprType.Kind.ULONG:
					expr = new AST.ConstULong(uint32_op(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value));
					break;
				case AST.ExprType.Kind.LONG:
					expr = new AST.ConstLong(int32_op(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value));
					break;
				default:
					throw new InvalidOperationException("Error: usual arithmetic conversion returns invalid type.");
				}

			} else {
				switch (enum_type) {
				case AST.ExprType.Kind.DOUBLE:
					expr = construct(lhs, rhs, new AST.TDouble(is_const, is_volatile));
					break;
				case AST.ExprType.Kind.FLOAT:
					expr = construct(lhs, rhs, new AST.TFloat(is_const, is_volatile));
					break;
				case AST.ExprType.Kind.ULONG:
					expr = construct(lhs, rhs, new AST.TULong(is_const, is_volatile));
					break;
				case AST.ExprType.Kind.LONG:
					expr = construct(lhs, rhs, new AST.TLong(is_const, is_volatile));
					break;
				default:
					throw new InvalidOperationException("Error: usual arithmetic conversion returns invalid type.");
				}
			}

			return new Tuple<AST.Env, AST.Expr>(env, expr);
		}


		/// <summary>
		/// Gets an arithmetic binary operation expression
		/// from two **unsemanted** expressions.
		/// </summary>
		public static Tuple<AST.Env, AST.Expr> GetArithmeticBinOpExpr<TRet>(
			AST.Env env,
			Expr expr_lhs,
			Expr expr_rhs,
			ConstOperation<Double> double_op,
			ConstOperation<Single> float_op,
			ConstOperation<UInt32> uint32_op,
			ConstOperation<Int32>  int32_op,
			BinExprConstructor<TRet> construct
		) where TRet : AST.Expr {

			AST.Expr lhs;
			AST.Expr rhs;

			Tuple<AST.Env, AST.Expr> r_lhs = expr_lhs.GetExprEnv(env);
			env = r_lhs.Item1;
			lhs = r_lhs.Item2;

			Tuple<AST.Env, AST.Expr> r_rhs = expr_rhs.GetExprEnv(env);
			env = r_rhs.Item1;
			rhs = r_rhs.Item2;

			return GetArithmeticBinOpExpr(
				env,
				lhs,
				rhs,
				double_op,
				float_op,
				uint32_op,
				int32_op,
				construct
			);
		}

        public static Tuple<AST.Env, AST.Expr> GetScalarBinLogicalOpExpr<TRet>(
            AST.Env env,
            Expr expr_lhs,
            Expr expr_rhs,
            ConstLogialOperation<Double> double_op,
            ConstLogialOperation<Single> float_op,
            ConstLogialOperation<UInt32> uint32_op,
            ConstLogialOperation<Int32> int32_op,
            BinExprConstructor<TRet> construct
        ) where TRet : AST.Expr {

            Tuple<AST.Env, AST.Expr> r_lhs = expr_lhs.GetExprEnv(env);
            env = r_lhs.Item1;
            AST.Expr lhs = r_lhs.Item2;

            Tuple<AST.Env, AST.Expr> r_rhs = expr_rhs.GetExprEnv(env);
            env = r_rhs.Item1;
            AST.Expr rhs = r_rhs.Item2;

            Tuple<AST.Expr, AST.Expr, AST.ExprType.Kind> r_cast = AST.TypeCast.UsualScalarConversion(lhs, rhs);

            lhs = r_cast.Item1;
            rhs = r_cast.Item2;
            Boolean c1 = lhs.type.is_const;
            Boolean c2 = rhs.type.is_const;
            Boolean v1 = lhs.type.is_volatile;
            Boolean v2 = rhs.type.is_volatile;
            Boolean is_const = c1 || c2;
            Boolean is_volatile = v1 || v2;

            AST.ExprType.Kind enum_type = r_cast.Item3;

            AST.Expr expr;
            if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
                switch (enum_type) {
                case AST.ExprType.Kind.DOUBLE:
                    expr = new AST.ConstLong(double_op(((AST.ConstDouble)lhs).value, ((AST.ConstDouble)rhs).value));
                    break;
                case AST.ExprType.Kind.FLOAT:
                    expr = new AST.ConstLong(float_op(((AST.ConstFloat)lhs).value, ((AST.ConstFloat)rhs).value));
                    break;
                case AST.ExprType.Kind.ULONG:
                    expr = new AST.ConstLong(uint32_op(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value));
                    break;
                case AST.ExprType.Kind.LONG:
                    expr = new AST.ConstLong(int32_op(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value));
                    break;
                default:
                    Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
                    return null;
                }

            } else {
                switch (enum_type) {
                case AST.ExprType.Kind.DOUBLE:
                case AST.ExprType.Kind.FLOAT:
                case AST.ExprType.Kind.ULONG:
                case AST.ExprType.Kind.LONG:
                    expr = construct(lhs, rhs, new AST.TLong(is_const, is_volatile));
                    break;
                default:
                    Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
                    return null;
                }
            }

            return new Tuple<AST.Env, AST.Expr>(env, expr);
        }

		public delegate Tuple<AST.Env, AST.Expr> BinOpExprMaker(AST.Env env, AST.Expr lvalue, AST.Expr rvalue);

		public static Tuple<AST.Env, AST.Expr> GetBinaryAssignOperation(
			AST.Env env,
			AST.Expr lvalue,
			AST.Expr rvalue,
			BinOpExprMaker Maker
		) {
			AST.Expr ans;

			Tuple<AST.Env, AST.Expr> r_ans = Maker(env, lvalue, rvalue);
			env = r_ans.Item1;
			ans = r_ans.Item2;

			ans = AST.TypeCast.MakeCast(ans, lvalue.type);

			return new Tuple<AST.Env, AST.Expr>(
				env,
				new AST.Assignment(
					lvalue,
					ans,
					lvalue.type
				)
			);
		}

		public static Tuple<AST.Env, AST.Expr> GetBinaryOpEnv(
			AST.Env env,
			Expr expr_lvalue,
			Expr expr_rvalue,
			BinOpExprMaker Maker
		) {
			AST.Expr lvalue;
			AST.Expr rvalue;

			Tuple<AST.Env, AST.Expr> r_lvalue = expr_lvalue.GetExprEnv(env);
			env = r_lvalue.Item1;
			lvalue = r_lvalue.Item2;

			Tuple<AST.Env, AST.Expr> r_rvalue = expr_rvalue.GetExprEnv(env);
			env = r_rvalue.Item1;
			rvalue = r_rvalue.Item2;

			return Maker(env, lvalue, rvalue);
		}

		public static Tuple<AST.Env, AST.Expr> GetBinaryAssignOperation(
			AST.Env env,
			Expr expr_lvalue,
			Expr expr_rvalue,
			BinOpExprMaker Maker
		) {
			AST.Expr lhs;
			AST.Expr rhs;

			Tuple<AST.Env, AST.Expr> r_lvalue = expr_lvalue.GetExprEnv(env);
			env = r_lvalue.Item1;
			lhs = r_lvalue.Item2;

			Tuple<AST.Env, AST.Expr> r_rvalue = expr_rvalue.GetExprEnv(env);
			env = r_rvalue.Item1;
			rhs = r_rvalue.Item2;

			return GetBinaryAssignOperation(
				env,
				lhs,
				rhs,
				Maker
			);
		}
        
		public static Tuple<AST.Env, AST.Expr> GetUnaryOpExpr(
            AST.Env env,
            Expr expr,
            Dictionary<AST.ExprType.Kind, UnaryExprConstructor> constructors,
            Dictionary<AST.ExprType.Kind, UnaryExprConstructor> const_constructors
        ) {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// An empty expression
    /// used in [], and empty initialization
    /// </summary>
    public class EmptyExpr : Expr {
        public EmptyExpr() { }
        public override AST.Expr GetExpr(AST.Env env) {
            return new AST.EmptyExpr();
        }
    }

    /// <summary>
    /// Only a name
    /// </summary>
    public class Variable : Expr {
        public Variable(string name) {
            this.name = name;
        }
		public readonly string name;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Env.Entry entry = env.Find(name);
            
            if (entry == null) {
                throw new InvalidOperationException($"Cannot find variable '{name}'");
            }

            switch (entry.entry_loc) {
                case AST.Env.EntryLoc.NOT_FOUND:
                    throw new InvalidOperationException($"Cannot find variable '{name}'");
                case AST.Env.EntryLoc.TYPEDEF:
                    throw new InvalidOperationException($"Expected a variable '{name}', not a typedef.");
                case AST.Env.EntryLoc.ENUM:
                    return new AST.ConstLong(entry.entry_offset);
                case AST.Env.EntryLoc.FRAME:
                case AST.Env.EntryLoc.GLOBAL:
                case AST.Env.EntryLoc.STACK:
                    return new AST.Variable(entry.entry_type, name);
                default:
                    throw new InvalidOperationException($"Cannot find variable '{name}'");
            }
        }
    }

    /// <summary>
    /// A list of assignment expressions.
    /// e.g.
    ///   a = 3, b = 4;
    /// </summary>
	public class AssignmentList : Expr {
		public AssignmentList(List<Expr> _exprs) {
			assign_exprs = _exprs;
		}
		public List<Expr> assign_exprs;

        public override AST.Expr GetExpr(AST.Env env) {
            List<AST.Expr> exprs = assign_exprs.ConvertAll(expr => expr.GetExpr(env));
            return new AST.AssignmentList(exprs, exprs.FindLast(_ => true).type);
        }

		//public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
		//	List<AST.Expr> exprs = new List<AST.Expr>();
		//	AST.ExprType type = new AST.TVoid();
		//	foreach (Expr expr in assign_exprs) {
		//		Tuple<AST.Env, AST.Expr> r_expr = expr.GetExprEnv(env);
		//		env = r_expr.Item1;
		//		type = r_expr.Item2.type;
		//		exprs.Add(r_expr.Item2);
		//	}
		//	return new Tuple<AST.Env, AST.Expr>(env, new AST.AssignmentList(exprs, type));
		//}
	}

	/// <summary>
	/// Conditional Expression
	/// 
	/// cond ? true_expr : false_expr
	/// 
	/// cond must be of scalar type
	/// 
	/// 1. if both true_expr and false_expr have arithmetic types
	///    perform usual arithmetic conversion
	/// 2. 
	/// </summary>
	// TODO : What if const???
    public class ConditionalExpression : Expr {
        public ConditionalExpression(Expr _cond, Expr _true_expr, Expr _false_expr) {
            cond_cond = _cond;
            cond_true_expr = _true_expr;
            cond_false_expr = _false_expr;
        }
        public readonly Expr cond_cond;
        public readonly Expr cond_true_expr;
        public readonly Expr cond_false_expr;
        
        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr cond = cond_cond.GetExpr(env);

            if (!cond.type.IsScalar()) {
                throw new InvalidOperationException("Expected a scalar condition in conditional expression.");
            }

            AST.Expr true_expr = cond_true_expr.GetExpr(env);
            AST.Expr false_expr = cond_false_expr.GetExpr(env);

            // 1. if both true_expr and false_Expr have arithmetic types:
            //    perform usual arithmetic conversion
            if (true_expr.type.IsArith() && false_expr.type.IsArith()) {
                var r_cast = AST.TypeCast.UsualArithmeticConversion(true_expr, false_expr);
                true_expr = r_cast.Item1;
                false_expr = r_cast.Item2;
                return new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type);
            }
            
            if (true_expr.type.kind != false_expr.type.kind) {
                throw new InvalidOperationException("Operand types not match in conditional expression.");
            }

            switch (true_expr.type.kind) {
                // 2. if both true_expr and false_expr have struct or union type
                //    make sure they are compatible
                case AST.ExprType.Kind.STRUCT:
                    if (!true_expr.type.EqualType(false_expr.type)) {
                        throw new InvalidOperationException("Expected compatible struct types in conditional expression.");
                    }
                    return new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type);

                case AST.ExprType.Kind.UNION:
                    if (!true_expr.type.EqualType(false_expr.type)) {
                        throw new InvalidOperationException("Expected compatible union types in conditional expression.");
                    }
                    return new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type);

                // 3. if both true_expr and false_expr have void type
                //    return void
                case AST.ExprType.Kind.VOID:
                    return new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type);

                // 4. if both true_expr and false_expr have pointer type
                case AST.ExprType.Kind.POINTER:

                    // if either points to void, convert to void *
                    if (((AST.TPointer)true_expr.type).referenced_type.kind == AST.ExprType.Kind.VOID
                        || ((AST.TPointer)false_expr.type).referenced_type.kind == AST.ExprType.Kind.VOID) {
                        return new AST.ConditionalExpr(cond, true_expr, false_expr, new AST.TPointer(new AST.TVoid()));
                    }

                    throw new NotImplementedException("More comparisons here.");

                default:
                    throw new InvalidOperationException("Expected compatible types in conditional expression.");
            }
        }

		//public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
		//	AST.Expr cond;
		//	AST.Expr true_expr;
		//	AST.Expr false_expr;

		//	Tuple<AST.Env, AST.Expr> r_cond = cond_cond.GetExprEnv(env);
		//	env = r_cond.Item1;
		//	cond = r_cond.Item2;

		//	if (!cond.type.IsScalar()) {
		//		throw new InvalidOperationException("Error: expected a scalar");
		//	}

		//	Tuple<AST.Env, AST.Expr> r_true_expr = cond_true_expr.GetExprEnv(env);
		//	env = r_true_expr.Item1;
		//	true_expr = r_true_expr.Item2;

		//	Tuple<AST.Env, AST.Expr> r_false_expr = cond_false_expr.GetExprEnv(env);
		//	env = r_false_expr.Item1;
		//	false_expr = r_false_expr.Item2;

		//	// 1. if both true_expr and false_expr have arithmetic types
		//	//    perform usual arithmetic conversion
		//	if (true_expr.type.IsArith() && false_expr.type.IsArith()) {
		//		Tuple<AST.Expr, AST.Expr, AST.ExprType.Kind> r_cast = AST.TypeCast.UsualArithmeticConversion(true_expr, false_expr);
		//		true_expr = r_cast.Item1;
		//		false_expr = r_cast.Item2;
		//		return new Tuple<AST.Env, AST.Expr>(env, new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type));
		//	}


		//	if (true_expr.type.type_kind == false_expr.type.type_kind) {
		//		switch (true_expr.type.type_kind) {

		//		// 2. if both true_expr and false_expr have struct or union type
		//		//    make sure they are compatible
		//		case AST.ExprType.Kind.STRUCT:
		//			if (!true_expr.type.EqualType(false_expr.type)) {
		//				throw new InvalidOperationException("Error: expected same struct");
		//			}
		//			return new Tuple<AST.Env, AST.Expr>(env, new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type));
				
		//		case AST.ExprType.Kind.UNION:
		//			if (!true_expr.type.EqualType(false_expr.type)) {
		//				throw new InvalidOperationException("Error: expected same union");
		//			}
		//			return new Tuple<AST.Env, AST.Expr>(env, new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type));
				
		//		// 3. if both true_expr and false_expr have void type
		//		//    return void
		//		case AST.ExprType.Kind.VOID:
		//			return new Tuple<AST.Env, AST.Expr>(env, new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type));

		//		// 4. if both true_expr and false_expr have pointer type
		//		case AST.ExprType.Kind.POINTER:

		//			// if either points to void, convert to void *
		//			if (((AST.TPointer)true_expr.type).referenced_type.type_kind == AST.ExprType.Kind.VOID
		//			    || ((AST.TPointer)false_expr.type).referenced_type.type_kind == AST.ExprType.Kind.VOID) {
		//				return new Tuple<AST.Env, AST.Expr>(env, new AST.ConditionalExpr(cond, true_expr, false_expr, new AST.TPointer(new AST.TVoid())));
		//			}

		//			break;

		//		default:
		//			break;
		//		}
		//	}

		//	throw new InvalidOperationException("Error: invalid types");
		//}
    }

    public class FunctionCall : Expr {
        public FunctionCall(Expr func, IEnumerable<Expr> args) {
            this.func = func;
            this.args = args;
        }
        public readonly Expr func;
        public readonly IEnumerable<Expr> args;

        public override AST.Expr GetExpr(AST.Env env) {
            AST.Expr func = this.func.GetExpr(env);

            if (func.type.kind != AST.ExprType.Kind.FUNCTION) {
                throw new InvalidOperationException("Expected a function in function call.");
            }

            AST.TFunction func_type = (AST.TFunction)(func.type);

            var args = this.args.Select(_ => _.GetExpr(env)).ToList();

            if (args.Count() != func_type.args.Count) {
                throw new InvalidOperationException("Number of arguments mismatch.");
            }

            // make implicit cast
            args = Enumerable.Zip(args, func_type.args, (arg, entry) => AST.TypeCast.MakeCast(arg, entry.entry_type)).ToList();

            return new AST.FunctionCall(func, func_type, args, func_type.ret_type);
        }

        [Obsolete]
        public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_func = this.func.GetExprEnv(env);
            env = r_func.Item1;
            AST.Expr func = r_func.Item2;

            if (func.type.kind != AST.ExprType.Kind.FUNCTION) {
                throw new Exception("Error: calling a non-function.");
            }

            AST.TFunction func_type = (AST.TFunction)(func.type);

            List<AST.Expr> args = new List<AST.Expr>();
            foreach (Expr expr in this.args) {
                Tuple<AST.Env, AST.Expr> r_expr = expr.GetExprEnv(env);
                env = r_expr.Item1;
                args.Add(r_expr.Item2);
            }

            if (func_type.args.Count != args.Count) {
                throw new Exception("Error: number of arguments mismatch.");
            }

            for (Int32 iarg = 0; iarg < args.Count; ++iarg) {
                args[iarg] = AST.TypeCast.MakeCast(args[iarg], func_type.args[iarg].entry_type);
            }

            return new Tuple<AST.Env, AST.Expr>(env, new AST.FunctionCall(func, func_type, args, func_type.ret_type));

        }

    }

    public class Attribute : Expr {
        public Attribute(Expr _expr, Variable _attrib) {
            attrib_expr = _expr;
            attrib_attrib = _attrib;
        }
        public readonly Expr attrib_expr;
        public readonly Variable attrib_attrib;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
			AST.Expr expr;
			string attrib;

			Tuple<AST.Env, AST.Expr> r_expr = attrib_expr.GetExprEnv(env);
			env = r_expr.Item1;
			expr = r_expr.Item2;
			attrib = attrib_attrib.name;

			switch (expr.type.kind) {
			case AST.ExprType.Kind.STRUCT:
				AST.TStruct struct_type = (AST.TStruct)expr.type;

				AST.Utils.StoreEntry r_struct_find = struct_type.attribs.Find(entry => entry.entry_name == attrib);
				if (r_struct_find == null) {
					throw new InvalidOperationException("Error: cannot find attribute " + attrib);
				}
				return new Tuple<AST.Env, AST.Expr>(env, new AST.Attribute(expr, attrib, r_struct_find.entry_type));
			
			case AST.ExprType.Kind.UNION:
				AST.TUnion union_type = (AST.TUnion)expr.type;

				Tuple<string, AST.ExprType> r_union_find = union_type.attribs.Find(entry => entry.Item1 == attrib);
				if (r_union_find == null) {
					throw new InvalidOperationException("Error: cannot find attribute " + attrib);
				}
				return new Tuple<AST.Env, AST.Expr>(env, new AST.Attribute(expr, attrib, r_union_find.Item2));

			default:
				throw new InvalidOperationException("Error: expected a struct or union");
			}


		}
    }


}