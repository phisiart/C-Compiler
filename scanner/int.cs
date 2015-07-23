using System;

// Int32
// ---
// there are four types of integers: signed, unsigned, signed long, unsigned long


public class TokenInt : Token {
    public enum Suffix {
        NONE,
        U,
        L,
        UL
    };

    public TokenInt(Int64 _val, Suffix _suffix, String _raw)
        : base(TokenType.INT) {
        val = _val;
        suffix = _suffix;
        raw = _raw;
    }

    public override String ToString() {
        String str = type.ToString();
        switch (suffix) {
        case Suffix.L:
            str += "(long)";
            break;
        case Suffix.U:
            str += "(unsigned)";
            break;
        case Suffix.UL:
            str += "(unsigned long)";
            break;
        default:
            break;
        }
        return str + ": " + val.ToString() + " \"" + raw + "\"";
    }

    public readonly Int64 val;
    public readonly String raw;
    public readonly Suffix suffix;
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
    private String raw;
    private TokenInt.Suffix suffix;
    private State state;

    public FSAInt() {
        state = State.START;
        val = 0;
        raw = "";
        suffix = TokenInt.Suffix.NONE;
    }

    public override sealed void Reset() {
        state = State.START;
        val = 0;
        raw = "";
        suffix = TokenInt.Suffix.NONE;
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
            return FSAStatus.RUNNING;
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
                suffix = TokenInt.Suffix.U;
                state = State.U;
            } else if (ch == 'l' || ch == 'L') {
                suffix = TokenInt.Suffix.L;
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
                suffix = TokenInt.Suffix.U;
                state = State.U;
            } else if (ch == 'l' || ch == 'L') {
                suffix = TokenInt.Suffix.L;
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
                suffix = TokenInt.Suffix.U;
                state = State.U;
            } else if (ch == 'l' || ch == 'L') {
                suffix = TokenInt.Suffix.L;
                state = State.L;
            } else {
                state = State.END;
            }
            break;
        case State.L:
            if (ch == 'u' || ch == 'U') {
                suffix = TokenInt.Suffix.UL;
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
                suffix = TokenInt.Suffix.U;
                state = State.U;
            } else if (ch == 'l' || ch == 'L') {
                suffix = TokenInt.Suffix.L;
                state = State.L;
            } else {
                state = State.END;
            }
            break;
        case State.U:
            if (ch == 'l' || ch == 'L') {
                suffix = TokenInt.Suffix.UL;
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
