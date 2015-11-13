using System;
using System.Collections.Generic;
using CodeGeneration;

namespace AST {
    public class TranslnUnit {
        public TranslnUnit(List<Tuple<Env, ExternDecln>> _declns) {
            this.declns = _declns;
        }
        public readonly List<Tuple<Env, ExternDecln>> declns;

        public void CodeGenerate(CGenState state) {
            foreach (Tuple<Env, ExternDecln> decln in this.declns) {
                decln.Item2.CGenDecln(decln.Item1, state);
            }

        }
    }

    public interface ExternDecln {
        void CGenDecln(Env env, CGenState state);
    }

    public class FuncDef : ExternDecln {
        public FuncDef(String name, StorageClass scs, TFunction type, Stmt stmt) {
            this.name = name;
            this.scs  = scs;
            this.type = type;
            this.stmt = stmt;
        }

        public override String ToString() => $"fn {this.name}: {this.type}";

        public void CGenDecln(Env env, CGenState state) {
            //     .text
            //     [.globl <func>]
            // <func>:
            //     pushl %ebp
            //     movl %esp, %ebp
            // 
            state.TEXT();
            Env.Entry entry = env.Find(this.name).Value;
            state.COMMENT(ToString());
            switch (entry.kind) {
            case Env.EntryKind.GLOBAL:
                switch (this.scs) {
                case StorageClass.AUTO:
                case StorageClass.EXTERN:
                    state.GLOBL(this.name);
                    break;
                case StorageClass.STATIC:
                    // static definition
                    break;
                default:
                    throw new InvalidOperationException();
                }
                break;
            default:
                throw new InvalidOperationException();
            }
            state.CGenFuncStart(this.name);

            state.InFunction(GotoLabelsGrabber.GrabLabels(this.stmt));

            this.stmt.CGenStmt(env, state);

            state.CGenLabel(state.ReturnLabel);
            state.OutFunction();

            //     leave
            //     ret
            state.LEAVE();
            state.RET();
            state.NEWLINE();
        }

        public readonly String      name;
        public readonly StorageClass   scs;
        public readonly TFunction   type;
        public readonly Stmt        stmt;
    }
}