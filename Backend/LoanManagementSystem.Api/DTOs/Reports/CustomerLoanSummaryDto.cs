namespace LoanManagementSystem.Api.DTOs.Reports
{
    public class CustomerLoanSummaryDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public decimal TotalLoanAmount { get; set; }
        public decimal TotalOutstanding { get; set; }
    }
}
