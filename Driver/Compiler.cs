using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parsing;

public class Compiler {
    private Compiler(String src) {
        this.src = src;

        Scanner scanner = new Scanner(src);
        this.tokens = scanner.Tokens.ToList();

        var parserResult = CParser.Parse(this.tokens);
        if (parserResult.Source.Count() != 1) {
            throw new InvalidOperationException("Error: not finished parsing");
        }

        SyntaxTree.TranslnUnit unit = parserResult.Result;

        //if (CParser.Parse(tokens)) {
        //    throw new InvalidOperationException("Error: not finished parsing");
        //}

        //ast = unit.GetTranslationUnit_();

        var ast = unit.GetTranslnUnit();
        this.ast = Tuple.Create(ast.Env, ast.Value);

        CGenState state = new CGenState();
        this.ast.Item2.CodeGenerate(state);

        assembly = state.ToString();
    }

    public static Compiler FromSrc(String src) {
        return new Compiler(src);
    }

    public static Compiler FromFile(String file_name) {
        if (File.Exists(file_name)) {
            return new Compiler(File.ReadAllText(file_name));
        } else {
            throw new FileNotFoundException($"{file_name} does not exist!");
        }
    }

    public readonly String src;
    public readonly IReadOnlyList<Token> tokens;
    public readonly Tuple<AST.Env, AST.TranslnUnit> ast;
    public readonly String assembly;
}

