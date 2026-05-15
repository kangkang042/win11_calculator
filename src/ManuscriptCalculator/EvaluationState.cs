using System.Globalization;

namespace ManuscriptCalculator
{
    public sealed class EvaluationState
    {
        public bool HasExpression { get; set; }

        public bool Success { get; set; }

        public decimal Value { get; set; }

        public string DisplayText { get; set; }

        public string ErrorMessage { get; set; }

        public static EvaluationState Empty()
        {
            return new EvaluationState
            {
                HasExpression = false,
                Success = false,
                Value = 0m,
                DisplayText = string.Empty,
                ErrorMessage = string.Empty
            };
        }

        public static EvaluationState FromValue(decimal value)
        {
            return new EvaluationState
            {
                HasExpression = true,
                Success = true,
                Value = value,
                DisplayText = FormatDecimal(value),
                ErrorMessage = string.Empty
            };
        }

        public static EvaluationState FromError(string message)
        {
            return new EvaluationState
            {
                HasExpression = true,
                Success = false,
                Value = 0m,
                DisplayText = string.Empty,
                ErrorMessage = message
            };
        }

        private static string FormatDecimal(decimal value)
        {
            string text = value.ToString("0.#############################", CultureInfo.InvariantCulture);
            if (text == "-0")
            {
                text = "0";
            }

            return text;
        }
    }
}
