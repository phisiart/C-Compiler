using System;
using System.Collections.Generic;

public class ParserScope {
    public ParserScope() {
        vars = new List<string>();
        typedef_names = new List<string>();
    }

    public Boolean HasTypedefName(string type) {
        return typedef_names.FindIndex(x => x == type) != -1;
    }

    public void AddTypedefName(string type) {
        typedef_names.Add(type);
    }


    public List<string> typedef_names;
    public List<string> vars;

}

public static class ParserEnvironment {
    static ParserEnvironment() {
        scopes = new Stack<ParserScope>();
        scopes.Push(new ParserScope());
    }

    public static void InScope() {
        scopes.Push(new ParserScope());
    }

    public static void OutScope() {
        scopes.Pop();
    }

    public static Boolean HasTypedefName(string type) {
        return scopes.Peek().HasTypedefName(type);
    }

    public static void AddTypedefName(string type) {
        scopes.Peek().AddTypedefName(type);
    }

    public static Stack<ParserScope> scopes;
}


