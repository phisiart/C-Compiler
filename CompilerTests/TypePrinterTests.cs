using System;
using System.Collections.Immutable;
using NUnit.Framework;

namespace CompilerTests {
    [TestFixture]
    public static class TypePrinterTests {
        [Test]
        public static void TestVoid() {
            var type = new AST.VoidType(isConst: true);
            Assert.AreEqual("const void a", type.Decl("a"));
            Assert.AreEqual("const void", type.Decl());
        }

        [Test]
        public static void TestChar() {
            var type = new AST.CharType();
            Assert.AreEqual("char a", type.Decl("a"));
            Assert.AreEqual("char", type.Decl());
        }

        [Test]
        public static void TestUChar() {
            var type = new AST.UCharType();
            Assert.AreEqual("unsigned char a", type.Decl("a"));
            Assert.AreEqual("unsigned char", type.Decl());
        }

        [Test]
        public static void TestLong() {
            var type = new AST.LongType(isConst: true, isVolatile: true);
            Assert.AreEqual("const volatile long a", type.Decl("a"));
            Assert.AreEqual("const volatile long", type.Decl());
        }

        [Test]
        public static void TestULong() {
            var type = new AST.ULongType(isConst: true);
            Assert.AreEqual("const unsigned long a", type.Decl("a"));
            Assert.AreEqual("const unsigned long", type.Decl());
        }

        [Test]
        public static void TestShort() {
            var type = new AST.ShortType(isConst: true);
            Assert.AreEqual("const short a", type.Decl("a"));
            Assert.AreEqual("const short", type.Decl());

        }

        [Test]
        public static void TestUShort() {
            var type = new AST.UShortType(isConst: true);
            Assert.AreEqual("const unsigned short a", type.Decl("a"));
            Assert.AreEqual("const unsigned short", type.Decl());
        }

        [Test]
        public static void TestFloat() {
            var type = new AST.FloatType(isConst: true);
            Assert.AreEqual("const float a", type.Decl("a"));
            Assert.AreEqual("const float", type.Decl());
        }

        [Test]
        public static void TestDouble() {
            var type = new AST.DoubleType(isConst: true);
            Assert.AreEqual("const double a", type.Decl("a"));
            Assert.AreEqual("const double", type.Decl());
        }

        [Test]
        public static void TestPointer() {
            var type = new AST.PointerType(
                new AST.LongType(isConst: true),
                isConst: true,
                isVolatile: true
            );
            Assert.AreEqual("const long *const volatile a", type.Decl("a"));
            Assert.AreEqual("const long *const volatile", type.Decl());
        }

        [Test]
        public static void TestIncompleteArrayType() {
            var type = new AST.IncompleteArrayType(
                new AST.LongType(isConst: true)
            );
            Assert.AreEqual("const long a[]", type.Decl("a"));
            Assert.AreEqual("const long []", type.Decl());

            type = new AST.IncompleteArrayType(
                new AST.IncompleteArrayType(
                    new AST.LongType()
                )
            );
            Assert.AreEqual("long a[][]", type.Decl("a"));
            Assert.AreEqual("long [][]", type.Decl());
        }

        [Test]
        public static void TestArrayType() {
            var type = new AST.ArrayType(
                new AST.LongType(isConst: true),
                3
            );
            Assert.AreEqual("const long a[3]", type.Decl("a"));
            Assert.AreEqual("const long [3]", type.Decl());

            var multiDimArrayType = new AST.ArrayType(
                new AST.ArrayType(
                    new AST.LongType(isConst: true),
                    3
                ),
                4
            );
            Assert.AreEqual("const long a[4][3]", multiDimArrayType.Decl("a"));
            Assert.AreEqual("const long [3]", type.Decl());
        }

        [Test]
        public static void TestStructOrUnionType() {
            var type = AST.StructOrUnionType.CreateIncompleteStruct("my_struct", false, true);
            Assert.AreEqual("volatile struct my_struct a", type.Decl("a"));
        }

