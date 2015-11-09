using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

public sealed class Scanner {
    public Scanner(String source) {
        this.Source = source;
        this.FSAs = ImmutableList.Create<FSA>(
            new FSAFloat(),
            new FSAInt(),
            new FSAOperator(),
            new FSAIdentifier(),
            new FSASpace(),
            new FSANewLine(),
            new FSACharConst(),
            new FSAString()
        );
        this.Tokens = Lex();
    }

    public static Scanner FromFile(String fileName) {
        if (File.Exists(fileName)) {
            String source = File.ReadAllText(fileName);
            return FromSource(source);
        }
        throw new FileNotFoundException("Source file does not exist.", fileName);
    }

    public static Scanner FromSource(String source) {
        return new Scanner(source);
    }

    private IEnumerable<Token> Lex() {
        var tokens = new List<Token>();
        for (Int32 i = 0; i < this.Source.Length; ++i) {
            this.FSAs.ForEach(fsa => fsa.ReadChar(this.Source[i]));

            // if no running
            if (this.FSAs.FindIndex(fsa => fsa.GetStatus() == FSAStatus.RUNNING) == -1) {
                Int32 idx = this.FSAs.FindIndex(fsa => fsa.GetStatus() == FSAStatus.END);
                if (idx != -1) {
                    Token token = this.FSAs[idx].RetrieveToken();
                    if (token.Kind != TokenKind.NONE) {
                        tokens.Add(token);
                    }
                    i--;
                    this.FSAs.ForEach(fsa => fsa.Reset());
                } else {
                    Console.WriteLine("error");
                }
            }
        }

        this.FSAs.ForEach(fsa => fsa.ReadEOF());
        // find END
        Int32 idx2 = this.FSAs.FindIndex(fsa => fsa.GetStatus() == FSAStatus.END);
        if (idx2 != -1) {
            Token token = this.FSAs[idx2].RetrieveToken();
            if (token.Kind != TokenKind.NONE) {
                tokens.Add(token);
            }
        } else {
            Console.WriteLine("error");
        }

        tokens.Add(new EmptyToken());
        return tokens;
    }

    public override String ToString() {
        String str = "";
        foreach (Token token in this.Tokens) {
            str += $"{token}\n";
        }
        return str;
    }

    public String Source { get; }
    private ImmutableList<FSA> FSAs { get; }
    public IEnumerable<Token> Tokens { get; }

}
