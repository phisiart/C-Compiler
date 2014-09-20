using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Parser {
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

}

class ParserEnvironment {
    public static bool debug = false;
}

class Scope {
    public Scope() {
        vars = new List<String>();
        typedef_names = new List<string>();
    }

    public bool HasVariable(String var) {
        return vars.FindIndex(x => x == var) != -1;
    }

    public bool HasTypedefName(String type) {
        return typedef_names.FindIndex(x => x == type) != -1;
    }

    public bool HasIdentifier(String id) {
        return HasVariable(id) || HasTypedefName(id);
    }

    public void AddTypedefName(String type) {
        typedef_names.Add(type);
    }

    public List<String> typedef_names;
    public List<String> vars;
}

class ScopeSandbox {
    public ScopeSandbox() {
        scopes = new Stack<Scope>();
        scopes.Push(new Scope());
    }

    public void InScope() {
        scopes.Push(new Scope());
    }

    public void OutScope() {
        scopes.Pop();
    }

    public bool HasVariable(String var) {
        return scopes.Peek().HasVariable(var);
    }

    public bool HasTypedefName(String type) {
        return scopes.Peek().HasTypedefName(type);
    }

    public void AddTypedefName(String type) {
        scopes.Peek().AddTypedefName(type);
    }

    public bool HasIdentifier(String id) {
        return scopes.Peek().HasIdentifier(id);
    }

    public Stack<Scope> scopes;
}

static class ScopeEnvironment {
    static ScopeEnvironment() {
        sandboxes = new Stack<ScopeSandbox>();
        sandboxes.Push(new ScopeSandbox());
    }

    public static void PushSandbox() {
        if (sandboxes.Count == 0) {
            return;
        }
        sandboxes.Push(sandboxes.Peek());
    }

    public static void PopSandbox() {
        if (sandboxes.Count < 2) {
            return;
        }
        ScopeSandbox top = sandboxes.Pop();
        sandboxes.Pop();
        sandboxes.Push(top);
    }

    public static void InScope() {
        sandboxes.Peek().InScope();
    }

    public static void OutScope() {
        sandboxes.Peek().OutScope();
    }

    public static bool HasVariable(String var) {
        return sandboxes.Peek().HasVariable(var);
    }

    public static bool HasTypedefName(String type) {
        return sandboxes.Peek().HasTypedefName(type);
    }

    public static void AddTypedefName(String type) {
        sandboxes.Peek().AddTypedefName(type);
    }

    public static bool HasIdentifier(String id) {
        return sandboxes.Peek().HasIdentifier(id);
    }

    public static Stack<ScopeSandbox> sandboxes;

}

interface PTNode {
}
interface ASTNode {
}

class Program {
    public static void Main(string[] args) {
        Console.WriteLine("hello");
    }
}
