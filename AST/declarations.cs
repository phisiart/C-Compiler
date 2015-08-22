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
                    Initr initr = this.initr.Value;
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
                Int32 pos = env.Find(name).Value.offset;
                if (this.initr.IsNone) {
                    return;
                }

                Initr initr = this.initr.Value;
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
        public Initr() { }
        public enum Kind {
            EXPR,
            INIT_LIST,
        }
        public abstract Kind kind { get; }

        public abstract Initr ConformType(MemberIterator iter);

        public Initr ConformType(ExprType type) => ConformType(new MemberIterator(type));

        public abstract void Iterate(MemberIterator iter, Action<Int32, Expr> action);

        public void Iterate(ExprType type, Action<Int32, Expr> action) => Iterate(new MemberIterator(type), action);
    }

    public class InitExpr : Initr {
        public InitExpr(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;
        public override Kind kind => Kind.EXPR;

        public override Initr ConformType(MemberIterator iter) {
            iter.Locate(this.expr.type);
            Expr expr = TypeCast.MakeCast(this.expr, iter.CurType);
            return new InitExpr(expr);
        }

        public override void Iterate(MemberIterator iter, Action<Int32, Expr> action) {
            iter.Locate(this.expr.type);
            Int32 offset = iter.CurOffset;
            Expr expr = this.expr;
            action(offset, expr);
        }
    }

    public class InitList : Initr {
        public InitList(List<Initr> initrs) {
            this.initrs = initrs;
        }
        public override Kind kind => Kind.INIT_LIST;
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
            trace = new List<Status> { new Status(type) };
        }

        public class Status {
            public Status(ExprType base_type) {
                this.base_type = base_type;
                indices = new List<Int32>();
            }

            public ExprType CurType => GetType(base_type, indices);

            public Int32 CurOffset => GetOffset(base_type, indices);

            //public List<Tuple<ExprType, Int32>> GetPath(ExprType base_type, IReadOnlyList<Int32> indices) {
            //    ExprType type = base_type;
            //    List<Tuple<ExprType, Int32>> path = new List<Tuple<ExprType, int>>();
            //    foreach (Int32 index in indices) {
            //        switch (type.kind) {
            //            case ExprType.Kind.ARRAY:
            //                type = ((TArray)type).elem_type;
            //                break;
            //            case ExprType.Kind.INCOMPLETE_ARRAY:
            //            case ExprType.Kind.STRUCT_OR_UNION:
            //            default:
            //                throw new InvalidProgramException("Not an aggregate type.");
            //        }
            //    }
            //}

            public static ExprType GetType(ExprType from_type, Int32 to_index) {
                switch (from_type.kind) {
                    case ExprType.Kind.ARRAY:
                        return ((TArray)from_type).elem_type;

                    case ExprType.Kind.INCOMPLETE_ARRAY:
                        return ((TIncompleteArray)from_type).elem_type;

                    case ExprType.Kind.STRUCT_OR_UNION:
                        return ((TStructOrUnion)from_type).Attribs[to_index].type;

                    default:
                        throw new InvalidProgramException("Not an aggregate type.");
                }
            }

            public static ExprType GetType(ExprType base_type, IReadOnlyList<Int32> indices) =>
                indices.Aggregate(base_type, GetType);

            public static Int32 GetOffset(ExprType from_type, Int32 to_index) {
                switch (from_type.kind) {
                    case ExprType.Kind.ARRAY:
                        return to_index * ((TArray)from_type).elem_type.SizeOf;

                    case ExprType.Kind.INCOMPLETE_ARRAY:
                        return to_index * ((TIncompleteArray)from_type).elem_type.SizeOf;

                    case ExprType.Kind.STRUCT_OR_UNION:
                        return ((TStructOrUnion)from_type).Attribs[to_index].offset;

                    default:
                        throw new InvalidProgramException("Not an aggregate type.");
                }
            }

            public static Int32 GetOffset(ExprType base_type, IReadOnlyList<Int32> indices) {
                Int32 offset = 0;
                ExprType from_type = base_type;
                foreach (Int32 to_index in indices) {
                    offset += GetOffset(from_type, to_index);
                    from_type = GetType(from_type, to_index);
                }
                return offset;
            }

            public List<ExprType> GetTypes(ExprType base_type, IReadOnlyList<Int32> indices) {
                List<ExprType> types = new List<ExprType> { base_type };
                ExprType from_type = base_type;
                foreach (Int32 to_index in indices) {
                    from_type = GetType(from_type, to_index);
                    types.Add(from_type);
                }
                return types;
            }

            public void Next() {

                // From base_type to CurType.
                List<ExprType> types = GetTypes(base_type, indices);

                // We try to jump as many levels out as we can.
                do {
                    Int32 index = indices.Last();
                    indices.RemoveAt(indices.Count - 1);

                    types.RemoveAt(types.Count - 1);
                    ExprType type = types.Last();

                    switch (type.kind) {
                        case ExprType.Kind.ARRAY:
                            if (index < ((TArray)type).num_elems - 1) {
                                // There are more elements in the array.
                                indices.Add(index + 1);
                                return;
                            }
                            break;

                        case ExprType.Kind.INCOMPLETE_ARRAY:
                            indices.Add(index + 1);
                            return;

                        case ExprType.Kind.STRUCT_OR_UNION:
                            if (((TStructOrUnion)type).IsStruct && index < ((TStructOrUnion)type).Attribs.Count - 1) {
                                // There are more members in the struct.
                                // (not union, since we can only initialize the first member of a union)
                                indices.Add(index + 1);
                                return;
                            }
                            break;

                        default:
                            break;
                    }

                } while (indices.Any());
            }

            /// <summary>
            /// Read an expression in the initializer list, locate the corresponding position.
            /// </summary>
            public void Locate(ExprType type) {
                switch (type.kind) {
                    case ExprType.Kind.STRUCT_OR_UNION:
                        LocateStruct((TStructOrUnion)type);
                        return;
                    default:
                        // Even if the expression is of array type, treat it as a scalar (pointer).
                        LocateScalar();
                        return;
                }
            }

            /// <summary>
            /// Try to match a scalar.
            /// This step doesn't check what scalar it is. Further steps would perform implicit conversions.
            /// </summary>
            private void LocateScalar() {
                while (!CurType.IsScalar) {
                    indices.Add(0);
                }
            }

            /// <summary>
            /// Try to match a given struct.
            /// Go down to find the first element of the same struct type.
            /// </summary>
            private void LocateStruct(TStructOrUnion type) {
                while (!CurType.EqualType(type)) {
                    if (CurType.IsScalar) {
                        throw new InvalidOperationException("Trying to match a struct or union, but found a scalar.");
                    }

                    // Go down one level.
                    indices.Add(0);
                }
            }

            public readonly ExprType base_type;
            public readonly List<Int32> indices;
        }

        public ExprType CurType => trace.Last().CurType;

        public Int32 CurOffset => trace.Select(_ => _.CurOffset).Sum();

        public void Next() => trace.Last().Next();

        public void Locate(ExprType type) => trace.Last().Locate(type);

        public void InBrace() {

            /// Push the current position into the stack, so that we can get back by <see cref="OutBrace"/>
            trace.Add(new Status(trace.Last().CurType));

            // For aggregate types, go inside and locate the first member.
            if (!CurType.IsScalar) {
                trace.Last().indices.Add(0);
            }
            
        }

        public void OutBrace() => trace.RemoveAt(trace.Count - 1);

        public readonly List<Status> trace;
    }
}
