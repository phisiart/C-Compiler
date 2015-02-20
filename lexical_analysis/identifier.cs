using System;

// identifier
// ----------
// if the identifier is found to be a keyword, then it will be a keyword
public class TokenIdentifier : Token {
    public TokenIdentifier(String _val)
        : base(TokenType.IDENTIFIER) {
        val = _val;
    }
    public String val;
    public override string ToString() {
        return type.ToString() + ": " + val;
    }
}

public class FSAIdentifier : FSA {
    public enum IdState {
        START,
        END,
        ERROR,
        ID
    };
    public IdState state;
    public FSAIdentifier() {
        state = IdState.START;
        str = "";
    }
    public void Reset() {
        state = IdState.START;
        str = "";
    }
    public FSAStatus GetStatus() {
        if (state == IdState.START) {
            return FSAStatus.NONE;
        } else if (state == IdState.END) {
            return FSAStatus.END;
        } else if (state == IdState.ERROR) {
            return FSAStatus.ERROR;
        } else {
            return FSAStatus.RUN;
        }
    }

    string str;
    private bool IsNonDigit(char ch) {
        return (ch == '_' || char.IsLetter(ch));
    }

    public Token RetrieveToken() {
        String name = str.Substring(0, str.Length - 1);
        if (TokenKeyword.keywords.ContainsKey(name)) {
            return new TokenKeyword(TokenKeyword.keywords[name]);
        } else {
            return new TokenIdentifier(name);
        }
    }

    public void ReadChar(char ch) {
        str = str + ch;
        switch (state) {
        case IdState.END:
        case IdState.ERROR:
            state = IdState.ERROR;
            break;
        case IdState.START:
            if (IsNonDigit(ch)) {
                state = IdState.ID;
            } else {
                state = IdState.ERROR;
            }
            break;
        case IdState.ID:
            if (IsNonDigit(ch) || char.IsDigit(ch)) {
                state = IdState.ID;
            } else {
                state = IdState.END;
            }
            break;
        }
    }

    public void ReadEOF() {
        str = str + '0';
        switch (state) {
        case IdState.ID:
            state = IdState.END;
            break;
        default:
            state = IdState.ERROR;
            break;
        }
    }
}
