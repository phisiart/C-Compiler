using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

// string literal
// --------------
public class TokenString : Token {
    public TokenString() {
        type = TokenType.STRING;
    }
    public string raw;
    public string val;
    public override string ToString() {
        return type.ToString() + ": " + "\"" + raw + "\"" + "\n\"" + val + "\"";
    }
}

public class FSAString : FSA {
    public enum StringState {
        START,
        END,
        ERROR,
        L,
        Q,
        QQ
    };

    public StringState state;
    public FSAString() {
        state = StringState.START;
        fsachar = new FSAChar('\"');
        raw = "";
        val = "";
    }
    public void Reset() {
        state = StringState.START;
        fsachar.Reset();
        raw = "";
        val = "";
    }
    public FSAStatus GetStatus() {
        if (state == StringState.START) {
            return FSAStatus.NONE;
        } else if (state == StringState.END) {
            return FSAStatus.END;
        } else if (state == StringState.ERROR) {
            return FSAStatus.ERROR;
        } else {
            return FSAStatus.RUN;
        }
    }
    public string val;
    public string raw;
    public Token RetrieveToken() {
        TokenString token = new TokenString();
        token.type = TokenType.STRING;
        token.val = val;
        token.raw = raw;
        return token;
    }
    public void ReadChar(char ch) {
        switch (state) {
        case StringState.END:
        case StringState.ERROR:
            state = StringState.ERROR;
            break;
        case StringState.START:
            switch (ch) {
            case 'L':
                state = StringState.L;
                break;
            case '\"':
                state = StringState.Q;
                fsachar.Reset();
                break;
            default:
                state = StringState.ERROR;
                break;
            }
            break;
        case StringState.L:
            if (ch == '\"') {
                state = StringState.Q;
                fsachar.Reset();
            } else {
                state = StringState.ERROR;
            }
            break;
        case StringState.Q:
            if (fsachar.GetStatus() == FSAStatus.NONE && ch == '\"') {
                state = StringState.QQ;
                fsachar.Reset();
            } else {
                fsachar.ReadChar(ch);
                switch (fsachar.GetStatus()) {
                case FSAStatus.END:
                    state = StringState.Q;
                    val = val + fsachar.RetrieveChar();
                    raw = raw + fsachar.RetrieveRaw();
                    fsachar.Reset();
                    ReadChar(ch);
                    break;
                case FSAStatus.ERROR:
                    state = StringState.ERROR;
                    break;
                default:
                    break;
                }
            }
            break;
        case StringState.QQ:
            state = StringState.END;
            break;
        default:
            state = StringState.ERROR;
            break;
        }
    }

    public void ReadEOF() {
        if (state == StringState.QQ) {
            state = StringState.END;
        } else {
            state = StringState.ERROR;
        }
    }
    private FSAChar fsachar;
}
