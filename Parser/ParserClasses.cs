using System;
using System.Collections.Immutable;
using System.Linq;

namespace Parsing {
    public class ParserThenParser<R1, R2> : IParser<Tuple<R2, R1>> {
        public ParserThenParser(IParser<R1> firstParser, IParser<R2> secondParser) {
            this.FirstParser = firstParser;
            this.SecondParser = secondParser;
        }

        public IParser<R1> FirstParser { get; }
        public IParser<R2> SecondParser { get; }
        public RuleCombining Combining => RuleCombining.THEN;

        public IParserResult<Tuple<R2, R1>> Parse(ParserInput input) {
            var firstResult = this.FirstParser.Parse(input);
            if (!firstResult.IsSuccessful) {
                return new ParserFailed<Tuple<R2, R1>>();
            }
            var secondResult = this.SecondParser.Parse(firstResult.ToInput());
            if (!secondResult.IsSuccessful) {
                return new ParserFailed<Tuple<R2, R1>>();
            }
            return ParserSucceeded.Create(Tuple.Create(secondResult.Result, firstResult.Result), secondResult.Environment, secondResult.Source);
        }
    }

    public class ParserThenConsumer<R> : IParser<R> {
        public ParserThenConsumer(IParser<R> parser, IConsumer consumer) {
            this.Parser = parser;
            this.Consumer = consumer;
        }

        public IParser<R> Parser { get; }
        public IConsumer Consumer { get; }
        public RuleCombining Combining => RuleCombining.THEN;

        public IParserResult<R> Parse(ParserInput input) {
            var firstResult = this.Parser.Parse(input);
            if (!firstResult.IsSuccessful) {
                return new ParserFailed<R>();
            }
            var secondResult = this.Consumer.Consume(firstResult.ToInput());
            if (!secondResult.IsSuccessful) {
                return new ParserFailed<R>();
            }
            return ParserSucceeded.Create(firstResult.Result, secondResult.Environment, secondResult.Source);
        }
    }

    public class ParserThenTransformer<R1, R2> : IParser<R2> {
        public ParserThenTransformer(IParser<R1> parser, ITransformer<R1, R2> transformer) {
            this.Parser = parser;
            this.Transformer = transformer;
        }

        public IParser<R1> Parser { get; }
        public ITransformer<R1, R2> Transformer { get; }
        public RuleCombining Combining => RuleCombining.THEN;

        public IParserResult<R2> Parse(ParserInput input) {
            var firstResult = this.Parser.Parse(input);
            if (!firstResult.IsSuccessful) {
                return new ParserFailed<R2>();
            }
            return this.Transformer.Transform(firstResult.Result, firstResult.ToInput());
        }
    }

    public class ConsumerThenParser<R> : IParser<R> {
        public ConsumerThenParser(IConsumer consumer, IParser<R> parser) {
            this.Consumer = consumer;
            this.Parser = parser;
        }

        public IConsumer Consumer { get; }
        public IParser<R> Parser { get; }
        public RuleCombining Combining => RuleCombining.THEN;

        public IParserResult<R> Parse(ParserInput input) {
            var firstResult = this.Consumer.Consume(input);
            if (!firstResult.IsSuccessful) {
                return new ParserFailed<R>();
            }
            return this.Parser.Parse(firstResult.ToInput());
        }
    }

    public class ConsumerThenConsumer : IConsumer {
        public ConsumerThenConsumer(IConsumer firstConsumer, IConsumer secondConsumer) {
            this.FirstConsumer = firstConsumer;
            this.SecondConsumer = secondConsumer;
        }

        public IConsumer FirstConsumer { get; }
        public IConsumer SecondConsumer { get; }

        public IParserResult Consume(ParserInput input) {
            var result1 = this.FirstConsumer.Consume(input);
            if (!result1.IsSuccessful) {
                return result1;
            }
            return this.SecondConsumer.Consume(result1.ToInput());
        }
    }

    public class TransformerThenParser<S, R1, R2> : ITransformer<S, Tuple<R2, R1>> {
        public TransformerThenParser(ITransformer<S, R1> transformer, IParser<R2> parser) {
            this.Transformer = transformer;
            this.Parser = parser;
        }

        public ITransformer<S, R1> Transformer { get; }
        public IParser<R2> Parser { get; }

        public IParserResult<Tuple<R2, R1>> Transform(S seed, ParserInput input) {
            var result1 = this.Transformer.Transform(seed, input);
            if (!result1.IsSuccessful) {
                return new ParserFailed<Tuple<R2, R1>>();
            }
            var result2 = this.Parser.Parse(result1.ToInput());
            if (!result2.IsSuccessful) {
                return new ParserFailed<Tuple<R2, R1>>();
            }
            return ParserSucceeded.Create(Tuple.Create(result2.Result, result1.Result), result2.Environment, result2.Source);
        }
    }

