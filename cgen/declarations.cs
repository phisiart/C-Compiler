using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST {

    public class Decln : ExternDecln {
        public enum SCS {
            AUTO,
            STATIC,
            EXTERN,
            TYPEDEF,
        }

        public Decln(String name, SCS scs, ExprType type, Expr init) {
            decln_name = name;
            decln_scs  = scs;
            decln_type = type;
            decln_init = init;
        }

        public override String ToString() {
            String str = "[" + decln_scs.ToString() + "] ";
            str += decln_name;
            str += " : " + decln_type.ToString();
            return str;
        }

        public void CGenExternDecln(Env env, CGenState state) {
            state.CGenExpandStack(env.GetStackOffset(), ToString());
            if (decln_init.type.expr_type != ExprType.ExprTypeKind.VOID) {
                // need initialization

                Env.Entry entry = env.Find(decln_name);
                switch (entry.entry_loc) {
                case Env.EntryLoc.STACK:
                    // %eax = <decln_init>
                    decln_init.CGenValue(env, state);

                    // -<offset>(%ebp) = %eax
                    state.STORE(Reg.EAX, -entry.entry_offset, Reg.EBP);

                    break;
                case Env.EntryLoc.GLOBAL:
                    // TODO : extern decln global
                    break;
                case Env.EntryLoc.ENUM:
                case Env.EntryLoc.FRAME:
                case Env.EntryLoc.NOT_FOUND:
                case Env.EntryLoc.TYPEDEF:
                default:
                    throw new NotImplementedException();
                }


            }
        }

        private readonly String     decln_name;
        private readonly SCS        decln_scs;
        private readonly ExprType   decln_type;
        private readonly Expr       decln_init;
    }

    public class InitList : Expr {
        public InitList(List<Expr> _exprs) :
            base(ExprType.CreateInitList()) {
            initlist_exprs = _exprs;
        }
        public readonly List<Expr> initlist_exprs;
    }
}
