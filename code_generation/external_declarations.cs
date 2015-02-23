using System;
using System.Collections.Generic;

namespace AST {
    public class TranslationUnit {
        public TranslationUnit(List<Tuple<Env, ExternDecln>> _declns) {
            declns = _declns;
        }
        public readonly List<Tuple<Env, ExternDecln>> declns;

        public void CodeGenerate(CGenState state) {
            foreach (Tuple<Env, ExternDecln> decln in declns) {
                decln.Item2.CGenExternDecln(decln.Item1, state);
            }

        }
    }

    public interface ExternDecln {
        void CGenExternDecln(Env env, CGenState state);
    }

    public class FuncDef : ExternDecln {
        public FuncDef(String _name, TFunction _type, Stmt _stmt) {
            func_name = _name;
            func_type = _type;
            func_stmt = _stmt;
        }

        public override String ToString() {
            return "fn " + func_name + " : " + func_type.ToString();
        }

        public void CGenExternDecln(Env env, CGenState state) {
            //     .text
            //     .globl <func>
            // <func>:
            //     pushl %ebp
            //     movl %esp, %ebp
            // 
            state.TEXT();
            Env.Entry entry = env.Find(func_name);
            switch (entry.entry_loc) {
            case Env.EntryLoc.GLOBAL:
                state.GLOBL(func_name);
                break;
            case Env.EntryLoc.ENUM:
            case Env.EntryLoc.FRAME:
            case Env.EntryLoc.NOT_FOUND:
            case Env.EntryLoc.STACK:
            case Env.EntryLoc.TYPEDEF:
            default:
                throw new NotImplementedException();
            }
            state.CGenFuncName(func_name);
            state.PUSHL(Reg.EBP);
            state.MOVL(Reg.ESP, Reg.EBP);

            func_stmt.CGenStmt(env, state);

            //     leave
            //     ret
            state.LEAVE();
            state.RET();
            state.NEWLINE();
        }

        public readonly String    func_name;
        public readonly TFunction func_type;
        public readonly Stmt      func_stmt;
    }
}