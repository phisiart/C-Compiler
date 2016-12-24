using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using ABT2.TypeSystem;
using ABT2.Expressions;

namespace ABT2.Initialization {
    using IRValueExpr = IRValueExpr<IExprType>;
    using IQualExprType = IQualExprType<IExprType>;
    using static TypeSystemUtils;

    public abstract class Initializer { }

    public sealed class InitializerExpr : Initializer {
        public InitializerExpr(IRValueExpr expr) {
            this.Expr = expr;
        }

        public IRValueExpr Expr { get; }
    }

    public sealed class InitializerList : Initializer {
        public InitializerList(ImmutableList<Initializer> initializers) {
            this.Initializers = initializers;
        }

        public ImmutableList<Initializer> Initializers { get; }
    }

    public sealed class InitializerExprEntry {
        public InitializerExprEntry(Int64 offset, IRValueExpr expr) {
            this.Offset = offset;
            this.Expr = expr;
        }

        Int64 Offset { get; }
        IRValueExpr Expr { get; }
    }

    public class InitializerVisitor : IExprTypeVisitor {
        public InitializerVisitor(Initializer initializer)
            : this(ImmutableList.Create(initializer)) { }

        public InitializerVisitor(ImmutableList<Initializer> initializers) {
            if (initializers.IsEmpty) {
                throw new InvalidOperationException($"{nameof(initializers)} cannot be empty");
            }

            this.Offset = 0;

            this.Initializers = new Stack<IEnumerator<Initializer>>();
            this.Initializers.Push(initializers.GetEnumerator());
            this.Initializers.Peek().MoveNext();

            this.Entries = ImmutableList.CreateBuilder<InitializerExprEntry>();
        }

        /// <summary>
        /// Match arithmetic type.
        /// </summary>
        /// <param name="type">Type.</param>
        public void VisitArithmeticType(IArithmeticType type) {
            Initializer initializer = this.Initializers.Peek().Current;
            if (initializer is InitializerExpr) {
                var expr = ((InitializerExpr)initializer).Expr;

                // TODO: change this to type cast

                if (!TypesAreEqual(expr.Type, type)) {
                    // unsuccessful match
                    throw new InvalidProgramException($"type mismatch: want {type}, but have {expr.Type}");
                }

                // successful match
                this.Entries.Add(new InitializerExprEntry(this.Offset, expr));
                this.Succeeded = true;
                return;

            } else {
                var initializers = ((InitializerList)initializer).Initializers;

                // Get one level deeper.
                this.Initializers.Push(initializers.GetEnumerator());

                // There should be at least one element.
                if (!this.Initializers.Peek().MoveNext()) {
                    throw new InvalidProgramException("scalar initializer cannot be empty");
                }

                this.VisitArithmeticType(type);

                // Get one level shallower.
                this.Initializers.Pop();

                this.Succeeded = true;
                return;
            }
        }

        // Every arithmetic type goes to the same routine.

        public void VisitSignedChar(TSChar type) {
            VisitArithmeticType(type);
        }

        public void VisitUnsignedChar(TUChar type) {
            VisitArithmeticType(type);
        }

        public void VisitSignedShort(TSShort type) {
            VisitArithmeticType(type);
        }

        public void VisitUnsignedShort(TUShort type) {
            VisitArithmeticType(type);
        }

        public void VisitSignedInt(TSInt type) {
            VisitArithmeticType(type);
        }

        public void VisitUnsignedInt(TUInt type) {
            VisitArithmeticType(type);
        }

        public void VisitSignedLong(TSLong type) {
            VisitArithmeticType(type);
        }

        public void VisitUnsignedLong(TULong type) {
            VisitArithmeticType(type);
        }

        public void VisitFloat(TFloat type) {
            VisitArithmeticType(type);
        }

        public void VisitDouble(TDouble type) {
            VisitArithmeticType(type);
        }

        public void VisitPointer(TPointer type) {
            // TODO: `char *` is different.

        }

        public void VisitStruct(StructOrUnionType type) {
            // TODO: implement this.
            //{
            //    Initializer initializer = this.Initializers.Peek().Current;
            //    if (initializer is InitializerExpr) {
            //        if (type.Members.IsEmpty) {
            //            throw new InvalidProgramException("initializer for aggregate with no elements requires explicit braces");
            //        }

            //        do {
            //            var expr = ((InitializerExpr)initializer).Expr;

            //        } while (!this.Initializers.Peek().MoveNext());

            //    } else {
            //        // The initializer has the following form:
            //        // `{ ... }`
            //        // Now we just match each element in the list with struct members.

            //        var initializers = ((InitializerList)initializer).Initializers;

            //        // Go one level deeper.
            //        this.Initializers.Push(initializers.GetEnumerator());

            //        Int64 startOffset = this.Offset;

            //        foreach (var member in type.Members) {
            //            this.Offset = startOffset + member.Offset;

            //            if (!this.Initializers.Peek().MoveNext()) {
            //                break;
            //            }

            //            member.QualType.Type.Visit(this);
            //        }

            //        // Go one level shallower.
            //        this.Initializers.Pop();

            //        this.Succeeded = true;
            //    }
            //}
        }

        public void VisitStructOrUnion(StructOrUnionType type) {
            // TODO: implement this

        }

        public void VisitFunction(FunctionType type) {
            // TODO: implement this
        }

        public void VisitArray(ArrayType type) {
            // TODO: `char [n]` is different.
        }

        public void VisitIncompleteArray(IncompleteArrayType type) {
            // TODO: `char []` is different.
        }

        public Boolean Succeeded;
        public Int64 Offset;
        public Stack<IEnumerator<Initializer>> Initializers;
        public ImmutableList<InitializerExprEntry>.Builder Entries;
    }
}
