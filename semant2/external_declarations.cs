using System;
using System.Collections.Generic;

namespace AST {
    public class TranslationUnit {
        public TranslationUnit(List<Tuple<Env, ExternDecln>> _declns) {
            declns = _declns;
        }
        public readonly List<Tuple<Env, ExternDecln>> declns;
    }

    public interface ExternDecln {
    }

    public class FuncDef : ExternDecln {
        public FuncDef(String _name, TFunction _type, Stmt _stmt) {
            func_name = _name;
            func_type = _type;
            func_stmt = _stmt;
        }

        public readonly String    func_name;
        public readonly TFunction func_type;
        public readonly Stmt      func_stmt;
    }
}