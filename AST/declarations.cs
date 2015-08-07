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

        public Decln(String name, SCS scs, ExprType type, Option<Initr> initr) {
            this.name = name;
            this.scs  = scs;
            this.type = type;
            this.initr = initr;
        }

        public override String ToString() {
            String str = "[" + scs.ToString() + "] ";
            str += name;
            str += " : " + type.ToString();
            return str;
        }

        public void CGenExternDecln(Env env, CGenState state) {
            state.CGenExpandStackTo(env.GetStackOffset(), ToString());
            //if (initr.type.kind != ExprType.Kind.VOID) {
            //    // need initialization

            //    Env.Entry entry = env.Find(name);
            //    switch (entry.kind) {
            //    case Env.EntryKind.STACK:
            //        // %eax = <decln_init>
            //        initr.CGenValue(env, state);

            //        // -<offset>(%ebp) = %eax
            //        state.MOVL(Reg.EAX, -entry.offset, Reg.EBP);

            //        break;
            //    case Env.EntryKind.GLOBAL:
            //        // TODO : extern decln global
            //        break;
            //    case Env.EntryKind.ENUM:
            //    case Env.EntryKind.FRAME:
            //    case Env.EntryKind.NOT_FOUND:
            //    case Env.EntryKind.TYPEDEF:
            //    default:
            //        throw new NotImplementedException();
            //    }


            //}
        }

        public void What(ExprType type, Initr initr) {
            switch (type.kind) {
                case ExprType.Kind.ARRAY:
                    throw new NotImplementedException();
            }
        }

        private readonly String         name;
        private readonly SCS            scs;
        private readonly ExprType       type;
        private readonly Option<Initr>  initr;
    }

    public class Initr {
        public Initr(Kind kind) {
            this.kind = kind;
        }
        public enum Kind {
            EXPR,
            INIT_LIST,
        }
        public readonly Kind kind;
    }

    public class InitExpr : Initr {
        public InitExpr(Expr expr)
            : base(Kind.EXPR) {
            this.expr = expr;
        }
        public readonly Expr expr;
    }

    public class InitList : Initr {
        public InitList(List<Initr> initrs) :
            base(Kind.INIT_LIST) {
            this.initrs = initrs;
        }
        public readonly List<Initr> initrs;
    }
}
