using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using ABT2.TypeSystem;

namespace ABT2.Environment {

    using IQualExprType = IQualExprType<IExprType>;

    // Global:
    //   typedef
    //   object/function
    //   enum
    //   Function:
    //     function-name
    //     params
    //     <del>typedef</del>
    //     
    //     Local:
    //       typedef
    public class Env {
        private SymbolTable Globals { get; }

        private IOption<FunctionScope> FunctionScope { get; }

        private ImmutableSortedDictionary<Int64, StructOrUnionLayout> StructOrUnionLayouts { get; }

        private Int64 nextTypeID { get; }

        private Env(SymbolTable globals,
                    IOption<FunctionScope> functionScope,
                    ImmutableSortedDictionary<Int64, StructOrUnionLayout> structOrUnionLayouts,
                    Int64 nextTypeID) {
            this.Globals = globals;
            this.FunctionScope = functionScope;
            this.StructOrUnionLayouts = structOrUnionLayouts;
            this.nextTypeID = nextTypeID;
        }

        private Env()
            : this(new SymbolTable(), Option<FunctionScope>.None, ImmutableSortedDictionary<Int64, StructOrUnionLayout>.Empty, 0) { }

        public Env(Env env)
            : this(env.Globals, env.FunctionScope, env.StructOrUnionLayouts, env.nextTypeID) { }

        private Env(Env env, SymbolTable globals)
            : this(globals, env.FunctionScope, env.StructOrUnionLayouts, env.nextTypeID) { }

        private Env(Env env, IOption<FunctionScope> functionScope)
            : this(env.Globals, functionScope, env.StructOrUnionLayouts, env.nextTypeID) { }

        private Env(Env env, ImmutableSortedDictionary<Int64, StructOrUnionLayout> structOrUnionLayouts)
            : this(env.Globals, env.FunctionScope, structOrUnionLayouts, env.nextTypeID) { }

        private Env(Env env, Int64 nextTypeID)
            : this(env.Globals, env.FunctionScope, env.StructOrUnionLayouts, nextTypeID) { }

        /// <summary>
        /// Generate a new type ID.
        /// </summary>
        private Env<Int64> NewTypeID() {
            return new Env<Int64>(
                new Env(this, this.nextTypeID + 1),
                this.nextTypeID
            );
        }

        /// <summary>
        /// Generate a new incomplete struct / union type, with a new type ID.
        /// </summary>
        private Env<TStructOrUnion> NewIncompleteStructOrUnionType(StructOrUnionKind kind) {
            var envTypeID = this.NewTypeID();
            var type = new TStructOrUnion(envTypeID.Value, kind);

            return new Env<TStructOrUnion>(envTypeID, type);
        }

        /// <summary>
        /// An empty environment.
        /// </summary>
        public static Env Empty = new Env();

        /// <summary>
        /// Generate a new incomplete struct type, with a new type ID.
        /// </summary>
        public Env<TStructOrUnion> NewIncompleteStructType() {
            return this.NewIncompleteStructOrUnionType(StructOrUnionKind.Struct);
        }

        /// <summary>
        /// Generate a new incomplete union type, with a new type ID.
        /// </summary>
        public Env<TStructOrUnion> NewIncompleteUnionType() {
            return this.NewIncompleteStructOrUnionType(StructOrUnionKind.Union);
        }

        /// <summary>
        /// Pretty print a type.
        /// </summary>
        public String DumpType(IQualExprType qualType) {
            var printer = new TypeSystemUtils.TypePrinter(qualType.TypeQuals, this);
            qualType.Type.Visit(printer);
            return printer.Name;
        }

        /// <summary>
        /// Add an symbol entry to the environment.
        /// </summary>
        public Env AddSymbol(Entry entry) {
            if (this.FunctionScope.IsSome) {
                var functionScope = this.FunctionScope.Value.AddSymbol(entry);

                return new Env(this, Option.Some(functionScope));

            } else {
                var globals = this.Globals.AddSymbol(entry);

                return new Env(this, globals);
            }
        }

        /// <summary>
        /// Add a typedef entry to the environment.
        /// </summary>
        public Env AddTypeDef(String name, IQualExprType qualType) {
            var entry = new TypeEntry(name, qualType);
            return this.AddSymbol(entry);
        }

