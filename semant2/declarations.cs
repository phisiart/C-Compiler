using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST {
    public class Decln {
        public enum EnumSCS {
            AUTO,
            STATIC,
            EXTERN,
            TYPEDEF,
        }

        public Decln(String name, EnumSCS scs, ExprType type) {
            decln_name = name;
            decln_scs = scs;
            decln_type = type;
        }

        private readonly String   decln_name;
        private readonly EnumSCS  decln_scs;
        private readonly ExprType decln_type;
    }
}
