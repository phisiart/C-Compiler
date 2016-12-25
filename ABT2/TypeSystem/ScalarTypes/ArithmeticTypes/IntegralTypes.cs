using ABT2.Environment;
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

        public Int64 SizeOf(Env env) => PlatformSpecificConstants.SizeOfChar;

        public Int64 Alignment(Env env) => PlatformSpecificConstants.AlignmentOfChar;
    }

    /// <summary>
    /// The signed char type.
    /// </summary>
    public sealed class TSChar : TChar, ISignedIntegralType {
        private TSChar() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitSignedChar(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitSChar(this);
        }

        public override String ToString() {
            return "char";
        }

        public static TSChar Get { get; } = new TSChar();
    }

    /// <summary>
    /// A cv-qualified signed char type.
    /// </summary>
    public sealed class QualSChar : QualExprType<TSChar> {
        public QualSChar(TypeQuals typeQuals, TSChar type)
            : base(typeQuals, type) { }
    }

    /// <summary>
    /// The unsigned char type.
    /// </summary>
    public sealed class TUChar : TChar, IUnsignedIntegralType {
        private TUChar() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitUnsignedChar(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitUChar(this);
        }

        public override String ToString() {
            return "unsigned char";
        }

        public static TUChar Get { get; } = new TUChar();
    }

    /// <summary>
    /// A cv-qualified unsigned char type.
    /// </summary>
    public sealed class QualUChar : QualExprType<TUChar> {
        public QualUChar(TypeQuals typeQuals, TUChar type)
            : base(typeQuals, type) { }
    }

    /// <summary>
    /// The abstract base class of signed and unsigned shorts.
    /// </summary>
    public abstract class TShort : IIntegralType {
        public abstract void Visit(IExprTypeVisitor visitor);

        public abstract R Visit<R>(IExprTypeVisitor<R> visitor);

        public Int64 SizeOf(Env env) => PlatformSpecificConstants.SizeOfShort;

        public Int64 Alignment(Env env) => PlatformSpecificConstants.AlignmentOfShort;
    }

    /// <summary>
    /// The signed short type.
    /// </summary>
    public sealed class TSShort : TShort, ISignedIntegralType {
        private TSShort() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitSignedShort(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitSShort(this);
        }

        public override String ToString() {
            return "short";
        }

        public static TSShort Get { get; } = new TSShort();
    }

    /// <summary>
    /// A cv-qualified signed short type.
    /// </summary>
    public sealed class QualSShort : QualExprType<TSShort> {
        public QualSShort(TypeQuals typeQuals, TSShort type)
            : base(typeQuals, type) { }
    }

    public sealed class TUShort : TShort, IUnsignedIntegralType {
        private TUShort() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitUnsignedShort(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitUShort(this);
        }

        public override String ToString() {
            return "unsigned short";
        }

        public static TUShort Get { get; } = new TUShort();
    }

    /// <summary>
    /// A cv-qualified unsigned short type.
    /// </summary>
    public sealed class QualUShort : QualExprType<TUShort> {
        public QualUShort(TypeQuals typeQuals, TUShort type)
            : base(typeQuals, type) { }
    }

    /// <summary>
    /// The abstract base class of signed and unsigned ints.
    /// </summary>
    public abstract class TInt : IIntegralType {
        public abstract void Visit(IExprTypeVisitor visitor);

        public abstract R Visit<R>(IExprTypeVisitor<R> visitor);

        public Int64 SizeOf(Env env) => PlatformSpecificConstants.SizeOfInt;

        public Int64 Alignment(Env env) => PlatformSpecificConstants.AlignmentOfInt;
    }

    /// <summary>
    /// The signed int type.
    /// </summary>
    public sealed class TSInt : TInt, ISignedIntegralType {
        private TSInt() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitSignedInt(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitSInt(this);
        }

        public override String ToString() {
            return "int";
        }

        public static TSInt Get { get; } = new TSInt();
    }

    /// <summary>
    /// A cv-qualified signed int type.
    /// </summary>
    public sealed class QualSInt : QualExprType<TSInt> {
        public QualSInt(TypeQuals typeQuals, TSInt type)
            : base(typeQuals, type) { }
    }

    /// <summary>
    /// The unsigned int type.
    /// </summary>
    public sealed class TUInt : TInt, IUnsignedIntegralType {
        private TUInt() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitUnsignedInt(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitUInt(this);
        }

        public override String ToString() {
            return "unsigned int";
        }

        public static TUInt Get { get; } = new TUInt();
    }

    /// <summary>
    /// A cv-qualified unsigned int type.
    /// </summary>
    public sealed class QualUInt : QualExprType<TUInt> {
        public QualUInt(TypeQuals typeQuals, TUInt type)
            : base(typeQuals, type) { }
    }

    /// <summary>
    /// The abstract base class of signed and unsigned longs.
    /// </summary>
    public abstract class TLong : IIntegralType {
        public abstract void Visit(IExprTypeVisitor visitor);

        public abstract R Visit<R>(IExprTypeVisitor<R> visitor);

        public Int64 SizeOf(Env env) => PlatformSpecificConstants.SizeOfLong;

        public Int64 Alignment(Env env) => PlatformSpecificConstants.AlignmentOfLong;
    }

    /// <summary>
    /// The signed long type.
    /// </summary>
    public sealed class TSLong : TLong, ISignedIntegralType {
        private TSLong() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitSignedLong(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitSLong(this);
        }

        public override String ToString() {
            return "long";
        }

        public static TSLong Get { get; } = new TSLong();
    }

    /// <summary>
    /// A cv-qualified signed long type.
    /// </summary>
    public sealed class QualSLong : QualExprType<TSLong> {
        public QualSLong(TypeQuals typeQuals, TSLong type)
            : base(typeQuals, type) { }
    }

    /// <summary>
    /// The unsigned long type.
    /// </summary>
    public sealed class TULong : TLong, IUnsignedIntegralType {
        private TULong() { }

        public override void Visit(IExprTypeVisitor visitor) {
            visitor.VisitUnsignedLong(this);
        }

        public override R Visit<R>(IExprTypeVisitor<R> visitor) {
            return visitor.VisitULong(this);
        }

        public override String ToString() {
            return "unsigned long";
        }

        public static TULong Get { get; } = new TULong();
    }

    /// <summary>
    /// A cv-qualified unsigned long type.
    /// </summary>
    public sealed class QualULong : QualExprType<TULong> {
        public QualULong(TypeQuals typeQuals, TULong type)
            : base(typeQuals, type) { }
    }
}