        public Env AddLocalVar(String name, IQualExprType qualType) {
            var entry = new ObjectEntry(name, qualType);
            return this.AddSymbol(entry);
        }

        public IOption<Entry> LookUpSymbol(String name) {
            var result = this.FunctionScope.FlatMap(_ => _.LookUpSymbol(name));
            if (result.IsSome) {
                return result;
            }
            return this.Globals.LookUpSymbol(name);
        }

        public Env AddStructOrUnionLayout(TStructOrUnion type,
                                          IEnumerable<ICovariantTuple<IOption<String>, IQualExprType>> members) {
            switch (type.Kind) {
                case StructOrUnionKind.Struct:
                    var structLayout = StructOrUnionLayout.CreateStructLayout(members, this);
                    return this.AddStructOrUnionLayout(type, structLayout);
                
                case StructOrUnionKind.Union:
                    var unionLayout = StructOrUnionLayout.CreateUnionLayout(members, this);
                    return this.AddStructOrUnionLayout(type, unionLayout);

                default:
                    throw new InvalidProgramException($"Invalid {nameof(StructOrUnionKind)}: {type.Kind}");
            }
        }

        public Env<TStructOrUnion> NewStructOrUnionType(StructOrUnionKind kind, IEnumerable<ICovariantTuple<IOption<String>, IQualExprType>> members) {
            var envType = this.NewIncompleteStructOrUnionType(kind);
            var type = envType.Value;
            var env = envType.AddStructOrUnionLayout(type, members);
            return new Env<TStructOrUnion>(env, type);
        }

        public Env<TStructOrUnion> NewStructType(IEnumerable<ICovariantTuple<IOption<String>, IQualExprType>> members) {
            return this.NewStructOrUnionType(StructOrUnionKind.Struct, members);
        }

        public Env<TStructOrUnion> NewUnionType(IEnumerable<ICovariantTuple<IOption<String>, IQualExprType>> members) {
            return this.NewStructOrUnionType(StructOrUnionKind.Union, members);
        }

        public Env AddStructOrUnionLayout(TStructOrUnion type, StructOrUnionLayout layout) {
            if (this.StructOrUnionLayouts.ContainsKey(type.TypeID)) {
                throw new InvalidProgramException("Redefinition of struct/union");
            }

            return new Env(this, this.StructOrUnionLayouts.Add(type.TypeID, layout));
        }

        public Env InScope() {
            if (this.FunctionScope.IsNone) {
                throw new InvalidProgramException("Cannot open a block at global level");
            }

            return new Env(this, Option.Some(this.FunctionScope.Value.InScope()));
        }

        public Env OutScope() {
            if (this.FunctionScope.IsNone) {
                throw new InvalidProgramException("Cannot close a block at global level");
            }

            return new Env(this, Option.Some(this.FunctionScope.Value.OutScope()));
        }

        public Env InFunction(FunctionScope functionScope) {
            if (this.FunctionScope.IsSome) {
                throw new InvalidProgramException("Already inside function");
            }

            return new Env(this, Option.Some(functionScope));
        }

        public Env OutFunction() {
            if (this.FunctionScope.IsNone) {
                throw new InvalidProgramException("Cannot close a function at global level");
            }

            return new Env(this, Option<FunctionScope>.None);
        }

        public IOption<StructOrUnionLayout> GetStructOrUnionLayoutOpt(TStructOrUnion type) {
            StructOrUnionLayout layout;
            if (this.StructOrUnionLayouts.TryGetValue(type.TypeID, out layout)) {
                return Option.Some(layout);
            } else {
                return Option<StructOrUnionLayout>.None;
            }
        }
    }

    // An Env monad.
    public class Env<T> : Env {
        public Env(Env env, T value)
            : base(env) {
            this.Value = value;
        }

        public T Value { get; }
    }

    /// <summary>
    /// Base of TypeEntry, ObjectEntry, EnumEntry.
    /// </summary>
    public abstract class Entry {
        protected Entry(String name, IQualExprType qualType) {
            this.Name = name;
            this.QualType = qualType;
        }

        public String Name { get; }

        public IQualExprType QualType { get; }

        public abstract R Visit<R>(IEntryVisitor<R> visitor);
    }

