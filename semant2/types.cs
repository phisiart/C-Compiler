using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST {
    /* From 3.1.2.5 Types (modified):
     
     * Types are partitioned into 1) object types (types that describe objects), 2) function types (types that describe functions), and 3) incomplete types (types that describe objects but lack information needed to determine their sizes).
     
     * An object declared as type char is large enough to store any member of the basic execution character set. An ANSI character is positive.

     * There are four signed integer types: signed char < short int < int < long int.

     * An object declared as type signed char occupies the same amount of storage as a "plain" char object. A "plain" int object has the natural size suggested by the architecture of the execution environment.

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

     * The type char, the signed and unsigned integer types, and the enumerated types are collectively called integral types.

     * Integral and floating types are collectively called arithmetic types. Arithmetic types and pointer types are collectively called scalar types. Array and structure types are collectively called aggregate types.

     * A pointer to void shall have the same representation and alignment requirements as a pointer to a character type. Other pointer types need not have the same representation or alignment requirements.

     * An array type of unknown size is an incomplete type. It is completed, for an identifier of that type, by specifying the size in a later declaration (with internal or external linkage). A structure or union type of unknown content is an incomplete type. It is completed, for all declarations of that type, by declaring the same structure or union tag with its defining content later in the same scope.

     * Array, function, and pointer types are collectively called derived declarator types. A declarator type derivation from a type T is the construction of a derived declarator type from T by the application of an array, a function, or a pointer type derivation to T.
     
     */

    public class ExprType {
        public enum EnumExprType {
            ERROR,
            CHAR,
            //SCHAR,
            UCHAR,
            SHORT,
            USHORT,
            LONG,
            ULONG,
            FLOAT,
            DOUBLE,
            POINTER,
            STRUCT,
            UNION,
        }

        public ExprType(EnumExprType _expr_type = EnumExprType.ERROR, bool _is_const = false, bool _is_volatile = false) {
            is_const = _is_const;
            is_volatile = _is_volatile;
            expr_type = EnumExprType.ERROR;
            size_of = 0;
        }

        public readonly EnumExprType expr_type;
        public virtual bool IsArith()                 { return false; }
        public virtual bool IsIntegral()              { return false; }
        public virtual bool EqualType(ExprType other) { return false; }
        // public virtual int  SizeOf()                  { return 0; }
        public int SizeOf {
            get { return size_of; }
        }
        protected int size_of;
        public readonly bool is_const;
        public readonly bool is_volatile;
        
    }
    
    public class ArithmeticType : ExprType {
        public ArithmeticType(EnumExprType _expr_type = EnumExprType.ERROR, bool _is_const = false, bool _is_volatile = false)
            : base(_expr_type, _is_const, _is_volatile) {
        }
        public override bool IsArith() {
            return true;
        }
        public override bool EqualType(ExprType other) {
 	         return expr_type == other.expr_type;
        }
    }

    public class IntegralType : ArithmeticType {
        public IntegralType(EnumExprType _expr_type = EnumExprType.ERROR, bool _is_const = false, bool _is_volatile = false)
            : base(_expr_type, _is_const, _is_volatile) {
        }
        public override bool IsIntegral() {
            return true;
        }
    }

    public class TChar : IntegralType {
        public TChar(bool _is_const = false, bool _is_volatile = false)
            : base(EnumExprType.CHAR, _is_const, _is_volatile) {
            size_of = 1;
        }
    }

    //public class TSChar : IntegralType {
    //    public TSChar(bool _is_const = false, bool _is_volatile = false)
    //        : base(EnumExprType.SCHAR, _is_const, _is_volatile) {}
    //}
    
    public class TUChar : IntegralType {
        public TUChar(bool _is_const = false, bool _is_volatile = false)
            : base(EnumExprType.UCHAR, _is_const, _is_volatile) {
            size_of = 1;
        }
    }

    public class TShort : IntegralType {
        public TShort(bool _is_const = false, bool _is_volatile = false)
            : base(EnumExprType.SHORT, _is_const, _is_volatile) {
            size_of = 2;
        }
    }

    public class TUShort : IntegralType {
        public TUShort(bool _is_const = false, bool _is_volatile = false)
            : base(EnumExprType.USHORT, _is_const, _is_volatile) {
            size_of = 2;
        }
    }

    //public class TInt : IntegralType {
    //    public TInt(bool _is_const = false, bool _is_volatile = false)
    //        : base(EnumExprType.INT, _is_const, _is_volatile) {}
    //}

    //public class TUInt : IntegralType {
    //    public TUInt(bool _is_const = false, bool _is_volatile = false)
    //        : base(EnumExprType.UINT, _is_const, _is_volatile) {}
    //}

    public class TLong : IntegralType {
        public TLong(bool _is_const = false, bool _is_volatile = false)
            : base(EnumExprType.LONG, _is_const, _is_volatile) {
            size_of = 4;
        }
    }

    public class TULong : IntegralType {
        public TULong(bool _is_const = false, bool _is_volatile = false)
            : base(EnumExprType.ULONG, _is_const, _is_volatile) {
            size_of = 4;
        }
    }

    public class TFloat : ArithmeticType {
        public TFloat(bool _is_const = false, bool _is_volatile = false)
            : base(EnumExprType.FLOAT, _is_const, _is_volatile) {
            size_of = 4;
        }
    }

    public class TDouble : ArithmeticType {
        public TDouble(bool _is_const = false, bool _is_volatile = false)
            : base(EnumExprType.DOUBLE, _is_const, _is_volatile) {
            size_of = 8;
        }
    }

    public class TPointer : ExprType {
        public TPointer(ExprType _referenced_type, bool _is_const = false, bool _is_volatile = false)
            : base(EnumExprType.POINTER, _is_const, _is_volatile) {
            referenced_type = _referenced_type;
            size_of = 4;
        }
        public readonly ExprType referenced_type;
        public override bool EqualType(ExprType other) {
            return other.expr_type == EnumExprType.POINTER && ((TPointer)other).referenced_type.EqualType(referenced_type);
        }
    }

    // Note: struct is aligned

    public class TStruct : ExprType {
        public TStruct(List<Tuple<String, ExprType>> _attribs, bool _is_const = false, bool _is_volatile = false)
            : base(EnumExprType.STRUCT, _is_const, _is_volatile) {
            attribs = new List<Tuple<string, ExprType, int>>();
            int offset = 0;
            int max_align = 0;
            foreach (Tuple<String, ExprType> _attrib in _attribs) {
                int align = _attrib.Item2.SizeOf;
                if (align > max_align) {
                    max_align = align;
                }
                offset = (offset + align - 1) & ~align;
                attribs.Add(new Tuple<string, ExprType, int>(_attrib.Item1, _attrib.Item2, offset));
                offset += _attrib.Item2.SizeOf;
            }
            offset = (offset + max_align - 1) & ~max_align;
            size_of = offset;
        }
        public readonly List<Tuple<String, ExprType, int>> attribs;
    }

    public class TUnion : ExprType {
        public TUnion(List<Tuple<String, ExprType>> _attribs, bool _is_const = false, bool _is_volatile = false)
            : base(EnumExprType.UNION, _is_const, _is_volatile) {
            attribs = _attribs;
            size_of = attribs.Max(x => x.Item2.SizeOf);
        }
        public readonly List<Tuple<String, ExprType>> attribs;
    }

    // public class 
    // ========================================================================
    public class Expression {
        public Expression(ExprType _type) {
            type = _type;
        }
        public readonly ExprType type;
    }

    public class Variable : Expression {
        public Variable(ExprType _type, String _name)
            : base(_type) {
            name = _name;
        }
        protected String name;
    }

    public class Constant : Expression {
        public Constant(ExprType _type)
            : base(_type) {}
    }

    public class ConstChar : Constant {
        public ConstChar(SByte _value)
            : base(new TChar(true)) {
            value = _value;
        }
        protected SByte value;
    }

    //public class ConstInt : Constant {
    //    public ConstInt(Int32 _value)
    //        : base(new TInt()) {
    //        value = _value;
    //    }
    //    protected Int32 value;
    //}

    //public class ConstUInt : Constant {
    //    public ConstUInt(UInt32 _value)
    //        : base(new TUInt()) {
    //        value = _value;
    //    }
    //    protected UInt32 value;
    //}

    public class ConstLong : Constant {
        public ConstLong(Int32 _value)
            : base(new TLong(true)) {
            value = _value;
        }
        protected Int32 value;
    }

    public class ConstULong : Constant {
        public ConstULong(UInt32 _value)
            : base(new TULong(true)) {
            value = _value;
        }
        protected UInt32 value;
    }

    public class ConstFloat : Constant {
        public ConstFloat(Single _value)
            : base(new TFloat(true)) {
            value = _value;
        }
        protected Single value;
    }

    public class ConstDouble : Constant {
        public ConstDouble(Double _value)
            : base(new TDouble(true)) {
            value = _value;
        }
        protected Double value;
    }

    public class ConstStringLiteral : Constant {
        public ConstStringLiteral(String _value)
            : base(new TPointer(new TChar(true), true)) {
            value=_value;
        }
        protected String value;
    }

}
