using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Api.DTOs.Loans
{
    public class LoanTypeUpdateDto
    {
        [Required]
        public decimal InterestRate { get; set; }

        [Required]
        public decimal MinAmount { get; set; }

        [Required]
        public decimal MaxAmount { get; set; }

        [Required]
        public int MaxTenureMonths { get; set; }
      
        public bool HasMoratorium { get; set; }

        public bool IsActive { get; set; }
    }
}
