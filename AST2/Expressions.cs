using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AST2 {
    public interface IExpr<out T> where T : IType {
        IQualifiedType<T> Type { get; }
    }
    
    public interface IConstExpr<out T> : IExpr<T> where T : IType { }
    
    public class ConstFloat : IConstExpr<FloatType> {
        public ConstFloat(Single value) {
            this.Value = value;
        }

        public IQualifiedType<FloatType> Type => _type;

        public Single Value { get; }

        private static readonly QualifiedType<FloatType> _type = QualifiedType.Const(FloatType.Instance);
    }

    public class ConstDouble : IConstExpr<DoubleType> {
        public ConstDouble(Double value) {
            this.Value = value;
        }

        public IQualifiedType<DoubleType> Type => _type;

        public Double Value { get; }

        private static readonly QualifiedType<DoubleType> _type = new QualifiedType<DoubleType>(DoubleType.Instance, isConst: true, isVolatile: false, isRestricted: true);
    }

    public class ConstSignedLong : IConstExpr<SignedLongType> {
        public ConstSignedLong(Int32 value) {
            this.Value = value;
        }

        public IQualifiedType<SignedLongType> Type => _type;

        public Int32 Value { get; }

        private static readonly QualifiedType<SignedLongType> _type = new QualifiedType<SignedLongType>(SignedLongType.Instance, isConst: true, isVolatile: false, isRestricted: true);
    }

    public class ConstUnsignedLong : IConstExpr<UnsignedLongType> {
        public ConstUnsignedLong(Int32 value) {
            this.Value = value;
        }

        public IQualifiedType<UnsignedLongType> Type => _type;

        public Int32 Value { get; }

        private static readonly QualifiedType<UnsignedLongType> _type = new QualifiedType<UnsignedLongType>(UnsignedLongType.Instance, isConst: true, isVolatile: false, isRestricted: true);
    }

    public class ConstSignedShort : IConstExpr<SignedShortType> {
        public ConstSignedShort(Int16 value) {
            this.Value = value;
        }

        public IQualifiedType<SignedShortType> Type => _type;

        public Int16 Value { get; }

        private static readonly QualifiedType<SignedShortType> _type = new QualifiedType<SignedShortType>(SignedShortType.Instance, isConst: true, isVolatile: false, isRestricted: true);
    }

    public class ConstUnsignedShort : IConstExpr<UnsignedShortType> {
        public ConstUnsignedShort(Int16 value) {
            this.Value = value;
        }

        public IQualifiedType<UnsignedShortType> Type => _type;

        public Int16 Value { get; }

        private static readonly QualifiedType<UnsignedShortType> _type = new QualifiedType<UnsignedShortType>(UnsignedShortType.Instance, isConst: true, isVolatile: false, isRestricted: true);
    }

    public class ConstSignedChar : IConstExpr<SignedCharType> {
        public ConstSignedChar(SByte value) {
            this.Value = value;
        }

        public IQualifiedType<SignedCharType> Type => _type;

        public SByte Value { get; }

        private static readonly QualifiedType<SignedCharType> _type = new QualifiedType<SignedCharType>(SignedCharType.Instance, isConst: true, isVolatile: false, isRestricted: false); 
    }

    public class ConstUnsignedChar : IConstExpr<UnsignedCharType> {
        public ConstUnsignedChar(Byte value) {
            this.Value = value;
        }

        public IQualifiedType<UnsignedCharType> Type => _type;

        public Byte Value { get; }

        private static readonly QualifiedType<UnsignedCharType> _type =
            QualifiedType.Const(UnsignedCharType.Instance);
    }

    public class ConstStringLiteral : IConstExpr<PointerType<SignedCharType>> {
        public ConstStringLiteral(String value) {
            this.Value = value;
        }

        public IQualifiedType<PointerType<SignedCharType>> Type => _type;

        public String Value { get; }

        private static readonly QualifiedType<PointerType<SignedCharType>> _type =
            QualifiedType.Const(
                new PointerType<SignedCharType>(QualifiedType.Const(SignedCharType.Instance))
            );
    }

    public class ConstPointer<T> : IConstExpr<IPointerType<T>> where T : IType {
        public ConstPointer(UInt32 address, IQualifiedType<T> pointeeType) {
            this.Address = address;
            this.Type = QualifiedType.Const(new PointerType<T>(pointeeType));
        }

        public UInt32 Address { get; }

        public IQualifiedType<IPointerType<T>> Type { get; }
    }
}
