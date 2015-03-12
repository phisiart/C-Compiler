using System;
using System.Collections.Generic;

namespace SyntaxTree {

	/// <summary>
	/// Multiplication
	/// 
	/// Perform usual arithmetic conversion.
	/// </summary>
	public class Multiplication : Expression {
		public Multiplication(Expression _lhs, Expression _rhs) {
			mult_lhs = _lhs;
			mult_rhs = _rhs;
		}
		public readonly Expression mult_lhs;
		public readonly Expression mult_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryOperation(
				env,
				mult_lhs,
				mult_rhs,
				AST.Multiply.MakeMultiply
			);
		}

	}

	/// <summary>
	/// Division
	/// 
	/// Perform usual arithmetic conversion.
	/// </summary>
	public class Division : Expression {
		public Division(Expression _lhs, Expression _rhs) {
			div_lhs = _lhs;
			div_rhs = _rhs;
		}
		public readonly Expression div_lhs;
		public readonly Expression div_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryOperation(
				env,
				div_lhs,
				div_rhs,
				AST.Divide.MakeDivide
			);
		}
	}


	/// <summary>
	/// Modulo
	/// 
	/// Only accepts integrals.
	/// </summary>
	public class Modulo : Expression {
		public Modulo(Expression _lhs, Expression _rhs) {
			mod_lhs = _lhs;
			mod_rhs = _rhs;
		}
		public readonly Expression mod_lhs;
		public readonly Expression mod_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return Expression.GetIntegralBinOpExpr(
				env,
				mod_lhs,
				mod_rhs,
				(x, y) => x % y,
				(x, y) => x % y,
				(lhs, rhs, type) => new AST.Modulo(lhs, rhs, type)
			);
		}

	}


	/// <summary>
	/// Addition
	/// 
	/// There are two kinds of addition:
	/// 1. both operands are of arithmetic type
	/// 2. one operand is a pointer, and the other is an integral
	/// 
	/// </summary>
	public class Addition : Expression {
		public Addition(Expression _lhs, Expression _rhs) {
			add_lhs = _lhs;
			add_rhs = _rhs;
		}

		public static AST.Expr GetPointerAddition(AST.Expr ptr, AST.Expr offset) {
			if (ptr.type.expr_type != AST.ExprType.EnumExprType.POINTER) {
				throw new InvalidOperationException("Error: expect a pointer");
			}
			if (offset.type.expr_type != AST.ExprType.EnumExprType.LONG) {
				throw new InvalidOperationException("Error: expect an integer");
			}

			if (ptr.IsConstExpr() && offset.IsConstExpr()) {
				Int32 _base = (Int32)((AST.ConstPtr)ptr).value;
				Int32 _scale = ((AST.TPointer)(ptr.type)).referenced_type.SizeOf;
				Int32 _offset = ((AST.ConstLong)offset).value;
				return new AST.ConstPtr((UInt32)(_base + _scale * _offset), ptr.type);
			}

			return AST.TypeCast.ToPointer(
				new AST.Add(
					AST.TypeCast.FromPointer(ptr, new AST.TLong(ptr.type.is_const, ptr.type.is_volatile)),
					new AST.Multiply(
						offset,
						new AST.ConstLong(((AST.TPointer)(ptr.type)).referenced_type.SizeOf),
						new AST.TLong(offset.type.is_const, offset.type.is_volatile)
					),
					new AST.TLong(offset.type.is_const, offset.type.is_volatile)
				),
				ptr.type
			);

		}

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			AST.Expr lhs;
			AST.Expr rhs;

			Tuple<AST.Env, AST.Expr> r_lhs = add_lhs.GetExpr(env);
			env = r_lhs.Item1;
			lhs = r_lhs.Item2;

			Tuple<AST.Env, AST.Expr> r_rhs = add_rhs.GetExpr(env);
			env = r_rhs.Item1;
			rhs = r_rhs.Item2;

			if (lhs.type.expr_type == AST.ExprType.EnumExprType.POINTER) {
				if (!rhs.type.IsIntegral()) {
					throw new InvalidOperationException("Error: must add an integral to a pointer");
				}
				rhs = AST.TypeCast.MakeCast(rhs, new AST.TLong(rhs.type.is_const, rhs.type.is_volatile));

				// lhs = base, rhs = offset
				return new Tuple<AST.Env, AST.Expr>(env, GetPointerAddition(lhs, rhs));

			} else if (rhs.type.expr_type == AST.ExprType.EnumExprType.POINTER) {
				if (!lhs.type.IsIntegral()) {
					throw new InvalidOperationException("Error: must add an integral to a pointer");
				}
				lhs = AST.TypeCast.MakeCast(lhs, new AST.TLong(lhs.type.is_const, rhs.type.is_volatile));

				// rhs = base, lhs = offset
				return new Tuple<AST.Env, AST.Expr>(env, GetPointerAddition(rhs, lhs));

			} else {
				return Expression.GetArithmeticBinOpExpr(
					env,
					lhs,
					rhs,
					(x, y) => x + y,
					(x, y) => x + y,
					(x, y) => x + y,
					(x, y) => x + y,
					(_lhs, _rhs, _type) => new AST.Add(_lhs, _rhs, _type)
				);
			}
		}

		public readonly Expression add_lhs;
		public readonly Expression add_rhs;

	}


	/// <summary>
	/// Subtraction
	/// 
	/// There are two kinds of subtractions:
	/// 1. arithmetic - arithmetic
	/// 2. pointer - integral
	/// 3. pointer - pointer
	/// </summary>
	public class Subtraction : Expression {
		public Subtraction(Expression _lhs, Expression _rhs) {
			sub_lhs = _lhs;
			sub_rhs = _rhs;
		}

		public readonly Expression sub_lhs;
		public readonly Expression sub_rhs;

		public static AST.Expr GetPointerSubtraction(AST.Expr ptr, AST.Expr offset) {
			if (ptr.type.expr_type != AST.ExprType.EnumExprType.POINTER) {
				throw new InvalidOperationException("Error: expect a pointer");
			}
			if (offset.type.expr_type != AST.ExprType.EnumExprType.LONG) {
				throw new InvalidOperationException("Error: expect an integer");
			}

			if (ptr.IsConstExpr() && offset.IsConstExpr()) {
				Int32 _base = (Int32)((AST.ConstPtr)ptr).value;
				Int32 _scale = ((AST.TPointer)(ptr.type)).referenced_type.SizeOf;
				Int32 _offset = ((AST.ConstLong)offset).value;
				return new AST.ConstPtr((UInt32)(_base - _scale * _offset), ptr.type);
			}

			return AST.TypeCast.ToPointer(
				new AST.Sub(
					AST.TypeCast.FromPointer(ptr, new AST.TLong(ptr.type.is_const, ptr.type.is_volatile)),
					new AST.Multiply(
						offset,
						new AST.ConstLong(((AST.TPointer)(ptr.type)).referenced_type.SizeOf),
						new AST.TLong(offset.type.is_const, offset.type.is_volatile)
					),
					new AST.TLong(offset.type.is_const, offset.type.is_volatile)
				),
				ptr.type
			);

		}

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			AST.Expr lhs;
			AST.Expr rhs;

			Tuple<AST.Env, AST.Expr> r_lhs = sub_lhs.GetExpr(env);
			env = r_lhs.Item1;
			lhs = r_lhs.Item2;

			Tuple<AST.Env, AST.Expr> r_rhs = sub_rhs.GetExpr(env);
			env = r_rhs.Item1;
			rhs = r_rhs.Item2;

			if (lhs.type.expr_type == AST.ExprType.EnumExprType.POINTER) {
				if (rhs.type.expr_type == AST.ExprType.EnumExprType.POINTER) {
					// both operands are pointers

					AST.TPointer lhs_type = (AST.TPointer)(lhs.type);
					AST.TPointer rhs_type = (AST.TPointer)(rhs.type);
					if (!lhs_type.referenced_type.EqualType(rhs_type.referenced_type)) {
						throw new InvalidOperationException("Error: the two pointers points to different types");
					}

					Int32 scale = lhs_type.referenced_type.SizeOf;

					if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
						return new Tuple<AST.Env, AST.Expr>(
							env,
							new AST.ConstLong(
								(Int32)(((AST.ConstPtr)lhs).value - ((AST.ConstPtr)rhs).value) / scale
							)
						);

					} else {
						return new Tuple<AST.Env, AST.Expr>(
							env,
							new AST.Divide(
								// long(lhs) - long(rhs)
								new AST.Sub(
									AST.TypeCast.MakeCast(lhs, new AST.TLong()),
									AST.TypeCast.MakeCast(rhs, new AST.TLong()),
									new AST.TLong()
								),
								// / scale
								new AST.ConstLong(scale),
								new AST.TLong()
							)
						);
					}

				} else {
					// pointer - integral

					if (!rhs.type.IsIntegral()) {
						throw new InvalidOperationException("Error: expected an integral");
					}

					rhs = AST.TypeCast.MakeCast(rhs, new AST.TLong(rhs.type.is_const, rhs.type.is_volatile));

					return new Tuple<AST.Env, AST.Expr>(env, GetPointerSubtraction(lhs, rhs));
				}

			} else {
				// lhs is not a pointer.

				// we need usual arithmetic cast
				return GetArithmeticBinOpExpr(
					env,
					lhs,
					rhs,
					(x, y) => x - y,
					(x, y) => x - y,
					(x, y) => x - y,
					(x, y) => x - y,
					(_lhs, _rhs, _type) => new AST.Sub(_lhs, _rhs, _type)
				);

			}
		}
	}


	/// <summary>
	/// Left Shift
	/// 
	/// Returns an integer from two integrals.
	/// </summary>
	public class LeftShift : Expression {
		public LeftShift(Expression _lhs, Expression _rhs) {
			shift_lhs = _lhs;
			shift_rhs = _rhs;
		}
		public Expression shift_lhs;
		public Expression shift_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return Expression.GetIntegralBinOpExpr(
				env,
				shift_lhs,
				shift_rhs,
				(x, y) => (UInt32)((Int32)x << (Int32)y),
				(x, y) => x << y,
				(lhs, rhs, type) => new AST.LShift(lhs, rhs, type)
			);
		}
	}


	/// <summary>
	/// Right Shift
	/// 
	/// Returns an integer from two integrals.
	/// </summary>
	public class RightShift : Expression {
		public RightShift(Expression _lhs, Expression _rhs) {
			shift_lhs = _lhs;
			shift_rhs = _rhs;
		}
		public Expression shift_lhs;
		public Expression shift_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return Expression.GetIntegralBinOpExpr(
				env,
				shift_lhs,
				shift_rhs,
				(x, y) => (UInt32)((Int32)x >> (Int32)y),
				(x, y) => x >> y,
				(lhs, rhs, type) => new AST.RShift(lhs, rhs, type)
			);
		}

	}


	/// <summary>
	/// Less than
	/// 
	/// Returns an integer.
	/// </summary>
	public class LessThan : Expression {
		public LessThan(Expression _lhs, Expression _rhs) {
			lt_lhs = _lhs;
			lt_rhs = _rhs;
		}
		public Expression lt_lhs;
		public Expression lt_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetScalarBinLogicalOpExpr(
				env,
				lt_lhs,
				lt_rhs,
				(x, y) => x < y ? 1 : 0,
				(x, y) => x < y ? 1 : 0,
				(x, y) => x < y ? 1 : 0,
				(x, y) => x < y ? 1 : 0,
				(lhs, rhs, type) => new AST.Less(lhs, rhs, type)
			);
		}
	}


	/// <summary>
	/// Less or Equal than
	/// 
	/// Returns an integer.
	/// </summary>
	public class LessEqualThan : Expression {
		public LessEqualThan(Expression _lhs, Expression _rhs) {
			leq_lhs = _lhs;
			leq_rhs = _rhs;
		}

		public Expression leq_lhs;
		public Expression leq_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetScalarBinLogicalOpExpr(
				env,
				leq_lhs,
				leq_rhs,
				(x, y) => x <= y ? 1 : 0,
				(x, y) => x <= y ? 1 : 0,
				(x, y) => x <= y ? 1 : 0,
				(x, y) => x <= y ? 1 : 0,
				(lhs, rhs, type) => new AST.LEqual(lhs, rhs, type)
			);
		}

	}


	/// <summary>
	/// Greater than
	/// 
	/// Returns an integer.
	/// </summary>
	public class GreaterThan : Expression {
		public GreaterThan(Expression _lhs, Expression _rhs) {
			gt_lhs = _lhs;
			gt_rhs = _rhs;
		}

		public Expression gt_lhs;
		public Expression gt_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetScalarBinLogicalOpExpr(
				env,
				gt_lhs,
				gt_rhs,
				(x, y) => x > y ? 1 : 0,
				(x, y) => x > y ? 1 : 0,
				(x, y) => x > y ? 1 : 0,
				(x, y) => x > y ? 1 : 0,
				(lhs, rhs, type) => new AST.Greater(lhs, rhs, type)
			);
		}
	}


	/// <summary>
	/// Greater or Equal than
	/// 
	/// Returns an integer.
	/// </summary>
	public class GreaterEqualThan : Expression {
		public GreaterEqualThan(Expression _lhs, Expression _rhs) {
			geq_lhs = _lhs;
			geq_rhs = _rhs;
		}

		public Expression geq_lhs;
		public Expression geq_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetScalarBinLogicalOpExpr(
				env,
				geq_lhs,
				geq_rhs,
				(x, y) => x >= y ? 1 : 0,
				(x, y) => x >= y ? 1 : 0,
				(x, y) => x >= y ? 1 : 0,
				(x, y) => x >= y ? 1 : 0,
				(lhs, rhs, type) => new AST.GEqual(lhs, rhs, type)
			);
		}
	}

	// Equal
	// =====
	// requires arithmetic or pointer type
	// 
	public class Equal : Expression {
		public Equal(Expression _lhs, Expression _rhs) {
			eq_lhs = _lhs;
			eq_rhs = _rhs;
		}

		public Expression eq_lhs;
		public Expression eq_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetScalarBinLogicalOpExpr(
				env,
				eq_lhs,
				eq_rhs,
				(x, y) => x == y ? 1 : 0,
				(x, y) => x == y ? 1 : 0,
				(x, y) => x == y ? 1 : 0,
				(x, y) => x == y ? 1 : 0,
				(lhs, rhs, type) => new AST.Equal(lhs, rhs, type)
			);
		}
	}

	// NotEqual
	// ========
	// requires arithmetic or pointer type
	// 
	public class NotEqual : Expression {
		public NotEqual(Expression _lhs, Expression _rhs) {
			neq_lhs = _lhs;
			neq_rhs = _rhs;
		}

		public Expression neq_lhs;
		public Expression neq_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetScalarBinLogicalOpExpr(
				env,
				neq_lhs,
				neq_rhs,
				(x, y) => x != y ? 1 : 0,
				(x, y) => x != y ? 1 : 0,
				(x, y) => x != y ? 1 : 0,
				(x, y) => x != y ? 1 : 0,
				(lhs, rhs, type) => new AST.NotEqual(lhs, rhs, type)
			);
		}
	}

	/// <summary>
	/// Bitwise And
	/// 
	/// Returns an integer.
	/// </summary>
	public class BitwiseAnd : Expression {
		public BitwiseAnd(Expression _lhs, Expression _rhs) {
			and_lhs = _lhs;
			and_rhs = _rhs;
		}

		public Expression and_lhs;
		public Expression and_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetIntegralBinOpExpr(
				env,
				and_lhs,
				and_rhs,
				(x, y) => x & y,
				(x, y) => x & y,
				(lhs, rhs, type) => new AST.BitwiseAnd(lhs, rhs, type)
			);
		}
	}

	// Xor
	// ===
	// requires integral type
	// 
	public class Xor : Expression {
		public Xor(Expression _lhs, Expression _rhs) {
			xor_lhs = _lhs;
			xor_rhs = _rhs;
		}
		public Expression xor_lhs;
		public Expression xor_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetIntegralBinOpExpr(
				env,
				xor_lhs,
				xor_rhs,
				(x, y) => x ^ y,
				(x, y) => x ^ y,
				(lhs, rhs, type) => new AST.Xor(lhs, rhs, type)
			);
		}
	}

	// BitwiseOr
	// =========
	// requires integral type
	// 
	public class BitwiseOr : Expression {
		public BitwiseOr(Expression _lhs, Expression _rhs) {
			or_lhs = _lhs;
			or_rhs = _rhs;
		}
		public Expression or_lhs;
		public Expression or_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetIntegralBinOpExpr(
				env,
				or_lhs,
				or_rhs,
				(x, y) => x | y,
				(x, y) => x | y,
				(lhs, rhs, type) => new AST.BitwiseOr(lhs, rhs, type)
			);
		}
	}

	// LogicalAnd
	// ==========
	// requires arithmetic or pointer type
	// 
	public class LogicalAnd : Expression {
		public LogicalAnd(Expression _lhs, Expression _rhs) {
			and_lhs = _lhs;
			and_rhs = _rhs;
		}
		public Expression and_lhs;
		public Expression and_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetScalarBinLogicalOpExpr(
				env,
				and_lhs,
				and_rhs,
				(x, y) => x != 0 && y != 0 ? 1 : 0,
				(x, y) => x != 0 && y != 0 ? 1 : 0,
				(x, y) => x != 0 && y != 0 ? 1 : 0,
				(x, y) => x != 0 && y != 0 ? 1 : 0,
				(lhs, rhs, type) => new AST.LogicalAnd(lhs, rhs, type)
			);
		}
	}

	// LogicalOr
	// =========
	// requires arithmetic or pointer type
	// 
	public class LogicalOr : Expression {
		public LogicalOr(Expression _lhs, Expression _rhs) {
			or_lhs = _lhs;
			or_rhs = _rhs;
		}
		public Expression or_lhs;
		public Expression or_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetScalarBinLogicalOpExpr(
				env,
				or_lhs,
				or_rhs,
				(x, y) => x != 0 || y != 0 ? 1 : 0,
				(x, y) => x != 0 || y != 0 ? 1 : 0,
				(x, y) => x != 0 || y != 0 ? 1 : 0,
				(x, y) => x != 0 || y != 0 ? 1 : 0,
				(lhs, rhs, type) => new AST.LogicalOr(lhs, rhs, type)
			);
		}
	}
}
