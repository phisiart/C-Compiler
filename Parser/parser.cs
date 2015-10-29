using SyntaxTree;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parsing.ParserCombinator;

namespace Parsing {

    // Chains
    // ======
    public class ParserThenParser<R1, R2> : IParser<Tuple<R2, R1>> {
        public ParserThenParser(IParser<R1> FirstParser, IParser<R2> SecondParser) {
            this.FirstParser = FirstParser;
            this.SecondParser = SecondParser;
        }

        public IParser<R1> FirstParser { get; }
        public IParser<R2> SecondParser { get; }
        public RuleCombining Combining => RuleCombining.Then;

        public IParserResult<Tuple<R2, R1>> Parse(ParserInput input) {
            var result1 = this.FirstParser.Parse(input);
            if (!result1.IsSuccessful) {
                return new ParserFailed<Tuple<R2, R1>>();
            }
            var result2 = this.SecondParser.Parse(result1.ToInput());
            if (!result2.IsSuccessful) {
                return new ParserFailed<Tuple<R2, R1>>();
            }
            return ParserSucceeded.Create(Tuple.Create(result2.Result, result1.Result), result2.Environment, result2.Source);
        }
    }

    public class ParserThenConsumer<R> : IParser<R> {
        public ParserThenConsumer(IParser<R> Parser, IConsumer Consumer) {
            this.Parser = Parser;
            this.Consumer = Consumer;
        }

        public IParser<R> Parser { get; }
        public IConsumer Consumer { get; }
        public RuleCombining Combining => RuleCombining.Then;

        public IParserResult<R> Parse(ParserInput input) {
            var result1 = this.Parser.Parse(input);
            if (!result1.IsSuccessful) {
                return new ParserFailed<R>();
            }
            var result2 = this.Consumer.Consume(result1.ToInput());
            if (!result2.IsSuccessful) {
                return new ParserFailed<R>();
            }
            return ParserSucceeded.Create(result1.Result, result2.Environment, result2.Source);
        }
    }

    public class ParserThenTransformer<R1, R2> : IParser<R2> {
        public ParserThenTransformer(IParser<R1> parser, ITransformer<R1, R2> transformer) {
            this.Parser = parser;
            this.Transformer = transformer;
        }

        public IParser<R1> Parser { get; }
        public ITransformer<R1, R2> Transformer { get; }
        public RuleCombining Combining => RuleCombining.Then;

        public IParserResult<R2> Parse(ParserInput input) {
            var result1 = this.Parser.Parse(input);
            if (!result1.IsSuccessful) {
                return new ParserFailed<R2>();
            }
            return this.Transformer.Transform(result1.Result, result1.ToInput());
        }
    }

    public class ConsumerThenParser<R> : IParser<R> {
        public ConsumerThenParser(IConsumer consumer, IParser<R> parser) {
            this.Consumer = consumer;
            this.Parser = parser;
        }

        public IConsumer Consumer { get; }
        public IParser<R> Parser { get; }
        public RuleCombining Combining => RuleCombining.Then;

        public IParserResult<R> Parse(ParserInput input) {
            var result1 = this.Consumer.Consume(input);
            if (!result1.IsSuccessful) {
                return new ParserFailed<R>();
            }
            return this.Parser.Parse(result1.ToInput());
        }
    }

