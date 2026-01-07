using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Api.DTOs.Loans
{
    public class LoanTypeCreateDto
    {
        [Required]
        public string LoanTypeName { get; set; } = string.Empty;

        [Range(0.1, 30)]
        public decimal InterestRate { get; set; }

        [Range(1, double.MaxValue)]
        public decimal MinAmount { get; set; }

        [Range(1, double.MaxValue)]
        public decimal MaxAmount { get; set; }

        [Range(1, 600)]
        public int MaxTenureMonths { get; set; }

        public bool IsActive { get; set; }

        public bool HasMoratorium { get; set; }

    }
}
