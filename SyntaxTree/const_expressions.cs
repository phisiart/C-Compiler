using System;

namespace SyntaxTree {

    public abstract class Constant : Expr { }

	/// <summary>
	/// May be a float or double
	/// </summary>
	public class ConstFloat : Constant {
		public ConstFloat(Double value, FloatSuffix suffix) {
			this.value = value;
			this.suffix = suffix;
		}
		public readonly FloatSuffix suffix;
		public readonly Double value;

        public override AST.Expr GetExpr(AST.Env env) {
            switch (suffix) {
                case FloatSuffix.F:
                    return new AST.ConstFloat((Single)value);
                case FloatSuffix.NONE:
                case FloatSuffix.L:
                    return new AST.ConstDouble(value);
                default:
                    throw new InvalidOperationException();
            }
        }
	}

	/// <summary>
	/// Constant Integer
	/// </summary>
	public class ConstInt : Constant {
		public ConstInt(Int64 value, IntSuffix suffix) {
			this.value = value;
			this.suffix = suffix;
		}
		public readonly IntSuffix suffix;
		public readonly Int64 value;

        public override AST.Expr GetExpr(AST.Env env) {
            switch (suffix) {
                case IntSuffix.U:
                case IntSuffix.UL:
                    return new AST.ConstULong((UInt32)value);
                case IntSuffix.NONE:
                case IntSuffix.L:
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
		public StringLiteral(string value) {
			this.value = value;
		}
		public readonly string value;

		public override AST.Expr GetExpr(AST.Env env) {
			return new AST.ConstStringLiteral(value);
		}
	}

}