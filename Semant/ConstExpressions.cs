using System;
using LexicalAnalysis;

namespace AST {
    public sealed partial class FloatLiteral {
        public override ABT.Expr GetExpr(ABT.Env env) {
            switch (this.FloatSuffix) {
                case TokenFloat.FloatSuffix.F:
                    return new ABT.ConstFloat((Single)this.Value, env);

                case TokenFloat.FloatSuffix.NONE:
                case TokenFloat.FloatSuffix.L:
                    return new ABT.ConstDouble(this.Value, env);

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public sealed partial class IntLiteral {
        public override ABT.Expr GetExpr(ABT.Env env) {
            switch (this.Suffix) {
                case TokenInt.IntSuffix.U:
                case TokenInt.IntSuffix.UL:
                    return new ABT.ConstULong((UInt32)this.Value, env);

                case TokenInt.IntSuffix.NONE:
                case TokenInt.IntSuffix.L:
                    return new ABT.ConstLong((Int32)this.Value, env);

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public sealed partial class StringLiteral {
        public override ABT.Expr GetExpr(ABT.Env env) {
            return new ABT.ConstStringLiteral(this.Value, env);
        }
    }
}