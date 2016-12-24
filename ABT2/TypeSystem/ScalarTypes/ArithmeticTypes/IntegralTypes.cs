using System;

namespace ABT2.TypeSystem {
    public interface IIntegralType : IArithmeticType { }

    public interface ISignedIntegralType : IIntegralType { }

    public interface IUnsignedIntegralType : IIntegralType { }

    /// <summary>
    /// The abstract base class of signed and unsigned chars.
    /// </summary>
    public abstract class TChar : IIntegralType {
        public abstract void Visit(IExprTypeVisitor visitor);

        public abstract R Visit<R>(IExprTypeVisitor<R> visitor);

        public Int64 SizeOf => PlatformSpecificConstants.SizeOfChar;

        public Int64 Alignment => PlatformSpecificConstants.AlignmentOfChar;
    }

    public sealed class TSChar : TChar, ISignedIntegralType {
        private TSChar() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitSignedChar(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitSignedChar(this);
        }

        public override String ToString() {
            return "char";
        }

        public static TSChar Get { get; } = new TSChar();
    }

    public sealed class TUChar : TChar, IUnsignedIntegralType {
        private TUChar() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitUnsignedChar(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitUnsignedChar(this);
        }

        public override String ToString() {
            return "unsigned char";
        }

        public static TUChar Get { get; } = new TUChar();
    }

    /// <summary>
    /// The abstract base class of signed and unsigned shorts.
    /// </summary>
    public abstract class TShort : IIntegralType {
        public abstract void Visit(IExprTypeVisitor visitor);

        public abstract R Visit<R>(IExprTypeVisitor<R> visitor);

        public Int64 SizeOf => PlatformSpecificConstants.SizeOfShort;

        public Int64 Alignment => PlatformSpecificConstants.AlignmentOfShort;
    }

    public sealed class TSShort : TShort, ISignedIntegralType {
        private TSShort() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitSignedShort(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitSignedShort(this);
        }

        public override String ToString() {
            return "short";
        }

        public static TSShort Get { get; } = new TSShort();
    }

    public sealed class TUShort : TShort, IUnsignedIntegralType {
        private TUShort() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitUnsignedShort(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitUnsignedShort(this);
        }

        public override String ToString() {
            return "unsigned short";
        }

        public static TUShort Get { get; } = new TUShort();
    }

    /// <summary>
    /// The abstract base class of signed and unsigned ints.
    /// </summary>
    public abstract class TInt : IIntegralType {
        public abstract void Visit(IExprTypeVisitor visitor);

        public abstract R Visit<R>(IExprTypeVisitor<R> visitor);

        public Int64 SizeOf => PlatformSpecificConstants.SizeOfInt;

        public Int64 Alignment => PlatformSpecificConstants.AlignmentOfInt;
    }

    public sealed class TSInt : TInt, ISignedIntegralType {
        private TSInt() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitSignedInt(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitSignedInt(this);
        }

        public override String ToString() {
            return "int";
        }

        public static TSInt Get { get; } = new TSInt();
    }

    public sealed class TUInt : TInt, IUnsignedIntegralType {
        private TUInt() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitUnsignedInt(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitUnsignedInt(this);
        }

        public override String ToString() {
            return "unsigned int";
        }

        public static TUInt Get { get; } = new TUInt();
    }

    /// <summary>
    /// The abstract base class of signed and unsigned longs.
    /// </summary>
    public abstract class TLong : IIntegralType {
        public abstract void Visit(IExprTypeVisitor visitor);

        public abstract R Visit<R>(IExprTypeVisitor<R> visitor);

        public Int64 SizeOf => PlatformSpecificConstants.SizeOfLong;

        public Int64 Alignment => PlatformSpecificConstants.AlignmentOfLong;
    }

    public sealed class TSLong : TLong, ISignedIntegralType {
        private TSLong() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitSignedLong(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitSignedLong(this);
        }

        public override String ToString() {
            return "long";
        }

        public static TSLong Get { get; } = new TSLong();
    }

    public sealed class TULong : TLong, IUnsignedIntegralType {
        private TULong() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitUnsignedLong(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitUnsignedLong(this);
        }

        public override String ToString() {
            return "unsigned long";
        }

        public static TULong Get { get; } = new TULong();
    }
}
