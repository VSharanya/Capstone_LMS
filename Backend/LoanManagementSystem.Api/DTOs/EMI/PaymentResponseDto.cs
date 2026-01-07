namespace LoanManagementSystem.Api.DTOs.EMI
{
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int InstallmentNumber { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? PaymentMode { get; set; }
    }
}
