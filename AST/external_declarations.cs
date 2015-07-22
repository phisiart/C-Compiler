using System;
using System.Collections.Generic;

namespace AST {
    public class TranslnUnit {
        public TranslnUnit(List<Tuple<Env, ExternDecln>> _declns) {
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
        public FuncDef(string _name, Decln.SCS _scs, TFunction _type, Stmt _stmt) {
            func_name = _name;
            func_scs  = _scs;
            func_type = _type;
            func_stmt = _stmt;
        }

        public override string ToString() {
            return "fn " + func_name + " : " + func_type.ToString();
        }

        public void CGenExternDecln(Env env, CGenState state) {
            //     .text
            //     [.globl <func>]
            // <func>:
            //     pushl %ebp
            //     movl %esp, %ebp
            // 
            state.TEXT();
            Env.Entry entry = env.Find(func_name);
            state.COMMENT(ToString());
            switch (entry.kind) {
            case Env.EntryKind.GLOBAL:
                switch (func_scs) {
                case Decln.SCS.AUTO:
                case Decln.SCS.EXTERN:
                    state.GLOBL(func_name);
                    break;
                case Decln.SCS.STATIC:
                    // static definition
                    break;
                default:
                    throw new InvalidOperationException();
                }
                break;
            default:
                throw new InvalidOperationException();
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

        public readonly string        func_name;
        public readonly Decln.SCS func_scs;
        public readonly TFunction     func_type;
        public readonly Stmt          func_stmt;
    }
}