using System;
using System.Collections.Generic;
using System.Linq;

namespace AST {
    /* From 3.1.2.5 Types (modified):
     
     * Types are partitioned into
       1) object types (types that describe objects)
       2) function types (types that describe functions)
       3) incomplete types (types that describe objects but lack information needed to determine their sizes).
     
     * [char] large enough to store any member of the basic execution character set.
       An ANSI character is positive.

     * There are 4 signed integer types:
       [signed char] < [short int] < [int] < [long int].

     * [signed char] occupies the same amount of storage as a "plain" char object.
     
     * [int] has the natural size suggested by the architecture of the execution environment.

     * For each of the signed integer types, there is a corresponding (but different) unsigned integer type (designated with the keyword unsigned) that uses the same amount of storage (including sign information) and has the same alignment requirements. The range of nonnegative values of a signed integer type is a subrange of the corresponding unsigned integer type, and the representation of the same value in each type is the same. A computation involving unsigned operands can never overflow, because a result that cannot be represented by the resulting unsigned integer type is reduced modulo the number that is one greater than the largest value that can be represented by the resulting unsigned integer type.

     * There are three floating types: float < double < long double.

     * The type char, the signed and unsigned integer types, and the floating types are collectively called the basic types.

     * There are three character types: char < signed char < unsigned char.

     * An enumeration comprises a set of named integer constant values. Each distinct enumeration constitutes a different enumerated type.

     * The void type comprises an empty set of values; it is an incomplete type that cannot be completed.

     * Any number of derived types can be constructed from the basic, enumerated, and incomplete types, as follows:

     * An array type describes a contiguously allocated set of objects with a particular member object type, called the element type. Array types are characterized by their element type and by the number of members of the array. An array type is said to be derived from its element type, and if its element type is T , the array type is sometimes called "array of T". The construction of an array type from an element type is called "array type derivation".

     * A structure type describes a sequentially allocated set of member objects, each of which has an optionally specified name and possibly distinct type.

     * A union type describes an overlapping set of member objects, each of which has an optionally specified name and possibly distinct type.

     * A function type describes a function with specified return type. A function type is characterized by its return type and the number and types of its parameters. A function type is said to be derived from its return type, and if its return type is T, the function type is sometimes called "function returning T". The construction of a function type from a return type is called "function type derivation".

     * A pointer type may be derived from a function type, an object type, or an incomplete type, called the referenced type. A pointer type describes an object whose value provides a reference to an entity of the referenced type. A pointer type derived from the referenced type T is sometimes called "pointer to T". The construction of a pointer type from a referenced type is called "pointer type derivation".

     * These methods of constructing derived types can be applied recursively.

     * <integral>   : [char], [signed/unsigned short/int/long], [enum]
     * <arithmetic> : <integral>, [float], [double]
     * <scalar>     : <arithmetic>, <pointer>
     * <aggregate> : <array>, <struct>, <union>

       function

       void

       array

       struct

       union

       scalar
         |
         +--- pointer
         |
         +--- arithmetic
                  |
                  +--- double
                  |
                  +--- float
                  |
                  +--- integral
                          |
                          +--- enum
                          |
                          +--- [signed/unsigned] long
                          |
                          +--- [signed/unsigned] short
                          |
                          +--- [signed/unsigned] char

     * A pointer to void shall have the same representation and alignment requirements as a pointer to a character type. Other pointer types need not have the same representation or alignment requirements.

     * An array type of unknown size is an incomplete type. It is completed, for an identifier of that type, by specifying the size in a later declaration (with internal or external linkage). A structure or union type of unknown content is an incomplete type. It is completed, for all declarations of that type, by declaring the same structure or union tag with its defining content later in the same scope.

     * Array, function, and pointer types are collectively called derived declarator types. A declarator type derivation from a type T is the construction of a derived declarator type from T by the application of an array, a function, or a pointer type derivation to T.
     
     */

