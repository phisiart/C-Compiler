using System;
using System.Collections.Generic;

namespace SyntaxTree {
	
	public abstract class Constant : Expression { }

	/// <summary>
	/// Constant Float
	/// </summary>
	public class ConstFloat : Constant {
		public ConstFloat(Double _val, FloatSuffix _suffix) {
			val = _val;
			suffix = _suffix;
		}
		public readonly FloatSuffix suffix;
		public readonly Double val;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			switch (suffix) {
			case FloatSuffix.F:
				return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstFloat((Single)val));
			default:
				return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstDouble(val));
			}
		}
	}

	/// <summary>
	/// Constant Integer
	/// </summary>
	public class ConstInt : Constant {
		public ConstInt(Int64 _val, IntSuffix _suffix) {
			val = _val;
			suffix = _suffix;
		}
		public readonly IntSuffix suffix;
		public readonly Int64 val;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			switch (suffix) {
			case IntSuffix.U:
			case IntSuffix.UL:
				return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstULong((UInt32)val));
			default:
				return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstLong((Int32)val));
			}
		}
	}

	/// <summary>
	/// String Literal
	/// </summary>
	public class StringLiteral : Expression {
		public StringLiteral(String _val) {
			val = _val;
		}
		public readonly String val;

		public override Tuple<AST.Env, AST.Expr> GetExpr(AST.Env env) {
			return new Tuple<AST.Env, AST.Expr>(env, new AST.ConstStringLiteral(val));
		}
	}

}