    public class TransformerThenConsumer<S, R> : ITransformer<S, R> {
        public TransformerThenConsumer(ITransformer<S, R> transformer, IConsumer consumer) {
            this.Transformer = transformer;
            this.Consumer = consumer;
        }

        public ITransformer<S, R> Transformer { get; }
        public IConsumer Consumer { get; }

        public IParserResult<R> Transform(S seed, ParserInput input) {
            var result1 = this.Transformer.Transform(seed, input);
            if (!result1.IsSuccessful) {
                return result1;
            }
            var result2 = this.Consumer.Consume(result1.ToInput());
            if (!result2.IsSuccessful) {
                return new ParserFailed<R>();
            }
            return ParserSucceeded.Create(result1.Result, result2.Environment, result2.Source);
        }
    }

    public class TransformerThenTransformer<S, I, R> : ITransformer<S, R> {
        public TransformerThenTransformer(ITransformer<S, I> firstTransformer, ITransformer<I, R> secondTransformer) {
            this.FirstTransformer = firstTransformer;
            this.SecondTransformer = secondTransformer;
        }

        public ITransformer<S, I> FirstTransformer { get; }
        public ITransformer<I, R> SecondTransformer { get; }

        public IParserResult<R> Transform(S seed, ParserInput input) {
            var result1 = this.FirstTransformer.Transform(seed, input);
            if (!result1.IsSuccessful) {
                return new ParserFailed<R>();
            }
            return this.SecondTransformer.Transform(result1.Result, result1.ToInput());
        }
    }

    public enum RuleCombining {
        NONE,
        THEN,
        OR
    }

    /// <summary>
    /// A parser consumes one or several tokens, and produces a result.
    /// </summary>
    public interface IParser<out R> {
        IParserResult<R> Parse(ParserInput input);
        RuleCombining Combining { get; }
    }

    public static class Parser {
        public static NamedParser<R> Create<R>(String name) =>
            new NamedParser<R>(name);
        public static IParser<R> Seed<R>(R seed) =>
            new AlwaysSucceedingParser<R>(seed);
    }

    public class NamedParser<R> : IParser<R> {
        public NamedParser(String name) {
            this.Name = name;
            this.Parser = new SetOnce<IParser<R>>();
        }

        public SetOnce<IParser<R>> Parser { get; }

        public void Is(IParser<R> parser) {
            this.Parser.Value = parser;
        }

        public IParserResult<R> Parse(ParserInput input) =>
            this.Parser.Value.Parse(input);

        public String Name { get; }
        public String Rule => this.Parser.Value.ToString();
        public RuleCombining Combining => RuleCombining.NONE;
        public override String ToString() => this.Name;
    }

    public class OptionalParser<R> : IParser<Option<R>> {
        public OptionalParser(IParser<R> parser) {
            this.Parser = parser;
        }
        public IParser<R> Parser { get; }
        public RuleCombining Combining => RuleCombining.NONE;
        public IParserResult<Option<R>> Parse(ParserInput input) {
            var result = this.Parser.Parse(input);
            if (result.IsSuccessful) {
                return ParserSucceeded.Create(new Some<R>(result.Result), result.Environment, result.Source);
            } else {
                return ParserSucceeded.Create(new None<R>(), input.Environment, input.Source);
            }
        }
    }

    public class OptionalParserWithDefault<R> : IParser<R> {
        public OptionalParserWithDefault(IParser<R> parser, R defaultValue) {
            this.Parser = parser;
            this.DefaultValue = defaultValue;
        }
        public IParser<R> Parser { get; }
        public R DefaultValue { get; }
        public RuleCombining Combining => RuleCombining.NONE;
        public IParserResult<R> Parse(ParserInput input) {
            var result = this.Parser.Parse(input);
            if (result.IsSuccessful) {
                return result;
            } else {
                return ParserSucceeded.Create(this.DefaultValue, input.Environment, input.Source);
            }
        }
    }

    public class AlwaysSucceedingParser<R> : IParser<R> {
        public AlwaysSucceedingParser(R result) {
            this.Result = result;
        }
        public RuleCombining Combining => RuleCombining.NONE;
        public R Result { get; }
        public IParserResult<R> Parse(ParserInput input) =>
            ParserSucceeded.Create(this.Result, input.Environment, input.Source);
    }

