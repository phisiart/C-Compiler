using System;
using System.Collections.Generic;

namespace AST {
    public class Env {

        // enum EntryLoc
        // =============
        // the location of an object
        //   STACK: this is a variable stored in the stack
        //   FRAME: this is a function parameter
        //   GLOBAL: this is a global symbol
        // 
        public enum EntryKind {
            ENUM,
            TYPEDEF,
            STACK,
            FRAME,
            GLOBAL
        }
        
        
        // class Entry
        // ===========
        // the return value when searching for a symbol in the environment
        // attributes:
        //   entry_loc: the location of this object
        //   entry_type: the type of the object
        //   entry_offset: this is used to determine the address of the object
        //              STACK: addr = %ebp - offset
        //              GLOBAL: N/A
        // 
        public class Entry {
            public Entry(EntryKind kind, ExprType type, Int32 offset) {
                this.kind = kind;
                this.type = type;
                this.offset = offset;
            }
            public readonly EntryKind kind;
            public readonly ExprType  type;
            public readonly Int32     offset;
        }

        private class Scope {

            // private constructor
            // ===================
            // 
            private Scope(List<Utils.StoreEntry> stack_entries,
                          Int32 stack_offset,
                          List<Utils.StoreEntry> global_entries,
                          TFunction curr_func,
                          List<Utils.StoreEntry> typedef_entries,
                          List<Utils.StoreEntry> enum_entries) {
                this.locals = stack_entries;
                this.esp_pos = stack_offset;
                this.globals = global_entries;
                this.func = curr_func;
                this.typedefs = typedef_entries;
                this.enums = enum_entries;
            }

            // copy constructor
            // ================
            // 
            private Scope(Scope other)
                : this(new List<Utils.StoreEntry>(other.locals),
                       other.esp_pos,
                       new List<Utils.StoreEntry>(other.globals),
                       other.func,
                       new List<Utils.StoreEntry>(other.typedefs),
                       new List<Utils.StoreEntry>(other.enums)) {}

            // empty Scope
            // ===========
            // 
            public Scope()
                : this(new List<Utils.StoreEntry>(),
                       0,
                       new List<Utils.StoreEntry>(),
                       new TEmptyFunction(),
                       new List<Utils.StoreEntry>(),
                       new List<Utils.StoreEntry>()) {}


            // InScope
            // =======
            // create a new scope with:
            //   the same stack offset
            //   the same current function
            //   other entries are empty
            // 
            public Scope InScope() {
                return new Scope(new List<Utils.StoreEntry>(), this.esp_pos,
                                 new List<Utils.StoreEntry>(), this.func,
                                 new List<Utils.StoreEntry>(),
                                 new List<Utils.StoreEntry>());
            }


            // PushEntry
            // =========
            // input: loc, name, type
            // output: Scope
            // returns a new scope with everything the same as this, excpet for a new entry
            // 
            public Scope PushEntry(EntryKind loc, String name, ExprType type) {
                Scope scope = new Scope(this);
                switch (loc) {
                case EntryKind.STACK:
                    scope.esp_pos -= Utils.RoundUp(type.SizeOf, 4);
                    scope.locals.Add(new Utils.StoreEntry(name, type, scope.esp_pos));
                    break;
                case EntryKind.GLOBAL:
                    scope.globals.Add(new Utils.StoreEntry(name, type, 0));
                    break;
                case EntryKind.TYPEDEF:
                    scope.typedefs.Add(new Utils.StoreEntry(name, type, 0));
                    break;
                default:
                    return null;
                }
                return scope;
            }


            // PushEnum
            // ========
            // input: name, type
            // output: Environment
            // return a new environment which adds a enum value
            // 
            public Scope PushEnum(String name, ExprType type, Int32 value) {
                Scope scope = new Scope(this);
                scope.enums.Add(new Utils.StoreEntry(name, type, value));
                return scope;
            }


            // SetCurrFunc
            // ===========
            // set the current function
            public Scope SetCurrentFunction(TFunction type) {
                return new Scope(this.locals, this.esp_pos, this.globals,
                    type, this.typedefs, this.enums
                );
            }


            // Find
            // ====
            // input: name
            // output: Entry
            // search for a symbol in the current scope
            // 
            public Entry Find(String name) {
                Utils.StoreEntry store_entry;

                // search the enum entries
                if ((store_entry = this.enums.FindLast(entry => entry.name == name)) != null) {
                    return new Entry(EntryKind.ENUM, store_entry.type, store_entry.offset);
                }

                // search the typedef entries
                if ((store_entry = this.typedefs.FindLast(entry => entry.name == name)) != null) {
                    return new Entry(EntryKind.TYPEDEF, store_entry.type, store_entry.offset);
                }
                
                // search the stack entries
                if ((store_entry = this.locals.FindLast(entry => entry.name == name)) != null) {
                    return new Entry(EntryKind.STACK, store_entry.type, store_entry.offset);
                }

                // search the function arguments
                if ((store_entry = this.func.args.FindLast(entry => entry.name == name)) != null) {
                    return new Entry(EntryKind.FRAME, store_entry.type, store_entry.offset);
                }

                // search the global entries
                if ((store_entry = this.globals.FindLast(entry => entry.name == name)) != null) {
                    return new Entry(EntryKind.GLOBAL, store_entry.type, store_entry.offset);
                }

                return null;
            }


