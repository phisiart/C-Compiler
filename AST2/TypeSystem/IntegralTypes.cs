using System;

namespace AST2 {

    /*
     * arithmetic
     *   +-- integral
     *         +-- char (1 byte size & align)
     *         |     +-- signed char
     *         |     +-- unsigned char
     *         |
     *         +-- short (2 byte size & align)
     *         |     +-- signed short
     *         |     +-- unsigned short
     *         |
     *         +-- long (4 byte size & align)
     *               +-- signed long
     *               +-- unsigned long
     */

    public interface IIntegralType : IArithmeticType { }

    public abstract class IntegralType : ArithmeticType, IIntegralType { }

    public interface ISignedIntegralType : IIntegralType { }

    public interface IUnsignedIntegralType : IIntegralType { }

    public abstract class CharType : IntegralType {
        public override UInt32 SizeOf => 1;

        public override UInt32 Alignment => 1;
    }

    public class SignedCharType : CharType, ISignedIntegralType {
        private SignedCharType() { }

        public static SignedCharType Instance = new SignedCharType();
    }

    public class UnsignedCharType : CharType, IUnsignedIntegralType {
        private UnsignedCharType() { }

        public static UnsignedCharType Instance = new UnsignedCharType();
    }

    public abstract class ShortType : IntegralType {
        public override UInt32 SizeOf => 2;

        public override UInt32 Alignment => 2;
    }

    public class SignedShortType : ShortType, ISignedIntegralType {
        private SignedShortType() { }

        public static SignedShortType Instance = new SignedShortType();
    }

    public class UnsignedShortType : ShortType, IUnsignedIntegralType {
        private UnsignedShortType() { }

        public static UnsignedShortType Instance = new UnsignedShortType();
    }

    public abstract class LongType : IntegralType {
        public override UInt32 SizeOf => 4;

        public override UInt32 Alignment => 4;
    }

    public class SignedLongType : LongType, ISignedIntegralType {
        private SignedLongType() { }

        public static SignedLongType Instance = new SignedLongType();
    }

    public class UnsignedLongType : LongType, IUnsignedIntegralType {
        private UnsignedLongType() { }

        public static UnsignedLongType Instance = new UnsignedLongType();
    }
}