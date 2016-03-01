using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using LexicalAnalysis;
using SyntaxTree;

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
    /// The input Type for every parsing function.
    /// </summary>
    public sealed class ParserInput {
        public ParserInput(ParserEnvironment environment, IEnumerable<Token> source) {
            this.Environment = environment;
            this.Source = source;
        }
        public ParserEnvironment Environment { get; }
        public IEnumerable<Token> Source { get; }
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

        IEnumerable<Token> Source { get; }
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

        public IEnumerable<Token> Source {
            get {
                throw new NotSupportedException("Parser failed, can't get source.");
            }
        }
    }

    /// <summary>
    /// A succeeded result.
    /// </summary>
    public sealed class ParserSucceeded : IParserResult {
        public ParserSucceeded(ParserEnvironment environment, IEnumerable<Token> source) {
            this.Environment = environment;
            this.Source = source;
        }

        public Boolean IsSuccessful => true;

        public ParserInput ToInput() => new ParserInput(this.Environment, this.Source);

        public ParserEnvironment Environment { get; }

        public IEnumerable<Token> Source { get; }

        public static ParserSucceeded Create(ParserEnvironment environment, IEnumerable<Token> source) =>
        new ParserSucceeded(environment, source);

        public static ParserSucceeded<R> Create<R>(R result, ParserEnvironment environment, IEnumerable<Token> source) =>
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

        public IEnumerable<Token> Source {
            get {
                throw new NotSupportedException("Parser failed, can't get source.");
            }
        }
    }

    public sealed class ParserSucceeded<R> : IParserResult<R> {
        public ParserSucceeded(R result, ParserEnvironment environment, IEnumerable<Token> source) {
            this.Result = result;
            this.Environment = environment;
            this.Source = source;
        }

        public ParserInput ToInput() => new ParserInput(this.Environment, this.Source);

        public Boolean IsSuccessful => true;

        public R Result { get; }

        public ParserEnvironment Environment { get; }

        public IEnumerable<Token> Source { get; }
    }

}
