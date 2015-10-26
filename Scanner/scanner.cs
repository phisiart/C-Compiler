using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

public class Scanner {
    public Scanner(String source) {
        this.Source = source;
        this.Tokens = this.Lex();
    }

    public static Scanner FromFile(String fileName) {
        if (File.Exists(fileName)) {
            String source = File.ReadAllText(fileName);
            return FromSource(source);
        } else {
            throw new FileNotFoundException("Source file does not exist.", fileName);
        }
    }

    public static Scanner FromSource(String source) {
        return new Scanner(source);
    }

    private IEnumerable<Token> Lex() {
        // Tokens = new List<Token>();
        for (Int32 i = 0; i < Source.Length; ++i) {

            FSAs.ForEach(fsa => fsa.ReadChar(Source[i]));

            // if no running
            if (FSAs.FindIndex(fsa => fsa.GetStatus() == FSAStatus.RUNNING) == -1) {
                Int32 idx = FSAs.FindIndex(fsa => fsa.GetStatus() == FSAStatus.END);
                if (idx != -1) {
                    // Console.WriteLine("> " + src.Substring(pos, i - pos));
                    Token token = FSAs[idx].RetrieveToken();
                    if (token.type != TokenType.NONE) {
                        yield return token;// Tokens.Add(token);
                    }
                    //Console.WriteLine(fsas[idx].RetrieveToken());
                    i--;
                    FSAs.ForEach(fsa => fsa.Reset());
                } else {
                    Console.WriteLine("error");
                }
            }
        }

        FSAs.ForEach(fsa => fsa.ReadEOF());
        // find END
        Int32 idx2 = FSAs.FindIndex(fsa => fsa.GetStatus() == FSAStatus.END);
        if (idx2 != -1) {
            Token token = FSAs[idx2].RetrieveToken();
            if (token.type != TokenType.NONE) {
                yield return token; // Tokens.Add(token);
            }
            //Console.WriteLine("> " + src.Substring(pos, src.Length - pos));
        } else {
            Console.WriteLine("error");
        }

        yield return new EmptyToken();// Tokens.Add(new EmptyToken());
    }

    public override String ToString() {
        String str = "";
        foreach (Token token in this.Tokens) {
            str += $"{token}\n";
        }
        return str;
    }

    public String Source { get; }
    private ImmutableList<FSA> FSAs { get; } = ImmutableList.Create<FSA>(
        new FSAFloat(),
        new FSAInt(),
        new FSAOperator(),
        new FSAIdentifier(),
        new FSASpace(),
        new FSANewLine(),
        new FSACharConst(),
        new FSAString()
    );
    public IEnumerable<Token> Tokens { get; }

}
