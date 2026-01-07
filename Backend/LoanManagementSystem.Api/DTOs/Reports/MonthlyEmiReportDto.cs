namespace LoanManagementSystem.Api.DTOs.Reports
{
    public class MonthlyEmiReportDto
    {
        public int LoanId { get; set; }
        public string CustomerName { get; set; }
        public string LoanType { get; set; }
        public DateOnly DueDate { get; set; }
        public decimal EmiAmount { get; set; }
        public bool IsPaid { get; set; }
    }
}
