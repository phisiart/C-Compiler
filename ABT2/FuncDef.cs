using System;
using ABT2.Environment;
using ABT2.TypeSystem;

namespace ABT2 {
    public sealed class FuncDef {
        public FuncDef(String name, Linkage linkage, TFunction type) {
            
        }

        public CompStmt Body { get; }

        /// <summary>
        /// The environment after the definition of the function.
        /// </summary>
        public Env Env { get; }
    }
}
