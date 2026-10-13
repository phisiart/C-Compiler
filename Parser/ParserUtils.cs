using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using LexicalAnalysis;
using AST;

namespace Parsing {

    /// <summary>
    /// A minimal environment solely for parsing, intended to resolve ambiguity.
    /// </summary>
    public sealed class ParserEnvironment {
        private ParserEnvironment(ImmutableStack<Scope> scopes) {
            this.Scopes = scopes;
        }

        public ParserEnvironment() 
            : this(ImmutableStack.Create(new Scope())) { }

        public ParserEnvironment InScope() =>
            new ParserEnvironment(this.Scopes.Push(new Scope()));

        public ParserEnvironment OutScope() =>
            new ParserEnvironment(this.Scopes.Pop());

        public ParserEnvironment AddSymbol(String name, StorageClsSpec storageClsSpec) =>
            new ParserEnvironment(
                this.Scopes.Pop().Push(
                    this.Scopes.Peek().AddSymbol(name, storageClsSpec)
                )
            );

        public Boolean IsTypedefName(String name) {
            foreach (var scope in this.Scopes) {
                if (scope.Symbols.ContainsKey(name)) {
                    return scope.Symbols[name] == StorageClsSpec.TYPEDEF;
                }
            }
            return false;
        }

        private class Scope {
            public Scope()
                : this(ImmutableDictionary<String, StorageClsSpec>.Empty) { }

            private Scope(ImmutableDictionary<String, StorageClsSpec> symbols) {
                this.Symbols = symbols;
            }

            public Scope AddSymbol(String name, StorageClsSpec storageClsSpec) =>
                new Scope(this.Symbols.Add(name, storageClsSpec));
            
            public ImmutableDictionary<String, StorageClsSpec> Symbols { get; }
        }

        private ImmutableStack<Scope> Scopes { get; }
    }

    /// <summary>
    /// An immutable cursor into the token list.
    /// All cursors created from the same list share a record of the furthest
    /// point any parser has reached, which is used to report syntax errors.
    /// </summary>
    public sealed class TokenStream {
        public TokenStream(IEnumerable<Token> tokens)
            : this(WithEndMarker(tokens), 0, new FailureTracker()) { }

        private TokenStream(ImmutableArray<Token> tokens, Int32 position, FailureTracker tracker) {
            this.Tokens = tokens;
            this.Position = position;
            this.Tracker = tracker;
        }

        private static ImmutableArray<Token> WithEndMarker(IEnumerable<Token> tokens) {
            var array = tokens.ToImmutableArray();
            if (array.IsEmpty || array[array.Length - 1].Kind != TokenKind.NONE) {
                array = array.Add(new EmptyToken());
            }
            return array;
        }

        private ImmutableArray<Token> Tokens { get; }
        private FailureTracker Tracker { get; }

        /// <summary>
        /// The index of the current token.
        /// </summary>
        public Int32 Position { get; }

        public Token Current => this.Tokens[this.Position];

        public Boolean IsAtEnd => this.Current.Kind == TokenKind.NONE;

        public TokenStream Advance() {
            if (this.IsAtEnd) {
                throw new InvalidOperationException("Cannot advance past the end of the token stream.");
            }
            return new TokenStream(this.Tokens, this.Position + 1, this.Tracker);
        }

        /// <summary>
        /// Records that <paramref name="expected"/> was tried and did not match the current token.
        /// </summary>
        public void Expected(String expected) =>
            this.Tracker.Record(this.Position, expected);

        /// <summary>
        /// Builds an error describing the furthest point the parse reached.
        /// </summary>
        public CompilationError SyntaxError() {
            Int32 position = Math.Max(this.Tracker.FurthestPosition, this.Position);
            Token token = this.Tokens[position];
            var expected = position == this.Tracker.FurthestPosition
                ? this.Tracker.FurthestExpected
                : ImmutableSortedSet<String>.Empty;

            String message;
            if (expected.Count == 0 || expected.Count > MaxExpectedInMessage) {
                message = $"unexpected {token.Describe()}";
            } else {
                String alternatives = expected.Count == 1
                    ? expected.Single()
                    : String.Join(", ", expected.Take(expected.Count - 1)) + " or " + expected.Last();
                message = token.Kind == TokenKind.NONE
                    ? $"expected {alternatives} at end of file"
                    : $"expected {alternatives} before {token.Describe()}";
            }
            return new CompilationError(message, token.Location);
        }

