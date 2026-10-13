using System;

namespace LexicalAnalysis {

    /// <summary>
    /// A position in the source file. Lines and columns are both 1-based.
    /// </summary>
    public readonly struct SourceLocation : IEquatable<SourceLocation> {
        public SourceLocation(Int32 line, Int32 column) {
            this.Line = line;
            this.Column = column;
        }

        public Int32 Line { get; }
        public Int32 Column { get; }

        public Boolean IsValid => this.Line > 0;

        public Boolean Equals(SourceLocation other) =>
            this.Line == other.Line && this.Column == other.Column;

        public override Boolean Equals(Object obj) =>
            obj is SourceLocation other && this.Equals(other);

        public override Int32 GetHashCode() => HashCode.Combine(this.Line, this.Column);

        public override String ToString() => $"{this.Line}:{this.Column}";
    }

    /// <summary>
    /// An error in the user's program (as opposed to a bug in the compiler).
    /// Carries the location of the error when it is known.
    /// </summary>
    public sealed class CompilationError : Exception {
        public CompilationError(String message, SourceLocation location)
            : base(message) {
            this.Location = location;
        }

        public CompilationError(String message)
            : this(message, new SourceLocation()) { }

        public SourceLocation Location { get; }

        /// <summary>
        /// Formats the error the way gcc and clang do: "file:line:col: error: message".
        /// </summary>
        public String Format(String fileName) =>
            this.Location.IsValid
                ? $"{fileName}:{this.Location}: error: {this.Message}"
                : $"{fileName}: error: {this.Message}";
    }
}
