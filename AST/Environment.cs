using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
        // the return Value when searching for a symbol in the environment
        // attributes:
        //   entry_loc: the location of this object
        //   entry_type: the Type of the object
        //   entry_offset: this is used to determine the address of the object
        //              STACK: addr = %ebp - offset
        //              GLOBAL: N/A
        // 
        public class Entry {
            public Entry(EntryKind kind, ExprType type, Int32 offset) {
                this.Kind = kind;
                this.Type = type;
                this.Offset = offset;
            }
            public readonly EntryKind Kind;
            public readonly ExprType  Type;
            public readonly Int32     Offset;
        }

        private class Scope {

            // private constructor
            // ===================
            // 
            private Scope(List<Utils.StoreEntry> stack_entries,
                          Int32 stack_offset,
                          List<Utils.StoreEntry> global_entries,
                          FunctionType curr_func,
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
                       new EmptyFunctionType(),
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
            // input: loc, name, Type
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
            // input: name, Type
            // output: Environment
            // return a new environment which adds a enum Value
            // 
            public Scope PushEnum(String name, ExprType type, Int32 value) {
                Scope scope = new Scope(this);
                scope.enums.Add(new Utils.StoreEntry(name, type, value));
                return scope;
            }


            // SetCurrFunc
            // ===========
            // set the current function
            public Scope SetCurrentFunction(FunctionType type) {
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
                if ((store_entry = this.func.Args.FindLast(entry => entry.name == name)) != null) {
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
                foreach (Utils.StoreEntry entry in this.func.Args) {
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
            public readonly FunctionType              func;
            public readonly List<Utils.StoreEntry> globals;
            public readonly List<Utils.StoreEntry> typedefs;
            public readonly List<Utils.StoreEntry> enums;
            public Int32 esp_pos;

        }

        // Environment
        // ===========
        // construct an environment with a single empty scope
        public Env() {
            this._scopes = ImmutableStack.Create(new Scope());
        }

        // Environment
        // ===========
        // construct an environment with the given scopes
        // 
        private Env(ImmutableStack<Scope> scopes) {
            this._scopes = scopes;
        }
        
        // InScope
        // =======
        // input: void
        // output: Environment
        // return a new environment which has a new inner scope
        // 
        public Env InScope() {
            return new Env(this._scopes.Push(this._scopes.Peek().InScope()));
        }

        // OutScope
        // ========
        // input: void
        // output: Environment
        // return a new environment which goes out of the most inner scope of the current environment
        // 
        public Env OutScope() {
            return new Env(this._scopes.Pop());
        }

        // PushEntry
        // =========
        // input: loc, name, Type
        // ouput: Environment
        // return a new environment which adds a symbol entry
        // 
        public Env PushEntry(EntryKind loc, String name, ExprType type) {
            Scope top = this._scopes.Peek();
            return new Env(this._scopes.Pop().Push(top.PushEntry(loc, name, type)));
        }

        // PushEnum
        // ========
        // input: name, Type
        // output: Environment
        // return a new environment which adds a enum Value
        // 
        public Env PushEnum(String name, ExprType type, Int32 value) {
            Scope top = this._scopes.Peek();
            return new Env(this._scopes.Pop().Push(top.PushEnum(name, type, value)));
        }

        // SetCurrentFunction
        // ==================
        // input: Type
        // ouput: Environment
        // return a new environment which sets the current function
        // 
        public Env SetCurrentFunction(FunctionType type) {
            Scope top = this._scopes.Peek();
            return new Env(this._scopes.Pop().Push(top.SetCurrentFunction(type)));
        }

        // GetCurrentFunction
        // ==================
        // input: void
        // output: FunctionType
        // return the Type of the current function
        // 
        public FunctionType GetCurrentFunction() {
            return this._scopes.Peek().func;
        }

        // GetStackOffset
        // ==============
        // input: void
        // output: Int32
        // return the current stack size
        // 
        public Int32 StackSize => -this._scopes.Peek().esp_pos;

        public Option<Entry> Find(String name) {
            Entry entry = null;
            foreach (Scope scope in this._scopes) {
                if ((entry = scope.Find(name)) != null) {
                    return new Some<Entry>(entry);
                }
            }
			return new None<Entry>();
        }

        public Option<Entry> FindInCurrentScope(String name) {
            var entry = this._scopes.Peek().Find(name);
            if (entry == null) {
                return Option<Entry>.None;
            }
            return Option.Some(entry);
        }

        public Boolean IsGlobal() {
            return this._scopes.Count() == 1;
        }

        public String Dump() {
            String str = "";
            Int32 depth = 0;
            foreach (Scope scope in this._scopes) {
                str += scope.Dump(depth, "  ");
                depth++;
            }
            return str;
        }

        private readonly ImmutableStack<Scope> _scopes;

    }

    /// <summary>
    /// 1. A global scope.
    /// 2. A function scope, with multiple name scopes.
    /// 3. ObjectId.
    /// 4. TypeId.
    /// 
    /// </summary>
    public sealed class Env2 {

        // global scope
        //   global object
        //   static object
        //   typedef
        //   enum value
        // local scope
        //   static object
        //   typedef
        //   frame object
        //   enum value
        //   parameter object

        public enum EntryKind {
            FRAME,
            // STACK,
            GLOBAL,
            TYPEDEF,
            ENUM,
            NAMED,
            PARAM
        }

        public abstract class SymbolEntry {
            public abstract EntryKind Kind { get; }
        }

        public abstract class ObjectEntry : SymbolEntry {
            public ExprType Type { get; }
        }

        // public sealed class StackObjectEntry : ObjectEntry {
        //     public override EntryKind Kind => EntryKind.STACK;
        // }

        public sealed class ParameterObjectEntry : ObjectEntry {
            public override EntryKind Kind => EntryKind.PARAM;
        }

        public sealed class FrameObjectEntry : ObjectEntry {
            public override EntryKind Kind => EntryKind.FRAME;
        }

        public sealed class NamedObjectEntry : ObjectEntry {
            public override EntryKind Kind => EntryKind.NAMED;
        }

        public sealed class TypeEntry : SymbolEntry {
            public override EntryKind Kind => EntryKind.TYPEDEF;
            public ExprType Type { get; }
        }

        public sealed class EnumEntry : SymbolEntry {
            public override EntryKind Kind => EntryKind.ENUM;
        }

        private abstract class SymbolTable {
            protected SymbolTable(ImmutableList<TypeEntry> typeDefs, ImmutableList<EnumEntry> enums) {
                this.TypeDefs = typeDefs;
                this.Enums = enums;
            }

            protected SymbolTable()
                : this(ImmutableList<TypeEntry>.Empty, ImmutableList<EnumEntry>.Empty) { }

            public ImmutableList<TypeEntry> TypeDefs { get; }

            public ImmutableList<EnumEntry> Enums { get; }
        }

        private sealed class GlobalSymbolTable : SymbolTable {
            public GlobalSymbolTable(ImmutableList<TypeEntry> typeDefs, ImmutableList<EnumEntry> enums, ImmutableList<NamedObjectEntry> globalObjects)
                : base(typeDefs, enums) {
                this.GlobalObjects = globalObjects;
            }

            public GlobalSymbolTable()
                : this(ImmutableList<TypeEntry>.Empty, ImmutableList<EnumEntry>.Empty, ImmutableList<NamedObjectEntry>.Empty) { }

            public GlobalSymbolTable Add(TypeEntry typeEntry) =>
                new GlobalSymbolTable(this.TypeDefs.Add(typeEntry), this.Enums, this.GlobalObjects);

            public GlobalSymbolTable Add(EnumEntry enumEntry) =>
                new GlobalSymbolTable(this.TypeDefs, this.Enums.Add(enumEntry), this.GlobalObjects);

            public GlobalSymbolTable Add(NamedObjectEntry globalObjectEntry) =>
                new GlobalSymbolTable(this.TypeDefs, this.Enums, this.GlobalObjects.Add(globalObjectEntry));

            public ImmutableList<NamedObjectEntry> GlobalObjects { get; }
        }

        private sealed class LocalSymbolTable : SymbolTable {
            public LocalSymbolTable(ImmutableList<TypeEntry> typeDefs, ImmutableList<EnumEntry> enums, ImmutableList<FrameObjectEntry> frameObjects)
                : base(typeDefs, enums) {
                this.FrameObjects = frameObjects;
            }

            public LocalSymbolTable() {
                this.FrameObjects = ImmutableList<FrameObjectEntry>.Empty;
            }

            public LocalSymbolTable Add(TypeEntry typeEntry) =>
                new LocalSymbolTable(this.TypeDefs.Add(typeEntry), this.Enums, this.FrameObjects);

            public LocalSymbolTable Add(EnumEntry enumEntry) =>
                new LocalSymbolTable(this.TypeDefs, this.Enums.Add(enumEntry), this.FrameObjects);

            public LocalSymbolTable Add(FrameObjectEntry frameObjectEntry) =>
                new LocalSymbolTable(this.TypeDefs, this.Enums, this.FrameObjects.Add(frameObjectEntry));

            public ImmutableList<FrameObjectEntry> FrameObjects { get; }
        }

        /// <summary>
        /// Function info, local symbol tables.
        /// </summary>
        private sealed class FunctionScope {
            public FunctionScope(FunctionType functionType, ImmutableList<ParameterObjectEntry> functionParams, ImmutableStack<LocalSymbolTable> localScopes) {
                this.FunctionType = functionType;
                this.FunctionParams = FunctionParams;
                this.LocalScopes = localScopes;
            }

            public FunctionScope(FunctionType functionType, ImmutableList<ParameterObjectEntry> functionParams)
                : this(functionType, functionParams, ImmutableStack<LocalSymbolTable>.Empty) { }

            public FunctionScope Add(TypeEntry typeEntry) {
                var localSymbleTable = this.LocalScopes.Peek().Add(typeEntry);
                return new FunctionScope(
                    this.FunctionType,
                    this.FunctionParams,
                    this.LocalScopes.Pop().Push(localSymbleTable)
                );
            }

            public FunctionScope Add(EnumEntry enumEntry) {
                var localSymbleTable = this.LocalScopes.Peek().Add(enumEntry);
                return new FunctionScope(
                    this.FunctionType,
                    this.FunctionParams,
                    this.LocalScopes.Pop().Push(localSymbleTable)
                );
            }

            public FunctionScope Add(FrameObjectEntry frameObjectEntry) {
                var localSymbleTable = this.LocalScopes.Peek().Add(frameObjectEntry);
                return new FunctionScope(
                    this.FunctionType,
                    this.FunctionParams,
                    this.LocalScopes.Pop().Push(localSymbleTable)
                );
            }

            public FunctionType FunctionType { get; }

            public ImmutableList<ParameterObjectEntry> FunctionParams { get; }

            public ImmutableStack<LocalSymbolTable> LocalScopes { get; }
        }

        private Env2(GlobalSymbolTable globalSymbolTable, Option<FunctionScope> functionScope) {
            this._globalSymbolTable = globalSymbolTable;
            this._functionScope = functionScope;
        }

        public Env2() : this(new GlobalSymbolTable(), Option<FunctionScope>.None) { }

        private GlobalSymbolTable _globalSymbolTable { get; }

        private Option<FunctionScope> _functionScope { get; }

        public Env2 InFunction(FunctionType functionType, ImmutableList<ParameterObjectEntry> functionParams) {
            if (this._functionScope.IsSome) {
                throw new InvalidProgramException("Is already in a function. Cannot go in function.");
            }
            return new Env2(
                this._globalSymbolTable,
                Option.Some(new FunctionScope(functionType, functionParams))
            );
        }

        public Env2 OutFunction() {
            if (this._functionScope.IsNone) {
                throw new InvalidProgramException("Is already global. Cannot go out of function.");
            }
            return new Env2(
                this._globalSymbolTable,
                Option<FunctionScope>.None
            );
        }

        /// <summary>
        /// Add a new local symbol table.
        /// </summary>
        /// <returns>
        /// The new environment.
        /// </returns>
        public Env2 InScope() {
            if (this._functionScope.IsNone) {
                throw new InvalidProgramException("Isn't in a function. Cannot push scope.");
            }

            return new Env2(
                this._globalSymbolTable,
                Option.Some(new FunctionScope(
                    this._functionScope.Value.FunctionType,
                    this._functionScope.Value.FunctionParams,
                    this._functionScope.Value.LocalScopes.Push(new LocalSymbolTable())
                ))
            );
        }

        /// <summary>
        /// Pop a local symbol table.
        /// </summary>
        /// <returns>
        /// The new environment.
        /// </returns>
        public Env2 OutScope() {
            if (this._functionScope.IsNone) {
                throw new InvalidProgramException("Isn't in a function. Cannot pop scope.");
            }
            if (this._functionScope.Value.LocalScopes.IsEmpty) {
                throw new InvalidProgramException("No Local scope to pop.");
            }
            return new Env2(
                this._globalSymbolTable,
                Option.Some(new FunctionScope(
                    this._functionScope.Value.FunctionType,
                    this._functionScope.Value.FunctionParams,
                    this._functionScope.Value.LocalScopes.Pop()
                ))
            );
        }

        public Env2 Add(EnumEntry entry) {
            if (this._functionScope.IsNone) {
                // global
                return new Env2(
                    this._globalSymbolTable.Add(entry),
                    this._functionScope
                );
            } else {
                // local
                return new Env2(
                    this._globalSymbolTable,
                    Option.Some(this._functionScope.Value.Add(entry))
                );
            }
        }

        public Env2 Add(TypeEntry entry) {
            if (this._functionScope.IsNone) {
                // global
                return new Env2(
                    this._globalSymbolTable.Add(entry),
                    this._functionScope
                );
            } else {
                // local
                return new Env2(
                    this._globalSymbolTable,
                    Option.Some(this._functionScope.Value.Add(entry))
                );
            }
        }

        public Env2 Add(NamedObjectEntry entry) {
            if (this._functionScope.IsSome) {
                
            }
            throw new Exception();
        }
    }
}