    public class ParserThenCheck<R> : IParser<R> {
        public ParserThenCheck(IParser<R> parser, Predicate<IParserResult<R>> predicate) {
            this.Parser = parser;
            this.Predicate = predicate;
        }
        public IParser<R> Parser { get; }
        public RuleCombining Combining => RuleCombining.NONE;
        public Predicate<IParserResult<R>> Predicate { get; }
        public IParserResult<R> Parse(ParserInput input) {
            var result1 = this.Parser.Parse(input);
            if (result1.IsSuccessful && !this.Predicate(result1)) {
                return new ParserFailed<R>();
            }
            return result1;
        }
    }

    public class ParserOrParser<R> : IParser<R> {
        public ParserOrParser(IParser<R> firstParser, IParser<R> secondParser) {
            this.FirstParser = firstParser;
            this.SecondParser = secondParser;
        }
        public IParser<R> FirstParser { get; }
        public IParser<R> SecondParser { get; }
        public RuleCombining Combining => RuleCombining.OR;
        public IParserResult<R> Parse(ParserInput input) {
            var result1 = this.FirstParser.Parse(input);
            if (result1.IsSuccessful) {
                return result1;
            } else {
                return this.SecondParser.Parse(input);
            }
        }
    }

    public class ZeroOrMoreParser<R> : IParser<ImmutableList<R>> {
        public ZeroOrMoreParser(IParser<R> parser) {
            this.Parser = parser;
        }

        public RuleCombining Combining => RuleCombining.NONE;

        public IParser<R> Parser { get; }

        public IParserResult<ImmutableList<R>> Parse(ParserInput input) {
            var list = ImmutableList<R>.Empty;
            IParserResult<R> curResult;
            while ((curResult = this.Parser.Parse(input)).IsSuccessful) {
                list = list.Add(curResult.Result);
                input = curResult.ToInput();
            }
            return ParserSucceeded.Create(list, input.Environment, input.Source);
        }
    }

    public class OneOrMoreParser<R> : IParser<ImmutableList<R>> {
        public OneOrMoreParser(IParser<R> parser) {
            this.Parser = parser;
        }

        public RuleCombining Combining => RuleCombining.NONE;
        public IParser<R> Parser { get; }

        public IParserResult<ImmutableList<R>> Parse(ParserInput input) {
            var list = ImmutableList<R>.Empty;
            var curResult = this.Parser.Parse(input);
            if (!curResult.IsSuccessful) {
                return new ParserFailed<ImmutableList<R>>();
            }

            IParserResult<R> lastSuccessfulResult;
            do {
                list = list.Add(curResult.Result);
                lastSuccessfulResult = curResult;
                curResult = this.Parser.Parse(lastSuccessfulResult.ToInput());
            } while (curResult.IsSuccessful);

            return ParserSucceeded.Create(list, lastSuccessfulResult.Environment, lastSuccessfulResult.Source);
        }
    }

    public class OneOrMoreParserWithSeparator<R> : IParser<ImmutableList<R>> {
        public OneOrMoreParserWithSeparator(IConsumer separatorConsumer, IParser<R> elementParser) {
            this.SeparatorConsumer = separatorConsumer;
            this.ElementParser = elementParser;
        }
        public RuleCombining Combining => RuleCombining.NONE;
        public IConsumer SeparatorConsumer { get; }
        public IParser<R> ElementParser { get; }

        public IParserResult<ImmutableList<R>> Parse(ParserInput input) {
            var list = ImmutableList<R>.Empty;
            var curResult = this.ElementParser.Parse(input);
            if (!curResult.IsSuccessful) {
                return new ParserFailed<ImmutableList<R>>();
            }
            IParserResult<R> lastElementResult;

            do {
                list = list.Add(curResult.Result);
                lastElementResult = curResult;

                var separatorResult = this.SeparatorConsumer.Consume(curResult.ToInput());
                if (!separatorResult.IsSuccessful) {
                    break;
                }
                curResult = this.ElementParser.Parse(separatorResult.ToInput());
            } while (curResult.IsSuccessful);

            return ParserSucceeded.Create(list, lastElementResult.Environment, lastElementResult.Source);
        }
    }

    /// <summary>
    /// A consumer consumes one or several tokens, and doesn't produce any result.
    /// </summary>
    public interface IConsumer {
        IParserResult Consume(ParserInput input);
    }

    public class NamedConsumer : IConsumer {
        public NamedConsumer() {
            this.Consumer = new SetOnce<IConsumer>();
        }

        public SetOnce<IConsumer> Consumer { get; }

        public void Is(IConsumer consumer) {
            this.Consumer.Value = consumer;
        }

