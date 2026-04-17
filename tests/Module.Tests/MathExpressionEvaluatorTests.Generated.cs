// =============================================================
// AUTO-GENERATED TESTS. DO NOT EDIT MANUALLY.
// Source: spec/math_expression_evaluator.yaml
// Generator: generator/gen_tests.py
// =============================================================

using System;
using NUnit.Framework;
using Lab.Interfaces;
using Lab.Implementations.GenCode1;

namespace Module.Tests;

[TestFixture]
[Description("Generated tests for MathExpressionEvaluator formal specification")]
public class MathExpressionEvaluatorGeneratedTests
{
    private IMathExpressionEvaluator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new MathExpressionEvaluator();
    }

    [TestCase("2+2", 4.0, TestName = "Evaluate_EC_1_FR_1_NFR_1")]
    [TestCase("10-3", 7.0, TestName = "Evaluate_EC_2_FR_1_NFR_1")]
    [TestCase("4*5", 20.0, TestName = "Evaluate_EC_3_FR_1_NFR_1")]
    [TestCase("10/2", 5.0, TestName = "Evaluate_EC_4_FR_1_NFR_1")]
    [TestCase("2+3*4", 14.0, TestName = "Evaluate_EC_5_FR_2")]
    [TestCase("10-8/2", 6.0, TestName = "Evaluate_EC_6_FR_2")]
    [TestCase(" 6 / 3 + 2 ", 4.0, TestName = "Evaluate_EC_7_FR_3")]
    [TestCase("1.5+2.25", 3.75, TestName = "Evaluate_EC_8_FR_1_NFR_2")]
    public void Evaluate_ReturnCases_ReturnsExpectedResult(string? expression, double expected)
    {
        TestContext.WriteLine("Pre: " + "expression is not null or whitespace and contains only numeric operands with supported operators");
        TestContext.WriteLine("Post: " + "returns the arithmetic result or throws the documented exception");

        var result = _sut.Evaluate(expression!);

        Assert.That(result, Is.EqualTo(expected).Within(1e-9));
    }

    [TestCase(null, typeof(ArgumentException), TestName = "Evaluate_EC_9_FR_4")]
    [TestCase("", typeof(ArgumentException), TestName = "Evaluate_EC_10_FR_4")]
    [TestCase("1+", typeof(ArgumentException), TestName = "Evaluate_EC_11_FR_4")]
    [TestCase("1+a", typeof(ArgumentException), TestName = "Evaluate_EC_12_FR_4")]
    [TestCase("1,5+2", typeof(ArgumentException), TestName = "Evaluate_EC_13_FR_4_NFR_2")]
    [TestCase("10/0", typeof(DivideByZeroException), TestName = "Evaluate_EC_14_FR_5")]
    public void Evaluate_ExceptionCases_ThrowsExpectedException(string? expression, Type expectedException)
    {
        TestContext.WriteLine("Pre: " + "expression is not null or whitespace and contains only numeric operands with supported operators");
        TestContext.WriteLine("Post: " + "returns the arithmetic result or throws the documented exception");

        Assert.Throws(expectedException, () => _sut.Evaluate(expression!));
    }

}
