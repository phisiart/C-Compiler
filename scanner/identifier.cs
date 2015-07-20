using System;

// TokenIdentifier
// ===============
// If the identifier is found to be a keyword, then it will be a keyword
// 
public class TokenIdentifier : Token {
    public TokenIdentifier(string _val)
        : base(TokenType.IDENTIFIER) {
        val = _val;
    }
    public readonly string val;
    public override string ToString() {
        return type.ToString() + ": " + val;
    }
}

public class FSAIdentifier : FSA {
    private enum State {
        START,
        END,
        ERROR,
        ID
    };
    private State state;
    private string scanned;

    public FSAIdentifier() {
        state = State.START;
        scanned = "";
    }

    public override sealed void Reset() {
        state = State.START;
        scanned = "";
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
        string name = scanned.Substring(0, scanned.Length - 1);
        if (TokenKeyword.keywords.ContainsKey(name)) {
            return new TokenKeyword(TokenKeyword.keywords[name]);
        } else {
            return new TokenIdentifier(name);
        }
    }

    public override sealed void ReadChar(Char ch) {
        scanned = scanned + ch;
        switch (state) {
        case State.END:
        case State.ERROR:
            state = State.ERROR;
            break;
        case State.START:
            if (ch == '_' || Char.IsLetter(ch)) {
                state = State.ID;
            } else {
                state = State.ERROR;
            }
            break;
        case State.ID:
            if (Char.IsLetterOrDigit(ch) || ch == '_') {
                state = State.ID;
            } else {
                state = State.END;
            }
            break;
        }
    }

    public override sealed void ReadEOF() {
        scanned = scanned + '0';
        switch (state) {
        case State.ID:
            state = State.END;
            break;
        default:
            state = State.ERROR;
            break;
        }
    }
}
