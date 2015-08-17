using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST {

    public class Decln : ExternDecln {
        public enum SCS {
            AUTO,
            STATIC,
            EXTERN,
            TYPEDEF,
        }

        public Decln(String name, SCS scs, ExprType type, Option<Initr> initr) {
            this.name = name;
            this.scs = scs;
            this.type = type;
            this.initr = initr;
        }

        public override String ToString() {
            String str = "[" + scs.ToString() + "] ";
            str += name;
            str += " : " + type.ToString();
            return str;
        }

        // * function;
        // * extern function;
        // * static function;
        // * obj;
        // * obj = init;
        // * static obj;
        // * static obj = init;
        // * extern obj;
        // * extern obj = init;
        public void CGenDecln(Env env, CGenState state) {

            if (env.IsGlobal()) {

                if (this.initr.IsSome) {
                    Initr initr = this.initr.value;
                    switch (scs) {
                        case SCS.AUTO:
                            state.GLOBL(name);
                            break;

                        case SCS.EXTERN:
                            throw new InvalidProgramException();

                        case SCS.STATIC:
                            break;

                        case SCS.TYPEDEF:
                            throw new InvalidProgramException();

                        default:
                            throw new InvalidProgramException();
                    }

                    state.DATA();

                    state.ALIGN(ExprType.ALIGN_LONG);

                    state.CGenLabel(name);

                    Int32 last = 0;
                    initr.Iterate(type, (Int32 offset, Expr expr) => {
                        if (offset > last) {
                            state.ZERO(offset - last);
                        }

                        if (!expr.IsConstExpr()) {
                            throw new InvalidOperationException("Cannot initialize with non-const expression.");
                        }

                        switch (expr.type.kind) {
                            // TODO: without const char/short, how do I initialize?
                            case ExprType.Kind.CHAR:
                            case ExprType.Kind.UCHAR:
                            case ExprType.Kind.SHORT:
                            case ExprType.Kind.USHORT:
                                throw new NotImplementedException();
                            case ExprType.Kind.LONG:
                                state.LONG(((ConstLong)expr).value);
                                break;

                            case ExprType.Kind.ULONG:
                                state.LONG((Int32)((ConstULong)expr).value);
                                break;

                            case ExprType.Kind.POINTER:
                                state.LONG((Int32)((ConstPtr)expr).value);
                                break;

                            case ExprType.Kind.FLOAT:
                                byte[] float_bytes = BitConverter.GetBytes(((ConstFloat)expr).value);
                                Int32 intval = BitConverter.ToInt32(float_bytes, 0);
                                state.LONG(intval);
                                break;

                            case ExprType.Kind.DOUBLE:
                                byte[] double_bytes = BitConverter.GetBytes(((ConstDouble)expr).value);
                                Int32 first_int = BitConverter.ToInt32(double_bytes, 0);
                                Int32 second_int = BitConverter.ToInt32(double_bytes, 4);
                                state.LONG(first_int);
                                state.LONG(second_int);
                                break;

                            default:
                                throw new InvalidProgramException();
                        }

                        last = offset + expr.type.SizeOf;
                    });

                } else {

                    switch (scs) {
                        case SCS.AUTO:
                            // .comm name,size,align
                            break;

                        case SCS.EXTERN:
                            break;

                        case SCS.STATIC:
                            // .local name
                            // .comm name,size,align
                            state.LOCAL(name);
                            break;

                        case SCS.TYPEDEF:
                            throw new InvalidProgramException();

                        default:
                            throw new InvalidProgramException();
                    }

                    state.COMM(name, type.SizeOf, ExprType.ALIGN_LONG);
                    
                }

            } else {
                // stack object

                state.CGenExpandStackTo(env.StackSize, ToString());

                Int32 stack_size = env.StackSize;

                // pos should be equal to stack_size, but whatever...
                Int32 pos = env.Find(name).offset;
                if (this.initr.IsNone) {
                    return;
                }

                Initr initr = this.initr.value;
                initr.Iterate(type, (Int32 offset, Expr expr) => {
                    Reg ret = expr.CGenValue(env, state);
                    switch (expr.type.kind) {
                        case ExprType.Kind.CHAR:
                        case ExprType.Kind.UCHAR:
                            state.MOVB(Reg.EAX, pos + offset, Reg.EBP);
                            break;

                        case ExprType.Kind.SHORT:
                        case ExprType.Kind.USHORT:
                            state.MOVW(Reg.EAX, pos + offset, Reg.EBP);
                            break;

                        case ExprType.Kind.DOUBLE:
                            state.FSTPL(pos + offset, Reg.EBP);
                            break;

                        case ExprType.Kind.FLOAT:
                            state.FSTPS(pos + offset, Reg.EBP);
                            break;

                        case ExprType.Kind.LONG:
                        case ExprType.Kind.ULONG:
                        case ExprType.Kind.POINTER:
                            state.MOVL(Reg.EAX, pos + offset, Reg.EBP);
                            break;

                        case ExprType.Kind.STRUCT_OR_UNION:
                            state.MOVL(Reg.EAX, Reg.ESI);
                            state.LEA(pos + offset, Reg.EBP, Reg.EDI);
                            state.MOVL(expr.type.SizeOf, Reg.ECX);
                            state.CGenMemCpy();
                            break;

                        case ExprType.Kind.ARRAY:
                        case ExprType.Kind.FUNCTION:
                            throw new InvalidProgramException($"How could a {expr.type.kind} be in a init list?");

                        default:
                            throw new InvalidProgramException();
                    }

                    state.CGenForceStackSizeTo(stack_size);

                });

            } // stack object
        }

        public void What(ExprType type, Initr initr) {
            switch (type.kind) {
                case ExprType.Kind.ARRAY:
                    throw new NotImplementedException();
            }
        }

        private readonly String name;
        private readonly SCS scs;
        private readonly ExprType type;
        private readonly Option<Initr> initr;
    }

    /// <summary>
    /// 1. Scalar: an expression, optionally enclosed in braces.
    ///    int a = 1;              // valid
    ///    int a = { 1 };          // valid
    ///    int a[] = { { 1 }, 2 }; // valid
    ///    int a = {{ 1 }};        // warning in gcc, a == 1; error in MSVC
    ///    int a = { { 1 }, 2 };   // warning in gcc, a == 1; error in MSVC
    ///    int a = { 1, 2 };       // warning in gcc, a == 1; error in MSVC
    ///    I'm following MSVC: you either put an expression, or add a single layer of brace.
    /// 
    /// 2. Union:
    ///    union A { int a; int b; };
    ///    union A u = { 1 };               // always initialize the first member, i.e. a, not b.
    ///    union A u = {{ 1 }};             // valid
    ///    union A u = another_union;       // valid
    /// 
    /// 3. Struct:
    ///    struct A { int a; int b; };
    ///    struct A = another_struct;       // valid
    ///    struct A = { another_struct };   // error, once you put a brace, the compiler assumes you want to initialize members.
    /// 
    /// From 2 and 3, once seen union or struct, either read expression or brace.
    /// 
    /// 4. Array of characters:
    ///    char a[] = { 'a', 'b' }; // valid
    ///    char a[] = "abc";        // becomes char a[4]: include '\0'
    ///    char a[3] = "abc";       // valid, ignore '\0'
    ///    char a[2] = "abc";       // warning in gcc; error in MSVC
    ///    If the aggregate contains members that are aggregates or unions, or if the first member of a union is an aggregate or union, the rules apply recursively to the subaggregates or contained unions. If the initializer of a subaggregate or contained union begins with a left brace, the initializers enclosed by that brace and its matching right brace initialize the members of the subaggregate or the first member of the contained union. Otherwise, only enough initializers from the list are taken to account for the members of the first subaggregate or the first member of the contained union; any remaining initializers are left to initialize the next member of the aggregate of which the current subaggregate or contained union is a part.
    /// </summary>
    public abstract class Initr {
        public Initr(Kind kind) {
            this.kind = kind;
        }
        public enum Kind {
            EXPR,
            INIT_LIST,
        }
        public readonly Kind kind;

        public abstract Initr ConformType(MemberIterator iter);

        public Initr ConformType(ExprType type) => ConformType(new MemberIterator(type));

        public abstract void Iterate(MemberIterator iter, Action<Int32, Expr> action);

        public void Iterate(ExprType type, Action<Int32, Expr> action) => Iterate(new MemberIterator(type), action);
    }

    public class InitExpr : Initr {
        public InitExpr(Expr expr)
            : base(Kind.EXPR) {
            this.expr = expr;
        }
        public readonly Expr expr;

        public override Initr ConformType(MemberIterator iter) {
            iter.Read(this.expr.type);
            Expr expr = TypeCast.MakeCast(this.expr, iter.CurType);
            return new InitExpr(expr);
        }

        public override void Iterate(MemberIterator iter, Action<Int32, Expr> action) {
            iter.Read(this.expr.type);
            Int32 offset = iter.CurOffset;
            ExprType type = iter.CurType;
            Expr expr = this.expr;
            action(offset, expr);
        }
    }

    public class InitList : Initr {
        public InitList(List<Initr> initrs) :
            base(Kind.INIT_LIST) {
            this.initrs = initrs;
        }
        public readonly List<Initr> initrs;

        public override Initr ConformType(MemberIterator iter) {
            iter.InBrace();
            List<Initr> initrs = new List<Initr>();
            for (Int32 i = 0; i < this.initrs.Count; ++i) {
                initrs.Add(this.initrs[i].ConformType(iter));
                if (i != this.initrs.Count - 1) {
                    iter.Next();
                }
            }
            iter.OutBrace();
            return new InitList(initrs);
        }

        public override void Iterate(MemberIterator iter, Action<Int32, Expr> action) {
            iter.InBrace();
            for (Int32 i = 0; i < initrs.Count; ++i) {
                initrs[i].Iterate(iter, action);
                if (i != initrs.Count - 1) {
                    iter.Next();
                }
            }
            iter.OutBrace();
        }
    }

    public class MemberIterator {
        public MemberIterator(ExprType type) {
            trace = new List<Status>();
            trace.Add(new Status(type));
        }

        public class Status {
            public Status(ExprType base_type) {
                this.base_type = base_type;
                indices = new List<Int32>();
            }

            public ExprType CurType { get { return GetType(base_type, indices); } }

            public Int32 CurOffset {
                get {
                    Int32 offset = 0;
                    ExprType type = base_type;
                    foreach (Int32 index in indices) {
                        switch (type.kind) {
                            case ExprType.Kind.ARRAY:
                                type = ((TArray)type).elem_type;
                                offset += index * type.SizeOf;
                                break;
                            case ExprType.Kind.STRUCT_OR_UNION:
                                type = ((TStructOrUnion)type).Attribs[index].type;
                                offset += ((TStructOrUnion)type).Attribs[index].offset;
                                break;
                            default:
                                throw new InvalidOperationException("Not an aggregate type.");
                        }
                    }
                    return offset;
                }
            }

            public ExprType GetType(ExprType base_type, IReadOnlyList<Int32> indices) {
                ExprType type = base_type;
                foreach (Int32 index in indices) {
                    switch (type.kind) {
                        case ExprType.Kind.ARRAY:
                            type = ((TArray)type).elem_type;
                            break;
                        case ExprType.Kind.STRUCT_OR_UNION:
                            type = ((TStructOrUnion)type).Attribs[index].type;
                            break;
                        default:
                            throw new InvalidOperationException("Not an aggregate type.");
                    }
                }
                return type;
            }

            public List<ExprType> GetTypes(ExprType base_type, IReadOnlyList<Int32> indices) {
                List<ExprType> types = new List<ExprType> { base_type };
                foreach (Int32 index in indices) {
                    switch (base_type.kind) {
                        case ExprType.Kind.ARRAY:
                            base_type = ((TArray)base_type).elem_type;
                            break;
                        case ExprType.Kind.STRUCT_OR_UNION:
                            base_type = ((TStructOrUnion)base_type).Attribs[index].type;
                            break;
                        default:
                            throw new InvalidOperationException("Not an aggregate type.");
                    }
                    types.Add(base_type);
                }
                return types;
            }

            public void Next() {
                List<ExprType> types = GetTypes(base_type, indices);
                do {
                    Int32 index = indices.Last();
                    indices.RemoveAt(indices.Count - 1);

                    types.RemoveAt(types.Count - 1);
                    ExprType type = types.Last();

                    switch (type.kind) {
                        case ExprType.Kind.ARRAY:
                            // TODO: what if incomplete?
                            if (index < ((TArray)type).num_elems - 1) {
                                indices.Add(index + 1);
                            }
                            return;
                        case ExprType.Kind.STRUCT_OR_UNION:
                            if (((TStructOrUnion)type).IsStruct && index < ((TStructOrUnion)type).Attribs.Count - 1) {
                                indices.Add(index + 1);
                            }
                            return;
                        default:
                            break;
                    }
                } while (true);
            }

            public void Read(ExprType type) {
                switch (type.kind) {
                    case ExprType.Kind.STRUCT_OR_UNION:
                        ReadStruct((TStructOrUnion)type);
                        return;
                    default:
                        if (type.IsScalar()) {
                            ReadScalar((ScalarType)type);
                            return;
                        }
                        throw new InvalidOperationException("Type not match.");
                }
            }

            private void ReadScalar(ScalarType type) {
                while (!CurType.IsScalar()) {
                    switch (CurType.kind) {
                        case ExprType.Kind.ARRAY:
                            indices.Add(0);
                            break;
                        case ExprType.Kind.STRUCT_OR_UNION:
                            indices.Add(0);
                            break;
                        default:
                            throw new InvalidOperationException("Cannot find matching struct.");
                    }
                }
                //if (!CurType.EqualType(type)) {
                //    throw new InvalidOperationException("Type not match.");
                //}
            }

            private void ReadStruct(TStructOrUnion type) {
                while (true) {
                    switch (CurType.kind) {
                        case ExprType.Kind.ARRAY:
                            indices.Add(0);
                            break;
                        case ExprType.Kind.STRUCT_OR_UNION:
                            if (CurType.EqualType(type)) {
                                return;
                            }
                            indices.Add(0);
                            break;
                        default:
                            throw new InvalidOperationException("Cannot find matching struct.");
                    }
                }
            }

            public readonly ExprType base_type;
            public readonly List<Int32> indices;
        }

        public ExprType CurType { get { return trace.Last().CurType; } }
        public Int32 CurOffset {
            get {
                return trace.Select(_ => _.CurOffset).Sum();
            }
        }
        public void Next() => trace.Last().Next();
        public void Read(ExprType type) => trace.Last().Read(type);
        public void InBrace() {
            trace.Add(new Status(trace.Last().CurType));
            switch (CurType.kind) {
                case ExprType.Kind.ARRAY:
                    trace.Last().indices.Add(0);
                    break;
                case ExprType.Kind.STRUCT_OR_UNION:
                    trace.Last().indices.Add(0);
                    break;
                default:
                    break;
                    // throw new InvalidOperationException("Invalid brace.");
            }
        }
        public void OutBrace() => trace.RemoveAt(trace.Count - 1);

        public readonly List<Status> trace;
    }
}
