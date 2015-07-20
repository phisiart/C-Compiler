using System;
using System.Collections.Generic;
using System.Linq;

// operator
// --------
// note that '...' is recognized as three '.'s
public enum OperatorVal {
    LBRACKET,
    RBRACKET,
    LPAREN,
    RPAREN,
    PERIOD,
    COMMA,
    QUESTION,
    COLON,
    TILDE,
    SUB,
    RARROW,
    DEC,
    SUBASSIGN,
    ADD,
    INC,
    ADDASSIGN,
    BITAND,
    AND,
    ANDASSIGN,
    MULT,
    MULTASSIGN,
    LT,
    LEQ,
    LSHIFT,
    LSHIFTASSIGN,
    GT,
    GEQ,
    RSHIFT,
    RSHIFTASSIGN,
    ASSIGN,
    EQ,
    BITOR,
    OR,
    ORASSIGN,
    NOT,
    NEQ,
    DIV,
    DIVASSIGN,
    MOD,
    MODASSIGN,
    XOR,
    XORASSIGN,
    SEMICOLON,
    LCURL,
    RCURL
}

public class TokenOperator : Token {
    public TokenOperator(OperatorVal _val)
        : base(TokenType.OPERATOR) {
        val = _val;
    }
    public readonly OperatorVal val;

    public static Dictionary<string, OperatorVal> ops = new Dictionary<string, OperatorVal>() {
        { "[",    OperatorVal.LBRACKET     },
        { "]",    OperatorVal.RBRACKET     },
        { "(",    OperatorVal.LPAREN       },
        { ")",    OperatorVal.RPAREN       },
        { ".",    OperatorVal.PERIOD       },
        { ",",    OperatorVal.COMMA        },
        { "?",    OperatorVal.QUESTION     },
        { ":",    OperatorVal.COLON        },
        { "~",    OperatorVal.TILDE        },
        { "-",    OperatorVal.SUB          },
        { "->",   OperatorVal.RARROW       },
        { "--",   OperatorVal.DEC          },
        { "-=",   OperatorVal.SUBASSIGN    },
        { "+",    OperatorVal.ADD          },
        { "++",   OperatorVal.INC          },
        { "+=",   OperatorVal.ADDASSIGN    },
        { "&",    OperatorVal.BITAND       },
        { "&&",   OperatorVal.AND          },
        { "&=",   OperatorVal.ANDASSIGN    },
        { "*",    OperatorVal.MULT         },
        { "*=",   OperatorVal.MULTASSIGN   },
        { "<",    OperatorVal.LT           },
        { "<=",   OperatorVal.LEQ          },
        { "<<",   OperatorVal.LSHIFT       },
        { "<<=",  OperatorVal.LSHIFTASSIGN },
        { ">",    OperatorVal.GT           },
        { ">=",   OperatorVal.GEQ          },
        { ">>",   OperatorVal.RSHIFT       },
        { ">>=",  OperatorVal.RSHIFTASSIGN },
        { "=",    OperatorVal.ASSIGN       },
        { "==",   OperatorVal.EQ           },
        { "|",    OperatorVal.BITOR        },
        { "||",   OperatorVal.OR           },
        { "|=",   OperatorVal.ORASSIGN     },
        { "!",    OperatorVal.NOT          },
        { "!=",   OperatorVal.NEQ          },
        { "/",    OperatorVal.DIV          },
        { "/=",   OperatorVal.DIVASSIGN    },
        { "%",    OperatorVal.MOD          },
        { "%=",   OperatorVal.MODASSIGN    },
        { "^",    OperatorVal.XOR          },
        { "^=",   OperatorVal.XORASSIGN    },
        { ";",    OperatorVal.SEMICOLON    },
        { "{",    OperatorVal.LCURL        },
        { "}",    OperatorVal.RCURL        }
    };

    public override string ToString() {
        return type.ToString() + " [" + val.ToString() + "]: " + ops.First(pair => pair.Value == val).Key;
    }
}

public class FSAOperator : FSA {
    private enum State {
        START,
        END,
        ERROR,
        FINISH,
        SUB,
        ADD,
        AMP,
        MULT,
        LT,
        LTLT,
        GT,
        GTGT,
        EQ,
        OR,
        NOT,
        DIV,
        MOD,
        XOR
    };

    public static List<Char> opchars = new List<Char>() {
        '[',
        ']',
        '(',
        ')',
        '.',
        ',',
        '?',
        ':',
        '-',
        '>',
        '+',
        '&',
        '*',
        '~',
        '!',
        '/',
        '%',
        '<',
        '=',
        '^',
        '|',
        ';',
        '{',
        '}'
    };

