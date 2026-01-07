using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanManagementSystem.Api.Models
{
    public class LoanApplication
    {
        [Key]
        public int LoanId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public User Customer { get; set; } = null!;

        [Required]
        public int LoanTypeId { get; set; }

        [ForeignKey("LoanTypeId")]
        public LoanType LoanType { get; set; } = null!;

        [Required]
        public decimal LoanAmount { get; set; }

        [Required]
        public int TenureMonths { get; set; }

        public int? MoratoriumMonths { get; set; }  
        public DateOnly EmiStartDate { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty; 

        public DateOnly AppliedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        public DateOnly? ApprovedDate { get; set; }

        public int? VerifiedBy { get; set; }

        [ForeignKey("VerifiedBy")]
        public User? VerifiedByUser { get; set; }

        public DateOnly? VerifiedDate { get; set; }

        public int? ApprovedBy { get; set; }

        [ForeignKey("ApprovedBy")]
        public User? ApprovedByUser { get; set; }

        public string? Remarks { get; set; }

        public ICollection<EMI> EMIs { get; set; } = new List<EMI>();
        public ICollection<LoanDocument> LoanDocuments { get; set; } = new List<LoanDocument>();
    }
}
