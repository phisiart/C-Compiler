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
            AST.Environment env = new AST.Environment();
            
            env = env.PushEntry(AST.Environment.EntryLoc.GLOBAL, "global_var", new AST.TLong());

            env = env.InScope();
            List<Tuple<String, AST.ExprType>> args = new List<Tuple<string, AST.ExprType>>();
            args.Add(new Tuple<string, AST.ExprType>("some_char", new AST.TChar()));
            args.Add(new Tuple<string, AST.ExprType>("another_char", new AST.TChar()));
            args.Add(new Tuple<string, AST.ExprType>("some_double", new AST.TDouble()));
            args.Add(new Tuple<string, AST.ExprType>("another_double", new AST.TDouble()));
            args.Add(new Tuple<string, AST.ExprType>("some_int", new AST.TLong()));
            AST.TFunction func = new AST.TFunction(new AST.TVoid(), args);
            AST.Environment env2 = env.SetCurrentFunction(func);

            String log = env.Dump();
            System.Diagnostics.Debug.WriteLine(log);
        }

        [TestMethod]
        public void TestShadow() {
            AST.Environment env = new AST.Environment();
            env = env.PushEntry(AST.Environment.EntryLoc.GLOBAL, "c", new AST.TChar());
            env = env.InScope();
            env = env.PushEntry(AST.Environment.EntryLoc.STACK, "c", new AST.TLong());

            String log = env.Dump();
            System.Diagnostics.Debug.WriteLine(log);

            AST.Environment.Entry entry = env.Find("c");

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
            List<Tuple<String, AST.ExprType>> args = new List<Tuple<string, AST.ExprType>>();
            args.Add(new Tuple<string, AST.ExprType>("some_char", new AST.TChar()));
            args.Add(new Tuple<string, AST.ExprType>("another_char", new AST.TChar()));
            args.Add(new Tuple<string, AST.ExprType>("some_double", new AST.TDouble()));
            args.Add(new Tuple<string, AST.ExprType>("another_double", new AST.TDouble()));
            args.Add(new Tuple<string, AST.ExprType>("some_int", new AST.TLong()));
            AST.TFunction func = new AST.TFunction(new AST.TVoid(), args);
            String log = func.Dump(true);
            System.Diagnostics.Debug.WriteLine(log);
        }

        [TestMethod]
        public void TestStruct() {
            List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<string, AST.ExprType>>();
            attribs.Add(new Tuple<string, AST.ExprType>("some_char", new AST.TChar()));
            attribs.Add(new Tuple<string, AST.ExprType>("another_char", new AST.TChar()));
            attribs.Add(new Tuple<string, AST.ExprType>("some_double", new AST.TDouble()));
            attribs.Add(new Tuple<string, AST.ExprType>("another_double", new AST.TDouble()));
            attribs.Add(new Tuple<string, AST.ExprType>("some_int", new AST.TLong()));
            AST.TStruct struct_ = new AST.TStruct(attribs);
            String log = struct_.Dump(true);
            System.Diagnostics.Debug.WriteLine(log);
        }

        [TestMethod]
        public void TestUnion() {
            List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<string, AST.ExprType>>();
            attribs.Add(new Tuple<string, AST.ExprType>("some_char", new AST.TChar()));
            attribs.Add(new Tuple<string, AST.ExprType>("another_char", new AST.TChar()));
            attribs.Add(new Tuple<string, AST.ExprType>("some_double", new AST.TDouble()));
            attribs.Add(new Tuple<string, AST.ExprType>("another_double", new AST.TDouble()));
            attribs.Add(new Tuple<string, AST.ExprType>("some_int", new AST.TLong()));
            AST.TUnion union_ = new AST.TUnion(attribs);
            String log = union_.Dump(true);
            System.Diagnostics.Debug.WriteLine(log);
        }

        [TestMethod]
        public void TestDeclnSpecs() {
            String src = "int long unsigned";
            List<Token> tokens = Parser.GetTokensFromString(src);
            DeclnSpecs decln_specs;
            int r = _declaration_specifiers.Parse(tokens, 0, out decln_specs);
            AST.Environment env = new AST.Environment();
            Tuple<AST.ExprType, AST.Environment> t = decln_specs.GetExprType(env);
        }

        [TestMethod]
        public void TestDeclnSpecsStruct() {
            String src = "struct MyStruct { int a; int b; }";
            List<Token> tokens = Parser.GetTokensFromString(src);
            DeclnSpecs decln_specs;
            int r = _declaration_specifiers.Parse(tokens, 0, out decln_specs);
            AST.Environment env = new AST.Environment();
            Tuple<AST.ExprType, AST.Environment> t = decln_specs.GetExprType(env);
        }

        [TestMethod]
        public void TestDeclnSpecsUnion() {
            String src = "union MyUnion { int a; int b; }";
            List<Token> tokens = Parser.GetTokensFromString(src);
            DeclnSpecs decln_specs;
            int r = _declaration_specifiers.Parse(tokens, 0, out decln_specs);
            AST.Environment env = new AST.Environment();
            Tuple<AST.ExprType, AST.Environment> t = decln_specs.GetExprType(env);
        }

    }
}
