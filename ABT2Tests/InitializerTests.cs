using NUnit.Framework;
using ABT2.TypeSystem;
using ABT2.Initialization;
using ABT2.Environment;
using static ABT2.TypeSystem.TypeSystemUtils;

namespace CompilerTests {
    [TestFixture]
    public class InitializerTests {
        [Test]
        public void TestInt() {
            var expr = new ABT2.Expressions.ConstSInt(10, Env.Empty);
            var initr = new ABT2.Initialization.InitializerExpr(expr);
            var typeAndInitializers = InitializationUtils.MatchInitializer(EmptyQual(TSInt.Get), initr);

            var qualType = typeAndInitializers.QualType;
            var initializers = typeAndInitializers.Entries;
        }
    }
}
