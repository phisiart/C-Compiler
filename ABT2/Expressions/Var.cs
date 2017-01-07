using System;
using ABT2.TypeSystem;
using ABT2.Environment;

namespace ABT2.Expressions {
    
    public interface IVar : ILValueExpr { }

    public interface IVar<out T> : ILValueExpr<T>, IVar
        where T : IExprType { }

    public abstract class Var<T> : LValueExpr<T>, IVar<T>
        where T : class, IExprType {

        protected Var(String name, IQualExprType<T> qualType, Env env) {
            this.Name = name;
            this.QualType = qualType;
            this.Env = env;
        }

        public override sealed Env Env { get; }

        public override sealed IQualExprType<T> QualType { get; }

        public override sealed T Type => this.QualType.Type;

        public String Name { get; }
    }

    public static class Var {
        private sealed class LValueCreator : IEntryVisitor<ILValueExpr> {
            public LValueCreator(Env env) {
                this.Env = env;
            }

            public ILValueExpr VisitEnumEntry(EnumEntry entry) {
                throw new InvalidProgramException("Symbol does not represent an object.");
            }

            public ILValueExpr VisitObjectEntry(ObjectEntry entry) {
                var creator = new VarCreator(entry, this.Env);
                return entry.QualType.Type.Visit(creator);
            }

            public ILValueExpr VisitTypeEntry(TypeEntry entry) {
                throw new InvalidProgramException("Symbol does not represent an object.");
            }

            public Env Env { get; }
        }

        private sealed class RValueCreator : IEntryVisitor<IRValueExpr> {
            public RValueCreator(Env env) {
                this.Env = env;
            }

            public IRValueExpr VisitEnumEntry(EnumEntry entry) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public IRValueExpr VisitObjectEntry(ObjectEntry entry) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public IRValueExpr VisitTypeEntry(TypeEntry entry) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public Env Env { get; }
        }

        private sealed class VarCreator : IExprTypeVisitor<ILValueExpr> {
            public VarCreator(ObjectEntry entry, Env env) {
                this.Entry = entry;
                this.Env = env;
            }

            public ObjectEntry Entry { get; }

            public Env Env { get; }

            public ILValueExpr VisitArray(ArrayType type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr VisitDouble(TDouble type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr VisitFloat(TFloat type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr VisitFunction(TFunction type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr VisitIncompleteArray(IncompleteArrayType type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr VisitPointer(TPointer type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr VisitSChar(TSChar type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr VisitSInt(TSInt type) {
                return new SIntVar(this.Entry.Name, (QualSInt)this.Entry.QualType, this.Env);
            }

            public ILValueExpr VisitSLong(TSLong type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr VisitSShort(TSShort type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr VisitStructOrUnion(TStructOrUnion type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr VisitUChar(TUChar type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr VisitUInt(TUInt type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr VisitULong(TULong type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr VisitUShort(TUShort type) {
                // TODO: implement this
                throw new NotImplementedException();
            }
        }

        public static ILValueExpr CreateLValue(String name, Env env) {
            var entryOpt = env.LookUpSymbol(name);
            if (entryOpt.IsNone) {
                throw new InvalidProgramException($"Cannot find symbol {name}");
            }

            var entry = entryOpt.Value;
            return entry.Visit(new LValueCreator(env));
        }

        public static IRValueExpr CreateRValue(String name, Env env) {
            var entryOpt = env.LookUpSymbol(name);
            if (entryOpt.IsNone) {
                throw new InvalidProgramException($"Cannot find symbol {name}");
            }

            var entry = entryOpt.Value;
            return entry.Visit(new RValueCreator(env));
        }
    }

    public sealed class SIntVar : Var<TSInt> {
        public SIntVar(String name, IQualExprType<TSInt> qualType, Env env)
            : base(name, qualType, env) { }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitSInt(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitSInt(this);
        }
    }

    public sealed class UIntVar : Var<TUInt> {
        public UIntVar(String name, IQualExprType<TUInt> qualType, Env env)
            : base(name, qualType, env) { }

        public override void Visit(IRValueExprByTypeVisitor visitor) {
            visitor.VisitUInt(this);
        }

        public override R Visit<R>(IRValueExprByTypeVisitor<R> visitor) {
            return visitor.VisitUInt(this);
        }
    }
}
