using System;
using System.Collections.Generic;

namespace ObsoleteParser {

    public class ParserScope {
        public ParserScope() {
            vars = new List<String>();
            typedef_names = new List<String>();
        }

        public Boolean HasTypedefName(String type) {
            return typedef_names.FindIndex(x => x == type) != -1;
        }

        public void AddTypedefName(String type) {
            typedef_names.Add(type);
        }


        public List<String> typedef_names;
        public List<String> vars;

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

        public static Boolean HasTypedefName(String type) {
            return scopes.Peek().HasTypedefName(type);
        }

        public static void AddTypedefName(String type) {
            scopes.Peek().AddTypedefName(type);
        }

        public static Stack<ParserScope> scopes;
    }


}