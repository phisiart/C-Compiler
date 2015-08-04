using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AST {

    /// <summary>
    /// expr++: must be integral, float or pointer.
    /// 
    /// If expr is an array, it is converted to a pointer in semantic analysis.
    /// </summary>
    public class PostIncrement : Expr {
        public PostIncrement(Expr expr)
            : base(expr.type) {
            Debug.Assert(expr.type.IsScalar());
            this.expr = expr;
        }
        public readonly Expr expr;

        public override Reg CGenValue(Env env, CGenState state) {

            // %eax = &expr
            expr.CGenAddress(env, state);

            // push address
            state.PUSHL(Reg.EAX);

            // %eax = expr or %st(0) = expr
            Reg ret = expr.CGenValue(env, state);

            switch (ret) {
            case Reg.EAX:
                // integral or pointer

                // pop address to %ecx
                state.POPL(Reg.ECX);

                // %eax = value (need to modify and save back)
                // %ebx = value (cache)
                // %ecx = address
                state.MOVL(Reg.EAX, Reg.EBX);

                switch (expr.type.kind) {
                case ExprType.Kind.CHAR:
                case ExprType.Kind.UCHAR:
                    state.ADDL(1, Reg.EAX);
                    state.MOVB(Reg.AL, 0, Reg.ECX);
                    break;

                case ExprType.Kind.SHORT:
                case ExprType.Kind.USHORT:
                    state.ADDL(1, Reg.EAX);
                    state.MOVW(Reg.AX, 0, Reg.ECX);
                    break;

                case ExprType.Kind.LONG:
                case ExprType.Kind.ULONG:
                    state.ADDL(1, Reg.EBX);
                    state.MOVL(Reg.EAX, 0, Reg.ECX);
                    break;

                case ExprType.Kind.POINTER:
                    state.ADDL(expr.type.size_of, Reg.EBX);
                    state.MOVL(Reg.EAX, 0, Reg.ECX);
                    break;

                default:
                    throw new InvalidProgramException();
                }

                // return original value to %eax
                state.MOVL(Reg.EBX, Reg.EAX);

                break;

            case Reg.ST0:
                // float
                switch (expr.type.kind) {
                case ExprType.Kind.FLOAT:
                case ExprType.Kind.DOUBLE:
                default:
                    throw new InvalidProgramException();
                }

            default:
                throw new InvalidProgramException();
            }

        }
    }

    /// <summary>
    /// expr--: must be a scalar
    /// </summary>
    public class PostDecrement : Expr {
        public PostDecrement(Expr expr)
            : base(expr.type) {
            Debug.Assert(expr.type.IsScalar());
            this.expr = expr;
        }
        public readonly Expr expr;
    }

    /// <summary>
    /// ++expr: must be a scalar
    /// </summary>
    public class PreIncrement : Expr {
        public PreIncrement(Expr expr)
            : base(expr.type) {
            Debug.Assert(expr.type.IsScalar());
            this.expr = expr;
        }
        public readonly Expr expr;


    }

    /// <summary>
    /// --expr: must be a scalar
    /// </summary>
    public class PreDecrement : Expr {
        public PreDecrement(Expr expr)
            : base(expr.type) {
            Debug.Assert(expr.type.IsScalar());
            this.expr = expr;
        }
        public readonly Expr expr;
    }

}
