using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Api.DTOs.Loans
{
    public class LoanApplyDto
    {
        [Required]
        public int LoanTypeId { get; set; }

        [Required]
        public decimal LoanAmount { get; set; }

        [Required]
        public int TenureMonths { get; set; }

        public int? MoratoriumMonths { get; set; }
    }
}
