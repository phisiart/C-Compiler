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
			return GetBinaryOperation(
				env,
				mod_lhs,
				mod_rhs,
				AST.Modulo.MakeModulo
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
		public readonly Expression add_lhs;
		public readonly Expression add_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryOperation(
				env,
				add_lhs,
				add_rhs,
				AST.Add.MakeAdd
			);
		}
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

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryOperation(
				env,
				sub_lhs,
				sub_rhs,
				AST.Sub.MakeSub
			);
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
		public readonly Expression shift_lhs;
		public readonly Expression shift_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryOperation(
				env,
				shift_lhs,
				shift_rhs,
				AST.LShift.MakeLShift
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
		public readonly Expression shift_lhs;
		public readonly Expression shift_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryOperation(
				env,
				shift_lhs,
				shift_rhs,
				AST.RShift.MakeRShift
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
		public readonly Expression lt_lhs;
		public readonly Expression lt_rhs;

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

		public readonly Expression leq_lhs;
		public readonly Expression leq_rhs;

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

		public readonly Expression gt_lhs;
		public readonly Expression gt_rhs;

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

		public readonly Expression geq_lhs;
		public readonly Expression geq_rhs;

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

		public readonly Expression eq_lhs;
		public readonly Expression eq_rhs;

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

		public readonly Expression neq_lhs;
		public readonly Expression neq_rhs;

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
		public readonly Expression and_lhs;
		public readonly Expression and_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryOperation(
				env,
				and_lhs,
				and_rhs,
				AST.BitwiseAnd.MakeBitwiseAnd
			);
		}
	}

	/// <summary>
	/// Xor
	/// 
	/// Returns an integer.
	/// </summary>
	public class Xor : Expression {
		public Xor(Expression _lhs, Expression _rhs) {
			xor_lhs = _lhs;
			xor_rhs = _rhs;
		}
		public readonly Expression xor_lhs;
		public readonly Expression xor_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryOperation(
				env,
				xor_lhs,
				xor_rhs,
				AST.Xor.MakeXor
			);
		}
	}

	/// <summary>
	/// Bitwise Or
	/// 
	/// Accepts two integrals, and returns an integer.
	/// </summary>
	public class BitwiseOr : Expression {
		public BitwiseOr(Expression _lhs, Expression _rhs) {
			or_lhs = _lhs;
			or_rhs = _rhs;
		}
		public readonly Expression or_lhs;
		public readonly Expression or_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryOperation(
				env,
				or_lhs,
				or_rhs,
				AST.BitwiseOr.MakeOr
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
		public readonly Expression and_lhs;
		public readonly Expression and_rhs;

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
		public readonly Expression or_lhs;
		public readonly Expression or_rhs;

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
