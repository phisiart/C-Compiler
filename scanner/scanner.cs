using System;
using System.Collections.Generic;
using System.IO;

public class Scanner {
    public Scanner() {
        fsas = new List<FSA>() {
            new FSAFloat(),
            new FSAInt(),
            new FSAOperator(),
            new FSAIdentifier(),
            new FSASpace(),
            new FSANewLine(),
            new FSACharConst(),
            new FSAString(),
        };
    }

    public void OpenFile(String file_name) {
        if (File.Exists(file_name)) {
            src = File.ReadAllText(file_name);
        } else {
            Console.WriteLine("{0} does not exist!", file_name);
        }
    }

    public void Lex() {
        Int32 pos = 0;
        tokens = new List<Token>();
        for (Int32 i = 0; i < src.Length; ++i) {

            fsas.ForEach(fsa => fsa.ReadChar(src[i]));

            // if no running
            if (fsas.FindIndex(fsa => fsa.GetStatus() == FSAStatus.RUN) == -1) {
                Int32 idx = fsas.FindIndex(fsa => fsa.GetStatus() == FSAStatus.END);
                if (idx != -1) {
                    // Console.WriteLine("> " + src.Substring(pos, i - pos));
                    Token token = fsas[idx].RetrieveToken();
                    if (token.type != TokenType.NONE) {
                        tokens.Add(token);
                    }
                    //Console.WriteLine(fsas[idx].RetrieveToken());
                    pos = i;
                    i--;
                    fsas.ForEach(fsa => fsa.Reset());
                } else {
                    Console.WriteLine("error");
                }
            }
        }

        fsas.ForEach(fsa => fsa.ReadEOF());
        // find END
        Int32 idx2 = fsas.FindIndex(fsa => fsa.GetStatus() == FSAStatus.END);
        if (idx2 != -1) {
            Token token = fsas[idx2].RetrieveToken();
            if (token.type != TokenType.NONE) {
                tokens.Add(token);
            }
            //Console.WriteLine("> " + src.Substring(pos, src.Length - pos));
        } else {
            Console.WriteLine("error");
        }

        tokens.Add(new EmptyToken());
    }

    public override String ToString() {
        String str = "";
        tokens.ForEach(token => str += token.ToString() + "\n");
        return str;
    }

    public String src;
    private List<FSA> fsas;
    public List<Token> tokens;

}