        public IParserResult Consume(ParserInput input) =>
            this.Consumer.Value.Consume(input);

        public override String ToString() {
            if (this.Consumer.IsSet) {
                return this.Consumer.Value.ToString();
            } else {
                return "<Unset Consumer>";
            }
        }
    }

    public class OptionalConsumer : IParser<Boolean> {
        public OptionalConsumer(IConsumer consumer) {
            this.Consumer = consumer;
        }

        public RuleCombining Combining => RuleCombining.NONE;

        public IConsumer Consumer { get; }

        public IParserResult<Boolean> Parse(ParserInput input) {
            var result = this.Consumer.Consume(input);
            if (result.IsSuccessful) {
                return ParserSucceeded.Create(true, result.Environment, result.Source);
            }
            return ParserSucceeded.Create(false, input.Environment, input.Source);
        }
    }

    public class ConsumerOrConsumer : IConsumer {
        public ConsumerOrConsumer(IConsumer firstConsumer, IConsumer secondConsumer) {
            this.FirstConsumer = firstConsumer;
            this.SecondConsumer = secondConsumer;
        }

        public IConsumer FirstConsumer { get; }
        public IConsumer SecondConsumer { get; }

        public IParserResult Consume(ParserInput input) {
            var result1 = this.FirstConsumer.Consume(input);
            if (!result1.IsSuccessful) {
                return new ParserFailed();
            }
            return this.SecondConsumer.Consume(result1.ToInput());
        }
    }

    public class EnvironmentTransformer : IConsumer {
        public EnvironmentTransformer(Func<ParserEnvironment, ParserEnvironment> transformer) {
            this.Transformer = transformer;
        }

        public Func<ParserEnvironment, ParserEnvironment> Transformer { get; }

        public IParserResult Consume(ParserInput input) {
            return ParserSucceeded.Create(this.Transformer(input.Environment), input.Source);
        }
    }

    /// <summary>
    /// A transformer consumes zero or more tokens, and takes a previous result to produce a new result.
    /// </summary>
    public interface ITransformer<in S, out R> {
        IParserResult<R> Transform(S seed, ParserInput input);
    }

    public class IdentityTransformer<R> : ITransformer<R, R> {
        public IParserResult<R> Transform(R seed, ParserInput input) =>
            ParserSucceeded.Create(seed, input.Environment, input.Source);
    }

    public class SimpleTransformer<S, R> : ITransformer<S, R> {
        public SimpleTransformer(Func<S, R> transformFunc) {
            this.TransformFunc = transformFunc;
        }
        public Func<S, R> TransformFunc { get; }
        public IParserResult<R> Transform(S seed, ParserInput input) =>
            ParserSucceeded.Create(this.TransformFunc(seed), input.Environment, input.Source);
    }

    public class NamedTransformer<S, R> : ITransformer<S, R> {
        public NamedTransformer() {
            this.Transformer = new SetOnce<ITransformer<S, R>>();
        }

        public SetOnce<ITransformer<S, R>> Transformer { get; }

        public void Is(ITransformer<S, R> transformer) {
            this.Transformer.Value = transformer;
        }

        public IParserResult<R> Transform(S seed, ParserInput input) =>
            this.Transformer.Value.Transform(seed, input);

        public override String ToString() {
            if (this.Transformer.IsSet) {
                return this.Transformer.Value.ToString();
            } else {
                return "<Unset transformer>";
            }
        }
    }

    public class OptionalTransformer<R> : ITransformer<R, R> {
        public OptionalTransformer(ITransformer<R, R> transformer) {
            this.Transformer = transformer;
        }
        public ITransformer<R, R> Transformer { get; }
        public IParserResult<R> Transform(R seed, ParserInput input) {
            var result = this.Transformer.Transform(seed, input);
            if (result.IsSuccessful) {
                return result;
            } else {
                return ParserSucceeded.Create(seed, input.Environment, input.Source);
            }
        }
    }

    public class TransformerOrTransformer<S, R> : ITransformer<S, R> {
        public TransformerOrTransformer(ITransformer<S, R> firstTransformer, ITransformer<S, R> secondTransformer) {
            this.FirstTransformer = firstTransformer;
            this.SecondTransformer = secondTransformer;
        }

        public ITransformer<S, R> FirstTransformer { get; }
        public ITransformer<S, R> SecondTransformer { get; }

        public IParserResult<R> Transform(S seed, ParserInput input) {
            var result1 = this.FirstTransformer.Transform(seed, input);
            if (result1.IsSuccessful) {
                return result1;
            }
            return this.SecondTransformer.Transform(seed, input);

        }

