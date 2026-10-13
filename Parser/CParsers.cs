using System;
using System.Collections.Generic;
using System.Linq;
using LexicalAnalysis;
using AST;
using static Parsing.ParserCombinator;
using System.Collections.Immutable;

namespace Parsing {
    public partial class CParsers {
        static CParsers() {
            SetExpressionRules();
            SetDeclarationRules();
            SetExternalDefinitionRules();
            SetStatementRules();
        }

        /// <summary>
        /// Parses a whole translation unit.
        /// Throws a <see cref="CompilationError"/> pointing at the furthest token the parser
        /// could reach if the tokens do not form a valid translation unit.
        /// </summary>
        public static TranslnUnit Parse(IEnumerable<Token> tokens) {
            var source = new TokenStream(tokens);
            var result = TranslationUnit.Parse(new ParserInput(new ParserEnvironment(), source));
            if (!result.IsSuccessful) {
                throw source.SyntaxError();
            }
            if (!result.Source.IsAtEnd) {
                throw result.Source.SyntaxError();
            }
            return result.Result;
        }

        public class ConstCharParser : IParser<Expr> {
            public RuleCombining Combining => RuleCombining.NONE;
            public IParserResult<Expr> Parse(ParserInput input) {
                var token = input.Source.Current as TokenCharConst;
                if (token == null) {
                    input.Source.Expected("character constant");
                    return new ParserFailed<Expr>();
                }
                return ParserSucceeded.Create(new IntLiteral(token.Value, TokenInt.IntSuffix.NONE), input.Environment, input.Source.Advance());
            }
        }

        public class ConstIntParser : IParser<Expr> {
            public RuleCombining Combining => RuleCombining.NONE;
            public IParserResult<Expr> Parse(ParserInput input) {
                var token = input.Source.Current as TokenInt;
                if (token == null) {
                    input.Source.Expected("integer constant");
                    return new ParserFailed<Expr>();
                }
                return ParserSucceeded.Create(new IntLiteral(token.Val, token.Suffix), input.Environment, input.Source.Advance());
            }
        }

        public class ConstFloatParser : IParser<Expr> {
            public RuleCombining Combining => RuleCombining.NONE;
            public IParserResult<Expr> Parse(ParserInput input) {
                var token = input.Source.Current as TokenFloat;
                if (token == null) {
                    input.Source.Expected("floating constant");
                    return new ParserFailed<Expr>();
                }
                return ParserSucceeded.Create(new FloatLiteral(token.Value, token.Suffix), input.Environment, input.Source.Advance());
            }
        }

        public class StringLiteralParser : IParser<Expr> {
            public RuleCombining Combining => RuleCombining.NONE;
            public IParserResult<Expr> Parse(ParserInput input) {
                var token = input.Source.Current as TokenString;
                if (token == null) {
                    input.Source.Expected("string literal");
                    return new ParserFailed<Expr>();
                }
                return ParserSucceeded.Create(new StringLiteral(token.Raw), input.Environment, input.Source.Advance());
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
            ImmutableList<ITransformer<Expr, Expr>> transformers = builders.Select(builder =>
                Given<Expr>()
                .Then(builder.OperatorConsumer)
                .Then(operandParser)
                .Then(builder.NodeCreator)
            ).ToImmutableList();
            return operandParser.Then((new OrTransformer<Expr, Expr>(transformers)).ZeroOrMore());
        }

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
            ).ToImmutableList();
            return lhsParser.Then((new OrTransformer<Expr, Expr>(transformers)).OneOrMore());
        }
    }
}