using NUnit.Framework;
using System;
using Lab.Interfaces;
using Lab.Implementations.GenCode1;

namespace Module.Tests
{
    public class EvaluatorTests
    {
        private IMathExpressionEvaluator _evaluator;

        [SetUp]
        public void Setup()
        {
            _evaluator = new MathExpressionEvaluator();
        }

        [TestCase("2+2", 4)]
        [TestCase("10-3", 7)]
        [TestCase("4*5", 20)]
        [TestCase("10/2", 5)]
        public void Evaluate_ValidExpressions_ReturnsCorrectResult(string expression, double expected)
        {
            Assert.AreEqual(expected, _evaluator.Evaluate(expression));
        }

        [Test]
        public void Evaluate_DivideByZero_ThrowsException()
        {
            Assert.Throws<DivideByZeroException>(() => _evaluator.Evaluate("5/0"));
        }
    }
}
