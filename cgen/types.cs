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

    public class ExprType {
        public enum ExprTypeKind {
            ERROR,
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
            STRUCT,
            INCOMPLETE_STRUCT,
            UNION,
            INCOMPLETE_UNION,
            FUNCTION,
            ARRAY,
            INCOMPLETE_ARRAY,
            INIT_LIST,
        }

        public ExprType(ExprTypeKind _expr_type, Int32 _size_of, Int32 _alignment, Boolean _is_const, Boolean _is_volatile) {
            is_const = _is_const;
            is_volatile = _is_volatile;
            expr_type = _expr_type;
            size_of = _size_of;
            alignment = _alignment;
        }

        public static ExprType CreateInitList() {
            return new ExprType(ExprTypeKind.INIT_LIST, 0, 0, true, false);
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

        public readonly ExprTypeKind expr_type;
        public virtual Boolean IsArith() { return false; }
        public virtual Boolean IsIntegral() { return false; }
        public virtual Boolean IsScalar() { return false; }
        public virtual Boolean EqualType(ExprType other) { return false; }
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

        public virtual ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return null;
        }

        public Int32 SizeOf {
            get { return size_of; }
        }
        public Int32 Alignment {
            get { return alignment; }
        }
        protected Int32 size_of;
        protected Int32 alignment;
        public readonly Boolean is_const;
        public readonly Boolean is_volatile;

    }

    public class TVoid : ExprType {
        public TVoid(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(ExprTypeKind.VOID, 0, 0, _is_const, _is_volatile) {
        }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TVoid(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "void";
        }
    }

    public abstract class ScalarType : ExprType {
        public ScalarType(ExprTypeKind _expr_type, Int32 _size_of, Int32 _alignment, Boolean _is_const, Boolean _is_volatile)
            : base(_expr_type, _size_of, _alignment, _is_const, _is_volatile) { }
        public override Boolean IsScalar() {
            return true;
        }
    }

    public abstract class ArithmeticType : ScalarType {
        public ArithmeticType(ExprTypeKind _expr_type, Int32 _size_of, Int32 _alignment, Boolean _is_const, Boolean _is_volatile)
            : base(_expr_type, _size_of, _alignment, _is_const, _is_volatile) { }
        public override Boolean IsArith() {
            return true;
        }
        public override Boolean EqualType(ExprType other) {
            return expr_type == other.expr_type;
        }
    }

    public abstract class IntegralType : ArithmeticType {
        public IntegralType(ExprTypeKind _expr_type, Int32 _size_of, Int32 _alignment, Boolean _is_const, Boolean _is_volatile)
            : base(_expr_type, _size_of, _alignment, _is_const, _is_volatile) { }
        public override Boolean IsIntegral() {
            return true;
        }
    }

    public class TChar : IntegralType {
        public TChar(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(ExprTypeKind.CHAR, SIZEOF_CHAR, ALIGN_CHAR, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TChar(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "char";
        }
    }

    public class TUChar : IntegralType {
        public TUChar(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(ExprTypeKind.UCHAR, SIZEOF_CHAR, ALIGN_CHAR, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TUChar(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "unsigned char";
        }
    }

    public class TShort : IntegralType {
        public TShort(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(ExprTypeKind.SHORT, SIZEOF_SHORT, ALIGN_SHORT, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TShort(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "short";
        }
    }

    public class TUShort : IntegralType {
        public TUShort(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(ExprTypeKind.USHORT, SIZEOF_SHORT, ALIGN_SHORT, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TUShort(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "unsigned short";
        }
    }

    public class TLong : IntegralType {
        public TLong(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(ExprTypeKind.LONG, SIZEOF_LONG, ALIGN_LONG, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TLong(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "long";
        }
    }

    public class TULong : IntegralType {
        public TULong(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(ExprTypeKind.ULONG, SIZEOF_LONG, ALIGN_LONG, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TULong(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "unsigned long";
        }
    }

    public class TFloat : ArithmeticType {
        public TFloat(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(ExprTypeKind.FLOAT, SIZEOF_FLOAT, ALIGN_FLOAT, _is_const, _is_volatile) { }
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TFloat(_is_const, _is_volatile);
        }
        public override String ToString() {
            return DumpQualifiers() + "float";
        }
    }

    public class TDouble : ArithmeticType {
        public TDouble(Boolean _is_const = false, Boolean _is_volatile = false)
            : base(ExprTypeKind.DOUBLE, SIZEOF_DOUBLE, ALIGN_DOUBLE, _is_const, _is_volatile) { }
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
            : base(ExprTypeKind.POINTER, SIZEOF_POINTER, ALIGN_POINTER, _is_const, _is_volatile) {
            referenced_type = _referenced_type;
        }
        public readonly ExprType referenced_type;
        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TPointer(referenced_type, _is_const, _is_volatile);
        }
        public override Boolean EqualType(ExprType other) {
            return other.expr_type == ExprTypeKind.POINTER && ((TPointer)other).referenced_type.EqualType(referenced_type);
        }
        public override String ToString() {
            return DumpQualifiers() + "ptr<" + referenced_type.ToString() + ">";
        }
    }

    // Incomplete Array
    // ================
    // 
    public class TIncompleteArray : ExprType {
        public TIncompleteArray(ExprType _elem_type, Boolean _is_const = false, Boolean _is_volatile = false)
            : base(ExprTypeKind.ARRAY, 0, _elem_type.Alignment, _is_const, _is_volatile) {
            array_elem_type = _elem_type;
        }

        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TIncompleteArray(array_elem_type, _is_const, _is_volatile);
        }

        public override Boolean EqualType(ExprType other) {
            return base.EqualType(other);
        }

        public override String ToString() {
            return array_elem_type.ToString() + "[]";
        }

        public readonly ExprType array_elem_type;
    }

    public class TArray : ExprType {
        public TArray(ExprType _elem_type, Int32 _nelems, Boolean _is_const = false, Boolean _is_volatile = false)
            : base(ExprTypeKind.ARRAY, _elem_type.SizeOf * _nelems, _elem_type.Alignment, _is_const, _is_volatile) {
            array_elem_type = _elem_type;
            array_nelems = _nelems;
        }

        public readonly ExprType array_elem_type;
        public readonly Int32    array_nelems;

        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TArray(array_elem_type, array_nelems, _is_const, _is_volatile);
        }

        public override Boolean EqualType(ExprType other) {
            return other.expr_type == ExprTypeKind.ARRAY && ((TArray)other).array_elem_type.EqualType(array_elem_type);
        }

        public override String ToString() {
            return "arr<" + array_nelems + ", " + array_elem_type.ToString() + ">";
        }

    }
    
	public class TIncompleteStruct : ExprType {
		public TIncompleteStruct(String _name, Boolean _is_const = false, Boolean _is_volatile = false)
			: base(ExprTypeKind.INCOMPLETE_STRUCT, 0, 0, _is_const, _is_volatile) {
			struct_name = _name;
		}
		public readonly String struct_name;
	}

    // class TStruct
    // =============
    // represets the structure
    // stores the names, types, and offsets of attributes
    // 
    public class TStruct : ExprType {
        private TStruct(List<Utils.StoreEntry> _attribs, Int32 _size_of, Int32 _alignment, Boolean _is_const, Boolean _is_volatile)
            : base(ExprTypeKind.STRUCT, _size_of, _alignment, _is_const, _is_volatile) {
            attribs = _attribs;
        }

        public static TStruct Create(List<Tuple<String, ExprType>> _attribs, Boolean _is_const = false, Boolean _is_volatile = false) {
            List<Utils.StoreEntry> attribs = new List<Utils.StoreEntry>();
            Int32 offset = 0;
            Int32 alignment = 0;
            foreach (Tuple<String, ExprType> _attrib in _attribs) {
                Int32 curr_align = _attrib.Item2.Alignment;
                if (curr_align > alignment) {
                    alignment = curr_align;
                }
                offset = Utils.RoundUp(offset, curr_align);
                attribs.Add(new Utils.StoreEntry(_attrib.Item1, _attrib.Item2, offset));
                offset += _attrib.Item2.SizeOf;
            }
            offset = Utils.RoundUp(offset, alignment);
            return new TStruct(attribs, offset, alignment, _is_const, _is_volatile);
        }
        
        public String Dump(Boolean dump_attribs) {
            String str = "struct (size = " + SizeOf + ")";
            if (dump_attribs) {
                str += "\n";
                foreach (Utils.StoreEntry attrib in attribs) {
                    str += "  [base + " + attrib.entry_offset.ToString() + "] " + attrib.entry_name + " : " + attrib.entry_type.ToString() + "\n";
                }
            }
            return str;
        }

        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TStruct(attribs, size_of, alignment, _is_const, _is_volatile);
        }

        public override String ToString() {
            String str = DumpQualifiers() + "struct { ";
            foreach (Utils.StoreEntry attrib in attribs) {
                str += attrib.entry_name + " : " + attrib.entry_type.ToString() + "; ";
            }
            str += "}";
            return str;
        }

        public readonly List<Utils.StoreEntry> attribs;
    }


    // class TUnion
    // ============
    // represents the union type
    // stores the names and types of attributes
    // 
    public class TUnion : ExprType {
        public TUnion(List<Tuple<String, ExprType>> _attribs, Int32 _size_of, Boolean _is_const = false, Boolean _is_volatile = false)
            : base(ExprTypeKind.UNION, _size_of, _size_of, _is_const, _is_volatile) {
            attribs = _attribs;
        }

        public static TUnion Create(List<Tuple<String, ExprType>> _attribs, Boolean _is_const = false, Boolean _is_volatile = false) {
            Int32 size_of;
            if (_attribs.Count != 0) {
                size_of = _attribs.Max(x => x.Item2.SizeOf);
            } else {
                size_of = 0;
            }
            return new TUnion(_attribs, size_of, _is_const, _is_volatile);
        }

        public override ExprType GetQualifiedType(Boolean _is_const, Boolean _is_volatile) {
            return new TUnion(attribs, SizeOf, _is_const, _is_volatile);
        }

        public String Dump(Boolean dump_attribs) {
            String str = "union (size = " + SizeOf + ")";
            if (dump_attribs) {
                str += "\n";
                foreach (Tuple<String, ExprType> attrib in attribs) {
                    str += "  " + attrib.Item1 + " : " + attrib.Item2.ToString() + "\n";
                }
            }
            return str;
        }

        public override String ToString() {
            String str = DumpQualifiers() + "union { ";
            foreach (Tuple<String, ExprType> attrib in attribs) {
                str += attrib.Item1 + " : " + attrib.Item2.ToString() + "; ";
            }
            str += "}";
            return str;
        }

        public readonly List<Tuple<String, ExprType>> attribs;
    }

	public class TIncompleteUnion : ExprType {
		public TIncompleteUnion(String _name, Boolean _is_const = false, Boolean _is_volatile = false)
			: base(ExprTypeKind.INCOMPLETE_UNION, 0, 0, _is_const, _is_volatile) {
			union_name = _name;
		}
		public readonly String union_name;
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
        public TFunction(ExprType _ret_type, List<Utils.StoreEntry> _args, Int32 _size_of, Int32 _alignment, Boolean _varargs)
            : base(ExprTypeKind.FUNCTION, _size_of, _alignment, true, false) {
            args = _args;
            ret_type = _ret_type;
            varargs = _varargs;
        }
        public static TFunction Create(ExprType _ret_type, List<Tuple<String, ExprType>> _args, Boolean _varargs) {
            List<Utils.StoreEntry> args = new List<Utils.StoreEntry>();
            Int32 regsz = SIZEOF_LONG; // 32-bit machine: Int32 = 4 bytes
            Int32 offset = 2 * regsz;  // first parameter should be at %ebp + 8
            Int32 alignment = regsz;
            foreach (Tuple<String, ExprType> arg in _args) {
                args.Add(new Utils.StoreEntry(arg.Item1, arg.Item2, offset));
                offset += arg.Item2.SizeOf;

                // even though the args are a bunch of chars, they still need to be 4-byte aligned.
                Int32 curr_align = Math.Max(regsz, arg.Item2.Alignment);
                offset = (offset + curr_align - 1) & ~(curr_align - 1);

                if (curr_align > alignment) {
                    alignment = curr_align;
                }
            }

            return new TFunction(_ret_type, args, offset, alignment, _varargs);
        }

        public String Dump(Boolean dump_args = false) {
            String str = "function";
            if (dump_args) {
                str += "\n";
                foreach (Utils.StoreEntry arg in args) {
                    str += "  [%ebp + " + arg.entry_offset.ToString() + "] " + arg.entry_name + " : " + arg.entry_type.ToString() + "\n";
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
                str += args[i].entry_type.ToString();
            }
            if (args.Count > 0) {
                str = "(" + str + ")";
            }
            return str + " -> " + ret_type;
        }

        public readonly Boolean                varargs;
        public readonly ExprType               ret_type;
        public readonly List<Utils.StoreEntry> args;
    }

    // class TEmptyFunction
    // ====================
    // defines an empty function: no arguments, returns void
    // 
    public class TEmptyFunction : TFunction {
        public TEmptyFunction() : base(new TVoid(), new List<Utils.StoreEntry>(), 0, 0, false) {
        }
    }

}
