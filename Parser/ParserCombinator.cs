using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing {
    public static class ParserCombinator {

        /// <summary>
        /// Create an optional parser.
        /// </summary>
        public static IParser<Option<R>> Optional<R>(this IParser<R> parser) =>
            new OptionalParser<R>(parser);

        public static IParser<R> Optional<R>(this IParser<R> parser, R defaultValue) =>
            new OptionalParserWithDefault<R>(parser, defaultValue);

        /// <summary>
        /// ( => R1 ) then ( => R2 ) is ( => Tuple[R2, R1] )
        /// </summary>
        public static IParser<Tuple<R2, R1>> Then<R1, R2>(this IParser<R1> firstParser, IParser<R2> secondParser) =>
            new ParserThenParser<R1, R2>(firstParser, secondParser);

        /// <summary>
        /// ( => R ) then ( => ) is ( => R )
        /// </summary>
        public static IParser<R> Then<R>(this IParser<R> parser, IConsumer consumer) =>
            new ParserThenConsumer<R>(parser, consumer);

        /// <summary>
        /// ( => ) then ( => R ) is ( => R )
        /// </summary>
        public static IParser<R> Then<R>(this IConsumer consumer, IParser<R> parser) =>
            new ConsumerThenParser<R>(consumer, parser);

        /// <summary>
        /// ( => S ) then ( S => R ) is ( => R )
        /// </summary>
        public static IParser<R> Then<S, R>(this IParser<S> parser, ITransformer<S, R> transformer) =>
            new ParserThenTransformer<S, R>(parser, transformer);

        /// <summary>
        /// ( => S ) then ( S => R ) is ( => R )
        /// </summary>
        public static IParser<R> Then<S, R>(this IParser<S> parser, Func<S, R> transformFunc) =>
            Then(parser, Transformer.Create(transformFunc));

        public static IParser<R> Then<S1, S2, S3, R>(this IParser<Tuple<S3, Tuple<S2, S1>>> parser, Func<S1, S2, S3, R> transformFunc) =>
            Then(parser, (Tuple<S3, Tuple<S2, S1>> _) => transformFunc(_.Item2.Item2, _.Item2.Item1, _.Item1));

        /// <summary>
        /// ( => R ) check Predicate[IParserResult[R]] is ( => R)
        /// </summary>
        public static IParser<R> Check<R>(this IParser<R> parser, Predicate<IParserResult<R>> predicate) =>
            new ParserThenCheck<R>(parser, predicate);
        
        /// <summary>
        /// ( => ) then ( => ) is ( => )
        /// </summary>
        public static IConsumer Then(this IConsumer firstConsumer, IConsumer secondConsumer) =>
            new ConsumerThenConsumer(firstConsumer, secondConsumer);

        /// <summary>
        /// ( => I ) then ( Tuple[I, S] => R ) is ( S => R )
        /// </summary>
        public static ITransformer<S, R> Then<S, I, R>(this IParser<I> parser, ITransformer<Tuple<I, S>, R> transformer) =>
            new ParserThenTransformer<S, I, R>(parser, transformer);

        /// <summary>
        /// ( => I ) then ( Tuple[I, S] => R ) is ( S => R )
        /// </summary>
        public static ITransformer<S, R> Then<S, I, R>(this IParser<I> parser, Func<Tuple<I, S>, R> transformFunc) =>
            Then(parser, Transformer.Create(transformFunc));

        public delegate R TransformFunc<S, I, R>(S seed, I intermediate);

        public static ITransformer<S, R> Then<S, I, R>(this IParser<I> parser, Func<S, I, R> transformFunc) =>
            Then(parser, Transformer.Create((Tuple<I, S> _) => transformFunc(_.Item2, _.Item1)));

        public static ITransformer<R, R> Optional<R>(this ITransformer<R, R> transformer) =>
            new OptionalTransformer<R>(transformer);

        public static ITransformer<S, R> Or<S, R>(this ITransformer<S, R> firstTransformer, ITransformer<S, R> secondTransformer) =>
            new TransformerOrTransformer<S, R>(firstTransformer, secondTransformer);

        public static IParser<R> Or<R>(this IParser<R> firstParser, IParser<R> secondParser) =>
            new ParserOrParser<R>(firstParser, secondParser);

        public static IParser<Boolean> Optional(this IConsumer consumer) =>
            new OptionalConsumer(consumer);

        public static ITransformer<R, R> Then<R>(this IConsumer consumer, ITransformer<R, R> transformer) =>
            new ConsumerThenTransformer<R>(consumer, transformer);

        public static ITransformer<R, R> Then<R>(this IConsumer consumer, Func<R, R> transformFunc) =>
            new ConsumerThenTransformer<R>(consumer, Transformer.Create(transformFunc));

        public static ITransformer<R, R> ZeroOrMore<R>(this ITransformer<R, R> transformer) =>
            new ZeroOrMoreTransformer<R>(transformer);

        public static ITransformer<R, R> OneOrMore<R>(this ITransformer<R, R> transformer) =>
            new OneOrMoreTransformer<R>(transformer);

        public static IParser<ImmutableList<R>> OneOrMore<R>(this IParser<R> parser) =>
            new OneOrMoreParser<R>(parser);

        public static IParser<ImmutableList<R>> OneOrMore<R>(this IParser<R> elementParser, IConsumer separatorConsumer) =>
            new OneOrMoreParserWithSeparator<R>(separatorConsumer, elementParser);

        public static ITransformer<R, R> Given<R>() =>
            new IdentityTransformer<R>();
    }
}
