using System;
using LexicalAnalysis;

namespace SyntaxTree {

    public abstract class Literal : Expr { }

	/// <summary>
	/// May be a float or double
	/// </summary>
	public class FloatLiteral : Literal {
		public FloatLiteral(Double value, TokenFloat.FloatSuffix floatSuffix) {
			this.Value = value;
			this.FloatSuffix = floatSuffix;
		}

		public TokenFloat.FloatSuffix FloatSuffix { get; }

		public Double Value { get; }

        public override AST.Expr GetExpr(AST.Env env) {
            switch (this.FloatSuffix) {
                case TokenFloat.FloatSuffix.F:
                    return new AST.ConstFloat((Single)this.Value, env);

                case TokenFloat.FloatSuffix.NONE:
                case TokenFloat.FloatSuffix.L:
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
	public class IntLiteral : Literal {
		public IntLiteral(Int64 value, TokenInt.IntSuffix suffix) {
			this.Value = value;
			this.Suffix = suffix;
		}

		public TokenInt.IntSuffix Suffix { get; }
		public Int64 Value { get; }

        public override AST.Expr GetExpr(AST.Env env) {
            switch (this.Suffix) {
                case TokenInt.IntSuffix.U:
                case TokenInt.IntSuffix.UL:
                    return new AST.ConstULong((UInt32)this.Value, env);

                case TokenInt.IntSuffix.NONE:
                case TokenInt.IntSuffix.L:
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