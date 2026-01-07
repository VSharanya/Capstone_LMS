namespace LoanManagementSystem.Api.Helpers
{
    public static class EmiCalculator
    {
        /// Calculates EMI using standard EMI formula
        public static decimal CalculateEmi(
            decimal principal,
            decimal monthlyRate,
            int tenureMonths)
        {
            if (monthlyRate == 0)
            {
                return principal / tenureMonths;
            }

            var ratePower = (decimal)Math.Pow(
                (double)(1 + monthlyRate),
                tenureMonths
            );

            var emi = principal * monthlyRate * ratePower
                      / (ratePower - 1);

            return Math.Round(emi, 2);
        }
    }
}
