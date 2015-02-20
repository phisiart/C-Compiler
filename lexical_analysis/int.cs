using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

// int
// ---
// there are four types of integers: signed, unsigned, signed long, unsigned long
public enum IntSuffix {
    NONE,
    U,
    L,
    UL
};

public class TokenInt : Token {
    public TokenInt(Int64 _val, IntSuffix _suffix, String _raw)
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
    public readonly String raw;
    public readonly IntSuffix suffix;
}

public class FSAInt : FSA {
    Int64 val;
    string raw;
    IntSuffix int_type;

    public enum IntState {
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
    public IntState state;
    public FSAInt() {
        state = IntState.START;
        val = 0;
        raw = "";
        int_type = IntSuffix.NONE;
    }

    public void Reset() {
        state = IntState.START;
        val = 0;
        raw = "";
        int_type = IntSuffix.NONE;
    }

    public FSAStatus GetStatus() {
        switch (state) {
        case IntState.START:
            return FSAStatus.NONE;
        case IntState.END:
            return FSAStatus.END;
        case IntState.ERROR:
            return FSAStatus.ERROR;
        default:
            return FSAStatus.RUN;
        }
    }


    public Token RetrieveToken() {
        return new TokenInt(val, int_type, raw.Substring(0, raw.Length - 1));
    }

    public void ReadChar(char ch) {
        raw += ch;
        switch (state) {
        case IntState.ERROR:
        case IntState.END:
            state = IntState.ERROR;
            break;
        case IntState.START:
            if (ch == '0') {
                state = IntState.Z;
            } else if (char.IsDigit(ch)) {
                state = IntState.D;
                val += ch - '0';
            } else {
                state = IntState.ERROR;
            }
            break;
        case IntState.Z:
            if (ch == 'x' || ch == 'X') {
                state = IntState.ZX;
            } else if (ch >= '0' && ch <= '7') {
                val *= 8;
                val += ch - '0';
                state = IntState.O;
            } else {
                state = IntState.END;
            }
            break;
        case IntState.D:
            if (char.IsDigit(ch)) {
                val *= 10;
                val += ch - '0';
                state = IntState.D;
            } else if (ch == 'u' || ch == 'U') {
                int_type = IntSuffix.U;
                state = IntState.U;
            } else if (ch == 'l' || ch == 'L') {
                int_type = IntSuffix.L;
                state = IntState.L;
            } else {
                state = IntState.END;
            }
            break;
        case IntState.ZX:
            if ((ch >= '0' && ch <= '9')
                || (ch >= 'a' && ch <= 'f')
                || (ch >= 'A' && ch <= 'F')) {
                val *= 16;
                if (ch >= '0' && ch <= '9') {
                    val += ch - '0';
                } else if (ch >= 'a' && ch <= 'f') {
                    val += ch - 'a' + 0xA;
                } else if (ch >= 'A' && ch <= 'F') {
                    val += ch - 'A' + 0xA;
                }
                state = IntState.H;
            } else {
                state = IntState.ERROR;
            }
            break;
        case IntState.O:
            if (ch >= '0' && ch <= '7') {
                val *= 8;
                val += ch - '0';
                state = IntState.O;
            } else if (ch == 'u' || ch == 'U') {
                int_type = IntSuffix.U;
                state = IntState.U;
            } else if (ch == 'l' || ch == 'L') {
                int_type = IntSuffix.L;
                state = IntState.L;
            } else {
                state = IntState.END;
            }
            break;
        case IntState.L:
            if (ch == 'u' || ch == 'U') {
                int_type = IntSuffix.UL;
                state = IntState.UL;
            } else {
                state = IntState.END;
            }
            break;
        case IntState.H:
            if ((ch >= '0' && ch <= '9')
                || (ch >= 'a' && ch <= 'f')
                || (ch >= 'A' && ch <= 'F')) {
                val *= 16;
                if (ch >= '0' && ch <= '9') {
                    val += ch - '0';
                } else if (ch >= 'a' && ch <= 'f') {
                    val += ch - 'a' + 0xA;
                } else if (ch >= 'A' && ch <= 'F') {
                    val += ch - 'A' + 0xA;
                }
                state = IntState.H;
            } else if (ch == 'u' || ch == 'U') {
                int_type = IntSuffix.U;
                state = IntState.U;
            } else if (ch == 'l' || ch == 'L') {
                int_type = IntSuffix.L;
                state = IntState.L;
            } else {
                state = IntState.END;
            }
            break;
        case IntState.U:
            if (ch == 'l' || ch == 'L') {
                int_type = IntSuffix.UL;
                state = IntState.UL;
            } else {
                state = IntState.END;
            }
            break;
        case IntState.UL:
            state = IntState.END;
            break;
        default:
            state = IntState.ERROR;
            break;
        }
    }

    public void ReadEOF() {
        switch (state) {
        case IntState.D:
        case IntState.Z:
        case IntState.O:
        case IntState.L:
        case IntState.H:
        case IntState.U:
        case IntState.UL:
            state = IntState.END;
            break;
        default:
            state = IntState.ERROR;
            break;
        }
    }


}
