using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using ABT2.TypeSystem;

namespace ABT2.Initialization {
    using IQualExprType = IQualExprType<IExprType>;

    public static class InitializationUtils {
        
        public sealed class TypeAndInittialzer {
            public TypeAndInittialzer(IQualExprType qualType, ImmutableList<InitializerExprEntry> entries) {
                this.QualType = qualType;
                this.Entries = entries;
            }

            public IQualExprType QualType { get; }

            public ImmutableList<InitializerExprEntry> Entries { get; }
        }

        public static TypeAndInittialzer MatchInitializer(IQualExprType qualType, Initializer initializer) {
            var visitor = new InitializerVisitor(initializer);
            qualType.Type.Visit(visitor);
            var entries = visitor.Entries;
            return new TypeAndInittialzer(qualType, entries.ToImmutable());
        }

        public static void Match(
            Int64 offset,
            ImmutableList<InitializerExprEntry>.Builder builder,
            IEnumerator<Initializer> initializers,
            IEnumerator<IQualExprType> types
        ) {
            if (!types.MoveNext()) {
                // No types!
                return;
            }


        }
    }
}
