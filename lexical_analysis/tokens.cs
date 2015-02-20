using System;
using System.Collections.Generic;
using System.IO;

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
            return FSAStatus.RUN;
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
            return FSAStatus.RUN;
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

public class Scanner {
    public Scanner() {
        fsas = new List<FSA>() {
            new FSAFloat(),
            new FSAInt(),
            new FSAOperator(),
            new FSAIdentifier(),
            new FSASpace(),
            new FSANewLine(),
            new FSACharConst(),
            new FSAString(),
        };
        //fsas.Add(new FSAFloat());
        //fsas.Add(new FSAInt());
        //fsas.Add(new FSAOperator());
        //fsas.Add(new FSAIdentifier());
        //fsas.Add(new FSASpace());
        //fsas.Add(new FSANewLine());
        //fsas.Add(new FSACharConst());
        //fsas.Add(new FSAString());
    }

    public void OpenFile(String file_name) {
        if (File.Exists(file_name)) {
            src = File.ReadAllText(file_name);
        } else {
            Console.WriteLine("{0} does not exist!", file_name);
        }
    }

    public void Lex() {
        int pos = 0;
        tokens = new List<Token>();
        for (int i = 0; i < src.Length; ++i) {

            fsas.ForEach(fsa => fsa.ReadChar(src[i]));

            // if no running
            if (fsas.FindIndex(fsa => fsa.GetStatus() == FSAStatus.RUN) == -1) {
                int idx = fsas.FindIndex(fsa => fsa.GetStatus() == FSAStatus.END);
                if (idx != -1) {
                    // Console.WriteLine("> " + src.Substring(pos, i - pos));
                    Token token = fsas[idx].RetrieveToken();
                    if (token.type != TokenType.NONE) {
                        tokens.Add(token);
                    }
                    //Console.WriteLine(fsas[idx].RetrieveToken());
                    pos = i;
                    i--;
                    fsas.ForEach(fsa => fsa.Reset());
                } else {
                    Console.WriteLine("error");
                }
            }
        }

        fsas.ForEach(fsa => fsa.ReadEOF());
        // find END
        int idx2 = fsas.FindIndex(fsa => fsa.GetStatus() == FSAStatus.END);
        if (idx2 != -1) {
            Token token = fsas[idx2].RetrieveToken();
            if (token.type != TokenType.NONE) {
                tokens.Add(token);
            }
            //Console.WriteLine("> " + src.Substring(pos, src.Length - pos));
        } else {
            Console.WriteLine("error");
        }

        tokens.Add(new EmptyToken());
    }

    public override String ToString() {
        String str = "";
        tokens.ForEach(token => str += token.ToString() + "\n");
        return str;
    }

    public String src;
    private List<FSA> fsas;
    public List<Token> tokens;

}
