using System;

// TokenChar
// =========
// A character constant
// 
public class TokenChar : Token {
    public TokenChar(String _raw, Char _val)
        : base(TokenType.CHAR) {
        raw = _raw;
        val = _val;
    }
    public readonly String raw;
    public readonly Char val;
    public override String ToString() {
        return type.ToString() + ": " + "\'" + raw + "\'" + "\n\'" + val + "\'";
    }
}

// FSAChar
// =======
// The FSA for scanning a C character.
// Note that this FSA doesn't scan the surrounding quotes.
// It is used in both FSACharConst and FSAString.
// 
// There are multiple ways to represent a character:
// * A normal character : any character other than \\ \n or <quote>
//     Note that <quote> might be \' or \" depending on the context.
//     For example, inside a string, single quote are allowed, which means that the following code is legal:
//       char *str = "single quote here >>> ' <<< see that?";

//     However, if we need a double quote inside a string, we have to use an escape character, like this:
//       char *str = "double quote needs to be escaped >>> \" <<<";
//
//     Inside a char, double quotes are allowed while single quotes need to be escaped.
//       char double_quote = '"';  // allowed
//       char single_quote = '\''; // needs to be escaped
// 
// * An escape character : \a \b \f \n \r \t \v \' \" \\ \?
//     Note that even though \' and \" might not needs to be escaped, you can always use them as escaped.
//     If you escape a character not listed above, the behavior is undefined in the standard.
//     I'll just assume you need the unescaped character.
//     For example, if you typed '\c', then I'll just treat it as 'c'.
// 
// * An octal number after a backslash. For example : \123.
// 
// * A hexadecimal number after a backslash and an 'x' or 'X'. FOr example : \xFF.
// 
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
    }

    private CharState state;
    private String scanned;
    
    // quote : Char
    // ============
    // \' in a char, and \" in a string.
    private Char quote;

    public FSAChar(char _quote) {
        state = CharState.START;
        quote = _quote;
        scanned = "";
    }
    public void Reset() {
        scanned = "";
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

    // IsChar : Char -> Boolean
    // ========================
    // the character is a 'normal' char, other than <quote> \\ or \n
    // 
    private Boolean IsChar(Char ch) {
        return ch != quote && ch != '\\' && ch != '\n';
    }

    // IsEscapeChar : Char -> Boolean
    // ==============================
    // 
    private Boolean IsEscapeChar(Char ch) {
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

    // IsHexDigit : Char -> Boolean
    // ============================
    // 
    private Boolean IsHexDigit(Char ch) {
        return (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F');
    }

    // IsOctDigit : Char -> Boolean
    // ============================
    // 
    private Boolean IsOctDigit(Char ch) {
        return ch >= '0' && ch <= '7';
    }

    // RetrieveRaw : () -> String
    // ==========================
    // 
    public String RetrieveRaw() {
        return scanned.Substring(0, scanned.Length - 1);
    }

    // RetrieveChar : () -> Char
    // =========================
    // 
    public Char RetrieveChar() {
        if (scanned.Length == 3) {
            switch (scanned[1]) {
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
                return scanned[1];
            }
        } else {
            return scanned[0];
        }
    }

    // RetrieveToken : () -> Token
    // ===========================
    // 
    public Token RetrieveToken() {
        return new EmptyToken();
    }

    // ReadChar : Char -> ()
    // =====================
    // Implementation of the FSA
    // 
    public void ReadChar(Char ch) {
        scanned = scanned + ch;
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
            } else if (IsOctDigit(ch)) {
                state = CharState.SO;
            } else if (ch == 'x' || ch == 'X') {
                state = CharState.SX;
            } else {
                state = CharState.ERROR;
            }
            break;
        case CharState.SX:
            if (IsHexDigit(ch)) {
                state = CharState.SXH;
            } else {
                state = CharState.ERROR;
            }
            break;
        case CharState.SXH:
            if (IsHexDigit(ch)) {
                state = CharState.SXHH;
            } else {
                state = CharState.END;
            }
            break;
        case CharState.SXHH:
            state = CharState.END;
            break;
        case CharState.SO:
            if (IsOctDigit(ch)) {
                state = CharState.SOO;
            } else {
                state = CharState.END;
            }
            break;
        case CharState.SOO:
            if (IsOctDigit(ch)) {
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

    // ReadEOF : () -> ()
    // ==================
    // 
    public void ReadEOF() {
        scanned = scanned + '0';
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

// FSACharConst
// ============
// The FSA for scanning a C character constant.
// Upon finish, we can retrive a token of character.
// 
// A character constant can either be represented by
// * '<char>'
// or
// * L'<char>'
//
// The character inside the quotes is read by FSAChar.
// Note that if the inner character is a single quote, it needs to be escaped.
// 
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

    private CharConstState state;
    private Char val;
    private String raw;

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

    public Token RetrieveToken() {
        return new TokenChar(raw, val);
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
