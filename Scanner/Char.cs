using System;

// TokenCharConst
// ==============
// A character constant
// 
public sealed class TokenCharConst : Token {
    public TokenCharConst(String raw, Char value) {
        this.Raw = raw;
        this.Value = value;
    }

    public override TokenKind Kind { get; } = TokenKind.CHAR;
    public String Raw { get; }
    public Char Value { get; }
    public override String ToString() => $"{this.Kind}: '{this.Raw}'";
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
public sealed class FSAChar : FSA {
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

    private State _state;
    private String _scanned;

    // quote : Char
    // ============
    // \' in a Char, and \" in a String.
    private readonly Char _quote;

    public FSAChar(Char quote) {
        this._state = State.START;
        this._quote = quote;
        this._scanned = "";
    }

    public override void Reset() {
        this._scanned = "";
        this._state = State.START;
    }

    public override FSAStatus GetStatus() {
        if (this._state == State.START) {
            return FSAStatus.NONE;
        }
        if (this._state == State.END) {
            return FSAStatus.END;
        }
        if (this._state == State.ERROR) {
            return FSAStatus.ERROR;
        }
        return FSAStatus.RUNNING;
    }

    // IsChar : Char -> Boolean
    // ========================
    // the character is a 'normal' Char, other than <quote> \\ or \n
    // 
    private Boolean IsChar(Char ch) {
        return ch != this._quote && ch != '\\' && ch != '\n';
    }



    // RetrieveRaw : () -> String
    // ==========================
    // 
    public String RetrieveRaw() {
        return this._scanned.Substring(0, this._scanned.Length - 1);
    }

    // RetrieveChar : () -> Char
    // =========================
    // 
    public Char RetrieveChar() {
        if (this._scanned.Length == 3) {
            switch (this._scanned[1]) {
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
                    return this._scanned[1];
            }
        }
        return this._scanned[0];
    }

    // RetrieveToken : () -> Token
    // ===========================
    // Note that this function never gets used, because FSAChar is just an inner FSA for other FSAs.
    // 
    public override Token RetrieveToken() {
        return new EmptyToken();
    }

    // ReadChar : Char -> ()
    // =====================
    // Implementation of the FSA
    // 
    public override void ReadChar(Char ch) {
        this._scanned = this._scanned + ch;
        switch (this._state) {
            case State.END:
            case State.ERROR:
                this._state = State.ERROR;
                break;
            case State.START:
                if (IsChar(ch)) {
                    this._state = State.C;
                } else if (ch == '\\') {
                    this._state = State.S;
                } else {
                    this._state = State.ERROR;
                }
                break;
            case State.C:
                this._state = State.END;
                break;
            case State.S:
                if (Utils.IsEscapeChar(ch)) {
                    this._state = State.C;
                } else if (Utils.IsOctDigit(ch)) {
                    this._state = State.SO;
                } else if (ch == 'x' || ch == 'X') {
                    this._state = State.SX;
                } else {
                    this._state = State.ERROR;
                }
                break;
            case State.SX:
                if (Utils.IsHexDigit(ch)) {
                    this._state = State.SXH;
                } else {
                    this._state = State.ERROR;
                }
                break;
            case State.SXH:
                if (Utils.IsHexDigit(ch)) {
                    this._state = State.SXHH;
                } else {
                    this._state = State.END;
                }
                break;
            case State.SXHH:
                this._state = State.END;
                break;
            case State.SO:
                if (Utils.IsOctDigit(ch)) {
                    this._state = State.SOO;
                } else {
                    this._state = State.END;
                }
                break;
            case State.SOO:
                if (Utils.IsOctDigit(ch)) {
                    this._state = State.SOOO;
                } else {
                    this._state = State.END;
                }
                break;
            case State.SOOO:
                this._state = State.END;
                break;
            default:
                this._state = State.ERROR;
                break;
        }
    }

    // ReadEOF : () -> ()
    // ==================
    // 
    public override void ReadEOF() {
        this._scanned = this._scanned + '0';
        switch (this._state) {
            case State.C:
            case State.SO:
            case State.SOO:
            case State.SOOO:
            case State.SXH:
            case State.SXHH:
                this._state = State.END;
                break;
            default:
                this._state = State.ERROR;
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
public sealed class FSACharConst : FSA {
    private enum State {
        START,
        END,
        ERROR,
        L,
        Q,
        QC,
        QCQ
    };

    private State _state;
    private Char _val;
    private String _raw;
    private readonly FSAChar _fsachar;

    public FSACharConst() {
        this._state = State.START;
        this._fsachar = new FSAChar('\'');
    }

    public override void Reset() {
        this._state = State.START;
        this._fsachar.Reset();
    }

    public override FSAStatus GetStatus() {
        if (this._state == State.START) {
            return FSAStatus.NONE;
        }
        if (this._state == State.END) {
            return FSAStatus.END;
        }
        if (this._state == State.ERROR) {
            return FSAStatus.ERROR;
        }
        return FSAStatus.RUNNING;
    }

    public override Token RetrieveToken() {
        return new TokenCharConst(this._raw, this._val);
    }

    public override void ReadChar(Char ch) {
        switch (this._state) {
            case State.END:
            case State.ERROR:
                this._state = State.ERROR;
                break;
            case State.START:
                switch (ch) {
                    case 'L':
                        this._state = State.L;
                        break;
                    case '\'':
                        this._state = State.Q;
                        this._fsachar.Reset();
                        break;
                    default:
                        this._state = State.ERROR;
                        break;
                }
                break;
            case State.L:
                if (ch == '\'') {
                    this._state = State.Q;
                    this._fsachar.Reset();
                } else {
                    this._state = State.ERROR;
                }
                break;
            case State.Q:
                this._fsachar.ReadChar(ch);
                switch (this._fsachar.GetStatus()) {
                    case FSAStatus.END:
                        this._state = State.QC;
                        this._raw = this._fsachar.RetrieveRaw();
                        this._val = this._fsachar.RetrieveChar();
                        this._fsachar.Reset();
                        ReadChar(ch);
                        break;
                    case FSAStatus.ERROR:
                        this._state = State.ERROR;
                        break;
                    default:
                        break;
                }
                break;
            case State.QC:
                if (ch == '\'') {
                    this._state = State.QCQ;
                } else {
                    this._state = State.ERROR;
                }
                break;
            case State.QCQ:
                this._state = State.END;
                break;
            default:
                this._state = State.ERROR;
                break;
        }
    }

    public override void ReadEOF() {
        if (this._state == State.QCQ) {
            this._state = State.END;
        } else {
            this._state = State.ERROR;
        }
    }

}
