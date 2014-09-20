using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

// int
// ---
// there are four types of integers: signed, unsigned, signed long, unsigned long
public enum IntType {
    NONE,
    U,
    L,
    UL
};

public class TokenInt : Token {
    public TokenInt() {
        type = TokenType.INT;
        int_type = IntType.NONE;
    }
    public override string ToString() {
        string str = type.ToString();
        switch (int_type) {
        case IntType.L:
            str += "(long)";
            break;
        case IntType.U:
            str += "(unsigned)";
            break;
        case IntType.UL:
            str += "(unsigned long)";
            break;
        default:
            break;
        }
        return str + ": " + val.ToString() + " \"" + raw + "\"";
    }

    public Int64 val;
    public string raw;
    public IntType int_type;
}

class FSAInt : FSA {
    Int64 val;
    string raw;
    IntType int_type;

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
        int_type = IntType.NONE;
    }

    public void Reset() {
        state = IntState.START;
        val = 0;
        raw = "";
        int_type = IntType.NONE;
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
        TokenInt token = new TokenInt();
        token.type = TokenType.INT;
        token.int_type = int_type;
        token.raw = raw.Substring(0, raw.Length - 1);
        token.val = val;
        return token;
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
                int_type = IntType.U;
                state = IntState.U;
            } else if (ch == 'l' || ch == 'L') {
                int_type = IntType.L;
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
                int_type = IntType.U;
                state = IntState.U;
            } else if (ch == 'l' || ch == 'L') {
                int_type = IntType.L;
                state = IntState.L;
            } else {
                state = IntState.END;
            }
            break;
        case IntState.L:
            if (ch == 'u' || ch == 'U') {
                int_type = IntType.UL;
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
                int_type = IntType.U;
                state = IntState.U;
            } else if (ch == 'l' || ch == 'L') {
                int_type = IntType.L;
                state = IntState.L;
            } else {
                state = IntState.END;
            }
            break;
        case IntState.U:
            if (ch == 'l' || ch == 'L') {
                int_type = IntType.UL;
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
