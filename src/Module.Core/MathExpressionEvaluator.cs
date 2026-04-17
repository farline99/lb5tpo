namespace Lab.Implementations.GenCode1
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Lab.Interfaces;

    public class MathExpressionEvaluator : IMathExpressionEvaluator
    {
        public double Evaluate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));

            var tokens = Tokenize(expression);
            if (tokens.Count == 0)
                throw new ArgumentException("Expression contains no valid numbers or operators.");

            var (result, remainder) = ParseAdditive(tokens);
            if (remainder.Any())
                throw new ArgumentException("Unexpected tokens after expression.");

            return result;
        }

        private enum TokenType { Number, Operator, Unknown }

        private record Token(TokenType Type, string Value);

        private List<Token> Tokenize(string expression)
        {
            var tokens = new List<Token>();
            var current = new System.Text.StringBuilder();

            bool IsDigit(char c) => char.IsDigit(c) || c == '.';

            foreach (var c in expression.Replace(" ", ""))
            {
                if (IsDigit(c))
                {
                    current.Append(c);
                }
                else
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(new Token(TokenType.Number, current.ToString()));
                        current.Clear();
                    }

                    tokens.Add(c switch
                    {
                        '+' or '-' or '*' or '/' => new Token(TokenType.Operator, c.ToString()),
                               _ => throw new ArgumentException($"Invalid character: '{c}'.")
                    });
                }
            }

            if (current.Length > 0)
                tokens.Add(new Token(TokenType.Number, current.ToString()));

            return tokens;
        }

        private (double value, List<Token> rest) ParseAdditive(List<Token> tokens)
        {
            var (left, rest) = ParseMultiplicative(tokens);
            while (rest.Count >= 2 && rest[0].Type == TokenType.Operator)
            {
                var op = rest[0].Value;
                if (op != "+" && op != "-") break;

                var (right, newRest) = ParseMultiplicative(rest.Skip(1).ToList());
                left = op == "+" ? left + right : left - right;
                rest = newRest;
            }
            return (left, rest);
        }

        private (double value, List<Token> rest) ParseMultiplicative(List<Token> tokens)
        {
            var (left, rest) = ParsePrimary(tokens);
            while (rest.Count >= 2 && rest[0].Type == TokenType.Operator)
            {
                var op = rest[0].Value;
                if (op != "*" && op != "/") break;

                var (right, newRest) = ParsePrimary(rest.Skip(1).ToList());
                if (op == "/" && Math.Abs(right) < double.Epsilon)
                    throw new DivideByZeroException("Division by zero.");
                left = op == "*" ? left * right : left / right;
                rest = newRest;
            }
            return (left, rest);
        }

        private (double value, List<Token> rest) ParsePrimary(List<Token> tokens)
        {
            if (tokens.Count == 0 || tokens[0].Type != TokenType.Number)
                throw new ArgumentException("Expected number.");

            if (!double.TryParse(tokens[0].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                throw new ArgumentException($"Invalid number format: '{tokens[0].Value}'.");

            return (value, tokens.Skip(1).ToList());
        }
    }
}
