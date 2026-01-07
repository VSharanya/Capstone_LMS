using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Api.DTOs.EMI
{
    public class PaymentRequestDto
    {
        [Required]
        public string? PaymentMode { get; set; }
    }
}
