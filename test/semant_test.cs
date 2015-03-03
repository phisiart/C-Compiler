using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace semant2_test {
    [TestClass]
    public class EnvironmentTest {
        [TestMethod]
        public void TestEnv() {
            AST.Env env = new AST.Env();
            
            env = env.PushEntry(AST.Env.EntryLoc.GLOBAL, "global_var", new AST.TLong());

            env = env.InScope();
            List<Tuple<String, AST.ExprType>> args = new List<Tuple<String, AST.ExprType>>();
            args.Add(new Tuple<String, AST.ExprType>("some_char", new AST.TChar()));
            args.Add(new Tuple<String, AST.ExprType>("another_char", new AST.TChar()));
            args.Add(new Tuple<String, AST.ExprType>("some_double", new AST.TDouble()));
            args.Add(new Tuple<String, AST.ExprType>("another_double", new AST.TDouble()));
            args.Add(new Tuple<String, AST.ExprType>("some_int", new AST.TLong()));
            AST.TFunction func = new AST.TFunction(new AST.TVoid(), args, false);
            AST.Env env2 = env.SetCurrentFunction(func);

            String log = env.Dump();
            System.Diagnostics.Debug.WriteLine(log);
        }

        [TestMethod]
        public void TestShadow() {
            AST.Env env = new AST.Env();
            env = env.PushEntry(AST.Env.EntryLoc.GLOBAL, "c", new AST.TChar());
            env = env.InScope();
            env = env.PushEntry(AST.Env.EntryLoc.STACK, "c", new AST.TLong());

            String log = env.Dump();
            System.Diagnostics.Debug.WriteLine(log);

            AST.Env.Entry entry = env.Find("c");

            System.Diagnostics.Debug.WriteLine("c : " + entry.entry_loc + " " + entry.entry_type);
        }
    }

    [TestClass]
    public class TypeTest {
        [TestMethod]
        public void TestDump() {
            AST.ExprType type = new AST.TDouble(true, true);
            type = type.GetQualifiedType(false, false);
        }

        [TestMethod]
        public void TestFunction() {
            List<Tuple<String, AST.ExprType>> args = new List<Tuple<String, AST.ExprType>>();
            args.Add(new Tuple<String, AST.ExprType>("some_char", new AST.TChar()));
            args.Add(new Tuple<String, AST.ExprType>("another_char", new AST.TChar()));
            args.Add(new Tuple<String, AST.ExprType>("some_double", new AST.TDouble()));
            args.Add(new Tuple<String, AST.ExprType>("another_double", new AST.TDouble()));
            args.Add(new Tuple<String, AST.ExprType>("some_int", new AST.TLong()));
            AST.TFunction func = new AST.TFunction(new AST.TVoid(), args, false);
            String log = func.Dump(true);
            System.Diagnostics.Debug.WriteLine(log);
        }

        [TestMethod]
        public void TestStruct() {
            List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<String, AST.ExprType>>();
            attribs.Add(new Tuple<String, AST.ExprType>("some_char", new AST.TChar()));
            attribs.Add(new Tuple<String, AST.ExprType>("another_char", new AST.TChar()));
            attribs.Add(new Tuple<String, AST.ExprType>("some_double", new AST.TDouble()));
            attribs.Add(new Tuple<String, AST.ExprType>("another_double", new AST.TDouble()));
            attribs.Add(new Tuple<String, AST.ExprType>("some_int", new AST.TLong()));
            AST.TStruct struct_ = new AST.TStruct(attribs);
            String log = struct_.Dump(true);
            System.Diagnostics.Debug.WriteLine(log);
        }

        [TestMethod]
        public void TestUnion() {
            List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<String, AST.ExprType>>();
            attribs.Add(new Tuple<String, AST.ExprType>("some_char", new AST.TChar()));
            attribs.Add(new Tuple<String, AST.ExprType>("another_char", new AST.TChar()));
            attribs.Add(new Tuple<String, AST.ExprType>("some_double", new AST.TDouble()));
            attribs.Add(new Tuple<String, AST.ExprType>("another_double", new AST.TDouble()));
            attribs.Add(new Tuple<String, AST.ExprType>("some_int", new AST.TLong()));
            AST.TUnion union_ = new AST.TUnion(attribs);
            String log = union_.Dump(true);
            System.Diagnostics.Debug.WriteLine(log);
        }

        [TestMethod]
        // This test should not pass!
        public void TestEnum2() {
            String src = "enum MyEnum";
            List<Token> tokens = Parser.GetTokensFromString(src);
            DeclarationSpecifiers decln_specs;
            Int32 r = _declaration_specifiers.Parse(tokens, 0, out decln_specs);
            AST.Env env = new AST.Env();
            Tuple<AST.Env, AST.ExprType> t = decln_specs.GetExprType(env);
        }
    }

    [TestClass]
    public class DeclnTest {
        [TestMethod]
        public void TestInt() {
            String src = "Int32 a, *b, c(Int32 haha, Int32), d[];";
            List<Token> tokens = Parser.GetTokensFromString(src);
            Declaration decln;
            Int32 r = _declaration.Parse(tokens, 0, out decln);

            AST.Env env = new AST.Env();
            Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_decln = decln.GetDeclns(env);
            //AST.ExprType type = new AST.TDouble(true, true);
            //type = type.GetQualifiedType(false, false);
        }

        [TestMethod]
        public void TestStruct() {
            String src = "struct MyStruct { Int32 a; Int32 b; } my_struct;";
            List<Token> tokens = Parser.GetTokensFromString(src);
            Declaration decln;
            Int32 r = _declaration.Parse(tokens, 0, out decln);

            AST.Env env = new AST.Env();
            Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_decln = decln.GetDeclns(env);
        }

        [TestMethod]
        public void TestUnion() {
            String src = "union MyUnion { Int32 a; Int32 b; } my_union;";
            List<Token> tokens = Parser.GetTokensFromString(src);
            Declaration decln;
            Int32 r = _declaration.Parse(tokens, 0, out decln);

            AST.Env env = new AST.Env();
            Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_decln = decln.GetDeclns(env);
        }
    }

    [TestClass]
    public class ExprTest {
        [TestMethod]
        public void TestMult() {
            String src = "3.0 * 5.0f;";
            List<Token> tokens = Parser.GetTokensFromString(src);
            Expression expr;
            Int32 r = _multiplicative_expression.Parse(tokens, 0, out expr);

            AST.Env env = new AST.Env();
            Tuple<AST.Env, AST.Expr> r_expr = expr.GetExpr(env);
            
        }
    }

    [TestClass]
    public class StmtTest {
        [TestMethod]
        public void TestCompountStmt() {
            String src = "{ Int32 a; Int32 b; 3.0f; a % a; }";
            List<Token> tokens = Parser.GetTokensFromString(src);
            CompoundStatement stmt;
            Int32 r = _compound_statement.Parse(tokens, 0, out stmt);

            AST.Env env = new AST.Env();
            Tuple<AST.Env, AST.Stmt> r_stmt = stmt.GetStmt(env);

        }

        [TestMethod]
        public void TestVariable() {
            String src = "{Int32 *a; a; }";
            List<Token> tokens = Parser.GetTokensFromString(src);
            CompoundStatement stmt;
            Int32 r = _compound_statement.Parse(tokens, 0, out stmt);
            AST.Env env = new AST.Env();
            Tuple<AST.Env, AST.Stmt> r_stmt = stmt.GetStmt(env);
        }
    }

    [TestClass]
    public class FullTest {
        [TestMethod]
        public void TestFunctionDef() {
            String src = "int main(int argc, char **argv) { 0; 1; 3.0f; }";
            List<Token> tokens = Parser.GetTokensFromString(src);
            TranslationUnit unit;
            Int32 r = _translation_unit.Parse(tokens, 0, out unit);
            
            Tuple<AST.Env, AST.TranslnUnit> r_unit = unit.GetTranslationUnit();

        }

    }
}