            // Dump
            // ====
            // input: depth, indent
            // output: String
            // dumps the content in this level
            // 
            public String Dump(Int32 depth, String single_indent) {
                String indent = "";
                for (; depth > 0; depth--) {
                    indent += single_indent;
                }

                String str = "";
                foreach (Utils.StoreEntry entry in this.func.args) {
                    str += indent;
                    str += "[%ebp + " + entry.offset + "] " + entry.name + " : " + entry.type + "\n";
                }
                foreach (Utils.StoreEntry entry in this.globals) {
                    str += indent;
                    str += "[extern] " + entry.name + " : " + entry.type + "\n";
                }
                foreach (Utils.StoreEntry entry in this.locals) {
                    str += indent;
                    str += "[%ebp - " + entry.offset + "] " + entry.name + " : " + entry.type + "\n";
                }
                foreach (Utils.StoreEntry entry in this.typedefs) {
                    str += indent;
                    str += "typedef: " + entry.name + " <- " + entry.type + "\n";
                }
                foreach (Utils.StoreEntry entry in this.enums) {
                    str += indent;
                    str += entry.name + " = " + entry.offset + "\n";
                }
                return str;

            }


            // ================================================================
            //  private members
            // ================================================================
            public readonly List<Utils.StoreEntry> locals;
            public readonly TFunction              func;
            public readonly List<Utils.StoreEntry> globals;
            public readonly List<Utils.StoreEntry> typedefs;
            public readonly List<Utils.StoreEntry> enums;
            public Int32 esp_pos;

        }

        // Environment
        // ===========
        // construct an environment with a single empty scope
        public Env() {
            this.env_scopes = new Stack<Scope>();
            this.env_scopes.Push(new Scope());
        }

        // Environment
        // ===========
        // construct an environment with the given scopes
        // 
        private Env(Stack<Scope> scopes) {
            this.env_scopes = scopes;
        }
        
        // InScope
        // =======
        // input: void
        // output: Environment
        // return a new environment which has a new inner scope
        // 
        public Env InScope() {
            Stack<Scope> scopes = new Stack<Scope>(new Stack<Scope>(this.env_scopes));
            scopes.Push(scopes.Peek().InScope());
            return new Env(scopes);
        }

        // OutScope
        // ========
        // input: void
        // output: Environment
        // return a new environment which goes out of the most inner scope of the current environment
        // 
        public Env OutScope() {
            Stack<Scope> scopes = new Stack<Scope>(new Stack<Scope>(this.env_scopes));
            scopes.Pop();
            return new Env(scopes);
        }

        // PushEntry
        // =========
        // input: loc, name, type
        // ouput: Environment
        // return a new environment which adds a symbol entry
        // 
        public Env PushEntry(EntryKind loc, String name, ExprType type) {
            // note the nested copy constructor. this is because the constructor would reverse the elements.
            Stack<Scope> scopes = new Stack<Scope>(new Stack<Scope>(this.env_scopes));
            Scope top = scopes.Pop().PushEntry(loc, name, type);
            scopes.Push(top);
            return new Env(scopes);
        }

        // PushEnum
        // ========
        // input: name, type
        // output: Environment
        // return a new environment which adds a enum value
        // 
        public Env PushEnum(String name, ExprType type, Int32 value) {
            Stack<Scope> scopes = new Stack<Scope>(new Stack<Scope>(this.env_scopes));
            Scope top = scopes.Pop().PushEnum(name, type, value);
            scopes.Push(top);
            return new Env(scopes);
        }

        // SetCurrentFunction
        // ==================
        // input: type
        // ouput: Environment
        // return a new environment which sets the current function
        // 
        public Env SetCurrentFunction(TFunction type) {
            Stack<Scope> scopes = new Stack<Scope>(new Stack<Scope>(this.env_scopes));
            Scope top = scopes.Pop().SetCurrentFunction(type);
            scopes.Push(top);
            return new Env(scopes);
        }

        // GetCurrentFunction
        // ==================
        // input: void
        // output: TFunction
        // return the type of the current function
        // 
        public TFunction GetCurrentFunction() {
            return this.env_scopes.Peek().func;
        }

        // GetStackOffset
        // ==============
        // input: void
        // output: Int32
        // return the current stack size
        // 
        public Int32 StackSize => -this.env_scopes.Peek().esp_pos;

        public Option<Entry> Find(String name) {
            Entry entry = null;
            foreach (Scope scope in this.env_scopes) {
                if ((entry = scope.Find(name)) != null) {
                    return new Some<Entry>(entry);
                }
            }
			return new None<Entry>();
        }

        public Option<Entry> FindInCurrentScope(String name) {
            var entry = this.env_scopes.Peek().Find(name);
            if (entry == null) {
                return Option<Entry>.None;
            }
            return Option.Some(entry);
        }

        public Boolean IsGlobal() {
            return this.env_scopes.Count == 1;
        }

        public String Dump() {
            String str = "";
            Int32 depth = 0;
            foreach (Scope scope in this.env_scopes) {
                str += scope.Dump(depth, "  ");
                depth++;
            }
            return str;
        }

        private readonly Stack<Scope> env_scopes;

    }
}