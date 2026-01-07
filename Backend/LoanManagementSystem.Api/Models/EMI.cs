using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanManagementSystem.Api.Models
{
    public class EMI
    {
        [Key]
        public int EMIId { get; set; }

        [Required]
        public int LoanId { get; set; }

        [ForeignKey("LoanId")]
        public LoanApplication LoanApplication { get; set; } = null!;

        [Required]
        public int InstallmentNumber { get; set; }

        [Required]
        public DateOnly DueDate { get; set; }

        [Required]
        public decimal EMIAmount { get; set; }

        public bool IsPaid { get; set; } = false;

        public DateOnly? PaidDate { get; set; }

        public Payment? Payment { get; set; }
    }
}
