using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using CodeGeneration;
using LexicalAnalysis;
using Parsing;

namespace Driver {
    public class Compiler {
        private Compiler(String source) {
            this.Source = source;

            // Lexical analysis
            Scanner scanner = new Scanner(source);
            this.Tokens = scanner.Tokens.ToImmutableList();

            // Parse
            var parserResult = CParsers.Parse(this.Tokens);
            if (parserResult.Source.Count() != 1) {
                throw new InvalidOperationException("Error: not finished parsing");
            }
            this.SyntaxTree = parserResult.Result;

            // Semantic analysis
            var semantReturn = this.SyntaxTree.GetTranslnUnit();
            this.AbstractSyntaxTree = semantReturn.Value;
            this.Environment = semantReturn.Env;

            // Code generation
            var state = new CGenState();
            this.AbstractSyntaxTree.CodeGenerate(state);
            this.Assembly = state.ToString();
        }

        public static Compiler FromSource(String src) {
            return new Compiler(src);
        }

        public static Compiler FromFile(String fileName) {
            if (File.Exists(fileName)) {
                return new Compiler(File.ReadAllText(fileName));
            }
            throw new FileNotFoundException($"{fileName} does not exist!");
        }

        public readonly String Source;
        public readonly ImmutableList<Token> Tokens;
        public readonly AST.TranslnUnit SyntaxTree;
        public readonly ABT.TranslnUnit AbstractSyntaxTree;
        public readonly ABT.Env Environment;
        public readonly String Assembly;
    }
}