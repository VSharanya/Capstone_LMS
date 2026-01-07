namespace LoanManagementSystem.Api.DTOs.Loans
{
    public class LoanResponseDto
    {
        public int LoanId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string LoanType { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public int TenureMonths { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? AnnualIncome { get; set; }
        public DateOnly AppliedDate { get; set; }
        public string? Remarks { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal OutstandingAmount { get; set; }
        public bool HasDocuments { get; set; }
        public DateOnly? VerifiedDate { get; set; }
        public DateOnly? ApprovedDate { get; set; }
    }
}
