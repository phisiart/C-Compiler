using System;
using System.Collections.Generic;

namespace SyntaxTree {
	public class Assignment : Expr {
		public Assignment(Expr _lvalue, Expr _rvalue) {
			assign_lvalue = _lvalue;
			assign_rvalue = _rvalue;
		}
		public readonly Expr assign_lvalue;
		public readonly Expr assign_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
			AST.Expr lvalue;
			AST.Expr rvalue;

			Tuple<AST.Env, AST.Expr> r_lhs = assign_lvalue.GetExprEnv(env);
			env = r_lhs.Item1;
			lvalue = r_lhs.Item2;

			Tuple<AST.Env, AST.Expr> r_rhs = assign_rvalue.GetExprEnv(env);
			env = r_rhs.Item1;
			rvalue = r_rhs.Item2;

			rvalue = AST.TypeCast.MakeCast(rvalue, lvalue.type);

			return new Tuple<AST.Env, AST.Expr>(env, new AST.Assignment(lvalue, rvalue, lvalue.type));
		}

	}


	public class MultAssign : Expr {
		public MultAssign(Expr _lvalue, Expr _rvalue) {
			mult_lvalue = _lvalue;
			mult_rvalue = _rvalue;
		}
		public readonly Expr mult_lvalue;
		public readonly Expr mult_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				mult_lvalue,
				mult_rvalue,
				AST.Multiply.MakeMultiply
			);
		}
	}


	public class DivAssign : Expr {
		public DivAssign(Expr _lvalue, Expr _rvalue) {
			div_lvalue = _lvalue;
			div_rvalue = _rvalue;
		}
		public readonly Expr div_lvalue;
		public readonly Expr div_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				div_lvalue,
				div_rvalue,
				AST.Divide.MakeDivide
			);
		}
	}


	public class ModAssign : Expr {
		public ModAssign(Expr _lvalue, Expr _rvalue) {
			mod_lvalue = _lvalue;
			mod_rvalue = _rvalue;
		}
		public readonly Expr mod_lvalue;
		public readonly Expr mod_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				mod_lvalue,
				mod_rvalue,
				AST.Modulo.MakeModulo
			);
		}
	}


	public class AddAssign : Expr {
		public AddAssign(Expr _lvalue, Expr _rvalue) {
			add_lvalue = _lvalue;
			add_rvalue = _rvalue;
		}
		public readonly Expr add_lvalue;
		public readonly Expr add_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				add_lvalue,
				add_rvalue,
				AST.Add.MakeAdd
			);
		}
	}


	public class SubAssign : Expr {
		public SubAssign(Expr _lvalue, Expr _rvalue) {
			sub_lvalue = _lvalue;
			sub_rvalue = _rvalue;
		}
		public readonly Expr sub_lvalue;
		public readonly Expr sub_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				sub_lvalue,
				sub_rvalue,
				AST.Sub.MakeSub
			);
		}
	}


	public class LeftShiftAssign : Expr {
		public LeftShiftAssign(Expr _lvalue, Expr _rvalue) {
			lshift_lvalue = _lvalue;
			lshift_rvalue = _rvalue;
		}
		public readonly Expr lshift_lvalue;
		public readonly Expr lshift_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				lshift_lvalue,
				lshift_rvalue,
				AST.LShift.MakeLShift
			);
		}
	}


	public class RightShiftAssign : Expr {
		public RightShiftAssign(Expr _lvalue, Expr _rvalue) {
			rshift_lvalue = _lvalue;
			rshift_rvalue = _rvalue;
		}
		public readonly Expr rshift_lvalue;
		public readonly Expr rshift_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				rshift_lvalue,
				rshift_rvalue,
				AST.RShift.MakeRShift
			);
		}
	}


	public class BitwiseAndAssign : Expr {
		public BitwiseAndAssign(Expr _lvalue, Expr _rvalue) {
			and_lvalue = _lvalue;
			and_rvalue = _rvalue;
		}
		public readonly Expr and_lvalue;
		public readonly Expr and_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				and_lvalue,
				and_rvalue,
				AST.BitwiseAnd.MakeBitwiseAnd
			);
		}
	}


	public class XorAssign : Expr {
		public XorAssign(Expr _lvalue, Expr _rvalue) {
			xor_lvalue = _lvalue;
			xor_rvalue = _rvalue;
		}
		public readonly Expr xor_lvalue;
		public readonly Expr xor_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
			return GetBinaryOperation(
				env,
				xor_lvalue,
				xor_rvalue,
				AST.Xor.MakeXor
			);
		}
	}


	public class BitwiseOrAssign : Expr {
		public BitwiseOrAssign(Expr _lvalue, Expr _rvalue) {
			or_lvalue = _lvalue;
			or_rvalue = _rvalue;
		}
		public readonly Expr or_lvalue;
		public readonly Expr or_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExprEnv(AST.Env env) {
			return GetBinaryOperation(
				env,
				or_lvalue,
				or_rvalue,
				AST.BitwiseOr.MakeOr
			);
		}

	}
}
