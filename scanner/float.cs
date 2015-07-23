using System;

// TokenFloatConst
// ===============
// The token representing a floating number.
// It can either be a float or double.
// 
public class TokenFloat : Token {

    public enum Suffix {
        NONE,
        F,
        L
    }

    public TokenFloat(Double value, Suffix suffix, String source)
        : base(TokenType.FLOAT) {
        this.value = value;
        this.suffix = suffix;
        this.source = source;
    }

    public readonly Double value;
    public readonly String source;
    public readonly Suffix suffix;

    public override String ToString() {
        String str = type.ToString();
        switch (suffix) {
        case Suffix.F:
            str += "(float)";
            break;
        case Suffix.L:
            str += "(long double)";
            break;
        default:
            str += "(double)";
            break;
        }
        return str + ": " + value.ToString() + " \"" + source + "\"";
    }
}

// FSAFloat
// ========
// The FSA for scanning a float.
// 
public class FSAFloat : FSA {
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

    private String raw;
    private Int64 int_part;
    private Int64 frac_part;
    private Int64 frac_count;
    private Int64 exp_part;
    private Boolean exp_pos;
    private TokenFloat.Suffix suffix;
    private State state;

    public FSAFloat() {
        state = State.START;
        int_part = 0;
        frac_part = 0;
        frac_count = 0;
        exp_part = 0;
        suffix = TokenFloat.Suffix.NONE;
        exp_pos = true;
        raw = "";
    }

    public override sealed void Reset() {
        state = State.START;
        int_part = 0;
        frac_part = 0;
        frac_count = 0;
        exp_part = 0;
        suffix = TokenFloat.Suffix.NONE;
        exp_pos = true;
        raw = "";
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
        Double val;
        if (exp_pos) {
            val = (int_part + frac_part * Math.Pow(0.1, frac_count)) * Math.Pow(10, exp_part);
        } else {
            val = (int_part + frac_part * Math.Pow(0.1, frac_count)) * Math.Pow(10, -exp_part);
        }
        return new TokenFloat(val, suffix, raw.Substring(0, raw.Length - 1));
    }

    public override sealed void ReadChar(Char ch) {
        raw += ch;
        switch (state) {
        case State.ERROR:
        case State.END:
            state = State.ERROR;
            break;

        case State.START:
            if (Char.IsDigit(ch)) {
                int_part = ch - '0';
                state = State.D;
            } else if (ch == '.') {
                state = State.P;
            } else {
                state = State.ERROR;
            }
            break;

        case State.D:
            if (Char.IsDigit(ch)) {
                int_part *= 10;
                int_part += ch - '0';
                state = State.D;
            } else if (ch == 'e' || ch == 'E') {
                state = State.DE;
            } else if (ch == '.') {
                state = State.DP;
            } else {
                state = State.ERROR;
            }
            break;

        case State.P:
            if (Char.IsDigit(ch)) {
                frac_part = ch - '0';
                frac_count = 1;
                state = State.PD;
            } else {
                state = State.ERROR;
            }
            break;

        case State.DP:
            if (Char.IsDigit(ch)) {
                frac_part = ch - '0';
                frac_count = 1;
                state = State.PD;
            } else if (ch == 'e' || ch == 'E') {
                state = State.DE;
            } else if (ch == 'f' || ch == 'F') {
                suffix = TokenFloat.Suffix.F;
                state = State.PDF;
            } else if (ch == 'l' || ch == 'L') {
                suffix = TokenFloat.Suffix.L;
                state = State.DPL;
            } else {
                state = State.END;
            }
            break;

        case State.PD:
            if (Char.IsDigit(ch)) {
                frac_part *= 10;
                frac_part += ch - '0';
                frac_count++;
                state = State.PD;
            } else if (ch == 'e' || ch == 'E') {
                state = State.DE;
            } else if (ch == 'f' || ch == 'F') {
                suffix = TokenFloat.Suffix.F;
                state = State.PDF;
            } else if (ch == 'l' || ch == 'L') {
                suffix = TokenFloat.Suffix.L;
                state = State.DPL;
            } else {
                state = State.END;
            }
            break;

        case State.DE:
            if (Char.IsDigit(ch)) {
                exp_part = ch - '0';
                state = State.DED;
            } else if (ch == '+' || ch == '-') {
                if (ch == '-') {
                    exp_pos = false;
                }
                state = State.DES;
            } else {
                state = State.ERROR;
            }
            break;

        case State.DES:
            if (Char.IsDigit(ch)) {
                exp_part = ch - '0';
                state = State.DED;
            } else {
                state = State.ERROR;
            }
            break;

        case State.DPL:
            suffix = TokenFloat.Suffix.L;
            state = State.END;
            break;

        case State.DED:
            if (Char.IsDigit(ch)) {
                exp_part *= 10;
                exp_part += ch - '0';
                state = State.DED;
            } else if (ch == 'f' || ch == 'F') {
                suffix = TokenFloat.Suffix.F;
                state = State.PDF;
            } else if (ch == 'l' || ch == 'L') {
                suffix = TokenFloat.Suffix.L;
                state = State.DPL;
            } else {
                state = State.END;
            }
            break;

        case State.PDF:
            state = State.END;
            break;

        default:
            state = State.ERROR;
            break;
        }

    }

    public override sealed void ReadEOF() {
        switch (state) {
        case State.DP:
        case State.PD:
        case State.DED:
        case State.PDF:
        case State.DPL:
            state = State.END;
            break;
        default:
            state = State.ERROR;
            break;
        }
    }

}

