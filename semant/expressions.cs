using System;
using System.Collections.Generic;

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

    public class Expression : PTNode {
        // TODO : [finished] Expression.GetExpression(env) -> (env, expr)
        public virtual Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            throw new NotImplementedException();
        }

        public delegate TValue ConstOperation<TValue>(TValue lhs, TValue rhs);

        public delegate Int32 ConstLogialOperation<TValue>(TValue lhs, TValue rhs);

        public delegate TRet BinExprConstructor<TRet>(AST.Expr lhs, AST.Expr rhs, AST.ExprType type);

        public delegate AST.Expr UnaryExprConstructor(AST.Expr expr);

		public static Tuple<AST.Env, AST.Expr> GetIntegralBinOpExpr<TRet>(
			AST.Env env,
			AST.Expr lhs,
			AST.Expr rhs,
			ConstOperation<UInt32> uint32_op,
			ConstOperation<Int32> int32_op,
			BinExprConstructor<TRet> construct
		) where TRet : AST.Expr {

			Tuple<AST.Expr, AST.Expr, AST.ExprType.EnumExprType> r_cast = AST.TypeCast.UsualArithmeticConversion(lhs, rhs);
			lhs = r_cast.Item1;
			rhs = r_cast.Item2;

			Boolean c1 = lhs.type.is_const;
			Boolean c2 = rhs.type.is_const;
			Boolean v1 = lhs.type.is_volatile;
			Boolean v2 = rhs.type.is_volatile;
			Boolean is_const = c1 || c2;
			Boolean is_volatile = v1 || v2;

			AST.ExprType.EnumExprType enum_type = r_cast.Item3;

			AST.Expr expr;
			if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
				switch (enum_type) {
				case AST.ExprType.EnumExprType.ULONG:
					expr = new AST.ConstULong(uint32_op(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value));
					break;
				case AST.ExprType.EnumExprType.LONG:
					expr = new AST.ConstLong(int32_op(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value));
					break;
				default:
					Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
					return null;
				}

			} else {
				switch (enum_type) {
				case AST.ExprType.EnumExprType.ULONG:
					expr = construct(lhs, rhs, new AST.TULong(is_const, is_volatile));
					break;
				case AST.ExprType.EnumExprType.LONG:
					expr = construct(lhs, rhs, new AST.TULong(is_const, is_volatile));
					break;
				default:
					Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
					return null;
				}
			}

			return new Tuple<AST.Env, AST.Expr>(env, expr);
		}

        public static Tuple<AST.Env, AST.Expr> GetIntegralBinOpExpr<TRet>(
            AST.Env env,
            Expression expr_lhs,
            Expression expr_rhs,
            ConstOperation<UInt32> uint32_op,
            ConstOperation<Int32> int32_op,
            BinExprConstructor<TRet> construct
        ) where TRet : AST.Expr {

			AST.Expr lhs;
			AST.Expr rhs;

            Tuple<AST.Env, AST.Expr> r_lhs = expr_lhs.GetExpr(env);
            env = r_lhs.Item1;
            lhs = r_lhs.Item2;

            Tuple<AST.Env, AST.Expr> r_rhs = expr_rhs.GetExpr(env);
            env = r_rhs.Item1;
            rhs = r_rhs.Item2;

			return GetIntegralBinOpExpr(
				env,
				lhs,
				rhs,
				uint32_op,
				int32_op,
				construct
			);

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

			Tuple<AST.Expr, AST.Expr, AST.ExprType.EnumExprType> r_cast = AST.TypeCast.UsualArithmeticConversion(lhs, rhs);
			lhs = r_cast.Item1;
			rhs = r_cast.Item2;

			Boolean c1 = lhs.type.is_const;
			Boolean c2 = rhs.type.is_const;
			Boolean v1 = lhs.type.is_volatile;
			Boolean v2 = rhs.type.is_volatile;
			Boolean is_const = c1 || c2;
			Boolean is_volatile = v1 || v2;

			AST.ExprType.EnumExprType enum_type = r_cast.Item3;

			AST.Expr expr;
			if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
				switch (enum_type) {
				case AST.ExprType.EnumExprType.DOUBLE:
					expr = new AST.ConstDouble(double_op(((AST.ConstDouble)lhs).value, ((AST.ConstDouble)rhs).value));
					break;
				case AST.ExprType.EnumExprType.FLOAT:
					expr = new AST.ConstFloat(float_op(((AST.ConstFloat)lhs).value, ((AST.ConstFloat)rhs).value));
					break;
				case AST.ExprType.EnumExprType.ULONG:
					expr = new AST.ConstULong(uint32_op(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value));
					break;
				case AST.ExprType.EnumExprType.LONG:
					expr = new AST.ConstLong(int32_op(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value));
					break;
				default:
					throw new InvalidOperationException("Error: usual arithmetic conversion returns invalid type.");
				}

			} else {
				switch (enum_type) {
				case AST.ExprType.EnumExprType.DOUBLE:
					expr = construct(lhs, rhs, new AST.TDouble(is_const, is_volatile));
					break;
				case AST.ExprType.EnumExprType.FLOAT:
					expr = construct(lhs, rhs, new AST.TFloat(is_const, is_volatile));
					break;
				case AST.ExprType.EnumExprType.ULONG:
					expr = construct(lhs, rhs, new AST.TULong(is_const, is_volatile));
					break;
				case AST.ExprType.EnumExprType.LONG:
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
			Expression expr_lhs,
			Expression expr_rhs,
			ConstOperation<Double> double_op,
			ConstOperation<Single> float_op,
			ConstOperation<UInt32> uint32_op,
			ConstOperation<Int32>  int32_op,
			BinExprConstructor<TRet> construct
		) where TRet : AST.Expr {

			AST.Expr lhs;
			AST.Expr rhs;

			Tuple<AST.Env, AST.Expr> r_lhs = expr_lhs.GetExpr(env);
			env = r_lhs.Item1;
			lhs = r_lhs.Item2;

			Tuple<AST.Env, AST.Expr> r_rhs = expr_rhs.GetExpr(env);
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
            Expression expr_lhs,
            Expression expr_rhs,
            ConstLogialOperation<Double> double_op,
            ConstLogialOperation<Single> float_op,
            ConstLogialOperation<UInt32> uint32_op,
            ConstLogialOperation<Int32> int32_op,
            BinExprConstructor<TRet> construct
        ) where TRet : AST.Expr {

            Tuple<AST.Env, AST.Expr> r_lhs = expr_lhs.GetExpr(env);
            env = r_lhs.Item1;
            AST.Expr lhs = r_lhs.Item2;

            Tuple<AST.Env, AST.Expr> r_rhs = expr_rhs.GetExpr(env);
            env = r_rhs.Item1;
            AST.Expr rhs = r_rhs.Item2;

            Tuple<AST.Expr, AST.Expr, AST.ExprType.EnumExprType> r_cast = AST.TypeCast.UsualScalarConversion(lhs, rhs);

            lhs = r_cast.Item1;
            rhs = r_cast.Item2;
            Boolean c1 = lhs.type.is_const;
            Boolean c2 = rhs.type.is_const;
            Boolean v1 = lhs.type.is_volatile;
            Boolean v2 = rhs.type.is_volatile;
            Boolean is_const = c1 || c2;
            Boolean is_volatile = v1 || v2;

            AST.ExprType.EnumExprType enum_type = r_cast.Item3;

            AST.Expr expr;
            if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
                switch (enum_type) {
                case AST.ExprType.EnumExprType.DOUBLE:
                    expr = new AST.ConstLong(double_op(((AST.ConstDouble)lhs).value, ((AST.ConstDouble)rhs).value));
                    break;
                case AST.ExprType.EnumExprType.FLOAT:
                    expr = new AST.ConstLong(float_op(((AST.ConstFloat)lhs).value, ((AST.ConstFloat)rhs).value));
                    break;
                case AST.ExprType.EnumExprType.ULONG:
                    expr = new AST.ConstLong(uint32_op(((AST.ConstULong)lhs).value, ((AST.ConstULong)rhs).value));
                    break;
                case AST.ExprType.EnumExprType.LONG:
                    expr = new AST.ConstLong(int32_op(((AST.ConstLong)lhs).value, ((AST.ConstLong)rhs).value));
                    break;
                default:
                    Log.SemantError("Error: usual arithmetic conversion returns invalid type.");
                    return null;
                }

            } else {
                switch (enum_type) {
                case AST.ExprType.EnumExprType.DOUBLE:
                case AST.ExprType.EnumExprType.FLOAT:
                case AST.ExprType.EnumExprType.ULONG:
                case AST.ExprType.EnumExprType.LONG:
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

		public static Tuple<AST.Env, AST.Expr> GetBinaryOperation(
			AST.Env env,
			Expression expr_lvalue,
			Expression expr_rvalue,
			BinOpExprMaker Maker
		) {
			AST.Expr lvalue;
			AST.Expr rvalue;

			Tuple<AST.Env, AST.Expr> r_lvalue = expr_lvalue.GetExpr(env);
			env = r_lvalue.Item1;
			lvalue = r_lvalue.Item2;

			Tuple<AST.Env, AST.Expr> r_rvalue = expr_rvalue.GetExpr(env);
			env = r_rvalue.Item1;
			rvalue = r_rvalue.Item2;

			return Maker(env, lvalue, rvalue);
		}

		public static Tuple<AST.Env, AST.Expr> GetBinaryAssignOperation(
			AST.Env env,
			Expression expr_lvalue,
			Expression expr_rvalue,
			BinOpExprMaker Maker
		) {
			AST.Expr lhs;
			AST.Expr rhs;

			Tuple<AST.Env, AST.Expr> r_lvalue = expr_lvalue.GetExpr(env);
			env = r_lvalue.Item1;
			lhs = r_lvalue.Item2;

			Tuple<AST.Env, AST.Expr> r_rvalue = expr_rvalue.GetExpr(env);
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
            Expression expr,
            Dictionary<AST.ExprType.EnumExprType, UnaryExprConstructor> constructors,
            Dictionary<AST.ExprType.EnumExprType, UnaryExprConstructor> const_constructors
        ) {
            throw new NotImplementedException();
        }
    }


    public class EmptyExpression : Expression {
        public EmptyExpression() { }
        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            return new Tuple<AST.Env, AST.Expr>(env, new AST.EmptyExpr());
        }
    }

    public class Variable : Expression {
        public Variable(String _name) {
            var_name = _name;
        }
		public readonly String var_name;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            AST.Env.Entry entry = env.Find(var_name);
            if (entry == null) {
                Log.SemantError("Error: cannot find variable '" + var_name + "'");
                return null;
            }
            if (entry.entry_loc == AST.Env.EntryLoc.TYPEDEF) {
                Log.SemantError("Error: expected a variable, not a typedef.");
                return null;
            }
            if (entry.entry_loc == AST.Env.EntryLoc.ENUM) {
                return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstLong(entry.entry_offset));
            } else {
                return new Tuple<AST.Env, AST.Expr>(env, new AST.Variable(entry.entry_type, var_name));
            }
        }

        
    }

	public class AssignmentList : Expression {
		public AssignmentList(List<Expression> _exprs) {
			assign_exprs = _exprs;
		}
		public List<Expression> assign_exprs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			List<AST.Expr> exprs = new List<AST.Expr>();
			AST.ExprType type = new AST.TVoid();
			foreach (Expression expr in assign_exprs) {
				Tuple<AST.Env, AST.Expr> r_expr = expr.GetExpr(env);
				env = r_expr.Item1;
				type = r_expr.Item2.type;
				exprs.Add(r_expr.Item2);
			}
			return new Tuple<AST.Env, AST.Expr>(env, new AST.AssignmentList(exprs, type));
		}
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
    public class ConditionalExpression : Expression {
        public ConditionalExpression(Expression _cond, Expression _true_expr, Expression _false_expr) {
            cond_cond = _cond;
            cond_true_expr = _true_expr;
            cond_false_expr = _false_expr;
        }
        public readonly Expression cond_cond;
        public readonly Expression cond_true_expr;
        public readonly Expression cond_false_expr;
        
		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			AST.Expr cond;
			AST.Expr true_expr;
			AST.Expr false_expr;

			Tuple<AST.Env, AST.Expr> r_cond = cond_cond.GetExpr(env);
			env = r_cond.Item1;
			cond = r_cond.Item2;

			if (!cond.type.IsScalar()) {
				throw new InvalidOperationException("Error: expected a scalar");
			}

			Tuple<AST.Env, AST.Expr> r_true_expr = cond_true_expr.GetExpr(env);
			env = r_true_expr.Item1;
			true_expr = r_true_expr.Item2;

			Tuple<AST.Env, AST.Expr> r_false_expr = cond_false_expr.GetExpr(env);
			env = r_false_expr.Item1;
			false_expr = r_false_expr.Item2;

			// 1. if both true_expr and false_expr have arithmetic types
			//    perform usual arithmetic conversion
			if (true_expr.type.IsArith() && false_expr.type.IsArith()) {
				Tuple<AST.Expr, AST.Expr, AST.ExprType.EnumExprType> r_cast = AST.TypeCast.UsualArithmeticConversion(true_expr, false_expr);
				true_expr = r_cast.Item1;
				false_expr = r_cast.Item2;
				return new Tuple<AST.Env, AST.Expr>(env, new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type));
			}


			if (true_expr.type.expr_type == false_expr.type.expr_type) {
				switch (true_expr.type.expr_type) {

				// 2. if both true_expr and false_expr have struct or union type
				//    make sure they are compatible
				case AST.ExprType.EnumExprType.STRUCT:
					if (!true_expr.type.EqualType(false_expr.type)) {
						throw new InvalidOperationException("Error: expected same struct");
					}
					return new Tuple<AST.Env, AST.Expr>(env, new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type));
				
				case AST.ExprType.EnumExprType.UNION:
					if (!true_expr.type.EqualType(false_expr.type)) {
						throw new InvalidOperationException("Error: expected same union");
					}
					return new Tuple<AST.Env, AST.Expr>(env, new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type));
				
				// 3. if both true_expr and false_expr have void type
				//    return void
				case AST.ExprType.EnumExprType.VOID:
					return new Tuple<AST.Env, AST.Expr>(env, new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type));

				// 4. if both true_expr and false_expr have pointer type
				case AST.ExprType.EnumExprType.POINTER:

					// if either points to void, convert to void *
					if (((AST.TPointer)true_expr.type).referenced_type.expr_type == AST.ExprType.EnumExprType.VOID
					    || ((AST.TPointer)false_expr.type).referenced_type.expr_type == AST.ExprType.EnumExprType.VOID) {
						return new Tuple<AST.Env, AST.Expr>(env, new AST.ConditionalExpr(cond, true_expr, false_expr, new AST.TPointer(new AST.TVoid())));
					}

					break;

				default:
					break;
				}
			}

			throw new InvalidOperationException("Error: invalid types");
		}
    }

    public class FunctionCall : Expression {
        public FunctionCall(Expression _func, List<Expression> _args) {
            call_func = _func;
            call_args = _args;
        }
        public Expression call_func;
        public List<Expression> call_args;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_func = call_func.GetExpr(env);
            env = r_func.Item1;
            AST.Expr func = r_func.Item2;

            if (func.type.expr_type != AST.ExprType.EnumExprType.FUNCTION) {
                throw new Exception("Error: calling a non-function.");
            }

            AST.TFunction func_type = (AST.TFunction)(func.type);

            List<AST.Expr> args = new List<AST.Expr>();
            foreach (Expression expr in call_args) {
                Tuple<AST.Env, AST.Expr> r_expr = expr.GetExpr(env);
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

    public class Attribute : Expression {
        public Attribute(Expression _expr, Variable _attrib) {
            attrib_expr = _expr;
            attrib_attrib = _attrib;
        }
        public readonly Expression attrib_expr;
        public readonly Variable attrib_attrib;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			AST.Expr expr;
			String attrib;

			Tuple<AST.Env, AST.Expr> r_expr = attrib_expr.GetExpr(env);
			env = r_expr.Item1;
			expr = r_expr.Item2;
			attrib = attrib_attrib.var_name;

			switch (expr.type.expr_type) {
			case AST.ExprType.EnumExprType.STRUCT:
				AST.TStruct struct_type = (AST.TStruct)expr.type;

				AST.Utils.StoreEntry r_struct_find = struct_type.attribs.Find(entry => entry.entry_name == attrib);
				if (r_struct_find == null) {
					throw new InvalidOperationException("Error: cannot find attribute " + attrib);
				}
				return new Tuple<AST.Env, AST.Expr>(env, new AST.Attribute(expr, attrib, r_struct_find.entry_type));
			
			case AST.ExprType.EnumExprType.UNION:
				AST.TUnion union_type = (AST.TUnion)expr.type;

				Tuple<String, AST.ExprType> r_union_find = union_type.attribs.Find(entry => entry.Item1 == attrib);
				if (r_union_find == null) {
					throw new InvalidOperationException("Error: cannot find attribute " + attrib);
				}
				return new Tuple<AST.Env, AST.Expr>(env, new AST.Attribute(expr, attrib, r_union_find.Item2));

			default:
				throw new InvalidOperationException("Error: expected a struct or union");
			}


		}
    }


	/// <summary>
	/// Increment
	/// 
	/// x++
	/// </summary>
    public class Increment : Expression {
        public Increment(Expression _expr) {
            inc_expr = _expr;
        }
        public readonly Expression inc_expr;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			AST.Expr expr;

			Tuple<AST.Env, AST.Expr> r_expr = inc_expr.GetExpr(env);
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
    public class Decrement : Expression {
        public Decrement(Expression _expr) {
            dec_expr = _expr;
        }
        public readonly Expression dec_expr;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			AST.Expr expr;

			Tuple<AST.Env, AST.Expr> r_expr = dec_expr.GetExpr(env);
			env = r_expr.Item1;
			expr = r_expr.Item2;

			if (!expr.type.IsScalar()) {
				throw new InvalidOperationException("Error: expected a scalar");
			}

			return new Tuple<AST.Env, AST.Expr>(env, new AST.PostDecrement(expr, expr.type));
		}
    }



    public class SizeofType : Expression {
        public SizeofType(TypeName _type_name) {
            sizeof_typename = _type_name;
        }
        public readonly TypeName sizeof_typename;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			AST.ExprType type;

			Tuple<AST.Env, AST.ExprType> r_typename = sizeof_typename.GetExprType(env);
			env = r_typename.Item1;
			type = r_typename.Item2;

			return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstULong((UInt32)type.SizeOf));
		}
    }


    public class SizeofExpression : Expression {
        public SizeofExpression(Expression _expr) {
            sizeof_expr = _expr;
        }
        public readonly Expression sizeof_expr;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			AST.Expr expr;

			Tuple<AST.Env, AST.Expr> r_expr = sizeof_expr.GetExpr(env);
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
    public class PrefixIncrement : Expression {
        public PrefixIncrement(Expression _expr) {
            inc_expr = _expr;
        }
        public readonly Expression inc_expr;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			AST.Expr expr;

			Tuple<AST.Env, AST.Expr> r_expr = inc_expr.GetExpr(env);
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
    public class PrefixDecrement : Expression {
        public PrefixDecrement(Expression _expr) {
            dec_expr = _expr;
        }
        public readonly Expression dec_expr;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			AST.Expr expr;

			Tuple<AST.Env, AST.Expr> r_expr = dec_expr.GetExpr(env);
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
    public class Reference : Expression {
        public Reference(Expression _expr) {
            ref_expr = _expr;
        }
        public readonly Expression ref_expr;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = ref_expr.GetExpr(env);
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
    public class Dereference : Expression {
        public Dereference(Expression _expr) {
            deref_expr = _expr;
        }
        public readonly Expression deref_expr;

        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = deref_expr.GetExpr(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            if (expr.type.expr_type != AST.ExprType.EnumExprType.POINTER) {
                throw new Exception("Error: dereferencing a non-pointer");
            }

			AST.ExprType ref_type = ((AST.TPointer)expr.type).referenced_type;
			if (ref_type.expr_type == AST.ExprType.EnumExprType.INCOMPLETE_STRUCT) {
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


    // Positive
    // ========
    // requires arithmetic type
    // 
    public class Positive : Expression {
        public Positive(Expression _expr) {
            pos_expr = _expr;
        }
        public readonly Expression pos_expr;

        // TODO : [finished] Positive.GetExpr(env) -> (env, expr)
        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = pos_expr.GetExpr(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            if (!expr.type.IsArith()) {
                Log.SemantError("Error: negation expectes arithmetic type.");
                return null;
            }

            return new Tuple<AST.Env, AST.Expr>(env, expr);
        }

    }

    // Negative
    // ========
    // requires aritmetic type
    // 
    public class Negative : Expression {
        public Negative(Expression _expr) {
            neg_expr = _expr;
        }
        public readonly Expression neg_expr;

        // TODO : [finished] Negative.GetExpr(env) -> (env, expr)
        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = neg_expr.GetExpr(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            if (!expr.type.IsArith()) {
                Log.SemantError("Error: negation expectes arithmetic type.");
                return null;
            }

            if (expr.IsConstExpr()) {
                switch (expr.type.expr_type) {
                case AST.ExprType.EnumExprType.LONG:
                    AST.ConstLong long_expr = (AST.ConstLong)expr;
                    expr = new AST.ConstLong(-long_expr.value);
                    break;
                case AST.ExprType.EnumExprType.ULONG:
                    AST.ConstULong ulong_expr = (AST.ConstULong)expr;
                    expr = new AST.ConstLong(-(Int32)ulong_expr.value);
                    break;
                case AST.ExprType.EnumExprType.FLOAT:
                    AST.ConstFloat float_expr = (AST.ConstFloat)expr;
                    expr = new AST.ConstFloat(-float_expr.value);
                    break;
                case AST.ExprType.EnumExprType.DOUBLE:
                    AST.ConstDouble double_expr = (AST.ConstDouble)expr;
                    expr = new AST.ConstDouble(-double_expr.value);
                    break;
                default:
                    Log.SemantError("Error: wrong constant type?");
                    break;
                }
            } else {
                expr = new AST.Negative(expr, expr.type);
            }

            return new Tuple<AST.Env, AST.Expr>(env, expr);
        }

    }

    // BitwiseNot
    // ==========
    // requires integral type
    // 
    public class BitwiseNot : Expression {
        public BitwiseNot(Expression _expr) {
            not_expr = _expr;
        }
        public readonly Expression not_expr;

        // TODO : [finished] BitwiseNot.GetExpr(env) -> (env, expr)
        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = not_expr.GetExpr(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            if (!expr.type.IsIntegral()) {
                Log.SemantError("Error: operator '~' expectes integral type.");
                return null;
            }

            if (expr.IsConstExpr()) {
                switch (expr.type.expr_type) {
                case AST.ExprType.EnumExprType.LONG:
                    AST.ConstLong long_expr = (AST.ConstLong)expr;
                    expr = new AST.ConstLong(~long_expr.value);
                    break;
                case AST.ExprType.EnumExprType.ULONG:
                    AST.ConstULong ulong_expr = (AST.ConstULong)expr;
                    expr = new AST.ConstULong(~ulong_expr.value);
                    break;
                default:
                    Log.SemantError("Error: wrong constant type?");
                    break;
                }
            } else {
                expr = new AST.BitwiseNot(expr, expr.type);
            }

            return new Tuple<AST.Env, AST.Expr>(env, expr);
        }

    }

    // Not
    // ===
    // requires scalar type
    // 
    public class Not : Expression {
        public Not(Expression _expr) {
            not_expr = _expr;
        }
        public readonly Expression not_expr;

        // TODO : [finished] Not.GetExpr(env) -> (env, expr(type=long))
        public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
            Tuple<AST.Env, AST.Expr> r_expr = not_expr.GetExpr(env);
            env = r_expr.Item1;
            AST.Expr expr = r_expr.Item2;

            if (!expr.type.IsArith()) {
                Log.SemantError("Error: operator '!' expectes arithmetic type.");
                return null;
            }

            if (expr.IsConstExpr()) {
                Boolean value = false;
                switch (expr.type.expr_type) {
                case AST.ExprType.EnumExprType.LONG:
                    AST.ConstLong long_expr = (AST.ConstLong)expr;
                    value = long_expr.value != 0;
                    break;
                case AST.ExprType.EnumExprType.ULONG:
                    AST.ConstULong ulong_expr = (AST.ConstULong)expr;
                    value = ulong_expr.value != 0;
                    break;
                case AST.ExprType.EnumExprType.FLOAT:
                    AST.ConstFloat float_expr = (AST.ConstFloat)expr;
                    value = float_expr.value != 0;
                    break;
                case AST.ExprType.EnumExprType.DOUBLE:
                    AST.ConstDouble double_expr = (AST.ConstDouble)expr;
                    value = double_expr.value != 0;
                    break;
                default:
                    Log.SemantError("Error: wrong constant type?");
                    break;
                }
                if (value) {
                    expr = new AST.ConstLong(1);
                } else {
                    expr = new AST.ConstLong(0);
                }
            } else {
                expr = new AST.LogicalNot(expr, new AST.TLong(expr.type.is_const, expr.type.is_volatile));
            }

            return new Tuple<AST.Env, AST.Expr>(env, expr);
        }

    }


	/// <summary>
	/// Type Cast
	/// 
	/// The user specifies the type.
	/// </summary>
    public class TypeCast : Expression {
        public TypeCast(TypeName _type_name, Expression _expr) {
            cast_typename = _type_name;
            cast_expr = _expr;
        }
        public readonly TypeName cast_typename;
        public readonly Expression cast_expr;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			AST.ExprType type;
			AST.Expr expr;

			Tuple<AST.Env, AST.ExprType> r_typename = cast_typename.GetExprType(env);
			env = r_typename.Item1;
			type = r_typename.Item2;

			Tuple<AST.Env, AST.Expr> r_expr = cast_expr.GetExpr(env);
			env = r_expr.Item1;
			expr = r_expr.Item2;

			return new Tuple<AST.Env, AST.Expr>(env, AST.TypeCast.MakeCast(expr, type));
		}
    }

}