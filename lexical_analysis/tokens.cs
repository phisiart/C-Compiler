using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public override string ToString() {
        return type.ToString();
    }
    public TokenType type;
}

public enum FSAStatus {
    NONE,
    END,
    RUN,
    ERROR
}

interface FSA {
    FSAStatus GetStatus();
    void ReadChar(char ch);
    void Reset();
    void ReadEOF();
    Token RetrieveToken();
}

public class FSASpace : FSA {
    public enum SpaceState {
        START,
        END,
        ERROR,
        SPACE
    };

    public SpaceState state;
    public FSASpace() {
        state = SpaceState.START;
    }
    public void Reset() {
        state = SpaceState.START;
    }
    public FSAStatus GetStatus() {
        if (state == SpaceState.START) {
            return FSAStatus.NONE;
        } else if (state == SpaceState.END) {
            return FSAStatus.END;
        } else if (state == SpaceState.ERROR) {
            return FSAStatus.ERROR;
        } else {
            return FSAStatus.RUN;
        }
    }

    private bool IsSpace(char ch) {
        return (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\f' || ch == '\v');
    }

    public Token RetrieveToken() {
        return new Token();
    }

    public void ReadChar(char ch) {
        switch (state) {
        case SpaceState.END:
        case SpaceState.ERROR:
            state = SpaceState.ERROR;
            break;
        case SpaceState.START:
            if (IsSpace(ch)) {
                state = SpaceState.SPACE;
            } else {
                state = SpaceState.ERROR;
            }
            break;
        case SpaceState.SPACE:
            if (IsSpace(ch)) {
                state = SpaceState.SPACE;
            } else {
                state = SpaceState.END;
            }
            break;
        }
    }

    public void ReadEOF() {
        switch (state) {
        case SpaceState.SPACE:
            state = SpaceState.END;
            break;
        default:
            state = SpaceState.ERROR;
            break;
        }
    }
}

public class FSANewLine : FSA {
    public enum NewLineState {
        START,
        END,
        ERROR,
        NEWLINE
    };

    public NewLineState state;
    public FSANewLine() {
        state = NewLineState.START;
    }
    public void Reset() {
        state = NewLineState.START;
    }
    public FSAStatus GetStatus() {
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
    public Token RetrieveToken() {
        Token token = new Token();
        token.type = TokenType.NONE;
        return token;
    }
    public void ReadChar(char ch) {
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

    public void ReadEOF() {
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

public class LexicalAnalysis {
    public LexicalAnalysis() {
        fsas = new List<FSA>();
        fsas.Add(new FSAFloat());
        fsas.Add(new FSAInt());
        fsas.Add(new FSAOperator());
        fsas.Add(new FSAIdentifier());
        fsas.Add(new FSASpace());
        fsas.Add(new FSANewLine());
        fsas.Add(new FSACharConst());
        fsas.Add(new FSAString());
    }

    public void OpenFile(string file_name) {
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
        tokens.Add(new Token());
    }

    public override string ToString() {
        string str = "";
        tokens.ForEach(token => str += token.ToString() + "\n");
        return str;
    }

    public string src;
    private List<FSA> fsas;
    public List<Token> tokens;

}
