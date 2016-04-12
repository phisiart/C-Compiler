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

        public static IParserResult<TranslnUnit> Parse(IEnumerable<Token> tokens) =>
            TranslationUnit.Parse(new ParserInput(new ParserEnvironment(), tokens));

        public class ConstCharParser : IParser<Expr> {
            public RuleCombining Combining => RuleCombining.NONE;
            public IParserResult<Expr> Parse(ParserInput input) {
                var token = input.Source.First() as TokenCharConst;
                if (token == null) {
                    return new ParserFailed<Expr>();
                }
                return ParserSucceeded.Create(new IntLiteral(token.Value, TokenInt.IntSuffix.NONE), input.Environment, input.Source.Skip(1));
            }
        }

        public class ConstIntParser : IParser<Expr> {
            public RuleCombining Combining => RuleCombining.NONE;
            public IParserResult<Expr> Parse(ParserInput input) {
                var token = input.Source.First() as TokenInt;
                if (token == null) {
                    return new ParserFailed<Expr>();
                }
                return ParserSucceeded.Create(new IntLiteral(token.Val, token.Suffix), input.Environment, input.Source.Skip(1));
            }
        }

        public class ConstFloatParser : IParser<Expr> {
            public RuleCombining Combining => RuleCombining.NONE;
            public IParserResult<Expr> Parse(ParserInput input) {
                var token = input.Source.First() as TokenFloat;
                if (token == null) {
                    return new ParserFailed<Expr>();
                }
                return ParserSucceeded.Create(new FloatLiteral(token.Value, token.Suffix), input.Environment, input.Source.Skip(1));
            }
        }

        public class StringLiteralParser : IParser<Expr> {
            public RuleCombining Combining => RuleCombining.NONE;
            public IParserResult<Expr> Parse(ParserInput input) {
                var token = input.Source.First() as TokenString;
                if (token == null) {
                    return new ParserFailed<Expr>();
                }
                return ParserSucceeded.Create(new StringLiteral(token.Raw), input.Environment, input.Source.Skip(1));
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