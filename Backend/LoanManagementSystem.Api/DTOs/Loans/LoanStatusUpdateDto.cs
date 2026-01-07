using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Api.DTOs.Loans
{
    public class LoanStatusUpdateDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; 

        public string? Remarks { get; set; }
    }
}
