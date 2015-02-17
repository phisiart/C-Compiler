using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Parser {
    public static bool IsSizeof(Token token) {
        if (token.type == TokenType.KEYWORD) {
            if (((TokenKeyword)token).val == KeywordVal.SIZEOF) {
                return true;
            }
        }
        return false;
    }

    public static bool IsLPAREN(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.LPAREN) {
                return true;
            }
        }
        return false;
    }

    public static bool IsRPAREN(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.RPAREN) {
                return true;
            }
        }
        return false;
    }

    public static bool IsCOLON(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.COLON) {
                return true;
            }
        }
        return false;
    }

    public static bool IsQuestionMark(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.QUESTION) {
                return true;
            }
        }
        return false;
    }

    public static bool IsAssignment(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.ASSIGN) {
                return true;
            }
        }
        return false;
    }

    public static bool IsCOMMA(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.COMMA) {
                return true;
            }
        }
        return false;
    }

    public static bool IsLCURL(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.LCURL) {
                return true;
            }
        }
        return false;
    }

    public static bool IsRCURL(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.RCURL) {
                return true;
            }
        }
        return false;
    }

    public static bool IsLBRACKET(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.LBRACKET) {
                return true;
            }
        }
        return false;
    }

    public static bool IsRBRACKET(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.RBRACKET) {
                return true;
            }
        }
        return false;
    }

    public static bool IsPERIOD(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.PERIOD) {
                return true;
            }
        }
        return false;
    }

    public static bool IsEllipsis(List<Token> src, int begin) {
        if (Parser.IsPERIOD(src[begin])) {
            begin++;
            if (Parser.IsPERIOD(src[begin])) {
                begin++;
                if (Parser.IsPERIOD(src[begin])) {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsSEMICOLON(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.SEMICOLON) {
                return true;
            }
        }
        return false;
    }

    public static bool IsKeyword(Token token, KeywordVal val) {
        if (token.type == TokenType.KEYWORD) {
            if (((TokenKeyword)token).val == val) {
                return true;
            }
        }
        return false;
    }

    public static bool IsOperator(Token token, OperatorVal val) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == val) {
                return true;
            }
        }
        return false;
    }

    public static String GetIdentifierValue(Token token) {
        if (token.type == TokenType.IDENTIFIER) {
            return ((TokenIdentifier)token).val;
        } else {
            return null;
        }
    }

    public static List<Token> GetTokensFromString(String src) {
        LexicalAnalysis lex = new LexicalAnalysis();
        lex.src = src;
        lex.Lex();
        return lex.tokens;
    }

    // tries to eat an operator
    // if succeed, current++, return true
    // if fail, current remains the same, return false
    public static bool EatOperator(List<Token> src, ref int current, OperatorVal val) {
        if (src[current].type != TokenType.OPERATOR) {
            return false;
        }

        if (((TokenOperator)src[current]).val != val) {
            return false;
        }

        current++;
        return true;
    }

    public delegate int FParse<TRet>(List<Token> src, int begin, out TRet node) where TRet : PTNode;

    public static int ParseList<TRet>(List<Token> src, int begin, out List<TRet> list, FParse<TRet> Parse) where TRet : PTNode {
        int current = begin;

        list = new List<TRet>();
        TRet item;

        while (true) {
            int saved = current;
            if ((current = Parse(src, current, out item)) == -1) {
                return saved;
            }
            list.Add(item);
        }

    }

    public static int ParseNonEmptyList<TRet>(List<Token> src, int begin, out List<TRet> list, FParse<TRet> Parse) where TRet : PTNode {
        begin = ParseList<TRet>(src, begin, out list, Parse);
        if (list.Any()) {
            return begin;
        } else {
            return -1;
        }
    }
    
    public static int Parse2Choices<TRet, T1, T2>(List<Token> src, int begin, out TRet node, FParse<T1> Parse1, FParse<T2> Parse2)
        where T1 : TRet
        where T2 : TRet
        where TRet : PTNode {
        int ret;

        T1 node1;
        if ((ret = Parse1(src, begin, out node1)) != -1) {
            node = node1;
            return ret;
        }

        T2 node2;
        if ((ret = Parse2(src, begin, out node2)) != -1) {
            node = node2;
            return ret;
        }
        
        node = null;
        return -1;
    }

    public static int ParseNonEmptyListWithSep<TRet>(List<Token> src, int pos, out List<TRet> list, FParse<TRet> Parse, OperatorVal op) where TRet : PTNode {
        list = new List<TRet>();
        TRet item;

        if ((pos = Parse(src, pos, out item)) == -1)
            return -1;
        list.Add(item);

        while (true) {
            int saved = pos;
            if (!Parser.EatOperator(src, ref pos, op))
                return saved;
            if ((pos = Parse(src, pos, out item)) == -1)
                return saved;
            list.Add(item);
        }

    }
}

public class ParserEnvironment {
    public static bool debug = false;
}


public class Program {
    public static void Main(string[] args) {
        LexicalAnalysis lex = new LexicalAnalysis();
        lex.OpenFile("../../../hello.c");
        lex.Lex();
        var src = lex.tokens;
        TranslationUnit root;
        int current = _translation_unit.Parse(src, 0, out root);
    }
}
