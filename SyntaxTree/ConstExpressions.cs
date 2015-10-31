using System;

namespace SyntaxTree {

    public abstract class Constant : Expr { }

	/// <summary>
	/// May be a float or double
	/// </summary>
	public class ConstFloat : Constant {
		public ConstFloat(Double value, TokenFloat.Suffix suffix) {
			this.Value = value;
			this.Suffix = suffix;
		}
		public TokenFloat.Suffix Suffix { get; }
		public Double Value { get; }

        public override AST.Expr GetExpr(AST.Env env) {
            switch (this.Suffix) {
                case TokenFloat.Suffix.F:
                    return new AST.ConstFloat((Single)this.Value, env);
                case TokenFloat.Suffix.NONE:
                case TokenFloat.Suffix.L:
                    return new AST.ConstDouble(this.Value, env);
                default:
                    throw new InvalidOperationException();
            }
        }
	}

	/// <summary>
	/// May be signed or unsigned
    /// C doesn't have char constant, only int constant
	/// </summary>
	public class ConstInt : Constant {
		public ConstInt(Int64 value, TokenInt.Suffix suffix) {
			this.Value = value;
			this.Suffix = suffix;
		}
		public TokenInt.Suffix Suffix { get; }
		public Int64 Value { get; }

        public override AST.Expr GetExpr(AST.Env env) {
            switch (this.Suffix) {
                case TokenInt.Suffix.U:
                case TokenInt.Suffix.UL:
                    return new AST.ConstULong((UInt32)this.Value, env);
                case TokenInt.Suffix.NONE:
                case TokenInt.Suffix.L:
                    return new AST.ConstLong((Int32)this.Value, env);
                default:
                    throw new InvalidOperationException();
            }
        }
    }

	/// <summary>
	/// String Literal
	/// </summary>
	public class StringLiteral : Expr {
		public StringLiteral(String value) {
			this.Value = value;
		}
		public String Value { get; }

		public override AST.Expr GetExpr(AST.Env env) {
			return new AST.ConstStringLiteral(this.Value, env);
		}
	}

}