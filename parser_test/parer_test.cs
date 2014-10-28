using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


[TestClass]
public class ParserTest {
    [TestMethod]
    public void test_primary_expression() {
        Assert.IsTrue(_primary_expression.Test());
    }

    [TestMethod]
    public void test_postfix_expression() {
        Assert.IsTrue(_postfix_expression.Test());
    }

    [TestMethod]
    public void test_unary_expression() {
        Assert.IsTrue(_unary_expression.Test());
    }

    [TestMethod]
    public void test_cast_expression() {
        Assert.IsTrue(_cast_expression.Test());
    }

    [TestMethod]
    public void test_multiplicative_expression() {
        Assert.IsTrue(_multiplicative_expression.Test());
    }

    [TestMethod]
    public void test_additive_expression() {
        Assert.IsTrue(_additive_expression.Test());
    }

    [TestMethod]
    public void test_shift_expression() {
        Assert.IsTrue(_shift_expression.Test());
    }

    [TestMethod]
    public void test_relational_expression() {
        Assert.IsTrue(_relational_expression.Test());
    }

    [TestMethod]
    public void test_declaration_specifiers() {
        Assert.IsTrue(_declaration_specifiers.Test());
    }

    [TestMethod]
    public void test_storage_class_specifier() {
        Assert.IsTrue(_storage_class_specifier.Test());
    }

    [TestMethod]
    public void test_type_specifier() {
        Assert.IsTrue(_type_specifier.Test());
    }

    [TestMethod]
    public void test_type_qualifier() {
        Assert.IsTrue(_type_qualifier.Test());
    }

    [TestMethod]
    public void test_function_definition() {
        Assert.IsTrue(_function_definition.Test());
    }

    [TestMethod]
    public void test_direct_declarator() {
        Assert.IsTrue(_direct_declarator.Test());
    }

    [TestMethod]
    public void test_direct_abstract_declarator() {
        Assert.IsTrue(_direct_abstract_declarator.Test());
    }

    [TestMethod]
    public void test_specifier_qualifier_list() {
        Assert.IsTrue(_specifier_qualifier_list.Test());
    }

    [TestMethod]
    public void test_struct_declarator_list() {
        Assert.IsTrue(_struct_declarator_list.Test());
    }

    [TestMethod]
    public void test_parameter_declaration() {
        Assert.IsTrue(_parameter_declaration.Test());
    }

    [TestMethod]
    public void test_declarator() {
        Assert.IsTrue(_declarator.Test());
    }

    [TestMethod]
    public void test_init_declarator() {
        Assert.IsTrue(_init_declarator.Test());
    }

    [TestMethod]
    public void test_pointer() {
        Assert.IsTrue(_pointer.Test());
    }

    [TestMethod]
    public void test_initializer() {
        Assert.IsTrue(_initializer.Test());
    }

    [TestMethod]
    public void test_declaration() {
        Assert.IsTrue(_declaration.Test());
    }

    [TestMethod]
    public void test_external_declaration() {
        Assert.IsTrue(_external_declaration.Test());
    }

    [TestMethod]
    public void test_translation_unit() {
        Assert.IsTrue(_translation_unit.Test());
    }

    [TestMethod]
    public void test_initializer_list() {
        Assert.IsTrue(_initializer_list.Test());
    }
}

