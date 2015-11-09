using System;

// TokenFloatConst
// ===============
// The token representing a floating number.
// It can either be a float or double.
// 
public sealed class TokenFloat : Token {

    public enum FloatSuffix {
        NONE,
        F,
        L
    }

    public TokenFloat(Double value, FloatSuffix suffix, String source) {
        this.Value = value;
        this.Suffix = suffix;
        this.Source = source;
    }

    public override TokenKind Kind { get; } = TokenKind.FLOAT;
    public Double Value { get; }
    public String Source { get; }
    public FloatSuffix Suffix { get; }

    public override String ToString() {
        String str = this.Kind.ToString();
        switch (this.Suffix) {
            case FloatSuffix.F:
                str += "(float)";
                break;
            case FloatSuffix.L:
                str += "(long double)";
                break;
            default:
                str += "(double)";
                break;
        }
        return str + ": " + this.Value + " \"" + this.Source + "\"";
    }
}

// FSAFloat
// ========
// The FSA for scanning a float.
// 
public sealed class FSAFloat : FSA {
    private enum State {
        START,
        END,
        ERROR,
        D,
        P,
        DP,
        PD,
        DE,
        DES,
        DED,
        PDF,
        DPL
    };

    private String _raw;
    private Int64 _intPart;
    private Int64 _fracPart;
    private Int64 _fracCount;
    private Int64 _expPart;
    private Boolean _expPos;
    private TokenFloat.FloatSuffix _suffix;
    private State _state;

    public FSAFloat() {
        this._state = State.START;
        this._intPart = 0;
        this._fracPart = 0;
        this._fracCount = 0;
        this._expPart = 0;
        this._suffix = TokenFloat.FloatSuffix.NONE;
        this._expPos = true;
        this._raw = "";
    }

    public override void Reset() {
        this._state = State.START;
        this._intPart = 0;
        this._fracPart = 0;
        this._fracCount = 0;
        this._expPart = 0;
        this._suffix = TokenFloat.FloatSuffix.NONE;
        this._expPos = true;
        this._raw = "";
    }

    public override FSAStatus GetStatus() {
        switch (this._state) {
            case State.START:
                return FSAStatus.NONE;
            case State.END:
                return FSAStatus.END;
            case State.ERROR:
                return FSAStatus.ERROR;
            default:
                return FSAStatus.RUNNING;
        }
    }

    public override Token RetrieveToken() {
        Double val;
        if (this._expPos) {
            val = (this._intPart + this._fracPart * Math.Pow(0.1, this._fracCount)) * Math.Pow(10, this._expPart);
        } else {
            val = (this._intPart + this._fracPart * Math.Pow(0.1, this._fracCount)) * Math.Pow(10, -this._expPart);
        }
        return new TokenFloat(val, this._suffix, this._raw.Substring(0, this._raw.Length - 1));
    }

    public override void ReadChar(Char ch) {
        this._raw += ch;
        switch (this._state) {
            case State.ERROR:
            case State.END:
                this._state = State.ERROR;
                break;

            case State.START:
                if (Char.IsDigit(ch)) {
                    this._intPart = ch - '0';
                    this._state = State.D;
                } else if (ch == '.') {
                    this._state = State.P;
                } else {
                    this._state = State.ERROR;
                }
                break;

            case State.D:
                if (Char.IsDigit(ch)) {
                    this._intPart *= 10;
                    this._intPart += ch - '0';
                    this._state = State.D;
                } else if (ch == 'e' || ch == 'E') {
                    this._state = State.DE;
                } else if (ch == '.') {
                    this._state = State.DP;
                } else {
                    this._state = State.ERROR;
                }
                break;

            case State.P:
                if (Char.IsDigit(ch)) {
                    this._fracPart = ch - '0';
                    this._fracCount = 1;
                    this._state = State.PD;
                } else {
                    this._state = State.ERROR;
                }
                break;

            case State.DP:
                if (Char.IsDigit(ch)) {
                    this._fracPart = ch - '0';
                    this._fracCount = 1;
                    this._state = State.PD;
                } else if (ch == 'e' || ch == 'E') {
                    this._state = State.DE;
                } else if (ch == 'f' || ch == 'F') {
                    this._suffix = TokenFloat.FloatSuffix.F;
                    this._state = State.PDF;
                } else if (ch == 'l' || ch == 'L') {
                    this._suffix = TokenFloat.FloatSuffix.L;
                    this._state = State.DPL;
                } else {
                    this._state = State.END;
                }
                break;

            case State.PD:
                if (Char.IsDigit(ch)) {
                    this._fracPart *= 10;
                    this._fracPart += ch - '0';
                    this._fracCount++;
                    this._state = State.PD;
                } else if (ch == 'e' || ch == 'E') {
                    this._state = State.DE;
                } else if (ch == 'f' || ch == 'F') {
                    this._suffix = TokenFloat.FloatSuffix.F;
                    this._state = State.PDF;
                } else if (ch == 'l' || ch == 'L') {
                    this._suffix = TokenFloat.FloatSuffix.L;
                    this._state = State.DPL;
                } else {
                    this._state = State.END;
                }
                break;

            case State.DE:
                if (Char.IsDigit(ch)) {
                    this._expPart = ch - '0';
                    this._state = State.DED;
                } else if (ch == '+' || ch == '-') {
                    if (ch == '-') {
                        this._expPos = false;
                    }
                    this._state = State.DES;
                } else {
                    this._state = State.ERROR;
                }
                break;

            case State.DES:
                if (Char.IsDigit(ch)) {
                    this._expPart = ch - '0';
                    this._state = State.DED;
                } else {
                    this._state = State.ERROR;
                }
                break;

            case State.DPL:
                this._suffix = TokenFloat.FloatSuffix.L;
                this._state = State.END;
                break;

            case State.DED:
                if (Char.IsDigit(ch)) {
                    this._expPart *= 10;
                    this._expPart += ch - '0';
                    this._state = State.DED;
                } else if (ch == 'f' || ch == 'F') {
                    this._suffix = TokenFloat.FloatSuffix.F;
                    this._state = State.PDF;
                } else if (ch == 'l' || ch == 'L') {
                    this._suffix = TokenFloat.FloatSuffix.L;
                    this._state = State.DPL;
                } else {
                    this._state = State.END;
                }
                break;

            case State.PDF:
                this._state = State.END;
                break;

            default:
                this._state = State.ERROR;
                break;
        }

    }

    public override void ReadEOF() {
        switch (this._state) {
            case State.DP:
            case State.PD:
            case State.DED:
            case State.PDF:
            case State.DPL:
                this._state = State.END;
                break;
            default:
                this._state = State.ERROR;
                break;
        }
    }

}

