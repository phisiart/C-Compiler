using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

// char
// ----
public class TokenChar : Token {
    public TokenChar() {
        type = TokenType.CHAR;
    }
    public string raw;
    public char val;
    public override string ToString() {
        return type.ToString() + ": " + "\'" + raw + "\'" + "\n\'" + val + "\'";
    }
}

public class FSAChar : FSA {
    public enum CharState {
        START,
        END,
        ERROR,
        S,
        C,
        SO,
        SOO,
        SOOO,
        SX,
        SXH,
        SXHH
    };

    public CharState state;
    public FSAChar(char _quote) {
        state = CharState.START;
        quote = _quote;
        str = "";
    }
    public void Reset() {
        str = "";
        state = CharState.START;
    }
    public FSAStatus GetStatus() {
        if (state == CharState.START) {
            return FSAStatus.NONE;
        } else if (state == CharState.END) {
            return FSAStatus.END;
        } else if (state == CharState.ERROR) {
            return FSAStatus.ERROR;
        } else {
            return FSAStatus.RUN;
        }
    }

    private char quote;
    private bool IsChar(char ch) {
        return ch != quote && ch != '\\' && ch != '\n';
    }

    private bool IsEscapeChar(char ch) {
        switch (ch) {
        case 'a':
        case 'b':
        case 'f':
        case 'n':
        case 'r':
        case 't':
        case 'v':
        case '\'':
        case '\"':
        case '\\':
        case '?':
            return true;
        default:
            return false;
        }
    }
    string str;
    public string RetrieveRaw() {
        return str.Substring(0, str.Length - 1);
    }
    public char RetrieveChar() {
        if (str.Length == 3) {
            switch (str[1]) {
            case 'a':
                return '\a';
            case 'b':
                return '\b';
            case 'f':
                return '\f';
            case 'n':
                return '\n';
            case 'r':
                return '\r';
            case 't':
                return '\t';
            case 'v':
                return '\v';
            case '\'':
                return '\'';
            case '\"':
                return '\"';
            case '\\':
                return '\\';
            case '?':
                return '?';
            default:
                return '?';
            }
        } else {
            return str[0];
        }
    }

    public Token RetrieveToken() {
        return new Token();
    }
    public void ReadChar(char ch) {
        str = str + ch;
        switch (state) {
        case CharState.END:
        case CharState.ERROR:
            state = CharState.ERROR;
            break;
        case CharState.START:
            if (IsChar(ch)) {
                state = CharState.C;
            } else if (ch == '\\') {
                state = CharState.S;
            } else {
                state = CharState.ERROR;
            }
            break;
        case CharState.C:
            state = CharState.END;
            break;
        case CharState.S:
            if (IsEscapeChar(ch)) {
                state = CharState.C;
            } else if (ch >= '0' && ch <= '7') {
                state = CharState.SO;
            } else if (ch == 'x' || ch == 'X') {
                state = CharState.SX;
            } else {
                state = CharState.ERROR;
            }
            break;
        case CharState.SX:
            if ((ch >= '0' && ch <= '9')
                || (ch >= 'a' && ch <= 'f')
                || (ch >= 'A' && ch <= 'F')) {
                state = CharState.SXH;
            } else {
                state = CharState.ERROR;
            }
            break;
        case CharState.SXH:
            if ((ch >= '0' && ch <= '9')
                || (ch >= 'a' && ch <= 'f')
                || (ch >= 'A' && ch <= 'F')) {
                state = CharState.SXHH;
            } else {
                state = CharState.END;
            }
            break;
        case CharState.SXHH:
            state = CharState.END;
            break;
        case CharState.SO:
            if (ch >= '0' && ch <= '7') {
                state = CharState.SOO;
            } else {
                state = CharState.END;
            }
            break;
        case CharState.SOO:
            if (ch >= '0' && ch <= '7') {
                state = CharState.SOOO;
            } else {
                state = CharState.END;
            }
            break;
        case CharState.SOOO:
            state = CharState.END;
            break;
        default:
            state = CharState.ERROR;
            break;
        }
    }

    public void ReadEOF() {
        str = str + '0';
        switch (state) {
        case CharState.C:
        case CharState.SO:
        case CharState.SOO:
        case CharState.SOOO:
        case CharState.SXH:
        case CharState.SXHH:
            state = CharState.END;
            break;
        default:
            state = CharState.ERROR;
            break;
        }
    }

}

public class FSACharConst : FSA {
    public enum CharConstState {
        START,
        END,
        ERROR,
        L,
        Q,
        QC,
        QCQ
    };

    public CharConstState state;
    public FSACharConst() {
        state = CharConstState.START;
        fsachar = new FSAChar('\'');
    }
    public void Reset() {
        state = CharConstState.START;
        fsachar.Reset();
    }
    public FSAStatus GetStatus() {
        if (state == CharConstState.START) {
            return FSAStatus.NONE;
        } else if (state == CharConstState.END) {
            return FSAStatus.END;
        } else if (state == CharConstState.ERROR) {
            return FSAStatus.ERROR;
        } else {
            return FSAStatus.RUN;
        }
    }

    char val;
    string raw;

    public Token RetrieveToken() {
        TokenChar token = new TokenChar();
        token.type = TokenType.CHAR;
        token.val = val;
        token.raw = raw;
        return token;
    }
    public void ReadChar(char ch) {
        switch (state) {
        case CharConstState.END:
        case CharConstState.ERROR:
            state = CharConstState.ERROR;
            break;
        case CharConstState.START:
            switch (ch) {
            case 'L':
                state = CharConstState.L;
                break;
            case '\'':
                state = CharConstState.Q;
                fsachar.Reset();
                break;
            default:
                state = CharConstState.ERROR;
                break;
            }
            break;
        case CharConstState.L:
            if (ch == '\'') {
                state = CharConstState.Q;
                fsachar.Reset();
            } else {
                state = CharConstState.ERROR;
            }
            break;
        case CharConstState.Q:
            fsachar.ReadChar(ch);
            switch (fsachar.GetStatus()) {
            case FSAStatus.END:
                state = CharConstState.QC;
                raw = fsachar.RetrieveRaw();
                val = fsachar.RetrieveChar();
                fsachar.Reset();
                ReadChar(ch);
                break;
            case FSAStatus.ERROR:
                state = CharConstState.ERROR;
                break;
            default:
                break;
            }
            break;
        case CharConstState.QC:
            if (ch == '\'') {
                state = CharConstState.QCQ;
            } else {
                state = CharConstState.ERROR;
            }
            break;
        case CharConstState.QCQ:
            state = CharConstState.END;
            break;
        default:
            state = CharConstState.ERROR;
            break;
        }
    }

    public void ReadEOF() {
        if (state == CharConstState.QCQ) {
            state = CharConstState.END;
        } else {
            state = CharConstState.ERROR;
        }
    }

    private FSAChar fsachar;

}
