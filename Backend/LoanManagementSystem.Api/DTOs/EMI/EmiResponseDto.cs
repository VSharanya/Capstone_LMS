namespace LoanManagementSystem.Api.DTOs.EMI
{
    public class EmiResponseDto
    {
        public int EMIId { get; set; }
        public int InstallmentNumber { get; set; }
        public DateOnly DueDate { get; set; }
        public decimal EMIAmount { get; set; }
        public bool IsPaid { get; set; }
        public DateOnly? PaidDate { get; set; }
    }
}
