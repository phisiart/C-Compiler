using System;
using System.Collections.Generic;

namespace SyntaxTree {

	/// <summary>
	/// Multiplication
	/// 
	/// Perform usual arithmetic conversion.
	/// </summary>
	public class Multiplication : Expr {
		public Multiplication(Expr _lhs, Expr _rhs) {
			mult_lhs = _lhs;
			mult_rhs = _rhs;
		}
		public readonly Expr mult_lhs;
		public readonly Expr mult_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class Division : Expr {
		public Division(Expr _lhs, Expr _rhs) {
			div_lhs = _lhs;
			div_rhs = _rhs;
		}
		public readonly Expr div_lhs;
		public readonly Expr div_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class Modulo : Expr {
		public Modulo(Expr _lhs, Expr _rhs) {
			mod_lhs = _lhs;
			mod_rhs = _rhs;
		}
		public readonly Expr mod_lhs;
		public readonly Expr mod_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class Addition : Expr {
		public Addition(Expr _lhs, Expr _rhs) {
			add_lhs = _lhs;
			add_rhs = _rhs;
		}
		public readonly Expr add_lhs;
		public readonly Expr add_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class Subtraction : Expr {
		public Subtraction(Expr _lhs, Expr _rhs) {
			sub_lhs = _lhs;
			sub_rhs = _rhs;
		}
		public readonly Expr sub_lhs;
		public readonly Expr sub_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class LeftShift : Expr {
		public LeftShift(Expr _lhs, Expr _rhs) {
			shift_lhs = _lhs;
			shift_rhs = _rhs;
		}
		public readonly Expr shift_lhs;
		public readonly Expr shift_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class RightShift : Expr {
		public RightShift(Expr _lhs, Expr _rhs) {
			shift_lhs = _lhs;
			shift_rhs = _rhs;
		}
		public readonly Expr shift_lhs;
		public readonly Expr shift_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class LessThan : Expr {
		public LessThan(Expr _lhs, Expr _rhs) {
			lt_lhs = _lhs;
			lt_rhs = _rhs;
		}
		public readonly Expr lt_lhs;
		public readonly Expr lt_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class LessEqualThan : Expr {
		public LessEqualThan(Expr _lhs, Expr _rhs) {
			leq_lhs = _lhs;
			leq_rhs = _rhs;
		}

		public readonly Expr leq_lhs;
		public readonly Expr leq_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class GreaterThan : Expr {
		public GreaterThan(Expr _lhs, Expr _rhs) {
			gt_lhs = _lhs;
			gt_rhs = _rhs;
		}

		public readonly Expr gt_lhs;
		public readonly Expr gt_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class GreaterEqualThan : Expr {
		public GreaterEqualThan(Expr _lhs, Expr _rhs) {
			geq_lhs = _lhs;
			geq_rhs = _rhs;
		}

		public readonly Expr geq_lhs;
		public readonly Expr geq_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class Equal : Expr {
		public Equal(Expr _lhs, Expr _rhs) {
			eq_lhs = _lhs;
			eq_rhs = _rhs;
		}

		public readonly Expr eq_lhs;
		public readonly Expr eq_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class NotEqual : Expr {
		public NotEqual(Expr _lhs, Expr _rhs) {
			neq_lhs = _lhs;
			neq_rhs = _rhs;
		}

		public readonly Expr neq_lhs;
		public readonly Expr neq_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class BitwiseAnd : Expr {
		public BitwiseAnd(Expr _lhs, Expr _rhs) {
			and_lhs = _lhs;
			and_rhs = _rhs;
		}
		public readonly Expr and_lhs;
		public readonly Expr and_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class Xor : Expr {
		public Xor(Expr _lhs, Expr _rhs) {
			xor_lhs = _lhs;
			xor_rhs = _rhs;
		}
		public readonly Expr xor_lhs;
		public readonly Expr xor_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class BitwiseOr : Expr {
		public BitwiseOr(Expr _lhs, Expr _rhs) {
			or_lhs = _lhs;
			or_rhs = _rhs;
		}
		public readonly Expr or_lhs;
		public readonly Expr or_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class LogicalAnd : Expr {
		public LogicalAnd(Expr _lhs, Expr _rhs) {
			and_lhs = _lhs;
			and_rhs = _rhs;
		}
		public readonly Expr and_lhs;
		public readonly Expr and_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
	public class LogicalOr : Expr {
		public LogicalOr(Expr _lhs, Expr _rhs) {
			or_lhs = _lhs;
			or_rhs = _rhs;
		}
		public readonly Expr or_lhs;
		public readonly Expr or_rhs;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
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
