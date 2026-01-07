using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanManagementSystem.Api.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int EMIId { get; set; }

        [ForeignKey("EMIId")]
        public EMI EMI { get; set; } = null!;

        [Required]
        public decimal PaidAmount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [MaxLength(30)]
        public string PaymentMode { get; set; } = string.Empty;// UPI, Card, NetBanking
    }
}