    public interface IEntryVisitor<out R> {
        R VisitTypeEntry(TypeEntry entry);

        R VisitObjectEntry(ObjectEntry entry);

        R VisitEnumEntry(EnumEntry entry);
    }

    public sealed class TypeEntry : Entry {
        public TypeEntry(String name, IQualExprType type)
            : base(name, type) { }

        public override R Visit<R>(IEntryVisitor<R> visitor) {
            return visitor.VisitTypeEntry(this);
        }
    }

    public enum Linkage {
        External,
        Internal,
        None
    }

    public sealed class ObjectEntry : Entry {
        public ObjectEntry(String name, IQualExprType type, Linkage linkage)
            : base(name, type) {
            this.Linkage = linkage;
        }

        public ObjectEntry(String name, IQualExprType type)
            : this(name, type, Linkage.None) { }

        public Linkage Linkage { get; }

        public override R Visit<R>(IEntryVisitor<R> visitor) {
            return visitor.VisitObjectEntry(this);
        }
    }

    public sealed class EnumEntry : Entry {
        public EnumEntry(String name, IQualExprType type, Int64 value)
            : base(name, type) {
            this.Value = value;
        }

        public Int64 Value { get; }

        public override R Visit<R>(IEntryVisitor<R> visitor) {
            return visitor.VisitEnumEntry(this);
        }
    }

    public class SymbolTable {
        public SymbolTable() {
            this.Entries = ImmutableList<Entry>.Empty;
        }

        public SymbolTable(ImmutableList<Entry> entries) {
            this.Entries = entries;
        }

        public SymbolTable AddSymbol(Entry entry) {
            return new SymbolTable(this.Entries.Add(entry));
        }

        public IOption<Entry> LookUpSymbol(String name) {
            var entry = this.Entries.Find(e => e.Name == name);
            return entry != null ? Option.Some(entry) : Option<Entry>.None;
        }

        public ImmutableList<Entry> Entries { get; }
    }

    public class FunctionScope {
        public FunctionScope(String name, ImmutableList<ObjectEntry> args)
            : this(name, args, ImmutableStack.Create(new SymbolTable())) { }

        public FunctionScope(String name, ImmutableList<ObjectEntry> args, ImmutableStack<SymbolTable> locals) {
            this.Name = name;
            this.Args = args;
            if (locals.IsEmpty) {
                throw new InvalidProgramException("A function has at least one block.");
            }
            this.Locals = locals;
        }

        public FunctionScope InScope() {
            return new FunctionScope(this.Name, this.Args, this.Locals.Push(new SymbolTable()));
        }

        public FunctionScope OutScope() {
            return new FunctionScope(this.Name, this.Args, this.Locals.Pop());
        }

        public FunctionScope AddSymbol(Entry entry) {
            var innerScope = this.Locals.Peek();
            var outerScopes = this.Locals.Pop();

            if (outerScopes.IsEmpty) {
                if (this.Args.Exists(e => e.Name == entry.Name)) {
                    throw new InvalidProgramException($"Redefinition of {entry.Name}");
                }
            }

            var newInnerScope = innerScope.AddSymbol(entry);
            var newScopes = outerScopes.Push(newInnerScope);

            return new FunctionScope(this.Name, this.Args, newScopes);
        }

        /// <summary>
        /// Look up a symbol.
        /// </summary>
        /// <returns>
        /// If symbol found, return Some(entry).
        /// Else return None.
        /// </returns>
        public IOption<Entry> LookUpSymbol(String name) {
            // First search for local symbols.
            foreach (var scope in this.Locals) {
                var entryOpt = scope.LookUpSymbol(name);
                if (entryOpt.IsSome) {
                    return entryOpt;
                }
            }

            // Then search for arguments.
            var entry = this.Args.Find(e => e.Name == name);
            if (entry == null) {
                return Option<Entry>.None;
            }
            return Option.Some(entry);
        }

        /// <summary>
        /// The name of the current function.
        /// </summary>
        public String Name { get; }

        /// <summary>
        /// The names and types of arguments.
        /// </summary>
        public ImmutableList<ObjectEntry> Args { get; }

        /// <summary>
        /// Scopes of local variables.
        /// </summary>
        public ImmutableStack<SymbolTable> Locals { get; }
    }
}
