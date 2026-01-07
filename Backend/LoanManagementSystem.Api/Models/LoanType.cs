using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Api.Models
{
    public class LoanType
    {
        [Key]
        public int LoanTypeId { get; set; }

        [Required, MaxLength(50)]
        public string LoanTypeName { get; set; } = string.Empty;

        [Required]
        public decimal InterestRate { get; set; } // Fixed interest

        [Required]
        public decimal MinAmount { get; set; }

        [Required]
        public decimal MaxAmount { get; set; }

        [Required]
        public int MaxTenureMonths { get; set; }

        public bool IsActive { get; set; } = true;
        public int? MoratoriumMonths { get; set; } // Only for education loan
        public bool HasMoratorium { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }



        // Navigation
        public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
    }
}
