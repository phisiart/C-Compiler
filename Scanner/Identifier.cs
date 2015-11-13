using System;

namespace LexicalAnalysis {
    /// <summary>
    /// If the identifier is found to be a keyword, then it will be a keyword
    /// </summary>
    public sealed class TokenIdentifier : Token {
        public TokenIdentifier(String val) {
            this.Val = val;
        }

        public override TokenKind Kind { get; } = TokenKind.IDENTIFIER;
        public String Val { get; }
        public override String ToString() {
            return this.Kind + ": " + this.Val;
        }
    }

    public sealed class FSAIdentifier : FSA {
        private enum State {
            START,
            END,
            ERROR,
            ID
        };
        private State _state;
        private String _scanned;

        public FSAIdentifier() {
            this._state = State.START;
            this._scanned = "";
        }

        public override void Reset() {
            this._state = State.START;
            this._scanned = "";
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
            String name = this._scanned.Substring(0, this._scanned.Length - 1);
            if (TokenKeyword.Keywords.ContainsKey(name)) {
                return new TokenKeyword(TokenKeyword.Keywords[name]);
            }
            return new TokenIdentifier(name);
        }

        public override void ReadChar(Char ch) {
            this._scanned = this._scanned + ch;
            switch (this._state) {
                case State.END:
                case State.ERROR:
                    this._state = State.ERROR;
                    break;
                case State.START:
                    if (ch == '_' || Char.IsLetter(ch)) {
                        this._state = State.ID;
                    } else {
                        this._state = State.ERROR;
                    }
                    break;
                case State.ID:
                    if (Char.IsLetterOrDigit(ch) || ch == '_') {
                        this._state = State.ID;
                    } else {
                        this._state = State.END;
                    }
                    break;
            }
        }

        public override void ReadEOF() {
            this._scanned = this._scanned + '0';
            switch (this._state) {
                case State.ID:
                    this._state = State.END;
                    break;
                default:
                    this._state = State.ERROR;
                    break;
            }
        }
    }
}