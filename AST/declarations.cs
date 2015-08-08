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
            this.scs  = scs;
            this.type = type;
            this.initr = initr;
        }

        public override String ToString() {
            String str = "[" + scs.ToString() + "] ";
            str += name;
            str += " : " + type.ToString();
            return str;
        }

        public void CGenExternDecln(Env env, CGenState state) {
            state.CGenExpandStackTo(env.GetStackOffset(), ToString());
            //if (initr.type.kind != ExprType.Kind.VOID) {
            //    // need initialization

            //    Env.Entry entry = env.Find(name);
            //    switch (entry.kind) {
            //    case Env.EntryKind.STACK:
            //        // %eax = <decln_init>
            //        initr.CGenValue(env, state);

            //        // -<offset>(%ebp) = %eax
            //        state.MOVL(Reg.EAX, -entry.offset, Reg.EBP);

            //        break;
            //    case Env.EntryKind.GLOBAL:
            //        // TODO : extern decln global
            //        break;
            //    case Env.EntryKind.ENUM:
            //    case Env.EntryKind.FRAME:
            //    case Env.EntryKind.NOT_FOUND:
            //    case Env.EntryKind.TYPEDEF:
            //    default:
            //        throw new NotImplementedException();
            //    }


            //}
        }

        public void What(ExprType type, Initr initr) {
            switch (type.kind) {
                case ExprType.Kind.ARRAY:
                    throw new NotImplementedException();
            }
        }

        private readonly String         name;
        private readonly SCS            scs;
        private readonly ExprType       type;
        private readonly Option<Initr>  initr;
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
    public class Initr {
        public Initr(Kind kind) {
            this.kind = kind;
        }
        public enum Kind {
            EXPR,
            INIT_LIST,
        }
        public readonly Kind kind;
    }

    public class InitExpr : Initr {
        public InitExpr(Expr expr)
            : base(Kind.EXPR) {
            this.expr = expr;
        }
        public readonly Expr expr;
    }

    public class InitList : Initr {
        public InitList(List<Initr> initrs) :
            base(Kind.INIT_LIST) {
            this.initrs = initrs;
        }
        public readonly List<Initr> initrs;
    }

    public class MemberIterator {
        public class Record {
            public Record(ExprType type, Int32 idx) {
                this.type = type;
                this.idx = idx;
            }
            public readonly ExprType type;
            public readonly Int32 idx;
        }

        public MemberIterator(ExprType type) {
            _records = new List<Record>();
            _records.Add(new Record(type, 0));
            _brace_level = 0;
        }

        public void InBrace() {
            Record record;
            switch (cur_type.kind) {
                case ExprType.Kind.ARRAY:
                    record = new Record(((TArray)cur_type).elem_type, 0);
                    break;
                case ExprType.Kind.STRUCT_OR_UNION:
                    record = new Record(((TStructOrUnion)cur_type).attribs[0].type, 0);
                    break;
                default:
                    throw new InvalidProgramException();
            }
            _records.Add(record);
            _brace_level++;
        }

        public void OutBrace() {
            Record cur_record = records.Last();
            _records.RemoveAt(_records.Count - 1);

        }

        private Int32 _brace_level;
        public Int32 brace_level { get { return _brace_level; } }

        private readonly List<Record> _records;
        public IReadOnlyList<Record> records { get { return _records; } }

        public ExprType cur_type { get { return records.Last().type; } }

        public Int32 cur_idx { get { return records.Last().idx; } }
    }
}