    public abstract class ExprType {
        public enum Kind {
            VOID,
            CHAR,
            UCHAR,
            SHORT,
            USHORT,
            LONG,
            ULONG,
            FLOAT,
            DOUBLE,
            POINTER,
            FUNCTION,
            ARRAY,
            INCOMPLETE_ARRAY,
            STRUCT_OR_UNION,
        }

        public ExprType(Kind kind, Int32 size_of, Int32 alignment, Boolean is_const, Boolean is_volatile) {
            this.is_const = is_const;
            this.is_volatile = is_volatile;
            this.kind = kind;
            _size_of = size_of;
            _alignment = alignment;
        }

        public static Int32 SIZEOF_CHAR = 1;
        public static Int32 SIZEOF_SHORT = 2;
        public static Int32 SIZEOF_LONG = 4;
        public static Int32 SIZEOF_FLOAT = 4;
        public static Int32 SIZEOF_DOUBLE = 8;
        public static Int32 SIZEOF_POINTER = 4;

        public static Int32 ALIGN_CHAR = 1;
        public static Int32 ALIGN_SHORT = 2;
        public static Int32 ALIGN_LONG = 4;
        public static Int32 ALIGN_FLOAT = 4;
        public static Int32 ALIGN_DOUBLE = 4;
        public static Int32 ALIGN_POINTER = 4;

        public readonly Kind kind;
        public virtual Boolean IsArith => false;
        public virtual Boolean IsIntegral => false;
        public virtual Boolean IsScalar => false;
        public virtual Boolean IsComplete => true;
        public abstract Boolean EqualType(ExprType other);

        public String DumpQualifiers() {
            String str = "";
            if (is_const) {
                str += "const ";
            }
            if (is_volatile) {
                str += "volatile ";
            }
            return str;
        }

        public abstract ExprType GetQualifiedType(Boolean is_const, Boolean is_volatile);

        private Int32 _size_of;
        private Int32 _alignment;

        public virtual Int32 SizeOf { get { return _size_of; } }
        public virtual Int32 Alignment { get { return _alignment; } }

        public readonly Boolean is_const;
        public readonly Boolean is_volatile;

    }

    public class TVoid : ExprType {
        public TVoid(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(Kind.VOID, 0, 0, _is_const, _is_volatile) {
        }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TVoid(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "void";
        }
        public override Boolean EqualType(ExprType other) => other.kind == Kind.VOID;

    }

    public abstract class ScalarType : ExprType {
        public ScalarType(Kind _expr_type, Int32 _size_of, Int32 _alignment, Boolean _is_const, Boolean _is_volatile)
            : base(_expr_type, _size_of, _alignment, _is_const, _is_volatile) { }
        public override Boolean IsScalar => true;
    }

    public abstract class ArithmeticType : ScalarType {
        public ArithmeticType(Kind _expr_type, Int32 _size_of, Int32 _alignment, Boolean _is_const, Boolean _is_volatile)
            : base(_expr_type, _size_of, _alignment, _is_const, _is_volatile) { }
        public override Boolean IsArith => true;
        public override Boolean EqualType(ExprType other) {
            return kind == other.kind;
        }
    }

    public abstract class IntegralType : ArithmeticType {
        public IntegralType(Kind _expr_type, Int32 _size_of, Int32 _alignment, Boolean _is_const, Boolean _is_volatile)
            : base(_expr_type, _size_of, _alignment, _is_const, _is_volatile) { }
        public override Boolean IsIntegral => true;
    }

