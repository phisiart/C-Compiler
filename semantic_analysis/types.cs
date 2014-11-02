using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// class: TType
// ------------
// the base class of all types
public class TType {
    public TType() {
        is_const = false;
        is_volatile = false;
        kind = Kind.OTHER;
    }
    public virtual int SizeOf() {
        return 0;
    }
    public enum Kind {
        VOID,
        INT8,
        UINT8,
        INT16,
        UINT16,
        INT32,
        UINT32,
        FLOAT32,
        FLOAT64,
        POINTER,
        ARRAY,
        STRUCT,
        UNION,
        OTHER
    }

    public bool IsInt() {
        switch (kind) {
        case Kind.INT8:
        case Kind.UINT8:
        case Kind.INT16:
        case Kind.UINT16:
        case Kind.INT32:
        case Kind.UINT32:
            return true;
        default:
            return false;
        }
    }

    public bool IsArith() {
        switch (kind) {
        case Kind.INT8:
        case Kind.UINT8:
        case Kind.INT16:
        case Kind.UINT16:
        case Kind.INT32:
        case Kind.UINT32:
        case Kind.FLOAT32:
        case Kind.FLOAT64:
            return true;
        default:
            return false;
        }
    }

    public bool IsScalar() {
        switch (kind) {
        case Kind.INT8:
        case Kind.UINT8:
        case Kind.INT16:
        case Kind.UINT16:
        case Kind.INT32:
        case Kind.UINT32:
        case Kind.FLOAT32:
        case Kind.FLOAT64:
        case Kind.POINTER:
            return true;
        default:
            return false;
        }
    }
    
    public Kind kind;
    public bool is_const;
    public bool is_volatile;
}

// class: TVoid
// ------------
// the class of type void
public class TVoid : TType {
    public TVoid() {
        kind = Kind.VOID;
    }
}


// class: TBasicType
// -----------------
// the base class of all built-in types of C
public class TBasicType : TType {
    public override int SizeOf() {
        return size;
    }
    public int size = 0;
}


public class TBasicType1Byte : TBasicType {
    public TBasicType1Byte() {
        size = 1;
    }
}

public class TBasicType2Bytes : TBasicType {
    public TBasicType2Bytes() {
        size = 2;
    }
}

public class TBasicType4Bytes : TBasicType {
    public TBasicType4Bytes() {
        size = 4;
    }
}

public class TBasicType8Bytes : TBasicType {
    public TBasicType8Bytes() {
        size = 8;
    }
}

public class TInt8 : TBasicType1Byte {
    public TInt8() {
        kind = Kind.INT8;
    }
}
public class TUInt8 : TBasicType1Byte {
    public TUInt8() {
        kind = Kind.UINT8;
    }
}
public class TInt16 : TBasicType2Bytes {
    public TInt16() {
        kind = Kind.INT16;
    }
}
public class TUInt16 : TBasicType2Bytes {
    public TUInt16() {
        kind = Kind.UINT16;
    }
}
public class TInt32 : TBasicType4Bytes {
    public TInt32() {
        kind = Kind.INT32;
    }
}
public class TUInt32 : TBasicType4Bytes {
    public TUInt32() {
        kind = Kind.UINT32;
    }
}
public class TFloat32 : TBasicType4Bytes {
    public TFloat32() {
        kind = Kind.FLOAT32;
    }
}
public class TFloat64 : TBasicType8Bytes {
    public TFloat64() {
        kind = Kind.FLOAT64;
    }
}


// char, unsigned char, signed char -- 1 byte
// ----------------------------------------------------------------------------
// type: char
public class TChar : TInt8 {}

// type: signed char (= char in this implementation)
public class TSChar : TInt8 {}

// type: unsigned char
public class TUChar : TUInt8 {}


// short, unsigned short -- 2 bytes
// ----------------------------------------------------------------------------
// type: short
public class TShort : TInt16 {}

// type: unsigned short
public class TUShort : TUInt16 {}


// int, unsigned int -- 4 bytes
// ----------------------------------------------------------------------------
// type: int
public class TInt : TInt32 {}

// type: unsigned int
public class TUInt : TUInt32 {}


// long, unsigned long -- 4 bytes
// ----------------------------------------------------------------------------
// type: long
public class TLong : TInt32 {}

// type: unsigned long
public class TULong : TUInt32 {}


// float -- 4 bytes
public class TFloat : TFloat32 {}


// double -- 8 bytes
public class TDouble : TFloat64 {}


// long double -- 8 bytes
public class TLongDouble : TFloat64 {}


// pointer -- 4 bytes
public class TPointer : TType {
    public override int SizeOf() {
        return 4;
    }
    public TPointer(TType _ref_t) {
        kind = Kind.POINTER;
        ref_t = _ref_t;
    }
    public TType ref_t;
}


// array
public class TArray : TType {
    public TArray(TType _elem_t, int _len) {
        kind = Kind.ARRAY;
        len = _len;
        elem_t = _elem_t;
    }
    public override int SizeOf() {
        return len * elem_t.SizeOf();
    }
    public int len = 0;
    public TType elem_t;
}


// struct
public class TStruct : TType {
    public TStruct() {
        kind = Kind.STRUCT;
        attribs = new List<ScopeEntry>();
    }
    
    private int GetAlignedAddress(int addr, int align) {
        return addr - (addr + align - 1) % align + align - 1;
    }

    public int GetOffset(int idx) {
        int offset = 0;
        for (int i = 0; i < idx; ++i) {
            offset = GetAlignedAddress(offset, attribs[i].type.SizeOf());
            offset += attribs[i].type.SizeOf();
        }
        return GetAlignedAddress(offset, attribs[idx].type.SizeOf());
    }
    
    public override int SizeOf() {
        int idx_last = attribs.Count - 1;
        int max_sizeof = attribs.Max(x => x.type.SizeOf());
        return GetAlignedAddress(GetOffset(idx_last) + attribs[idx_last].type.SizeOf(), max_sizeof);
    }

    public List<ScopeEntry> attribs;
}


// union
public class TUnion : TType {
    public TUnion() {
        kind = Kind.UNION;
        attribs = new List<ScopeEntry>();
    }

    public override int SizeOf() {
        return attribs.Max(x => x.type.SizeOf());
    }

    public List<ScopeEntry> attribs;
}

public class TAttribute : TType {
    public TAttribute(String _name, TType _type) {
        name = _name;
        type = _type;
    }
    public String name;
    public TType type;
}

public class TFunction : TType {
    public TFunction(List<ScopeEntry> _params_, bool _varargs = false) {
        params_ = _params_;
        varargs = _varargs;
    }
    public List<ScopeEntry> params_;
    public bool varargs;
}

// type name : not a complete type
public class TName : TType {
    public TName(String _name) {
        name = _name;
    }

    public override int SizeOf() {
        return 0;
    }
    public String name;
}