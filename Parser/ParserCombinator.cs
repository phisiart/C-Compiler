using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing {

public static class ParserCombinator {

    public static ParsingFunction<Tuple<R2, R1>> then<R1, R2>(this ParsingFunction<R1> first, ParsingFunction<R2> second) => input => {
        var result1 = first(input);
        if (!result1.IsSuccessful) {
            return new ParserFailed<Tuple<R2, R1>>();
        }
        var result2 = second(result1.ToInput());
        if (!result2.IsSuccessful) {
            return new ParserFailed<Tuple<R2, R1>>();
        }

        return new ParserSucceeded<Tuple<R2, R1>>(Tuple.Create(result2.Result, result1.Result), result2.Environment, result2.Source);
    };

    public static ParsingFunction<R> then<R>(this ParsingFunction first, ParsingFunction<R> second) => input => {
        var result1 = first(input);
        if (!result1.IsSuccessful) {
            return new ParserFailed<R>();
        }
        return second(result1.ToInput());
    };

    public static ParsingFunction<R> then<R>(this ParsingFunction<R> first, ParsingFunction second) => input => {
        var result1 = first(input);
        if (!result1.IsSuccessful) {
            return new ParserFailed<R>();
        }
        var result2 = second(result1.ToInput());
        if (!result2.IsSuccessful) {
            return new ParserFailed<R>();
        }
        return new ParserSucceeded<R>(result1.Result, result2.Environment, result2.Source);
    };

    public static ParsingFunction then(this ParsingFunction first, ParsingFunction second) => input => {
        var result1 = first(input);
        if (!result1.IsSuccessful) {
            return result1;
        }
        return second(result1.ToInput());
    };

    public static ParsingFunction<To> transform<From, To>(this ParsingFunction<From> parsingFunction, Func<From, To> transformFunction) => input => {
        var result = parsingFunction(input);
        if (!result.IsSuccessful) {
            return new ParserFailed<To>();
        }
        return new ParserSucceeded<To>(transformFunction(result.Result), result.Environment, result.Source);
    };

    public static ParsingFunction<R> or<R>(this ParsingFunction<R> first, ParsingFunction<R> second) => input => {
        var result1 = first(input);
        if (result1.IsSuccessful) {
            return result1;
        }
        return second(input);
    };

    public static ParsingFunction<R> aggregate<R, M>(this ParsingFunction<R> ParseSeed, ParsingFunction<M> ParseMore, Func<R, M, R> Combine) => input => {
        var curResult = ParseSeed(input);
        if (!curResult.IsSuccessful) {
            return new ParserFailed<R>();
        }

        var lastSuccessfulResult = curResult;

        do {
            lastSuccessfulResult = curResult;
            input = lastSuccessfulResult.ToInput();
            curResult = ParseMore.transform(more => Combine(lastSuccessfulResult.Result, more))(input);
        } while (curResult.IsSuccessful);

        return lastSuccessfulResult;
    };
}
}