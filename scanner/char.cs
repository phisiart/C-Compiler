using System;

// TokenCharConst
// ==============
// A character constant
// 
public class TokenCharConst : Token {
    public TokenCharConst(String raw, Char value)
        : base(TokenType.CHAR) {
        this.raw = raw;
        this.value = value;
    }
    public readonly String raw;
    public readonly Char value;
    public override String ToString() => $"{type}: '{raw}'";
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
//     For example, inside a String, single quote are allowed, which means that the following code is legal:
//       Char *str = "single quote here >>> ' <<< see that?";

//     However, if we need a double quote inside a String, we have to use an escape character, like this:
//       Char *str = "double quote needs to be escaped >>> \" <<<";
//
//     Inside a Char, double quotes are allowed while single quotes need to be escaped.
//       Char double_quote = '"';  // allowed
//       Char single_quote = '\''; // needs to be escaped
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
    private enum State {
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

    private State state;
    private String scanned;
    
    // quote : Char
    // ============
    // \' in a Char, and \" in a String.
    private Char quote;

    public FSAChar(Char _quote) {
        state = State.START;
        quote = _quote;
        scanned = "";
    }

    public override sealed void Reset() {
        scanned = "";
        state = State.START;
    }

    public override sealed FSAStatus GetStatus() {
        if (state == State.START) {
            return FSAStatus.NONE;
        } else if (state == State.END) {
            return FSAStatus.END;
        } else if (state == State.ERROR) {
            return FSAStatus.ERROR;
        } else {
            return FSAStatus.RUNNING;
        }
    }

    // IsChar : Char -> Boolean
    // ========================
    // the character is a 'normal' Char, other than <quote> \\ or \n
    // 
    private Boolean IsChar(Char ch) {
        return ch != quote && ch != '\\' && ch != '\n';
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
    // Note that this function never gets used, because FSAChar is just an inner FSA for other FSAs.
    // 
    public override sealed Token RetrieveToken() {
        return new EmptyToken();
    }

    // ReadChar : Char -> ()
    // =====================
    // Implementation of the FSA
    // 
    public override sealed void ReadChar(Char ch) {
        scanned = scanned + ch;
        switch (state) {
        case State.END:
        case State.ERROR:
            state = State.ERROR;
            break;
        case State.START:
            if (IsChar(ch)) {
                state = State.C;
            } else if (ch == '\\') {
                state = State.S;
            } else {
                state = State.ERROR;
            }
            break;
        case State.C:
            state = State.END;
            break;
        case State.S:
            if (Utils.IsEscapeChar(ch)) {
                state = State.C;
            } else if (Utils.IsOctDigit(ch)) {
                state = State.SO;
            } else if (ch == 'x' || ch == 'X') {
                state = State.SX;
            } else {
                state = State.ERROR;
            }
            break;
        case State.SX:
            if (Utils.IsHexDigit(ch)) {
                state = State.SXH;
            } else {
                state = State.ERROR;
            }
            break;
        case State.SXH:
            if (Utils.IsHexDigit(ch)) {
                state = State.SXHH;
            } else {
                state = State.END;
            }
            break;
        case State.SXHH:
            state = State.END;
            break;
        case State.SO:
            if (Utils.IsOctDigit(ch)) {
                state = State.SOO;
            } else {
                state = State.END;
            }
            break;
        case State.SOO:
            if (Utils.IsOctDigit(ch)) {
                state = State.SOOO;
            } else {
                state = State.END;
            }
            break;
        case State.SOOO:
            state = State.END;
            break;
        default:
            state = State.ERROR;
            break;
        }
    }

    // ReadEOF : () -> ()
    // ==================
    // 
    public override sealed void ReadEOF() {
        scanned = scanned + '0';
        switch (state) {
        case State.C:
        case State.SO:
        case State.SOO:
        case State.SOOO:
        case State.SXH:
        case State.SXHH:
            state = State.END;
            break;
        default:
            state = State.ERROR;
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
// * '<Char>'
// or
// * L'<Char>'
//
// The character inside the quotes is read by FSAChar.
// Note that if the inner character is a single quote, it needs to be escaped.
// 
public class FSACharConst : FSA {
    private enum State {
        START,
        END,
        ERROR,
        L,
        Q,
        QC,
        QCQ
    };

    private State state;
    private Char val;
    private String raw;
    private FSAChar fsachar;

    public FSACharConst() {
        state = State.START;
        fsachar = new FSAChar('\'');
    }

    public override sealed void Reset() {
        state = State.START;
        fsachar.Reset();
    }

    public override sealed FSAStatus GetStatus() {
        if (state == State.START) {
            return FSAStatus.NONE;
        } else if (state == State.END) {
            return FSAStatus.END;
        } else if (state == State.ERROR) {
            return FSAStatus.ERROR;
        } else {
            return FSAStatus.RUNNING;
        }
    }

    public override sealed Token RetrieveToken() {
        return new TokenCharConst(raw, val);
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
            case '\'':
                state = State.Q;
                fsachar.Reset();
                break;
            default:
                state = State.ERROR;
                break;
            }
            break;
        case State.L:
            if (ch == '\'') {
                state = State.Q;
                fsachar.Reset();
            } else {
                state = State.ERROR;
            }
            break;
        case State.Q:
            fsachar.ReadChar(ch);
            switch (fsachar.GetStatus()) {
            case FSAStatus.END:
                state = State.QC;
                raw = fsachar.RetrieveRaw();
                val = fsachar.RetrieveChar();
                fsachar.Reset();
                ReadChar(ch);
                break;
            case FSAStatus.ERROR:
                state = State.ERROR;
                break;
            default:
                break;
            }
            break;
        case State.QC:
            if (ch == '\'') {
                state = State.QCQ;
            } else {
                state = State.ERROR;
            }
            break;
        case State.QCQ:
            state = State.END;
            break;
        default:
            state = State.ERROR;
            break;
        }
    }

    public override sealed void ReadEOF() {
        if (state == State.QCQ) {
            state = State.END;
        } else {
            state = State.ERROR;
        }
    }

}
