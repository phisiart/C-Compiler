using System;

namespace LexicalAnalysis {
    public enum TokenKind {
        NONE,
        FLOAT,
        INT,
        CHAR,
        STRING,
        IDENTIFIER,
        KEYWORD,
        OPERATOR
    }

    public abstract class Token {
        public override String ToString() {
            return this.Kind.ToString();
        }
        public abstract TokenKind Kind { get; }

        /// <summary>
        /// Where the token starts in the source file. Filled in by the scanner.
        /// </summary>
        public SourceLocation Location { get; internal set; }

        /// <summary>
        /// The exact source text of the token. Filled in by the scanner.
        /// </summary>
        public String Text { get; internal set; } = "";

        /// <summary>
        /// A human-readable description of the token, for error messages.
        /// </summary>
        public virtual String Describe() => $"'{this.Text}'";
    }

    public sealed class EmptyToken : Token {
        public override TokenKind Kind { get; } = TokenKind.NONE;
        public override String Describe() => "end of file";
    }

    public sealed class FSASpace : FSA {
        private enum State {
            START,
            END,
            ERROR,
            SPACE
        };

        private State _state;

        public FSASpace() {
            this._state = State.START;
        }

        public override void Reset() {
            this._state = State.START;
        }

        public override FSAStatus GetStatus() {
            if (this._state == State.START) {
                return FSAStatus.NONE;
            }
            if (this._state == State.END) {
                return FSAStatus.END;
            }
            if (this._state == State.ERROR) {
                return FSAStatus.ERROR;
            }
            return FSAStatus.RUNNING;
        }

        public override Token RetrieveToken() {
            return new EmptyToken();
        }

        public override void ReadChar(Char ch) {
            switch (this._state) {
                case State.END:
                case State.ERROR:
                    this._state = State.ERROR;
                    break;
                case State.START:
                    if (Utils.IsSpace(ch)) {
                        this._state = State.SPACE;
                    } else {
                        this._state = State.ERROR;
                    }
                    break;
                case State.SPACE:
                    if (Utils.IsSpace(ch)) {
                        this._state = State.SPACE;
                    } else {
                        this._state = State.END;
                    }
                    break;
            }
        }

        public override void ReadEOF() {
            switch (this._state) {
                case State.SPACE:
                    this._state = State.END;
                    break;
                default:
                    this._state = State.ERROR;
                    break;
            }
        }
    }

    public sealed class FSANewLine : FSA {
        private enum State {
            START,
            END,
            ERROR,
            NEWLINE
        };

        private State _state;

        public FSANewLine() {
            this._state = State.START;
        }

        public override void Reset() {
            this._state = State.START;
        }

        public override FSAStatus GetStatus() {
            if (this._state == State.START) {
                return FSAStatus.NONE;
            }
            if (this._state == State.END) {
                return FSAStatus.END;
            }
            if (this._state == State.ERROR) {
                return FSAStatus.ERROR;
            }
            return FSAStatus.RUNNING;
        }

        public override Token RetrieveToken() {
            return new EmptyToken();
        }

        public override void ReadChar(Char ch) {
            switch (this._state) {
                case State.END:
                case State.ERROR:
                    this._state = State.ERROR;
                    break;
                case State.START:
                    if (ch == '\n') {
                        this._state = State.NEWLINE;
                    } else {
                        this._state = State.ERROR;
                    }
                    break;
                case State.NEWLINE:
                    this._state = State.END;
                    break;
            }
        }

        public override void ReadEOF() {
            switch (this._state) {
                case State.NEWLINE:
                    this._state = State.END;
                    break;
                default:
                    this._state = State.ERROR;
                    break;
            }
        }
    }

    /// <summary>
    /// Recognizes "// ..." and "/* ... */" comments. Like whitespace, comments
    /// produce no token.
    /// </summary>
    public sealed class FSAComment : FSA {
        private enum State {
            START,
            END,
            ERROR,
            SLASH,
            LINE,
            BLOCK,
            BLOCK_STAR,
            BLOCK_DONE
        };

        private State _state;

        public FSAComment() {
            this._state = State.START;
        }

        public override void Reset() {
            this._state = State.START;
        }

        public override FSAStatus GetStatus() {
            if (this._state == State.START) {
                return FSAStatus.NONE;
            }
            if (this._state == State.END) {
                return FSAStatus.END;
            }
            if (this._state == State.ERROR) {
                return FSAStatus.ERROR;
            }
            return FSAStatus.RUNNING;
        }

        public override Token RetrieveToken() {
            return new EmptyToken();
        }

        public override void ReadChar(Char ch) {
            switch (this._state) {
                case State.END:
                case State.ERROR:
                    this._state = State.ERROR;
                    break;
                case State.START:
                    this._state = ch == '/' ? State.SLASH : State.ERROR;
                    break;
                case State.SLASH:
                    if (ch == '/') {
                        this._state = State.LINE;
                    } else if (ch == '*') {
                        this._state = State.BLOCK;
                    } else {
                        this._state = State.ERROR;
                    }
                    break;
                case State.LINE:
                    // The newline itself is left for FSANewLine.
                    this._state = ch == '\n' ? State.END : State.LINE;
                    break;
                case State.BLOCK:
                    this._state = ch == '*' ? State.BLOCK_STAR : State.BLOCK;
                    break;
                case State.BLOCK_STAR:
                    if (ch == '/') {
                        this._state = State.BLOCK_DONE;
                    } else if (ch == '*') {
                        this._state = State.BLOCK_STAR;
                    } else {
                        this._state = State.BLOCK;
                    }
                    break;
                case State.BLOCK_DONE:
                    this._state = State.END;
                    break;
            }
        }

        public override void ReadEOF() {
            switch (this._state) {
                case State.LINE:
                case State.BLOCK_DONE:
                    this._state = State.END;
                    break;
                default:
                    this._state = State.ERROR;
                    break;
            }
        }
    }
}