    public class TChar : IntegralType {
        public TChar(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(Kind.CHAR, SIZEOF_CHAR, ALIGN_CHAR, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TChar(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "char";
        }
    }

    public class TUChar : IntegralType {
        public TUChar(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(Kind.UCHAR, SIZEOF_CHAR, ALIGN_CHAR, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TUChar(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "unsigned char";
        }
    }

    public class TShort : IntegralType {
        public TShort(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(Kind.SHORT, SIZEOF_SHORT, ALIGN_SHORT, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TShort(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "short";
        }
    }

    public class TUShort : IntegralType {
        public TUShort(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(Kind.USHORT, SIZEOF_SHORT, ALIGN_SHORT, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TUShort(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "unsigned short";
        }
    }

    public class TLong : IntegralType {
        public TLong(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(Kind.LONG, SIZEOF_LONG, ALIGN_LONG, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TLong(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "long";
        }
    }

    public class TULong : IntegralType {
        public TULong(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(Kind.ULONG, SIZEOF_LONG, ALIGN_LONG, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TULong(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "unsigned long";
        }
    }

    public class TFloat : ArithmeticType {
        public TFloat(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(Kind.FLOAT, SIZEOF_FLOAT, ALIGN_FLOAT, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TFloat(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "float";
        }
    }

    public class TDouble : ArithmeticType {
        public TDouble(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(Kind.DOUBLE, SIZEOF_DOUBLE, ALIGN_DOUBLE, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TDouble(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "double";
        }
    }

    // class TPointer
    // ==============
    // 
    public class TPointer : ScalarType {
        public TPointer(ExprType _referenced_type, Boolean _is_const = false, Boolean _is_volatile = false)
            : base(Kind.POINTER, SIZEOF_POINTER, ALIGN_POINTER, _is_const, _is_volatile) {
            ref_t = _referenced_type;
        }
        public readonly ExprType ref_t;
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TPointer(ref_t, _is_const, _is_volatile);
        }
        public override Boolean EqualType(ExprType other) {
            return other.kind == Kind.POINTER && ((TPointer)other).ref_t.EqualType(ref_t);
        }
        public override String ToString() {
            return DumpQualifiers() + "ptr<" + ref_t.ToString() + ">";
        }
    }

    // Incomplete Array
    // ================
    // 
    public class TIncompleteArray : ExprType {
        public TIncompleteArray(ExprType elem_type, Boolean is_const = false, Boolean is_volatile = false)
            : base(Kind.INCOMPLETE_ARRAY, 0, elem_type.Alignment, is_const, is_volatile) {
            this.elem_type = elem_type;
        }

        public override ExprType GetQualifiedType(Boolean is_const, Boolean is_volatile) {
            return new TIncompleteArray(elem_type, is_const, is_volatile);
        }

        public override Boolean EqualType(ExprType other) => false;

        public override Boolean IsComplete => false;

        public ExprType Complete(Int32 num_elems) => new TArray(elem_type, num_elems, is_const, is_volatile);

        public override String ToString() {
            return elem_type.ToString() + "[]";
        }

        public readonly ExprType elem_type;
    }

    public class TArray : ExprType {
        public TArray(ExprType elem_type, Int32 num_elems, Boolean is_const = false, Boolean is_volatile = false)
            : base(Kind.ARRAY, elem_type.SizeOf * num_elems, elem_type.Alignment, is_const, is_volatile) {
            this.elem_type = elem_type;
            this.num_elems = num_elems;
        }

        public readonly ExprType elem_type;
        public readonly Int32    num_elems;

        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TArray(elem_type, num_elems, _is_const, _is_volatile);
        }

        public override Boolean EqualType(ExprType other) {
            return other.kind == Kind.ARRAY && ((TArray)other).elem_type.EqualType(elem_type);
        }

        public override String ToString() {
            return $"Arr[{num_elems}, {elem_type.ToString()}]";
        }

    }
    
    public class TStructOrUnion : ExprType {
        private TStructOrUnion(StructOrUnionLayout layout, Boolean is_const, Boolean is_volatile)
            : base(Kind.STRUCT_OR_UNION, 0, 0, is_const, is_volatile) {
            this.layout = layout;
        }

        public override ExprType GetQualifiedType(Boolean is_const, Boolean is_volatile) =>
            new TStructOrUnion(layout, is_const, is_volatile);

        public static TStructOrUnion CreateIncompleteStruct(String name, Boolean is_const, Boolean is_volatile) =>
            new TStructOrUnion(new StructOrUnionLayout($"struct {name}"), is_const, is_volatile);

        public static TStructOrUnion CreateIncompleteUnion(String name, Boolean is_const, Boolean is_volatile) =>
            new TStructOrUnion(new StructOrUnionLayout($"union {name}"), is_const, is_volatile);

        public static TStructOrUnion CreateStruct(String name, IReadOnlyList<Tuple<String, ExprType>> attribs, Boolean is_const, Boolean is_volatile) {
            StructOrUnionLayout layout = new StructOrUnionLayout($"struct {name}");
            layout.DefineStruct(attribs);
            return new TStructOrUnion(layout, is_const, is_volatile);
        }

        public static TStructOrUnion CreateUnion(String name, IReadOnlyList<Tuple<String, ExprType>> attribs, Boolean is_const, Boolean is_volatile) {
            StructOrUnionLayout layout = new StructOrUnionLayout($"union {name}");
            layout.DefineUnion(attribs);
            return new TStructOrUnion(layout, is_const, is_volatile);
        }

        public void DefineStruct(IReadOnlyList<Tuple<String, ExprType>> attribs) => layout.DefineStruct(attribs);

        public void DefineUnion(IReadOnlyList<Tuple<String, ExprType>> attribs) => layout.DefineUnion(attribs);

        public String Dump(Boolean dump_attribs) {
            if (!IsComplete) {
                return "incompleted type " + layout.TypeName;
            } else {
                String str = layout.TypeName + " (size = " + SizeOf + ")";
                if (dump_attribs) {
                    str += "\n";
                    foreach (Utils.StoreEntry attrib in layout.attribs) {
                        str += "  [base + " + attrib.offset.ToString() + "] " + attrib.name + " : " + attrib.type.ToString() + "\n";
                    }
                }
                return str;
            }
        }

        public override String ToString() {
            return Dump(false);
            //String str = DumpQualifiers() + layout.typename + " { ";
            //foreach (Utils.StoreEntry attrib in layout.attribs) {
            //    str += attrib.name + " : " + attrib.type.ToString() + "; ";
            //}
            //str += "}";
            //return str;
        }

        public override Boolean EqualType(ExprType other) =>
            other.kind == Kind.STRUCT_OR_UNION && ReferenceEquals(((TStructOrUnion)other).layout, layout);

        public override Boolean IsComplete => layout.IsComplete;

        public override Int32 SizeOf { get { return layout.SizeOf; } }

        public override Int32 Alignment { get { return layout.Alignment; } }

        public Boolean IsStruct { get { return layout.IsStruct; } }

        public IReadOnlyList<Utils.StoreEntry> Attribs { get { return layout.attribs; } }

        private readonly StructOrUnionLayout layout;

        private class StructOrUnionLayout {
            public StructOrUnionLayout(String typename) {
                _attribs = null;
                _size_of = 0;
                _alignment = 0;
                _typename = typename;
            }

            public void DefineStruct(IReadOnlyList<Tuple<String, ExprType>> attribs) {
                if (IsComplete) {
                    throw new InvalidOperationException("Redefining a struct.");
                }

                _attribs = new List<Utils.StoreEntry>();
                Int32 offset = 0;
                Int32 struct_alignment = 0;
                foreach (Tuple<String, ExprType> attrib in attribs) {
                    String name = attrib.Item1;
                    ExprType type = attrib.Item2;

                    Int32 attrib_alignment = type.Alignment;

                    // All attributes must be aligned.
                    // This means that the alignment of the struct is the largest attribute alignment.
                    struct_alignment = Math.Max(struct_alignment, attrib_alignment);

                    // Make sure all attributes are put into aligned places.
                    offset = Utils.RoundUp(offset, attrib_alignment);

                    _attribs.Add(new Utils.StoreEntry(name, type, offset));

                    offset += type.SizeOf;
                }

                _size_of = Utils.RoundUp(offset, struct_alignment);
                _alignment = struct_alignment;
            }

            public void DefineUnion(IReadOnlyList<Tuple<String, ExprType>> attribs) {
                if (IsComplete) {
                    throw new InvalidOperationException("Redefining a union.");
                }

                _attribs = attribs
                    .Select(_ => new Utils.StoreEntry(_.Item1, _.Item2, 0))
                    .ToList();
            }

            public IReadOnlyList<Utils.StoreEntry> attribs { get { return _attribs; } }

            public Boolean IsStruct { get { return TypeName.StartsWith("struct"); } }

            public Boolean IsComplete { get { return _attribs != null; } }

            public Int32 SizeOf {
                get {
                    if (IsComplete) {
                        return _size_of;
                    } else {
                        throw new InvalidOperationException("An incomplete type. Cannot get size.");
                    }
                }
            }

            public Int32 Alignment { get { return _alignment; } }

            public String TypeName { get { return _typename; } }

            /// <summary>
            /// Private records of all the attribs with their names, types, and offsets.
            /// </summary>
            private List<Utils.StoreEntry> _attribs;

            /// <summary>
            /// _size_of and _alignment can only be changed by defining the layout.
            /// </summary>
            private Int32 _size_of;
            private Int32 _alignment;

            private String _typename;
        }
    }

    // class TFunction
    // ===============
    // represents the function type
    // stores the names, types, and offsets of arguments
    // 
    // calling convention:
    // https://developer.apple.com/library/mac/documentation/DeveloperTools/Conceptual/LowLevelABI/130-IA-32_Function_Calling_Conventions/IA32.html
    // 
    public class TFunction : ExprType {
        protected TFunction(ExprType ret_type, List<Utils.StoreEntry> args, Boolean is_varargs)
            : base(Kind.FUNCTION, SIZEOF_POINTER, SIZEOF_POINTER, true, false) {
            this.args = args;
            this.ret_type = ret_type;
            this.is_varargs = is_varargs;
        }

        public override ExprType GetQualifiedType(Boolean is_const, Boolean is_volatile) {
            return new TFunction(ret_type, args, is_varargs);
        }

        public override Boolean EqualType(ExprType other) {
            throw new NotImplementedException();
        }

        public static TFunction Create(ExprType ret_type, List<Tuple<String, ExprType>> _args, Boolean is_varargs) {
            Tuple<Int32, IReadOnlyList<Int32>> r_pack = Utils.PackArguments(_args.ConvertAll(_ => _.Item2));
            IReadOnlyList<Int32> offsets = r_pack.Item2;
            if (ret_type is TStructOrUnion) {
                offsets = offsets.Select(_ => _ + 3 * SIZEOF_POINTER).ToList();
            } else {
                offsets = offsets.Select(_ => _ + 2 * SIZEOF_POINTER).ToList();
            }
            return new TFunction(
                ret_type,
                Enumerable.Zip(
                    _args,
                    offsets,
                    (name_type, offset) => new Utils.StoreEntry(name_type.Item1, name_type.Item2, offset)
                ).ToList(),
                is_varargs
            );
        }

        public String Dump(Boolean dump_args = false) {
            String str = "function";
            if (dump_args) {
                str += "\n";
                foreach (Utils.StoreEntry arg in args) {
                    str += $"  [%ebp + {arg.offset}] {arg.name} : {arg.type}\n";
                }
            }
            return str;
        }

        public override String ToString() {
            String str = "";
            for (Int32 i = 0; i < args.Count; ++i) {
                if (i != 0) {
                    str += ", ";
                }
                str += args[i].type.ToString();
            }
            if (args.Count > 0) {
                str = "(" + str + ")";
            }
            return str + " -> " + ret_type;
        }

        public readonly Boolean                is_varargs;
        public readonly ExprType               ret_type;
        public readonly List<Utils.StoreEntry> args;
        public readonly Int32                  arg_size;
    }

    // class TEmptyFunction
    // ====================
    // defines an empty function: no arguments, returns void
    // 
    public class TEmptyFunction : TFunction {
        public TEmptyFunction() : base(new TVoid(), new List<Utils.StoreEntry>(), false) {
        }
    }

}