    private State state;
    private string scanned;

    public FSAOperator() {
        state = State.START;
        scanned = "";
    }

    public override sealed void Reset() {
        state = State.START;
        scanned = "";
    }

    public override sealed FSAStatus GetStatus() {
        switch (state) {
        case State.START:
            return FSAStatus.NONE;
        case State.END:
            return FSAStatus.END;
        case State.ERROR:
            return FSAStatus.ERROR;
        default:
            return FSAStatus.RUN;
        }
    }

    public override sealed Token RetrieveToken() {
        return new TokenOperator(TokenOperator.ops[scanned.Substring(0, scanned.Length - 1)]);
    }

    public override sealed void ReadChar(Char ch) {
        scanned = scanned + ch;
        switch (state) {
        case State.END:
        case State.ERROR:
            state = State.ERROR;
            break;
        case State.START:
            if (opchars.Exists(x => x == ch)) {
                switch (ch) {
                case '-':
                    state = State.SUB;
                    break;
                case '+':
                    state = State.ADD;
                    break;
                case '&':
                    state = State.AMP;
                    break;
                case '*':
                    state = State.MULT;
                    break;
                case '<':
                    state = State.LT;
                    break;
                case '>':
                    state = State.GT;
                    break;
                case '=':
                    state = State.EQ;
                    break;
                case '|':
                    state = State.OR;
                    break;
                case '!':
                    state = State.NOT;
                    break;
                case '/':
                    state = State.DIV;
                    break;
                case '%':
                    state = State.MOD;
                    break;
                case '^':
                    state = State.XOR;
                    break;
                default:
                    state = State.FINISH;
                    break;
                }
            } else {
                state = State.ERROR;
            }
            break;
        case State.FINISH:
            state = State.END;
            break;
        case State.SUB:
            switch (ch) {
            case '>':
            case '-':
            case '=':
                state = State.FINISH;
                break;
            default:
                state = State.END;
                break;
            }
            break;
        case State.ADD:
            switch (ch) {
            case '+':
            case '=':
                state = State.FINISH;
                break;
            default:
                state = State.END;
                break;
            }
            break;
        case State.AMP:
            switch (ch) {
            case '&':
            case '=':
                state = State.FINISH;
                break;
            default:
                state = State.END;
                break;
            }
            break;
        case State.MULT:
            if (ch == '=') {
                state = State.FINISH;
            } else {
                state = State.END;
            }
            break;
        case State.LT:
            switch (ch) {
            case '=':
                state = State.FINISH;
                break;
            case '<':
                state = State.LTLT;
                break;
            default:
                state = State.END;
                break;
            }
            break;
        case State.GT:
            switch (ch) {
            case '=':
                state = State.FINISH;
                break;
            case '>':
                state = State.GTGT;
                break;
            default:
                state = State.END;
                break;
            }
            break;
        case State.EQ:
            if (ch == '=') {
                state = State.FINISH;
            } else {
                state = State.END;
            }
            break;
        case State.OR:
            switch (ch) {
            case '|':
            case '=':
                state = State.FINISH;
                break;
            default:
                state = State.END;
                break;
            }
            break;
        case State.NOT:
            if (ch == '=') {
                state = State.FINISH;
            } else {
                state = State.END;
            }
            break;
        case State.DIV:
            if (ch == '=') {
                state = State.FINISH;
            } else {
                state = State.END;
            }
            break;
        case State.MOD:
            if (ch == '=') {
                state = State.FINISH;
            } else {
                state = State.END;
            }
            break;
        case State.XOR:
            if (ch == '=') {
                state = State.FINISH;
            } else {
                state = State.END;
            }
            break;
        case State.LTLT:
            if (ch == '=') {
                state = State.FINISH;
            } else {
                state = State.END;
            }
            break;
        case State.GTGT:
            if (ch == '=') {
                state = State.FINISH;
            } else {
                state = State.END;
            }
            break;
        default:
            state = State.ERROR;
            break;
        }
    }

    public override sealed void ReadEOF() {
        scanned = scanned + '0';
        switch (state) {
        case State.FINISH:
        case State.SUB:
        case State.ADD:
        case State.AMP:
        case State.MULT:
        case State.LT:
        case State.LTLT:
        case State.GT:
        case State.GTGT:
        case State.EQ:
        case State.OR:
        case State.NOT:
        case State.DIV:
        case State.MOD:
        case State.XOR:
            state = State.END;
            break;
        default:
            state = State.ERROR;
            break;
        }
    }

}
