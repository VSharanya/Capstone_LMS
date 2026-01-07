namespace LoanManagementSystem.Api.DTOs.Reports
{
    public class EmiOverdueReportDto
    {
        public int LoanId { get; set; }
        public int EmiId { get; set; }
        public DateOnly DueDate { get; set; }
        public decimal EmiAmount { get; set; }
        public int DaysOverdue { get; set; }
    }
}
