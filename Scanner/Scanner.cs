using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

namespace LexicalAnalysis {
    public sealed class Scanner {
        public Scanner(String source) {
            this.Source = source;
            this.LineStarts = ComputeLineStarts(source);
            this.FSAs = ImmutableList.Create<FSA>(
                new FSAFloat(),
                new FSAInt(),
                new FSAOperator(),
                new FSAIdentifier(),
                new FSASpace(),
                new FSANewLine(),
                new FSAComment(),
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

            // The index of the first character of the token being scanned.
            Int32 tokenStart = 0;

            for (Int32 i = 0; i < this.Source.Length; ++i) {
                this.FSAs.ForEach(fsa => fsa.ReadChar(this.Source[i]));

                // if no running
                if (this.FSAs.FindIndex(fsa => fsa.GetStatus() == FSAStatus.RUNNING) == -1) {
                    Int32 idx = this.FSAs.FindIndex(fsa => fsa.GetStatus() == FSAStatus.END);
                    if (idx == -1) {
                        throw this.InvalidToken(tokenStart, i);
                    }

                    // The current character is not part of the token; it will be read again.
                    Token token = this.FSAs[idx].RetrieveToken();
                    if (token.Kind != TokenKind.NONE) {
                        AddToken(token, tokenStart, i);
                    }
                    tokenStart = i;
                    i--;
                    this.FSAs.ForEach(fsa => fsa.Reset());
                }
            }

            if (tokenStart < this.Source.Length) {
                this.FSAs.ForEach(fsa => fsa.ReadEOF());
                // find END
                Int32 idx = this.FSAs.FindIndex(fsa => fsa.GetStatus() == FSAStatus.END);
                if (idx == -1) {
                    throw this.UnterminatedToken(tokenStart);
                }
                Token token = this.FSAs[idx].RetrieveToken();
                if (token.Kind != TokenKind.NONE) {
                    AddToken(token, tokenStart, this.Source.Length);
                }
            }

            // The end-of-file marker.
            AddToken(new EmptyToken(), this.Source.Length, this.Source.Length);
            return tokens;

            void AddToken(Token token, Int32 start, Int32 end) {
                token.Location = this.LocationOf(start);
                token.Text = this.Source.Substring(start, end - start);
                tokens.Add(token);
            }
        }

        /// <summary>
        /// The 1-based line and column of the character at <paramref name="index"/>.
        /// </summary>
        public SourceLocation LocationOf(Int32 index) {
            // Find the last line that starts at or before index.
            Int32 line = this.LineStarts.BinarySearch(index);
            if (line < 0) {
                line = ~line - 1;
            }
            return new SourceLocation(line + 1, index - this.LineStarts[line] + 1);
        }

        private static ImmutableList<Int32> ComputeLineStarts(String source) {
            var lineStarts = ImmutableList.CreateBuilder<Int32>();
            lineStarts.Add(0);
            for (Int32 i = 0; i < source.Length; ++i) {
                if (source[i] == '\n') {
                    lineStarts.Add(i + 1);
                }
            }
            return lineStarts.ToImmutable();
        }

        private CompilationError InvalidToken(Int32 tokenStart, Int32 badCharIndex) {
            if (tokenStart == badCharIndex) {
                return new CompilationError(
                    $"unexpected character {Utils.Quote(this.Source[badCharIndex])}",
                    this.LocationOf(badCharIndex)
                );
            }
            String text = this.Source.Substring(tokenStart, badCharIndex - tokenStart + 1);
            String message;
            if (this.Source[badCharIndex] == '\n' && text.StartsWith("\"")) {
                message = "unterminated string literal";
            } else if (this.Source[badCharIndex] == '\n' && text.StartsWith("'")) {
                message = "unterminated character constant";
            } else {
                message = $"invalid token {Utils.Quote(text)}";
            }
            return new CompilationError(message, this.LocationOf(tokenStart));
        }

        private CompilationError UnterminatedToken(Int32 tokenStart) {
            String text = this.Source.Substring(tokenStart);
            String message;
            if (text.StartsWith("/*")) {
                message = "unterminated comment";
            } else if (text.StartsWith("\"")) {
                message = "unterminated string literal";
            } else if (text.StartsWith("'")) {
                message = "unterminated character constant";
            } else {
                message = $"invalid token {Utils.Quote(text)} at end of file";
            }
            return new CompilationError(message, this.LocationOf(tokenStart));
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
        private ImmutableList<Int32> LineStarts { get; }
        public IEnumerable<Token> Tokens { get; }

    }
}