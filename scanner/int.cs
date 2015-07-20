using System;

// Int32
// ---
// there are four types of integers: signed, unsigned, signed long, unsigned long
public enum IntSuffix {
    NONE,
    U,
    L,
    UL
};

public class TokenInt : Token {
    public TokenInt(Int64 _val, IntSuffix _suffix, string _raw)
        : base(TokenType.INT) {
        val = _val;
        suffix = _suffix;
        raw = _raw;
    }

    public override string ToString() {
        string str = type.ToString();
        switch (suffix) {
        case IntSuffix.L:
            str += "(long)";
            break;
        case IntSuffix.U:
            str += "(unsigned)";
            break;
        case IntSuffix.UL:
            str += "(unsigned long)";
            break;
        default:
            break;
        }
        return str + ": " + val.ToString() + " \"" + raw + "\"";
    }

    public readonly Int64 val;
    public readonly string raw;
    public readonly IntSuffix suffix;
}

public class FSAInt : FSA {
    private enum State {
        START,
        END,
        ERROR,
        Z,
        O,
        D,
        ZX,
        H,
        L,
        U,
        UL
    };

    private Int64 val;
    private string raw;
    private IntSuffix suffix;
    private State state;

    public FSAInt() {
        state = State.START;
        val = 0;
        raw = "";
        suffix = IntSuffix.NONE;
    }

    public override sealed void Reset() {
        state = State.START;
        val = 0;
        raw = "";
        suffix = IntSuffix.NONE;
    }

    public override sealed FSAStatus GetStatus() {
        switch (state) {
        case State.START:
            return FSAStatus.NONE;
        case State.END:
            return FSAStatus.END;
        case State.ERROR:
            return FSAStatus.ERROR;
        default:
            return FSAStatus.RUN;
        }
    }

    public override sealed Token RetrieveToken() {
        return new TokenInt(val, suffix, raw.Substring(0, raw.Length - 1));
    }

    public override sealed void ReadChar(Char ch) {
        raw += ch;
        switch (state) {
        case State.ERROR:
        case State.END:
            state = State.ERROR;
            break;
        case State.START:
            if (ch == '0') {
                state = State.Z;
            } else if (Char.IsDigit(ch)) {
                state = State.D;
                val += ch - '0';
            } else {
                state = State.ERROR;
            }
            break;
        case State.Z:
            if (ch == 'x' || ch == 'X') {
                state = State.ZX;
            } else if (Utils.IsOctDigit(ch)) {
                val *= 8;
                val += ch - '0';
                state = State.O;
            } else if (ch == 'u' || ch == 'U') {
                suffix = IntSuffix.U;
                state = State.U;
            } else if (ch == 'l' || ch == 'L') {
                suffix = IntSuffix.L;
                state = State.L;
            } else {
                state = State.END;
            }
            break;
        case State.D:
            if (Char.IsDigit(ch)) {
                val *= 10;
                val += ch - '0';
                state = State.D;
            } else if (ch == 'u' || ch == 'U') {
                suffix = IntSuffix.U;
                state = State.U;
            } else if (ch == 'l' || ch == 'L') {
                suffix = IntSuffix.L;
                state = State.L;
            } else {
                state = State.END;
            }
            break;
        case State.ZX:
            if (Utils.IsHexDigit(ch)) {
                val *= 0x10;
                val += Utils.GetHexDigit(ch);
                state = State.H;
            } else {
                state = State.ERROR;
            }
            break;
        case State.O:
            if (Utils.IsOctDigit(ch)) {
                val *= 8;
                val += ch - '0';
                state = State.O;
            } else if (ch == 'u' || ch == 'U') {
                suffix = IntSuffix.U;
                state = State.U;
            } else if (ch == 'l' || ch == 'L') {
                suffix = IntSuffix.L;
                state = State.L;
            } else {
                state = State.END;
            }
            break;
        case State.L:
            if (ch == 'u' || ch == 'U') {
                suffix = IntSuffix.UL;
                state = State.UL;
            } else {
                state = State.END;
            }
            break;
        case State.H:
            if (Utils.IsHexDigit(ch)) {
                val *= 0x10;
                val += Utils.GetHexDigit(ch);
                state = State.H;
            } else if (ch == 'u' || ch == 'U') {
                suffix = IntSuffix.U;
                state = State.U;
            } else if (ch == 'l' || ch == 'L') {
                suffix = IntSuffix.L;
                state = State.L;
            } else {
                state = State.END;
            }
            break;
        case State.U:
            if (ch == 'l' || ch == 'L') {
                suffix = IntSuffix.UL;
                state = State.UL;
            } else {
                state = State.END;
            }
            break;
        case State.UL:
            state = State.END;
            break;
        default:
            state = State.ERROR;
            break;
        }
    }

    public override sealed void ReadEOF() {
        switch (state) {
        case State.D:
        case State.Z:
        case State.O:
        case State.L:
        case State.H:
        case State.U:
        case State.UL:
            state = State.END;
            break;
        default:
            state = State.ERROR;
            break;
        }
    }

}
