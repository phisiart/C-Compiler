using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST {
    public class Utils {

        // class StoreEntry
        // ================
        // the inner storage of entries
        // 
        public class StoreEntry {
            public StoreEntry(String name, ExprType type, int offset) {
                entry_name = name;
                entry_type = type;
                entry_offset = offset;
            }
            public readonly String entry_name;
            public readonly ExprType entry_type;
            public readonly int entry_offset;
        }

        public static int RoundUp(int value, int alignment) {
            return (value + alignment - 1) & ~(alignment- 1);
        }
    }
}
