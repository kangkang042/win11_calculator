using System;
using System.Collections.Generic;
using System.Globalization;

namespace ManuscriptCalculator
{
    internal static class ExpressionEvaluator
    {
        public static EvaluationState Evaluate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return EvaluationState.Empty();
            }

            try
            {
                Parser parser = new Parser(Normalize(expression));
                decimal value = parser.Parse();
                return EvaluationState.FromValue(value);
            }
            catch (ExpressionException ex)
            {
                return EvaluationState.FromError(ex.Message);
            }
            catch (DivideByZeroException)
            {
                return EvaluationState.FromError("除数不能为 0");
            }
            catch (OverflowException)
            {
                return EvaluationState.FromError("数值超出范围");
            }
            catch (Exception)
            {
                return EvaluationState.FromError("表达式无法解析");
            }
        }

        private static string Normalize(string text)
        {
            return text
                .Replace('（', '(')
                .Replace('）', ')')
                .Replace('，', ',')
                .Replace('＋', '+')
                .Replace('－', '-')
                .Replace('×', '*')
                .Replace('÷', '/')
                .Replace('％', '%');
        }

        private sealed class Parser
        {
            private readonly string _text;
            private int _index;

            public Parser(string text)
            {
                _text = text;
            }

            public decimal Parse()
            {
                decimal value = ParseExpression();
                SkipWhiteSpace();

                if (!IsEnd())
                {
                    throw Error("存在无法识别的字符");
                }

                return value;
            }

            private decimal ParseExpression()
            {
                decimal value = ParseTerm();

                while (true)
                {
                    SkipWhiteSpace();

                    if (Match('+'))
                    {
                        value += ParseTerm();
                    }
                    else if (Match('-'))
                    {
                        value -= ParseTerm();
                    }
                    else
                    {
                        return value;
                    }
                }
            }

            private decimal ParseTerm()
            {
                decimal value = ParsePower();

                while (true)
                {
                    SkipWhiteSpace();

                    if (Match('*'))
                    {
                        value *= ParsePower();
                    }
                    else if (Match('/'))
                    {
                        decimal divisor = ParsePower();
                        if (divisor == 0m)
                        {
                            throw new DivideByZeroException();
                        }

                        value /= divisor;
                    }
                    else
                    {
                        return value;
                    }
                }
            }

            private decimal ParsePower()
            {
                decimal value = ParseUnary();
                SkipWhiteSpace();

                if (Match('^'))
                {
                    decimal exponent = ParsePower();
                    return FromDouble(Math.Pow(decimal.ToDouble(value), decimal.ToDouble(exponent)));
                }

                return value;
            }

            private decimal ParseUnary()
            {
                SkipWhiteSpace();

                if (Match('+'))
                {
                    return ParseUnary();
                }

                if (Match('-'))
                {
                    return -ParseUnary();
                }

                return ParsePostfix();
            }

            private decimal ParsePostfix()
            {
                decimal value = ParsePrimary();

                while (true)
                {
                    SkipWhiteSpace();

                    if (Match('%'))
                    {
                        value /= 100m;
                    }
                    else
                    {
                        return value;
                    }
                }
            }

            private decimal ParsePrimary()
            {
                SkipWhiteSpace();

                if (Match('('))
                {
                    decimal inner = ParseExpression();
                    SkipWhiteSpace();
                    Expect(')');
                    return inner;
                }

                if (char.IsLetter(Peek()))
                {
                    string identifier = ParseIdentifier();
                    SkipWhiteSpace();

                    if (Match('('))
                    {
                        List<decimal> args = new List<decimal>();
                        SkipWhiteSpace();

                        if (!Match(')'))
                        {
                            while (true)
                            {
                                args.Add(ParseExpression());
                                SkipWhiteSpace();

                                if (Match(')'))
                                {
                                    break;
                                }

                                Expect(',');
                            }
                        }

                        return EvaluateFunction(identifier, args);
                    }

                    return EvaluateConstant(identifier);
                }

                if (char.IsDigit(Peek()) || Peek() == '.')
                {
                    return ParseNumber();
                }

                throw Error("缺少数字或函数");
            }

            private decimal ParseNumber()
            {
                int start = _index;
                bool dotFound = false;

                while (!IsEnd())
                {
                    char current = _text[_index];
                    if (char.IsDigit(current))
                    {
                        _index++;
                        continue;
                    }

                    if (current == '.')
                    {
                        if (dotFound)
                        {
                            break;
                        }

                        dotFound = true;
                        _index++;
                        continue;
                    }

                    break;
                }

                string raw = _text.Substring(start, _index - start);
                decimal value;
                if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
                {
                    throw Error("数字格式不正确");
                }

                return value;
            }

            private string ParseIdentifier()
            {
                int start = _index;
                while (!IsEnd() && (char.IsLetter(_text[_index]) || char.IsDigit(_text[_index]) || _text[_index] == '_'))
                {
                    _index++;
                }

                return _text.Substring(start, _index - start).ToLowerInvariant();
            }

            private decimal EvaluateConstant(string identifier)
            {
                if (identifier == "pi")
                {
                    return FromDouble(Math.PI);
                }

                if (identifier == "e")
                {
                    return FromDouble(Math.E);
                }

                throw Error("未知常量: " + identifier);
            }

            private decimal EvaluateFunction(string identifier, IList<decimal> args)
            {
                switch (identifier)
                {
                    case "sqrt":
                        RequireArgs(identifier, args, 1);
                        return FromDouble(Math.Sqrt(decimal.ToDouble(args[0])));
                    case "abs":
                        RequireArgs(identifier, args, 1);
                        return Math.Abs(args[0]);
                    case "sin":
                        RequireArgs(identifier, args, 1);
                        return FromDouble(Math.Sin(decimal.ToDouble(args[0])));
                    case "cos":
                        RequireArgs(identifier, args, 1);
                        return FromDouble(Math.Cos(decimal.ToDouble(args[0])));
                    case "tan":
                        RequireArgs(identifier, args, 1);
                        return FromDouble(Math.Tan(decimal.ToDouble(args[0])));
                    case "log":
                        RequireArgs(identifier, args, 1);
                        return FromDouble(Math.Log10(decimal.ToDouble(args[0])));
                    case "ln":
                        RequireArgs(identifier, args, 1);
                        return FromDouble(Math.Log(decimal.ToDouble(args[0])));
                    case "round":
                        if (args.Count == 1)
                        {
                            return decimal.Round(args[0], 0, MidpointRounding.AwayFromZero);
                        }

                        if (args.Count == 2)
                        {
                            return decimal.Round(args[0], (int)args[1], MidpointRounding.AwayFromZero);
                        }

                        throw Error("round 参数数量不正确");
                    case "floor":
                        RequireArgs(identifier, args, 1);
                        return decimal.Floor(args[0]);
                    case "ceil":
                        RequireArgs(identifier, args, 1);
                        return decimal.Ceiling(args[0]);
                    case "min":
                        RequireAtLeastArgs(identifier, args, 2);
                        return Aggregate(args, true);
                    case "max":
                        RequireAtLeastArgs(identifier, args, 2);
                        return Aggregate(args, false);
                    case "pow":
                        RequireArgs(identifier, args, 2);
                        return FromDouble(Math.Pow(decimal.ToDouble(args[0]), decimal.ToDouble(args[1])));
                    default:
                        throw Error("未知函数: " + identifier);
                }
            }

            private static decimal Aggregate(IList<decimal> args, bool findMin)
            {
                decimal result = args[0];
                for (int i = 1; i < args.Count; i++)
                {
                    if (findMin)
                    {
                        if (args[i] < result)
                        {
                            result = args[i];
                        }
                    }
                    else if (args[i] > result)
                    {
                        result = args[i];
                    }
                }

                return result;
            }

            private static void RequireArgs(string identifier, IList<decimal> args, int expectedCount)
            {
                if (args.Count != expectedCount)
                {
                    throw new ExpressionException(identifier + " 参数数量不正确");
                }
            }

            private static void RequireAtLeastArgs(string identifier, IList<decimal> args, int minimumCount)
            {
                if (args.Count < minimumCount)
                {
                    throw new ExpressionException(identifier + " 至少需要 " + minimumCount + " 个参数");
                }
            }

            private char Peek()
            {
                return IsEnd() ? '\0' : _text[_index];
            }

            private bool Match(char expected)
            {
                if (Peek() != expected)
                {
                    return false;
                }

                _index++;
                return true;
            }

            private void Expect(char expected)
            {
                if (!Match(expected))
                {
                    throw Error("缺少 '" + expected + "'");
                }
            }

            private void SkipWhiteSpace()
            {
                while (!IsEnd() && char.IsWhiteSpace(_text[_index]))
                {
                    _index++;
                }
            }

            private bool IsEnd()
            {
                return _index >= _text.Length;
            }

            private ExpressionException Error(string message)
            {
                return new ExpressionException(message);
            }

            private static decimal FromDouble(double value)
            {
                return Convert.ToDecimal(value);
            }
        }

        private sealed class ExpressionException : Exception
        {
            public ExpressionException(string message)
                : base(message)
            {
            }
        }
    }
}
