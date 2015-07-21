using System;

namespace SyntaxTree {

    public abstract class Constant : Expr { }

	/// <summary>
	/// May be a float or double
	/// </summary>
	public class ConstFloat : Constant {
		public ConstFloat(Double value, TokenFloat.Suffix suffix) {
			this.value = value;
			this.suffix = suffix;
		}
		public readonly TokenFloat.Suffix suffix;
		public readonly Double value;

        public override AST.Expr GetExpr(AST.Env env) {
            switch (suffix) {
                case TokenFloat.Suffix.F:
                    return new AST.ConstFloat((Single)value);
                case TokenFloat.Suffix.NONE:
                case TokenFloat.Suffix.L:
                    return new AST.ConstDouble(value);
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
			this.value = value;
			this.suffix = suffix;
		}
		public readonly TokenInt.Suffix suffix;
		public readonly Int64 value;

        public override AST.Expr GetExpr(AST.Env env) {
            switch (suffix) {
                case TokenInt.Suffix.U:
                case TokenInt.Suffix.UL:
                    return new AST.ConstULong((UInt32)value);
                case TokenInt.Suffix.NONE:
                case TokenInt.Suffix.L:
                    return new AST.ConstLong((Int32)value);
                default:
                    throw new InvalidOperationException();
            }
        }
    }

	/// <summary>
	/// string Literal
	/// </summary>
	public class StringLiteral : Expr {
		public StringLiteral(String value) {
			this.value = value;
		}
		public readonly String value;

		public override AST.Expr GetExpr(AST.Env env) {
			return new AST.ConstStringLiteral(value);
		}
	}

}