        [Test]
        public static void TestFunctionType() {
            var oneArg = AST.FunctionType.Create(
                AST.StructOrUnionType.CreateIncompleteStruct("my_struct", false, false),
                ImmutableList.Create(
                    Tuple.Create(Option<String>.None, new AST.LongType() as AST.ExprType)
                ),
                false
            );
            Assert.AreEqual("struct my_struct a(long)", oneArg.Decl("a"));
            Assert.AreEqual("struct my_struct (long)", oneArg.Decl());

            var moreArg = AST.FunctionType.Create(
                AST.StructOrUnionType.CreateIncompleteStruct("my_struct", false, false),
                ImmutableList.Create(
                    Tuple.Create(Option<String>.None, new AST.LongType() as AST.ExprType),
                    Tuple.Create(Option<String>.None, new AST.FloatType(isConst: true) as AST.ExprType)
                ),
                false
            );
            Assert.AreEqual("struct my_struct a(long, const float)", moreArg.Decl("a"));
            Assert.AreEqual("struct my_struct (long, const float)", moreArg.Decl());

            var emptyArg = AST.FunctionType.Create(
                AST.StructOrUnionType.CreateIncompleteStruct("my_struct", false, false),
                ImmutableList<Tuple<Option<String>, AST.ExprType>>.Empty,
                false
            );
            Assert.AreEqual("struct my_struct a(void)", emptyArg.Decl("a"));
            Assert.AreEqual("struct my_struct (void)", emptyArg.Decl());

            var emptyVarArg = AST.FunctionType.Create(
                AST.StructOrUnionType.CreateIncompleteStruct("my_struct", false, false),
                ImmutableList<Tuple<Option<String>, AST.ExprType>>.Empty,
                true
            );
            Assert.AreEqual("struct my_struct a(...)", emptyVarArg.Decl("a"));
            Assert.AreEqual("struct my_struct (...)", emptyVarArg.Decl());

            var varArg = AST.FunctionType.Create(
                AST.StructOrUnionType.CreateIncompleteStruct("my_struct", false, false),
                ImmutableList.Create(
                    Tuple.Create(Option<String>.None, new AST.LongType() as AST.ExprType),
                    Tuple.Create(Option<String>.None, new AST.FloatType(isConst: true) as AST.ExprType)
                ),
                true
            );
            Assert.AreEqual("struct my_struct a(long, const float, ...)", varArg.Decl("a"));
            Assert.AreEqual("struct my_struct (long, const float, ...)", varArg.Decl());
        }

        [Test]
        public static void TestFunctionPointer() {
            var funcPtr = new AST.PointerType(
                AST.FunctionType.Create(
                    AST.StructOrUnionType.CreateIncompleteStruct("my_struct", false, false),
                    ImmutableList.Create(
                        Tuple.Create(Option<String>.None, new AST.LongType() as AST.ExprType)
                    ),
                    false
                )
            );
            Assert.AreEqual("struct my_struct (*a)(long)", funcPtr.Decl("a"));
            Assert.AreEqual("struct my_struct (*)(long)", funcPtr.Decl());
        }

        [Test]
        public static void TestPointerFunction() {
            var ptrFunc = AST.FunctionType.Create(
                new AST.PointerType(
                    new AST.LongType()
                ),
                ImmutableList.Create(
                    Tuple.Create(Option<String>.None, new AST.LongType() as AST.ExprType)
                ),
                false
            );
            Assert.AreEqual("long *a(long)", ptrFunc.Decl("a"));
            Assert.AreEqual("long *(long)", ptrFunc.Decl());
        }

        [Test]
        public static void TestArrayPointer() {
            var arrPtr = new AST.PointerType(
                new AST.ArrayType(
                    new AST.LongType(),
                    3
                )
            );
            Assert.AreEqual("long (*a)[3]", arrPtr.Decl("a"));
            Assert.AreEqual("long (*)[3]", arrPtr.Decl());
        }

        [Test]
        public static void TestPointerArray() {
            var ptrArr = new AST.ArrayType(
                new AST.PointerType(
                    new AST.LongType()
                ),
                3
            );
            Assert.AreEqual("long *a[3]", ptrArr.Decl("a"));
            Assert.AreEqual("long *[3]", ptrArr.Decl());
        }
    }
}

