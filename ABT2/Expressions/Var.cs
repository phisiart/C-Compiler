using System;
using ABT2.TypeSystem;
using ABT2.Environment;

namespace ABT2.Expressions {
    public abstract class Var<T> : ILValueExpr<T> where T : IExprType {
        public Var(String name, IQualExprType<T> qualType, Env env) {
            this.Name = name;
            this.QualType = qualType;
            this.Env = env;
        }

        public Env Env { get; }

        public IQualExprType<T> QualType { get; }

        public T Type => this.QualType.Type;

        public String Name { get; }

        public abstract void Visit(IRValueExprByTypeVisitor visitor);

        public abstract R Visit<R>(IRValueExprByTypeVisitor<R> visitor);
    }

    public static class Var {
        private sealed class LValueCreator : IEntryVisitor<ILValueExpr<IExprType>> {
            public LValueCreator(Env env) {
                this.Env = env;
            }

            public ILValueExpr<IExprType> VisitEnumEntry(EnumEntry entry) {
                throw new InvalidProgramException("Symbol does not represent an object.");
            }

            public ILValueExpr<IExprType> VisitObjectEntry(ObjectEntry entry) {
                var creator = new VarCreator(entry, this.Env);
                // TODO: implement this.
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitTypeEntry(TypeEntry entry) {
                throw new InvalidProgramException("Symbol does not represent an object.");
            }

            public Env Env { get; }
        }

        private sealed class VarCreator : IExprTypeVisitor<ILValueExpr<IExprType>> {
            public VarCreator(ObjectEntry entry, Env env) {
                this.Entry = entry;
                this.Env = env;
            }

            public ObjectEntry Entry { get; }

            public Env Env { get; }

            public ILValueExpr<IExprType> VisitArray(ArrayType type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitDouble(TDouble type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitFloat(TFloat type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitFunction(FunctionType type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitIncompleteArray(IncompleteArrayType type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitPointer(TPointer type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitSChar(TSChar type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitSInt(TSInt type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitSLong(TSLong type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitSShort(TSShort type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitStructOrUnion(StructOrUnionType type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitUChar(TUChar type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitUInt(TUInt type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitULong(TULong type) {
                // TODO: implement this
                throw new NotImplementedException();
            }

            public ILValueExpr<IExprType> VisitUShort(TUShort type) {
                // TODO: implement this
                throw new NotImplementedException();
            }
        }

        public static ILValueExpr<IExprType> CreateLValue(String name, Env env) {
            var entryOpt = env.LookUpSymbol(name);
            if (entryOpt.IsNone) {
                throw new InvalidProgramException($"Cannot find symbol {name}");
            }

            var entry = entryOpt.Value;
            return entry.Visit(new LValueCreator(env));
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