        /// <summary>
        /// Listing more alternatives than this is more noise than help.
        /// </summary>
        private const Int32 MaxExpectedInMessage = 3;

        private sealed class FailureTracker {
            public Int32 FurthestPosition { get; private set; } = -1;

            public ImmutableSortedSet<String> FurthestExpected { get; private set; } =
                ImmutableSortedSet<String>.Empty;

            public void Record(Int32 position, String expected) {
                if (position > this.FurthestPosition) {
                    this.FurthestPosition = position;
                    this.FurthestExpected = ImmutableSortedSet.Create(StringComparer.Ordinal, expected);
                } else if (position == this.FurthestPosition) {
                    this.FurthestExpected = this.FurthestExpected.Add(expected);
                }
            }
        }
    }

    /// <summary>
    /// The input Type for every parsing function.
    /// </summary>
    public sealed class ParserInput {
        public ParserInput(ParserEnvironment environment, TokenStream source) {
            this.Environment = environment;
            this.Source = source;
        }
        public ParserInput(ParserEnvironment environment, IEnumerable<Token> tokens)
            : this(environment, new TokenStream(tokens)) { }
        public ParserEnvironment Environment { get; }
        public TokenStream Source { get; }
        public IParserResult<R> Parse<R>(IParser<R> parser) =>
            parser.Parse(this);
    }

    /// <summary>
    /// A parser result with/without content.
    /// </summary>
    public interface IParserResult {
        ParserInput ToInput();

        Boolean IsSuccessful { get; }

        ParserEnvironment Environment { get; }

        TokenStream Source { get; }
    }

    /// <summary>
    /// A failed result.
    /// </summary>
    public sealed class ParserFailed : IParserResult {
        public ParserInput ToInput() {
            throw new InvalidOperationException("Parser failed, can't construct input.");
        }

        public Boolean IsSuccessful => false;

        public ParserEnvironment Environment {
            get {
                throw new NotSupportedException("Parser failed, can't get environment.");
            }
        }

        public TokenStream Source {
            get {
                throw new NotSupportedException("Parser failed, can't get source.");
            }
        }
    }

    /// <summary>
    /// A succeeded result.
    /// </summary>
    public sealed class ParserSucceeded : IParserResult {
        public ParserSucceeded(ParserEnvironment environment, TokenStream source) {
            this.Environment = environment;
            this.Source = source;
        }

        public Boolean IsSuccessful => true;

        public ParserInput ToInput() => new ParserInput(this.Environment, this.Source);

        public ParserEnvironment Environment { get; }

        public TokenStream Source { get; }

        public static ParserSucceeded Create(ParserEnvironment environment, TokenStream source) =>
        new ParserSucceeded(environment, source);

        public static ParserSucceeded<R> Create<R>(R result, ParserEnvironment environment, TokenStream source) =>
        new ParserSucceeded<R>(result, environment, source);
    }

    /// <summary>
    /// A parser result with content.
    /// </summary>
    public interface IParserResult<out R> : IParserResult {
        R Result { get; }
    }

    public sealed class ParserFailed<R> : IParserResult<R> {
        public ParserInput ToInput() {
            throw new InvalidOperationException("Parser failed, can't construct input.");
        }

        public Boolean IsSuccessful => false;

        public R Result {
            get {
                throw new NotSupportedException("Parser failed, can't get result.");
            }
        }

        public ParserEnvironment Environment {
            get {
                throw new NotSupportedException("Parser failed, can't get environment.");
            }
        }

        public TokenStream Source {
            get {
                throw new NotSupportedException("Parser failed, can't get source.");
            }
        }
    }

    public sealed class ParserSucceeded<R> : IParserResult<R> {
        public ParserSucceeded(R result, ParserEnvironment environment, TokenStream source) {
            this.Result = result;
            this.Environment = environment;
            this.Source = source;
        }

        public ParserInput ToInput() => new ParserInput(this.Environment, this.Source);

        public Boolean IsSuccessful => true;

        public R Result { get; }

        public ParserEnvironment Environment { get; }

        public TokenStream Source { get; }
    }

}
