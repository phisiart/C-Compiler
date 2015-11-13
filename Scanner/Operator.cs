using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LexicalAnalysis {
    /// <summary>
    /// Note that '...' is recognized as three '.'s
    /// </summary>
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

    public sealed class TokenOperator : Token {
        public TokenOperator(OperatorVal val) {
            this.Val = val;
        }

        public override TokenKind Kind { get; } = TokenKind.OPERATOR;
        public OperatorVal Val { get; }

        public static Dictionary<String, OperatorVal> Operators { get; } = new Dictionary<String, OperatorVal> {
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

        public override String ToString() {
            return this.Kind + " [" + this.Val + "]: " + Operators.First(pair => pair.Value == this.Val).Key;
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

        public static ImmutableHashSet<Char> OperatorChars { get; } = ImmutableHashSet.Create(
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
            );

        private State _state;
        private String _scanned;

        public FSAOperator() {
            this._state = State.START;
            this._scanned = "";
        }

        public override sealed void Reset() {
            this._state = State.START;
            this._scanned = "";
        }

        public override sealed FSAStatus GetStatus() {
            switch (this._state) {
                case State.START:
                    return FSAStatus.NONE;
                case State.END:
                    return FSAStatus.END;
                case State.ERROR:
                    return FSAStatus.ERROR;
                default:
                    return FSAStatus.RUNNING;
            }
        }

        public override sealed Token RetrieveToken() {
            return new TokenOperator(TokenOperator.Operators[this._scanned.Substring(0, this._scanned.Length - 1)]);
        }

        public override sealed void ReadChar(Char ch) {
            this._scanned = this._scanned + ch;
            switch (this._state) {
                case State.END:
                case State.ERROR:
                    this._state = State.ERROR;
                    break;
                case State.START:
                    if (OperatorChars.Contains(ch)) {
                        switch (ch) {
                            case '-':
                                this._state = State.SUB;
                                break;
                            case '+':
                                this._state = State.ADD;
                                break;
                            case '&':
                                this._state = State.AMP;
                                break;
                            case '*':
                                this._state = State.MULT;
                                break;
                            case '<':
                                this._state = State.LT;
                                break;
                            case '>':
                                this._state = State.GT;
                                break;
                            case '=':
                                this._state = State.EQ;
                                break;
                            case '|':
                                this._state = State.OR;
                                break;
                            case '!':
                                this._state = State.NOT;
                                break;
                            case '/':
                                this._state = State.DIV;
                                break;
                            case '%':
                                this._state = State.MOD;
                                break;
                            case '^':
                                this._state = State.XOR;
                                break;
                            default:
                                this._state = State.FINISH;
                                break;
                        }
                    } else {
                        this._state = State.ERROR;
                    }
                    break;
                case State.FINISH:
                    this._state = State.END;
                    break;
                case State.SUB:
                    switch (ch) {
                        case '>':
                        case '-':
                        case '=':
                            this._state = State.FINISH;
                            break;
                        default:
                            this._state = State.END;
                            break;
                    }
                    break;
                case State.ADD:
                    switch (ch) {
                        case '+':
                        case '=':
                            this._state = State.FINISH;
                            break;
                        default:
                            this._state = State.END;
                            break;
                    }
                    break;
                case State.AMP:
                    switch (ch) {
                        case '&':
                        case '=':
                            this._state = State.FINISH;
                            break;
                        default:
                            this._state = State.END;
                            break;
                    }
                    break;
                case State.MULT:
                    if (ch == '=') {
                        this._state = State.FINISH;
                    } else {
                        this._state = State.END;
                    }
                    break;
                case State.LT:
                    switch (ch) {
                        case '=':
                            this._state = State.FINISH;
                            break;
                        case '<':
                            this._state = State.LTLT;
                            break;
                        default:
                            this._state = State.END;
                            break;
                    }
                    break;
                case State.GT:
                    switch (ch) {
                        case '=':
                            this._state = State.FINISH;
                            break;
                        case '>':
                            this._state = State.GTGT;
                            break;
                        default:
                            this._state = State.END;
                            break;
                    }
                    break;
                case State.EQ:
                    if (ch == '=') {
                        this._state = State.FINISH;
                    } else {
                        this._state = State.END;
                    }
                    break;
                case State.OR:
                    switch (ch) {
                        case '|':
                        case '=':
                            this._state = State.FINISH;
                            break;
                        default:
                            this._state = State.END;
                            break;
                    }
                    break;
                case State.NOT:
                    if (ch == '=') {
                        this._state = State.FINISH;
                    } else {
                        this._state = State.END;
                    }
                    break;
                case State.DIV:
                    if (ch == '=') {
                        this._state = State.FINISH;
                    } else {
                        this._state = State.END;
                    }
                    break;
                case State.MOD:
                    if (ch == '=') {
                        this._state = State.FINISH;
                    } else {
                        this._state = State.END;
                    }
                    break;
                case State.XOR:
                    if (ch == '=') {
                        this._state = State.FINISH;
                    } else {
                        this._state = State.END;
                    }
                    break;
                case State.LTLT:
                    if (ch == '=') {
                        this._state = State.FINISH;
                    } else {
                        this._state = State.END;
                    }
                    break;
                case State.GTGT:
                    if (ch == '=') {
                        this._state = State.FINISH;
                    } else {
                        this._state = State.END;
                    }
                    break;
                default:
                    this._state = State.ERROR;
                    break;
            }
        }

        public override sealed void ReadEOF() {
            this._scanned = this._scanned + '0';
            switch (this._state) {
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
                    this._state = State.END;
                    break;
                default:
                    this._state = State.ERROR;
                    break;
            }
        }

    }
}