using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST {
    public class Decln : ExternDecln {
        public enum EnumSCS {
            AUTO,
            STATIC,
            EXTERN,
            TYPEDEF,
        }

        public Decln(String name, EnumSCS scs, ExprType type, Expr init) {
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
        }

        

        private readonly String     decln_name;
        private readonly EnumSCS    decln_scs;
        private readonly ExprType   decln_type;
        private readonly Expr       decln_init;
    }
}
