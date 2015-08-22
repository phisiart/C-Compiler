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
                decln.Item2.CGenDecln(decln.Item1, state);
            }

        }
    }

    public interface ExternDecln {
        void CGenDecln(Env env, CGenState state);
    }

    public class FuncDef : ExternDecln {
        public FuncDef(String name, Decln.SCS scs, TFunction type, Stmt stmt) {
            this.name = name;
            this.scs  = scs;
            this.type = type;
            this.stmt = stmt;
        }

        public override String ToString() => $"fn {name}: {type}";

        public void CGenDecln(Env env, CGenState state) {
            //     .text
            //     [.globl <func>]
            // <func>:
            //     pushl %ebp
            //     movl %esp, %ebp
            // 
            state.TEXT();
            Env.Entry entry = env.Find(name).Value;
            state.COMMENT(ToString());
            switch (entry.kind) {
            case Env.EntryKind.GLOBAL:
                switch (scs) {
                case Decln.SCS.AUTO:
                case Decln.SCS.EXTERN:
                    state.GLOBL(name);
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
            state.CGenFuncStart(name);

            stmt.CGenStmt(env, state);

            //     leave
            //     ret
            state.LEAVE();
            state.RET();
            state.NEWLINE();
        }

        public readonly String      name;
        public readonly Decln.SCS   scs;
        public readonly TFunction   type;
        public readonly Stmt        stmt;
    }
}