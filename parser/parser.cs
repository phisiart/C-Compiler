using System;
using System.Collections.Generic;
using System.Linq;

using SyntaxTree;

public interface ParseRule {
}

public class PTNode {
}


public class Parser {
    public static Boolean IsQuestionMark(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.QUESTION) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsCOMMA(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.COMMA) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsLCURL(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.LCURL) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsRCURL(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.RCURL) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsPERIOD(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.PERIOD) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsEllipsis(List<Token> src, Int32 begin) {
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

    public static Boolean IsSEMICOLON(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.SEMICOLON) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsKeyword(Token token, KeywordVal val) {
        if (token.type == TokenType.KEYWORD) {
            if (((TokenKeyword)token).val == val) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsOperator(Token token, OperatorVal val) {
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

    public static Int32 ParseIdentifier(List<Token> src, Int32 begin, out String id) {
        if (src[begin].type == TokenType.IDENTIFIER) {
            id = ((TokenIdentifier)src[begin]).val;
            return begin + 1;
        } else {
            id = null;
            return -1;
        }
    }

    public static List<Token> GetTokensFromString(String src) {
        Scanner lex = new Scanner();
        lex.src = src;
        lex.Lex();
        return lex.tokens;
    }

    // EatOperator : (src, ref current, val) -> Boolean
    // =============================================
    // tries to eat an operator
    // if succeed, current++, return true
    // if fail, current remains the same, return false
    // 
    public static Boolean EatOperator(List<Token> src, ref Int32 current, OperatorVal val) {
        if (src[current].type != TokenType.OPERATOR) {
            return false;
        }

        if (((TokenOperator)src[current]).val != val) {
            return false;
        }

        current++;
        return true;
    }

    public delegate Int32 FParse<TRet>(List<Token> src, Int32 begin, out TRet node);

    public static FParse<Boolean> GetOperatorParser(OperatorVal val) {
        return delegate (List<Token> src, Int32 begin, out Boolean succeeded) {
            if (succeeded = (
                src[begin].type == TokenType.OPERATOR && ((TokenOperator)src[begin]).val == val
            )) {
                return begin + 1;
            } else {
                return -1;
            }
        };
    }

    /// <summary>
    /// turn a parsing function into an optional parsing function
    /// if parsing fails, return the default value
    /// </summary>
    public static FParse<TRet> GetOptionalParser<TRet>(TRet default_val, FParse<TRet> Parse) {
        return delegate (List<Token> src, Int32 begin, out TRet node) {
            Int32 current;
            if ((current = Parse(src, begin, out node)) == -1) {
                // if parsing fails: return default value
                node = default_val;
                return begin;
            } else {
                return current;
            }
        };
    }

    public static FParse<TRet> GetOptionalParser<TRet>(FParse<TRet> Parse) {
        return delegate (List<Token> src, Int32 begin, out TRet node) {
            Int32 current;
            if ((current = Parse(src, begin, out node)) == -1) {
                // if parsing fails: return default value
                node = default(TRet);
                return begin;
            } else {
                return current;
            }
        };
    }

    public static Int32 ParseOptional<TRet>(List<Token> src, Int32 begin, TRet default_val, out TRet node, FParse<TRet> Parse) where TRet : class {
        return GetOptionalParser(default_val, Parse)(src, begin, out node);
    }

    public static Int32 ParseList<TRet>(List<Token> src, Int32 begin, out List<TRet> list, FParse<TRet> Parse) {
        Int32 current = begin;

        list = new List<TRet>();
        TRet item;

        while (true) {
            Int32 saved = current;
            if ((current = Parse(src, current, out item)) == -1) {
                return saved;
            }
            list.Add(item);
        }

    }

    public static Int32 ParseNonEmptyList<TRet>(List<Token> src, Int32 begin, out List<TRet> list, FParse<TRet> Parse) {
        begin = ParseList(src, begin, out list, Parse);
        if (list.Any()) {
            return begin;
        } else {
            return -1;
        }
    }
    
    public static Int32 Parse2Choices<TRet, T1, T2>(List<Token> src, Int32 begin, out TRet node, FParse<T1> Parse1, FParse<T2> Parse2)
        where T1 : TRet
        where T2 : TRet
        where TRet : PTNode {
        Int32 ret;

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

    public static FParse<TRet> GetChoicesParser<TRet>(List<FParse<TRet>> ParseFuncs) where TRet : class {
        return delegate (List<Token> src, Int32 begin, out TRet node) {
            Int32 r;
            foreach (FParse<TRet> Parse in ParseFuncs) {
                if ((r = Parse(src, begin, out node)) != -1) {
                    return r;
                }
            }
            node = null;
            return -1;
        };
    }

    public static Int32 ParseChoices<TRet>(List<Token> src, Int32 begin, out TRet node, List<FParse<TRet>> ParseFuncs) where TRet : class {
        return GetChoicesParser(ParseFuncs)(src, begin, out node);
    }

    public static Int32 ParseNonEmptyListWithSep<TRet>(List<Token> src, Int32 pos, out List<TRet> list, FParse<TRet> Parse, OperatorVal op) where TRet : PTNode {
        list = new List<TRet>();
        TRet item;

        if ((pos = Parse(src, pos, out item)) == -1)
            return -1;
        list.Add(item);

        while (true) {
            Int32 saved = pos;
            if (!Parser.EatOperator(src, ref pos, op))
                return saved;
            if ((pos = Parse(src, pos, out item)) == -1)
                return saved;
            list.Add(item);
        }

    }

    public static Int32 ParseParenExpr(List<Token> src, Int32 begin, out Expression expr) {
        if (!Parser.EatOperator(src, ref begin, OperatorVal.LPAREN)) {
            expr = null;
            return -1;
        }

        if ((begin = _expression.Parse(src, begin, out expr)) == -1) {
            expr = null;
            return -1;
        }

        if (!Parser.EatOperator(src, ref begin, OperatorVal.RPAREN)) {
            expr = null;
            return -1;
        }

        return begin;
    }

	public static FParse<TRet> GetBraceSurroundedParser<TRet>(FParse<TRet> Parse) where TRet : class {
		return delegate (List<Token> src, Int32 begin, out TRet node) {
			if (!Parser.EatOperator(src, ref begin, OperatorVal.LCURL)) {
				node = null;
				return -1;
			}

			if ((begin = Parse(src, begin, out node)) == -1) {
				node = null;
				return -1;
			}

			if (!Parser.EatOperator(src, ref begin, OperatorVal.RCURL)) {
				node = null;
				return -1;
			}

			return begin;
		};
	}

	public delegate TAfter FModifier<TAfter, TBefore>(TBefore before);
	public static FParse<TAfter> GetModifiedParser<TAfter, TBefore>(FParse<TBefore> Parse, FModifier<TAfter, TBefore> ModifierFunc) where TAfter : class {
		return delegate (List<Token> src, Int32 begin, out TAfter node) {
			TBefore before;
			if ((begin = Parse(src, begin, out before)) == -1) {
				node = null;
				return -1;
			}

			node = ModifierFunc(before);
			return begin;
		};
	}

    public delegate TRet FBinaryCombine<TRet, TRet1, TRet2>(TRet1 obj1, TRet2 obj2);

    public delegate TRet FTernaryCombine<TRet, TRet1, TRet2, TRet3>(TRet1 obj1, TRet2 obj2, TRet3 obj3);

    /// <summary>
    /// Pass in two parsing functions, and a combining function,
    /// Return a parsing function
    /// </summary>
    public static FParse<TRet> GetSequenceParser<TRet, TRet1, TRet2>(
        FParse<TRet1> ParseFirstNode,
        FParse<TRet2> ParseSecondNode,
        FBinaryCombine<TRet, TRet1, TRet2> Combine
    ) where TRet : class {
        return delegate (List<Token> src, Int32 begin, out TRet node) {
            TRet1 node1;
            if ((begin = ParseFirstNode(src, begin, out node1)) == -1) {
                node = null;
                return -1;
            }
            TRet2 node2;
            if ((begin = ParseSecondNode(src, begin, out node2)) == -1) {
                node = null;
                return -1;
            }
            node = Combine(node1, node2);
            return begin;
        };
    }

    /// <summary>
    /// Pass in two parsing functions, and a combining function,
    /// Return a parsing function
    /// </summary>
    public static FParse<TRet> GetSequenceParser<TRet, TRet1, TRet2, TRet3>(
        FParse<TRet1> ParseFirstNode,
        FParse<TRet2> ParseSecondNode,
        FParse<TRet3> ParseThirdNode,
        FTernaryCombine<TRet, TRet1, TRet2, TRet3> Combine
    ) where TRet : class {
        return delegate (List<Token> src, Int32 begin, out TRet node) {
            TRet1 node1;
            if ((begin = ParseFirstNode(src, begin, out node1)) == -1) {
                node = null;
                return -1;
            }
            TRet2 node2;
            if ((begin = ParseSecondNode(src, begin, out node2)) == -1) {
                node = null;
                return -1;
            }
            TRet3 node3;
            if ((begin = ParseThirdNode(src, begin, out node3)) == -1) {
                node = null;
                return -1;
            }
            node = Combine(node1, node2, node3);
            return begin;
        };
    }

    /// <summary>
    /// Parse a sequence
    /// </summary>
    public static Int32 ParseSequence<TRet, TRet1, TRet2>(
        List<Token> src,
        Int32 begin,
        out TRet node,
        FParse<TRet1> ParseFirstNode,
        FParse<TRet2> ParseSecondNode,
        FBinaryCombine<TRet, TRet1, TRet2> Combine
    ) where TRet : class {
        return GetSequenceParser(ParseFirstNode, ParseSecondNode, Combine)(src, begin, out node);
    }

    /// <summary>
    /// Parse a sequence
    /// </summary>
    public static Int32 ParseSequence<TRet, TRet1, TRet2, TRet3>(
        List<Token> src,
        Int32 begin,
        out TRet node,
        FParse<TRet1> ParseFirstNode,
        FParse<TRet2> ParseSecondNode,
        FParse<TRet3> ParseThirdNode,
        FTernaryCombine<TRet, TRet1, TRet2, TRet3> Combine
    ) where TRet : class {
        return GetSequenceParser(ParseFirstNode, ParseSecondNode, ParseThirdNode, Combine)(src, begin, out node);
    }

}

