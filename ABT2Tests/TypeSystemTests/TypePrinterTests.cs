using System;
using NUnit.Framework;
using ABT2.TypeSystem;
using static ABT2.TypeSystem.TypeSystemUtils;
using System.Collections.Immutable;
using ABT2.Environment;

namespace CompilerTests.ABT2Tests.TypeSystemTests {
    using IQualExprType = IQualExprType<IExprType>;

    [TestFixture]
    public static class TypePrinterTests {

        [Test]
        public static void TestSignedChar() {
            IQualExprType type = EmptyQual(TSChar.Get);
            String str = type.ToString();
            Assert.AreEqual(str, "char");

            type = Const(TSChar.Get);
            str = type.ToString();
            Assert.AreEqual(str, "const char");

            type = Volatile(TSChar.Get);
            str = type.ToString();
            Assert.AreEqual(str, "volatile char");

            type = ConstVolatile(TSChar.Get);
            str = type.ToString();
            Assert.AreEqual(str, "const volatile char");
        }

        [Test]
        public static void TestUnsignedChar() {
            var type = EmptyQual(TUChar.Get);
            var str = type.ToString();
            Assert.AreEqual(str, "unsigned char");

            type = Const(TUChar.Get);
            str = type.ToString();
            Assert.AreEqual(str, "const unsigned char");

            type = Volatile(TUChar.Get);
            str = type.ToString();
            Assert.AreEqual(str, "volatile unsigned char");

            type = ConstVolatile(TUChar.Get);
            str = type.ToString();
            Assert.AreEqual(str, "const volatile unsigned char");
        }

        [Test]
        public static void TestIncompleteArray() {
            var type = EmptyQual(new IncompleteArrayType(EmptyQual(TSChar.Get)));
            var str = type.ToString();
            Assert.AreEqual(str, "char []");

            type = EmptyQual(new IncompleteArrayType(Const(TSChar.Get)));
            str = type.ToString();
            Assert.AreEqual(str, "const char []");

            type = EmptyQual(new IncompleteArrayType(Volatile(TSChar.Get)));
            str = type.ToString();
            Assert.AreEqual(str, "volatile char []");

            type = EmptyQual(new IncompleteArrayType(ConstVolatile(TSChar.Get)));
            str = type.ToString();
            Assert.AreEqual(str, "const volatile char []");
        }

        [Test]
        public static void TestFunction() {
            var type = EmptyQual(
                FunctionType.Create(
                    Const(TSChar.Get),
                    ImmutableList.Create(
                        Const(TSChar.Get),
                        EmptyQual(TSChar.Get)
                    ),
                    hasVarArgs: true
                )
            );
            var str = type.ToString();
            Assert.AreEqual(str, "const char (const char, char, ...)");

            type = EmptyQual(
                FunctionType.Create(
                    EmptyQual(TSChar.Get),
                    ImmutableList<IQualExprType>.Empty,
                    hasVarArgs: true
                )
            );
            str = type.ToString();
            Assert.AreEqual(str, "char (...)");

            type = EmptyQual(
                FunctionType.Create(
                    EmptyQual(TSChar.Get),
                    ImmutableList<IQualExprType>.Empty,
                    hasVarArgs: false
                )
            );
            str = type.ToString();
            Assert.AreEqual(str, "char (void)");
        }

        [Test]
        public static void TestPointer() {
            var type = EmptyQual(
                new TPointer(
                    EmptyQual(TSChar.Get)
                )
            );
            var str = type.ToString();
            Assert.AreEqual(str, "char *");

            type = Const(
                new TPointer(
                    EmptyQual(TSChar.Get)
                )
            );
            str = type.ToString();
            Assert.AreEqual(str, "char *const");

            type = Const(
                new TPointer(
                    Const(TSChar.Get)
                )
            );
            str = type.ToString();
            Assert.AreEqual(str, "const char *const");

            type = Const(
                new TPointer(
                    Const(
                        new TPointer(
                            Const(TSChar.Get)
                        )
                    )
                )
            );
            str = type.ToString();
            Assert.AreEqual(str, "const char *const *const");
        }

        [Test]
        public static void TestPointerToArray() {
            var type = EmptyQual(
                new TPointer(
                    EmptyQual(
                        new ArrayType(
                            EmptyQual(TSChar.Get),
                            3
                        )
                    )
                )
            );
            var str = type.ToString();
            Assert.AreEqual(str, "char (*)[3]");

            type = Const(
                new TPointer(
                    EmptyQual(
                        new ArrayType(
                            Const(TSChar.Get),
                            3
                        )
                    )
                )
            );
            str = type.ToString();
            Assert.AreEqual(str, "const char (*const)[3]");

            type = Const(
                new TPointer(
                    EmptyQual(
                        new IncompleteArrayType(
                            Const(TSChar.Get)
                        )
                    )
                )
            );
            str = type.ToString();
            Assert.AreEqual(str, "const char (*const)[]");
        }

        [Test]
        public static void TestStruct() {
            var typeEnv = Env.Empty.NewStructType(
                    ImmutableList.Create(
                        CovariantTuple.Create(
                            Option.Some("a"),
                            EmptyQual(TSChar.Get)
                        )
                    )
                );

            var env = typeEnv as Env;
            var type = EmptyQual(typeEnv.Value);
            var str = env.DumpType(type);
            Assert.AreEqual(str, "struct { char a; }");

            typeEnv = Env.Empty.NewStructType(
                ImmutableList.Create(
                        CovariantTuple.Create(
                            Option.Some("a"),
                            Const(TSChar.Get)
                        ),
                        CovariantTuple.Create(
                            Option.Some("b"),
                            EmptyQual(TSChar.Get)
                        )
                    )
            );
            env = typeEnv as Env;
            type = ConstVolatile(typeEnv.Value);
            str = env.DumpType(type);
            Assert.AreEqual(str, "const volatile struct { const char a; char b; }");
        }
    }
}
