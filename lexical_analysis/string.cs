using System;

// String literal
// --------------
public class TokenString : Token {
    public TokenString(String _val, Int32 _idx, String _raw)
        : base(TokenType.STRING) {
        val = _val;
        idx = _idx;
        raw = _raw;
    }
    public readonly String raw;
    public readonly String val;
    public readonly int idx;

    public override String ToString() {
        return type.ToString() + ": " + "\"" + raw + "\"" + "\n\"" + val + "\"";
    }
}

public class FSAString : FSA {
    private enum State {
        START,
        END,
        ERROR,
        L,
        Q,
        QQ
    };

    private State state;
    private FSAChar fsachar;
    public String val;
    public String raw;

    public FSAString() {
        state = State.START;
        fsachar = new FSAChar('\"');
        raw = "";
        val = "";
    }

    public override sealed void Reset() {
        state = State.START;
        fsachar.Reset();
        raw = "";
        val = "";
    }

    public override sealed FSAStatus GetStatus() {
        if (state == State.START) {
            return FSAStatus.NONE;
        } else if (state == State.END) {
            return FSAStatus.END;
        } else if (state == State.ERROR) {
            return FSAStatus.ERROR;
        } else {
            return FSAStatus.RUN;
        }
    }

    public override sealed Token RetrieveToken() {
        int idx;
        if ((idx = StringTable.entrys.FindIndex(x => x == raw)) == -1) {
            StringTable.entrys.Add(raw);
            idx = StringTable.entrys.Count - 1;
        }
        return new TokenString(val, idx, raw);
    }

    public override sealed void ReadChar(Char ch) {
        switch (state) {
        case State.END:
        case State.ERROR:
            state = State.ERROR;
            break;
        case State.START:
            switch (ch) {
            case 'L':
                state = State.L;
                break;
            case '\"':
                state = State.Q;
                fsachar.Reset();
                break;
            default:
                state = State.ERROR;
                break;
            }
            break;
        case State.L:
            if (ch == '\"') {
                state = State.Q;
                fsachar.Reset();
            } else {
                state = State.ERROR;
            }
            break;
        case State.Q:
            if (fsachar.GetStatus() == FSAStatus.NONE && ch == '\"') {
                state = State.QQ;
                fsachar.Reset();
            } else {
                fsachar.ReadChar(ch);
                switch (fsachar.GetStatus()) {
                case FSAStatus.END:
                    state = State.Q;
                    val = val + fsachar.RetrieveChar();
                    raw = raw + fsachar.RetrieveRaw();
                    fsachar.Reset();
                    ReadChar(ch);
                    break;
                case FSAStatus.ERROR:
                    state = State.ERROR;
                    break;
                default:
                    break;
                }
            }
            break;
        case State.QQ:
            state = State.END;
            break;
        default:
            state = State.ERROR;
            break;
        }
    }

    public override sealed void ReadEOF() {
        if (state == State.QQ) {
            state = State.END;
        } else {
            state = State.ERROR;
        }
    }

}
