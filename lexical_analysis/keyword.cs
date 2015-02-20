using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
    public TokenKeyword(KeywordVal _val)
        : base(TokenType.KEYWORD) {
        val = _val;
    }
    public readonly KeywordVal val;
    public static Dictionary<string, KeywordVal> keywords = new Dictionary<string, KeywordVal>(StringComparer.InvariantCultureIgnoreCase) {
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

    public override string ToString() {
        return type.ToString() + ": " + val.ToString();
    }

}
