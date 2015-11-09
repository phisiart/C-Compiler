using System;
using System.Collections.Generic;

// keyword
// -------
public enum KeywordVal {
    AUTO,
    DOUBLE,
    INT,
    STRUCT,
    BREAK,
    ELSE,
    LONG,
    SWITCH,
    CASE,
    ENUM,
    REGISTER,
    TYPEDEF,
    CHAR,
    EXTERN,
    RETURN,
    UNION,
    CONST,
    FLOAT,
    SHORT,
    UNSIGNED,
    CONTINUE,
    FOR,
    SIGNED,
    VOID,
    DEFAULT,
    GOTO,
    SIZEOF,
    VOLATILE,
    DO,
    IF,
    STATIC,
    WHILE
}

public class TokenKeyword : Token {
    public TokenKeyword(KeywordVal val) {
        this.Val = val;
    }

    public override TokenKind Kind { get; } = TokenKind.KEYWORD;
    public KeywordVal Val { get; }
    public static Dictionary<String, KeywordVal> Keywords { get; } = new Dictionary<String, KeywordVal>(StringComparer.InvariantCultureIgnoreCase) {
        { "AUTO",        KeywordVal.AUTO      },
        { "DOUBLE",      KeywordVal.DOUBLE    },
        { "INT",         KeywordVal.INT       },
        { "STRUCT",      KeywordVal.STRUCT    },
        { "BREAK",       KeywordVal.BREAK     },
        { "ELSE",        KeywordVal.ELSE      },
        { "LONG",        KeywordVal.LONG      },
        { "SWITCH",      KeywordVal.SWITCH    },
        { "CASE",        KeywordVal.CASE      },
        { "ENUM",        KeywordVal.ENUM      },
        { "REGISTER",    KeywordVal.REGISTER  },
        { "TYPEDEF",     KeywordVal.TYPEDEF   },
        { "CHAR",        KeywordVal.CHAR      },
        { "EXTERN",      KeywordVal.EXTERN    },
        { "RETURN",      KeywordVal.RETURN    },
        { "UNION",       KeywordVal.UNION     },
        { "CONST",       KeywordVal.CONST     },
        { "FLOAT",       KeywordVal.FLOAT     },
        { "SHORT",       KeywordVal.SHORT     },
        { "UNSIGNED",    KeywordVal.UNSIGNED  },
        { "CONTINUE",    KeywordVal.CONTINUE  },
        { "FOR",         KeywordVal.FOR       },
        { "SIGNED",      KeywordVal.SIGNED    },
        { "VOID",        KeywordVal.VOID      },
        { "DEFAULT",     KeywordVal.DEFAULT   },
        { "GOTO",        KeywordVal.GOTO      },
        { "SIZEOF",      KeywordVal.SIZEOF    },
        { "VOLATILE",    KeywordVal.VOLATILE  },
        { "DO",          KeywordVal.DO        },
        { "IF",          KeywordVal.IF        },
        { "STATIC",      KeywordVal.STATIC    },
        { "WHILE",       KeywordVal.WHILE     }
    };

    public override String ToString() {
        return this.Kind + ": " + this.Val;
    }

}
