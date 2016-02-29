using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

    public enum ExprTypeKind {
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
        STRUCT_OR_UNION
    }

    public abstract class ExprType {
        protected ExprType(Boolean isConst, Boolean isVolatile) {
            this.IsConst = isConst;
            this.IsVolatile = isVolatile;
        }

        public const Int32 SIZEOF_CHAR = 1;
        public const Int32 SIZEOF_SHORT = 2;
        public const Int32 SIZEOF_LONG = 4;
        public const Int32 SIZEOF_FLOAT = 4;
        public const Int32 SIZEOF_DOUBLE = 8;
        public const Int32 SIZEOF_POINTER = 4;

        public const Int32 ALIGN_CHAR = 1;
        public const Int32 ALIGN_SHORT = 2;
        public const Int32 ALIGN_LONG = 4;
        public const Int32 ALIGN_FLOAT = 4;
        public const Int32 ALIGN_DOUBLE = 4;
        public const Int32 ALIGN_POINTER = 4;

        public abstract ExprTypeKind Kind { get; }

        public virtual Boolean IsArith => false;

        public virtual Boolean IsIntegral => false;

        public virtual Boolean IsScalar => false;

        public virtual Boolean IsComplete => true;

        public abstract Boolean EqualType(ExprType other);

        public String DumpQualifiers() {
            String str = "";
            if (this.IsConst) {
                str += "const ";
            }
            if (this.IsVolatile) {
                str += "volatile ";
            }
            return str;
        }

        public abstract ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile);
        
        public abstract Int32 SizeOf { get; }
        public abstract Int32 Alignment { get; }

        public readonly Boolean IsConst;
        public readonly Boolean IsVolatile;

    }

    

    public class VoidType : ExprType {
        public VoidType(Boolean isConst = false, Boolean isVolatile = false)
            : base(isConst, isVolatile) {
        }

        public override ExprTypeKind Kind => ExprTypeKind.VOID;

        public override Int32 SizeOf => SIZEOF_POINTER;

        public override Int32 Alignment => SIZEOF_POINTER;

        public override ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile) =>
            new VoidType(isConst, isVolatile);
        
        public override String ToString() =>
            DumpQualifiers() + "void";
        
        public override Boolean EqualType(ExprType other) => other.Kind == ExprTypeKind.VOID;

    }

    public abstract class ScalarType : ExprType {
        protected ScalarType(Boolean isConst, Boolean isVolatile)
            : base(isConst, isVolatile) { }
        public override Boolean IsScalar => true;
    }

    public abstract class ArithmeticType : ScalarType {
        protected ArithmeticType(Boolean isConst, Boolean isVolatile)
            : base(isConst, isVolatile) { }
        public override Boolean IsArith => true;
        public override Boolean EqualType(ExprType other) => this.Kind == other.Kind;
    }

    public abstract class IntegralType : ArithmeticType {
        protected IntegralType(Boolean isConst, Boolean isVolatile)
            : base(isConst, isVolatile) { }
        public override Boolean IsIntegral => true;
    }

    public class CharType : IntegralType {
        public CharType(Boolean isConst = false, Boolean isVolatile = false)
            : base(isConst, isVolatile) { }

        public override ExprTypeKind Kind => ExprTypeKind.CHAR;

        public override Int32 SizeOf => SIZEOF_CHAR;

        public override Int32 Alignment => ALIGN_CHAR;

        public override ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile) =>
            new CharType(isConst, isVolatile);

        public override String ToString() => DumpQualifiers() + "char";
    }

    public class UCharType : IntegralType {
        public UCharType(Boolean isConst = false, Boolean isVolatile = false)
            : base(isConst, isVolatile) { }

        public override ExprTypeKind Kind => ExprTypeKind.UCHAR;

        public override Int32 SizeOf => SIZEOF_CHAR;

        public override Int32 Alignment => ALIGN_CHAR;

        public override ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile) =>
            new UCharType(isConst, isVolatile);

        public override String ToString() => DumpQualifiers() + "unsigned char";
    }

    public class ShortType : IntegralType {
        public ShortType(Boolean isConst = false, Boolean isVolatile = false)
            : base(isConst, isVolatile) { }

        public override ExprTypeKind Kind => ExprTypeKind.SHORT;

        public override Int32 SizeOf => SIZEOF_SHORT;

        public override Int32 Alignment => ALIGN_SHORT;

        public override ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile) =>
            new ShortType(isConst, isVolatile);

        public override String ToString() => DumpQualifiers() + "short";
    }

    public class UShortType : IntegralType {
        public UShortType(Boolean isConst = false, Boolean isVolatile = false)
            : base(isConst, isVolatile) { }

        public override ExprTypeKind Kind => ExprTypeKind.USHORT;

        public override Int32 SizeOf => SIZEOF_SHORT;

        public override Int32 Alignment => ALIGN_SHORT;

        public override ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile) =>
            new UShortType(isConst, isVolatile);

        public override String ToString() => DumpQualifiers() + "unsigned short";
    }

    public class LongType : IntegralType {
        public LongType(Boolean isConst = false, Boolean isVolatile = false)
            : base(isConst, isVolatile) { }

        public override ExprTypeKind Kind => ExprTypeKind.LONG;

        public override Int32 SizeOf => SIZEOF_LONG;

        public override Int32 Alignment => ALIGN_LONG;

        public override ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile) {
            return new LongType(isConst, isVolatile);
        }

        public override String ToString() {
            return DumpQualifiers() + "long";
        }
    }

    public class ULongType : IntegralType {
        public ULongType(Boolean isConst = false, Boolean isVolatile = false)
            : base(isConst, isVolatile) { }

        public override ExprTypeKind Kind => ExprTypeKind.ULONG;

        public override Int32 SizeOf => SIZEOF_LONG;

        public override Int32 Alignment => ALIGN_LONG;

        public override ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile) {
            return new ULongType(isConst, isVolatile);
        }

        public override String ToString() {
            return DumpQualifiers() + "unsigned long";
        }
    }

    public class FloatType : ArithmeticType {
        public FloatType(Boolean isConst = false, Boolean isVolatile = false)
            : base(isConst, isVolatile) { }

        public override ExprTypeKind Kind => ExprTypeKind.FLOAT;

        public override Int32 SizeOf => SIZEOF_FLOAT;

        public override Int32 Alignment => ALIGN_FLOAT;

        public override ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile) =>
            new FloatType(isConst, isVolatile);

        public override String ToString() => DumpQualifiers() + "float";
    }

    public class DoubleType : ArithmeticType {
        public DoubleType(Boolean isConst = false, Boolean isVolatile = false)
            : base(isConst, isVolatile) { }

        public override ExprTypeKind Kind => ExprTypeKind.DOUBLE;

        public override Int32 SizeOf => SIZEOF_DOUBLE;

        public override Int32 Alignment => ALIGN_DOUBLE;

        public override ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile) =>
            new DoubleType(isConst, isVolatile);

        public override String ToString() => DumpQualifiers() + "double";
    }

    public class PointerType : ScalarType {
        public PointerType(ExprType refType, Boolean isConst = false, Boolean isVolatile = false)
            : base(isConst, isVolatile) {
            this.RefType = refType;
        }

        public override ExprTypeKind Kind => ExprTypeKind.POINTER;

        public override Int32 SizeOf => SIZEOF_POINTER;

        public override Int32 Alignment => ALIGN_POINTER;

        public readonly ExprType RefType;

        public override ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile) =>
            new PointerType(this.RefType, isConst, isVolatile);

        public override Boolean EqualType(ExprType other) =>
            other.Kind == ExprTypeKind.POINTER && ((PointerType)other).RefType.EqualType(this.RefType);

        public override String ToString() => $"{DumpQualifiers()}ptr<{this.RefType}>";
    }

    /// <summary>
    /// Incomplete array: an array with unknown length.
    /// </summary>
    public class IncompleteArrayType : ExprType {
        public IncompleteArrayType(ExprType elemType, Boolean isConst = false, Boolean isVolatile = false)
            : base(isConst, isVolatile) {
            this.ElemType = elemType;
        }

        public override ExprTypeKind Kind => ExprTypeKind.INCOMPLETE_ARRAY;

        public override Int32 SizeOf {
            get {
                throw new InvalidOperationException("Incomplete array. Cannot get sizeof.");
            }
        }

        public override Int32 Alignment => this.ElemType.Alignment;

        public override ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile) =>
            new IncompleteArrayType(this.ElemType, isConst, isVolatile);

        public override Boolean EqualType(ExprType other) => false;

        public override Boolean IsComplete => false;

        public ExprType Complete(Int32 numElems) => new ArrayType(this.ElemType, numElems, this.IsConst, this.IsVolatile);

        public override String ToString() => $"{this.ElemType}[]";

        public readonly ExprType ElemType;
    }

    public class ArrayType : ExprType {
        public ArrayType(ExprType elemType, Int32 numElems, Boolean isConst = false, Boolean isVolatile = false)
            : base(isConst, isVolatile) {
            this.ElemType = elemType;
            this.NumElems = numElems;
        }

        public override ExprTypeKind Kind => ExprTypeKind.ARRAY;

        public override Int32 SizeOf => this.ElemType.SizeOf * this.NumElems;

        public override Int32 Alignment => this.ElemType.Alignment;

        public readonly ExprType ElemType;

        public readonly Int32 NumElems;

        public override ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile) =>
            new ArrayType(this.ElemType, this.NumElems, isConst, isVolatile);

        public override Boolean EqualType(ExprType other) =>
            other.Kind == ExprTypeKind.ARRAY && ((ArrayType)other).ElemType.EqualType(this.ElemType);

        public override String ToString() => $"Arr[{this.NumElems}, {this.ElemType}]";
    }
    
    public class StructOrUnionType : ExprType {
        private StructOrUnionType(StructOrUnionLayout layout, Boolean isConst, Boolean isVolatile)
            : base(isConst, isVolatile) {
            this._layout = layout;
        }

        public override ExprTypeKind Kind => ExprTypeKind.STRUCT_OR_UNION;

        public override ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile) =>
            new StructOrUnionType(this._layout, isConst, isVolatile);

        public static StructOrUnionType CreateIncompleteStruct(String name, Boolean is_const, Boolean is_volatile) =>
            new StructOrUnionType(new StructOrUnionLayout($"struct {name}"), is_const, is_volatile);

        public static StructOrUnionType CreateIncompleteUnion(String name, Boolean is_const, Boolean is_volatile) =>
            new StructOrUnionType(new StructOrUnionLayout($"union {name}"), is_const, is_volatile);

        public static StructOrUnionType CreateIncompleteType(SyntaxTree.StructOrUnion structOrUnion, String name) =>
            structOrUnion == SyntaxTree.StructOrUnion.STRUCT
                ? CreateIncompleteStruct(name, false, false)
                : CreateIncompleteUnion(name, false, false);

        public static StructOrUnionType CreateStruct(String name, IReadOnlyList<Tuple<String, ExprType>> attribs, Boolean is_const, Boolean is_volatile) {
            StructOrUnionLayout layout = new StructOrUnionLayout($"struct {name}");
            layout.DefineStruct(attribs);
            return new StructOrUnionType(layout, is_const, is_volatile);
        }

        public static StructOrUnionType CreateUnion(String name, IReadOnlyList<Tuple<String, ExprType>> attribs, Boolean is_const, Boolean is_volatile) {
            StructOrUnionLayout layout = new StructOrUnionLayout($"union {name}");
            layout.DefineUnion(attribs);
            return new StructOrUnionType(layout, is_const, is_volatile);
        }

        public void DefineStruct(IReadOnlyList<Tuple<String, ExprType>> attribs) => this._layout.DefineStruct(attribs);

        public void DefineUnion(IReadOnlyList<Tuple<String, ExprType>> attribs) => this._layout.DefineUnion(attribs);

        public void Define(
            SyntaxTree.StructOrUnion structOrUnion,
            ImmutableList<Tuple<Option<String>, ExprType>> members) {
            var _members = members.ConvertAll(_ => Tuple.Create(_.Item1.Value, _.Item2));
            if (structOrUnion == SyntaxTree.StructOrUnion.STRUCT) {
                DefineStruct(_members);
            } else {
                DefineUnion(_members);
            }
        }

        public String Dump(Boolean dump_attribs) {
            if (!this.IsComplete) {
                return "incompleted type " + this._layout.TypeName;
            }
            String str = $"{this._layout.TypeName} (size = {this.SizeOf})";
            if (dump_attribs) {
                str += "\n";
                foreach (Utils.StoreEntry attrib in this._layout.Attribs) {
                    str += $"  [base + {attrib.offset}] {attrib.name} : {attrib.type}\n";
                }
            }
            return str;
        }

        public override String ToString() => Dump(false);

        public override Boolean EqualType(ExprType other) =>
            other.Kind == ExprTypeKind.STRUCT_OR_UNION && ReferenceEquals(((StructOrUnionType)other)._layout, this._layout);

        public override Boolean IsComplete => this._layout.IsComplete;

        public override Int32 SizeOf => this._layout.SizeOf;

        public override Int32 Alignment => this._layout.Alignment;

        public Boolean IsStruct => this._layout.IsStruct;

        public IReadOnlyList<Utils.StoreEntry> Attribs => this._layout.Attribs;

        private readonly StructOrUnionLayout _layout;

        private class StructOrUnionLayout {

            // Create an incomplete struct.
            public StructOrUnionLayout(String typename) {
                this._attribs = null;
                this._size_of = 0;
                this.TypeName = typename;
            }

            public void DefineStruct(IReadOnlyList<Tuple<String, ExprType>> attribs) {
                if (this.IsComplete) {
                    throw new InvalidOperationException("Cannot redefine a struct.");
                }

                this._attribs = new List<Utils.StoreEntry>();
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

                    this._attribs.Add(new Utils.StoreEntry(name, type, offset));

                    offset += type.SizeOf;
                }

                this._size_of = Utils.RoundUp(offset, struct_alignment);
            }

            public void DefineUnion(IEnumerable<Tuple<String, ExprType>> attribs) {
                if (this.IsComplete) {
                    throw new InvalidOperationException("Redefining a union.");
                }

                this._attribs = attribs
                    .Select(attrib => new Utils.StoreEntry(attrib.Item1, attrib.Item2, 0))
                    .ToList();

                this._size_of = this.Attribs.Select(attrib => attrib.type.Alignment).Max();
            }

            public IReadOnlyList<Utils.StoreEntry> Attribs {
                get {
                    if (!this.IsComplete) {
                        throw new InvalidOperationException("Incomplete struct or union. Cannot get attributes.");
                    }
                    return this._attribs;
                }
            }

            // Is this a struct or union.
            public Boolean IsStruct => this.TypeName.StartsWith("struct");

            // Whether the attributes are supplied.
            public Boolean IsComplete => this._attribs != null;

            // Only a complete type has a valid size.
            public Int32 SizeOf {
                get {
                    if (!this.IsComplete) {
                        throw new InvalidOperationException("Incomplete struct or union. Cannot get size.");
                    }
                    return this._size_of;
                }
            }

            public Int32 Alignment => this.Attribs.Select(_ => _.type.Alignment).Max();

            public String TypeName { get; }

            /// <summary>
            /// Private records of all the Attribs with their names, types, and offsets.
            /// </summary>
            private List<Utils.StoreEntry> _attribs;

            /// <summary>
            /// size_of and alignment can only be changed by defining the layout.
            /// </summary>
            private Int32 _size_of;
        }
    }

    // class FunctionType
    // ===============
    // represents the function type
    // stores the names, types, and offsets of arguments
    // 
    // calling convention:
    // https://developer.apple.com/library/mac/documentation/DeveloperTools/Conceptual/LowLevelABI/130-IA-32_Function_Calling_Conventions/IA32.html
    // 
    // TODO: name is optional
    public class FunctionType : ExprType {
        protected FunctionType(ExprType ret_t, List<Utils.StoreEntry> args, Boolean is_varargs)
            : base(true, false) {
            this.Args = args;
            this.ReturnType = ret_t;
            this.HasVarArgs = is_varargs;
        }

        public override ExprTypeKind Kind => ExprTypeKind.FUNCTION;

        public override Int32 SizeOf => SIZEOF_POINTER;

        public override Int32 Alignment => ALIGN_POINTER;

        public override ExprType GetQualifiedType(Boolean isConst, Boolean isVolatile) {
            return new FunctionType(this.ReturnType, this.Args, this.HasVarArgs);
        }

        public override Boolean EqualType(ExprType other) {
            return (other is FunctionType)
                && (other as FunctionType).HasVarArgs == this.HasVarArgs

                // same return type
                && (other as FunctionType).ReturnType.EqualType(this.ReturnType)

                // same number of arguments
                && (other as FunctionType).Args.Count == this.Args.Count

                // same argument types
                && (other as FunctionType).Args.Zip(this.Args, (entry1, entry2) => entry1.type.EqualType(entry2.type)).All(_ => _);
        }

        public static FunctionType Create(ExprType ret_type, List<Tuple<String, ExprType>> args, Boolean is_varargs) {
            Tuple<Int32, IReadOnlyList<Int32>> r_pack = Utils.PackArguments(args.ConvertAll(_ => _.Item2));
            IReadOnlyList<Int32> offsets = r_pack.Item2;
            if (ret_type is StructOrUnionType) {
                offsets = offsets.Select(_ => _ + 3 * SIZEOF_POINTER).ToList();
            } else {
                offsets = offsets.Select(_ => _ + 2 * SIZEOF_POINTER).ToList();
            }
            return new FunctionType(
                ret_type,
                args.Zip(offsets,
                    (name_type, offset) => new Utils.StoreEntry(name_type.Item1, name_type.Item2, offset)
                ).ToList(),
                is_varargs
            );
        }

        // TODO: param name should be optional
        public static FunctionType Create(ExprType returnType, ImmutableList<Tuple<Option<String>, ExprType>> args, Boolean hasVarArgs) =>
            Create(returnType, args.Select(_ => Tuple.Create(_.Item1.IsSome ? _.Item1.Value : "", _.Item2)).ToList(), hasVarArgs);

        public static FunctionType Create(ExprType returnType) =>
            Create(returnType, ImmutableList<Tuple<Option<String>, ExprType>>.Empty, true);

        public String Dump(Boolean dump_args = false) {
            String str = "function";
            if (dump_args) {
                str += "\n";
                foreach (Utils.StoreEntry arg in this.Args) {
                    str += $"  [%ebp + {arg.offset}] {arg.name} : {arg.type}\n";
                }
            }
            return str;
        }

        public override String ToString() {
            String str = "";
            for (Int32 i = 0; i < this.Args.Count; ++i) {
                if (i != 0) {
                    str += ", ";
                }
                str += this.Args[i].type.ToString();
            }
            if (this.Args.Count > 0) {
                str = $"({str})";
            }
            return str + " -> " + this.ReturnType;
        }

        public readonly Boolean                HasVarArgs;
        public readonly ExprType               ReturnType;
        public readonly List<Utils.StoreEntry> Args;
    }

    // class EmptyFunctionType
    // ====================
    // defines an empty function: no arguments, returns void
    // 
    public class EmptyFunctionType : FunctionType {
        public EmptyFunctionType() : base(new VoidType(), new List<Utils.StoreEntry>(), false) {
        }
    }

}