        public override String ToString() {
            return this.FirstTransformer + " | " + this.SecondTransformer;
        }
    }

    public class ResultTransformer<R> : ITransformer<R, R> {
        public ResultTransformer(Func<IParserResult<R>, IParserResult<R>> transformFunc) {
            this.TransformFunc = transformFunc;
        }

        public Func<IParserResult<R>, IParserResult<R>> TransformFunc { get; }

        public IParserResult<R> Transform(R seed, ParserInput input) =>
            this.TransformFunc(ParserSucceeded.Create(seed, input.Environment, input.Source));
    }

    public class ZeroOrMoreTransformer<R> : ITransformer<R, R> {
        public ZeroOrMoreTransformer(ITransformer<R, R> transformer) {
            this.Transformer = transformer;
        }
        public ITransformer<R, R> Transformer { get; }
        public IParserResult<R> Transform(R seed, ParserInput input) {
            IParserResult<R> curResult = ParserSucceeded.Create(seed, input.Environment, input.Source);

            IParserResult<R> lastSuccessfulResult;
            do {
                lastSuccessfulResult = curResult;
                curResult = this.Transformer.Transform(lastSuccessfulResult.Result, lastSuccessfulResult.ToInput());
            } while (curResult.IsSuccessful);

            return lastSuccessfulResult;
        }
    }

    public class OneOrMoreTransformer<R> : ITransformer<R, R> {
        public OneOrMoreTransformer(ITransformer<R, R> transformer) {
            this.Transformer = transformer;
        }
        public ITransformer<R, R> Transformer { get; }
        public IParserResult<R> Transform(R seed, ParserInput input) {
            var curResult = this.Transformer.Transform(seed, input);
            if (!curResult.IsSuccessful) {
                return new ParserFailed<R>();
            }

            IParserResult<R> lastSuccessfulResult;
            do {
                lastSuccessfulResult = curResult;
                curResult = this.Transformer.Transform(lastSuccessfulResult.Result, lastSuccessfulResult.ToInput());
            } while (curResult.IsSuccessful);

            return lastSuccessfulResult;
        }
    }

    public class OperatorConsumer : IConsumer {
        public OperatorConsumer(OperatorVal operatorVal) {
            this.OperatorVal = operatorVal;
        }

        public static IConsumer Create(OperatorVal operatorVal) =>
            new OperatorConsumer(operatorVal);

        public OperatorVal OperatorVal { get; }

        public IParserResult Consume(ParserInput input) {
            if ((input.Source.First() as TokenOperator)?.Val == this.OperatorVal) {
                return ParserSucceeded.Create(input.Environment, input.Source.Skip(1));
            } else {
                return new ParserFailed();
            }
        }
    }

    public class IdentifierParser : IParser<String> {
        public RuleCombining Combining => RuleCombining.NONE;
        public IParserResult<String> Parse(ParserInput input) {
            var token = input.Source.First() as TokenIdentifier;
            if (token == null) {
                return new ParserFailed<String>();
            }
            return ParserSucceeded.Create(token.Val, input.Environment, input.Source.Skip(1));
        }
    }

    public class KeywordConsumer : IConsumer {
        public KeywordConsumer(KeywordVal keywordVal) {
            this.KeywordVal = keywordVal;
        }
        public KeywordVal KeywordVal { get; }
        public static KeywordConsumer Create(KeywordVal keywordVal) =>
            new KeywordConsumer(keywordVal);
        public IParserResult Consume(ParserInput input) {
            if ((input.Source.First() as TokenKeyword)?.Val == this.KeywordVal) {
                return ParserSucceeded.Create(input.Environment, input.Source.Skip(1));
            } else {
                return new ParserFailed();
            }
        }
    }

    public class KeywordParser<R> : IParser<R> {
        public KeywordParser(KeywordVal keywordVal, R result) {
            this.KeywordVal = keywordVal;
            this.Result = result;
        }

        public RuleCombining Combining => RuleCombining.NONE;

        public KeywordVal KeywordVal { get; }
        public R Result { get; }

        public IParserResult<R> Parse(ParserInput input) {
            if ((input.Source.First() as TokenKeyword)?.Val == this.KeywordVal) {
                return ParserSucceeded.Create(this.Result, input.Environment, input.Source.Skip(1));
            } else {
                return new ParserFailed<R>();
            }
        }
    }

    public class KeywordParser {
        public static KeywordParser<R> Create<R>(KeywordVal keywordVal, R result) =>
            new KeywordParser<R>(keywordVal, result);
    }

}