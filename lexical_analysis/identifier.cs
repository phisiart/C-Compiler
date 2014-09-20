using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

// identifier
// ----------
// if the identifier is found to be a keyword, then it will be a keyword
public class TokenIdentifier : Token {
    public TokenIdentifier() {
        type = TokenType.IDENTIFIER;
    }
    public string val;
    public override string ToString() {
        return type.ToString() + ": " + val;
    }
}

class FSAIdentifier : FSA {
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
        if (TokenKeyword.keywords.ContainsKey(str.Substring(0, str.Length - 1))) {
            TokenKeyword token = new TokenKeyword();
            token.type = TokenType.KEYWORD;
            token.val = TokenKeyword.keywords[str.Substring(0, str.Length - 1)];
            return token;
        } else {
            TokenIdentifier token = new TokenIdentifier();
            token.type = TokenType.IDENTIFIER;
            token.val = str.Substring(0, str.Length - 1);
            return token;
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
