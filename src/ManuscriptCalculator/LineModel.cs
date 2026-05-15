namespace ManuscriptCalculator
{
    public class LineModel
    {
        public string Expression { get; set; }
        public EvaluationState Result { get; set; }

        public LineModel()
        {
            Expression = string.Empty;
            Result = EvaluationState.Empty();
        }
    }
}
