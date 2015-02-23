using System;
using System.Collections.Generic;

public class Scope {
    public Scope() {
        vars = new List<String>();
        typedef_names = new List<string>();
    }

    public bool HasTypedefName(String type) {
        return typedef_names.FindIndex(x => x == type) != -1;
    }

    public void AddTypedefName(String type) {
        typedef_names.Add(type);
    }


    public List<String> typedef_names;
    public List<String> vars;

}

public static class ScopeEnvironment {
    static ScopeEnvironment() {
        scopes = new Stack<Scope>();
        scopes.Push(new Scope());
    }

    public static void InScope() {
        scopes.Push(new Scope());
    }

    public static void OutScope() {
        scopes.Pop();
    }

    public static bool HasTypedefName(String type) {
        return scopes.Peek().HasTypedefName(type);
    }

    public static void AddTypedefName(String type) {
        scopes.Peek().AddTypedefName(type);
    }

    public static Stack<Scope> scopes;
}


