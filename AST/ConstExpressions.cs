using System;
using LexicalAnalysis;

namespace AST {

    public abstract class Literal : Expr { }

	/// <summary>
	/// May be a float or double
	/// </summary>
	public sealed partial class FloatLiteral : Literal {
		public FloatLiteral(Double value, TokenFloat.FloatSuffix floatSuffix) {
			this.Value = value;
			this.FloatSuffix = floatSuffix;
		}

		public TokenFloat.FloatSuffix FloatSuffix { get; }

		public Double Value { get; }
	}

	/// <summary>
	/// May be signed or unsigned
    /// C doesn't have char constant, only int constant
	/// </summary>
	public sealed partial class IntLiteral : Literal {
		public IntLiteral(Int64 value, TokenInt.IntSuffix suffix) {
			this.Value = value;
			this.Suffix = suffix;
		}

		public TokenInt.IntSuffix Suffix { get; }

		public Int64 Value { get; }
    }

	/// <summary>
	/// String Literal
	/// </summary>
	public sealed partial class StringLiteral : Expr {
		public StringLiteral(String value) {
			this.Value = value;
		}

		public String Value { get; }
	}
}