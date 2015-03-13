using System;
using System.Collections.Generic;

namespace SyntaxTree {
	public class Assignment : Expression {
		public Assignment(Expression _lvalue, Expression _rvalue) {
			assign_lvalue = _lvalue;
			assign_rvalue = _rvalue;
		}
		public readonly Expression assign_lvalue;
		public readonly Expression assign_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			AST.Expr lvalue;
			AST.Expr rvalue;

			Tuple<AST.Env, AST.Expr> r_lhs = assign_lvalue.GetExpr(env);
			env = r_lhs.Item1;
			lvalue = r_lhs.Item2;

			Tuple<AST.Env, AST.Expr> r_rhs = assign_rvalue.GetExpr(env);
			env = r_rhs.Item1;
			rvalue = r_rhs.Item2;

			rvalue = AST.TypeCast.MakeCast(rvalue, lvalue.type);

			return new Tuple<AST.Env, AST.Expr>(env, new AST.Assignment(lvalue, rvalue, lvalue.type));
		}

	}


	public class MultAssign : Expression {
		public MultAssign(Expression _lvalue, Expression _rvalue) {
			mult_lvalue = _lvalue;
			mult_rvalue = _rvalue;
		}
		public readonly Expression mult_lvalue;
		public readonly Expression mult_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				mult_lvalue,
				mult_rvalue,
				AST.Multiply.MakeMultiply
			);
		}
	}


	public class DivAssign : Expression {
		public DivAssign(Expression _lvalue, Expression _rvalue) {
			div_lvalue = _lvalue;
			div_rvalue = _rvalue;
		}
		public readonly Expression div_lvalue;
		public readonly Expression div_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				div_lvalue,
				div_rvalue,
				AST.Divide.MakeDivide
			);
		}
	}


	public class ModAssign : Expression {
		public ModAssign(Expression _lvalue, Expression _rvalue) {
			mod_lvalue = _lvalue;
			mod_rvalue = _rvalue;
		}
		public readonly Expression mod_lvalue;
		public readonly Expression mod_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				mod_lvalue,
				mod_rvalue,
				AST.Modulo.MakeModulo
			);
		}
	}


	public class AddAssign : Expression {
		public AddAssign(Expression _lvalue, Expression _rvalue) {
			add_lvalue = _lvalue;
			add_rvalue = _rvalue;
		}
		public readonly Expression add_lvalue;
		public readonly Expression add_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				add_lvalue,
				add_rvalue,
				AST.Add.MakeAdd
			);
		}
	}


	public class SubAssign : Expression {
		public SubAssign(Expression _lvalue, Expression _rvalue) {
			sub_lvalue = _lvalue;
			sub_rvalue = _rvalue;
		}
		public readonly Expression sub_lvalue;
		public readonly Expression sub_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				sub_lvalue,
				sub_rvalue,
				AST.Sub.MakeSub
			);
		}
	}


	public class LeftShiftAssign : Expression {
		public LeftShiftAssign(Expression _lvalue, Expression _rvalue) {
			lshift_lvalue = _lvalue;
			lshift_rvalue = _rvalue;
		}
		public readonly Expression lshift_lvalue;
		public readonly Expression lshift_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				lshift_lvalue,
				lshift_rvalue,
				AST.LShift.MakeLShift
			);
		}
	}


	public class RightShiftAssign : Expression {
		public RightShiftAssign(Expression _lvalue, Expression _rvalue) {
			rshift_lvalue = _lvalue;
			rshift_rvalue = _rvalue;
		}
		public readonly Expression rshift_lvalue;
		public readonly Expression rshift_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				rshift_lvalue,
				rshift_rvalue,
				AST.RShift.MakeRShift
			);
		}
	}


	public class BitwiseAndAssign : Expression {
		public BitwiseAndAssign(Expression _lvalue, Expression _rvalue) {
			and_lvalue = _lvalue;
			and_rvalue = _rvalue;
		}
		public readonly Expression and_lvalue;
		public readonly Expression and_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryAssignOperation(
				env,
				and_lvalue,
				and_rvalue,
				AST.BitwiseAnd.MakeBitwiseAnd
			);
		}
	}


	public class XorAssign : Expression {
		public XorAssign(Expression _lvalue, Expression _rvalue) {
			xor_lvalue = _lvalue;
			xor_rvalue = _rvalue;
		}
		public readonly Expression xor_lvalue;
		public readonly Expression xor_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryOperation(
				env,
				xor_lvalue,
				xor_rvalue,
				AST.Xor.MakeXor
			);
		}
	}


	public class BitwiseOrAssign : Expression {
		public BitwiseOrAssign(Expression _lvalue, Expression _rvalue) {
			or_lvalue = _lvalue;
			or_rvalue = _rvalue;
		}
		public readonly Expression or_lvalue;
		public readonly Expression or_rvalue;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return GetBinaryOperation(
				env,
				or_lvalue,
				or_rvalue,
				AST.BitwiseOr.MakeOr
			);
		}

	}
}