    public class ConsumerThenConsumer : IConsumer {
        public ConsumerThenConsumer(IConsumer FirstConsumer, IConsumer SecondConsumer) {
            this.FirstConsumer = FirstConsumer;
            this.SecondConsumer = SecondConsumer;
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
            if (!result1.IsSuccessful) {
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
        None,
        Then,
        Or
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
        
        public void Is(IParser<R> Parser) {
            this.Parser.Value = Parser;
        }

        public IParserResult<R> Parse(ParserInput input) =>
            this.Parser.Value.Parse(input);

        public String Name { get; }
        public String Rule => this.Parser.Value.ToString();
        public RuleCombining Combining => RuleCombining.None;
        public override String ToString() => this.Name;
    }

    public class OptionalParser<R> : IParser<Option<R>> {
        public OptionalParser(IParser<R> Parser) {
            this.Parser = Parser;
        }
        public IParser<R> Parser { get; }
        public RuleCombining Combining => RuleCombining.None;
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
        public RuleCombining Combining => RuleCombining.None;
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
        public RuleCombining Combining => RuleCombining.None;
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
        public RuleCombining Combining => RuleCombining.None;
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
        public ParserOrParser(IParser<R> FirstParser, IParser<R> SecondParser) {
            this.FirstParser = FirstParser;
            this.SecondParser = SecondParser;
        }
        public IParser<R> FirstParser { get; }
        public IParser<R> SecondParser { get; }
        public RuleCombining Combining => RuleCombining.Or;
        public IParserResult<R> Parse(ParserInput input) {
            var result1 = FirstParser.Parse(input);
            if (result1.IsSuccessful) {
                return result1;
            } else {
                return SecondParser.Parse(input);
            }
        }
    }

    public class ZeroOrMoreParser<R> : IParser<ImmutableList<R>> {
        public ZeroOrMoreParser(IParser<R> parser) {
            this.Parser = parser;
        }

        public RuleCombining Combining => RuleCombining.None;

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

        public RuleCombining Combining => RuleCombining.None;
        public IParser<R> Parser { get; }

        public IParserResult<ImmutableList<R>> Parse(ParserInput input) {
            var list = ImmutableList<R>.Empty;
            var curResult = this.Parser.Parse(input);
            if (!curResult.IsSuccessful) {
                return new ParserFailed<ImmutableList<R>>();
            }
            var lastSuccessfulResult = curResult;

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
        public RuleCombining Combining => RuleCombining.None;
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

    public class SetOnceConsumer : IConsumer {
        public SetOnceConsumer() {
            this.Consumer = new SetOnce<IConsumer>();
        }

        public SetOnce<IConsumer> Consumer { get; }

        public void Is(IConsumer Consumer) {
            this.Consumer.Value = Consumer;
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
        public OptionalConsumer(IConsumer Consumer) {
            this.Consumer = Consumer;
        }

        public RuleCombining Combining => RuleCombining.None;

        public IConsumer Consumer { get; }
        public IParserResult<Boolean> Parse(ParserInput input) {
            var result = this.Consumer.Consume(input);
            if (result.IsSuccessful) {
                return ParserSucceeded.Create(true, input.Environment, input.Source);
            } else {
                return ParserSucceeded.Create(false, input.Environment, input.Source);
            }
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

    /// <summary>
    /// A transformer consumes zero or more tokens, and takes a previous result to produce a new result.
    /// </summary>
    public interface ITransformer<in S, out R> {
        IParserResult<R> Transform(S seed, ParserInput input);
    }

    public static class Transformer {
        public static SimpleTransformer<S, R> Create<S, R>(Func<S, R> transformFunc) =>
            new SimpleTransformer<S, R>(transformFunc);

        public static SetOnceTransformer<S, R> Create<S, R>() =>
            new SetOnceTransformer<S, R>();
        
    }

    public class IdentityTransformer<R> : ITransformer<R, R> {
        public IParserResult<R> Transform(R seed, ParserInput input) =>
            ParserSucceeded.Create(seed, input.Environment, input.Source);
    }

    public class AlwaysFailingTransformer<S, R> : ITransformer<S, R> {
        public IParserResult<R> Transform(S seed, ParserInput input) =>
            new ParserFailed<R>();
    }

    public class SimpleTransformer<S, R> : ITransformer<S, R> {
        public SimpleTransformer(Func<S, R> transformFunc) {
            this.TransformFunc = transformFunc;
        }
        public Func<S, R> TransformFunc { get; }
        public IParserResult<R> Transform(S seed, ParserInput input) =>
            ParserSucceeded.Create(this.TransformFunc(seed), input.Environment, input.Source);
    }

    public class SetOnceTransformer<S, R> : ITransformer<S, R> {
        public SetOnceTransformer() {
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
            var result = Transformer.Transform(seed, input);
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
            } else {
                return this.SecondTransformer.Transform(seed, input);
            }
        }
        public override String ToString() {
            return this.FirstTransformer.ToString() + " | " + this.SecondTransformer.ToString();
        }
    }

    //public class ParserThenTransformer<S, I, R> : ITransformer<S, R> {
    //    public ParserThenTransformer(IParser<I> parser, ITransformer<Tuple<I, S>, R> transformer) {
    //        this.Parser = parser;
    //        this.Transformer = transformer;
    //    }
    //    public IParser<I> Parser { get; }
    //    public ITransformer<Tuple<I, S>, R> Transformer { get; }
    //    public IParserResult<R> Transform(S seed, ParserInput input) {
    //        var intermediateResult = this.Parser.Parse(input);
    //        if (!intermediateResult.IsSuccessful) {
    //            return new ParserFailed<R>();
    //        }
    //        return this.Transformer.Transform(Tuple.Create(intermediateResult.Result, seed), intermediateResult.ToInput());
    //    }
    //}

    //public class ConsumerThenTransformer<R> : ITransformer<R, R> {
    //    public ConsumerThenTransformer(IConsumer consumer, ITransformer<R, R> transformer) {
    //        this.Consumer = consumer;
    //        this.Transformer = transformer;
    //    }
    //    public IConsumer Consumer { get; }
    //    public ITransformer<R, R> Transformer { get; }
    //    public IParserResult<R> Transform(R seed, ParserInput input) {
    //        var result1 = this.Consumer.Consume(input);
    //        if (!result1.IsSuccessful) {
    //            return new ParserFailed<R>();
    //        }
    //        return this.Transformer.Transform(seed, result1.ToInput());
    //    }
    //}

    public class ZeroOrMoreTransformer<R> : ITransformer<R, R> {
        public ZeroOrMoreTransformer(ITransformer<R, R> transformer) {
            this.Transformer = transformer;
        }
        public ITransformer<R, R> Transformer { get; }
        public IParserResult<R> Transform(R seed, ParserInput input) {
            IParserResult<R> curResult = ParserSucceeded.Create(seed, input.Environment, input.Source);
            var lastSuccessfulResult = curResult;

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
            var lastSuccessfulResult = curResult;

            do {
                lastSuccessfulResult = curResult;
                curResult = this.Transformer.Transform(lastSuccessfulResult.Result, lastSuccessfulResult.ToInput());
            } while (curResult.IsSuccessful);

            return lastSuccessfulResult;
        }
    }

    public partial class CParser {

        static CParser() {
            SetExpressionRules();
        }

        public class OperatorConsumer : IConsumer {
            public OperatorConsumer(OperatorVal operatorVal) {
                this.OperatorVal = operatorVal;
            }

            public static IConsumer Create(OperatorVal operatorVal) =>
                new OperatorConsumer(operatorVal);

            public OperatorVal OperatorVal { get; }

            public IParserResult Consume(ParserInput input) {
                if ((input.Source.First() as TokenOperator)?.val == OperatorVal) {
                    return ParserSucceeded.Create(input.Environment, input.Source.Skip(1));
                } else {
                    return new ParserFailed();
                }
            }
        }

        public class IdentifierParser : IParser<String> {
            public RuleCombining Combining => RuleCombining.None;
            public IParserResult<String> Parse(ParserInput input) {
                if (input.Source.First() is TokenIdentifier) {
                    return ParserSucceeded.Create((input.Source.First() as TokenIdentifier).val, input.Environment, input.Source.Skip(1));
                } else {
                    return new ParserFailed<String>();
                }
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
                if ((input.Source.First() as TokenKeyword)?.val == KeywordVal) {
                    return ParserSucceeded.Create(input.Environment, input.Source.Skip(1));
                } else {
                    return new ParserFailed();
                }
            }
        }

        public class KeywordParser<R> : IParser<R> {
            public KeywordParser(KeywordVal keywordVal, R result) {
                this.KeywordVal = KeywordVal;
                this.Result = result;
            }

            public RuleCombining Combining => RuleCombining.None;

            public KeywordVal KeywordVal { get; }
            public R Result { get; }

            public IParserResult<R> Parse(ParserInput input) {
                if ((input.Source.First() as TokenKeyword)?.val == KeywordVal) {
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

        public class ConstCharParser : IParser<Expr> {
            public RuleCombining Combining => RuleCombining.None;
            public IParserResult<Expr> Parse(ParserInput input) {
                if (input.Source.First() is TokenCharConst) {
                    var token = input.Source.First() as TokenCharConst;
                    return ParserSucceeded.Create(new ConstInt(token.value, TokenInt.Suffix.NONE), input.Environment, input.Source.Skip(1));
                } else {
                    return new ParserFailed<Expr>();
                }
            }
        }

        public class ConstIntParser : IParser<Expr> {
            public RuleCombining Combining => RuleCombining.None;
            public IParserResult<Expr> Parse(ParserInput input) {
                if (input.Source.First() is TokenInt) {
                    var token = input.Source.First() as TokenInt;
                    return ParserSucceeded.Create(new ConstInt(token.val, token.suffix), input.Environment, input.Source.Skip(1));
                } else {
                    return new ParserFailed<Expr>();
                }
            }
        }

        public class ConstFloatParser : IParser<Expr> {
            public RuleCombining Combining => RuleCombining.None;
            public IParserResult<Expr> Parse(ParserInput input) {
                if (input.Source.First() is TokenFloat) {
                    var token = input.Source.First() as TokenFloat;
                    return ParserSucceeded.Create(new ConstFloat(token.value, token.suffix), input.Environment, input.Source.Skip(1));
                } else {
                    return new ParserFailed<Expr>();
                }
            }
        }

        public class StringLiteralParser : IParser<Expr> {
            public RuleCombining Combining => RuleCombining.None;
            public IParserResult<Expr> Parse(ParserInput input) {
                if (input.Source.First() is TokenString) {
                    var token = input.Source.First() as TokenString;
                    return ParserSucceeded.Create(new StringLiteral(token.raw), input.Environment, input.Source.Skip(1));
                } else {
                    return new ParserFailed<Expr>();
                }
            }
        }

        public class BinaryOperatorBuilder {
            public BinaryOperatorBuilder(IConsumer operatorConsumer, Func<Expr, Expr, Expr> nodeCreator) {
                this.OperatorConsumer = operatorConsumer;
                this.NodeCreator = nodeCreator;
            }

            public static BinaryOperatorBuilder Create(IConsumer operatorConsumer, Func<Expr, Expr, Expr> nodeCreator) =>
                new BinaryOperatorBuilder(operatorConsumer, nodeCreator);

            public IConsumer OperatorConsumer { get; }
            public Func<Expr, Expr, Expr> NodeCreator { get; }
        }

        // TODO: create a dedicated class for this.
        public static IParser<Expr> BinaryOperator(IParser<Expr> operandParser, params BinaryOperatorBuilder[] builders) {
            var transformers = builders.Select(builder =>
                Given<Expr>()
                .Then(builder.OperatorConsumer)
                .Then(operandParser)
                .Then(builder.NodeCreator)
            );
            return operandParser.Then(transformers.Aggregate(ParserCombinator.Or).ZeroOrMore());
        }

        //public static IParser<Expr> BinaryOperator(
        //    IParser<Expr> operandParser,
        //    params Tuple<IConsumer, BinaryOp.Creator>[] operatorConsumerAndTransformers
        //) {
        //    var transformers = operatorConsumerAndTransformers.Select(_ =>
        //        (_.Item1).Then(operandParser).Then((Expr lhs, Expr rhs) => _.Item2(lhs, rhs))
        //    );
        //    return operandParser.Then(transformers.Aggregate(ParserCombinator.Or).ZeroOrMore());
        //}
        public static IParser<Expr> AssignmentOperator(
            IParser<Expr> lhsParser,
            IParser<Expr> rhsParser,
            params BinaryOperatorBuilder[] builders
        ) {
            var transformers = builders.Select(builder =>
                Given<Expr>()
                .Then(builder.OperatorConsumer)
                .Then(rhsParser)
                .Then(builder.NodeCreator)
            );
            return lhsParser.Then(transformers.Aggregate(ParserCombinator.Or).ZeroOrMore());

        }

        //public static IParser<Expr> AssignmentOperatorParser(
        //    IParser<Expr> lhsParser,
        //    IParser<Expr> rhsParser,
        //    params Tuple<IConsumer, Func<Expr, Expr, Expr>>[] operatorConsumerAndTransformers
        //) {
        //    var transformers = operatorConsumerAndTransformers.Select(_ =>
        //        (_.Item1).Then(rhsParser).Then((Expr lhs, Expr rhs) => _.Item2(lhs, rhs))
        //    );
        //    return lhsParser.Then(transformers.Aggregate(ParserCombinator.Or));
        //}
    }
}