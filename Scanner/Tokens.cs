using System;

public enum TokenType {
    NONE,
    FLOAT,
    INT,
    CHAR,
    STRING,
    IDENTIFIER,
    KEYWORD,
    OPERATOR
}

public class Token {
    public Token(TokenType _type) {
        type = _type;
    }

    public override String ToString() {
        return type.ToString();
    }
    public readonly TokenType type;
}

public class EmptyToken : Token {
    public EmptyToken()
        : base(TokenType.NONE) {}
}



public class FSASpace : FSA {
    private enum State {
        START,
        END,
        ERROR,
        SPACE
    };

    private State state;

    public FSASpace() {
        state = State.START;
    }

    public override sealed void Reset() {
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

    public override sealed Token RetrieveToken() {
        return new EmptyToken();
    }

    public override sealed void ReadChar(Char ch) {
        switch (state) {
        case State.END:
        case State.ERROR:
            state = State.ERROR;
            break;
        case State.START:
            if (Utils.IsSpace(ch)) {
                state = State.SPACE;
            } else {
                state = State.ERROR;
            }
            break;
        case State.SPACE:
            if (Utils.IsSpace(ch)) {
                state = State.SPACE;
            } else {
                state = State.END;
            }
            break;
        }
    }

    public override sealed void ReadEOF() {
        switch (state) {
        case State.SPACE:
            state = State.END;
            break;
        default:
            state = State.ERROR;
            break;
        }
    }
}

public class FSANewLine : FSA {
    private enum NewLineState {
        START,
        END,
        ERROR,
        NEWLINE
    };

    private NewLineState state;

    public FSANewLine() {
        state = NewLineState.START;
    }

    public override sealed void Reset() {
        state = NewLineState.START;
    }

    public override sealed FSAStatus GetStatus() {
        if (state == NewLineState.START) {
            return FSAStatus.NONE;
        } else if (state == NewLineState.END) {
            return FSAStatus.END;
        } else if (state == NewLineState.ERROR) {
            return FSAStatus.ERROR;
        } else {
            return FSAStatus.RUNNING;
        }
    }

    public override sealed Token RetrieveToken() {
        return new EmptyToken();
    }

    public override sealed void ReadChar(Char ch) {
        switch (state) {
        case NewLineState.END:
        case NewLineState.ERROR:
            state = NewLineState.ERROR;
            break;
        case NewLineState.START:
            if (ch == '\n') {
                state = NewLineState.NEWLINE;
            } else {
                state = NewLineState.ERROR;
            }
            break;
        case NewLineState.NEWLINE:
            state = NewLineState.END;
            break;
        }
    }

    public override sealed void ReadEOF() {
        switch (state) {
        case NewLineState.NEWLINE:
            state = NewLineState.END;
            break;
        default:
            state = NewLineState.ERROR;
            break;
        }
    }
}
