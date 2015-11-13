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
    }

    public sealed class EmptyToken : Token {
        public override TokenKind Kind { get; } = TokenKind.NONE;
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
}